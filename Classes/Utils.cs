using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NotificationsExtensions.TileContent;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.System.UserProfile;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.StartScreen;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace TheShoppingList.Classes
{
    public class Utils
    {
        public static bool IsNumber(string text)
        {
            if(text == null)
                return false;
            var regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        public static void UpdateSecondaryTile(string tileID, ObservableCollection<Product> products, string ListName)
        {
            if(SecondaryTile.Exists(tileID)  == false)
                return;


            //var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            ITileWidePeekImage02 secAppTile = TileContentFactory.CreateTileWidePeekImage02();
            secAppTile.Image.Src = "ms-appx:///Assets/WideLogo.png";
            secAppTile.Image.Alt = ListName;

            secAppTile.TextHeading.Text = "Products On List";
            //XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText03);
            //XmlNodeList tileTextAttributes = tileXml.GetElementsByTagName("text");
            List<string> productNames = new List<string>();
             for (int i = 0; i < 4; i++)
                {
                    if (products.Count > i && products[i].IsBought == false)
                        productNames.Add(products[i].Title);
                    else break;
                }
            if (productNames.Count > 0)
                 secAppTile.TextBody1.Text = productNames[0];
             if (productNames.Count > 1)
                 secAppTile.TextBody2.Text = productNames[1];
             if (productNames.Count > 2)
                 secAppTile.TextBody3.Text = productNames[2];
             if (productNames.Count > 3)
                 secAppTile.TextBody4.Text = productNames[3];


            ITileSquarePeekImageAndText01 secSquareTile = TileContentFactory.CreateTileSquarePeekImageAndText01();

            secSquareTile.Image.Src = "ms-appx:///Assets/Logo.png";
            secSquareTile.Image.Alt = "Products";

            secSquareTile.TextHeading.Text = "Products";

            if (productNames.Count > 0)
                secSquareTile.TextBody1.Text = productNames[0];
            if (productNames.Count > 1)
                secSquareTile.TextBody2.Text = productNames[1];
            if (productNames.Count > 2)
                secSquareTile.TextBody3.Text = productNames[2];

            secAppTile.SquareContent = secSquareTile;
            TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileID).Update(secAppTile.CreateNotification());
            XmlDocument badgeXml = BadgeUpdateManager.GetTemplateContent(BadgeTemplateType.BadgeGlyph);
            XmlElement badgeElement = (XmlElement)badgeXml.SelectSingleNode("/badge");
            badgeElement.SetAttribute("value",products.Count.ToString());
            BadgeNotification badge = new BadgeNotification(badgeXml);
            BadgeUpdateManager.CreateBadgeUpdaterForSecondaryTile(tileID).Update(badge);
            
        }

        

        public static void SortProducts(ShoppingList list)
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

        public static Rect GetElementRect(FrameworkElement element)
        {
            GeneralTransform buttonTransform = element.TransformToVisual(null);
            Windows.Foundation.Point point = buttonTransform.TransformPoint(new Windows.Foundation.Point());
            return new Rect(point, new Size(element.ActualWidth, element.ActualHeight));
        }

        public static int Comparison(Product product, Product product1)
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

        public static T FindDescendant<T>(DependencyObject obj) where T : DependencyObject
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
                if (child != null)
                    return child as T;
            }
            
            return null;
        }

        public static RegionInfo GetCountryInfo()
        {
            string geo = GlobalizationPreferences.HomeGeographicRegion;
            var region = new RegionInfo(geo);
            return region;
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

        public static int IndexFromQuantityType(QuantityType type)
        {
            int index;
            switch (type)
            {
                case QuantityType.pcs:
                    index = 0;
                    break;
                case QuantityType.kg:
                    index = 1;
                    break;
                case QuantityType.l:
                    index = 2;
                    break;
                case QuantityType.m:
                    index = 3;
                    break;
                case QuantityType.ft:
                    index = 4;
                    break;
                case QuantityType.lb:
                    index = 5;
                    break;
                default:
                    index = -1;
                    break;
            }
            return index;
        }

    }
}
