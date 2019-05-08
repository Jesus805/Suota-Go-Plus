using Prism;
using Prism.Ioc;
using Prism.Logging;
using Prism.Unity;
using suota_pgp.Services;
using System;
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

            var result = await NavigationService.NavigateAsync("NavigationPage/ConnectView");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ILoggerFacade, DebugLogger>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage>();
            containerRegistry.RegisterForNavigation<DeviceInfoView>();
            containerRegistry.RegisterForNavigation<ConnectView>();
            containerRegistry.RegisterForNavigation<SuotaView>();
        }

        private void SetMainPageFromException(Exception ex)
        {
            var layout = new StackLayout
            {
                Padding = new Thickness(40)
            };
            layout.Children.Add(new Label
            {
                Text = ex?.GetType()?.Name ?? "Unknown Error encountered",
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center
            });

            layout.Children.Add(new ScrollView
            {
                Content = new Label
                {
                    Text = $"{ex}",
                    LineBreakMode = LineBreakMode.WordWrap
                }
            });

            MainPage = new ContentPage
            {
                Content = layout
            };
        }
    }
}
