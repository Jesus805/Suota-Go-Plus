using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using suota_pgp.Model;
using suota_pgp.Services;
using System.Collections.ObjectModel;

namespace suota_pgp
{
    public class DeviceInfoViewModel : ViewModelBase
    {
        private IEventAggregator _aggregator;
        private IBleManager _bleManager;
        private IFileManager _fileService;
        /// <summary>
        /// Used to ignore 
        /// </summary>
        private bool _isExtracting;

        private ErrorState _errorState;
        public ErrorState ErrorState
        {
            get => _errorState;
            set
            {
                if (SetProperty(ref _errorState, value))
                {
                    GetDeviceInfoCommand.RaiseCanExecuteChanged();
                    RestoreCommand.RaiseCanExecuteChanged();
                    SaveCommand.RaiseCanExecuteChanged();
                    ScanCommand.RaiseCanExecuteChanged();
                    StopScanCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public ObservableCollection<GoPlus> Devices { get; private set; }

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

        private AppState _state;
        public AppState State
        {
            get => _state;
            private set
            {
                if (SetProperty(ref _state, value))
                {
                    GetDeviceInfoCommand.RaiseCanExecuteChanged();
                    RestoreCommand.RaiseCanExecuteChanged();
                    SaveCommand.RaiseCanExecuteChanged();
                    ScanCommand.RaiseCanExecuteChanged();
                    StopScanCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand GetDeviceInfoCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }

        public DelegateCommand ScanCommand { get; private set; }

        public DelegateCommand StopScanCommand { get; private set; }

        public DelegateCommand RestoreCommand { get; private set; }

        public DeviceInfoViewModel(IEventAggregator aggregator,
                                   IBleManager bleService, 
                                   IFileManager fileService)
        {
            _aggregator = aggregator;
            _bleManager = bleService;
            _fileService = fileService;
            _isExtracting = false;

            Devices = new ObservableCollection<GoPlus>();

            GetDeviceInfoCommand = new DelegateCommand(GetDeviceInfo, GetDeviceInfoCanExecute);
            RestoreCommand = new DelegateCommand(Restore, RestoreCanExecute);
            SaveCommand = new DelegateCommand(Save, SaveCanExecute);
            ScanCommand = new DelegateCommand(Scan, ScanCanExecute);
            StopScanCommand = new DelegateCommand(StopScan, StopScanCanExecute);

            _aggregator.GetEvent<PrismEvents.ScanStateChangedEvent>().Subscribe(OnScanStateChanged, ThreadOption.UIThread);
            _aggregator.GetEvent<PrismEvents.GoPlusFoundEvent>().Subscribe(OnGoPlusFound, ThreadOption.UIThread);
            _aggregator.GetEvent<PrismEvents.ErrorStateChangedEvent>().Subscribe(OnErrorStateChanged, ThreadOption.UIThread);
        }

        private async void GetDeviceInfo()
        {
            if (SelectedDevice == null)
            {
                return;
            }

            StopScan();
            await _bleManager.GetDeviceInfo(SelectedDevice);
            SaveCommand.RaiseCanExecuteChanged();
        }

        private bool GetDeviceInfoCanExecute()
        {
            return State == AppState.Idle && 
                   !ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !ErrorState.HasFlag(ErrorState.LocationUnauthorized) &&
                   SelectedDevice != null;
        }

        private void Save()
        {
            _fileService.Save(SelectedDevice);
        }

        private bool SaveCanExecute()
        {
            return State == AppState.Idle &&
                   !ErrorState.HasFlag(ErrorState.StorageUnauthorized) &&
                   SelectedDevice != null;
        }

        private void Scan()
        {
            SelectedDevice = null;
            _isExtracting = true;
            Devices.Clear();
            _bleManager.Scan();
        }

        private bool ScanCanExecute()
        {
            return State == AppState.Idle && 
                   !ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        private void StopScan()
        {
            if (State == AppState.Scanning)
            {
                _bleManager.StopScan();
            }
        }

        private bool StopScanCanExecute()
        {
            return State == AppState.Scanning &&
                   !ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        private void Restore()
        {

        }

        private bool RestoreCanExecute()
        {
            return State == AppState.Idle &&
                   !ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !ErrorState.HasFlag(ErrorState.LocationUnauthorized) &&
                   SelectedDevice != null;
        }

        #region Events

        private void OnScanStateChanged(ScanState state)
        {
            if (_isExtracting)
            {
                State = (state == ScanState.Running) ? AppState.Scanning : AppState.Idle;
                if (State == AppState.Idle)
                {
                    _isExtracting = false;
                }
            }
        }

        private void OnErrorStateChanged(ErrorState state)
        {
            ErrorState = state;
        }

        private void OnGoPlusFound(GoPlus pgp)
        {
            if (_isExtracting && pgp != null)
            {
                Devices.Add(pgp);
            }
        }

        #endregion
    }
}
