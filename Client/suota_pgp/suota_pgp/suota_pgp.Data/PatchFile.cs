using Prism.Mvvm;

namespace suota_pgp.Data
{
    public class PatchFile : BindableBase
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}
