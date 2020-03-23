using System.ComponentModel;

namespace suota_pgp
{
    public interface IStateManager : INotifyPropertyChanged
    {
        AppState State { get; set; }

        ErrorState ErrorState { get; }
    }
}
