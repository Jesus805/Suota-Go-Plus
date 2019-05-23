using Prism.Mvvm;

namespace suota_pgp.Model
{
    /// <summary>
    /// Pokemon Go Plus identifier.
    /// </summary>
    public class GoPlus : BindableBase
    {
        /// <summary>
        /// Pokemon GO Plus' Name.
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        /// <summary>
        /// Pokemon GO Plus' Bluetooth Address.
        /// </summary>
        private string _btAddress;
        public string BtAddress
        {
            get => _btAddress;
            set => SetProperty(ref _btAddress, value);
        }
        /// <summary>
        /// 16 byte unique key
        /// </summary>
        private string _key;
        public string Key
        {
            get => _key;
            set => SetProperty(ref _key, value);
        }
        /// <summary>
        /// 256 byte unique blob
        /// </summary>
        private string _blob;
        public string Blob
        {
            get => _blob;
            set => SetProperty(ref _blob, value);
        }

        public GoPlus()
        {
            Name = string.Empty;
            BtAddress = string.Empty;
            Key = string.Empty;
            Blob = string.Empty;
        }
    }
}
