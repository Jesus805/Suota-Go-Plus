using Android.Content.Res;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using suota_pgp.Data;
using suota_pgp.Infrastructure;
using suota_pgp.Services.Interface;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Resources = suota_pgp.Droid.Properties.Resources;

namespace suota_pgp.Droid.Services
{
    internal class SuotaManager : BindableBase, ISuotaManager
    {
        private readonly IBleManager _bleManager;
        private readonly IEventAggregator _eventAggregator;
        private readonly IFileManager _fileManager;
        private readonly ILoggerFacade _logger;
        private readonly INotifyManager _notifyManager;
        private readonly IStateManager _stateManager;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _invalidImgBankExpected;
        private int _progressPercent;
        private GoPlus _suotaDevice;

        public event EventHandler<ProgressUpdateEventArgs> ProgressUpdate = delegate { };

        private bool _revertChanges;
        public bool RevertChanges
        {
            get => _revertChanges;
            set => SetProperty(ref _revertChanges, value);
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SuotaManager"/>
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="bleManager"></param>
        /// <param name="fileManager"></param>
        /// <param name="logger"></param>
        /// <param name="stateManager"></param>
        public SuotaManager(IEventAggregator eventAggregator,
                            IBleManager bleManager,
                            IFileManager fileManager,
                            ILoggerFacade logger,
                            INotifyManager notifyManager,
                            IStateManager stateManager)
        {
            _bleManager = bleManager;
            _eventAggregator = eventAggregator;
            _fileManager = fileManager;
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Run Software Update Over The Air (SUOTA).
        /// </summary>
        /// <param name="device">Go+ device to update.</param>
        /// <param name="fileName">Firmware filename.</param>
        public async void RunSuota(GoPlus device, string fileName)
        {
            Reset();

            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            _stateManager.AppState = AppState.Suota;
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.LoadingFirmware));
            
            try
            {
                // Load the file into memory and build blocks.
                _fileManager.LoadFirmware(fileName);

                ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.ConnectingToBondedGoPlus));
                _logger.Log(Resources.ConnectingToBondedGoPlus, Category.Info, Priority.None);

