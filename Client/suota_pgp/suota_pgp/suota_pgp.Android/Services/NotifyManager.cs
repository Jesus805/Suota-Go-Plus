using Android.App;
using Android.Widget;
using Plugin.CurrentActivity;
using suota_pgp.Services.Interface;
using Xamarin.Forms;

namespace suota_pgp.Droid.Services
{
    internal class NotifyManager : INotifyManager
    {
        /// <summary>
        /// Current Activity.
        /// </summary>
        private readonly ICurrentActivity _activity;
        
        /// <summary>
        /// Initialize a new instance of 'NotificationManager'.
        /// </summary>
        /// <param name="activity">Current Activity.</param>
        public NotifyManager(ICurrentActivity activity)
        {
            _activity = activity;
        }

        /// <summary>
        /// Show a long duration toast notification.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public void ShowLongToast(string message)
        {
            Device.BeginInvokeOnMainThread(
            () => Toast.MakeText(_activity.Activity,
            message, ToastLength.Long).Show());
        }

        /// <summary>
        /// Show a short duration toast notification.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public void ShowShortToast(string message)
        {
            Device.BeginInvokeOnMainThread(
            () => Toast.MakeText(_activity.Activity,
                                 message, ToastLength.Short).Show());
        }

        /// <summary>
        /// Show an information dialog box.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public void ShowDialogInfoBox(string message)
        {
            Device.BeginInvokeOnMainThread(
            () => new AlertDialog.Builder(_activity.Activity)
                 .SetMessage(message)
                 .SetPositiveButton("OK", (sender, e) => { })
                 .Show());
        }

        /// <summary>
        /// Show an error dialog box.
        /// </summary>
        /// <param name="message">Message to show.</param>
        public void ShowDialogErrorBox(string message)
        {
            Device.BeginInvokeOnMainThread(
            () => new AlertDialog.Builder(CrossCurrentActivity.Current.Activity)
                  .SetMessage(message)
                  .SetTitle("Error")
                  .SetPositiveButton("OK", (sender, e) => { })
                  .Show());
        }
    }
}