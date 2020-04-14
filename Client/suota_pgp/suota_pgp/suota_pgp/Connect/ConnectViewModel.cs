using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using suota_pgp.Data;
using suota_pgp.Services.Interface;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace suota_pgp
{
    /// <summary>
    /// Configure and Connect ViewModel.
    /// Upload the firmware here, select the PGP device to connect to,
    /// then pass navigation to SUOTA.
    /// </summary>
    public class ConnectViewModel : BindableBase, INavigationAware
    {
        private readonly IBleManager _bleManager;
        private readonly IFileManager _fileManager;
        private readonly INavigationService _navigationService;
        private readonly ISuotaManager _suotaManager;

        public IStateManager StateManager { get; }

        /// <summary>
        /// List of GO+ devices.
        /// </summary>
        public ObservableCollection<GoPlus> Devices { get; }

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
        /// List of patch files with a .img extension.
        /// </summary>
        public ObservableCollection<PatchFile> PatchFiles { get; }

        /// <summary>
        /// Selected patch file.
        /// </summary>
        private PatchFile _selectedPatchFile;
        public PatchFile SelectedPatchFile
        {
            get => _selectedPatchFile;
            set
            {
                if (SetProperty(ref _selectedPatchFile, value))
                {
                    BeginSuotaCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Initialize a new instance of 'ConnectViewModel'
        /// </summary>
        /// <param name="bleManager">Prism dependency injected 'IBleManager'</param>
        /// <param name="fileManager">Prism dependency injected 'IFileManager'</param>
        /// <param name="navigationService">Prism dependency injected 'INavigationService'</param>
        /// <param name="suotaManager">Prism dependency injected 'ISuotaManager'</param>
        /// <param name="stateManager">Prism dependency injected 'IStateManager'</param>
        public ConnectViewModel(IBleManager bleManager,
                                IFileManager fileManager,
                                INavigationService navigationService,
                                ISuotaManager suotaManager,
                                IStateManager stateManager)
        {
            _bleManager = bleManager;
            _fileManager = fileManager;
            _suotaManager = suotaManager;
            _navigationService = navigationService;

            StateManager = stateManager;
            StateManager.PropertyChanged += StateManager_PropertyChanged;

            Devices = new ObservableCollection<GoPlus>();            
            PatchFiles = new ObservableCollection<PatchFile>();

            BeginSuotaCommand = new DelegateCommand(BeginSuota, CanBeginSuota);
            GetPairedPgpCommand = new DelegateCommand(GetPairedPgp, CanGetPairedPgp);
            RefreshFilesCommand = new DelegateCommand(RefreshFirmwares, CanRefreshFirmwares);
        }

        #region Begin Suota

        /// <summary>
        /// Command to Begin SUOTA.
        /// </summary>
        public DelegateCommand BeginSuotaCommand { get; }

        /// <summary>
        /// Begin Software Update Over The Air (SUOTA).
        /// </summary>
        private void BeginSuota()
        {
            _suotaManager.RunSuota(SelectedDevice, SelectedPatchFile.Name);
            _navigationService.NavigateAsync(nameof(SuotaView));
        }

        private bool CanBeginSuota()
        {
            return StateManager.AppState == AppState.Idle &&
                   StateManager.ErrorState == ErrorState.None &&
                   SelectedDevice != null &&
                   SelectedPatchFile != null &&
                   !string.IsNullOrEmpty(SelectedPatchFile.Name);
        }

        #endregion

        #region Get Paired PGP

        /// <summary>
        /// Command to retrieve bonded GO+ devices.
        /// </summary>
        public DelegateCommand GetPairedPgpCommand { get; }

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
            return StateManager.AppState == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                   !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized);
        }

        #endregion

        #region Refresh Files

        /// <summary>
        /// Command to refresh the list of firmware files.
        /// </summary>
        public DelegateCommand RefreshFilesCommand { get; }

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
                    PatchFiles.Add(fileName);
                }
            }
        }

        private bool CanRefreshFirmwares()
        {
            return StateManager.AppState == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized);
        }

        #endregion

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
            PatchFiles.Clear();
            SelectedPatchFile = null;
        }

        #region Events

        private void StateManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StateManager.AppState))
            {
                BeginSuotaCommand.RaiseCanExecuteChanged();
                GetPairedPgpCommand.RaiseCanExecuteChanged();
                RefreshFilesCommand.RaiseCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(StateManager.ErrorState))
            {
                BeginSuotaCommand.RaiseCanExecuteChanged();
                GetPairedPgpCommand.RaiseCanExecuteChanged();
                RefreshFilesCommand.RaiseCanExecuteChanged();

                if (StateManager.ErrorState != ErrorState.None)
                {
                    ClearDevices();
                    ClearFiles();
                }
            }
        }
        
        #endregion

        void INavigatedAware.OnNavigatedFrom(INavigationParameters parameters) { }

        void INavigatedAware.OnNavigatedTo(INavigationParameters parameters)
        {
            try
            {
                if (!StateManager.ErrorState.HasFlag(ErrorState.BluetoothDisabled) &&
                    !StateManager.ErrorState.HasFlag(ErrorState.LocationUnauthorized))
                {
                    GetPairedPgp();
                }
            }
            catch { }

            try
            {
                if (!StateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized))
                {
                    RefreshFirmwares();
                }
            }
            catch { }
        }
    }
}
