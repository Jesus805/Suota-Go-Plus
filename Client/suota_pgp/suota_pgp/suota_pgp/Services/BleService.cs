using suota_pgp.Model;

namespace suota_pgp.Services
{
    public class BleService : IBleService
    {
        public KeyBlobPair KeyBlob { get; set; }

        public BleService()
        {
            KeyBlob = new KeyBlobPair();
        }

        public void GetKeyBlob()
        {
            KeyBlob.Blob = "Got Blob";
            KeyBlob.Key = "Got Key";
        }
    }
}
