using Prism.Commands;
using Prism.Mvvm;
using suota_pgp.Data;
using suota_pgp.Services.Interface;
using System.ComponentModel;

namespace suota_pgp
{
    public class DeviceInfoViewModel : BindableBase
    {
        private readonly IFileManager _fileManager;
        private readonly IKeyExtractManager _keyExtractManager;

        public IBleManager BleManager { get; }

        public IStateManager StateManager { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="DeviceInfoViewModel"/>
        /// </summary>
        /// <param name="bleService"></param>
        /// <param name="fileManager"></param>
        /// <param name="keyExtractManager"></param>
        /// <param name="stateManager"></param>
        public DeviceInfoViewModel(IBleManager bleService,
                                   IFileManager fileManager,
                                   IKeyExtractManager keyExtractManager,
                                   IStateManager stateManager)
        {
            _fileManager = fileManager;
            _keyExtractManager = keyExtractManager;

            BleManager = bleService;
            BleManager.PropertyChanged += BleManager_PropertyChanged;

            StateManager = stateManager;
            StateManager.PropertyChanged += StateManager_PropertyChanged;

            GetDeviceInfoCommand = new DelegateCommand(GetDeviceInfo, GetDeviceInfoCanExecute);
            RestoreCommand = new DelegateCommand(Restore, RestoreCanExecute);
            SaveCommand = new DelegateCommand(Save, SaveCanExecute);
            ScanCommand = new DelegateCommand(Scan, ScanCanExecute);
            StopScanCommand = new DelegateCommand(StopScan, StopScanCanExecute);
        }

        #region Get Device Info

        /// <summary>
        /// Get device and blob key from <see cref="GoPlus"/> device.
        /// </summary>
        public DelegateCommand GetDeviceInfoCommand { get; }

        private async void GetDeviceInfo()
        {
            await _keyExtractManager.GetDeviceInfo(BleManager.SelectedScannedDevice);
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool GetDeviceInfoCanExecute()
        {
            return StateManager.AppState == AppState.Idle && 
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized) &&
                   BleManager.SelectedScannedDevice != null;
        }

        #endregion

        #region Restore

        /// <summary>
        /// Restore <see cref="GoPlus"/> to it's pre-patched state.
        /// </summary>
        public DelegateCommand RestoreCommand { get; }

        private void Restore()
        {
            _keyExtractManager.RestoreDevice(BleManager.SelectedScannedDevice);
        }

        private bool RestoreCanExecute()
        {
            return StateManager.AppState == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized) &&
                   BleManager.SelectedScannedDevice != null;
        }

        #endregion

        #region Save

        /// <summary>
        /// Save the <see cref="GoPlus"/>'s BLE address and keys to a JSON file.
        /// </summary>
        /// <remarks>
        /// The keys must first be extracted from the device (<see cref="GetDeviceInfoCommand"/>).
        /// </remarks>
        public DelegateCommand SaveCommand { get; }

        private void Save()
        {
            _fileManager.Save(BleManager.SelectedScannedDevice);
        }

        private bool SaveCanExecute()
        {
            return StateManager.AppState == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized) &&
                   BleManager.SelectedScannedDevice != null &&
                   BleManager.SelectedScannedDevice.IsComplete;
        }

        #endregion

        #region Scan

        /// <summary>
        /// Scan for patched <see cref="GoPlus"/> devices.
        /// </summary>
        public DelegateCommand ScanCommand { get; }

        private void Scan()
        {
            BleManager.Scan(Constants.ExtractorServiceUuid);
        }

        private bool ScanCanExecute()
        {
            return StateManager.AppState == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        #endregion

        #region Stop Scan

        /// <summary>
        /// Stop scan for patched <see cref="GoPlus"/> devices.
        /// </summary>
        public DelegateCommand StopScanCommand { get; }

        private void StopScan()
        {
            BleManager.StopScan();
        }

        private bool StopScanCanExecute()
        {
            return StateManager.AppState == AppState.Scanning &&
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        #endregion

        private void RefreshCommands()
        {
            GetDeviceInfoCommand.RaiseCanExecuteChanged();
            RestoreCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            ScanCommand.RaiseCanExecuteChanged();
            StopScanCommand.RaiseCanExecuteChanged();
        }

        #region Events

        private void BleManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IBleManager.SelectedScannedDevice))
            {
                GetDeviceInfoCommand.RaiseCanExecuteChanged();
                RestoreCommand.RaiseCanExecuteChanged();
            }
        }

        private void StateManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IStateManager.AppState))
            {
                RefreshCommands();
            }
            else if (e.PropertyName == nameof(IStateManager.ErrorState))
            {
                RefreshCommands();
            }
        }

        #endregion
    }
}
