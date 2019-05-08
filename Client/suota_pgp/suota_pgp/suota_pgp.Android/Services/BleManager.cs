using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Prism.Events;
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
        private IBluetoothLE _ble;
        private IAdapter _adapter;
        private Dictionary<GoPlus, IDevice> _devicesFound;

        /// <summary>
        /// Initialize a new instance of 'BleManager'.
        /// </summary>
        /// <param name="aggregator">Prism dependency injected IEventAggregator.</param>
        public BleManager(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _devicesFound = new Dictionary<GoPlus, IDevice>();
            _ble = CrossBluetoothLE.Current;
            _adapter = _ble.Adapter;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.DeviceConnected += _adapter_DeviceConnected;
            _adapter.DeviceConnectionLost += _adapter_DeviceConnectionLost;
            _adapter.DeviceDisconnected += _adapter_DeviceDisconnected;
            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
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

            StringBuilder sb = new StringBuilder();
            foreach (byte b in keyChar.Value)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            string key = sb.ToString();

            sb.Clear();
            foreach (byte b in blobChar.Value)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            string blob = sb.ToString();

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
            // Disconnect all devices.
            foreach (IDevice connectedDevice in _adapter.ConnectedDevices)
            {
                await _adapter.DisconnectDeviceAsync(connectedDevice);
            }

            _devicesFound.Clear();

            try
            {
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
            await _adapter.StopScanningForDevicesAsync();
            _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Stopped);
        }

        /// <summary>
        /// Connect to a Go+ device.
        /// </summary>
        /// <param name="device">Go+ to connect.</param>
        public async void ConnectDevice(GoPlus device)
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
                await _adapter.DisconnectDeviceAsync(connectedDevice);
            }

            await _adapter.ConnectToDeviceAsync(_devicesFound[device]);
        }

        /// <summary>
        /// Disconnect from a Go+ device.
        /// </summary>
        /// <param name="device">Go+ to disconnect from.</param>
        public async void DisconnectDevice(GoPlus device)
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
                await _adapter.DisconnectDeviceAsync(_devicesFound[device]);
            }
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="device"></param>
        /// <param name="characteristic"></param>
        /// <param name="value"></param>
        public async void WriteCharacteristic(GoPlus device, Guid characteristic, byte[] value)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (characteristic == null)
                throw new ArgumentNullException("characteristic");
           
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

            Guid serviceUuid = Constants.Char2ServiceMap[characteristic];

            IService service = await pgp.GetServiceAsync(serviceUuid);

            ICharacteristic keyChar = await service.GetCharacteristicAsync(characteristic);

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
        /// <param name="characteristic"></param>
        /// <param name="value"></param>
        public async Task<byte[]> ReadCharacteristic(GoPlus device, Guid characteristic)
        {
            if (device == null)
                throw new ArgumentNullException("device");

            if (characteristic == null)
                throw new ArgumentNullException("characteristic");

            if (!_devicesFound.ContainsKey(device))
                throw new Exception("This device does not exist in discovered devices");
            

            IDevice pgp = _devicesFound[device];

            if (!_adapter.ConnectedDevices.Contains(pgp))
                throw new Exception("This device is not connected");

            Guid serviceUuid = Constants.Char2ServiceMap[characteristic];

            IService service = await pgp.GetServiceAsync(serviceUuid);

            ICharacteristic keyChar = await service.GetCharacteristicAsync(characteristic);

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

        #region Events

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            IDevice device = e.Device;

            string name = device.Name;

            if (device.Name == "Pokemon GO Plus" ||
                device.Name == "PGP Key Extractor")
            {
                var androidDev = (Android.Bluetooth.BluetoothDevice)device.NativeDevice;
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
            _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Stopped);
        }

        private void _adapter_DeviceConnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
        }

        private void _adapter_DeviceDisconnected(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
        }

        private void _adapter_DeviceConnectionLost(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceErrorEventArgs e)
        {
        }

        #endregion
    }
}