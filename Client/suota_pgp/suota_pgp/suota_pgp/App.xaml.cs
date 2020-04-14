using Prism;
using Prism.Events;
using Prism.Ioc;
using Prism.Logging;
using Prism.Unity;
using suota_pgp.Data;
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
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<DeviceInfoView>();
            containerRegistry.RegisterForNavigation<ConnectView>();
            containerRegistry.RegisterForNavigation<SuotaView>();
        }

        public void SetPermissionState(PermissionState state)
        {
            IEventAggregator aggregator = Container.Resolve<IEventAggregator>();
            aggregator.GetEvent<AppEvents.PermissionStateChangedEvent>().Publish(state);
        }
    }
}
