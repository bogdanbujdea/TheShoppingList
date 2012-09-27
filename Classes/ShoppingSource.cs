using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Popups;

namespace TheShoppingList.Classes
{
    public class ShoppingSource
    {
        public ShoppingSource()
        {
            _shoppingLists = new ObservableCollection<ShoppingList>();
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
                if (await FileExistAsync("source.xml") == false)
                    return false;
                StorageFile sessionFile = await ApplicationData.Current.LocalFolder.GetFileAsync("source.xml");
                if (sessionFile == null)
                {
                    return false;
                }
                
                IInputStream sessionInputStream = await sessionFile.OpenReadAsync();

                //Using DataContractSerializer , look at the cat-class
                // var sessionSerializer = new DataContractSerializer(typeof(List<object>), new Type[] { typeof(T) });
                //_data = (List<object>)sessionSerializer.ReadObject(sessionInputStream.AsStreamForRead());

                //Using XmlSerializer , look at the Dog-class
                var serializer = new XmlSerializer(typeof(ShoppingSource));
                var shoppingSource = serializer.Deserialize(sessionInputStream.AsStreamForRead()) as ShoppingSource;
                if (shoppingSource != null)
                    ShoppingLists = shoppingSource.ShoppingLists;
                sessionInputStream.Dispose();
                return true;
            }
            catch (Exception exception)
            {
                if (exception is FileNotFoundException)
                    return false;
                new MessageDialog(exception.Message).ShowAsync();
                return false;
            }
        }

        public async Task<bool> SaveListsAsync()
        {
            try
            {
                StorageFile sessionFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "source.xml", CreationCollisionOption.ReplaceExisting);
                IRandomAccessStream sessionRandomAccess = await sessionFile.OpenAsync(FileAccessMode.ReadWrite);
                IOutputStream sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
                var serializer = new XmlSerializer(typeof(ShoppingSource));

                //Using DataContractSerializer , look at the cat-class
                //var sessionSerializer = new DataContractSerializer(typeof(List<object>), new Type[] { typeof(T) });
                //sessionSerializer.WriteObject(sessionOutputStream.AsStreamForWrite(), _data);

                //Using XmlSerializer , look at the Dog-class
                serializer.Serialize(sessionOutputStream.AsStreamForWrite(), this);
                sessionRandomAccess.Dispose();
                await sessionOutputStream.FlushAsync();
                sessionOutputStream.Dispose();
                return true;
            }
            catch (Exception exception)
            {
                new MessageDialog(exception.Message).ShowAsync();
                return false;
            }
        }
    }
}
