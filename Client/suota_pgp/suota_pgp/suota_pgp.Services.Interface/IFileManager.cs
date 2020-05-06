using suota_pgp.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace suota_pgp.Services.Interface
{
    public interface IFileManager : INotifyPropertyChanged
    {
        ObservableCollection<PatchFile> PatchFiles { get; }

        PatchFile SelectedPatchFile { get; set; }

        byte Crc { get; }

        int FileSize { get; }

        int NumOfBlocks { get; }

        byte[] Patch { get; }

        byte[] Header { get; }

        List<byte[]> GetChunks(int blockIndex);

        List<byte[]> GetHeaderChunks();

        void GetFirmwareFileNames();

        void LoadFirmware(string fileName);
        
        void Save(GoPlus device);
    }
}
