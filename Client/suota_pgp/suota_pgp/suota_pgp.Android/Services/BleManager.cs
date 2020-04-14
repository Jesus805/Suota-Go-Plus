using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Prism.Events;
using Prism.Logging;
using suota_pgp.Data;
using suota_pgp.Services.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    /// <summary>
    /// Bluetooth Low Energy Manager.
    /// Used to communicate with a Go+.
    /// </summary>
    internal class BleManager : IBleManager
    {
        private IAdapter _adapter;
        private IBluetoothLE _ble;
        private IEventAggregator _aggregator;
        private ILoggerFacade _logger;
        private INotifyManager _notifyManager;
        private IStateManager _stateManager;
        private Dictionary<Guid, ICharacteristic> _charCache;
        private Dictionary<GoPlus, IDevice> _devicesFound;
        private List<ICharacteristic> _registeredNotifyChar;

        /// <summary>
        /// Initialize a new instance of 'BleManager'.
        /// </summary>
        /// <param name="aggregator">Prism dependency injected IEventAggregator.</param>
        public BleManager(IBluetoothLE ble,
                          IEventAggregator aggregator,
                          INotifyManager notifyManager,
                          ILoggerFacade logger,
                          IStateManager stateManager)
        {
            _aggregator = aggregator;
            _charCache = new Dictionary<Guid, ICharacteristic>();
            _devicesFound = new Dictionary<GoPlus, IDevice>();
            _registeredNotifyChar = new List<ICharacteristic>();
            _ble = ble;
            _ble.StateChanged += _ble_StateChanged;
            _adapter = _ble.Adapter;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.DeviceConnected += _adapter_DeviceConnected;
            _adapter.DeviceConnectionLost += _adapter_DeviceConnectionLost;
            _adapter.DeviceDisconnected += _adapter_DeviceDisconnected;
            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;

            PublishBluetoothState(_ble.State);
        }

        /// <summary>
        /// Get Paired or Connected devices.
        /// </summary>
        /// <returns></returns>
        public List<GoPlus> GetBondedDevices()
        {
            _devicesFound.Clear();
            IReadOnlyList<IDevice> devices = _adapter.GetSystemConnectedOrPairedDevices();
            List<GoPlus> pgpList = new List<GoPlus>();

            foreach (var device in devices)
            {
                if (device.Name == Constants.GoPlusName)
                {
                    var androidDev = (Android.Bluetooth.BluetoothDevice)device.NativeDevice;
                    GoPlus pgp = new GoPlus()
                    {
                        Name = androidDev.Name,
                        BtAddress = androidDev.Address
                    };

                    _devicesFound.Add(pgp, device);
                    pgpList.Add(pgp);
                }
            }

            if (pgpList.Count == 0)
            {
                _notifyManager.ShowShortToast("No paired Pokemon GO Plus found. Please make sure it's connected via Pokemon GO.");
            }

            return pgpList;
        }

        /// <summary>
        /// Scan for patched Go+ devices.
        /// </summary>
        public async void Scan()
        {
            _devicesFound.Clear();

            try
            {
                if (_stateManager.AppState != AppState.Idle && 
                    _stateManager.AppState != AppState.Suota)
                    return;

                _logger.Log("Scanning for Go+ Devices.", Category.Info, Priority.None);
                _stateManager.AppState = AppState.Scanning;
                await _adapter.StartScanningForDevicesAsync();
            }
            catch (Exception e)
            {
                _logger.Log($"Unable to scan for GO+ Devices. Reason: {e.Message}", Category.Info, Priority.None);
                _stateManager.AppState = AppState.Idle;
            }
        }

        /// <summary>
        /// Prematurely end scan.
        /// </summary>
        public async void StopScan()
        {
            if (_stateManager.AppState != AppState.Scanning)
                return;

            _logger.Log("Stopping scan for GO+ Devices", Category.Info, Priority.None);
            await _adapter.StopScanningForDevicesAsync();

            _stateManager.AppState = AppState.Idle;
        }

        /// <summary>
        /// Remove an existing bond.
        /// </summary>
        /// <param name="device">Go+ to unbond.</param>
        public void RemoveBond(GoPlus device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (!_devicesFound.ContainsKey(device))
            {
                throw new ArgumentException("This device does not exist in discovered devices");
            }

            var androidDev = (Android.Bluetooth.BluetoothDevice) _devicesFound[device].NativeDevice;
            var mi = androidDev.Class.GetMethod("removeBond", null);
            mi.Invoke(androidDev, null);
        }

        /// <summary>
        /// Connect to a Go+ device.
        /// </summary>
        /// <param name="device">Go+ to connect.</param>
        public async Task ConnectDevice(GoPlus device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (!_devicesFound.ContainsKey(device))
            {
                throw new ArgumentException("This device does not exist in discovered devices");
            }

            // There should only be one connection at a time.
            foreach (IDevice connectedDevice in _adapter.ConnectedDevices)
            {
                _logger.Log("Connected device already exists! Disconnecting.", Category.Info, Priority.None);
                await _adapter.DisconnectDeviceAsync(connectedDevice);
            }

            _charCache.Clear();

            _logger.Log("Connecting to Pokemon GO Plus.", Category.Info, Priority.None);
            
            // Wait a bit before connecting.
            await Task.Delay(Constants.DelayMS);

            for (int i = 0; i < Constants.RetryCount; i++)
            {
                try
                {
                    await _adapter.ConnectToDeviceAsync(_devicesFound[device]);
                    _logger.Log("Successfully connected to Pokemon GO Plus.", Category.Info, Priority.None);
                    return;
                }
                catch (Exception e)
                {
                    if (i < Constants.RetryCount - 1)
                    {
                        _logger.Log($"Error connecting to Pokemon GO Plus: {e.Message}. Trying Again.", Category.Exception, Priority.High);
                        await Task.Delay(Constants.DelayMS);
                    }
                    else
                    {
                        _logger.Log($"Error connecting to Pokemon GO Plus: {e.Message}.", Category.Exception, Priority.High);
                        _notifyManager.ShowShortToast("Unable to connect to Pokemon GO Plus.");
                    }
                }
            }

            _logger.Log($"Unable to Connect to Pokemon GO Plus.", Category.Exception, Priority.High);
            throw new Exception("Unable to Connect to Pokemon GO Plus");
        }

        /// <summary>
        /// Disconnect from a Go+ device.
        /// </summary>
        /// <param name="device">Go+ to disconnect from.</param>
        public async Task DisconnectDevice(GoPlus device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (!_devicesFound.ContainsKey(device))
            {
                throw new ArgumentException("This device does not exist in discovered devices");
            }

            await Task.Delay(Constants.DelayMS);

            // Disconnect the device if connected.
            if (_adapter.ConnectedDevices.Contains(_devicesFound[device]))
            {
                _logger.Log("Disconnecting from Go+ device.", Category.Info, Priority.None);
                await _adapter.DisconnectDeviceAsync(_devicesFound[device]);
            }
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteCharacteristic(GoPlus device, Guid charUuid, byte value, bool noResponse = false)
        {
            await WriteCharacteristic(device, charUuid, new byte[] { value }, noResponse);
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public async Task WriteCharacteristic(GoPlus device, Guid charUuid, short value, bool noResponse = false)
        {
            byte[] b = new byte[2];
            b[0] = (byte)value;
            b[1] = (byte)(((uint)value >> 8) & 0xFF);
            await WriteCharacteristic(device, charUuid, b, noResponse);
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <param name="value"></param>
        public async Task WriteCharacteristic(GoPlus device, Guid charUuid, int value, bool noResponse = false)
        {
            byte[] b = new byte[4];
            b[0] = (byte)value;
            b[1] = (byte)(((uint)value >>  8) & 0xFF);
            b[2] = (byte)(((uint)value >> 16) & 0xFF);
            b[3] = (byte)(((uint)value >> 24) & 0xFF);

            await WriteCharacteristic(device, charUuid, b, noResponse);
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <param name="value"></param>
        public async Task WriteCharacteristic(GoPlus device, Guid charUuid, byte[] value, bool noResponse = false)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (charUuid == null)
                throw new ArgumentNullException("charUuid");

            if (value == null || value.Length == 0)
                throw new ArgumentNullException("value");

            if (!_devicesFound.ContainsKey(device))
            {
                throw new Exception("This device does not exist in discovered devices");
            }

            if (value.Length > Constants.ChunkSize)
            {
                throw new ArgumentException($"Length must be less than {Constants.ChunkSize}", "value");
            }

            IDevice pgp = _devicesFound[device];

            if (!_adapter.ConnectedDevices.Contains(pgp))
            {
                throw new Exception("This device is not connected");
            }

            // Wait a bit before reading.
            await Task.Delay(Constants.DelayMS);

            ICharacteristic characteristic;

            if (_charCache.ContainsKey(charUuid))
            {
                characteristic = _charCache[charUuid];
            }
            else
            {
                Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

                IService service = await pgp.GetServiceAsync(serviceUuid);

                characteristic = await service.GetCharacteristicAsync(charUuid);

                _charCache.Add(charUuid, characteristic);
            }

            if (!characteristic.CanWrite)
            {
                throw new Exception("Characteristic is not writable");
            }

            characteristic.WriteType = (noResponse) ? CharacteristicWriteType.WithoutResponse :
                                                      CharacteristicWriteType.Default;

            for (int i = 0; i < Constants.RetryCount; i++)
            {
                try
                {
                    bool success = await characteristic.WriteAsync(value);
                    if (success)
                        return;
                    else
                    {
                        if (i < Constants.RetryCount - 1)
                        {
                            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress($"Write to characteristic unsuccessful, trying again."));
                            _logger.Log($"Write to characteristic unsuccessful, trying again.", Category.Exception, Priority.High);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Log($"Error writing characteristic: {e.Message}. Trying again.", Category.Exception, Priority.High);
                    _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress($"Error writing characteristic: {e.Message}. Trying again."));
                }
                await Task.Delay(1000);
            }

            throw new Exception("Unable to write to characteristic");
        }

        /// <summary>
        /// Read a BLE Characteristic.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <param name="value"></param>
        public async Task<byte[]> ReadCharacteristic(GoPlus device, Guid charUuid)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (charUuid == null)
                throw new ArgumentNullException("charUuid");

            if (!_devicesFound.ContainsKey(device))
                throw new Exception("This device does not exist in discovered devices");
            
            IDevice pgp = _devicesFound[device];

            if (!_adapter.ConnectedDevices.Contains(pgp))
                throw new Exception("This device is not connected");

            ICharacteristic characteristic;

            // Wait a bit before reading.
            await Task.Delay(Constants.DelayMS);

            if (_charCache.ContainsKey(charUuid))
            {
                characteristic = _charCache[charUuid];
            }
            else
            {
                Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

                IService service = await pgp.GetServiceAsync(serviceUuid);

                characteristic = await service.GetCharacteristicAsync(charUuid);

                _charCache.Add(charUuid, characteristic);
            }

            if (characteristic.CanRead)
            {
                for (int i = 0; i < Constants.RetryCount; i++)
                {
                    try
                    {
                        byte[] result = await characteristic.ReadAsync();
                        return result;
                    }
                    catch (Exception e)
                    {
                        if (i < Constants.RetryCount - 1)
                        {
                            _logger.Log($"Error reading characteristic: {e.Message}. Trying again.", Category.Exception, Priority.High);
                            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress($"Error reading characteristic: {e.Message}. Trying again."));
                            await Task.Delay(1000);
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Characteristic is not readable");
            }

            throw new Exception("Unable to read characteristic");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <returns></returns>
        public async Task NotifyRegister(GoPlus device, Guid charUuid)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (charUuid == null)
                throw new ArgumentNullException("charUuid");

            if (!_devicesFound.ContainsKey(device))
                throw new Exception("This device does not exist in discovered devices");


            IDevice pgp = _devicesFound[device];

            if (!_adapter.ConnectedDevices.Contains(pgp))
                throw new Exception("This device is not connected");

            ICharacteristic characteristic;

            // Wait a delay first
            await Task.Delay(1000);

            if (_charCache.ContainsKey(charUuid))
            {
                characteristic = _charCache[charUuid];
            }
            else
            {
                Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

                IService service = await pgp.GetServiceAsync(serviceUuid);

                characteristic = await service.GetCharacteristicAsync(charUuid);

                _charCache.Add(charUuid, characteristic);
            }

            _registeredNotifyChar.Add(characteristic);

            characteristic.ValueUpdated += KeyChar_ValueUpdated;

            await characteristic.StartUpdatesAsync();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <returns></returns>
        public async Task NotifyUnregister(GoPlus device, Guid charUuid)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (charUuid == null)
                throw new ArgumentNullException("charUuid");

            if (!_devicesFound.ContainsKey(device))
                throw new Exception("This device does not exist in discovered devices");

            IDevice pgp = _devicesFound[device];

            if (!_adapter.ConnectedDevices.Contains(pgp))
                throw new Exception("This device is not connected");

            ICharacteristic characteristic;

            // Wait a delay first
            await Task.Delay(Constants.DelayMS);

            if (_charCache.ContainsKey(charUuid))
            {
                characteristic = _charCache[charUuid];
            }
            else
            {
                Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

                IService service = await pgp.GetServiceAsync(serviceUuid);

                characteristic = await service.GetCharacteristicAsync(charUuid);

                _charCache.Add(charUuid, characteristic);
            }

            characteristic.ValueUpdated -= KeyChar_ValueUpdated;

            await characteristic.StopUpdatesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void PublishBluetoothState(BluetoothState state)
        {
            _aggregator.GetEvent<ManagerEvents.BluetoothStateChangedEvent>().Publish(state);
        }

        #region Events

        private void KeyChar_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            Guid uuid = e.Characteristic.Id;
            byte[] value = e.Characteristic.Value;
            string valStr = ByteArrayHelper.ByteArrayToString(value);
            _logger.Log($"Characteristic updated {uuid}; New value {valStr}", Category.Info, Priority.None);
            var charValue = new CharacteristicUpdate(uuid, value);
            _aggregator.GetEvent<AppEvents.CharacteristicUpdatedEvent>().Publish(charValue);
        }

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            IDevice device = e.Device;
            var androidDev = (Android.Bluetooth.BluetoothDevice)device.NativeDevice;

            string name = string.IsNullOrWhiteSpace(androidDev.Name) ? "<No Name>" : androidDev.Name;

            _logger.Log($"Device discovered. Name: {name} Address: {androidDev.Address}", Category.Debug, Priority.None);

            foreach (var record in device.AdvertisementRecords)
            {
                if (record.Type == AdvertisementRecordType.UuidsComplete128Bit)
                {
                    Guid guid = ByteArrayHelper.ByteArrayToGuid(record.Data);

                    if (Constants.ExtractorServiceUuid.Equals(guid) ||
                        Constants.GoPlusServiceUuuid.Equals(guid) ||
                        Constants.SpotaServiceUuid.Equals(guid))
                    {
                        _logger.Log($"GO Plus Discovered!", Category.Info, Priority.None);
                        GoPlus pgp = new GoPlus()
                        {
                            Name = androidDev.Name,
                            BtAddress = androidDev.Address
                        };
                        _devicesFound.Add(pgp, device);
                        _aggregator.GetEvent<AppEvents.GoPlusFoundEvent>().Publish(pgp);
                    }
                }
                else if (record.Type == AdvertisementRecordType.UuidsComplete16Bit)
                {
                    if (string.Compare(Constants.GoPlusName, androidDev.Name) == 0)
                    {
                        if (record.Data != null &&
                            record.Data.Length > 1 && 
                            record.Data[0] == Constants.SuotaAdvertisementUuid[0] &&
                            record.Data[1] == Constants.SuotaAdvertisementUuid[1])
                        {
                            _logger.Log($"GO Plus SUOTA Discovered!", Category.Info, Priority.None);
                            GoPlus pgp = new GoPlus()
                            {
                                Name = androidDev.Name,
                                BtAddress = androidDev.Address
                            };
                            _devicesFound.Add(pgp, device);
                            _aggregator.GetEvent<AppEvents.GoPlusFoundEvent>().Publish(pgp);
                        }
                    }
                }
            } 
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            _logger.Log($"Scanning timeout elapsed.", Category.Info, Priority.None);
            _stateManager.AppState = AppState.Idle;
        }

        private void _adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            _logger.Log($"Connected to {e.Device.Name}", Category.Info, Priority.None);
        }

        private void _adapter_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            _logger.Log($"Disconnected from {e.Device.Name}", Category.Info, Priority.None);
        }

        private void _adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
            _logger.Log($"Connection lost from {e.Device.Name}", Category.Info, Priority.None);
        }

        private void _ble_StateChanged(object sender, Plugin.BLE.Abstractions.EventArgs.BluetoothStateChangedArgs e)
        {
            PublishBluetoothState(e.NewState);
        }

        #endregion
    }
}