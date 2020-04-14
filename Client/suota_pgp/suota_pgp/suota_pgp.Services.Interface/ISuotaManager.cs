using suota_pgp.Data;

namespace suota_pgp.Services.Interface
{
    public interface ISuotaManager
    {
        void RunSuota(GoPlus device, string fileName);
    }
}
