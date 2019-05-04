using Prism.Commands;
using suota_pgp.Model;
using suota_pgp.Services;
using System.Text;

namespace suota_pgp
{
    public class KeyBlobViewModel : ViewModelBase
    {
        private IFileManager _fileService;
        private IBleManager _bleService;

        private KeyBlobPair _keyBlob;
        public KeyBlobPair KeyBlob
        {
            get => _keyBlob;
            private set => SetProperty(ref _keyBlob, value);
        }

        public DelegateCommand GetKeyBlobCommand { get; private set; }

        public DelegateCommand SaveFileCommand { get; private set; }

        public KeyBlobViewModel(IBleManager bleService, IFileManager fileService)
        {
            _bleService = bleService;
            _fileService = fileService;

            KeyBlob = new KeyBlobPair();
            GetKeyBlobCommand = new DelegateCommand(GetKeyBlob);
            SaveFileCommand = new DelegateCommand(SaveKeyBlob);
        }

        public void GetKeyBlob()
        {
            KeyBlob = _bleService.GetKeyBlob();
        }

        public void SaveKeyBlob()
        {
            _fileService.SaveKeyBlob(KeyBlob.Key, KeyBlob.Blob);
        }


    }
}
