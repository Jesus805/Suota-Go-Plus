using suota_pgp.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace suota_pgp.Services
{
    public interface IFileManager
    {
        Task<List<string>> GetFirmwareFileNames();

        void LoadFirmware(string fileName);
        
        void SaveDeviceInfo(DeviceInfo info);
    }
}
