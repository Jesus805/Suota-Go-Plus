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
        private IEventAggregator _aggregator;
        private IBleManager _bleManager;
        private IFileManager _fileManager;
        private ISuotaManager _suotaManager;
        private INavigationService _navigationService;

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
        /// List of GO+ devices.
        /// </summary>
        public ObservableCollection<GoPlus> Devices { get; private set; }

        /// <summary>
        /// List of firmware files with a .img extension.
        /// </summary>
        public ObservableCollection<string> FileNames { get; private set; }

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

        /// <summary>
        /// Application State.
        /// </summary>
        private AppState _state;
        public AppState State
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

        /// <summary>
        /// Error message to display
        /// </summary>
        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
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
        /// <param name="aggregator">Prism dependency injected 'IEventAggregator'</param>
        /// <param name="bleManager">Prism dependency injected 'IBleManager'</param>
        /// <param name="fileManager">Prism dependency injected 'IFileManager'</param>
        /// <param name="navService">Prism dependency injected 'INavigationService'</param>
        /// <param name="suotaManager">Prism dependency injected 'ISuotaManager'</param>
        public ConnectViewModel(IEventAggregator aggregator,
                                IBleManager bleManager,
                                IFileManager fileManager,
                                INavigationService navService,
                                ISuotaManager suotaManager)
        {
            _aggregator = aggregator;
            _bleManager = bleManager;
            _fileManager = fileManager;
            _suotaManager = suotaManager;
            _navigationService = navService;
            
            Devices = new ObservableCollection<GoPlus>();            
            FileNames = new ObservableCollection<string>();

            BeginSuotaCommand = new DelegateCommand(BeginSuota, CanBeginSuota);
            GetPairedPgpCommand = new DelegateCommand(GetPairedPgp, CanGetPairedPgp);
            RefreshFilesCommand = new DelegateCommand(RefreshFirmwares, CanRefreshFirmwares);

            _aggregator.GetEvent<PrismEvents.AppStateChangedEvent>().Subscribe(OnAppStateChanged, ThreadOption.UIThread);
            _aggregator.GetEvent<PrismEvents.ErrorStateChangedEvent>().Subscribe(OnErrorStateChanged, ThreadOption.UIThread);
        }

        /// <summary>
        /// Retrieve bonded GO+ devices.
        /// </summary>
        private void GetPairedPgp()
        {
            Devices.Clear();
            SelectedDevice = null;

            List<GoPlus> pgpList = _bleManager.GetBondedDevices();
            foreach (GoPlus pgp in pgpList)
            {
                Devices.Add(pgp);
            }
        }

        private bool CanGetPairedPgp()
        {
            return State == AppState.Idle &&
                   !ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        /// <summary>
        /// Refresh the firmware list.
        /// </summary>
        private async void RefreshFirmwares()
        {
            FileNames.Clear();
            SelectedFileName = null;

            _aggregator.GetEvent<PrismEvents.AppStateChangedEvent>().Publish(AppState.Loading);

            var files = await _fileManager.GetFirmwareFileNames();

            if (files != null)
            {
                foreach (string fileName in files)
                {
                    FileNames.Add(fileName);
                }
            }

            _aggregator.GetEvent<PrismEvents.AppStateChangedEvent>().Publish(AppState.Idle);
        }

        private bool CanRefreshFirmwares()
        {
            return State == AppState.Idle &&
                   !ErrorState.HasFlag(ErrorState.StorageUnauthorized);
        }

        /// <summary>
        /// Begin Software Update Over The Air (SUOTA).
        /// </summary>
        private void BeginSuota()
        {
            if (SelectedDevice == null ||
                string.IsNullOrEmpty(SelectedFileName))
            {
                return;
            }

            _suotaManager.RunSuota(SelectedDevice, SelectedFileName);
            _navigationService.NavigateAsync("SuotaView");
        }
        
        private bool CanBeginSuota()
        {
            return (State == AppState.Idle) &&
                   (ErrorState == ErrorState.None) &&
                   (SelectedDevice != null) &&
                   (SelectedFileName != null);
        }

        /// <summary>
        /// Clear devices and file names.
        /// </summary>
        private void Clear()
        {
            Devices.Clear();
            FileNames.Clear();
            SelectedDevice = null;
            SelectedFileName = null;
        }

        #region Events

        private void OnAppStateChanged(AppState state)
        {
            State = state;
        }

        private void OnErrorStateChanged(ErrorState state)
        {
            ErrorState = state;
            if (ErrorState != ErrorState.None)
            {
                Clear();
            }
        }

        #endregion

        #region Navigation

        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                //if (State == AppState.Idle)
                //{
                //    GetPairedPgp();
                //}
            }
            catch { }

            try
            {
                //if (State == AppState.Idle)
                //{
                //    RefreshFirmwares();
                //}
            }
            catch { }
        }

        #endregion
    }
}
