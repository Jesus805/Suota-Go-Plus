namespace suota_pgp
{
    public enum State
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
    }
}
