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
        /// Loading Firmware file.
        /// </summary>
        Loading,
        /// <summary>
        /// Performing SUOTA.
        /// </summary>
        Suota,
    }
}
