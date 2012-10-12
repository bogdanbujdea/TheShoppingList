using System;
using System.Collections.Generic;
using TheShoppingList.Classes;
using TheShoppingList.Common;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
            if (ProductControl != null) ProductControl.NewProductAdded += ProductControl_NewProductAdded;
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
            ListIndex = navigationParameter is int ? (int)navigationParameter : -1;
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            ShoppingList = source.ShoppingLists[ListIndex];
            DataContext = ShoppingList.Products;

            for (int i = 0; i < ShoppingList.Products.Count; i++)
            {
                if (ShoppingList.Products[i] != null)
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
            //List<Product> products = ProductControl.Products;
            //if (products == null)
            //    return;

        }

        private async void ProductControl_NewProductAdded(object sender, ProductAddedArgs args)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                if (ProductControl.Mode == NewProduct.InputMode.EditProduct)
                {
                    source.ShoppingLists[ListIndex].Products[productsList.SelectedIndex] = args.Product;
                    SortProducts(source.ShoppingLists[ListIndex]);
                }
                else
                    source.ShoppingLists[ListIndex].Products.Add(args.Product);

                await source.SaveListsAsync();
            }
        }

        private async void RemoveProduct(object sender, RoutedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                if (sender == btnRemoveAll)
                {
                    source.ShoppingLists[ListIndex].Products.Clear();
                    await source.SaveListsAsync();
                    return;
                }
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
            //productsList_SelectionChanged_1(sender,null);

        }

        private void Grid_RightTapped_1(object sender, RightTappedRoutedEventArgs e)
        {
            var product = productsList.SelectedItem as Product;
            if (product != null && product.IsBought)
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
                if (source != null)
                {
                    source.ShoppingLists[ListIndex].Products.Remove(product);
                    if (position == 0)
                        source.ShoppingLists[ListIndex].Products.Insert(position, product);
                    else
                        source.ShoppingLists[ListIndex].Products.Add(product);
                    await source.SaveListsAsync();
                }
            }
        }

        public T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
        {
            // Check if this object is the specified type
            if (obj is T)
                return obj as T;

            // Check for children
            int childrenCount = VisualTreeHelper.GetChildrenCount(obj);
            if (childrenCount < 1)
                return null;

            // First check all the children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child is T)
                    return child as T;
            }

            // Then check the childrens children
            for (int i = 0; i < childrenCount; i++)
            {
                DependencyObject child = FindDescendant<T>(VisualTreeHelper.GetChild(obj, i));
                if (child != null && child is T)
                    return child as T;
            }

            return null;
        }

        private void listBox_DoubleTapped_1(object sender, DoubleTappedRoutedEventArgs e)
        {


        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        private void productsList_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (productsList.SelectedIndex == -1)
                return;

            var currentSelectedListBoxItem = this.productsList.ItemContainerGenerator.ContainerFromIndex(productsList.SelectedIndex) as ListViewItem;

            if (currentSelectedListBoxItem == null)
                return;

            // Iterate whole listbox tree and search for this items
            Grid nameBox = FindDescendant<Grid>(currentSelectedListBoxItem);
            foreach (TextBlock tb in FindVisualChildren<TextBlock>(nameBox))
            {
                tb.Foreground = new SolidColorBrush(Colors.IndianRed);
            }
            if (e.RemovedItems.Count <= 0)
                return;
            var lastProduct = e.RemovedItems[0] as Product;
            if (lastProduct != null)
            {
                var lvitem = this.productsList.ItemContainerGenerator.ContainerFromIndex(lastProduct.Index) as ListViewItem;
                Grid lastSelection = FindDescendant<Grid>(lvitem);
                foreach (var child in FindVisualChildren<TextBlock>(lastSelection))
                {
                    if (lastProduct.IsBought == false)
                        child.Foreground = new SolidColorBrush(Colors.White);
                    else
                        child.Foreground = new SolidColorBrush(Colors.Green);
                }
            }
        }

        private void appBar_Closed(object sender, object e)
        {
            var source = App.Current.Resources["shoppingSource"] as ShoppingSource;
            SortProducts(source.ShoppingLists[ListIndex]);
            for (int index = 0; index < productsList.Items.Count; index++)
            {
                ListViewItem listViewItem = productsList.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
                if(listViewItem == null)
                    return;
                Grid lastSelection = FindDescendant<Grid>(listViewItem);
                foreach (var child in FindVisualChildren<TextBlock>(lastSelection))
                {
                    var product = productsList.Items[index] as Product;
                    if (product != null)
                        if (product.IsBought == false)
                            child.Foreground = new SolidColorBrush(Colors.White);
                        else
                            child.Foreground = new SolidColorBrush(Colors.Green);
                }
            }

        }
    }
}