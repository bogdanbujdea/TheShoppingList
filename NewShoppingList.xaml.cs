using System;
using TheShoppingList.Classes;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TheShoppingList
{
    public sealed partial class NewShoppingList : UserControl
    {
        public NewShoppingList()
        {
            InitializeComponent();
            Loaded += NewShoppingList_Loaded;
        }

        public bool NewListSaved { get; set; }

        private void NewShoppingList_Loaded(object sender, RoutedEventArgs e)
        {
            var size = Application.Current.Resources["newListSize"] as Point;
            var CurrentViewState = ApplicationView.Value;
            if(CurrentViewState == ApplicationViewState.Snapped)
            {
                transparentBorder.Width = 300;
                control.Width = 300;
                gridControl.Width = 300;
            }
            else
                transparentBorder.Width = size.Width;
            transparentBorder.Height = size.Height;
            newListBorder.Width = transparentBorder.Width;
            transparentBorder.Visibility=Visibility.Visible;
        }

        private async void OnSaveListName(object sender, RoutedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (string.IsNullOrEmpty(txtListName.Text))
            {
                await new MessageDialog("You must type a name for your list!").ShowAsync();
                return;
            }
            double balance = 0;
            if (string.IsNullOrEmpty(txtBalance.Text) == false && Utils.IsNumber(txtBalance.Text)) //review check for digits
                balance = double.Parse(txtBalance.Text);
            
            if (source != null)
                source.ShoppingLists.Add(new ShoppingList {Name = txtListName.Text, Balance = balance, TotalCost = 0, CreatedTime = DateTime.Now});
            transparentBorder.Visibility = Visibility.Collapsed;
            txtListName.Text = string.Empty;
            txtBalance.Text = string.Empty;
            ClosePopup();
        }

        private void txtListName_TextChanged(object sender, TextChangedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source == null) return;
            foreach (ShoppingList list in source.ShoppingLists)
            {
                if (String.Compare(list.Name, txtListName.Text, StringComparison.Ordinal) == 0)
                {
                    txtListName.BorderBrush = new SolidColorBrush(Colors.Red);
                    var color = new Color();
                    //string colorcode = "#FFD87474";
                    //int argb = Int32.Parse(colorcode.Replace("#", ""), NumberStyles.HexNumber);
                    Color clr = Color.FromArgb(0xFF, 0xD8, 0x74, 0x74);
                    txtListName.Background = new SolidColorBrush(clr);
                    btnSave.IsEnabled = false;
                }
                else
                {
                    txtListName.BorderBrush = new SolidColorBrush(Colors.Black);
                    txtListName.Background = new SolidColorBrush(Colors.White);
                    if (btnSave.IsEnabled == false)
                        btnSave.IsEnabled = true;
                }
            }
        }

        public void ClosePopup()
        {
            var p = Parent as Popup;
            if (p != null) p.IsOpen = false; // close the Popup
        }

        private void OnClosePopup(object sender, RoutedEventArgs e)
        {
            ClosePopup();
        }
    }
}