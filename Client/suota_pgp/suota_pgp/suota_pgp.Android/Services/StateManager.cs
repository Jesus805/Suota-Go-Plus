using Prism.Mvvm;
using suota_pgp.Data;
using suota_pgp.Services.Interface;

namespace suota_pgp.Droid.Services
{
    internal class StateManager : BindableBase, IStateManager
    {
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
            set => SetProperty(ref _errorState, value);
        }

        public StateManager() { }

        public void SetErrorFlag(ErrorState state)
        {
            ErrorState |= state;
        }

        public void ClearErrorFlag(ErrorState state)
        {
            ErrorState &= ~state;
        }
    }
}