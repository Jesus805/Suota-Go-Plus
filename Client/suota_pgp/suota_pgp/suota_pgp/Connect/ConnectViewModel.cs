using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using suota_pgp.Model;
using suota_pgp.Services;
using System.Collections.ObjectModel;

namespace suota_pgp
{
    /// <summary>
    /// Configure and Connect ViewModel.
    /// Upload the firmware here, select the PGP device to connect to, 
    /// then pass navigation to SUOTA.
    /// </summary>
    public class ConnectViewModel : ViewModelBase
    {
        private IEventAggregator _aggregator;
        private IBleManager _bleManager;
        private IFileManager _fileManager;
        private ISuotaManager _suotaManager;

        /// <summary>
        /// List of Go+ Devices
        /// </summary>
        public ObservableCollection<GoPlus> Devices { get; private set; }

        /// <summary>
        /// List of files with a .img extension.
        /// </summary>
        public ObservableCollection<string> Files { get; private set; }

        private GoPlus _selectedDevice;
        public GoPlus SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (SetProperty(ref _selectedDevice, value))
                {
                    BeginSuotaCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private string _selectedFileName;
        public string SelectedFileName
        {
            get => _selectedFileName;
            set
            {
                if (SetProperty(ref _selectedFileName, value))
                {
                    BeginSuotaCommand.RaiseCanExecuteChanged();
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
                    BeginSuotaCommand.RaiseCanExecuteChanged();
                    RefreshFilesCommand.RaiseCanExecuteChanged();
                    GetPairedPgpCommand.RaiseCanExecuteChanged();
                }
            }
        }

        private bool _firmwareIsLoaded;
        public bool FirmwareIsLoaded
        {
            get => _firmwareIsLoaded;
            set
            {
                if (SetProperty(ref _firmwareIsLoaded, value))
                {
                    BeginSuotaCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public DelegateCommand BeginSuotaCommand { get; private set; }

        public DelegateCommand GetPairedPgpCommand { get; private set; }

        public DelegateCommand RefreshFilesCommand { get; private set; }

        public ConnectViewModel(IEventAggregator aggregator,
                                IBleManager bleManager,
                                IFileManager fileManager,
                                ISuotaManager suotaManager)
        {
            _aggregator = aggregator;
            _bleManager = bleManager;
            _fileManager = fileManager;
            _suotaManager = suotaManager;

            Devices = new ObservableCollection<GoPlus>();
            Files = new ObservableCollection<string>();

            BeginSuotaCommand = new DelegateCommand(BeginSuota, CanBeginSuota);
            GetPairedPgpCommand = new DelegateCommand(GetPairedPgp, CanGetPairedPgp);
            RefreshFilesCommand = new DelegateCommand(RefreshFirmwares, CanRefreshFirmwares);
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetPairedPgp()
        {
            SelectedDevice = null;
            Devices.Clear();
            var pgpList = _bleManager.GetPairedDevices();
            foreach (var pgp in pgpList)
            {
                Devices.Add(pgp);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CanGetPairedPgp()
        {
            return State == State.Idle;
        }

        /// <summary>
        /// Refresh the firmware list.
        /// </summary>
        private async void RefreshFirmwares()
        {
            SelectedFileName = null;
            Files.Clear();
            State = State.Loading;
            var wareFiles = await _fileManager.GetFirmwareFileNames();
            foreach (var wareFile in wareFiles)
            {
                Files.Add(wareFile);
            }
            State = State.Idle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CanRefreshFirmwares()
        {
            return State == State.Idle;
        }

        /// <summary>
        /// 
        /// </summary>
        private void BeginSuota()
        {
            if (SelectedDevice == null ||
                string.IsNullOrEmpty(SelectedFileName))
            {
                return;
            }

            _suotaManager.BeginSuota(SelectedDevice, SelectedFileName);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool CanBeginSuota()
        {
            return (State == State.Idle) &&
                   (SelectedDevice != null) &&
                   (SelectedFileName != null);
        }

        #region Events

        public void OnFileLoaded()
        {
            FirmwareIsLoaded = true;
        }

        public void OnScanStateChanged(ScanState state)
        {
            State = (state == ScanState.Running) ? State.Scanning : State.Idle;
        }

        public void OnGoPlusFound(GoPlus pgp)
        {
            if (pgp != null)
            {
                Devices.Add(pgp);
            }
        }

        #endregion

        #region Navigation

        public override void OnNavigatedFrom(INavigationParameters parameters) { }

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            GetPairedPgp();
            RefreshFirmwares();
        }

        public override void OnNavigatingTo(INavigationParameters parameters) { }

        #endregion
    }
}
