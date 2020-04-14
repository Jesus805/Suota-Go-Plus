using Prism.Events;
using Prism.Mvvm;
using suota_pgp.Data;
using suota_pgp.Properties;

namespace suota_pgp
{
    public class SuotaViewModel : BindableBase
    {
        private readonly IEventAggregator _eventAggregator;

        private string _status;
        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

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

        public SuotaViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<AppEvents.SuotaProgressUpdateEvent>().Subscribe(OnProgressUpdate, ThreadOption.UIThread);

            ProgressText = string.Empty;
        }

        private void OnProgressUpdate(SuotaProgress progress)
        {
            if (progress.IsComplete)
            {
                Progress = 1.0;
                ProgressText += Resources.SuccessString;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(progress.Status))
                {
                    ProgressText += $"{progress.Status}\n";
                }

                if (progress.Percentage.HasValue)
                {
                    int percentage = progress.Percentage.Value;

                    Progress = ((percentage > 100) ? 100 : percentage) / 100;
                }
            }
        }
    }
}