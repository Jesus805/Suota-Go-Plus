using Prism.Navigation;

namespace suota_pgp.Infrastructure
{
    public class ToastParameters : NavigationParameters, IToastParameters
    {
        public ToastParameters() { }

        public ToastParameters(string query) : base(query) { }
    }
}
