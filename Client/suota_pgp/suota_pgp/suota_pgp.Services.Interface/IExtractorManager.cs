using suota_pgp.Data;
using System.Threading.Tasks;

namespace suota_pgp.Services.Interface
{
    public interface IExtractorManager
    {
        Task GetDeviceInfo(GoPlus device);
        Task RestoreDevice(GoPlus device);
    }
}
