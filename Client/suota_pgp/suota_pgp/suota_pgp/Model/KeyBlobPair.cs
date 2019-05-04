using Prism.Mvvm;

namespace suota_pgp.Model
{
    public class KeyBlobPair : BindableBase
    {
        private string _btAddress;
        public string BtAddress
        {
            get => _btAddress;
            set => SetProperty(ref _btAddress, value);
        }

        private string _key;
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }

        private string _blob;
        public string Blob
        {
            get => _blob;
            set => SetProperty(ref _blob, value);
        }

        public KeyBlobPair()
        {
            Key = string.Empty;
            Blob = string.Empty;
        }
    }
}
