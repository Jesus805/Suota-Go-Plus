using Android.OS;
using Java.IO;
using suota_pgp.Services;
using System.Text;

namespace suota_pgp.Droid.Services
{
    public class FileManager : IFileManager
    {
        public File File { get; set; }

        public FileManager()
        {
            File = Environment.ExternalStorageDirectory;
        }

        public void SaveKeyBlob(string key, string blob)
        {
            if (string.IsNullOrWhiteSpace(key) || 
                string.IsNullOrWhiteSpace(blob))
            {
                return;
            }

            string json = SerializeKeyBlob(key, blob);
        }

        private string SerializeKeyBlob(string key, string blob)
        {
            // Save as JSON
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"key\": \"");
            sb.Append(key);
            sb.Append("\",\n");
            sb.Append("  \"blob\": \"");
            sb.Append(blob);
            sb.Append("\"\n}");
            return sb.ToString();
        }
    }
}