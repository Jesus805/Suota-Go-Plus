using Prism.Events;
using Prism.Logging;
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
        private ILoggerFacade _logger;
        private bool _IsSuotaActive;
        private GoPlus _SuotaDevice;

        public SuotaManager(IEventAggregator aggregator,
                            IBleManager bleManager,
                            IFileManager fileManager,
                            ILoggerFacade logger)
        {
            _aggregator = aggregator;
            _bleManager = bleManager;
            _fileManager = fileManager;
            _logger = logger;

            _aggregator.GetEvent<PrismEvents.CharacteristicUpdatedEvent>().Subscribe(OnServiceStatusUpdated);
            _aggregator.GetEvent<PrismEvents.GoPlusFoundEvent>().Subscribe(OnGoPlusFound);
        }

        public async void RunSuota(GoPlus device, string fileName)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            // Load the file into memory
            //_fileManager.LoadFirmware(fileName);

            await _bleManager.ConnectDevice(device);

            _logger.Log("Enabling SUOTA on Go+", Category.Info, Priority.None);
            // Enable Suota on Go+ device.
            await _bleManager.WriteCharacteristic(device, 
                                                  Constants.GoPlusUpdateRequestUuid, 
                                                  Constants.EnableSuota);

            _IsSuotaActive = true;

            _logger.Log("Go+ Automatically disconnected, rescanning", Category.Info, Priority.None);
            _bleManager.Scan();
        }

        public async void ContinueSuota()
        {
            // Set GPIO Map
            int gpioMap = 0;
            _logger.Log($"Writing \"{gpioMap}\" to GPIO Characteristic.", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_SuotaDevice,
                                                  Constants.SpotaGpioMapUuid, 
                                                  gpioMap);

            // Set SPOTA Patch Length
            _logger.Log($"Writing \"{Constants.BlockSize}\" to SPOTA patch length", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_SuotaDevice,
                                                  Constants.SpotaPatchLenUuid,
                                                  Constants.BlockSize);

            
            _logger.Log("Disconnecting from PGP", Category.Info, Priority.None);
            await _bleManager.DisconnectDevice(_SuotaDevice);
        }

        public void OnServiceStatusUpdated(CharValue charValue)
        {
            // Ignore if not performing SUOTA
            if (!_IsSuotaActive)
                return;

            if (charValue == null)
            {
                _logger.Log("Encountered null charValue, ignoring", Category.Exception, Priority.High);
                return;
            }

            if (charValue.IntValue == Constants.SpotarImgStarted)
            {
                _logger.Log("Received SpotarImgStarted", Category.Debug, Priority.None);

                ContinueSuota();
            }
        }

        public async void OnGoPlusFound(GoPlus device)
        {
            // Ignore if not performing SUOTA
            if (!_IsSuotaActive)
                return;
            
            if (device == null)
            {
                _logger.Log("Encountered null device, ignoring", Category.Exception, Priority.High);
                return;
            }

            _logger.Log("Go+ found!", Category.Info, Priority.None);

            _logger.Log("Attempting to reconnect to Go+", Category.Info, Priority.None);
            _SuotaDevice = device;
            await _bleManager.ConnectDevice(device);

            // Reconnect to Go+
            _logger.Log("Listening to SPOTA Service status characteristic", Category.Info, Priority.None);
            await _bleManager.NotifyRegister(device, Constants.SpotaServStatusUuid);

            // Set MemType
            int memType = (Constants.SpiMemTypeExternal << 24) | Constants.MemoryBank;
            _logger.Log($"Setting MemType to {memType}", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(device, Constants.SpotaMemDevUuid, memType);
        }
    }
}
 