using Plugin.Permissions;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using suota_pgp.Data;
using suota_pgp.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    internal class SuotaManager : BindableBase, ISuotaManager
    {
        private IBleManager _bleManager;
        private IEventAggregator _aggregator;
        private IFileManager _fileManager;
        private ILoggerFacade _logger;
        private INotifyManager _notifyManager;
        private IStateManager _stateManager;

        private bool _invalidImgBankExpected;
        private GoPlus _suotaDevice;
        private int _progressPercent;
        private object propLock = new object();

        private bool _isSuotaActive;
        private bool IsSuotaActive
        {
            get
            {
                lock (propLock)
                {
                    return _isSuotaActive;
                }
            }
            set
            {
                lock (propLock)
                {
                    _isSuotaActive = value;
                }
            }
        }

        private bool _cancelRequested;
        private bool CancelRequested
        {
            get
            {
                lock (propLock)
                {
                    return _cancelRequested;
                }
            }
            set
            {
                lock (propLock)
                {
                    _cancelRequested = value;
                }
            }
        }

        private bool _suotaFailure;
        public bool SuotaFailure
        {
            get => _suotaFailure;
            set => SetProperty(ref _suotaFailure, value);
        }

        /// <summary>
        /// Initialize a new instance of 'SuotaManager'.
        /// </summary>
        /// <param name="aggregator">Prism dependency injected 'IEventAggregator'.</param>
        /// <param name="bleManager">Prism dependency injected 'IBleManager'</param>
        /// <param name="fileManager">Prism dependency injected 'IFileManager'</param>
        /// <param name="logger">Prism dependency injected 'ILoggerFacade'</param>
        public SuotaManager(IEventAggregator aggregator,
                            IBleManager bleManager,
                            IFileManager fileManager,
                            ILoggerFacade logger,
                            INotifyManager notifyManager,
                            IStateManager stateManager)
        {
            _aggregator = aggregator;
            _bleManager = bleManager;
            _fileManager = fileManager;
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;

            _aggregator.GetEvent<AppEvents.CharacteristicUpdatedEvent>().Subscribe(OnCharacteristicNotify);
            _aggregator.GetEvent<AppEvents.GoPlusFoundEvent>().Subscribe(OnGoPlusFound);
        }

        /// <summary>
        /// Run Software Update Over The Air (SUOTA).
        /// </summary>
        /// <param name="device">Go+ device to update.</param>
        /// <param name="fileName">Firmware filename.</param>
        public async void RunSuota(GoPlus device, string fileName)
        {
            var locationStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);
            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);

            if (locationStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted &&
                storageStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                _notifyManager.ShowDialogErrorBox("Location and Storage Permissions are required to use SUOTA.");
                return;
            }
            else if (locationStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                _notifyManager.ShowDialogErrorBox("Location Permissions are required to use SUOTA.");
                return;
            }
            else if (storageStatus != Plugin.Permissions.Abstractions.PermissionStatus.Granted)
            {
                _notifyManager.ShowDialogErrorBox("Storage Permissions are required to use SUOTA.");
                return;
            }

            ResetState();

            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException("fileName");
            }

            _stateManager.AppState = AppState.Suota;
            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, "Loading Firmware"));
            
            try
            {
                // Load the file into memory and build blocks.
                _fileManager.LoadFirmware(fileName);

                _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, "Connecting to Bonded Go+"));
                await _bleManager.ConnectDevice(device);

                _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, "Enabling SUOTA on Go+"));
                _logger.Log("Enabling SUOTA on Go+", Category.Info, Priority.None);
                await _bleManager.WriteCharacteristic(device,
                                                      Constants.GoPlusUpdateRequestUuid,
                                                      Constants.EnableSuota);

                IsSuotaActive = true;

                _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, "Go+ Automatically disconnected, rescanning"));
                _logger.Log("Go+ Automatically disconnected, rescanning", Category.Info, Priority.None);
                _bleManager.Scan();
            }
            catch (Exception e)
            {
                _logger.Log($"Error {e.Message}", Category.Exception, Priority.High);
                CancelRequested = true;
                IsSuotaActive = false;
            }
        }

        /// <summary>
        /// In the event of an unexpected error. Cancel SUOTA.
        /// </summary>
        private async void CancelSuota()
        {
            // Exit SUOTA service
            // Note: The DA14580 reads memory devices and 
            // commands from the same characteristic.
            CancelRequested = true;
            int exitCommand = Constants.SpotaMemServiceExit << 24;
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaMemDevUuid,
                                                  exitCommand);
        }

        /// <summary>
        /// Reset SUOTA State.
        /// </summary>
        private void ResetState()
        {
            _isSuotaActive = false;
            _invalidImgBankExpected = false;
            _suotaDevice = null;
            _progressPercent = 0;
            SuotaFailure = false;
        }

        /// <summary>
        /// Step One:
        /// Connect to SUOTA device and set memory type.
        /// </summary>
        private async void StepOne()
        {
            if (CancelRequested)
                return;

            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, "Reconnecting to Go+"));
            _logger.Log("Go+ found!", Category.Info, Priority.None);
            _logger.Log("Attempting to reconnect to Go+", Category.Info, Priority.None);

            try
            {
                await _bleManager.ConnectDevice(_suotaDevice);

                // Begin listening to service status to intercept any errors and MemType confirmation.
                _logger.Log("Listening to SPOTA Service Status Characteristic", Category.Info, Priority.None);
                await _bleManager.NotifyRegister(_suotaDevice, Constants.SpotaServStatusUuid);

                // Set MemType
                int memType = (Constants.SpiMemoryType << 24) | Constants.MemoryBank;
                _logger.Log($"Setting MemType to 0x{memType.ToString("x2")}", Category.Info, Priority.None);
                _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Setting MemType to 0x{memType.ToString("x2")}"));
                await _bleManager.WriteCharacteristic(_suotaDevice, Constants.SpotaMemDevUuid, memType);
            }
            catch (Exception)
            {
                CancelRequested = true;
                IsSuotaActive = false;
            }
        }

        /// <summary>
        /// Step two:
        /// Set GPIO, blocksize, and begin writing data.
        /// </summary>
        private async void StepTwo()
        {
            if (CancelRequested)
                return;

            try
            {
                // Set GPIO Map
                int gpioMap = (Constants.SpiMiso << 24) | (Constants.SpiMosi << 16) |
                              (Constants.SpiCs << 8) | (Constants.SpiSck);
                _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing 0x{gpioMap.ToString("x2")} to GPIO Characteristic."));
                _logger.Log($"Writing 0x{gpioMap.ToString("x2")} to GPIO Characteristic.", Category.Info, Priority.None);
                await _bleManager.WriteCharacteristic(_suotaDevice,
                                                      Constants.SpotaGpioMapUuid,
                                                      gpioMap);
                _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing {Constants.BlockSize} to SPOTA patch length"));
                // Set SPOTA Patch Length
                _logger.Log($"Writing {Constants.BlockSize} to SPOTA patch length", Category.Info, Priority.None);
                await _bleManager.WriteCharacteristic(_suotaDevice,
                                                      Constants.SpotaPatchLenUuid,
                                                      (short)Constants.BlockSize);

                // Begin writing chunks.

                for (int i = 0; i < _fileManager.NumOfBlocks; i++)
                {
                    List<byte[]> chunks = _fileManager.GetChunks(i);

                    // Last Firmware Block
                    if (i == _fileManager.NumOfBlocks - 1)
                    {
                        chunks = _fileManager.GetChunks(i);
                        short finalBlockSize = 0;
                        foreach (byte[] chunk in chunks)
                        {
                            finalBlockSize += (short)chunk.Length;
                        }

                        if (finalBlockSize != Constants.BlockSize)
                        {
                            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing {finalBlockSize} to SPOTA patch length"));
                            // Set SPOTA Patch Length with last blocksize
                            _logger.Log($"Writing {finalBlockSize} to SPOTA patch length", Category.Info, Priority.None);
                            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                                  Constants.SpotaPatchLenUuid,
                                                                  finalBlockSize);
                        }
                    }

                    _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing block: {i + 1}"));
                    int j = 1;
                    foreach (byte[] chunk in chunks)
                    {
                        _logger.Log($"Writing block: {i + 1} chunk: {j++}", Category.Info, Priority.None);
                        await _bleManager.WriteCharacteristic(_suotaDevice,
                                                              Constants.SpotaPatchDataUuid,
                                                              chunk, true);
                    }
                }

                // After all the blocks are written, begin patching.
                BeginPatch();
            }
            catch(Exception)
            {
                CancelRequested = true;
                IsSuotaActive = false;
            }
        }

        /// <summary>
        /// Write CRC.
        /// </summary>
        private async Task WriteCrc()
        {
            // Set SPOTA Patch Length with last blocksize
            _logger.Log($"Writing 1 to SPOTA patch length for CRC", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaPatchLenUuid,
                                                  (short)1);

            _logger.Log($"Writing CRC", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaPatchDataUuid,
                                                  _fileManager.Crc, true);
        }

        /// <summary>
        /// Begin patching the header, set the device memory address.
        /// </summary>
        private async void BeginPatch()
        {
            _invalidImgBankExpected = true;

            // Calculate new address
            int address = Constants.PatchAddress - _fileManager.FileSize;
            // Create command
            int memType = (Constants.SpiMemoryType << 24) | address;

            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Patching Header: Setting MemType to 0x{memType.ToString("x2")}"));
            _logger.Log($"Setting MemType to 0x{memType.ToString("x2")}", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_suotaDevice, Constants.SpotaMemDevUuid, memType);
        }

        /// <summary>
        /// Write patch to device.
        /// </summary>
        private async void WritePatch()
        {
            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing 0x05 to SPOTA patch length"));
            // Set SPOTA Patch Length to the header size
            _logger.Log($"Writing {Constants.PatchLength} to SPOTA patch length", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaPatchLenUuid,
                                                  (short)Constants.PatchLength);

            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, "Patching Valid Flag"));
            // Send SPOTA Patch Data
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaPatchDataUuid,
                                                  _fileManager.Patch, true);

            await WriteCrc();

            // End Image Update
            // Note: The DA14580 reads memory devices and 
            // commands from the same characteristic.
            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Sending Image End command."));
            _logger.Log($"Sending Image End command.", Category.Info, Priority.None);
            int imgEndCommand = Constants.SpotaImgEnd << 24;
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaMemDevUuid,
                                                  imgEndCommand);
        }

        private async void RevertStepOne()
        {
            int memType = (Constants.SpiMemoryType << 24) | Constants.MemoryBank;
            _logger.Log($"Setting MemType to 0x{memType.ToString("x2")}", Category.Info, Priority.None);
            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Setting MemType to 0x{memType.ToString("x2")}"));
            await _bleManager.WriteCharacteristic(_suotaDevice, Constants.SpotaMemDevUuid, memType);
        }

        private async void RevertStepTwo()
        {
            // Set GPIO Map
            int gpioMap = (Constants.SpiMiso << 24) | (Constants.SpiMosi << 16) |
                          (Constants.SpiCs << 8) | (Constants.SpiSck);
            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing 0x{gpioMap.ToString("x2")} to GPIO Characteristic."));
            _logger.Log($"Writing 0x{gpioMap.ToString("x2")} to GPIO Characteristic.", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaGpioMapUuid,
                                                  gpioMap);

            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing {Constants.HeaderSize} to SPOTA patch length"));
            // Set SPOTA Patch Length
            _logger.Log($"Writing {Constants.HeaderSize} to SPOTA patch length", Category.Info, Priority.None);
            await _bleManager.WriteCharacteristic(_suotaDevice,
                                                  Constants.SpotaPatchLenUuid,
                                                  (short)Constants.HeaderSize);

            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Writing Block"));
            // Set SPOTA Patch Length
            List<byte[]> chunks = _fileManager.GetHeaderChunks();
            for (int i = 0; i < chunks.Count; i++)
            {
                _logger.Log($"Writing Block", Category.Info, Priority.None);
                await _bleManager.WriteCharacteristic(_suotaDevice,
                                                      Constants.SpotaPatchDataUuid,
                                                      chunks[i]);
            }

            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(_progressPercent++, $"Unregistering from Service Status Notification"));
            // Set SPOTA Patch Length
            _logger.Log($"Unregistering from Service Status Notification", Category.Info, Priority.None);
            await _bleManager.NotifyUnregister(_suotaDevice, Constants.SpotaServStatusUuid);

            await _bleManager.DisconnectDevice(_suotaDevice);

            _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(100, "Successfully wiped out corrupted image."));
            _logger.Log("Successfully wiped out corrupted image.", Category.Info, Priority.None);
        }

        #region Events

        /// <summary>
        /// 
        /// </summary>
        /// <param name="charUpdate"></param>
        private void OnCharacteristicNotify(CharacteristicUpdate charUpdate)
        {
            // Ignore if not performing SUOTA
            if (!_isSuotaActive)
                return;

            if (charUpdate == null)
            {
                _logger.Log("Encountered null characteristic, ignoring", Category.Exception, Priority.High);
                return;
            }

            if (charUpdate.Uuid == Constants.SpotaServStatusUuid)
            {
                SpotarStatusUpdate status = (SpotarStatusUpdate)charUpdate.IntValue;
                switch (status)
                {
                    case SpotarStatusUpdate.ImgStarted:
                        if (SuotaFailure)
                        {
                            RevertStepTwo();
                        }
                        else
                        {
                            _logger.Log("Received SpotarImgStarted", Category.Info, Priority.None);
                            StepTwo();
                        }
                        break;
                    case SpotarStatusUpdate.InvalidImgBank:
                        if (_invalidImgBankExpected)
                        {
                            _logger.Log("Received Invalid Image Bank, but it was expected", Category.Info, Priority.None);
                            _invalidImgBankExpected = false;
                            WritePatch();
                        }
                        else
                        {
                            _logger.Log("\"Invalid image bank\" error returned from device.", Category.Exception, Priority.High);
                        }
                        break;
                    case SpotarStatusUpdate.CrcError:
                        _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(0, "Critical Error: CRC error, reverting changes."));
                        _logger.Log("CRC Error, attempting to revert back", Category.Info, Priority.None);
                        SuotaFailure = true;
                        RevertStepOne();
                        break;
                    case SpotarStatusUpdate.InvalidImgHeader:
                        _logger.Log("\"Invalid Image header\" error returned from device. Please make sure you have a valid firmware.", Category.Exception, Priority.High);
                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.InvalidImgSize:
                        _logger.Log("Invalid Image Size returned. Please make sure you have a valid firmware.", Category.Exception, Priority.High);
                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.InvalidProductHeader:
                        _logger.Log("Invalid Product Header returned, there is something wrong with your GO+. This should never happen.", Category.Exception, Priority.High);
                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.GoPlusFailedIntegrity:
                        _logger.Log("PGP Failed integrity, no biggie, we patched the image header", Category.Info, Priority.None);
                        _bleManager.NotifyUnregister(_suotaDevice, Constants.SpotaServStatusUuid);
                        _bleManager.DisconnectDevice(_suotaDevice);
                        _bleManager.RemoveBond(_suotaDevice);
                        _aggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Publish(new SuotaProgress(100, "Finished", true));
                        _notifyManager.ShowDialogInfoBox("Update Complete. Please restart your Pokemon GO Plus if it doesn't show up");
                        ResetState();
                        break;
                    case SpotarStatusUpdate.PatchLengthError:
                        _logger.Log("Patch Length Error", Category.Exception, Priority.High);
                        _notifyManager.ShowDialogErrorBox("Critical Error! Please see the log for details.");
                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.IntMemError:
                        _logger.Log("Internal Memory Error (Not enough space for Patch)", Category.Exception, Priority.High);
                        _notifyManager.ShowDialogErrorBox("Critical Error! Please see the log for details.");
                        CancelSuota();
                        break;
                    default:
                        _logger.Log($"Received value {charUpdate.IntValue}", Category.Debug, Priority.None);
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="device"></param>
        private void OnGoPlusFound(GoPlus device)
        {
            // Ignore if not performing SUOTA or if a device was already found
            if (!IsSuotaActive)
                return;

            if (device == null)
            {
                _logger.Log("Encountered null device, ignoring", Category.Exception, Priority.High);
                return;
            }

            _bleManager.StopScan();

            _suotaDevice = device;
            StepOne();
        }

        #endregion
    }
}
 