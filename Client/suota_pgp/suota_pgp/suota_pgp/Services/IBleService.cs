using suota_pgp.Model;

namespace suota_pgp.Services
{
    public interface IBleService
    {
        KeyBlobPair KeyBlob { get; set; }

        void GetKeyBlob();
    }
}
