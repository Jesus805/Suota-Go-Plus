using Prism;
using Prism.Ioc;
using Prism.Logging;
using Prism.Unity;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace suota_pgp
{
    public partial class App : PrismApplication
    {
        public App(IPlatformInitializer initializer = null) : base(initializer) { }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("NavigationPage/MainPage");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ILoggerFacade, DebugLogger>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<ConnectView, ConnectViewModel>();
            containerRegistry.RegisterForNavigation<DeviceInfoView, DeviceInfoViewModel>();
            containerRegistry.RegisterForNavigation<SuotaView, SuotaViewModel>();
            containerRegistry.RegisterForNavigation<MainPage>();
        }
    }
}
