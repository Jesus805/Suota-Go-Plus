using Prism.Mvvm;

namespace suota_pgp.Model
{
    /// <summary>
    /// Pokemon Go Plus device information.
    /// </summary>
    public class DeviceInfo : BindableBase
    {
        /// <summary>
        /// Bluetooth address.
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

        /// <summary>
        /// Initialize a new instance of 'PgpUniqueInfo'.
        /// </summary>
        public DeviceInfo()
        {
            BtAddress = string.Empty;
            Key = string.Empty;
            Blob = string.Empty;
        }
    }
}
