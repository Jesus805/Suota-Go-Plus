using Prism.Events;
using suota_pgp.Model;
using suota_pgp.Services;
using System;

namespace suota_pgp.Droid.Services
{
    public class SuotaManager : ISuotaManager
    {
        private IEventAggregator _aggregator;
        private IBleManager _bleManager;
        private IFileManager _fileManager;

        public SuotaManager(IEventAggregator aggregator,
                            IBleManager bleManager,
                            IFileManager fileManager)
        {
            _aggregator = aggregator;
            _bleManager = bleManager;
            _fileManager = fileManager;
        }

        public async void BeginSuota(GoPlus device, string fileName)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }
            
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            //_fileManager.LoadFirmware(fileName);

            await _bleManager.ConnectDevice(device);

            // Enable Suota on Go+ device.
            await _bleManager.WriteCharacteristic(device, Constants.GoPlusUpdateRequestUuid, new byte[] { 0x01 });

            // Reconnect to Go+
            await _bleManager.ConnectDevice(device);

            // Set MemType()
            int memType = (Constants.SpiMemTypeExternal << 24) | Constants.MemoryBank;
            await _bleManager.WriteCharacteristic(device, Constants.SpotaMemDevUuid, memType);

            // Set GPIO Map
            int gpioMap = 0;
            await _bleManager.WriteCharacteristic(device, Constants.SpotaGpioMapUuid, gpioMap);

            // Set SPOTA Patch Length
            int patchLength = 0;
            await _bleManager.WriteCharacteristic(device, Constants.SpotaPatchLenUuid, patchLength);



            // Send Block
            byte[] sendBlock = new byte[3];
            await _bleManager.WriteCharacteristic(device, Constants.SpotaPatchDataUuid, sendBlock);


        }


    }
}