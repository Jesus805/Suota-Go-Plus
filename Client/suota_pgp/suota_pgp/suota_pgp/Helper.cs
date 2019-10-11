using System;
using System.Text;

namespace suota_pgp
{
    public static class Helper
    {
        public static string ByteArrayToString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }

        public static Guid ByteArrayToGuid(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }

            if (bytes.Length != 16)
            {
                throw new ArgumentException("The byte array must be 16 bytes", "bytes");
            }

            // Convert first three groups to little endian, copy the rest.
            byte[] guid = new byte[16]
            {
                bytes[3], bytes[2], bytes[1], bytes[0],
                bytes[5], bytes[4],
                bytes[7], bytes[6],
                bytes[8], bytes[9],
                bytes[10], bytes[11], bytes[12], bytes[13], bytes[14], bytes[15]
            };

            return new Guid(guid);
        }
    }
}