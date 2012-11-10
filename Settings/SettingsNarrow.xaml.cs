using TheShoppingList.Classes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TheShoppingList.Settings
{
    public sealed partial class SettingsNarrow : UserControl
    {
        public SettingsNarrow()
        {
            this.InitializeComponent();
            if (MainPage.Page.FacebookShare == true)
                checkFacebook.IsChecked = true;
        }

        private void OnFacebookChecked(object sender, RoutedEventArgs e)
        {
            MainPage.Page.btnShare.Visibility = Visibility.Visible;
            MainPage.Page.FacebookShare = true;
        }

        private void checkFacebook_Click(object sender, RoutedEventArgs e)
        {
            if (checkFacebook.IsChecked == false)
            {
                MainPage.Page.btnShare.Visibility = Visibility.Collapsed;
                MainPage.Page.FacebookShare = false;
            }
        }

        private async void OnRefreshApp(object sender, RoutedEventArgs e)
        {
            MessageBoxResult showAsync =
                await
                MessageBox.ShowAsync("Are you sure you want to remove all shopping lists?", "Please confirm",
                                     MessageBoxButton.YesNo);
            if (showAsync == MessageBoxResult.No)
                return;
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                source.ShoppingLists.Clear();
                MainPage.Page.btnAddList.Visibility = Visibility.Visible;
                await source.SaveListsAsync();
            }
        }
    }
}
