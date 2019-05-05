using suota_pgp.Model;

namespace suota_pgp.Services
{
    public interface IFileManager
    {
        void SaveDeviceInfo(DeviceInfo info);
    }
}
