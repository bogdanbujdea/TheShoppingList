using System;
using System.Collections.Generic;
using TheShoppingList.Classes;
using TheShoppingList.Common;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace TheShoppingList
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ProductsPage : LayoutAwarePage
    {
        public ProductsPage()
        {
            InitializeComponent();
            ProductControl = ProductPopup.Child as NewProduct;
        }

        public int ListIndex { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public NewProduct ProductControl { get; set; }
        public Product SelectedProduct { get; set; }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            ListIndex = navigationParameter is int ? (int) navigationParameter : -1;
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            ShoppingList = source.ShoppingLists[ListIndex];
            DataContext = ShoppingList.Products;

            for (int i = 0; i < ShoppingList.Products.Count; i++)
            {
                ShoppingList.Products[i].Index = i;
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param nLame="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
        }

        private async void PopupClosed(object sender, object e)
        {
            Product product = ProductControl.Product;
            if (product == null)
                return;
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                source.ShoppingLists[ListIndex].Products.Add(product);
                await source.SaveListsAsync();
            }
        }

        private async void RemoveProduct(object sender, RoutedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                Product product = null;
                if (productsList.SelectedItem != null)
                    product = productsList.SelectedItem as Product;

                if (product != null)
                {
                    source.ShoppingLists[ListIndex].Products.Remove(product);
                    await source.SaveListsAsync();
                }
            }
        }

        private void AddProduct(object sender, RoutedEventArgs e)
        {
            ProductControl.Mode = NewProduct.InputMode.AddProduct;
            ShowPopup();
        }

        private void ShowPopup()
        {
            if (!ProductPopup.IsOpen)
            {
                ProductPopup.IsLightDismissEnabled = false;
                ProductPopup.IsOpen = true;
                ProductPopup.Visibility = Visibility.Visible;
            }
            else
            {
                ProductPopup.IsOpen = false;
                ShowPopup();
            }
        }

        private void EditProduct(object sender, RoutedEventArgs e)
        {
            ProductControl.Mode = NewProduct.InputMode.EditProduct;
            if (ProductControl == null)
            {
                new MessageDialog("Could not get a handle to the popup control! Please try again!").ShowAsync();
                return;
            }
            var product = productsList.SelectedItem as Product;
            ProductControl.Product = product;
            ShowPopup();
        }

        private void productsList_RightTapped_1(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = false;
            appBar.IsOpen = true;
        }

        private void Grid_RightTapped_1(object sender, RightTappedRoutedEventArgs e)
        {
            var product = productsList.SelectedItem as Product;
            if (product == null)
                return;
            if (product.IsBought)
            {
                var image = new Image();
                image.Source = new BitmapImage(new Uri("ms-appx:/Assets/removefromcart.png", UriKind.RelativeOrAbsolute));
                btnAddRemove.SetValue(AutomationProperties.NameProperty, "Remove from cart");
            }
            else
            {
                var image = new Image();
                image.Source = new BitmapImage(new Uri("ms-appx:/Assets/addtocart.png", UriKind.RelativeOrAbsolute));
                btnAddRemove.SetValue(AutomationProperties.NameProperty, "Add to cart");
            }
            appBar.IsOpen = true;
        }

        private async void btnAddToCart_Click_1(object sender, RoutedEventArgs e)
        {
            var product = productsList.SelectedItem as Product;
            if (product != null)
            {
                var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
                int position = ShoppingList.Products.Count;
                if (product.IsBought)
                {
                    position = 0;
                    product.IsBought = false;
                }
                else
                    product.IsBought = true;
                source.ShoppingLists[ListIndex].Products.Remove(product);
                if (position == 0)
                    source.ShoppingLists[ListIndex].Products.Insert(position, product);
                else
                    source.ShoppingLists[ListIndex].Products.Add(product);
                await source.SaveListsAsync();
            }
        }

        private void productsList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            //SelectedProduct = e.AddedItems[0] as Product;
        }
    }
}