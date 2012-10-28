using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace TheShoppingList.Classes
{
    public class ShoppingSource
    {
        private string listsFile;
        private string backupFile;
        public ShoppingSource()
        {
            _shoppingLists = new ObservableCollection<ShoppingList>();
            listsFile = "source.xml";
            backupFile = "backup.xml";
        }

        private ObservableCollection<ShoppingList> _shoppingLists;

        public ObservableCollection<ShoppingList> ShoppingLists
        {
            get { return _shoppingLists; }
            set { _shoppingLists = value; }
        }

        private static async Task<bool> FileExistAsync(string fileName)
        {
            try
            {
                await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> GetListsAsync()
        {
            try
            {
                bool loaded = await LoadListsFromFileAsync(listsFile);
                if (loaded == false)
                {
                    await LoadListsFromFileAsync(backupFile);
                    var sessionFile = await ApplicationData.Current.LocalFolder.GetFileAsync(backupFile);
                    sessionFile.CopyAsync(ApplicationData.Current.LocalFolder, listsFile, NameCollisionOption.ReplaceExisting);
                    //new MessageDialog(
                    //    "We are sorry for this inconveninence, but it seems that the shopping lists are missing or corrupted. The app loaded the latest backup!")
                    //    .ShowAsync();
                }
                return true;
            }
            catch (Exception exception)
            {
                
                try
                {
                    LoadListsFromFileAsync(backupFile);
                }
                catch (Exception)
                {
                    new MessageDialog(
                        "We are sorry for this inconveninence, but it seems that the shopping lists were deleted and loading the backup failed!")
                        .ShowAsync();
                    return false;
                }
                
                return true;
            }
        }

        private async Task<bool> LoadListsFromFileAsync(string fileName)
        {
            try
            {
                if (await FileExistAsync(fileName) == false)
                    return false;
                StorageFile sessionFile = await ApplicationData.Current.LocalFolder.GetFileAsync(fileName);
                if (sessionFile == null)
                {
                    return false;
                }

                IInputStream sessionInputStream = await sessionFile.OpenReadAsync();
                var serializer = new XmlSerializer(typeof(ShoppingSource));
                var shoppingSource = serializer.Deserialize(sessionInputStream.AsStreamForRead()) as ShoppingSource;
                if (shoppingSource != null)
                    ShoppingLists = shoppingSource.ShoppingLists;
                sessionInputStream.Dispose();
                return true;
            }
            catch (Exception exception)
            {
                return false;
            }
            
        }

        private async Task<bool> SaveToFileAsync(string fileName)
        {
            StorageFile sessionFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
            fileName, CreationCollisionOption.ReplaceExisting);
            IRandomAccessStream sessionRandomAccess = await sessionFile.OpenAsync(FileAccessMode.ReadWrite);
            IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
            var serializer = new XmlSerializer(typeof(ShoppingSource));


            serializer.Serialize(sessionOutputStream.AsStreamForWrite(), this);
            sessionRandomAccess.Dispose();
            await sessionOutputStream.FlushAsync();
            sessionOutputStream.Dispose();
            await sessionFile.CopyAsync(ApplicationData.Current.LocalFolder, "backup.xml", NameCollisionOption.ReplaceExisting);
            return true;


        }

        public async Task<bool> SaveListsAsync()
        {
            try
            {
                int i = 0;
                while (await SaveToFileAsync(listsFile) == false)
                {
                    i++;
                    if (i == 3)
                    {
                        StorageFile sessionFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                        backupFile, CreationCollisionOption.ReplaceExisting);
                        await sessionFile.CopyAsync(ApplicationData.Current.LocalFolder, listsFile, NameCollisionOption.ReplaceExisting);
                        return true;
                    }
                }
                return true;
            }
            catch (Exception exception)
            {
                new MessageDialog(exception.Message).ShowAsync();
                IAsyncOperation<StorageFile> sessionFile = ApplicationData.Current.LocalFolder.CreateFileAsync(
                        backupFile, CreationCollisionOption.ReplaceExisting);
                sessionFile.GetResults().CopyAsync(ApplicationData.Current.LocalFolder, listsFile, NameCollisionOption.ReplaceExisting);
                return false;
            }





        }
    }
}
