using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using suota_pgp.Model;
using suota_pgp.Services;
using System;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    internal class ExtractorManager : BindableBase, IExtractorManager
    {
        private IBleManager _bleManager;
        private IEventAggregator _aggregator;
        private ILoggerFacade _logger;
        private INotifyManager _notifyManager;
        private IStateManager _stateManager;

        private GoPlus _device;

        public ExtractorManager(IBleManager bleManager,
                                IEventAggregator aggregator,
                                ILoggerFacade logger,
                                INotifyManager notifyManager,
                                IStateManager stateManager)
        {
            _bleManager = bleManager;
            _aggregator = aggregator;
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;

            _aggregator.GetEvent<PrismEvents.CharacteristicUpdatedEvent>().Subscribe(OnCharacteristicNotify, ThreadOption.UIThread);
        }

        /// <summary>
        /// Get the GO+ key and blob.
        /// </summary>
        /// <returns>Async task to await.</returns>
        public async Task GetDeviceInfo(GoPlus device)
        {
            if (device == null)
            {
                throw new ArgumentNullException("device");
            }

            if (_stateManager.State != AppState.Idle)
                return;

            try
            {
                _stateManager.State = AppState.Getting;

                await _bleManager.ConnectDevice(device);

                byte[] key = await _bleManager.ReadCharacteristic(device, Constants.DeviceKeyCharacteristicUuid);
                device.DeviceKey = Helper.ByteArrayToString(key);

                byte[] blob = await _bleManager.ReadCharacteristic(device, Constants.BlobKeyCharacteristicUuid);
                device.BlobKey = Helper.ByteArrayToString(blob);

                await _bleManager.DisconnectDevice(device);
            }
            catch (Exception e)
            {
                _notifyManager.ShowDialogInfoBox($"Unable to get Device Information. Please try again. Error: {e.Message}");
            }
            finally
            {
                _stateManager.State = AppState.Idle;
            }
        }

        /// <summary>
        /// Restore the Original PGP Firmware.
        /// </summary>
        /// <param name="device"></param>
        /// <returns></returns>
        public async Task RestoreDevice(GoPlus device)
        {
            _device = device ?? throw new ArgumentNullException("device");

            if (_stateManager.State != AppState.Idle)
                return;

            try
            {
                _stateManager.State = AppState.Restoring;

                await _bleManager.ConnectDevice(device);

                await _bleManager.NotifyRegister(device, Constants.RestoreCharacteristicStatusUuid);

                await _bleManager.WriteCharacteristic(device, Constants.RestoreCharacteristicUuid, (byte)0x01);
            }
            catch (Exception e)
            {
                _notifyManager.ShowDialogInfoBox($"Unable to restore. Error: {e.Message}");
                _stateManager.State = AppState.Idle;
            }
        }

        private async void FinishRestore()
        {
            await _bleManager.NotifyUnregister(_device, Constants.RestoreCharacteristicStatusUuid);

            await _bleManager.DisconnectDevice(_device);

            _stateManager.State = AppState.Idle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="charUpdate"></param>
        private void OnCharacteristicNotify(CharacteristicUpdate charUpdate)
        {
            if (charUpdate == null)
            {
                _logger.Log("Encountered null characteristic, ignoring", Category.Exception, Priority.High);
                return;
            }

            if (charUpdate.Uuid == Constants.RestoreCharacteristicStatusUuid)
            {
                int value = charUpdate.IntValue;
                _logger.Log($"Received value {value}", Category.Debug, Priority.None);

                if (value == 1)
                {
                    _notifyManager.ShowDialogInfoBox("Restore Complete, the device should automatically restart.");
                    _aggregator.GetEvent<PrismEvents.RestoreCompleteEvent>().Publish();
                }
                else
                {
                    _notifyManager.ShowDialogInfoBox($"Unable to restore. Returned value: {value}");
                }

                FinishRestore();
            }
        }
    }
}