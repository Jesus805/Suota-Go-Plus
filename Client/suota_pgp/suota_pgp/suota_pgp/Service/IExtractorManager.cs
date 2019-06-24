using suota_pgp.Model;
using System.Threading.Tasks;

namespace suota_pgp
{
    public interface IExtractorManager
    {
        Task GetDeviceInfo(GoPlus device);
        Task RestoreDevice(GoPlus device);
    }
}
