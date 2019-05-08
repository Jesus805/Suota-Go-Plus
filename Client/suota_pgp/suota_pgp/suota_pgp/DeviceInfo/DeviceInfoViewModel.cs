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
        protected IEventAggregator _aggregator;
        private IBleManager _bleManager;
        private IFileManager _fileService;
        private SubscriptionToken _scanStateToken;
        private SubscriptionToken _goPlusFoundToken;

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

        private DeviceInfo _deviceInfo;
        public DeviceInfo DeviceInfo
        {
            get => _deviceInfo;
            private set
            {
                if (SetProperty(ref _deviceInfo, value))
                {
                    SaveCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private State _state;
        public State State
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

            Devices = new ObservableCollection<GoPlus>();

            GetDeviceInfoCommand = new DelegateCommand(GetDeviceInfo, GetDeviceInfoCanExecute);
            RestoreCommand = new DelegateCommand(Restore, RestoreCanExecute);
            SaveCommand = new DelegateCommand(Save, SaveCanExecute);
            ScanCommand = new DelegateCommand(Scan, ScanCanExecute);
            StopScanCommand = new DelegateCommand(StopScan, StopScanCanExecute);

            State = State.Idle;
        }

        private async void GetDeviceInfo()
        {
            if (SelectedDevice == null)
            {
                return;
            }

            StopScan();
            DeviceInfo = await _bleManager.GetDeviceInfo(SelectedDevice);
        }

        private bool GetDeviceInfoCanExecute()
        {
            return (State == State.Idle) && (SelectedDevice != null);
        }

        private void Save()
        {
            _fileService.SaveDeviceInfo(DeviceInfo);
        }

        private bool SaveCanExecute()
        {
            return (State == State.Idle) && (DeviceInfo != null);
        }

        private void Scan()
        {
            SelectedDevice = null;
            DeviceInfo = null;
            Devices.Clear();
            _bleManager.Scan();
        }

        private bool ScanCanExecute()
        {
            return State == State.Idle;
        }

        private void StopScan()
        {
            if (State == State.Scanning)
            {
                _bleManager.StopScan();
            }
        }

        private bool StopScanCanExecute()
        {
            return State == State.Scanning;
        }

        private void Restore()
        {

        }

        private bool RestoreCanExecute()
        {
            return (State == State.Idle) && (SelectedDevice != null);
        }

        #region Events

        private void OnScanStateChanged(ScanState state)
        {
            State = (state == ScanState.Running) ? State.Scanning : State.Idle;
        }

        private void OnGoPlusFound(GoPlus pgp)
        {
            if (pgp != null)
            {
                Devices.Add(pgp);
            }
        }

        #endregion

        #region Navigation

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (_scanStateToken != null)
            {
                _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Unsubscribe(_scanStateToken);
                _scanStateToken = null;
            }

            if (_goPlusFoundToken != null)
            {
                _aggregator.GetEvent<PrismEvents.GoPlusFoundEvent>().Subscribe(OnGoPlusFound, ThreadOption.UIThread);
                _goPlusFoundToken = null;
            }
        }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            _scanStateToken = _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Subscribe(OnScanStateChanged, ThreadOption.UIThread);
            _goPlusFoundToken = _aggregator.GetEvent<PrismEvents.GoPlusFoundEvent>().Subscribe(OnGoPlusFound, ThreadOption.UIThread);
        }

        public override void OnNavigatingTo(INavigationParameters parameters)
        {

        }

        #endregion
    }
}
