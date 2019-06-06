namespace suota_pgp
{
    public enum AppState
    {
        /// <summary>
        /// Doing nothing.
        /// </summary>
        Idle,
        /// <summary>
        /// Scanning for BLE devices.
        /// </summary>
        Scanning,
        /// <summary>
        /// Getting DeviceInfo.
        /// </summary>
        Getting,
        /// <summary>
        /// Getting firmware file names.
        /// </summary>
        Loading,
        /// <summary>
        /// Restoring original firmware.
        /// </summary>
        Restoring,
        /// <summary>
        /// Performing SUOTA.
        /// </summary>
        Suota,
    }
}
