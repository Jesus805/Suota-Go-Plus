using Java.IO;
using Plugin.CurrentActivity;
using Prism.Events;
using Prism.Logging;
using Prism.Mvvm;
using suota_pgp.Droid.Properties;
using suota_pgp.Model;
using suota_pgp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    internal class FileManager : BindableBase, IFileManager
    {
        private IEventAggregator _aggregator;
        private ILoggerFacade _logger;
        private INotifyManager _notifyManager;
        private IStateManager _stateManager;

        /// <summary>
        /// Firmware search directory.
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Contents of the firmware file.
        /// </summary>
        private byte[] _firmware;

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
        /// <param name="aggregator">Prism Dependency Injected IEventAggregator.</param>
        /// <param name="logger">Prism Dependency Injected ILoggerFacade.</param>
        public FileManager(IEventAggregator aggregator,
                           ILoggerFacade logger,
                           INotifyManager notifyManager,
                           IStateManager stateManager)
        {
            _aggregator = aggregator;
            _logger = logger;
            _notifyManager = notifyManager;
            _stateManager = stateManager;
            _path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" +
                    Resources.appFolderNameString;

            Patch = new byte[Constants.PatchLength];
            Header = new byte[Constants.HeaderSize];

            if (!stateManager.ErrorState.HasFlag(ErrorState.StorageUnauthorized))
            {
                CreateAppFolder();
            }
        }

        /// <summary>
        /// Load firmware from a file.
        /// </summary>
        /// <param name="fileName">Firmware filename</param>
        public void LoadFirmware(string fileName)
        {
            _firmware = null;

            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".img"))
            {
                _logger.Log("Unable to load firmware file, invalid filename or extension.", Category.Exception, Priority.High);
                return;
            }

            _logger.Log("Attempting to load firmware file", Category.Info, Priority.None);

            string filePath = _path + "/" + fileName;

            if (!System.IO.File.Exists(filePath))
            {
                _logger.Log("Unable to load firmware file, file does not exist.", Category.Exception, Priority.High);
                return;
            }

            _logger.Log("Reading file...", Category.Info, Priority.None);
            try
            {
                _firmware = System.IO.File.ReadAllBytes(filePath);

                FileSize = _firmware.Length;

                _logger.Log("Read file complete", Category.Info, Priority.None);

                NumOfBlocks = FileSize / Constants.BlockSize +
                            ((FileSize % Constants.BlockSize == 0) ? 0 : 1);

                // Populate patch
                for (int i = 0; i < Constants.PatchLength; i++)
                {
                    Patch[i] = _firmware[i + 2];
                }

                for (int i = 0; i < Constants.HeaderSize; i++)
                {
                    Header[i] = _firmware[i];
                }

                // Extract imagesize from firmware
                int imageSize = (_firmware[7] << 24) | (_firmware[6] << 16) |
                                (_firmware[5] << 8) | (_firmware[4]);

                // Add 5 bytes to send the patch
                imageSize += 6;

                // Write new image size to firmware image header
                for (int i = 0; i < 4; i++)
                {
                    _firmware[4 + i] = (byte)((imageSize >> (8 * i)) & 0xFF);
                }

                CalculateCRC();
            }
            catch (Exception e)
            {
                _logger.Log($"Unable to read file. Reason: {e.Message}", Category.Exception, Priority.High);
                _firmware = null;
                FileSize = 0;
                NumOfBlocks = 0;
            }
        }

        /// <summary>
        /// Get all chunks in the block.
        /// </summary>
        /// <param name="blockIndex">Block number to get.</param>
        /// <returns>The block at the index.</returns>
        public List<byte[]> GetChunks(int blockIndex)
        {
            int startIndex = blockIndex * Constants.BlockSize;

            int blockSize;

            if (startIndex + Constants.BlockSize > FileSize)
            {
                blockSize = FileSize - startIndex;
            }
            else
            {
                blockSize = Constants.BlockSize;
            }

            // Add another chunk if there is a remainder
            int numOfChunks = blockSize / Constants.ChunkSize +
                            ((blockSize % Constants.ChunkSize == 0) ? 0 : 1);

            List<byte[]> chunks = new List<byte[]>(numOfChunks);

            int bytesLeft = blockSize;

            for (int i = 0; i < numOfChunks; i++)
            {
                if (bytesLeft >= Constants.ChunkSize)
                {
                    chunks.Add(new byte[Constants.ChunkSize]);
                    bytesLeft -= Constants.ChunkSize;
                }
                else
                {
                    chunks.Add(new byte[bytesLeft]);
                    bytesLeft = 0;
                }

                for (int j = 0; j < chunks[i].Length; j++)
                {
                    int index = startIndex + (i * Constants.ChunkSize) + j;
                    chunks[i][j] = _firmware[index];
                }
            }

            return chunks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<byte[]> GetHeaderChunks()
        {
            int blockSize = Constants.HeaderSize;

            // Add another chunk if there is a remainder
            int numOfChunks = blockSize / Constants.ChunkSize +
                            ((blockSize % Constants.ChunkSize == 0) ? 0 : 1);

            List<byte[]> chunks = new List<byte[]>(numOfChunks);

            int bytesLeft = blockSize;

            for (int i = 0; i < numOfChunks; i++)
            {
                if (bytesLeft >= Constants.ChunkSize)
                {
                    chunks.Add(new byte[Constants.ChunkSize]);
                    bytesLeft -= Constants.ChunkSize;
                }
                else
                {
                    chunks.Add(new byte[bytesLeft]);
                    bytesLeft = 0;
                }

                for (int j = 0; j < chunks[i].Length; j++)
                {
                    int index = (i * Constants.ChunkSize) + j;
                    chunks[i][j] = _header[index];
                }
            }

            return chunks;
        }

        /// <summary>
        /// Get all firmware files (.img).
        /// </summary>
        /// <returns>A list of firmware names</returns>
        public async Task<List<PatchFile>> GetFirmwareFileNames()
        {
            if (_stateManager.State != AppState.Idle)
                return null;

            _stateManager.State = AppState.Loading;

            _logger.Log("Attempting to get firmware File names", Category.Info, Priority.None);

            List<PatchFile> fileNames = new List<PatchFile>();
            File dir = new File(_path);

            if (!IsExternalStorageAccessible(dir))
            {
                _logger.Log("ExternalStorage was not mounted or is readonly", Category.Exception, Priority.High);
                return null;
            }

            var files = await dir.ListFilesAsync();
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Name.EndsWith(".img"))
                    {
                        fileNames.Add(new PatchFile() { Name = file.Name });
                    }
                }

                if (fileNames.Count == 0)
                {
                    _notifyManager.ShowShortToast($"No files found. Please make sure the firmware file is in the \'{Resources.appFolderNameString}\' folder.");
                }
            }
            else
            {
                _notifyManager.ShowShortToast("Storage inaccessible. Please make sure that storage permissions are enabled.");
            }

            _stateManager.State = AppState.Idle;

            return fileNames;
        }

        /// <summary>
        /// Save Pokemon Go Plus Device Info to external storage.
        /// </summary>
        /// <param name="device">Device Info to save.</param>
        public async void Save(GoPlus device)
        {
            _logger.Log("Attempting to Save DeviceInfo", Category.Info, Priority.None);

            if (device == null)
            {
                _logger.Log("DeviceInfo was null", Category.Exception, Priority.High);
                return;
            }

            File file = new File(_path);
            if (!IsExternalStorageAccessible(file))
            {
                _logger.Log("ExternalStorage was not mounted or is readonly", Category.Exception, Priority.High);
                return;
            }

            if (!file.Exists())
            {
                try
                {
                    file.Mkdir();
                    _logger.Log("Created \"" + Resources.appFolderNameString +
                                "\" directory", Category.Info, Priority.None);
                }
                catch
                {
                    _logger.Log("Unable to create \"" + Resources.appFolderNameString + 
                                "\" directory", Category.Exception, Priority.High);
                }
            }

            string json = Serialize(device);

            string fileName = Resources.appFileNameString + " " +
                              DateTime.Now.ToString().Replace(':', '-').Replace('/', '-') + 
                              ".json";

            try
            {
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(_path + "/" + fileName))
                {
                    _logger.Log("Writing to file \"" + fileName + "\"", Category.Info, Priority.None);
                    await sw.WriteAsync(json);
                    _notifyManager.ShowShortToast($"Saved to {_path + "/" + fileName}");
                }
            }
            catch
            {
                _notifyManager.ShowShortToast("Unable to write key file. Make sure you have storage permissions enabled.");
                _logger.Log("Error writing to file \"" + fileName + "\"", Category.Exception, Priority.None);
            }
        }

        /// <summary>
        /// Serialize Pokemon Go Plus Device Info to JSON format.
        /// </summary>
        /// <param name="device">Device Info to serialize.</param>
        /// <returns>string in JSON format.</returns>
        private string Serialize(GoPlus device)
        {
            // Save as JSON
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"bluetooth\": \"");
            sb.Append(device.BtAddress);
            sb.Append("\",\n");
            sb.Append("  \"key\": \"");
            sb.Append(device.Key);
            sb.Append("\",\n");
            sb.Append("  \"blob\": \"");
            sb.Append(device.Blob);
            sb.Append("\"\n}");
            return sb.ToString();
        }

        /// <summary>
        /// Check if External Storage is accessible and allows read/write.
        /// </summary>
        /// <param name="dir">Directory to check.</param>
        /// <returns>true if read/writable, false otherwise.</returns>
        private bool IsExternalStorageAccessible(File dir)
        {
            if (dir == null)
            {
                return false;
            }

            string state = Android.OS.Environment.GetExternalStorageState(dir);
            return Android.OS.Environment.MediaMounted.Equals(state);
        }

        /// <summary>
        /// Create PgpExtractor folder in the external drive.
        /// </summary>
        private void CreateAppFolder()
        {
            File dir = new File(_path);
            if (!IsExternalStorageAccessible(dir))
            {
                _logger.Log("ExternalStorage was not mounted or is readonly", Category.Exception, Priority.High);
                return;
            }

            if (!dir.Exists())
            {
                try
                {
                    dir.Mkdir();
                    _logger.Log("Created \"" + Resources.appFolderNameString +
                                "\" directory", Category.Info, Priority.None);
                }
                catch
                {
                    _logger.Log("Unable to create \"" + Resources.appFolderNameString +
                                "\" directory", Category.Exception, Priority.High);
                }
            }
        }

        /// <summary>
        /// Calculate firmware CRC. 
        /// </summary>
        /// <param name="firmware">Contents of the firmware file.</param>
        private void CalculateCRC()
        {
            byte crc = 0;

            for (int i = 0; i < _firmware.Length; i++)
            {
                crc ^= _firmware[i];
            }

            // Add ValidFlag to CRC calculation.
            for (int i = 0; i < Constants.PatchLength; i++)
            {
                crc ^= Patch[i];
            }

            Crc = crc;
        }
    }
}