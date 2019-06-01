using System;

namespace suota_pgp
{
    [Flags]
    public enum ErrorState
    {
        /// <summary>
        /// No Errors.
        /// </summary>
        None = 0,
        /// <summary>
        /// Bluetooth turned off.
        /// </summary>
        BluetoothDisabled = 1,
        /// <summary>
        /// Location permissions not granted.
        /// </summary>
        LocationUnauthorized = 2,
        /// <summary>
        /// Storage permissions not granted.
        /// </summary>
        StorageUnauthorized = 4,
    }
}
