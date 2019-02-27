using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Devices.Portable;

namespace TransactionCollectorX
{
    class Collector : IDisposable
    {
        string devicePath;

        public Collector(string devicePath) => this.devicePath = devicePath;

        public void Dispose() { }

        public async void Collect()
        {
            try
            {
                var dataFile = await FindSourceFile();
                if (dataFile != null)
                {
                    Program.f1.pi.Show("Found \"scandata.txt\" file");
                    var data = await ReadSourceFile(dataFile);
                    if (data != null && await WriteDestinationFile(data)) { PurgeSourceFile(dataFile); }
                    Program.f1.pi.Show("Data transferred successfully!");
                }
            }
            catch (Exception e) { Program.f1.pi.Show("Error: \"" + e.Message + "\""); }
        }

        private async Task<StorageFile> FindSourceFile()
        {
            var removableStorage = StorageDevice.FromId(devicePath);
            if (removableStorage != null)
            {
                Program.f1.pi.Show("Storage Device \"" + removableStorage.Name + "\" mounted");
                var deviceStorages = await removableStorage.GetFoldersAsync();
                foreach (var deviceStorage in deviceStorages)
                {
                    // try-catch for each, as the folder and file may be not on the first storage
                    try
                    {
                        var appFolder = await deviceStorage.GetFolderAsync("Documents\\TechStore");
                        if (appFolder != null) { return await appFolder.GetFileAsync("scandata.txt"); }
                    }
                    catch (Exception /* e */) { /* Program.f1.pi.Show("Error: \"" + e.Message + "\""); */ }
                }
            }
            return null;
        }

        private async Task<string> ReadSourceFile(StorageFile dataFile) =>
            await FileIO.ReadTextAsync(dataFile);

        private async Task<bool> WriteDestinationFile(string data)
        {
            var dataFile = await FindDestinationFile();
            if (dataFile != null)
            {
                await FileIO.AppendTextAsync(dataFile, data);
                return true;
            }
            return false;
        }

        private async Task<StorageFile> FindDestinationFile()
        {
            string filePath = null;
            StorageFolder folder = null;
            string fileName = "scandata.txt";
            filePath = Properties.Settings.Default.Path;
            try { folder = await StorageFolder.GetFolderFromPathAsync(filePath); }
            catch (Exception) { }
            // for UWP app
            // ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            // try
            // {
            //     filePath = localSettings.Containers["Destination"].Values["Path"].ToString();
            //     folder = await StorageFolder.GetFolderFromPathAsync(filePath);
            // }
            // catch (Exception)
            // {
            //     // if the path is wrong or folder does not exist, just ignore it
            // }
            if (folder == null)
            {
                // UWP code
                // FolderPicker folderPicker = new FolderPicker();
                // folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
                // folderPicker.FileTypeFilter.Add("*");
                // folder = await folderPicker.PickSingleFolderAsync();
                // user cancelled - return
                // if (folder == null) return null;
                using (var dialog = new FolderBrowserDialog())
                {
                    DialogResult result = dialog.ShowDialog();
                    // user cancelled - return
                    if (result != DialogResult.OK) return null;
                    folder = await StorageFolder.GetFolderFromPathAsync(dialog.SelectedPath);
                    if (folder == null) return null;
                }
                // save setting .NET
                Properties.Settings.Default.Path = folder.Path;
                Properties.Settings.Default.Save();
                // save the settings for UWP app
                // ApplicationDataContainer container;
                // if (!localSettings.Containers.ContainsKey("Destination"))
                // {
                //     container = localSettings.CreateContainer("Destination", Windows.Storage.ApplicationDataCreateDisposition.Always);
                // }
                // else
                // {
                //     container = localSettings.Containers["Destination"];
                // }
                // container.Values["Path"] = folder.Path;
            }
            return await folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
        }

        private async void PurgeSourceFile(StorageFile dataFile)
        {
            // somehow does not work ...
            // await FileIO.WriteTextAsync(dataFile, "aaa");
            // var folder = await dataFile.GetParentAsync();
            // await folder.CreateFileAsync(dataFile.Name, CreationCollisionOption.ReplaceExisting);
            // just delete the file
            await dataFile.DeleteAsync();
        }
    }
}