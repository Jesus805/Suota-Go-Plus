using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Prism.Mvvm;
using suota_pgp.Data.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace suota_pgp.Data
{
    /// <summary>
    /// Pokemon Go Plus model.
    /// </summary>
    public class GoPlus : BindableBase, IDisposable
    {
        private readonly IBluetoothLE _bluetoothLE;
        /// <summary>
        /// Characteristic cache.
        /// </summary>
        private readonly List<ICharacteristic> _characteristics;

        /// <summary>
        /// Bluetooth device.
        /// </summary>
        public IDevice Device { get; }

        /// <summary>
        /// Device name.
        /// </summary>
        public string Name => Device.Name;

        /// <summary>
        /// Device Id.
        /// </summary>
        public Guid Id => Device.Id;

        /// <summary>
        /// Bluetooth address.
        /// </summary>
        private string _address;
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    RaisePropertyChanged(nameof(IsComplete));
                }
            }
        }

        /// <summary>
        /// Display name.
        /// </summary>
        public string Display
        {
            get => $"{Name ?? string.Empty} - {Address ?? string.Empty}";
        }

        /// <summary>
        /// 16 byte unique device key.
        /// </summary>
        private string _deviceKey;
        public string DeviceKey
        {
            get => _deviceKey;
            set
            {
                if (SetProperty(ref _deviceKey, value))
                {
                    RaisePropertyChanged(nameof(IsComplete));
                }
            }
        }

        /// <summary>
        /// 256 byte unique blob key.
        /// </summary>
        private string _blobKey;
        public string BlobKey
        {
            get => _blobKey;
            set
            {
                if (SetProperty(ref _blobKey, value))
                {
                    RaisePropertyChanged(nameof(IsComplete));
                }
            }
        }

        /// <summary>
        /// <c>true</c> if name, address, and key properties are filled in; <c>false</c> otherwise.
        /// </summary>
        public bool IsComplete
        {
            get => !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(Address) &&
                   !string.IsNullOrEmpty(DeviceKey) &&
                   !string.IsNullOrEmpty(BlobKey);
        }

        public bool IsConnected => _bluetoothLE.Adapter.ConnectedDevices.Contains(Device);

        /// <summary>
        /// Initializes a new instance of <see cref="GoPlus"/>.
        /// </summary>
        /// <param name="device">Bluetooth Device.</param>
        public GoPlus(IBluetoothLE bluetoothLE, IDevice device, string address)
        {
            // bluetoothLE
            _bluetoothLE = bluetoothLE ?? throw new ArgumentNullException(nameof(bluetoothLE));

            // device cannot be null
            Device = device ?? throw new ArgumentNullException(nameof(device));

            // monitor bluetooth state changed
            _bluetoothLE.StateChanged += BluetoothLE_StateChanged;

            _characteristics = new List<ICharacteristic>();
            Address = address;
        }

        #region Connect

        public async Task Connect()
        {
            _characteristics.Clear();

            if (!_bluetoothLE.IsOn)
            {
                throw new InvalidOperationException(ExceptionMessages.BluetoothIsNotOn);
            }

            if (_bluetoothLE.Adapter.IsScanning)
            {
                throw new InvalidOperationException(ExceptionMessages.CannotConnectInScan);
            }

            if (_bluetoothLE.Adapter.ConnectedDevices.Contains(Device))
            {
                return;
            }

            await _bluetoothLE.Adapter.ConnectToDeviceAsync(Device);

            RaisePropertyChanged(nameof(IsConnected));
        }

        public async Task Disconnect()
        {
            if (!_bluetoothLE.IsOn)
            {
                throw new InvalidOperationException(ExceptionMessages.BluetoothIsNotOn);
            }

            if (!_bluetoothLE.Adapter.ConnectedDevices.Contains(Device))
            {
                return;
            }

            await _bluetoothLE.Adapter.DisconnectDeviceAsync(Device);

            RaisePropertyChanged(nameof(IsConnected));
        }

        #endregion

        #region Read

        public async Task<byte[]> ReadCharacteristic(Guid uuid)
        {
            if (!_bluetoothLE.IsOn)
            {
                throw new InvalidOperationException(ExceptionMessages.BluetoothIsNotOn);
            }

            if (_bluetoothLE.Adapter.IsScanning)
            {
                throw new InvalidOperationException(ExceptionMessages.CannotReadCharacteristicInScan);
            }

            if (!_bluetoothLE.Adapter.ConnectedDevices.Contains(Device))
            {
                throw new InvalidOperationException(ExceptionMessages.DeviceNotConnected);
            }

            ICharacteristic characteristic;

            // Wait a bit before reading.
            await Task.Delay(Constants.DelayMS);

            // Check characteristic cache for existing characteristic
            characteristic = _characteristics.FirstOrDefault(c => c.Id == uuid);

            if (characteristic == default)
            {
                Guid serviceUuid = Constants.Char2ServiceMap[uuid];
                IService service = await Device.GetServiceAsync(serviceUuid);
                characteristic = await service.GetCharacteristicAsync(uuid);
                // Add characteristic to cache
                _characteristics.Add(characteristic);
            }

            if (characteristic.CanRead)
            {
                return await characteristic.ReadAsync();
            }
            else
            {
                throw new InvalidOperationException(ExceptionMessages.CharacteristicNotReadable);
            }
        }

        #endregion

        #region Write

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="value"></param>
        /// <param name="noResponse"></param>
        public async Task WriteCharacteristic(Guid uuid, byte value, bool noResponse = false)
        {
            await WriteCharacteristic(uuid, new byte[] { value }, noResponse);
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="value"></param>
        /// <param name="noResponse"></param>
        public async Task WriteCharacteristic(Guid uuid, short value, bool noResponse = false)
        {
            byte[] b = new byte[2];
            b[0] = (byte)value;
            b[1] = (byte)(((uint)value >> 8) & 0xFF);
            await WriteCharacteristic(uuid, b, noResponse);
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="value"></param>
        /// <param name="noResponse"></param>
        public async Task WriteCharacteristic(Guid uuid, int value, bool noResponse = false)
        {
            byte[] b = new byte[4];
            b[0] = (byte)value;
            b[1] = (byte)(((uint)value >> 8) & 0xFF);
            b[2] = (byte)(((uint)value >> 16) & 0xFF);
            b[3] = (byte)(((uint)value >> 24) & 0xFF);

            await WriteCharacteristic(uuid, b, noResponse);
        }

        /// <summary>
        /// Write to a BLE Characteristic.
        /// </summary>
        /// <param name="uuid"></param>
        /// <param name="value"></param>
        /// <param name="noResponse"></param>
        public async Task WriteCharacteristic(Guid uuid, byte[] value, bool noResponse = false)
        {
            if (value == null || value.Length == 0)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value.Length > Constants.ChunkSize)
            {
                string message = string.Format(ExceptionMessages.LengthMustBeLessThanChunkSize, Constants.ChunkSize);
                throw new ArgumentException(message, nameof(value));
            }

            if (!_bluetoothLE.IsOn)
            {
                throw new InvalidOperationException(ExceptionMessages.BluetoothIsNotOn);
            }

            if (_bluetoothLE.Adapter.IsScanning)
            {
                throw new InvalidOperationException(ExceptionMessages.CannotWriteCharacteristicInScan);
            }

            if (!_bluetoothLE.Adapter.ConnectedDevices.Contains(Device))
            {
                throw new InvalidOperationException(ExceptionMessages.DeviceNotConnected);
            }

            // Wait a bit before writing.
            await Task.Delay(Constants.DelayMS);

            ICharacteristic characteristic;

            // Check characteristic cache for existing characteristic
            characteristic = _characteristics.FirstOrDefault(c => c.Id == uuid);

            if (characteristic == default)
            {
                Guid serviceUuid = Constants.Char2ServiceMap[uuid];
                IService service = await Device.GetServiceAsync(serviceUuid);
                characteristic = await service.GetCharacteristicAsync(uuid);
                // Add characteristic to cache
                _characteristics.Add(characteristic);
            }

            characteristic.WriteType = (noResponse) ? CharacteristicWriteType.WithoutResponse :
                                                      CharacteristicWriteType.Default;

            if (characteristic.CanWrite)
            {
                bool result = await characteristic.WriteAsync(value);
                if (!result)
                {
                    throw new Exception(ExceptionMessages.FailedToWriteCharacteristic);
                }
            }
            else
            {
                throw new InvalidOperationException(ExceptionMessages.CharacteristicNotWritable);
            }
        }

        #endregion

        #region Notify

        public event EventHandler<CharacteristicNotificationEventArgs> CharacteristicNotification;

        public async Task NotifyRegister(Guid uuid)
        {
            if (!_bluetoothLE.IsOn)
            {
                throw new InvalidOperationException(ExceptionMessages.BluetoothIsNotOn);
            }

            if (_bluetoothLE.Adapter.IsScanning)
            {
                throw new InvalidOperationException(ExceptionMessages.CannotNotifyCharacteristicInScan);
            }

            if (!_bluetoothLE.Adapter.ConnectedDevices.Contains(Device))
            {
                throw new InvalidOperationException(ExceptionMessages.DeviceNotConnected);
            }

            // Wait a delay first
            await Task.Delay(1000);

            // Check characteristic cache for existing characteristic
            ICharacteristic characteristic = _characteristics.FirstOrDefault(c => c.Id == uuid);

            if (characteristic == default)
            {
                Guid serviceUuid = Constants.Char2ServiceMap[uuid];
                IService service = await Device.GetServiceAsync(serviceUuid);
                characteristic = await service.GetCharacteristicAsync(uuid);
                // Add characteristic to cache
                _characteristics.Add(characteristic);
            }

            characteristic.ValueUpdated += Characteristic_ValueUpdated;

            try
            {
                await characteristic.StartUpdatesAsync();
            }
            catch (Exception e)
            {
                // start updates failed, remove event handler.
                characteristic.ValueUpdated -= Characteristic_ValueUpdated;
                throw e;
            }
        }

        public async Task NotifyUnregister(Guid uuid)
        {
            if (!_bluetoothLE.IsOn)
            {
                throw new InvalidOperationException(ExceptionMessages.BluetoothIsNotOn);
            }

            if (_bluetoothLE.Adapter.IsScanning)
            {
                throw new InvalidOperationException(ExceptionMessages.CannotUnnotifyCharacteristicInScan);
            }

            if (!_bluetoothLE.Adapter.ConnectedDevices.Contains(Device))
            {
                throw new InvalidOperationException(ExceptionMessages.DeviceNotConnected);
            }

            // Wait a delay first
            await Task.Delay(Constants.DelayMS);

            ICharacteristic characteristic;

            // Check characteristic cache for existing characteristic
            characteristic = _characteristics.FirstOrDefault(c => c.Id == uuid);

            if (characteristic == default)
            {
                Guid serviceUuid = Constants.Char2ServiceMap[uuid];
                IService service = await Device.GetServiceAsync(serviceUuid);
                characteristic = await service.GetCharacteristicAsync(uuid);
                // Add characteristic to cache
                _characteristics.Add(characteristic);
            }

            characteristic.ValueUpdated -= Characteristic_ValueUpdated;

            await characteristic.StopUpdatesAsync();
        }

        #endregion

        private void Clear()
        {
            _characteristics.Clear();
        }

        private void BluetoothLE_StateChanged(object sender, BluetoothStateChangedArgs e)
        {
            if (e.NewState == BluetoothState.TurningOff)
            {
                Clear();
            }
        }

        private void Characteristic_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            Guid uuid = e.Characteristic.Id;
            byte[] value = e.Characteristic.Value;

            CharacteristicNotification?.Invoke(this, new CharacteristicNotificationEventArgs(uuid, value));
        }

        void IDisposable.Dispose()
        {

        }
    }
}