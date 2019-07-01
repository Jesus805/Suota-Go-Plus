using Prism.Commands;
using Prism.Events;
using Prism.Navigation;
using suota_pgp.Model;
using suota_pgp.Services;
using System;
using System.Collections.Generic;
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
        private IEventAggregator _eventAggregator;
        private IBleManager _bleManager;
        private IFileManager _fileManager;
        private ISuotaManager _suotaManager;
        private INavigationService _navigationService;

        /// <summary>
        /// List of GO+ devices.
        /// </summary>
        public ObservableCollection<GoPlus> Devices { get; private set; }

        /// <summary>
        /// List of firmware files with a .img extension.
        /// </summary>
        public ObservableCollection<PatchFile> FileNames { get; private set; }

        /// <summary>
        /// Selected GO+ device.
        /// </summary>
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

        /// <summary>
        /// Selected firmware file name.
        /// </summary>
        private PatchFile _selectedFileName;
        public PatchFile SelectedFileName
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

        /// <summary>
        /// Application State.
        /// </summary>
        private AppState _appState;
        public AppState AppState
        {
            get => _appState;
            private set
            {
                if (SetProperty(ref _appState, value))
                {
                    BeginSuotaCommand.RaiseCanExecuteChanged();
                    RefreshFilesCommand.RaiseCanExecuteChanged();
                    GetPairedPgpCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Error State.
        /// </summary>
        private ErrorState _errorState;
        public ErrorState ErrorState
        {
            get => _errorState;
            set
            {
                if (SetProperty(ref _errorState, value))
                {
                    BeginSuotaCommand.RaiseCanExecuteChanged();
                    GetPairedPgpCommand.RaiseCanExecuteChanged();
                    RefreshFilesCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Command to Begin SUOTA.
        /// </summary>
        public DelegateCommand BeginSuotaCommand { get; private set; }

        /// <summary>
        /// Command to retrieve bonded GO+ devices.
        /// </summary>
        public DelegateCommand GetPairedPgpCommand { get; private set; }

        /// <summary>
        /// Command to refresh the list of firmware files.
        /// </summary>
        public DelegateCommand RefreshFilesCommand { get; private set; }

        /// <summary>
        /// Initialize a new instance of 'ConnectViewModel'
        /// </summary>
        /// <param name="eventAggregator">Prism dependency injected 'IEventAggregator'</param>
        /// <param name="bleManager">Prism dependency injected 'IBleManager'</param>
        /// <param name="fileManager">Prism dependency injected 'IFileManager'</param>
        /// <param name="navService">Prism dependency injected 'INavigationService'</param>
        /// <param name="suotaManager">Prism dependency injected 'ISuotaManager'</param>
        public ConnectViewModel(IBleManager bleManager,
                                IEventAggregator eventAggregator,
                                IFileManager fileManager,
                                INavigationService navService,
                                ISuotaManager suotaManager,
                                IStateManager stateManager)
        {
            _bleManager = bleManager;
            _eventAggregator = eventAggregator;
            _fileManager = fileManager;
            _suotaManager = suotaManager;
            _navigationService = navService;
            
            Devices = new ObservableCollection<GoPlus>();            
            FileNames = new ObservableCollection<PatchFile>();

            BeginSuotaCommand = new DelegateCommand(BeginSuota, CanBeginSuota);
            GetPairedPgpCommand = new DelegateCommand(GetPairedPgp, CanGetPairedPgp);
            RefreshFilesCommand = new DelegateCommand(RefreshFirmwares, CanRefreshFirmwares);

            AppState = stateManager.State;
            ErrorState = stateManager.ErrorState;

            _eventAggregator.GetEvent<PrismEvents.AppStateChangedEvent>().Subscribe(OnAppStateChanged, ThreadOption.UIThread);
            _eventAggregator.GetEvent<PrismEvents.ErrorStateChangedEvent>().Subscribe(OnErrorStateChanged, ThreadOption.UIThread);
        }

        /// <summary>
        /// Retrieve bonded GO+ devices.
        /// </summary>
        private void GetPairedPgp()
        {
            ClearDevices();
            List<GoPlus> pgpList = _bleManager.GetBondedDevices();
            foreach (GoPlus pgp in pgpList)
            {
                Devices.Add(pgp);
            }
        }

        private bool CanGetPairedPgp()
        {
            return AppState == AppState.Idle &&
                   !ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        /// <summary>
        /// Refresh the firmware list.
        /// </summary>
        private async void RefreshFirmwares()
        {
            ClearFiles();
            var files = await _fileManager.GetFirmwareFileNames();

            if (files != null)
            {
                foreach (var fileName in files)
                {
                    FileNames.Add(fileName);
                }
            }
        }

        private bool CanRefreshFirmwares()
        {
            return AppState == AppState.Idle &&
                   !ErrorState.HasFlag(ErrorState.StorageUnauthorized);
        }

        /// <summary>
        /// Begin Software Update Over The Air (SUOTA).
        /// </summary>
        private void BeginSuota()
        {
            _suotaManager.RunSuota(SelectedDevice, SelectedFileName.Name);
            _navigationService.NavigateAsync("SuotaView");
        }
        
        private bool CanBeginSuota()
        {
            return AppState == AppState.Idle &&
                   ErrorState == ErrorState.None &&
                   SelectedDevice != null &&
                   SelectedFileName != null &&
                   !string.IsNullOrEmpty(SelectedFileName.Name);
        }

        /// <summary>
        /// Clear device list.
        /// </summary>
        private void ClearDevices()
        {
            Devices.Clear();
            SelectedDevice = null;
        }

        /// <summary>
        /// Clear file list.
        /// </summary>
        private void ClearFiles()
        {
            FileNames.Clear();
            SelectedFileName = null;
        }

        #region Events

        private void OnAppStateChanged(AppState state)
        {
            AppState = state;
        }

        private void OnErrorStateChanged(ErrorState state)
        {
            ErrorState = state;
            if (ErrorState != ErrorState.None)
            {
                ClearDevices();
                ClearFiles();
            }
        }

        #endregion

        #region Navigation

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);

            try
            {
                if (!ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                    !ErrorState.HasFlag(ErrorState.LocationUnauthorized))
                {
                    GetPairedPgp();
                }
            }
            catch { }

            try
            {
                if (!ErrorState.HasFlag(ErrorState.StorageUnauthorized))
                {
                    RefreshFirmwares();
                }
            }
            catch { }
        }

        #endregion
    }
}
