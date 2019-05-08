using suota_pgp.Model;

namespace suota_pgp.Services
{
    public interface ISuotaManager
    {
        void BeginSuota(GoPlus device, string fileName);
    }
}
