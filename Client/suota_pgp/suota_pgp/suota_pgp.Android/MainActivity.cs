using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Plugin.BLE;
using Prism;
using Prism.Ioc;
using suota_pgp.Data;
using suota_pgp.Droid.Services;
using suota_pgp.Services.Interface;
using Xamarin.Essentials;

namespace suota_pgp.Droid
{
    [Activity(Label = "PoGo Plus Extractor", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private App _app;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            _app = new App(new AndroidInitializer());

            LoadApplication(_app);

            VerifyPermissions();
        }

        protected async void VerifyPermissions()
        {
            var stateManager = _app.Container.Resolve<IStateManager>();

            // Location Permissions
            PermissionStatus locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            // Clear location unauthorized flag
            stateManager.ClearErrorFlag(ErrorState.LocationUnauthorized);

            if (locationStatus != PermissionStatus.Granted)
            {
                // If the app did not request permissions then request them
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                {
                    // Permissions not granted, set location unauthorized flag
                    stateManager.SetErrorFlag(ErrorState.LocationUnauthorized);
                }
            }

            // Clear storage unauthorized flag
            stateManager.ClearErrorFlag(ErrorState.StorageUnauthorized);

            // Storage Read/Write Permissions
            PermissionStatus readStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();

            if (readStatus != PermissionStatus.Granted)
            {
                // If the app did not request permissions then request them
                readStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                if (readStatus != PermissionStatus.Granted)
                {
                    // Permissions not granted, set storage unauthorized flag
                    stateManager.SetErrorFlag(ErrorState.StorageUnauthorized);
                }
            }

            PermissionStatus writeStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (writeStatus != PermissionStatus.Granted)
            {
                // If the app did not request permissions then request them
                writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (writeStatus != PermissionStatus.Granted)
                {
                    // Permissions not granted, set storage unauthorized flag
                    stateManager.SetErrorFlag(ErrorState.StorageUnauthorized);
                }
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public class AndroidInitializer : IPlatformInitializer
        {
            public void RegisterTypes(IContainerRegistry containerRegistry)
            {
                containerRegistry.RegisterInstance(CrossBluetoothLE.Current);
                containerRegistry.RegisterSingleton<IBleManager, BleManager>();
                containerRegistry.RegisterSingleton<IKeyExtractManager, KeyExtractManager>();
                containerRegistry.RegisterSingleton<IFileManager, FileManager>();
                containerRegistry.RegisterSingleton<INotifyManager, NotifyManager>();
                containerRegistry.RegisterSingleton<ISuotaManager, SuotaManager>();
                containerRegistry.RegisterSingleton<IStateManager, StateManager>();
            }
        }
    }
}