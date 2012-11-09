using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public static void UpdateSecondaryTile(string tileID, ObservableCollection<Product> products)
        {
            if(SecondaryTile.Exists(tileID)  == false)
                return;
            var textXmlTile = GetXmlTextTile(products);
            TileNotification tileTextNotification = new TileNotification(textXmlTile) {Tag = "products"};
            var imageXmlTile = GetXmlImageTile(products);
            TileNotification tileImageNotification = new TileNotification(imageXmlTile) {Tag = "image"};
            // Send the notification to the secondary tile by creating a secondary tile updater
            try
            {
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileID).Update(tileTextNotification);
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileID).Update(tileImageNotification);
            }
            catch (Exception)
            {
                
            }
        }

        private static XmlDocument GetXmlImageTile(ObservableCollection<Product> products)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareImage);
            XmlNodeList imageTags = tileXml.GetElementsByTagName("image");
            imageTags[0].Attributes[1].InnerText = "ms-appx:///Assets/squareTile-sdk.png";
            return tileXml;
        }

        private static XmlDocument GetXmlTextTile(ObservableCollection<Product> products)
        {
            XmlDocument tileXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileSquareText03);
            XmlNodeList textAttributes = tileXml.GetElementsByTagName("text");
            int i = 0;
            for (int index = 0; index < products.Count; index++)
            {
                if (products.Count > i)
                    textAttributes[i].InnerText = products[i].Title;
                else break;
                if (i++ == 3)
                    break;
            }
            return tileXml;
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
