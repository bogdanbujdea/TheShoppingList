using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TheShoppingList.Classes;
using TheShoppingList.Common;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
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
            App.Current.Resources["newListSize"] = size;

        }

        async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source == null)
                source = new ShoppingSource();
            bool noLists = await source.GetListsAsync();
            if (noLists == false)
                btnAddList.Visibility = Visibility.Visible;
            DefaultViewModel["Items"] = source.ShoppingLists;
            itemGridView.SelectionMode = ListViewSelectionMode.Multiple;
            itemListView.SelectedItem = ListViewSelectionMode.Multiple;
        }

        void Current_Resuming(object sender, object e)
        {
            LoadState(null,null);
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

        protected async override void SaveState(Dictionary<string,object> pageState)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if(source != null)
                await source.SaveListsAsync();
        }

        private void OnAddNewList(object sender, RoutedEventArgs e)
        {
            if (!ParentedPopup.IsOpen)
            {
                ParentedPopup.IsLightDismissEnabled = false;
                ParentedPopup.IsOpen = true;
                ParentedPopup.Visibility=Visibility.Visible;
                btnAddList.Visibility=Visibility.Collapsed;
                var shoppingSource = Application.Current.Resources["shoppingSource"] as ShoppingSource;
                if (shoppingSource != null)
                    listCount = shoppingSource.ShoppingLists.Count;
            }
        }

        private async void PopupClosed(object sender, object e)
        {
            var shoppingSource = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (shoppingSource != null)
            {
                if(shoppingSource.ShoppingLists.Count == 0)
                {
                    btnAddList.Visibility = Visibility.Visible;
                }
                else
                {
                    btnAddList.Visibility = Visibility.Collapsed;
                    await shoppingSource.SaveListsAsync();
                }
            }
        }

       

        private void EditList(object sender, RoutedEventArgs e)
        {

        }

        private async void RemoveList(object sender, RoutedEventArgs e)
        {
            ShoppingList selectedItem;

            VisualState currentState = ApplicationViewStates.CurrentState;
            if (currentState.Name == "Snapped")
                selectedItem = itemListView.SelectedItem as ShoppingList;
            else
                selectedItem = itemGridView.SelectedItem as ShoppingList;
            var source = App.Current.Resources["shoppingSource"] as ShoppingSource;
            if(selectedItem != null && source != null)
            {
                source.ShoppingLists.Remove(selectedItem);
                await source.SaveListsAsync();
            }
        }

        private void ListClicked(object sender, ItemClickEventArgs e)
        {
            var list = e.ClickedItem as ShoppingList;
            if(list != null)
                Frame.Navigate(typeof (ProductsPage), itemGridView.Items.IndexOf(e.ClickedItem));
        }

        private void itemListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var list = e.ClickedItem as ShoppingList;
            if (list != null)
                Frame.Navigate(typeof(ProductsPage), itemListView.Items.IndexOf(e.ClickedItem));
        }
    }
}
