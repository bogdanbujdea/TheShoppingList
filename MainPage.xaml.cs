﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Callisto.Controls;
using NotificationsExtensions.TileContent;
using TheShoppingList.Classes;
using TheShoppingList.Social;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Store;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Security.Authentication.Web;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Point = TheShoppingList.Classes.Point;

// The Items Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234233

namespace TheShoppingList
{
    /// <summary>
    /// A page that displays a collection of item previews.  In the Split Application this page
    /// is used to display and select one of the available groups.
    /// </summary>
    public sealed partial class MainPage
    {
        private LicenseInformation licenseInformation;
        public string SecondaryTileID { get; set; }
        public static MainPage Page { get; set; }
        public ShoppingList SelectedList { get; set; }
        public FacebookClient fbClient;
        public bool FacebookShare;
        public bool Searching;

        public ShoppingList DraggedList { get; set; }

        public MainPage()
        {
            InitializeComponent();
            FacebookShare = true;
            Loaded += MainPage_Loaded;
            Application.Current.Suspending += Current_Suspending;
            Application.Current.Resuming += Current_Resuming;
            var windowSize = new Point();
            Rect bounds = Window.Current.Bounds;
            windowSize.Width = bounds.Width;
            windowSize.Height = bounds.Height;
            Application.Current.Resources["newListSize"] = windowSize;
            Page = this;

            RegisterForShare();
            fbClient = new FacebookClient();
        }


        #region Initialize App
        private void InitializeLicense()
        {
            licenseInformation = CurrentAppSimulator.LicenseInformation;
            if (licenseInformation.IsActive)
            {
                if (licenseInformation.IsTrial == false)
                {
                    adDuplexAd.Visibility = Visibility.Collapsed;
                }
                else
                {
                    adDuplexAd.Visibility = Visibility.Visible;
                }
            }
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
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
            foreach (ShoppingList list in source.ShoppingLists)
            {
                if (String.Compare(list.UniqueID, SecondaryTileID, StringComparison.Ordinal) == 0)
                {
                    Frame.Navigate(typeof(GroupedProducts), source.ShoppingLists.IndexOf(list));
                }
            }
            DefaultViewModel["Items"] = source.ShoppingLists;
            InitializeAppTiles();
            itemGridView.SelectionMode = ListViewSelectionMode.Single;
            bottomAppBar.IsOpen = false;
            itemGridView.SelectedIndex = -1;
        }

        #endregion

        #region App State

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
            if (navigationParameter is string)
                SecondaryTileID = navigationParameter as string;
            
        }

        private async Task<bool> LoadList(StorageFile args)
        {
            try
            {
                StorageFile storageFile = args;

                var source = App.Current.Resources["shoppingSource"] as ShoppingSource;
                IInputStream sessionInputStream = await storageFile.OpenReadAsync();
                var serializer = new XmlSerializer(typeof(ShoppingList));
                var list = serializer.Deserialize(sessionInputStream.AsStreamForRead()) as ShoppingList;
                if (list != null)
                    source.ShoppingLists.Add(list);
                sessionInputStream.Dispose();
                new MessageDialog("New list added").ShowAsync();
                return true;
            }
            catch (Exception exception)
            {
                new MessageDialog(exception.Message).ShowAsync();
                return false;
            }
        }

