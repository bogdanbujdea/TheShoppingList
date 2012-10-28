using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using Point = Windows.Foundation.Point;

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
        public Product DraggedProduct { get; set; }

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
            itemGridView.SelectedIndex = -1;

            ShoppingList.TotalCost = totalPrice;
            txtTotal.Text = totalPrice.ToString();
            txtBalance.Text = ShoppingList.Balance.ToString();
            txtRest.Text = (ShoppingList.Balance - ShoppingList.TotalCost).ToString();
            //review add totalcost, rest,etc.
        }

        private bool SetGridBinding()
        {
            try
            {
                var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

                if (source != null)
                {
                    ShoppingList = source.ShoppingLists[ListIndex];
                    var viewModel = new ProductsPageViewModel(ShoppingList);
                    if (viewModel.Items.Count <= 0)
                    {
                        btnAddProduct.Visibility = Visibility.Visible;
                        DataContext = viewModel;
                        ((ListViewBase)Zoom.ZoomedOutView).ItemsSource = groupedItemsViewSource.View.CollectionGroups;
                        itemGridView.SelectedIndex = -1;
                        return true;
                    }
                    if (viewModel.Items.Count == 1)
                    {
                        string title = "Remaining Products";
                        if (viewModel.Items[0].Title == "Remaining Products")
                            title = "In Cart";
                        viewModel.Items.Add(new ProductsCategory { Title = title });
                    }
                    btnAddProduct.Visibility = Visibility.Collapsed;
                    if (String.Compare(viewModel.Items[0].Title, "In Cart", StringComparison.Ordinal) == 0)
                    {
                        var group = viewModel.Items[0];
                        viewModel.Items[0] = viewModel.Items[1];
                        viewModel.Items[1] = group;
                    }

                    DataContext = viewModel;
                    var collectionGroups = groupedItemsViewSource.View.CollectionGroups;
                    ((ListViewBase)Zoom.ZoomedOutView).ItemsSource = collectionGroups;
                    itemGridView.SelectedIndex = -1;
                }
                return false;
            }
            catch (Exception exception)
            {
                new MessageDialog(exception.Message + "In SetGridBinding").ShowAsync();
                return false;
            }
        }


        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param nLame="pageState">An empty dictionary to be populated with serializable state.</param>
        protected async override void SaveState(Dictionary<String, Object> pageState)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

            if (source != null)
            {
                await source.SaveListsAsync();
            }
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
                    await DeleteProduct(product);
                }
            }
        }

        private async Task DeleteProduct(Product product)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                source.ShoppingLists[ListIndex].Products.Remove(product);
                ShoppingList.TotalCost -= product.Price;
                txtTotal.Text = ShoppingList.TotalCost.ToString();
                txtRest.Text = (ShoppingList.Balance - ShoppingList.TotalCost).ToString();
                SetGridBinding();
                await source.SaveListsAsync();
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
                    if (Math.Abs(oldPrice - args.Product.Price) > 0)
                    {
                        ShoppingList.TotalCost -= oldPrice;
                        ShoppingList.TotalCost += args.Product.Price;
                        txtTotal.Text = ShoppingList.TotalCost.ToString();
                    }
                    source.ShoppingLists[ListIndex].Products[SelectedIndex] = args.Product;
                }
                else
                {
                    source.ShoppingLists[ListIndex].Products.Add(args.Product);
                    ShoppingList.TotalCost += args.Product.Price;
                }
                double rest = ShoppingList.Balance - ShoppingList.TotalCost;
                txtRest.Text = rest.ToString();
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
            ShowHideButtons();
            appBar.IsOpen = true;
        }

        private async void OnAddRemoveFromCart(object sender, RoutedEventArgs e)
        {
            var product = itemGridView.SelectedItem as Product;

            if (product != null)
            {
                product.IsBought = !product.IsBought;
                SetGridBinding();
            }
            appBar.IsOpen = true;
        }


        private void appBar_Closed(object sender, object e)
        {
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

        private void ItemGridView_OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowHideButtons();
            e.Handled = false;
            appBar.IsOpen = true;
        }

        private void ShowHideButtons()
        {

            var product = itemGridView.SelectedItem as Product;
            if (product == null)
            {
                contextMenu.Visibility = Visibility.Collapsed;
            }
            else
            {
                contextMenu.Visibility = Visibility.Visible;
                if (product.IsBought)
                {
                    var image = new Image();
                    image.Source = new BitmapImage(new Uri("ms-appx:/Assets/removefromcart.png", UriKind.RelativeOrAbsolute));
                    btnAddRemoveFromCart.SetValue(AutomationProperties.NameProperty, "Remove from cart");
                }
                else
                {
                    var image = new Image();
                    image.Source = new BitmapImage(new Uri("ms-appx:/Assets/addtocart.png", UriKind.RelativeOrAbsolute));
                    btnAddRemoveFromCart.SetValue(AutomationProperties.NameProperty, "Add to cart");
                }
            }
        }

        private void ItemGridView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            SelectedProduct = e.AddedItems[0] as Product;

            btnAddRemoveFromCart.Visibility = Visibility.Visible;

        }


        private void ItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            ShowHideButtons();
            appBar.IsOpen = true;
        }

        private void GridDragStarting(object sender, DragItemsStartingEventArgs e)
        {

            if (e.Items.Count > 0)
                DraggedProduct = e.Items[0] as Product;
            else
                DraggedProduct = null;
        }

        private async void ItemsPanelDrop(object sender, DragEventArgs e)
        {
            if (DraggedProduct != null)
            {
                DraggedProduct.IsBought = !DraggedProduct.IsBought;
                e.Handled = true;
            }
            SetGridBinding();
        }

        private async void ItemDropDelete(object sender, DragEventArgs e)
        {
            Product product = DraggedProduct;
            if (DraggedProduct != null)
            {
                await DeleteProduct(product);
                SetGridBinding();
            }
        }

        private async void OnClickDiscard(object sender, RoutedEventArgs e)
        {
            if (itemGridView.SelectedItem != null)
            {
                var product = itemGridView.SelectedItem as Product;
                await DeleteProduct(product);
            }
        }

        private void AppBarOpened(object sender, object e)
        {
            ShowHideButtons();
        }

        private void OnBalanceTextDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            txtBalance.IsReadOnly = false;
            txtBalance.Background = new SolidColorBrush(Colors.White);
            txtBalance.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void OnTxtBalanceLostFocus(object sender, RoutedEventArgs e)
        {
            txtBalance.IsReadOnly = true;
            Color blueBkg = Color.FromArgb(0xff, 0x16, 0x49, 0x9A);
            txtBalance.Background = new SolidColorBrush(blueBkg);
            txtBalance.Text = ShoppingList.Balance.ToString();
            txtBalance.Foreground = new SolidColorBrush(Colors.White);
        }

        private void OnTextBalanceTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBalance.Text) == false && Utils.IsNumber(txtBalance.Text))
            {
                ShoppingList.Balance = Convert.ToDouble(txtBalance.Text);
                txtBalance.Foreground = new SolidColorBrush(Colors.White);
            }
            else
            {
                txtBalance.Background = new SolidColorBrush(Colors.IndianRed);
            }
        }

        private async void pageRoot_Unloaded(object sender, RoutedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

            if (source != null)
            {
                await source.SaveListsAsync();
            }
        }
    }
}