using Prism.Commands;
using suota_pgp.Model;
using suota_pgp.Services;

namespace suota_pgp
{
    public class DeviceInfoViewModel : ViewModelBase
    {
        private IFileManager _fileService;
        private IBleManager _bleService;

        private DeviceInfo _deviceInfo;
        public DeviceInfo DeviceInfo
        {
            get => _deviceInfo;
            private set => SetProperty(ref _deviceInfo, value);
        }

        public DelegateCommand GetDeviceInfoCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }

        public DeviceInfoViewModel(IBleManager bleService, IFileManager fileService)
        {
            _bleService = bleService;
            _fileService = fileService;

            DeviceInfo = new DeviceInfo();
            GetDeviceInfoCommand = new DelegateCommand(GetDeviceInfo);
            SaveCommand = new DelegateCommand(Save);
        }

        public void GetDeviceInfo()
        {
            DeviceInfo = _bleService.GetDeviceInfo();
        }

        public void Save()
        {
            _fileService.SaveDeviceInfo(DeviceInfo);
        }
    }
}