        protected override async void SaveState(Dictionary<string, object> pageState)
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
                try
                {
                    InitializeAppTiles();
                    await source.SaveListsAsync();
                }
                catch (Exception)
                {
                }
        }

        private void Current_Resuming(object sender, object e)
        {
            LoadState(null, null);
        }

        private void Current_Suspending(object sender, SuspendingEventArgs e)
        {
        }

        #endregion

        #region Not Sorted

        private void OnAddNewList(object sender, RoutedEventArgs e)
        {
            if (!ParentedPopup.IsOpen)
            {
                ParentedPopup.IsLightDismissEnabled = false;
                ParentedPopup.IsOpen = true;
                ParentedPopup.Visibility = Visibility.Visible;
                btnAddList.Visibility = Visibility.Collapsed;
                newShoppingList.Mode = NewProduct.InputMode.Add;
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
                newShoppingList.Mode = NewProduct.InputMode.Edit;
                ParentedPopup.Visibility = Visibility.Visible;
                btnAddList.Visibility = Visibility.Collapsed;
                SelectedList = itemGridView.SelectedItem as ShoppingList;
            }
        }

        private async void RemoveList(object sender, RoutedEventArgs e)
        {
            ShoppingList selectedItem;
            if (sender.Equals(e))
                new MessageDialog("dsa").ShowAsync();
            VisualState currentState = ApplicationViewStates.CurrentState;

            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source == null)
                return;
            if (currentState.Name == "Snapped")
            {
                if (itemListView.SelectedItems.Count > 1)
                {
                    foreach (object item in itemListView.SelectedItems)
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
                    foreach (object item in itemGridView.SelectedItems)
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
            foreach (Product product in products)
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
                Frame.Navigate(typeof(GroupedProducts), itemListView.Items.IndexOf(e.ClickedItem));
            }
        }

        private void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
        }

        private async void RemoveAllShoppingLists(object sender, RoutedEventArgs e)
        {
            MessageBoxResult showAsync =
                await
                MessageBox.ShowAsync("Are you sure you want to remove all shopping lists?", "Please confirm",
                                     MessageBoxButton.YesNo);
            if (showAsync == MessageBoxResult.No)
                return;
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            if (source != null)
            {
                source.ShoppingLists.Clear();
                btnAddList.Visibility = Visibility.Visible;
                await source.SaveListsAsync();
            }
        }

        private void OnItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count <= 0)
            {
                SelectedList = null;

                return;
            }
            SelectedList = e.AddedItems[0] as ShoppingList;
            ItemControls.Visibility = Visibility.Visible;
            if(FacebookShare == false)
                btnShare.Visibility = Visibility.Collapsed;
            ToggleAppBarButton();
            bottomAppBar.IsOpen = true;
        }

        private void OnAppBarOpened(object sender, object e)
        {
            if (SelectedList == null)
            {
                ItemControls.Visibility = Visibility.Collapsed;
                btnPinList.Visibility = Visibility.Collapsed;
            }
            else
            {
                ItemControls.Visibility = Visibility.Visible;
                if (FacebookShare == false)
                    btnShare.Visibility = Visibility.Collapsed;
                btnPinList.Visibility = Visibility.Visible;
            }
        }

        #endregion

       
        #region ShareContract

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            // Unregister the current page as a share source.
            UnregisterForShare();
            if (e.Parameter is FileActivatedEventArgs)
            {
                var files = (e.Parameter as FileActivatedEventArgs).Files;
                foreach (var storageItem in files)
                {
                    LoadList(storageItem as StorageFile);
                }
            }
            base.OnNavigatedFrom(e);
        }

        public void RegisterForShare()
        {
            try
            {

                DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();

                dataTransferManager.DataRequested += ShareStorageItemsHandler;
            }
            catch (Exception)
            {
                
            }
        }

        public void UnregisterForShare()
        {
            DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
            dataTransferManager.DataRequested -= ShareStorageItemsHandler;
        }

        private async  void ShareStorageItemsHandler(DataTransferManager sender,
                                                    DataRequestedEventArgs e)
        {
            DataRequest request = e.Request;
            request.Data.Properties.Title = "Send this list";
            request.Data.Properties.Description = "Use any of the options below";

            // Because we are making async calls in the DataRequested event handler,
            // we need to get the deferral first.
            DataRequestDeferral deferral = request.GetDeferral();

            // Make sure we always call Complete on the deferral.
            try
            {
                if (SelectedList != null)
                {
                    //request.Data.SetHtmlFormat(SelectedList.ToHtml());
                    
                    request.Data.SetText(SelectedList.ToFacebook());
                    var source = App.Current.Resources["shoppingSource"] as ShoppingSource;
                    if (source != null)
                    {
                        List<StorageFile> storageFiles = new List<StorageFile>();
                        storageFiles.Add(await source.SerializeList(SelectedList));
                        request.Data.SetStorageItems(storageFiles);
                    }
                    //request.Data.SetUri();
                    sender.TargetApplicationChosen += sender_TargetApplicationChosen;
                }
                else
                {
                    e.Request.FailWithDisplayText(
                        "You haven't selected anything. Please click on a shopping list before trying to share it!");
                }
            }
            finally
            {
                deferral.Complete();
            }
        }


        private void sender_TargetApplicationChosen(DataTransferManager sender, TargetApplicationChosenEventArgs args)
        {
        }

        #endregion

        #region Tiles

        private async void OnPinList(object sender, RoutedEventArgs e)
        {
            bottomAppBar.IsSticky = true;
            foreach (SecondaryTile tile in await SecondaryTile.FindAllAsync())
            {
                if (String.Compare(tile.Arguments, SelectedList.UniqueID, StringComparison.Ordinal) == 0)
                {
                    // Now make the delete request.
                    bool isUnpinned =
                        await
                        tile.RequestDeleteForSelectionAsync(Utils.GetElementRect((FrameworkElement)sender),
                                                            Placement.Above);
                    if (isUnpinned)
                    {
                        SelectedList.IsPinned = false;
                    }

                    ToggleAppBarButton();
                    bottomAppBar.IsSticky = false;
                    return;
                }
            }

            // Prepare package images for use as the Tile Logo and small Logo in our tile to be pinned
            var logo = new Uri("ms-appx:///Assets/squareTile-sdk.png");
            var smallLogo = new Uri("ms-appx:///Assets/smallTile-sdk.png");

            // During creation of secondary tile, an application may set additional arguments on the tile that will be passed in during activation.
            // These arguments should be meaningful to the application. In this sample, we'll pass in the date and time the secondary tile was pinned.
            string tileActivationArguments = SelectedList.UniqueID;

            // Create a 1x1 Secondary tile
            var secondaryTile = new SecondaryTile(SelectedList.UniqueID,
                                                  SelectedList.Name,
                                                  SelectedList.Name,
                                                  tileActivationArguments,
                                                  TileOptions.ShowNameOnLogo,
                                                  logo);

            // Specify a foreground text value.
            // The tile background color is inherited from the parent unless a separate value is specified.
            secondaryTile.ForegroundText = ForegroundText.Dark;
            secondaryTile.BackgroundColor = Colors.GreenYellow;

            // Like the background color, the small tile logo is inherited from the parent application tile by default. Let's override it, just to see how that's done.
            secondaryTile.SmallLogo = smallLogo;

            // OK, the tile is created and we can now attempt to pin the tile.
            // Note that the status message is updated when the async operation to pin the tile completes.
            bool isPinned =
                await
                secondaryTile.RequestCreateForSelectionAsync(Utils.GetElementRect((FrameworkElement)sender),
                                                             Placement.Above);
            if (isPinned)
            {
                SelectedList.IsPinned = true;
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(SelectedList.UniqueID).EnableNotificationQueue(true);

                Utils.UpdateSecondaryTile(SelectedList.UniqueID, SelectedList.Products, SelectedList.Name);
            }
            else
            {
                SelectedList.IsPinned = false;
            }
            ToggleAppBarButton();
        }


        private void ToggleAppBarButton()
        {
            if (SelectedList == null) return;
            btnPinList.Visibility = Visibility.Visible;
            if (SelectedList.IsPinned)
            {
                btnPinList.SetValue(AutomationProperties.NameProperty, "Unpin from Start");
                btnPinList.Style = Application.Current.Resources["UnPinAppBarButtonStyle"] as Style;
            }
            else
            {
                btnPinList.SetValue(AutomationProperties.NameProperty, "Pin to Start");
                btnPinList.Style = Application.Current.Resources["PinAppBarButtonStyle"] as Style;
            }
        }

        private void InitializeAppTiles()
        {
            var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            ITileWidePeekImage02 appTile = TileContentFactory.CreateTileWidePeekImage02();
            appTile.Image.Src = "ms-appx:///Assets/WideLogo.png";
            appTile.Image.Alt = "Shopping List";

            appTile.TextHeading.Text = "Latest Shopping Lists";
            //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText03);
            //XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            List<string> listNames = new List<string>();
            if (source != null)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (source.ShoppingLists.Count > i)
                        listNames.Add(source.ShoppingLists[i].Name);
                    else break;
                }
            }
            
            if (listNames.Count > 0)
                appTile.TextBody1.Text = listNames[0];
            if (listNames.Count > 1)
                appTile.TextBody2.Text = listNames[1];
            if (listNames.Count > 2)
                appTile.TextBody3.Text = listNames[2];
            if (listNames.Count > 3)
                appTile.TextBody4.Text = listNames[3];


            ITileSquarePeekImageAndText01 squareTile = TileContentFactory.CreateTileSquarePeekImageAndText01();

            squareTile.Image.Src = "ms-appx:///Assets/Logo.png";
            squareTile.Image.Alt = "Lists";

            squareTile.TextHeading.Text = "Lists";

            if (listNames.Count > 0)
                squareTile.TextBody1.Text = listNames[0];
            if (listNames.Count > 1)
                squareTile.TextBody2.Text = listNames[1];
            if (listNames.Count > 2)
                squareTile.TextBody3.Text = listNames[2];

            appTile.SquareContent = squareTile;
            TileUpdateManager.CreateTileUpdaterForApplication().Update(appTile.CreateNotification());

            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value", source.ShoppingLists.Count.ToString());
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);

            //XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            //XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            //badgeElement.SetAttribute("value", source.ShoppingLists.Count.ToString());
            //BadgeNotification badge = new BadgeNotification(badgeXml);
            //BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(badge);
            //var tileNotification = new TileNotification(tileXml);
            //TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);

            if (source != null)
                foreach (ShoppingList shoppingList in source.ShoppingLists)
                {
                    if (SecondaryTile.Exists(shoppingList.UniqueID))
                    {
                        Utils.UpdateSecondaryTile(shoppingList.UniqueID, shoppingList.Products, shoppingList.Name);
                    }
                }
        }

        #endregion

        private void OnShareList(object sender, RoutedEventArgs e)
        {
            Popup popUp = new Popup();
            popUp.IsLightDismissEnabled = true;
            StackPanel panel = new StackPanel();
            panel.Background = bottomAppBar.Background;
            panel.Height = 60;
            panel.Width = 180;
            Button btnFaceboook = new Button();
            btnFaceboook.Content = "On Facebook";
            btnFaceboook.Style = (Style)App.Current.Resources["TextButtonStyle"];
            btnFaceboook.Margin = new Thickness(20, 5, 20, 5);
            btnFaceboook.Click += ShareOnFacebookClick;
            Button btnTwitter = new Button();
            btnTwitter.Content = "On Twitter";
            btnTwitter.Style = (Style)App.Current.Resources["TextButtonStyle"];
            btnTwitter.Margin = new Thickness(20, 5, 20, 5);
            btnTwitter.Click += ShareOnTwitterClick;
            panel.Children.Add(btnFaceboook);
            popUp.Child = panel;
            popUp.HorizontalOffset = Window.Current.CoreWindow.Bounds.Right - (Window.Current.CoreWindow.Bounds.Right - panel.Width - 4);
            popUp.VerticalOffset = Window.Current.CoreWindow.Bounds.Bottom - bottomAppBar.ActualHeight - panel.Height - 4;
            popUp.IsOpen = true;

        }

        private void ShareOnTwitterClick(object sender, RoutedEventArgs e)
        {

        }

        private async void ShareOnFacebookClick(object sender, RoutedEventArgs e)
        {
            try
            {
                await fbClient.GetUserDetails("me");
            }
            catch (Exception)
            {
                fbClient.AccessToken = string.Empty;
                fbClient.IsLoggedIn = false;
            }
            if (fbClient.IsLoggedIn == false)
                await fbClient.Login();
            Popup popup = new Popup();
            popup.Child = new FacebookDialog();
            popup.VerticalAlignment = VerticalAlignment.Center;
            popup.HorizontalAlignment = HorizontalAlignment.Center;
            double height = Window.Current.Bounds.Height;
            double width = Window.Current.Bounds.Width;

            popup.Margin = new Thickness((width - 400) / 2, (height - 300)/2, (width - 400)/2, (height - 400)/2);
            popup.IsOpen = true;
        }

        private async void OnFilePickerOpen(object sender, RoutedEventArgs e)
        {

            
                FileOpenPicker openPicker = new FileOpenPicker();
                openPicker.ViewMode = PickerViewMode.List;
                openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".shoplist");
                IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();
                if (files.Count > 0)
                {
                    // Application now has read/write access to the picked file(s)
                    foreach (StorageFile file in files)
                    {
                        LoadList(file);
                    }
                }
                else
                {
                    new MessageDialog("Operation cancelled.").ShowAsync();
                }
        }
        #region Drag Events
        private void OnListDragStarging(object sender, DragItemsStartingEventArgs e)
        {
            DraggedList = e.Items[0] as ShoppingList;
        }

        private async void ListDropped(object sender, DragEventArgs e)
        {
            var point = e.GetPosition(itemGridView);
            var point2 = Utils.GetElementRect(sender as Grid);
            //var findDescendant = Utils.FindDescendant<Grid>(itemGridView);
            var texts = Utils.FindVisualChildren<TextBlock>(sender as Grid);
            var source = App.Current.Resources["shoppingSource"] as ShoppingSource;
            ShoppingList copyList = null;
            
            foreach (var shoppingList in source.ShoppingLists)
            {
                foreach (var textBlock in texts)
                {
                    if(textBlock.Text == shoppingList.Name)
                    {
                        copyList = shoppingList;
                        break;
                    }
                }
                if (copyList != null)
                {
                    foreach (var product in DraggedList.Products)
                    {
                        copyList.Products.Add(product);
                    }
                    itemGridView.ItemsSource = itemsViewSource.Source = source.ShoppingLists;
                    return;
                }
            }
        }
        #endregion
    }
}