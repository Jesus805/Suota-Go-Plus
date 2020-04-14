namespace suota_pgp.Data
{
    public class PermissionState
    {
        public bool LocationAuthorized { get; set; }

        public bool StorageAuthorized { get; set; }

        public PermissionState()
        {
            LocationAuthorized = false;
            StorageAuthorized = false;
        }
    }
}