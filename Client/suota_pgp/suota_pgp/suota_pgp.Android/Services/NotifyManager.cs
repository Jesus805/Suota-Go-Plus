using Android.App;
using Android.Widget;
using Prism.Ioc;
using Prism.Services.Dialogs;
using suota_pgp.Infrastructure;
using suota_pgp.Services.Interface;
using System;
using Xamarin.Forms;

namespace suota_pgp.Droid.Services
{
    internal class NotifyManager : INotifyManager
    {
        public readonly IContainerExtension _containerExtension;

        /// <summary>
        /// Initialize a new instance of 'NotificationManager'.
        /// </summary>
        /// <param name="containerExtension"></param>
        public NotifyManager(IContainerExtension containerExtension)
        {
            _containerExtension = containerExtension;
        }

        public void ShowDialog(string name, IDialogParameters parameters, Action<IDialogResult> callback)
        {
            ShowDialogInternal(name, parameters, callback);
        }

        /// <summary>
        /// Show a toast notification.
        /// </summary>
        public void ShowToast(string name, IDialogParameters parameters)
        {
            ShowToastInternal(name, parameters);
        }

        private void ShowDialogInternal(string name, IDialogParameters parameters, Action<IDialogResult> callback)
        {
            Activity activity;

            if (string.IsNullOrWhiteSpace(name))
            {
                activity = _containerExtension.Resolve<Activity>();
            }
            else
            {
                activity = _containerExtension.Resolve<Activity>(name);
            }

            var alertDialog = new AlertDialog.Builder(activity);
            
            if (parameters.TryGetValue(DialogParameterKeys.Message, out string message))
            {
                alertDialog.SetMessage(message);
            }
            else
            {
                throw new Exception(Properties.Resources.DialogMessageRequiredString);
            }

            if (parameters.TryGetValue(DialogParameterKeys.Title, out string title))
            {
                alertDialog.SetTitle(title);
            }

            if (parameters.TryGetValue(DialogParameterKeys.PositiveButtonText, out string positiveText))
            {
                alertDialog.SetPositiveButton(positiveText, (sender, e) => { });
            }

            if (parameters.TryGetValue(DialogParameterKeys.NegativeButtonText, out string negativeText))
            {
                alertDialog.SetNegativeButton(negativeText, (sender, e) => { });
            }

            Device.BeginInvokeOnMainThread(() =>
            {
                alertDialog.Show();
            });
        }

        /// <summary>
        /// Show a toast notification.
        /// </summary>
        private void ShowToastInternal(string name, IDialogParameters parameters)
        {
            Activity activity;
            string message = string.Empty;
            ToastLength length = ToastLength.Short;

            if (string.IsNullOrEmpty(name))
            {
                activity = _containerExtension.Resolve<Activity>();
            }
            else
            {
                activity = _containerExtension.Resolve<Activity>(name);
            }

            parameters.TryGetValue(ToastParameterKeys.Message, out message);
            parameters.TryGetValue(ToastParameterKeys.Duration, out length);

            Device.BeginInvokeOnMainThread(() =>
            {
                Toast.MakeText(Xamarin.Essentials.Platform.AppContext, message, length).Show();
            });
        }
    }
}