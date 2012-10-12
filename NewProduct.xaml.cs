using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TheShoppingList.Classes;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace TheShoppingList
{
    public sealed partial class NewProduct
    {
        private List<Product> _products;
        private Product _product;

        public bool ProductAdded { get; set; }

        public enum InputMode
        {
            AddProduct,
            EditProduct
        }

        public NewProduct()
        {
            InitializeComponent();
            _products = new List<Product>();
            Loaded += PopupLoaded;
            ProductAdded = false;
        }

        public InputMode Mode { get; set; }

        public Product Product
        {
            get { return _product; }
            set { _product = value; }
        }

        public List<Product> Products
        {
            get { return _products; }
            set { _products = value; }
        }

        private void PopupLoaded(object sender, RoutedEventArgs e)
        {
            var size = Application.Current.Resources["newListSize"] as Point;
            if (Mode == InputMode.AddProduct)
            {
                _product = new Product();
                btnSave.SetValue(AutomationProperties.NameProperty, "Add");
            }
            else
            {
                FillProperties();
                btnSave.SetValue(AutomationProperties.NameProperty, "Save");
            }

            if (size != null)
            {
                transparentBorder.Width = size.Width;
                transparentBorder.Height = size.Height;
                newListBorder.Width = transparentBorder.Width;
                transparentBorder.Visibility = Visibility.Visible;
            }
        }

        private void FillProperties()
        {
            if (Product == null)
            {
                var p = Parent as Popup;
                if (p != null) p.IsOpen = false;
            }
            txtProductName.Text = Product.Name;
            txtPrice.Text = Product.Price.ToString();
            txtQuantity.Text = Product.Quantity.ToString();
            txtShopName.Text = Product.ShopName;
            quantityType.SelectedIndex = IndexFromQuantityType(Product.QuantityType);
        }

        private int IndexFromQuantityType(QuantityType type)
        {
            int index = -1;
            switch (type)
            {
                case QuantityType.pcs:
                    index = 0;
                    break;
                case QuantityType.kg:
                    index = 1;
                    break;
                case QuantityType.m:
                    index = 2;
                    break;
                case QuantityType.ft:
                    index = 3;
                    break;
                case QuantityType.lb:
                    index = 4;
                    break;
            }
            return index;
        }

        private void OnSaveProductDetails(object sender, RoutedEventArgs e)
        {
            if (Product == null)
                Product = new Product();
            if (txtProductName.Text == string.Empty)
            {
                new MessageDialog("You must type the product name").ShowAsync();
                Product = null;
                return;
            }
            int index = quantityType.SelectedIndex;
            var type = QuantityType.kg;
            switch (index)
            {
                case 0:
                    type = QuantityType.kg;
                    break;
                case 1:
                    type = QuantityType.m;
                    break;
                case 2:
                    type = QuantityType.ft;
                    break;
                case 3:
                    type = QuantityType.lb;
                    break;
            }
            if (txtProductName.Text != string.Empty)
                Product.Name = txtProductName.Text;
            if (txtPrice.Text != string.Empty)
                Product.Price = Convert.ToDouble(txtPrice.Text);
            if (txtQuantity.Text != string.Empty)
                Product.Quantity = Convert.ToDouble(txtQuantity.Text);
            Product.QuantityType = type;
            if (txtShopName.Text != string.Empty)
                Product.ShopName = txtShopName.Text;
            txtProductName.Text = string.Empty;
            txtPrice.Text = string.Empty;
            txtQuantity.Text = string.Empty;
            txtShopName.Text = string.Empty;
            quantityType.SelectedIndex = -1;
            btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Close");
            Products.Add(Product);
            ProductAdded = true;
        }

        private bool IsNumber(string text)
        {
            var regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
            return !regex.IsMatch(text);
        }

        private void ProductNameChanged(object sender, TextChangedEventArgs e)
        {
            //var source = Application.Current.Resources["shoppingSource"] as ShoppingSource;
            //if (source == null) return;
            //foreach (ShoppingList list in source.ShoppingLists)
            //{
            //    if (String.Compare(list.Name, txtPrice.Text, StringComparison.Ordinal) == 0)
            //    {
            //        txtPrice.BorderBrush = new SolidColorBrush(Colors.Red);
            //        var color = new Color();
            //        //string colorcode = "#FFD87474";
            //        //int argb = Int32.Parse(colorcode.Replace("#", ""), NumberStyles.HexNumber);
            //        Color clr = Color.FromArgb(0xFF, 0xD8, 0x74, 0x74);
            //        txtPrice.Background = new SolidColorBrush(clr);
            //        btnSave.IsEnabled = false;
            //    }
            //    else
            //    {
            //        txtPrice.BorderBrush = new SolidColorBrush(Colors.Black);
            //        txtPrice.Background = new SolidColorBrush(Colors.White);
            //        if (btnSave.IsEnabled == false)
            //            btnSave.IsEnabled = true;
            //    }

            //}
        }

        private void ShopNameChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductAdded == true)
            {
                ProductAdded = false;
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Close");
            }
            else
            {
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Cancel");
            }
        }

        private void PriceChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductAdded == true)
            {
                ProductAdded = false;
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Close");
            }
            else
            {
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Cancel");
            }
            if (IsNumber(txtPrice.Text) == false)
            {
                txtPrice.BorderBrush = new SolidColorBrush(Colors.Red);
                txtPrice.Background = new SolidColorBrush(Colors.IndianRed);
            }
            else
            {
                txtPrice.BorderBrush = new SolidColorBrush(Colors.Transparent);
                txtPrice.Background = new SolidColorBrush(Colors.White);
            }
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            Product = null;
            var p = Parent as Popup;
            if (p != null) p.IsOpen = false; // close the Popup
        }

        private void QuantityTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductAdded == true)
            {
                ProductAdded = false;
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Close");
            }
            else
            {
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Cancel");
            }
        }

        private void OnProductNameChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductAdded == true)
            {
                ProductAdded = false;
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Close");
            }
            else
            {
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Cancel");
            }
        }

        private void OnQuantityTextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductAdded == true)
            {
                ProductAdded = false;
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Close");
            }
            else
            {
                btnCancelOrClose.SetValue(AutomationProperties.NameProperty, "Cancel");
            }
        }

        private void newListBorder_Unloaded(object sender, RoutedEventArgs e)
        {
        }
    }
}