using suota_pgp.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace suota_pgp.Services
{
    public interface IBleManager
    {
        Task<DeviceInfo> GetDeviceInfo(GoPlus device);

        List<GoPlus> GetPairedDevices();

        void Scan();

        void StopScan();

        Task ConnectDevice(GoPlus device);

        Task DisconnectDevice(GoPlus device);

        Task WriteCharacteristic(GoPlus device, Guid characteristic, int value);

        Task WriteCharacteristic(GoPlus device, Guid characteristic, byte[] value);

        Task<byte[]> ReadCharacteristic(GoPlus device, Guid characteristic);
    }
}
