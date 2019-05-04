using Prism.Commands;
using suota_pgp.Model;
using suota_pgp.Services;

namespace suota_pgp
{
    public class KeyBlobViewModel : ViewModelBase
    {
        private IBleService _bleService;

        public KeyBlobPair KeyBlob { get; set; }

        public DelegateCommand GetKeyBlobCommand { get; set; }

        public KeyBlobViewModel(IBleService bleService)
        {
            _bleService = bleService;
            GetKeyBlobCommand = new DelegateCommand(GetKeyBlob);
            KeyBlob = new KeyBlobPair();
        }

        public void GetKeyBlob()
        {
            _bleService.GetKeyBlob();
            KeyBlob.Key = _bleService.KeyBlob.Key;
            KeyBlob.Blob = _bleService.KeyBlob.Blob;
        }
    }
}
