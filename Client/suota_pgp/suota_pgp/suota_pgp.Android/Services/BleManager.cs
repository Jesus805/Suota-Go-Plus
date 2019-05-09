using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Prism.Events;
using Prism.Logging;
using suota_pgp.Model;
using suota_pgp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    /// <summary>
    /// Bluetooth Low Energy Manager.
    /// Used to communicate with a Go+.
    /// </summary>
    internal class BleManager : IBleManager
    {
        private IEventAggregator _aggregator;
        private ILoggerFacade _logger;
        private IBluetoothLE _ble;
        private IAdapter _adapter;
        private Dictionary<GoPlus, IDevice> _devicesFound;
        private Dictionary<ICharacteristic, Guid> _registeredNotifyChar;

        /// <summary>
        /// Initialize a new instance of 'BleManager'.
        /// </summary>
        /// <param name="aggregator">Prism dependency injected IEventAggregator.</param>
        public BleManager(IEventAggregator aggregator,
                          ILoggerFacade logger)
        {
            _aggregator = aggregator;
            _devicesFound = new Dictionary<GoPlus, IDevice>();
            _registeredNotifyChar = new Dictionary<ICharacteristic, Guid>();
            _ble = CrossBluetoothLE.Current;
            _adapter = _ble.Adapter;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.DeviceConnected += _adapter_DeviceConnected;
            _adapter.DeviceConnectionLost += _adapter_DeviceConnectionLost;
            _adapter.DeviceDisconnected += _adapter_DeviceDisconnected;
            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _logger = logger;
        }

        /// <summary>
        /// Get the 'DeviceInfo' in a single transation.
        /// </summary>
        /// <returns>Async task that returns DeviceInfo.</returns>
        public async Task<DeviceInfo> GetDeviceInfo(GoPlus device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (!_devicesFound.ContainsKey(device))
            {
                throw new ArgumentException("This device does not exist in discovered devices");
            }

            IDevice pgp = _devicesFound[device];

            var androidDev = (Android.Bluetooth.BluetoothDevice)pgp.NativeDevice;

            await _adapter.ConnectToDeviceAsync(pgp);

            IService service = await pgp.GetServiceAsync(Constants.ExtractorServiceUuid);

            ICharacteristic keyChar = await service.GetCharacteristicAsync(Constants.KeyCharacteristicUuid);

            await keyChar.ReadAsync();

            ICharacteristic blobChar = await service.GetCharacteristicAsync(Constants.BlobCharacteristicUuid);

            await blobChar.ReadAsync();

            string key = Helper.ByteArrayToString(keyChar.Value);

            string blob = Helper.ByteArrayToString(blobChar.Value);

            DeviceInfo result = new DeviceInfo
            {
                BtAddress = androidDev.Address,
                Blob = blob,
                Key = key
            };

            await _adapter.DisconnectDeviceAsync(pgp);

            return result;
        }

        /// <summary>
        /// Get Paired or Connected devices.
        /// </summary>
        /// <returns></returns>
        public List<GoPlus> GetPairedDevices()
        {
            _devicesFound.Clear();
            List<IDevice> devices = _adapter.GetSystemConnectedOrPairedDevices();
            List<GoPlus> pgpList = new List<GoPlus>();

            foreach (var device in devices)
            {
                if (device.Name == "Pokemon GO Plus")
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

            return pgpList;
        }

        /// <summary>
        /// Scan for patched Go+ devices.
        /// </summary>
        public async void Scan()
        {
            _logger.Log("Disconnecting all devices before scanning.", Category.Info, Priority.None);
            // Disconnect all devices.
            //foreach (IDevice connectedDevice in _adapter.ConnectedDevices)
            //{
            // await _adapter.DisconnectDeviceAsync(connectedDevice);
            //}

            _devicesFound.Clear();

            try
            {
                _logger.Log("Scanning for Go+ Devices.", Category.Info, Priority.None);
                _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Running);
                await _adapter.StartScanningForDevicesAsync();
            }
            catch (Exception e)
            {
                _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Stopped);
                throw e;
            }
        }

        /// <summary>
        /// Prematurely end scan.
        /// </summary>
        public async void StopScan()
        {
            _logger.Log("Stopping scan for GO+ Devices", Category.Info, Priority.None);
            await _adapter.StopScanningForDevicesAsync();
            _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Stopped);
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

            _logger.Log("Connecting to Go+ Device.", Category.Info, Priority.None);
            await _adapter.ConnectToDeviceAsync(_devicesFound[device]);
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
        public async Task WriteCharacteristic(GoPlus device, Guid charUuid, int value)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (charUuid == null)
                throw new ArgumentNullException("charUuid");

            byte[] b = new byte[4];
            b[0] = (byte)value;
            b[1] = (byte)(((uint)value >>  8) & 0xFF);
            b[2] = (byte)(((uint)value >> 16) & 0xFF);
            b[3] = (byte)(((uint)value >> 24) & 0xFF);

            await WriteCharacteristic(device, charUuid, b);
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="charUuid"></param>
        /// <param name="value"></param>
        public async Task WriteCharacteristic(GoPlus device, Guid charUuid, byte[] value)
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

            IDevice pgp = _devicesFound[device];

            if (!_adapter.ConnectedDevices.Contains(pgp))
            {
                throw new Exception("This device is not connected");
            }

            Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

            IService service = await pgp.GetServiceAsync(serviceUuid);

            ICharacteristic keyChar = await service.GetCharacteristicAsync(charUuid);

            if (keyChar.CanWrite)
            {
                bool success = await keyChar.WriteAsync(value);
                if (!success)
                {
                    throw new Exception("Unable to write to characteristic");
                }
            }
            else
            {
                throw new Exception("Characteristic is not writable");
            }
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

            Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

            IService service = await pgp.GetServiceAsync(serviceUuid);

            ICharacteristic keyChar = await service.GetCharacteristicAsync(charUuid);

            if (keyChar.CanWrite)
            {
                byte[] result = await keyChar.ReadAsync();
                return result;
            }
            else
            {
                throw new Exception("Characteristic is not writable");
            }
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

            Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

            IService service = await pgp.GetServiceAsync(serviceUuid);

            ICharacteristic characteristic = await service.GetCharacteristicAsync(charUuid);

            _registeredNotifyChar.Add(characteristic, charUuid);

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

            Guid serviceUuid = Constants.Char2ServiceMap[charUuid];

            IService service = await pgp.GetServiceAsync(serviceUuid);

            ICharacteristic characteristic = await service.GetCharacteristicAsync(charUuid);

            _registeredNotifyChar.Remove(characteristic);

            characteristic.ValueUpdated -= KeyChar_ValueUpdated;

            await characteristic.StopUpdatesAsync();
        }

        #region Events

        private void KeyChar_ValueUpdated(object sender, Plugin.BLE.Abstractions.EventArgs.CharacteristicUpdatedEventArgs e)
        {
            Guid uuid = _registeredNotifyChar[e.Characteristic];
            byte[] value = e.Characteristic.Value;
            string valStr = Helper.ByteArrayToString(value);
            _logger.Log($"Characteristic updated {uuid}; New value {valStr}", Category.Info, Priority.None);
            var charValue = new CharValue(uuid, value);
            _aggregator.GetEvent<PrismEvents.CharacteristicUpdatedEvent>().Publish(charValue);
        }

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            IDevice device = e.Device;
            var androidDev = (Android.Bluetooth.BluetoothDevice)device.NativeDevice;

            string name = device.Name;

            _logger.Log($"{name} discovered. Address: {androidDev.Address}", Category.Info, Priority.None);

            if (device.Name == "Pokemon GO Plus" ||
                device.Name == "PGP Key Extractor")
            {
                _logger.Log($"Go+ Discovered", Category.Info, Priority.None);

                GoPlus pgp = new GoPlus()
                {
                    Name = androidDev.Name,
                    BtAddress = androidDev.Address
                };

                _devicesFound.Add(pgp, device);
                _aggregator.GetEvent<PrismEvents.GoPlusFoundEvent>().Publish(pgp);
            }

            /*
             * TODO: Search by UUID instead of Device Name
            foreach (var record in device.AdvertisementRecords)
            {
                if (record.Type == AdvertisementRecordType.UuidsComplete128Bit)
                {
                    for (int i = 0; i < record.Data.Length; i++)
                    {
                        if (Constants.ExtractorServiceUuid[i] != record.Data[i])
                        {
                            break;
                        }
                    }
                    _devices.Add(device);
                }
            }
            */
        }

        private void _adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            _logger.Log($"Scanning timeout elapsed.", Category.Info, Priority.None);
            _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Stopped);
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

        #endregion
    }
}