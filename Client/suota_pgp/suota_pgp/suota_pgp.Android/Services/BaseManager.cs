using Android.App;
using Android.Content;
using Android.Widget;
using Plugin.CurrentActivity;
using Prism.Mvvm;
using Xamarin.Forms;

namespace suota_pgp.Droid.Services
{
    /// <summary>
    /// Base Manager provides Toast and Dialog notifications.
    /// </summary>
    internal class BaseManager : BindableBase
    {
        private ICurrentActivity _activity;

        /// <summary>
        /// Initialize a new instance of 'BaseManager'.
        /// </summary>
        protected BaseManager(ICurrentActivity activity)
        {
            _activity = activity;
        }

        /// <summary>
        /// Show a long duration toast notification.
        /// </summary>
        /// <param name="message">Message to show.</param>
        protected void ShowLongToast(string message)
        {
            Device.BeginInvokeOnMainThread(
            () => Toast.MakeText(_activity.Activity,
            message, ToastLength.Long).Show());
        }

        /// <summary>
        /// Show a short duration toast notification.
        /// </summary>
        /// <param name="message">Message to show.</param>
        protected void ShowShortToast(string message)
        {
            Device.BeginInvokeOnMainThread(
            () => Toast.MakeText(_activity.Activity,
                                 message, ToastLength.Short).Show());
        }

        /// <summary>
        /// Show an information dialog box.
        /// </summary>
        /// <param name="message">Message to show.</param>
        protected void ShowDialogInfoBox(string message)
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
        protected void ShowDialogErrorBox(string message)
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