using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using suota_pgp.Data;
using suota_pgp.Infrastructure;
using suota_pgp.Services.Interface;
using System;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    internal class KeyExtractManager : BindableBase, IKeyExtractManager
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILoggerFacade _logger;
        private readonly INotifyManager _notifyManager;
        private readonly IStateManager _stateManager;

        /// <summary>
        /// Initialize a new instance of <see cref="KeyExtractManager"/>.
        /// </summary>
        /// <param name="eventAggregator"></param>
        /// <param name="logger"></param>
        /// <param name="notifyManager"></param>
        /// <param name="stateManager"></param>
        public KeyExtractManager(IEventAggregator eventAggregator,
                                 ILoggerFacade logger,
                                 INotifyManager notifyManager,
                                 IStateManager stateManager)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;
        }

        /// <summary>
        /// Get the PGP device and blob key.
        /// </summary>
        /// <param name="device">Device to get keys from.</param>
        /// <returns>Async task to await.</returns>
        public async Task GetDeviceInfo(GoPlus device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (_stateManager.AppState != AppState.Idle)
            {
                throw new InvalidOperationException(Properties.Resources.AppNotIdleString);
            }

            _stateManager.AppState = AppState.Getting;

            try
            {
                await device.Connect();

                // Try to read the device key characteristic
                for (int i = 0; i < Constants.RetryCount; i++)
                {
                    try
                    {
                        byte[] deviceKey = await device.ReadCharacteristic(Constants.DeviceKeyCharacteristicUuid);
                        device.DeviceKey = ByteArrayHelper.ByteArrayToString(deviceKey);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (i < Constants.RetryCount - 1)
                        {
                            string message = string.Format(Properties.Resources.ErrorReadingDeviceKey, e.Message, Constants.RetryCount - i + 1);
                            _logger.Log(message, Category.Exception, Priority.High);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }

                // Try to read the blob key characteristic
                for (int i = 0; i < Constants.RetryCount; i++)
                {
                    try
                    {
                        byte[] blobKey = await device.ReadCharacteristic(Constants.BlobKeyCharacteristicUuid);
                        device.BlobKey = ByteArrayHelper.ByteArrayToString(blobKey);
                        break;
                    }
                    catch (Exception e)
                    {
                        if (i < Constants.RetryCount - 1)
                        {
                            string message = string.Format(Properties.Resources.ErrorReadingBlobKey, e.Message, Constants.RetryCount - i + 1);
                            _logger.Log(message, Category.Exception, Priority.High);
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                DialogParameters parameters = new DialogParameters
                {
                    { DialogParameterKeys.Title, Properties.Resources.UnableToGetDeviceInfoString },
                    { DialogParameterKeys.Message, e.Message },
                    { DialogParameterKeys.NeutralButtonText, Properties.Resources.OkString }
                };

                _notifyManager.ShowDialog(string.Empty, parameters, null);
            }
            finally
            {
                _stateManager.AppState = AppState.Idle;
                await device.Disconnect();
            }
        }

        /// <summary>
        /// Restore the Original PGP Firmware.
        /// </summary>
        /// <param name="device">Device to restore.</param>
        /// <returns>Async task to await.</returns>
        public async Task RestoreDevice(GoPlus device)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (_stateManager.AppState != AppState.Idle)
            {
                throw new InvalidOperationException(Properties.Resources.AppNotIdleString);
            }

            try
            {
                _stateManager.AppState = AppState.Restoring;

                await device.Connect();

                device.CharacteristicNotification += Device_CharacteristicNotification;

                await device.NotifyRegister(Constants.RestoreCharacteristicStatusUuid);

                await device.WriteCharacteristic(Constants.RestoreCharacteristicUuid, (byte)0x01);
            }
            catch (Exception e)
            {
                DialogParameters dialogParameters = new DialogParameters()
                {
                    { DialogParameterKeys.Title, Properties.Resources.UnableToRestoreTitleString },
                    { DialogParameterKeys.Message, e.Message },
                    { DialogParameterKeys.NeutralButtonText, Properties.Resources.OkString },
                };

                _notifyManager.ShowDialog(null, dialogParameters);

                await device.Disconnect();

                _stateManager.AppState = AppState.Idle;
            }
        }

        private async void FinishRestore(GoPlus device)
        {
            try
            {
                await device.NotifyUnregister(Constants.RestoreCharacteristicStatusUuid);
            }
            finally
            {
                _stateManager.AppState = AppState.Idle;
                await device.Disconnect();
            }
        }

        private void Device_CharacteristicNotification(object sender, CharacteristicNotificationEventArgs e)
        {
            if (e.Uuid == Constants.RestoreCharacteristicStatusUuid)
            {
                int value = e.IntValue;
                _logger.Log(string.Format(Properties.Resources.RestoreResult, value), Category.Debug, Priority.None);

                if (value == 1)
                {
                    DialogParameters dialogParameters = new DialogParameters()
                    {
                        { DialogParameterKeys.Title, Properties.Resources.RestoreCompleteTitleString },
                        { DialogParameterKeys.Message, Properties.Resources.RestoreCompleteMessageString },
                        { DialogParameterKeys.NeutralButtonText, Properties.Resources.OkString },
                    };

                    _notifyManager.ShowDialog(null, dialogParameters);

                    _eventAggregator.GetEvent<AppEvents.ClearEvent>().Publish();
                }
                else
                {
                    DialogParameters dialogParameters = new DialogParameters()
                    {
                        { DialogParameterKeys.Title, Properties.Resources.UnableToRestoreTitleString },
                        { DialogParameterKeys.Message, Properties.Resources.UnableToRestoreMessageString },
                        { DialogParameterKeys.NeutralButtonText, Properties.Resources.OkString },
                    };

                    _notifyManager.ShowDialog(null, dialogParameters);
                }

                FinishRestore((GoPlus)sender);
            }
        }
    }
}