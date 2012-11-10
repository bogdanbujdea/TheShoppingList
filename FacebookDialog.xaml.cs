using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TheShoppingList.Classes;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TheShoppingList
{
    public sealed partial class FacebookDialog : UserControl
    {

        public PostDetails PostDetails { get; set; }

        public ShoppingList ShoppingList { get; set; }

        public FacebookDialog()
        {
            this.InitializeComponent();
            this.Loaded += OnLoaded;
            ShoppingList = MainPage.Page.SelectedList;
            PostDetails = new PostDetails();
        }

        private async void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            txtListName.Text = ShoppingList.Name;
            string fbText = ShoppingList.ToFacebook();
            txtMessage.Text = fbText;
            UserDetails details = await MainPage.Page.fbClient.GetUserDetails("me");
            txtByUser.Text = details.Name;
            txtMessageBlock.Text = fbText.Substring(0, 50) + "...";
            userImage.Source = new BitmapImage(new Uri(MainPage.Page.fbClient.GetUrlImage(details.Username), UriKind.RelativeOrAbsolute));
        }

        private async void OnFacebookShare(object sender, RoutedEventArgs e)
        {
            string user = "me";
            if (cmbShareTarget.SelectedIndex == -1) cmbShareTarget.SelectedIndex = 0;
            if (cmbShareTarget.SelectedIndex == 0)
                user = "me";
            else user = txtFriendName.Text;
            PostDetails.Message = txtMessage.Text;
            PostDetails.Name = ShoppingList.Name;
            await MainPage.Page.fbClient.PostMessage(user, PostDetails);
            ClosePopup();
        }

        private void ClosePopup()
        {
            var popup = Parent as Popup;
            if (popup != null) popup.IsOpen = false;
        }

        private void OnCancel(object sender, RoutedEventArgs e)
        {
         ClosePopup();   
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(cmbShareTarget == null)
                return;
            switch (cmbShareTarget.SelectedIndex)
            {
                case 0:
                    txtFriendName.Visibility = Visibility.Collapsed;
                    break;
                case 1:
                    txtFriendName.Visibility = Visibility.Visible;
                    break;
                default:
                    txtFriendName.Visibility = Visibility.Collapsed;
                    break;
            }
        }
    }
}
