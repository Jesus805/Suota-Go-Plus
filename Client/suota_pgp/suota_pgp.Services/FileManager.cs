//using Android.Widget;
//using Java.IO;
using Prism.Logging;
using Prism.Mvvm;
using suota_pgp.Data;
using suota_pgp.Infrastructure;
using suota_pgp.Services.Interface;
using suota_pgp.Services.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace suota_pgp.Services
{
    public class FileManager : BindableBase, IFileManager
    {
        private readonly ILoggerFacade _logger;
        private readonly INotifyManager _notifyManager;
        private readonly IStateManager _stateManager;

        /// <summary>
        /// Firmware search directory.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// List of patch files with a .img extension.
        /// </summary>
        public ObservableCollection<PatchFile> PatchFiles { get; }

        /// <summary>
        /// Selected patch file.
        /// </summary>
        private PatchFile _selectedPatchFile;
        public PatchFile SelectedPatchFile
        {
            get => _selectedPatchFile;
            set => SetProperty(ref _selectedPatchFile, value);
        }

        /// <summary>
        /// File CRC.
        /// </summary>
        private byte _crc;
        public byte Crc
        {
            get => _crc;
            set => SetProperty(ref _crc, value);
        }

        /// <summary>
        /// File size.
        /// </summary>
        private int _fileSize;
        public int FileSize
        {
            get => _fileSize;
            set => SetProperty(ref _fileSize, value);
        }

        /// <summary>
        /// Valid Flag Patch
        /// </summary>
        private byte[] _patch;
        public byte[] Patch
        {
            get => _patch;
            private set => SetProperty(ref _patch, value);
        }

        /// <summary>
        /// Header
        /// </summary>
        private byte[] _header;
        public byte[] Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        /// <summary>
        /// Number of blocks.
        /// </summary>
        private int _numOfBlocks;
        public int NumOfBlocks
        {
            get => _numOfBlocks;
            private set => SetProperty(ref _numOfBlocks, value);
        }

        /// <summary>
        /// Initialize a new instance of 'FileManager'.
        /// </summary>
        /// <param name="logger">Prism Dependency Injected ILoggerFacade.</param>
        public FileManager(ILoggerFacade logger,
                           INotifyManager notifyManager,
                           IStateManager stateManager)
        {
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;

            PatchFiles = new ObservableCollection<PatchFile>();

            Patch = new byte[Constants.PatchLength];
            Header = new byte[Constants.HeaderSize];

            if (!stateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized))
            {
                //CreateAppFolder();
            }
        }

        public List<byte[]> GetChunks(int blockIndex)
        {
            throw new NotImplementedException();
        }

        public List<byte[]> GetHeaderChunks()
        {
            throw new NotImplementedException();
        }

        public void GetFirmwareFileNames()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string[] children = Directory.GetDirectories(path);
            string[] files = Directory.GetFiles(path);
        }

        public void LoadFirmware(string fileName)
        {
            throw new NotImplementedException();
        }

        public void Save(GoPlus device)
        {
            throw new NotImplementedException();
        }
    }
}