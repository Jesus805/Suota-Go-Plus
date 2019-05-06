using Prism.Commands;
using Prism.Events;
using suota_pgp.Model;
using suota_pgp.Services;
using System.Collections.ObjectModel;

namespace suota_pgp
{
    public class DeviceInfoViewModel : ViewModelBase
    {
        private IEventAggregator _aggregator;
        private IFileManager _fileService;
        private IBleManager _bleManager;

        public ObservableCollection<GoPlus> Devices { get; private set; }

        private GoPlus _selectedDevice;
        public GoPlus SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (value != null && SetProperty(ref _selectedDevice, value))
                {
                    _aggregator.GetEvent<PrismEvents.GoPlusSelectedEvent>().Publish(value);
                }
            }
        }

        private DeviceInfo _deviceInfo;
        public DeviceInfo DeviceInfo
        {
            get => _deviceInfo;
            private set => SetProperty(ref _deviceInfo, value);
        }

        private bool _isScanning;
        public bool IsScanning
        {
            get => _isScanning;
            set => SetProperty(ref _isScanning, value);
        }

        public DelegateCommand GetDeviceInfoCommand { get; private set; }

        public DelegateCommand SaveCommand { get; private set; }

        public DelegateCommand ScanCommand { get; private set; }

        public DelegateCommand StopScanCommand { get; private set; }

        public DeviceInfoViewModel(IEventAggregator aggregator,
                                   IBleManager bleService, 
                                   IFileManager fileService)
        {
            _aggregator = aggregator;
            _bleManager = bleService;
            _fileService = fileService;

            _aggregator.GetEvent<PrismEvents.ScanStateChangeEvent>().Subscribe(OnScanStateChanged, ThreadOption.UIThread);
            _aggregator.GetEvent<PrismEvents.GoPlusFoundEvent>().Subscribe(OnGoPlusFound);

            Devices = new ObservableCollection<GoPlus>();
            DeviceInfo = new DeviceInfo();
            GetDeviceInfoCommand = new DelegateCommand(GetDeviceInfo);
            SaveCommand = new DelegateCommand(Save);
            ScanCommand = new DelegateCommand(Scan);
            StopScanCommand = new DelegateCommand(StopScan);

            IsScanning = false;
        }

        public async void GetDeviceInfo()
        {
            DeviceInfo = await _bleManager.GetDeviceInfo();
        }

        public void Save()
        {
            _fileService.SaveDeviceInfo(DeviceInfo);
        }

        public void Scan()
        {
            Devices.Clear();
            _bleManager.Scan();
        }

        public void StopScan()
        {
            _bleManager.StopScan();
        }

        public void OnScanStateChanged(ScanState state)
        {
            IsScanning = (state == ScanState.Running);
        }

        public void OnGoPlusFound(GoPlus pgp)
        {
            Devices.Add(pgp);
        }
    }
}
