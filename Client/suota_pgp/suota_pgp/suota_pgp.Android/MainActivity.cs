using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Plugin.CurrentActivity;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Prism;
using Prism.Ioc;
using suota_pgp.Droid.Services;
using suota_pgp.Services;
using System.Collections.Generic;

namespace suota_pgp.Droid
{
    [Activity(Label = "PoGo Plus Extractor", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity, IPlatformInitializer
    {
        private bool _requestedPermissions = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            CrossCurrentActivity.Current.Init(this, savedInstanceState);

            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App(this));
        }

        protected override void OnResume()
        {
            base.OnResume();

            if (!_requestedPermissions)
            {
                RequestPermissions();
                _requestedPermissions = true;
            }
        }

        public async void RequestPermissions()
        {
            var results = new Dictionary<string, PermissionStatus>();
            var permissionsToRequest = new List<Plugin.Permissions.Abstractions.Permission>();

            var locationStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Location);
            if (locationStatus != PermissionStatus.Granted)
            {
                permissionsToRequest.Add(Plugin.Permissions.Abstractions.Permission.Location);
            }
            else
            {
                results.Add("location", PermissionStatus.Granted);
            }

            var storageStatus = await CrossPermissions.Current.CheckPermissionStatusAsync(Plugin.Permissions.Abstractions.Permission.Storage);
            if (storageStatus != PermissionStatus.Granted)
            {
                permissionsToRequest.Add(Plugin.Permissions.Abstractions.Permission.Storage);
            }
            else
            {
                results.Add("storage", PermissionStatus.Granted);
            }

            var res = await CrossPermissions.Current.RequestPermissionsAsync(permissionsToRequest.ToArray());

            if (res.ContainsKey(Plugin.Permissions.Abstractions.Permission.Location))
            {
                results.Add("location", res[Plugin.Permissions.Abstractions.Permission.Location]);
            }

            if (res.ContainsKey(Plugin.Permissions.Abstractions.Permission.Storage))
            {
                results.Add("storage", res[Plugin.Permissions.Abstractions.Permission.Storage]);
            }

            if (results["location"] != PermissionStatus.Granted &&
                results["storage"] != PermissionStatus.Granted)
            {
                Toast.MakeText(this, "Please enable location and storage permissions to use this app.", ToastLength.Long).Show();
            }
            else if (results["location"] != PermissionStatus.Granted)
            {
                Toast.MakeText(this, "Please enable location permissions to use this app.", ToastLength.Long).Show();
            }
            else if (results["storage"] != PermissionStatus.Granted)
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
            containerRegistry.RegisterSingleton<IBleManager, BleManager>();
            containerRegistry.RegisterSingleton<IFileManager, FileManager>();
            containerRegistry.RegisterSingleton<ISuotaManager, SuotaManager>();
        }
    }
}