using Prism.Mvvm;

namespace suota_pgp.Data
{
    public class PatchFile : BindableBase
    {
        private string _path;
        public string Path { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        /// <summary>
        /// Contents of the firmware file.
        /// </summary>
        private byte[] _firmware;

        public PatchFile()
        {

        }

        /*
        /// <summary>
        /// Calculate firmware CRC. 
        /// </summary>
        /// <param name="firmware">Contents of the firmware file.</param>
        private void CalculateCRC()
        {
            byte crc = 0;

            for (int i = 0; i < _firmware.Length; i++)
            {
                crc ^= _firmware[i];
            }

            // Add ValidFlag to CRC calculation.
            for (int i = 0; i < Constants.PatchLength; i++)
            {
                crc ^= Patch[i];
            }

            Crc = crc;
        }
        */
    }
}
