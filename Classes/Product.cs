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

        private string _name;
        private string _shopName;
        private double _price;
        private bool _isBought;
        private double _quantity;
        private QuantityType _quantityType;
        private string _listPrice;
        private string _image;

        public string Image
        {
            get
            {
                if (IsBought)
                    _image = @"Assets/checked.png";
                else
                    _image = @"Assets/unchecked.png";
                return _image;
            }
            set
            {
                _image = value;
                OnPropertyChanged("Image");
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                OnPropertyChanged("Name");
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
                    _image = @"Assets/checked.png";
                else
                    _image = @"Assets/unchecked.png";
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
