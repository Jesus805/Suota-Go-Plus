using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Prism.Events;
using Prism.Mvvm;
using suota_pgp.Model;
using suota_pgp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    internal class BleManager : IBleManager
    {
        private IEventAggregator _aggregator;
        private IBluetoothLE _ble;
        private IAdapter _adapter;
        private Dictionary<GoPlus, IDevice> _devicesFound;
        private GoPlus _selectedDevice;

        public BleManager(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _aggregator.GetEvent<PrismEvents.GoPlusSelectedEvent>().Subscribe(OnGoPlusSelected, ThreadOption.BackgroundThread);
            _devicesFound = new Dictionary<GoPlus, IDevice>();
            _ble = CrossBluetoothLE.Current;
            _adapter = _ble.Adapter;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.DeviceDiscovered += _adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += _adapter_ScanTimeoutElapsed;
            _selectedDevice = null;
        }

        public async void Scan()
        {
            _devicesFound.Clear();
            _selectedDevice = null;

            try
            {
                _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Running);
                await _adapter.StartScanningForDevicesAsync();
            }
            catch
            {
                _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Stopped);
            }
        }

        public async void StopScan()
        {
            await _adapter.StopScanningForDevicesAsync();
            _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Publish(ScanState.Stopped);
        }

        public async Task<DeviceInfo> GetDeviceInfo()
        {
            if (_devicesFound.Count == 0 || _selectedDevice == null)
            {
                return null;
            }

            try
            {
                IDevice device = _devicesFound[_selectedDevice];

                var androidDev = (Android.Bluetooth.BluetoothDevice)device.NativeDevice;

                await _adapter.ConnectToDeviceAsync(device);

                IService service = await device.GetServiceAsync(Constants.ExtractorServiceUuid);

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
                    Blob = key,
                    Key = blob
                };

                await _adapter.DisconnectDeviceAsync(device);

                return result;
            }
            catch
            {
                return null;
            }
        }

        #region Events

        private void _adapter_DeviceDiscovered(object sender, Plugin.BLE.Abstractions.EventArgs.DeviceEventArgs e)
        {
            IDevice device = e.Device;

            if (device.Name == "PGP Key Extractor")
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

        private void OnGoPlusSelected(GoPlus pgp)
        {
            if (_devicesFound.ContainsKey(pgp))
            {
                _selectedDevice = pgp;
            }
        }

        #endregion
    }
}