using Java.Lang.Reflect;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using suota_pgp.Data;
using suota_pgp.Infrastructure;
using suota_pgp.Services.Interface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    /// <summary>
    /// Bluetooth Low Energy Manager.
    /// Used to communicate with a Go+.
    /// </summary>
    internal class BleManager : BindableBase, IBleManager
    {
        private readonly IAdapter _adapter;
        private readonly IBluetoothLE _ble;
        private readonly ILoggerFacade _logger;
        private readonly INotifyManager _notifyManager;
        private readonly IStateManager _stateManager;

        /// <summary>
        /// Found GO+ Devices that are bonded.
        /// </summary>
        public ObservableCollection<GoPlus> BondedDevices { get; }

        private GoPlus _selectedBondedDevice;
        public GoPlus SelectedBondedDevice 
        {
            get => _selectedBondedDevice;
            set => SetProperty(ref _selectedBondedDevice, value);
        }

        /// <summary>
        /// Found GO+ devices by scanning.
        /// </summary>
        public ObservableCollection<GoPlus> ScannedDevices { get; }

        private GoPlus _selectedScannedDevice;
        public GoPlus SelectedScannedDevice 
        { 
            get => _selectedScannedDevice;
            set => SetProperty(ref _selectedScannedDevice, value);
        }

        /// <summary>
        /// Initialize a new instance of <see cref="BleManager"/>.
        /// </summary>
        /// <param name="ble"></param>
        /// <param name="logger"></param>
        /// <param name="notifyManager"></param>
        /// <param name="stateManager"></param>
        public BleManager(IBluetoothLE ble,
                          ILoggerFacade logger,
                          INotifyManager notifyManager,
                          IStateManager stateManager)
        {
            _ble = ble;
            _ble.StateChanged += Ble_StateChanged;

            _adapter = _ble.Adapter;
            _adapter.ScanMode = ScanMode.Balanced;
            _adapter.DeviceDiscovered += Adapter_DeviceDiscovered;
            _adapter.ScanTimeoutElapsed += Adapter_ScanTimeoutElapsed;

            _logger = logger;
            _notifyManager = notifyManager;

            _stateManager = stateManager;
            _stateManager.PropertyChanged += StateManager_PropertyChanged;

            BondedDevices = new ObservableCollection<GoPlus>();
            ScannedDevices = new ObservableCollection<GoPlus>();

            UpdateBluetoothState(_ble.State);
        }

        #region Connect To Known Device

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceGuid"></param>
        /// <returns></returns>
        public async Task<GoPlus> ConnectToKnownDevice(Guid deviceGuid)
        {
            IDevice device = await _adapter.ConnectToKnownDeviceAsync(deviceGuid);
            var androidDev = (Android.Bluetooth.BluetoothDevice)device.NativeDevice;

            return new GoPlus(_ble, device, androidDev.Address);
        }

        #endregion

        #region Bonded Devices

        /// <summary>
        /// Get Paired devices.
        /// </summary>
        /// <param name="name">Device name</param>
        /// <param name="service">BLE service</param>
        /// <returns></returns>
        public void GetBondedDevices(string name, Guid service)
        {
            // Reset Bonded Devices
            BondedDevices.Clear();
            SelectedBondedDevice = null;

            IReadOnlyList<IDevice> bleDevices = _adapter.GetSystemConnectedOrPairedDevices(new Guid[] { service });

            foreach (var bleDevice in bleDevices)
            {
                // Verify that the device names match if provided
                if (string.IsNullOrEmpty(name) || bleDevice.Name == name)
                {
                    var androidDev = (Android.Bluetooth.BluetoothDevice)bleDevice.NativeDevice;

                    GoPlus pgp = new GoPlus(_ble, bleDevice, androidDev.Address);

                    BondedDevices.Add(pgp);
                }
            }

            // No PGP devices found, inform the user.
            if (BondedDevices.Count == 0)
            {
                ToastParameters toastParameters = new ToastParameters()
                {
                    { ToastParameterKeys.Message, Properties.Resources.NoPairedGoPlusFoundString },
                    { ToastParameterKeys.Duration, Android.Widget.ToastLength.Short }
                };

                _notifyManager.ShowToast(null, toastParameters);
            }
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

            try
            {
                var androidDev = (Android.Bluetooth.BluetoothDevice)device.Device.NativeDevice;

                Method mi = androidDev.Class.GetMethod("removeBond", null);
                mi.Invoke(androidDev, null);
            }
            catch (Exception e)
            {
                _logger.Log($"Unable to removeBond from {device.Name}. {e.Message}", Category.Exception, Priority.High);
            }
        }

        #endregion

        #region Device Scanning

        /// <summary>
        /// Scan for patched Go+ devices.
        /// </summary>
        public async void Scan(Guid serviceUuid)
        {
            if (_stateManager.AppState != AppState.Idle)
            {
                throw new Exception("");
            }

            try
            {
                _logger.Log(Properties.Resources.ScanStartString, Category.Info, Priority.None);
                _stateManager.AppState = AppState.Scanning;
                await _adapter.StartScanningForDevicesAsync(new Guid[] { serviceUuid });
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
            {
                throw new Exception("");
            }

            try
            {
                _logger.Log(Properties.Resources.ScanStopString, Category.Info, Priority.None);
                await _adapter.StopScanningForDevicesAsync();
            }
            catch (Exception e)
            {

            }
            finally
            {
                _stateManager.AppState = AppState.Idle;
            }
        }

        #endregion

        public void Clear()
        {
            BondedDevices.Clear();
            SelectedBondedDevice = null;
            ScannedDevices.Clear();
            SelectedScannedDevice = null;
        }

        private void UpdateBluetoothState(BluetoothState state)
        {
            if (state == BluetoothState.On)
            {
                _stateManager.ClearErrorFlag(ErrorState.BluetoothDisabled);
            }
            else if (state == BluetoothState.Off)
            {
                _stateManager.SetErrorFlag(ErrorState.BluetoothDisabled);
            }
        }

        #region Events

        private void Adapter_DeviceDiscovered(object sender, DeviceEventArgs e)
        {
            IDevice device = e.Device;
            var androidDev = (Android.Bluetooth.BluetoothDevice)device.NativeDevice;

            ScannedDevices.Add(new GoPlus(_ble, device, androidDev.Address));
        }

        private void Adapter_ScanTimeoutElapsed(object sender, EventArgs e)
        {
            _logger.Log($"Scanning timeout elapsed.", Category.Info, Priority.None);
            _stateManager.AppState = AppState.Idle;
        }

        private void Ble_StateChanged(object sender, BluetoothStateChangedArgs e)
        {
            UpdateBluetoothState(e.NewState);
        }

        private void StateManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IStateManager.ErrorState))
            {
                if (_stateManager.ErrorState != ErrorState.None)
                {
                    Clear();
                }
            }
        }

        #endregion
    }
}