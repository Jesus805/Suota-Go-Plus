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
            throw new NotImplementedException();
        }
    }
}
