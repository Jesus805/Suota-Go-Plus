using suota_pgp.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace suota_pgp.Services
{
    public interface IBleManager
    {
        Task ConnectDevice(GoPlus device);
        Task DisconnectDevice(GoPlus device);
        List<GoPlus> GetBondedDevices();
        Task NotifyRegister(GoPlus device, Guid characteristic);
        Task NotifyUnregister(GoPlus device, Guid characteristic);
        Task<byte[]> ReadCharacteristic(GoPlus device, Guid characteristic);
        void RemoveBond(GoPlus device);
        void Scan();
        void StopScan();
        Task WriteCharacteristic(GoPlus device, Guid characteristic, byte value, bool noResponse = false);
        Task WriteCharacteristic(GoPlus device, Guid characteristic, short value, bool noResponse = false);
        Task WriteCharacteristic(GoPlus device, Guid characteristic, int value, bool noResponse = false);
        Task WriteCharacteristic(GoPlus device, Guid characteristic, byte[] value, bool noResponse = false);
    }
}