using Prism.Mvvm;
using Prism.Navigation;

namespace suota_pgp
{
    public abstract class ViewModelBase : BindableBase, INavigationAware
    {
        protected bool _isViewActive;
        
        public ViewModelBase() { }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {
            _isViewActive = false;
        }

        public virtual void OnNavigatedTo(INavigationParameters parameters) { }

        public virtual void OnNavigatingTo(INavigationParameters parameters)
        {
            _isViewActive = true;
        }
    }
}
