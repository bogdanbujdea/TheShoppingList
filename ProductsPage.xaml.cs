using System;
using System.Collections.Generic;
using TheShoppingList.Classes;
using TheShoppingList.Common;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace TheShoppingList
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class ProductsPage
    {
        public ProductsPage()
        {
            InitializeComponent();
        }

        public int ListIndex { get; set; }


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
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            ListIndex = navigationParameter is int ? (int)navigationParameter : -1;

            if (ListIndex == -1 || source == null)
            {
                Frame.Navigate(typeof(MainPage));
                return;
            }

            ShoppingList list = source.ShoppingLists[ListIndex];
            if (list.Products.Count == 0)
                btnAddProduct.Visibility = Visibility.Visible;
            DefaultViewModel["Items"] = list.Products;
        }

        private void EditProduct(object sender, RoutedEventArgs e)
        {
        }

        private void RemoveProduct(object sender, RoutedEventArgs e)
        {
        }

        private void AddProduct(object sender, RoutedEventArgs e)
        {
            OnAddNewProduct(null, null);
        }

        private void OnAddNewProduct(object sender, RoutedEventArgs e)
        {
            if (!ProductPopup.IsOpen)
            {
                ProductPopup.IsLightDismissEnabled = false;
                ProductPopup.IsOpen = true;
                ProductPopup.Visibility = Visibility.Visible;
                btnAddProduct.Visibility = Visibility.Collapsed;
                //var shoppingSource = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            }
        }

        private async void PopupClosed(object sender, object e)
        {
            btnAddProduct.Visibility = Visibility.Collapsed;
            var control = ProductPopup.Child as NewProduct;
            if (control != null && control.Product == null)
                return;

            Product product = control.Product;

            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                source.ShoppingLists[ListIndex].Products.Add(product);
                await source.SaveListsAsync();
            }

        }
    }
}