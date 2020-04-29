using System;

namespace suota_pgp.Data
{
    public class CharacteristicNotificationEventArgs : EventArgs
    {
        public Guid Uuid { get; }

        public byte[] Value { get; }

        /// <summary>
        /// New value as an Integer (Little Endian).
        /// </summary>
        public int IntValue
        {
            get
            {
                // Larger than an int
                if (Value.Length > 4)
                {
                    return -1;
                }
                else
                {
                    int result = 0;
                    for (int i = 0; i < Value.Length; i++)
                    {
                        result |= Value[i] << (8 * i);
                    }
                    return result;
                }
            }
        }

        public CharacteristicNotificationEventArgs(Guid uuid, byte[] value)
        {
            Uuid = uuid;
            Value = value;
        }
    }
}