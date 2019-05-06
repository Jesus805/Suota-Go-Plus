using suota_pgp.Model;
using System.Threading.Tasks;

namespace suota_pgp.Services
{
    public interface IBleManager
    {
        Task<DeviceInfo> GetDeviceInfo();

        void Scan();

        void StopScan();
    }
}
