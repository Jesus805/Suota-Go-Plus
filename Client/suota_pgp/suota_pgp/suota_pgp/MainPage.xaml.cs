using Prism.Navigation;
using Xamarin.Forms;

namespace suota_pgp
{
    public partial class MainPage : TabbedPage, INavigationAware
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void OnNavigatedFrom(INavigationParameters parameters) { }

        public void OnNavigatedTo(INavigationParameters parameters) { }
    }
}
