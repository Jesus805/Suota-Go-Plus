using Prism.Mvvm;
using suota_pgp.Data;
using suota_pgp.Properties;
using suota_pgp.Services.Interface;

namespace suota_pgp
{
    public class SuotaViewModel : BindableBase
    {
        private readonly ISuotaManager _suotaManager;

        private double _progress;
        public double Progress
        {
            get => _progress;
            private set => SetProperty(ref _progress, value);
        }
        
        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            private set => SetProperty(ref _progressText, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SuotaViewModel"/>
        /// </summary>
        /// <param name="eventAggregator"></param>
        public SuotaViewModel(ISuotaManager suotaManager)
        {
            _suotaManager = suotaManager;
            _suotaManager.ProgressUpdate += SuotaManager_ProgressUpdate;

            ProgressText = string.Empty;
            Progress = .5;
        }

        private void SuotaManager_ProgressUpdate(object sender, ProgressUpdateEventArgs e)
        {
            if (e.IsComplete)
            {
                Progress = 1.0;
                ProgressText += Resources.SuccessString;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(e.Status))
                {
                    ProgressText += $"{e.Status}\n";
                }

                if (e.Percentage.HasValue)
                {
                    int percentage = e.Percentage.Value;

                    Progress = ((percentage > 100) ? 100 : percentage) / 100;
                }
            }
        }
    }
}