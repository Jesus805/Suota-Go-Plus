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

            await _bleManager.ConnectDevice(device);

            //_fileManager.LoadFirmware(fileName);

            // Enable Suota on Go+ device. 
            await _bleManager.WriteCharacteristic(device, Constants.GoPlusUpdateRequestUuid, new byte[] { 0x01 });
        }
    }
}