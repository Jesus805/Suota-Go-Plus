using Prism.Navigation;
using Xamarin.Forms;

namespace suota_pgp
{
    public partial class MainPage : TabbedPage, INavigatingAware
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public void OnNavigatingTo(INavigationParameters parameters)
        {
            foreach (var child in Children)
            {
                (child as INavigatingAware)?.OnNavigatingTo(parameters);
                (child?.BindingContext as INavigatingAware)?.OnNavigatingTo(parameters);
            }
        }
    }
}
