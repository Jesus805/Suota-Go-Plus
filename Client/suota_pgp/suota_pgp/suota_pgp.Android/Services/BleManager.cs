using suota_pgp.Model;
using suota_pgp.Services;

namespace suota_pgp.Droid.Services
{
    class BleManager : IBleManager
    {
        public BleManager()
        {
        }

        public DeviceInfo GetDeviceInfo()
        {
            DeviceInfo result = new DeviceInfo
            {
                BtAddress = "Got Bluetooth Address",
                Blob = "Got Blob",
                Key = "Got Key"
            };

            return result;
        }
    }
}