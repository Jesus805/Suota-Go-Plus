namespace suota_pgp.Services.Interface
{
    public interface INotifyManager
    {
        /// <summary>
        /// Show a long duration toast notification.
        /// </summary>
        /// <param name="message">Message to show.</param>
        void ShowLongToast(string message);
        /// <summary>
        /// Show a short duration toast notification.
        /// </summary>
        /// <param name="message">Message to show.</param>
        void ShowShortToast(string message);
        /// <summary>
        /// Show an information dialog box.
        /// </summary>
        /// <param name="message">Message to show.</param>
        void ShowDialogInfoBox(string message);
        /// <summary>
        /// Show an error dialog box.
        /// </summary>
        /// <param name="message">Message to show.</param>
        void ShowDialogErrorBox(string message);
    }
}
