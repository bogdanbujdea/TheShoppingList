using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheShoppingList.Classes;
using TheShoppingList.Common;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace TheShoppingList
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MainPage
    {
        private int listCount;

        public MainPage()
        {
            InitializeComponent();
            this.Loaded += MainPage_Loaded;
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
            listCount = 0;
            var size = new Point();
            var bounds = Window.Current.Bounds;
            size.Width = bounds.Width;
            size.Height = bounds.Height;
            Application.Current.Resources["newListSize"] = size;

        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source == null)
                source = new ShoppingSource();
            bool noLists = await source.GetListsAsync();
            if (noLists == false || source.ShoppingLists.Count == 0)
            {
                btnAddList.Visibility = Visibility.Visible;
                return;
            }
            DefaultViewModel["Items"] = source.ShoppingLists;
            itemGridView.SelectedIndex = -1;
        }

        void Current_Resuming(object sender, object e)
        {
            LoadState(null, null);
        }

        void Current_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {

        }


        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected async override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {

        }

        protected async override void SaveState(Dictionary<string, object> pageState)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
                await source.SaveListsAsync();
        }

        private void OnAddNewList(object sender, RoutedEventArgs e)
        {
            if (!ParentedPopup.IsOpen)
            {
                ParentedPopup.IsLightDismissEnabled = false;
                ParentedPopup.IsOpen = true;
                ParentedPopup.Visibility = Visibility.Visible;
                btnAddList.Visibility = Visibility.Collapsed;
                var shoppingSource = Application.Current.Resources["shoppingSource"] as ShoppingSource;
                
            }
        }

        private async void PopupClosed(object sender, object e)
        {
            var shoppingSource = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (shoppingSource != null)
            {
                if (shoppingSource.ShoppingLists.Count == 0)
                {
                    btnAddList.Visibility = Visibility.Visible;
                }
                else
                {
                    btnAddList.Visibility = Visibility.Collapsed;
                    if (itemGridView.Items.Count != shoppingSource.ShoppingLists.Count)
                        DefaultViewModel["Items"] = shoppingSource.ShoppingLists;
                    await shoppingSource.SaveListsAsync();
                }
            }
        }



        private void EditList(object sender, RoutedEventArgs e)
        {
            if (!ParentedPopup.IsOpen)
            {
                ParentedPopup.IsLightDismissEnabled = false;
                ParentedPopup.IsOpen = true;
                ParentedPopup.Visibility = Visibility.Visible;
                btnAddList.Visibility = Visibility.Collapsed;
                var shoppingSource = Application.Current.Resources["shoppingSource"] as ShoppingSource;

            }
        }

        private async void RemoveList(object sender, RoutedEventArgs e)
        {
            ShoppingList selectedItem;
            if (sender.Equals(e))
                new MessageDialog("dsa").ShowAsync();
            VisualState currentState = ApplicationViewStates.CurrentState;

            var source = App.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source == null)
                return;
            if (currentState.Name == "Snapped")
            {
                if (itemListView.SelectedItems.Count > 1)
                {
                    foreach (var item in itemListView.SelectedItems)
                    {
                        source.ShoppingLists.Remove(item as ShoppingList);
                    }
                }
                else
                {
                    selectedItem = itemListView.SelectedItem as ShoppingList;
                    source.ShoppingLists.Remove(selectedItem);
                }
            }
            else //if view it's normal, get the selected items from the gridview
            {
                if (itemGridView.SelectedItems.Count > 1)
                {
                    foreach (var item in itemGridView.SelectedItems)
                    {
                        source.ShoppingLists.Remove(item as ShoppingList);
                    }
                }
                else
                {
                    selectedItem = itemGridView.SelectedItem as ShoppingList;
                    source.ShoppingLists.Remove(selectedItem);
                }
            }
            await source.SaveListsAsync();
            if (source.ShoppingLists.Count == 0)
                btnAddList.Visibility = Visibility.Visible;
        }

        private void ListClicked(object sender, ItemClickEventArgs e)
        {
            var list = e.ClickedItem as ShoppingList;
            if (list != null)
            {
                SortProducts(list);
                Frame.Navigate(typeof(GroupedProducts), itemGridView.Items.IndexOf(e.ClickedItem));
            }
        }

        private void SortProducts(ShoppingList list)
        {
            var products = new Product[list.Products.Count];
            list.Products.CopyTo(products, 0);

            Array.Sort(products, Comparison);
            list.Products.Clear();
            foreach (var product in products)
            {
                if (product != null)
                    list.Products.Add(product);
            }
        }

        private int Comparison(Product product, Product product1)
        {
            if (product == null)
                return 1;
            if (product1 == null)
                return -1;
            if (product.IsBought == false && product1.IsBought == false)
                return 0;
            if (product.IsBought == false)
                return -1;
            return 1;

        }

        private void itemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var list = e.ClickedItem as ShoppingList;
            if (list != null)
            {
                SortProducts(list);
                Frame.Navigate(typeof(ProductsPage), itemListView.Items.IndexOf(e.ClickedItem));
            }
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            bottomAppBar.IsOpen = true;
            e.Handled = true;
            var list = itemGridView.SelectedItem as ShoppingList;
        }

        private async void RemoveAllShoppingLists(object sender, RoutedEventArgs e)
        {
            MessageBoxResult showAsync = await MessageBox.ShowAsync("Are you sure you want to remove all shopping lists?", "Please confirm", MessageBoxButton.YesNo);
            if(showAsync == MessageBoxResult.No)
                return;
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                source.ShoppingLists.Clear();
                btnAddList.Visibility = Visibility.Visible;
                await source.SaveListsAsync();
            }
    }
    }
}
