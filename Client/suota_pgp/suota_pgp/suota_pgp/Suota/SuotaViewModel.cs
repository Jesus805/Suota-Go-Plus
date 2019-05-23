using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using suota_pgp.Model;

namespace suota_pgp
{
    public class SuotaViewModel : BindableBase
    {
        private IEventAggregator _aggregator;
        private INavigationService _navigation;

        private string _status;
        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }
        
        private string _progressText;
        public string ProgressText
        {
            get => _progressText;
            set => SetProperty(ref _progressText, value);
        }

        public SuotaViewModel(IEventAggregator aggregator, INavigationService navigation)
        {
            _aggregator = aggregator;
            _navigation = navigation;
            _aggregator.GetEvent<PrismEvents.ProgressUpdateEvent>().Subscribe(OnProgressUpdate, ThreadOption.UIThread);
            ProgressText = string.Empty;
        }

        private void OnProgressUpdate(Progress progress)
        {
            if (progress.IsComplete)
            {
                Progress = 1.0;
                ProgressText += "Success";
                //NavigationParameters p = new NavigationParameters();
                //p.Add("suotaSuccess", true);
                //_navigation.GoBackAsync(p);
            }
            else
            {
                if (!string.IsNullOrEmpty(progress.Status))
                {
                    ProgressText += progress.Status + "\n";
                }

                if (progress.Percentage.HasValue)
                {
                    int val;
                    if (progress.Percentage.Value > 100)
                    {
                        val = 100;
                    }
                    else
                    {
                        val = progress.Percentage.Value;
                    }

                    Progress = (double)progress.Percentage.Value / 100;
                }
            }
        }
    }
}
