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

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace TheShoppingList
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedProducts : LayoutAwarePage
    {
        public GroupedProducts()
        {
            InitializeComponent();
            ProductControl = ProductPopup.Child as NewProduct;
            if (ProductControl != null) ProductControl.NewProductAdded += ProductControl_NewProductAdded;
        }

        public int ListIndex { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public NewProduct ProductControl { get; set; }
        public Product SelectedProduct { get; set; }
        public int SelectedIndex { get; set; }

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
            SetGridBinding();
            double totalPrice = 0;
            for (int i = 0; i < ShoppingList.Products.Count; i++)
            {
                if (ShoppingList.Products[i] != null)
                {
                    ShoppingList.Products[i].Index = i;
                    totalPrice += ShoppingList.Products[i].Price;
                }
            }
            ShoppingList.TotalCost = totalPrice;
            //review add totalcost, rest,etc.
        }

        private void SetGridBinding()
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

            if (source != null)
            {
                ShoppingList = source.ShoppingLists[ListIndex];
                var viewModel = new ProductsPageViewModel(ShoppingList);
                DataContext = viewModel;
                var collectionGroups = groupedItemsViewSource.View.CollectionGroups;
                ((ListViewBase)this.Zoom.ZoomedOutView).ItemsSource = collectionGroups;
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
            //review save cost,etc.
        }

        #region List Operations

        private void AddProduct(object sender, RoutedEventArgs e)
        {
            ProductControl.Mode = NewProduct.InputMode.AddProduct;
            ShowPopup();
        }

        private void EditProduct(object sender, RoutedEventArgs e)
        {
            ProductControl.Mode = NewProduct.InputMode.EditProduct;
            if (ProductControl == null)
            {
                new MessageDialog("Could not get a handle to the popup control! Please try again!").ShowAsync();
                return;
            }
            if (SelectedProduct == null)
                return;
            ProductControl.Product = SelectedProduct;

            ShowPopup();
        }

        private async void RemoveProduct(object sender, RoutedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                if (sender == btnRemoveAll)
                {
                    source.ShoppingLists[ListIndex].Products.Clear();
                    ShoppingList.TotalCost = 0;
                    txtRest.Text = "0";
                    txtTotal.Text = "0";
                    await source.SaveListsAsync();
                    return;
                }
                Product product = null;
                if (itemGridView.SelectedItem != null)
                    product = itemGridView.SelectedItem as Product;

                if (product != null)
                {
                    source.ShoppingLists[ListIndex].Products.Remove(product);
                    //ShoppingList.TotalCost -= product.Price;
                    //txtTotal.Text = ShoppingList.TotalCost.ToString();
                    //txtRest.Text = (ShoppingList.Balance - ShoppingList.TotalCost).ToString();
                    SetGridBinding();
                    await source.SaveListsAsync();
                }
            }
        }

        #endregion


        private async void ProductControl_NewProductAdded(object sender, ProductAddedArgs args)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                if (ProductControl.Mode == NewProduct.InputMode.EditProduct)
                {
                    if (SelectedIndex == -1)
                        SelectedIndex = ShoppingList.Products.IndexOf(SelectedProduct);
                    double oldPrice = source.ShoppingLists[ListIndex].Products[SelectedIndex].Price;
                    if (oldPrice != args.Product.Price)
                    {
                        ShoppingList.TotalCost -= oldPrice;
                        ShoppingList.TotalCost += args.Product.Price;
                        txtTotal.Text = ShoppingList.TotalCost.ToString();
                    }
                    source.ShoppingLists[ListIndex].Products[SelectedIndex] = args.Product;
                    //SortProducts(source.ShoppingLists[ListIndex]);
                    //review add category
                }
                else
                {
                    source.ShoppingLists[ListIndex].Products.Add(args.Product);
                    ShoppingList.TotalCost += args.Product.Price;
                }
                //double rest = ShoppingList.Balance - ShoppingList.TotalCost;
                //txtRest.Text = rest.ToString(); review add cost...
                await source.SaveListsAsync();
            }
        }


        #region Popup
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
                ProductPopup.Visibility = Visibility.Collapsed;
            }
        }
        private void PopupClosed(object sender, object e)
        {
            appBar.IsOpen = false;
            SetGridBinding();
        }
        #endregion

        #region UI Events

        private void GridRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var product = itemGridView.SelectedItem as Product;
            if (product != null)
            {
                SelectedProduct = product;
                SelectedIndex = itemGridView.SelectedIndex;
            }
            else return;
            contextMenu.Visibility = Visibility.Visible;
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
            var product = itemGridView.SelectedItem as Product;

            if (product != null)
            {
                var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
                int position = ShoppingList.Products.Count;
                if (product.IsBought)
                {
                    position = 0; //review add in cart category
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

        private void itemGridView_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (itemGridView.SelectedIndex == -1)
            {
                contextMenu.Visibility = Visibility.Collapsed;
                return;
            }
            btnAddRemove.Visibility = Visibility.Visible;
            //var currentSelectedListBoxItem =
            //    itemGridView.ItemContainerGenerator.ContainerFromIndex(itemGridView.SelectedIndex) as ListViewItem;

            //if (currentSelectedListBoxItem == null)
            //    return;

            //// Iterate whole listbox tree and search for this items
            //var nameBox = Utils.FindDescendant<Grid>(currentSelectedListBoxItem);
            //foreach (TextBlock tb in Utils.FindVisualChildren<TextBlock>(nameBox))
            //{
            //    tb.Foreground = new SolidColorBrush(Colors.IndianRed);
            //}
            //if (e.RemovedItems.Count <= 0)
            //    return;
            //var lastProduct = e.RemovedItems[0] as Product;
            //if (lastProduct != null)
            //{
            //    var lvitem = itemGridView.ItemContainerGenerator.ContainerFromIndex(lastProduct.Index) as ListViewItem;
            //    var lastSelection = Utils.FindDescendant<Grid>(lvitem);
            //    foreach (TextBlock child in Utils.FindVisualChildren<TextBlock>(lastSelection))
            //    {
            //        if (lastProduct.IsBought == false)
            //            child.Foreground = new SolidColorBrush(Colors.White);
            //        else
            //            child.Foreground = new SolidColorBrush(Colors.Green);
            //    }
        }

        private void appBar_Closed(object sender, object e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            //Utils.SortProducts(source.ShoppingLists[ListIndex]);
            //for (int index = 0; index < itemGridView.Items.Count; index++)
            //{
            //    var listViewItem = itemGridView.ItemContainerGenerator.ContainerFromIndex(index) as ListViewItem;
            //    if (listViewItem == null)
            //        return;
            //    var lastSelection = Utils.FindDescendant<Grid>(listViewItem);
            //    foreach (TextBlock child in Utils.FindVisualChildren<TextBlock>(lastSelection))
            //    {
            //        var product = itemGridView.Items[index] as Product;
            //        if (product != null)
            //            if (product.IsBought == false)
            //                child.Foreground = new SolidColorBrush(Colors.White);
            //            else
            //                child.Foreground = new SolidColorBrush(Colors.Green);
            //    }
            //}
            SelectedProduct = null;
            SelectedIndex = -1;
        }

        private void txtBalance_TextChanged(object sender, TextChangedEventArgs e)
        {
            //review costs
            if (Utils.IsNumber(txtBalance.Text))
            {
                double balance = 0;
                if (double.TryParse(txtBalance.Text, out balance))
                {
                    txtBalance.Text = balance.ToString();
                    double rest = balance - ShoppingList.TotalCost;
                    txtRest.Text = rest.ToString();
                }
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
        }
        #endregion

        private void ItemGridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            SelectedProduct = e.ClickedItem as Product;
        }
    }
}