using Prism.Mvvm;

namespace suota_pgp.Data
{
    /// <summary>
    /// Pokemon Go Plus model.
    /// </summary>
    public class GoPlus : BindableBase
    {
        /// <summary>
        /// Instance Name.
        /// </summary>
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    RaisePropertyChanged(nameof(IsComplete));
                }
            }
        }

        /// <summary>
        /// Bluetooth Address.
        /// </summary>
        private string _btAddress;
        public string BtAddress
        {
            get => _btAddress;
            set
            {
                if (SetProperty(ref _btAddress, value))
                {
                    RaisePropertyChanged(nameof(IsComplete));
                }
            }
        }

        /// <summary>
        /// 16 byte unique device key.
        /// </summary>
        private string _deviceKey;
        public string DeviceKey
        {
            get => _deviceKey;
            set
            {
                if (SetProperty(ref _deviceKey, value))
                {
                    RaisePropertyChanged(nameof(IsComplete));
                }
            }
        }

        /// <summary>
        /// 256 byte unique blob key.
        /// </summary>
        private string _blobKey;
        public string BlobKey
        {
            get => _blobKey;
            set
            {
                if (SetProperty(ref _blobKey, value))
                {
                    RaisePropertyChanged(nameof(IsComplete));
                }
            }
        }

        /// <summary>
        /// returns true if all fields are filled in; false otherwise.
        /// </summary>
        public bool IsComplete
        {
            get => !string.IsNullOrEmpty(Name) &&
                   !string.IsNullOrEmpty(BtAddress) &&
                   !string.IsNullOrEmpty(DeviceKey) &&
                   !string.IsNullOrEmpty(BlobKey);
        }

        /// <summary>
        /// Initializes a new Instance of 'GoPlus'
        /// </summary>
        public GoPlus() { }
    }
}
