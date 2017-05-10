using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Xamarin.Forms;
using Cut.WinPhone;

[assembly: Dependency(typeof(SaveAndLoad_Windows))]

namespace Cut.WinPhone
{
    public class SaveAndLoad_Windows : ISaveAndLoad
    {
        #region ISaveAndLoad implementation

        public async Task SaveTextAsync(string filename, string text)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageFile file = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.WriteTextAsync(file, text);
        }

        public async Task AppendTextAsync(string filename, string text)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageFile file;
            if(FileExists(filename))
                file = await localFolder.GetFileAsync(filename);
            else
                file = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
            await Windows.Storage.FileIO.AppendTextAsync(file, text);
        }

        public async Task<string> LoadTextAsync(string filename)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            IStorageFile file = await localFolder.GetFileAsync(filename);
            return await Windows.Storage.FileIO.ReadTextAsync(file);
        }

        public bool FileExists(string filename)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            try
            {
                localFolder.GetFileAsync(filename).AsTask().Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
