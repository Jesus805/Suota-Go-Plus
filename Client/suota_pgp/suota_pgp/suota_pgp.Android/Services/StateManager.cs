using Plugin.BLE.Abstractions.Contracts;
using Prism.Events;
using Prism.Mvvm;
using suota_pgp.Data;
using suota_pgp.Services.Interface;

namespace suota_pgp.Droid.Services
{
    internal class StateManager : BindableBase, IStateManager
    {
        private IEventAggregator _aggregator;

        private AppState _appState;
        public AppState AppState
        {
            get => _appState;
            set => SetProperty(ref _appState, value);
        }

        private ErrorState _errorState;
        public ErrorState ErrorState
        {
            get => _errorState;
            protected set => SetProperty(ref _errorState, value);
        }

        public StateManager(IEventAggregator aggregator)
        {
            _aggregator = aggregator;
            _aggregator.GetEvent<ManagerEvents.BluetoothStateChangedEvent>().Subscribe(OnBluetoothStateChanged, ThreadOption.UIThread);
            _aggregator.GetEvent<AppEvents.PermissionStateChangedEvent>().Subscribe(OnPermissionStateChanged, ThreadOption.UIThread);
        }

        protected void OnAppStateChanged(AppState state)
        {
            AppState = state;
        }

        protected void OnBluetoothStateChanged(BluetoothState state)
        {
            if (state == BluetoothState.On)
            {
                ClearErrorFlag(ErrorState.BluetoothDisabled);
            }
            else if (state == BluetoothState.Off)
            {
                SetErrorFlag(ErrorState.BluetoothDisabled);
            }
        }

        protected void OnPermissionStateChanged(PermissionState state)
        {
            if (state.LocationAuthorized)
            {
                ClearErrorFlag(ErrorState.LocationUnauthorized);
            }
            else
            {
                SetErrorFlag(ErrorState.LocationUnauthorized);
            }

            if (state.StorageAuthorized)
            {
                ClearErrorFlag(ErrorState.StorageUnauthorized);
            }
            else
            {
                SetErrorFlag(ErrorState.StorageUnauthorized);
            }
        }

        protected void SetErrorFlag(ErrorState state)
        {
            ErrorState |= state;
        }

        protected void ClearErrorFlag(ErrorState state)
        {
            ErrorState &= ~state;
        }
    }
}