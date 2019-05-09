using Prism.Mvvm;
using System;

namespace suota_pgp.Model
{
    public class CharValue
    {
        public Guid Uuid { get; }

        public byte[] Value { get; }

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

        public CharValue(Guid uuid, byte[] value)
        {
            Uuid = uuid;
            Value = value;
        }
    }
}
