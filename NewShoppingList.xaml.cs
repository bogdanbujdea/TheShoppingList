using System;
using System.Collections.Generic;
using System.Globalization;
using TheShoppingList.Classes;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
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

        public ShoppingList List { get; set; }

        public NewProduct.InputMode Mode { get; set; }

        #region InitializeWindow
        private void NewShoppingList_Loaded(object sender, RoutedEventArgs e)
        {
            SetWindowSize();
            if (Mode == NewProduct.InputMode.Edit)
            {
                List = MainPage.Page.SelectedList;
                txtBalance.Text = List.Budget.ToString();
                txtListName.Text = List.Name;
            }
            
            txtListName.Focus(FocusState.Programmatic);
            txtBalanceLabel.Text = "Budget (" + Utils.GetCountryInfo().CurrencySymbol + "):";
        }

        private void SetWindowSize()
        {
            var size = Application.Current.Resources["newListSize"] as Point;
            var CurrentViewState = ApplicationView.Value;
            if (CurrentViewState == ApplicationViewState.Snapped)
            {
                transparentBorder.Width = 300;
                control.Width = 300;
                gridControl.Width = 300;
            }
            else
                transparentBorder.Width = size.Width;
            transparentBorder.Height = size.Height;
            transparentBorder.Visibility = Visibility.Visible;
            newListBorder.Width = transparentBorder.Width;
        }
        #endregion

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
            if (Mode == NewProduct.InputMode.Edit)
            {
                List.Name = txtListName.Text;
                List.Budget = balance;
            }
            else
                if (source != null)
                    source.ShoppingLists.Add(new ShoppingList { Name = txtListName.Text, Budget = balance, TotalCost = 0, CreatedTime = DateTime.Now });
            transparentBorder.Visibility = Visibility.Collapsed;
            txtListName.Text = string.Empty;
            txtBalance.Text = string.Empty;
            ClosePopup();
        }

        /// <summary>
        /// Warn the user by changing the background and foreground of the textbox, if he tries
        /// to add a list with the same name
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnListNameChanged(object sender, TextChangedEventArgs e)
        {
            if (Mode == NewProduct.InputMode.Edit)
                return;
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source == null) return;
            foreach (ShoppingList list in source.ShoppingLists)
            {
                if (String.Compare(list.Name, txtListName.Text, StringComparison.Ordinal) == 0)
                {
                    txtListName.BorderBrush = new SolidColorBrush(Colors.Red);
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