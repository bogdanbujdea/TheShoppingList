using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheShoppingList.Classes
{
    public enum QuantityType
    {
        kg,
        m,
        lb,
        ft,
        pcs
    };

    public class Product : INotifyPropertyChanged
    {

        public Product()
        {
            IsBought = false;
        }

        public int Index { get; set; }

        private string _title;
        private string _shopName;
        private double _price;
        private bool _isBought;
        private double _quantity;
        private QuantityType _quantityType;
        private string _listPrice;
        private string _image;
        private string _category;

        public string Image
        {
            get
            {
                if (IsBought)
                    _image = @"Assets/removefromcart.png";
                else
                    _image = @"Assets/addtocart.png";
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                OnPropertyChanged("Title");
            }
        }

        public string ListPrice
        {
            get { return _listPrice; }
            set
            {
                _listPrice = value;
                OnPropertyChanged("ListPrice");
            }
        }

        public string Currency
        {
            get
            {
                return RegionInfo.CurrentRegion.ISOCurrencySymbol;
            }
        }

        public string ShopName
        {
            get { return _shopName; }
            set
            {
                _shopName = value;
                OnPropertyChanged("ShopName");
            }
        }

        public double Price
        {
            get { return _price; }
            set
            {
                _listPrice = value + " " + Currency;
                _price = value;
                OnPropertyChanged("Price");
            }
        }

        public bool IsBought
        {
            get { return _isBought; }
            set
            {
                _isBought = value;

                if (_isBought)
                {
                    Category = "In Cart";
                    _image = @"Assets/checked.png";
                }
                else
                {
                    Category = "Remaining Products";
                    _image = @"Assets/unchecked.png";
                }
                OnPropertyChanged("IsBought");
            }
        }

        public double Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        public QuantityType QuantityType
        {
            get { return _quantityType; }
            set
            {
                _quantityType = value;
                OnPropertyChanged("QuantityType");
            }
        }

        public string Category
        {
            get { return _category; }
            set { _category = value; OnPropertyChanged("Category");}
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
