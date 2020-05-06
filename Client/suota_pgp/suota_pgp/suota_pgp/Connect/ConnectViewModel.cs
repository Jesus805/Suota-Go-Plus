using Prism.Commands;
using Prism.Events;
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
    /// Select the <see cref="GoPlus"/> device and <see cref="PatchFile"/>.
    /// Pass selected devices to <see cref="ISuotaManager"/>.
    /// </summary>
    public class ConnectViewModel : BindableBase
    {
        private readonly INavigationService _navigationService;
        private readonly ISuotaManager _suotaManager;

        public IBleManager BleManager { get; }

        public IFileManager FileManager { get; }

        public IStateManager StateManager { get; }

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
        /// Initialize a new instance of <see cref="ConnectViewModel"/>
        /// </summary>
        /// <param name="bleManager"></param>
        /// <param name="fileManager"></param>
        /// <param name="navigationService"></param>
        /// <param name="suotaManager"></param>
        /// <param name="stateManager"></param>
        public ConnectViewModel(IBleManager bleManager,
                                IFileManager fileManager,
                                INavigationService navigationService,
                                ISuotaManager suotaManager,
                                IStateManager stateManager)
        {
            _navigationService = navigationService;
            _suotaManager = suotaManager;

            FileManager = fileManager;

            BleManager = bleManager;
            BleManager.PropertyChanged += BleManager_PropertyChanged;

            StateManager = stateManager;
            StateManager.PropertyChanged += StateManager_PropertyChanged;
          
            PatchFiles = new ObservableCollection<PatchFile>();

            BeginSuotaCommand = new DelegateCommand(BeginSuota, CanBeginSuota);
            GetPairedPgpCommand = new DelegateCommand(GetPairedPgp, CanGetPairedPgp);
            RefreshFilesCommand = new DelegateCommand(RefreshFiles, CanRefreshFiles);
        }

        #region Begin Suota

        /// <summary>
        /// Begin Software Update Over The Air (SUOTA).
        /// </summary>
        public DelegateCommand BeginSuotaCommand { get; }

        private void BeginSuota()
        {
            _suotaManager.RunSuota(BleManager.SelectedBondedDevice, FileManager.SelectedPatchFile.Name);
            _navigationService.NavigateAsync(nameof(SuotaView));
        }

        private bool CanBeginSuota()
        {
            return StateManager.AppState == AppState.Idle &&
                   StateManager.ErrorState == ErrorState.None &&
                   BleManager.SelectedBondedDevice != null &&
                   FileManager.SelectedPatchFile != null &&
                   !string.IsNullOrEmpty(FileManager.SelectedPatchFile.Name);
        }

        #endregion

        #region Get Paired PGP

        /// <summary>
        /// Retrieve bonded GO+ devices.
        /// </summary>
        public DelegateCommand GetPairedPgpCommand { get; }

        private void GetPairedPgp()
        {
            BleManager.GetBondedDevices(Constants.GoPlusName, Constants.GoPlusServiceUuuid);
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
        /// Refresh the list of firmware files.
        /// </summary>
        public DelegateCommand RefreshFilesCommand { get; }

        private void RefreshFiles()
        {
            FileManager.GetFirmwareFileNames();
        }

        private bool CanRefreshFiles()
        {
            return StateManager.AppState == AppState.Idle &&
                   !StateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized);
        }

        #endregion

        #region Events

        private void BleManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IBleManager.SelectedBondedDevice))
            {
                BeginSuotaCommand.RaiseCanExecuteChanged();
            }
        }

        private void StateManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IStateManager.AppState))
            {
                BeginSuotaCommand.RaiseCanExecuteChanged();
                GetPairedPgpCommand.RaiseCanExecuteChanged();
                RefreshFilesCommand.RaiseCanExecuteChanged();
            }
            else if (e.PropertyName == nameof(IStateManager.ErrorState))
            {
                BeginSuotaCommand.RaiseCanExecuteChanged();
                GetPairedPgpCommand.RaiseCanExecuteChanged();
                RefreshFilesCommand.RaiseCanExecuteChanged();
            }
        }
        
        #endregion
    }
}
