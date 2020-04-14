using System;

namespace suota_pgp.Data
{
    /// <summary>
    /// Broadcast payload when a characteristic value is updated.
    /// </summary>
    public class CharacteristicUpdate
    {
        /// <summary>
        /// Characteristic UUID.
        /// </summary>
        public Guid Uuid { get; }

        /// <summary>
        /// New value.
        /// </summary>
        public byte[] Value { get; }

        /// <summary>
        /// New value as an Integer (Little Endian).
        /// </summary>
        public int IntValue
        {
            get
            {
                int result = 0;
                if (Value.Length > 4)
                {
                    return -1;
                }
                for (int i = 0; i < Value.Length; i++)
                {
                    result |= Value[i] << (8 * i);
                }
                return result;
            }
        }

        /// <summary>
        /// Initialize a new instance of 'CharacteristicUpdate'.
        /// </summary>
        /// <param name="uuid">Characteristic UUID.</param>
        /// <param name="value">New Value.</param>
        public CharacteristicUpdate(Guid uuid, byte[] value)
        {
            Uuid = uuid;
            Value = value;
        }
    }
}
