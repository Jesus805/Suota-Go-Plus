using Prism.Mvvm;

namespace suota_pgp.Data
{
    public class PatchFile : BindableBase
    {
        private string _path;
        public string Path { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private string[] _content;
        public string[] Content { get; }

        public PatchFile()
        {

        }
    }
}
