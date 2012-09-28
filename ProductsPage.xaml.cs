using System;
using System.Collections.Generic;
using TheShoppingList.Classes;
using TheShoppingList.Common;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Point = TheShoppingList.Classes.Point;

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
            ProductControl = ProductPopup.Child as NewProduct;
        }

        public int ListIndex { get; set; }

        public NewProduct ProductControl { get; set; }
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
            itemGridView.SelectedIndex = -1;
        }

        private void EditProduct(object sender, RoutedEventArgs e)
        {
            ProductControl.Mode = NewProduct.InputMode.EditProduct;
            if (ProductControl == null)
            {
                new MessageDialog("Could not get a handle to the popup control! Please try again!").ShowAsync();
                return;
            }
            var product = itemGridView.SelectedItem as Product;
            ProductControl.Product = product;
            ShowPopup();
        }

        private void ShowPopup()
        {
            if (!ProductPopup.IsOpen)
            {
                ProductPopup.IsLightDismissEnabled = false;
                ProductPopup.IsOpen = true;
                ProductPopup.Visibility = Visibility.Visible;
                btnAddProduct.Visibility = Visibility.Collapsed;
            }
            else
            {
                ProductPopup.IsOpen = false;
                ShowPopup();
            }
        }

        private void RemoveProduct(object sender, RoutedEventArgs e)
        {
            var source = App.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                Product product = null;
                if (itemGridView.SelectedItem != null)
                    product = itemGridView.SelectedItem as Product;
                else if (itemListView.SelectedItem != null)
                    product = itemListView.SelectedItem as Product;
                if (product != null)
                {
                    source.ShoppingLists[ListIndex].Products.Remove(product);
                }
            }
        }

        private void AddProduct(object sender, RoutedEventArgs e)
        {
            OnAddNewProduct(null, null);
        }

        private void OnAddNewProduct(object sender, RoutedEventArgs e)
        {
            ProductControl.Mode = NewProduct.InputMode.AddProduct;
            ShowPopup();
        }

        private async void PopupClosed(object sender, object e)
        {
            btnAddProduct.Visibility = Visibility.Collapsed;
            

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

        //private async void RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        //{
        //    // Create a menu and add commands specifying a callback delegate for each.
        //    // Since command delegates are unique, no need to specify command Ids.
        //    try
        //    {
        //        Windows.Foundation.Point position = e.GetPosition(itemGridView);
        //        //new MessageDialog("X=" + position.X.ToString() + "     Y=" + position.Y.ToString()).ShowAsync();

        //        var menu = new PopupMenu();

        //        menu.Commands.Add(new UICommand("Delete",
        //                                        command => DeleteProduct(itemGridView.SelectedItem as Product)));
        //        menu.Commands.Add(new UICommand("Edit",
        //                                        command => EditProduct(itemGridView.SelectedItem as Product)));
                
        //        // We don't want to obscure content, so pass in a rectangle representing the sender of the context menu event.
        //        // We registered command callbacks; no need to handle the menu completion event
        //        //OutputTextBlock.Text = "Context menu shown";
        //        Rect elementRect = GetElementRect((FrameworkElement) sender);
        //        //new MessageDialog("X=" + elementRect.X.ToString() + "     Y=" + elementRect.Y.ToString()).ShowAsync();
        //        elementRect.Height = position.Y;
        //        elementRect.Width = position.X;
        //        var button = (GridView)sender;
        //        if (itemGridView.SelectedIndex == -1)
        //        {
        //            appBar.IsOpen = true;
        //            return;
        //        }
        //        appBar.IsOpen = false;
        //        var transform = button.TransformToVisual(this);

        //        var point = transform.TransformPoint(new Windows.Foundation.Point(position.X, position.Y));
        //        IUICommand chosenCommand = await menu.ShowAsync(point);
        //        //IUICommand chosenCommand = await menu.ShowForSelectionAsync(elementRect, Placement.Left);
        //        if (chosenCommand == null) // The command is null if no command was invoked.
        //        {
        //            //OutputTextBlock.Text = "Context menu dismissed";
        //        }
        //    }
        //    catch (Exception exception)
        //    {
        //        new MessageDialog("I'm afraid I can't let you do that!" + exception.Message).ShowAsync();
        //    }
        //}

        private void EditProduct(Product sender)
        {
            
        }

        private void DeleteProduct(Product product)
        {
            var source = App.Current.Resources["shoppingSource"] as ShoppingSource;

            if (source != null && product != null)
                source.ShoppingLists[ListIndex].Products.Remove(product);
        }

        public static Rect GetElementRect(FrameworkElement element)
        {
            try
            {
                GeneralTransform buttonTransform = element.TransformToVisual(null);
                Windows.Foundation.Point point = buttonTransform.TransformPoint(new Windows.Foundation.Point());
                return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
            }
            catch (Exception exception)
            {
                new MessageDialog("I'm afraid I can't let you do that!" + exception.Message).ShowAsync();
                return new Rect();
            }
        }

        private void OnRightTappedGrid(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            appBar.IsOpen = true;
            if(itemGridView.SelectedIndex == -1)
            {
                btnAdd.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
                btnRemove.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnAdd.Visibility = Visibility.Visible;
                btnEdit.Visibility = Visibility.Visible;
                btnRemove.Visibility = Visibility.Visible;
            }
        }

    }
}