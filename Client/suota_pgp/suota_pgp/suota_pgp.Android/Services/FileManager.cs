using Java.IO;
using Prism.Events;
using Prism.Logging;
using suota_pgp.Droid.Properties;
using suota_pgp.Model;
using suota_pgp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace suota_pgp.Droid.Services
{
    public class FileManager : IFileManager
    {
        private IEventAggregator _aggregator;
        private readonly ILoggerFacade _logger;
        private readonly string _path;

        /// <summary>
        /// Contents of the firmware file.
        /// </summary>
        private byte[] _firmware;

        /// <summary>
        /// Initialize a new instance of 'FileManager'.
        /// </summary>
        /// <param name="logger">Prism Dependency Injected ILoggerFacade.</param>
        public FileManager(IEventAggregator aggregator,
                           ILoggerFacade logger)
        {
            _aggregator = aggregator;
            _logger = logger;
            _path = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/" +
                    Resources.appFolderNameString;

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
        /// Load firmware from a file.
        /// </summary>
        /// <param name="fileName">Firmware filename</param>
        public async void LoadFirmware(string fileName)
        {
            _firmware = null;

            if (string.IsNullOrWhiteSpace(fileName) || !fileName.EndsWith(".img"))
            {
                _logger.Log("Unable to load firmware file, invalid filename or extension.", Category.Exception, Priority.High);
                return;
            }

            _logger.Log("Attempting to load firmware file", Category.Info, Priority.None);

            string filePath = _path + "/" + fileName;

            File file = new File(filePath);

            if (!file.Exists())
            {
                _logger.Log("Unable to load firmware file, file does not exist.", Category.Exception, Priority.High);
            }

            _firmware = new byte[file.Length()];
            var temp = new char[file.Length()];

            BufferedReader reader = new BufferedReader(new FileReader(file));

            _logger.Log("Reading file...", Category.Info, Priority.None);
            try
            {
                await reader.ReadAsync(temp);
                for (int i = 0; i < file.Length(); i++)
                {
                    _firmware[i] = (byte)temp[i];
                }

                _logger.Log("Read file complete", Category.Info, Priority.None);
                _aggregator.GetEvent<PrismEvents.FileLoadedEvent>().Publish();
            }
            catch
            {
                _logger.Log("Unable to read file", Category.Exception, Priority.High);
                _firmware = null;
            }
        }

        /// <summary>
        /// Get all firmware files (.img).
        /// </summary>
        /// <returns>A list of firmware names</returns>
        public async Task<List<string>> GetFirmwareFileNames()
        {
            _logger.Log("Attempting to get firmware File names", Category.Info, Priority.None);

            List<string> fileNames = new List<string>();
            File dir = new File(_path);

            if (!IsExternalStorageAccessible(dir))
            {
                _logger.Log("ExternalStorage was not mounted or is readonly", Category.Exception, Priority.High);
                return null;
            }

            var files = await dir.ListFilesAsync();

            foreach (var file in files)
            {
                if (file.Name.EndsWith(".img"))
                {
                    fileNames.Add(file.Name);
                }
            }

            return fileNames;
        }

        /// <summary>
        /// Save Pokemon Go Plus Device Info to external storage.
        /// </summary>
        /// <param name="info">Device Info to save.</param>
        public async void SaveDeviceInfo(DeviceInfo info)
        {
            _logger.Log("Attempting to Save DeviceInfo", Category.Info, Priority.None);

            if (info == null)
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

            string json = SerializeDeviceInfo(info);

            string fileName = Resources.appFileNameString + " " +
                              DateTime.Now.ToString().Replace(':', '-').Replace('/', '-') + 
                              ".json";

            BufferedWriter writer = new BufferedWriter(new FileWriter(_path + "/" + fileName));

            try
            {
                _logger.Log("Writing to file \"" + fileName + "\"", Category.Info, Priority.None);
                await writer.WriteAsync(json);
            }
            catch
            {
                _logger.Log("Error writing to file \"" + fileName + "\"", Category.Exception, Priority.None);
                return;
            }
            finally
            {
                writer.Close();
            }
        }

        /// <summary>
        /// Serialize Pokemon Go Plus Device Info to JSON format.
        /// </summary>
        /// <param name="info">Device Info to serialize.</param>
        /// <returns>string in JSON format.</returns>
        private string SerializeDeviceInfo(DeviceInfo info)
        {
            // Save as JSON
            StringBuilder sb = new StringBuilder();
            sb.Append("{\n");
            sb.Append("  \"bluetooth\": \"");
            sb.Append(info.BtAddress);
            sb.Append("\",\n");
            sb.Append("  \"key\": \"");
            sb.Append(info.Key);
            sb.Append("\",\n");
            sb.Append("  \"blob\": \"");
            sb.Append(info.Blob);
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
            string state = Android.OS.Environment.GetExternalStorageState(dir);
            return Android.OS.Environment.MediaMounted.Equals(state);
        }
    }
}