                await Run(() => 
                { 
                    return device.Connect(); 
                });
                

                ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.EnablingSuotaGoPlus));
                _logger.Log(Resources.EnablingSuotaGoPlus, Category.Info, Priority.None);

                await Run(() =>
                {
                    return device.WriteCharacteristic(Constants.GoPlusUpdateRequestUuid, Constants.EnableSuota);
                });

                ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.GoPlusDisconnectedReconnecting));
                _logger.Log(Resources.GoPlusDisconnectedReconnecting, Category.Info, Priority.None);

                for (int i = 0; i < Constants.RetryCount; i++)
                {
                    try
                    {
                        _suotaDevice = await _bleManager.ConnectToKnownDevice(device.Id);

                        if (_suotaDevice == null)
                        {
                            _logger.Log("WARNING: _suotaDevice is null", Category.Exception, Priority.High);
                        }
                        else
                        {

                            foreach (var record in _suotaDevice.Device.AdvertisementRecords)
                            {
                                Guid guid = ByteArrayHelper.ByteArrayToGuid(record.Data);
                                _logger.Log($"{record.Type} {guid}", Category.Exception, Priority.High);
                            }

                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        if (i < Constants.RetryCount - 1)
                        {
                            _logger.Log($"ConnectToKnownDevice threw an exception: {e.Message}", Category.Exception, Priority.High);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }

                if (_suotaDevice.IsConnected)
                {
                    _logger.Log("Suota Device connected!", Category.Debug, Priority.High);
                }
                else
                {
                    _logger.Log("Failed to connect to PGP :(", Category.Warn, Priority.High);
                }

                // Begin listening to service status to intercept any errors and MemType confirmation.
                _logger.Log(Resources.SpotaServiceRegister, Category.Info, Priority.None);

                _suotaDevice.CharacteristicNotification += SuotaDevice_CharacteristicNotification;
                await Run(() =>
                {
                    return _suotaDevice.NotifyRegister(Constants.SpotaServStatusUuid);
                });

                // Set MemType
                int memType = (Constants.SpiMemoryType << 24) | Constants.MemoryBank;

                ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, string.Format(Resources.SettingMemType, memType.ToString("x2"))));
                _logger.Log(string.Format(Resources.SettingMemType, memType.ToString("x2")), Category.Info, Priority.None);
                await Run(() =>
                {
                    return _suotaDevice.WriteCharacteristic(Constants.SpotaMemDevUuid, memType);
                });
            }
            catch (Exception e)
            {
                _logger.Log($"Error {e.Message}", Category.Exception, Priority.High);
            }
        }

        public async Task Run(Func<Task> action)
        {
            for (int i = 0; i < Constants.RetryCount; i++)
            {
                try
                {
                    await action.Invoke();
                    break;
                }
                catch (Exception e)
                {
                    if (i < Constants.RetryCount - 1)
                    {
                        string message = string.Format("Failed: {0} {1}", e.Message, Constants.RetryCount - i + 1);
                        _logger.Log(message, Category.Exception, Priority.High);
                    }
                    else
                    {
                        throw e;
                    }
                }
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
            int exitCommand = Constants.SpotaMemServiceExit << 24;
            await _suotaDevice.WriteCharacteristic(Constants.SpotaMemDevUuid, exitCommand);
        }

        /// <summary>
        /// Reset SUOTA State.
        /// </summary>
        private void Reset()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _invalidImgBankExpected = false;
            _suotaDevice = null;
            _progressPercent = 0;
            RevertChanges = false;
        }

        /// <summary>
        /// Set GPIO, blocksize, and begin writing data.
        /// </summary>
        private async void WriteFirmware()
        {
            try
            {
                // Set GPIO Map
                int gpioMap = (Constants.SpiMiso << 24) | (Constants.SpiMosi << 16) |
                              (Constants.SpiCs   <<  8) | (Constants.SpiSck);

                string message = string.Format(Resources.WritingGpio, gpioMap.ToString("x2"));

                ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
                _logger.Log(message, Category.Info, Priority.None);

                await _suotaDevice.WriteCharacteristic(Constants.SpotaGpioMapUuid, gpioMap);

                message = string.Format(Resources.WritingSpotaPatchLength, Constants.BlockSize);

                // Set SPOTA Patch Length
                ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
                _logger.Log(message, Category.Info, Priority.None);

                await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchLenUuid, (short)Constants.BlockSize);

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
                            message = string.Format(Resources.WritingSpotaPatchLength, finalBlockSize);

                            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
                            _logger.Log(message, Category.Info, Priority.None);

                            // Set SPOTA Patch Length with last blocksize
                            await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchLenUuid, finalBlockSize);
                        }
                    }

                    message = string.Format(Resources.WritingBlock, i + 1);
                    ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));

                    int j = 1;
                    foreach (byte[] chunk in chunks)
                    {
                        message = string.Format(Resources.WritingBlock, i + 1, j++);
                        _logger.Log(message, Category.Info, Priority.None);

                        await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchDataUuid, chunk, true);
                    }
                }

                // After all the blocks are written, begin patching.
                BeginPatch();
            }
            catch(Exception)
            {

            }
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

            string message = string.Format(Resources.SettingMemType, memType.ToString("x2"));

            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
            _logger.Log(message, Category.Info, Priority.None);

            await _suotaDevice.WriteCharacteristic(Constants.SpotaMemDevUuid, memType);
        }

        /// <summary>
        /// Write patch to device.
        /// </summary>
        private async void WritePatch()
        {
            // Set SPOTA Patch Length to the header size
            string message = string.Format(Resources.WritingSpotaPatchLength, Constants.PatchLength);
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
            _logger.Log(message, Category.Info, Priority.None);

            await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchLenUuid, (short)Constants.PatchLength);

            // Send SPOTA Patch Data
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.PatchingValidFlag));
            _logger.Log(message, Category.Info, Priority.None);

            await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchDataUuid, _fileManager.Patch, true);

            // Overwrite CRC
            await WriteCrc();

            // End Image Update
            // Note: The DA14580 reads memory devices and 
            // commands from the same characteristic.
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.SendingImageEnd));
            _logger.Log(Resources.SendingImageEnd, Category.Info, Priority.None);

            int imgEndCommand = Constants.SpotaImgEnd << 24;
            await _suotaDevice.WriteCharacteristic(Constants.SpotaMemDevUuid, imgEndCommand);
        }

        private async void RevertStepOne()
        {
            int memType = (Constants.SpiMemoryType << 24) | Constants.MemoryBank;

            string message = string.Format(Resources.SettingMemType, memType.ToString("x2"));

            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
            _logger.Log(message, Category.Info, Priority.None);
            
            await _suotaDevice.WriteCharacteristic(Constants.SpotaMemDevUuid, memType);
        }

        private async void RevertStepTwo()
        {
            // Set GPIO Map
            int gpioMap = (Constants.SpiMiso << 24) | (Constants.SpiMosi << 16) |
                          (Constants.SpiCs   <<  8) | (Constants.SpiSck);

            string message = string.Format(Resources.WritingGpio, gpioMap.ToString("x2"));

            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
            _logger.Log(message, Category.Info, Priority.None);

            await _suotaDevice.WriteCharacteristic(Constants.SpotaGpioMapUuid, gpioMap);

            message = string.Format(Resources.WritingSpotaPatchLength, Constants.HeaderSize);
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));
            _logger.Log(message, Category.Info, Priority.None);

            // Set SPOTA Patch Length
            await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchLenUuid, (short)Constants.HeaderSize);

            message = string.Format(Resources.WritingBlock, 0);
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, message));

            // Set SPOTA Patch Length
            List<byte[]> chunks = _fileManager.GetHeaderChunks();
            for (int i = 0; i < chunks.Count; i++)
            {
                message = string.Format(Resources.WritingChunk, 0, i);
                _logger.Log(message, Category.Info, Priority.None);

                await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchDataUuid, chunks[i]);
            }

            // Service unregister
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.SpotaServiceUnregister));
            _logger.Log(Resources.SpotaServiceUnregister, Category.Info, Priority.None);

            await _suotaDevice.NotifyUnregister(Constants.SpotaServStatusUuid);

            await _suotaDevice.Disconnect();

            // Log wipe success
            ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(100, Resources.WipeSuccessful));
            _logger.Log(Resources.WipeSuccessful, Category.Info, Priority.None);
        }

        /// <summary>
        /// Write CRC.
        /// </summary>
        private async Task WriteCrc()
        {
            // Set SPOTA Patch Length with last blocksize
            _logger.Log(string.Format(Resources.WritingSpotaPatchLength, 1), Category.Info, Priority.None);
            await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchLenUuid, (short)1, true);

            // Write CRC byte
            _logger.Log(Resources.WritingCrc, Category.Info, Priority.None);
            await _suotaDevice.WriteCharacteristic(Constants.SpotaPatchDataUuid, _fileManager.Crc, true);
        }

        #region Events

        private void SuotaDevice_CharacteristicNotification(object sender, CharacteristicNotificationEventArgs e)
        {
            DialogParameters parameters;

            if (e.Uuid == Constants.SpotaServStatusUuid)
            {
                SpotarStatusUpdate status = (SpotarStatusUpdate)e.IntValue;

                switch (status)
                {
                    case SpotarStatusUpdate.ImgStarted:

                        if (RevertChanges)
                        {
                            RevertStepTwo();
                        }
                        else
                        {
                            _logger.Log(Resources.ReceivedImgStarted, Category.Info, Priority.None);
                            // WriteFirmware();
                        }

                        break;
                    case SpotarStatusUpdate.InvalidImgBank:

                        if (_invalidImgBankExpected)
                        {
                            _logger.Log(Resources.ExpectedInvalidImageBankError, Category.Info, Priority.None);
                            _invalidImgBankExpected = false;

                            WritePatch();
                        }
                        else
                        {
                            _logger.Log(Resources.InvalidImageBankError, Category.Exception, Priority.High);
                        }

                        break;
                    case SpotarStatusUpdate.CrcError:

                        ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(_progressPercent++, Resources.WritingCrc));
                        _logger.Log(Resources.WritingCrc, Category.Info, Priority.None);

                        RevertChanges = true;
                        RevertStepOne();
                        break;
                    case SpotarStatusUpdate.InvalidImgHeader:

                        _logger.Log(Resources.InvalidImageHeaderError, Category.Exception, Priority.High);

                        parameters = new DialogParameters()
                        {
                            { DialogParameterKeys.Title, Resources.UpdateCompleteTitle },
                            { DialogParameterKeys.Message, Resources.InvalidImageHeaderError }
                        };
                        _notifyManager.ShowDialog(null, parameters);

                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.InvalidImgSize:
                        
                        _logger.Log(Resources.InvalidImageSizeError, Category.Exception, Priority.High);

                        parameters = new DialogParameters()
                        {
                            { DialogParameterKeys.Title, Resources.UpdateCompleteTitle },
                            { DialogParameterKeys.Message, Resources.InvalidImageSizeError }
                        };
                        _notifyManager.ShowDialog(null, parameters);

                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.InvalidProductHeader:
                        
                        _logger.Log(Resources.InvalidProductHeaderError, Category.Exception, Priority.High);

                        parameters = new DialogParameters()
                        {
                            { DialogParameterKeys.Title, Resources.UpdateCompleteTitle },
                            { DialogParameterKeys.Message, Resources.InvalidProductHeaderError }
                        };
                        _notifyManager.ShowDialog(null, parameters);

                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.GoPlusFailedIntegrity:

                        _logger.Log(Resources.PgpFailedIntegrityError, Category.Info, Priority.None);
                        _suotaDevice.NotifyUnregister(Constants.SpotaServStatusUuid);
                        _suotaDevice.Disconnect();
                        _bleManager.RemoveBond(_suotaDevice);

                        ProgressUpdate.Invoke(this, new ProgressUpdateEventArgs(100, Resources.Finished));
                        _logger.Log(Resources.Finished, Category.Info, Priority.None);

                        parameters = new DialogParameters()
                        {
                            { DialogParameterKeys.Title, Resources.UpdateCompleteTitle },
                            { DialogParameterKeys.Message, Resources.UpdateCompleteMessage }
                        };
                        _notifyManager.ShowDialog(null, parameters);

                        Reset();

                        break;
                    case SpotarStatusUpdate.PatchLengthError:

                        _logger.Log(Resources.PatchLengthError, Category.Exception, Priority.High);

                        parameters = new DialogParameters()
                        {
                            { DialogParameterKeys.Title, Resources.CriticalErrorTitle },
                            { DialogParameterKeys.Message, Resources.PatchLengthError }
                        };
                        _notifyManager.ShowDialog(null, parameters);

                        CancelSuota();
                        break;
                    case SpotarStatusUpdate.IntMemError:

                        _logger.Log(Resources.InternalMemoryError, Category.Exception, Priority.High);

                        parameters = new DialogParameters()
                        {
                            { DialogParameterKeys.Title, Resources.CriticalErrorTitle },
                            { DialogParameterKeys.Message, Resources.InternalMemoryError }
                        };
                        _notifyManager.ShowDialog(null, parameters);

                        CancelSuota();
                        break;
                    default:

                        _logger.Log(string.Format(Resources.ReceivedNotificationValue, e.IntValue), Category.Debug, Priority.None);
                        break;
                }
            }
        }

        #endregion
    }
}
 