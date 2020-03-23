using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using suota_pgp.Model;
using suota_pgp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace suota_pgp
{
    public class DeviceInfoViewModel : BindableBase, INavigationAware
    {
        private readonly IBleManager _bleManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IExtractorManager _extractManager;
        private readonly IFileManager _fileService;

        public IStateManager StateManager { get; }

        public ObservableCollection<GoPlus> Devices { get; }

        private GoPlus _selectedDevice;
        public GoPlus SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                {
                    GetDeviceInfoCommand.RaiseCanExecuteChanged();
                    RestoreCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DeviceInfoViewModel(IBleManager bleService,
                                   IEventAggregator eventAggregator,
                                   IExtractorManager extractManager,
                                   IFileManager fileService,
                                   IStateManager stateManager)
        {
            _bleManager = bleService;
            _eventAggregator = eventAggregator;
            _extractManager = extractManager;
            _fileService = fileService;

            StateManager = stateManager;
            StateManager.PropertyChanged += StateManager_PropertyChanged;

            Devices = new ObservableCollection<GoPlus>();

            GetDeviceInfoCommand = new DelegateCommand(GetDeviceInfo, GetDeviceInfoCanExecute);
            RestoreCommand = new DelegateCommand(Restore, RestoreCanExecute);
            SaveCommand = new DelegateCommand(Save, SaveCanExecute);
            ScanCommand = new DelegateCommand(Scan, ScanCanExecute);
            StopScanCommand = new DelegateCommand(StopScan, StopScanCanExecute);

            _eventAggregator.GetEvent<PrismEvents.GoPlusFoundEvent>().Subscribe(OnGoPlusFound, ThreadOption.UIThread);
            _eventAggregator.GetEvent<PrismEvents.RestoreCompleteEvent>().Subscribe(OnRestoreComplete, ThreadOption.UIThread);
        }

        #region Get Device Info

        public DelegateCommand GetDeviceInfoCommand { get; }

        private async void GetDeviceInfo()
        {
            await _extractManager.GetDeviceInfo(SelectedDevice);
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool GetDeviceInfoCanExecute()
        {
            return StateManager.State == AppState.Idle && 
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized) &&
                   SelectedDevice != null;
        }

        #endregion

        #region Save

        public DelegateCommand SaveCommand { get; }

        private void Save()
        {
            _fileService.Save(SelectedDevice);
        }

        private bool SaveCanExecute()
        {
            return StateManager.State == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized) &&
                   SelectedDevice != null &&
                   SelectedDevice.IsComplete;
        }

        #endregion

        #region Scan

        public DelegateCommand ScanCommand { get; }

        private void Scan()
        {
            Clear();
            _bleManager.Scan();
        }

        private bool ScanCanExecute()
        {
            return StateManager.State == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        #endregion

        #region Stop Scan

        public DelegateCommand StopScanCommand { get; }

        private void StopScan()
        {
            _bleManager.StopScan();
        }

        private bool StopScanCanExecute()
        {
            return StateManager.State == AppState.Scanning &&
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        #endregion

        #region Restore

        public DelegateCommand RestoreCommand { get; }

        private void Restore()
        {
            _extractManager.RestoreDevice(SelectedDevice);
        }

        private bool RestoreCanExecute()
        {
            return StateManager.State == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized) &&
                   SelectedDevice != null;
        }

        #endregion

        private void Clear()
        {
            SelectedDevice = null;
            Devices.Clear();
        }

        private void RefreshCommands()
        {
            GetDeviceInfoCommand.RaiseCanExecuteChanged();
            RestoreCommand.RaiseCanExecuteChanged();
            SaveCommand.RaiseCanExecuteChanged();
            ScanCommand.RaiseCanExecuteChanged();
            StopScanCommand.RaiseCanExecuteChanged();
        }

        #region Events

        private void OnGoPlusFound(GoPlus pgp)
        {
            if (pgp != null && StateManager.State == AppState.Scanning)
            {
                Devices.Add(pgp);
            }
        }

        private void OnRestoreComplete()
        {
            Clear();
        }

        private void StateManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IStateManager.State))
            {
                RefreshCommands();
            }
            else if (e.PropertyName == nameof(IStateManager.ErrorState))
            {
                RefreshCommands();
            }
        }

        void INavigatedAware.OnNavigatedFrom(INavigationParameters parameters) { }

        void INavigatedAware.OnNavigatedTo(INavigationParameters parameters) { }

        #endregion
    }
}
