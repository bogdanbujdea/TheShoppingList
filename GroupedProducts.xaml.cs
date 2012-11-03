using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheShoppingList.Classes;
using TheShoppingList.Common;
using Windows.Devices.Input;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Grouped Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234231

namespace TheShoppingList
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class GroupedProducts
    {
        public GroupedProducts()
        {
            InitializeComponent();
            ProductControl = ProductPopup.Child as NewProduct;
            if (ProductControl != null) ProductControl.NewProductAdded += OnNewProductAdded;
        }

        public int ListIndex { get; set; }
        public ShoppingList ShoppingList { get; set; }
        public NewProduct ProductControl { get; set; }
        public Product SelectedProduct { get; set; }
        public int SelectedIndex { get; set; }
        public Product DraggedProduct { get; set; }
        private GridViewUtils gridViewUtils;
        private bool rightTapped;

        #region Initialize List
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
            gridViewUtils = new GridViewUtils();
            ListIndex = navigationParameter is int ? (int)navigationParameter : -1; //get the List Index
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource; //load the source

            if (source != null) //if we have shopping lists
            {
                ShoppingList = source.ShoppingLists[ListIndex]; //set the property list
                SetGridBinding(); //set the binding
                SetProductsPrice(); //initialize prices
                itemGridView.SelectedIndex = -1; //deselect the first item
            }
        }

        private void SetProductsPrice()
        {
            double totalPrice = 0;
            for (int i = 0; i < ShoppingList.Products.Count; i++) //review set totalprice in mainpage IDIOT
            {
                if (ShoppingList.Products[i] == null) continue;
                ShoppingList.Products[i].Index = i;
                totalPrice += ShoppingList.Products[i].Price;
            }

            ShoppingList.TotalCost = totalPrice;
            txtTotal.Text = totalPrice.ToString();
            txtBalance.Text = ShoppingList.Balance.ToString();
            txtRest.Text = (ShoppingList.Balance - ShoppingList.TotalCost).ToString();
            CheckForTotalCostOverflow();
        }

        private void SetGridBinding()
        {
            try
            {
                var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

                if (source != null)
                {
                    ShoppingList shoppingList = ShoppingList;
                    gridViewUtils.CleanShoppingList(ref shoppingList);
                    var viewModel = new ProductsPageViewModel(ShoppingList);


                    gridViewUtils.FixViewModel(ref viewModel);

                    DataContext = viewModel;
                    IObservableVector<object> collectionGroups = groupedItemsViewSource.View.CollectionGroups;
                    ((ListViewBase)Zoom.ZoomedOutView).ItemsSource = collectionGroups;


                    btnAddProduct.Visibility = gridViewUtils.ListIsEmpty(viewModel);
                    itemGridView.SelectedIndex = -1;
                }
            }
            catch (Exception exception)
            {
                new MessageDialog(exception.Message + "In SetGridBinding").ShowAsync();
            }
        }

        #endregion

        #region Product Helper Functions
        private void HandleModifiedProduct(ref ShoppingSource source, ProductAddedArgs args)
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

        #endregion

        #region GridView Events

        private void OnRightTappedGridView(object sender, RightTappedRoutedEventArgs e)
        {
            e.Handled = true;
            appBar.IsOpen = true;
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

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0) return;
            SelectedProduct = e.AddedItems[0] as Product;
            
            if(rightTapped)
            {
                ShowHideButtons();
                appBar.IsOpen = true;
            }
            rightTapped = false;
        }

        private void ItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            
            //review right tapped mode(mouse,touch,pen...)
            rightTapped = true;
        }

        #endregion

        #region Drag Events

        private void ItemDragStarting(object sender, DragItemsStartingEventArgs e)
        {
            if (e.Items.Count > 0)
                DraggedProduct = e.Items[0] as Product;
            else
                DraggedProduct = null;
        }

        private void ItemsPanelDrop(object sender, DragEventArgs e)
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
        #endregion

        #region AppBar Events
        private void AppBarOpened(object sender, object e)
        {
            ShowHideButtons();
        }

        private void AppBarClosed(object sender, object e)
        {
            SelectedProduct = null;
            SelectedIndex = -1;
        }
        #endregion

        #region Money Events

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
            CheckForTotalCostOverflow();
        }

        private void OnTextBalanceTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtBalance.Text) == false && Utils.IsNumber(txtBalance.Text))
            {
                double balance;
                if (double.TryParse(txtBalance.Text, out balance))
                {
                    txtBalance.Text = balance.ToString();
                    double rest = balance - ShoppingList.TotalCost;
                    txtRest.Text = rest.ToString();
                }
                ShoppingList.Balance = Convert.ToDouble(txtBalance.Text);
            }
            else
            {
                txtBalance.Background = new SolidColorBrush(Colors.IndianRed);
            }
        }


        private void CheckForTotalCostOverflow()
        {
            if (ShoppingList.Balance < ShoppingList.TotalCost)
            {
                txtTotal.Foreground = new SolidColorBrush(Colors.Red);
                txtRest.Foreground = new SolidColorBrush(Colors.Red);
            }
            else if (ShoppingList.Balance == ShoppingList.TotalCost)
            {
                txtBalance.Foreground = new SolidColorBrush(Colors.White);
                txtTotal.Foreground = new SolidColorBrush(Colors.OrangeRed);
                txtRest.Foreground = new SolidColorBrush(Colors.OrangeRed);
            }
            else
            {
                txtTotal.Foreground = new SolidColorBrush(Colors.GreenYellow);
                txtBalance.Foreground = new SolidColorBrush(Colors.White);
                txtRest.Foreground = new SolidColorBrush(Colors.GreenYellow);
            }
        }


        #endregion

        #region Save List Events
        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override async void SaveState(Dictionary<String, Object> pageState)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

            if (source != null)
            {
                await source.SaveListsAsync();
            }
        }
        #endregion

        #region Popup Events

        private void ShowPopup()
        {
            if (!ProductPopup.IsOpen)
            {
                ProductPopup.IsLightDismissEnabled = true;
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
            itemGridView.SelectedIndex = -1;
            SelectedProduct = null;
            SetGridBinding();
        }

        #endregion

        #region List Operations Events

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
                    SetGridBinding();
                    ShoppingList.TotalCost = 0;
                    txtRest.Text = "0";
                    txtTotal.Text = "0";
                    txtRest.Foreground = new SolidColorBrush(Colors.White);
                    txtTotal.Foreground = new SolidColorBrush(Colors.White);
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
                CheckForTotalCostOverflow();
                SetGridBinding();
                await source.SaveListsAsync();
            }
        }

        private async void OnNewProductAdded(object sender, ProductAddedArgs args)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                if (ProductControl.Mode == NewProduct.InputMode.EditProduct)
                {
                    HandleModifiedProduct(ref source, args);
                }
                else
                {
                    source.ShoppingLists[ListIndex].Products.Add(args.Product);
                    ShoppingList.TotalCost += args.Product.Price;
                }
                double rest = ShoppingList.Balance - ShoppingList.TotalCost;
                txtRest.Text = rest.ToString();
                CheckForTotalCostOverflow();
                await source.SaveListsAsync();
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

        private void OnAddRemoveFromCart(object sender, RoutedEventArgs e)
        {
            var product = itemGridView.SelectedItem as Product;
            if (product != null)
            {
                product.IsBought = !product.IsBought;
                ShowHideButtons();
                SetGridBinding();
            }
            SelectedProduct = product;
            contextMenu.Visibility = Visibility.Collapsed;
            itemGridView.SelectedIndex = -1; //ShoppingList.Products.IndexOf(SelectedProduct);
            appBar.IsOpen = true;
        }

        #endregion

        #region SemanticZoom Events

        private void Zoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            try
            {
                var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;

                if (source != null)
                {
                    ShoppingList shoppingList = ShoppingList;
                    gridViewUtils.CleanShoppingList(ref shoppingList);
                    var viewModel = new ProductsPageViewModel(ShoppingList);
                    gridViewUtils.FixViewModel(ref viewModel);

                    if(viewModel.Items[1].Items == null || viewModel.Items[1].Items.Count == 0)
                    {
                        viewModel.Items[1].Items = new List<Product>();
                        var productsCategory = new ProductsCategory {Title = "In Cart", Items = new List<Product>()};
                        //ShoppingList.Products.Add(new Product { Title = "", Category = "In Cart" });
                        productsCategory.Items.Add(new Product { Title = "" });
                        viewModel.Items.Add(productsCategory);
                    }
                    if (e.IsSourceZoomedInView == true)
                        btnAddProduct.Visibility = Visibility.Collapsed;
                    DataContext = viewModel;
                    IObservableVector<object> collectionGroups = groupedItemsViewSource.View.CollectionGroups;
                    ((ListViewBase)Zoom.ZoomedOutView).ItemsSource = collectionGroups;
                    itemGridView.SelectedIndex = -1;
                }
            }
            catch (Exception exception)
            {
                new MessageDialog(exception.Message + "In SetGridBinding").ShowAsync();
            }
        }

        private void Zoom_ViewChangeCompleted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            SetGridBinding();
            if(e.IsSourceZoomedInView)
                btnAddProduct.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Other

        private void ShowHideButtons()
        {
            var product = itemGridView.SelectedItem as Product;
            if (product == null)
            {
                if (SelectedProduct == null)
                {
                    contextMenu.Visibility = Visibility.Collapsed;
                    return;
                }
                product = SelectedProduct;

            }

            contextMenu.Visibility = Visibility.Visible;
            if (product.IsBought)
            {
                var image = new Image();
                image.Source =
                    new BitmapImage(new Uri("ms-appx:/Assets/removefromcart.png", UriKind.RelativeOrAbsolute));
                btnAddRemoveFromCart.SetValue(AutomationProperties.NameProperty, "Remove from cart");
            }
            else
            {
                var image = new Image();
                image.Source = new BitmapImage(new Uri("ms-appx:/Assets/addtocart.png", UriKind.RelativeOrAbsolute));
                btnAddRemoveFromCart.SetValue(AutomationProperties.NameProperty, "Add to cart");
            }

        }

        #endregion

    }
}
