using suota_pgp.Data;
using System.ComponentModel;

namespace suota_pgp.Services.Interface
{
    public interface IStateManager : INotifyPropertyChanged
    {
        AppState AppState { get; set; }

        ErrorState ErrorState { get; set; }

        void SetErrorFlag(ErrorState state);

        void ClearErrorFlag(ErrorState state);
    }
}
