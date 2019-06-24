using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Plugin.BLE;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Prism;
using Prism.Ioc;
using suota_pgp.Droid.Services;
using suota_pgp.Model;
using suota_pgp.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace suota_pgp.Droid
{
    [Activity(Label = "PoGo Plus Extractor", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IPlatformInitializer
    {
        private bool _requestedPermissions;
        private App _app;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            _app = new App(this);
            _requestedPermissions = false;

            LoadApplication(_app);
        }

        protected override void OnResume()
        {
            base.OnResume();

            VerifyPermissions();
        }

        protected async void VerifyPermissions()
        {
            PermissionState state = new PermissionState();
            var locationStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);
            state.LocationAuthorized = locationStatus == PermissionStatus.Granted;

            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
            state.StorageAuthorized = storageStatus == PermissionStatus.Granted;

            var permissionsToRequest = new List<Plugin.Permissions.Abstractions.Permission>();

            if (!state.LocationAuthorized)
            {
                permissionsToRequest.Add(Plugin.Permissions.Abstractions.Permission.Location);
            }

            if (!state.StorageAuthorized)
            {
                permissionsToRequest.Add(Plugin.Permissions.Abstractions.Permission.Storage);
            }

            if (permissionsToRequest.Count > 0)
            {
                await RequestPermissions(state, permissionsToRequest);
            }

            _app.SetPermissionState(state);
        }

        protected async Task RequestPermissions(PermissionState state, List<Plugin.Permissions.Abstractions.Permission> permissions)
        {
            if (_requestedPermissions)
                return;
            else
                _requestedPermissions = true;

            var res = await CrossPermissions.Current.RequestPermissionsAsync(permissions.ToArray());

            if (res.ContainsKey(Plugin.Permissions.Abstractions.Permission.Location))
            {
                state.LocationAuthorized = res[Plugin.Permissions.Abstractions.Permission.Location] == PermissionStatus.Granted;
            }

            if (res.ContainsKey(Plugin.Permissions.Abstractions.Permission.Storage))
            {
                state.StorageAuthorized = res[Plugin.Permissions.Abstractions.Permission.Storage] == PermissionStatus.Granted;
            }

            if (!state.LocationAuthorized &&
                !state.StorageAuthorized)
            {
                Toast.MakeText(this, "Please enable location and storage permissions to use this app.", ToastLength.Long).Show();
            }
            else if (!state.LocationAuthorized)
            {
                Toast.MakeText(this, "Please enable location permissions to use this app.", ToastLength.Long).Show();
            }
            else if (!state.StorageAuthorized)
            {
                Toast.MakeText(this, "Please enable storage permissions to use this app.", ToastLength.Long).Show();
            }
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterInstance(CrossCurrentActivity.Current);
            containerRegistry.RegisterInstance(CrossBluetoothLE.Current);
            containerRegistry.RegisterSingleton<IBleManager, BleManager>();
            containerRegistry.RegisterSingleton<IExtractorManager, ExtractorManager>();
            containerRegistry.RegisterSingleton<IFileManager, FileManager>();
            containerRegistry.RegisterSingleton<INotifyManager, NotifyManager>();
            containerRegistry.RegisterSingleton<ISuotaManager, SuotaManager>();
            containerRegistry.RegisterSingleton<IStateManager, StateManager>();
        }
    }
}