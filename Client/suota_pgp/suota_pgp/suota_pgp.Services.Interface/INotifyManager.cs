using Prism.Services.Dialogs;
using suota_pgp.Infrastructure;

namespace suota_pgp.Services.Interface
{
    public interface INotifyManager : IDialogService
    {
        void ShowToast(string name, IToastParameters dialogParameters);
    }
}
