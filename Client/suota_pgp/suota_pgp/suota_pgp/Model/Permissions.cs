namespace suota_pgp.Model
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
