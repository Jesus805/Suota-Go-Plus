using suota_pgp.Model;
using suota_pgp.Services;

namespace suota_pgp.Droid.Services
{
    class BleManager : IBleManager
    {
        public BleManager()
        {
        }

        public KeyBlobPair GetKeyBlob()
        {
            KeyBlobPair result = new KeyBlobPair
            {
                Blob = "Got Blob",
                Key = "Got Key"
            };

            return result;
        }
    }
}