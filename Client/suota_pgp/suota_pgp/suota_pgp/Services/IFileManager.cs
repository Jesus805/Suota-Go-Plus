namespace suota_pgp.Services
{
    public interface IFileManager
    {
        void SaveKeyBlob(string key, string blob);
    }
}
