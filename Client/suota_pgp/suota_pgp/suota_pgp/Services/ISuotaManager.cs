using suota_pgp.Model;

namespace suota_pgp.Services
{
    public interface ISuotaManager
    {
        void RunSuota(GoPlus device, string fileName);
    }
}
