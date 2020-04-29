using Prism.Services.Dialogs;

namespace suota_pgp.Services.Interface
{
    public interface INotifyManager : IDialogService
    {
        void ShowToast(string name, IDialogParameters dialogParameters);
    }
}
