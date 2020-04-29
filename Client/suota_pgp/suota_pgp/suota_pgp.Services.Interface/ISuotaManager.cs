using suota_pgp.Data;
using System;

namespace suota_pgp.Services.Interface
{
    public class RestoreCompleteEventArgs : EventArgs { }

    public delegate void RestoreComplete(object sender, RestoreCompleteEventArgs e);

    public class SuotaProgressUpdateEventArgs : EventArgs { }

    public delegate void SuotaProgressUpdate(object sender, SuotaProgressUpdateEventArgs e);

    public interface ISuotaManager
    {
        // event RestoreComplete OnRestoreComplete;

        // event ProgressUpdate OnProgressUpdate;

        event EventHandler<ProgressUpdateEventArgs> ProgressUpdate;

        void RunSuota(GoPlus device, string fileName);
    }
}
