using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Windows.UI.StartScreen;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TheShoppingList.Classes
{
    public class ShoppingList : INotifyPropertyChanged
    {
        public ShoppingList()
        {
            _products = new ObservableCollection<Product>();
            _uniqueId = Guid.NewGuid().ToString();
           Image = @"Assets/cart.png";
        }
        private ObservableCollection<Product> _products;
        private string _name;
        private DateTime _createdTime;
        private DateTime _reminderTime;
        private double _budget;
        private double _totalCost;
        private int _count;
        private string _uniqueId;
        private bool _isPinned;
        private string _uiBudget;
        private int _inlistCount;
        public string Image { get; set; }

        public double TotalCost
        {
            get { return _totalCost; }
            set { _totalCost = value; OnPropertyChanged("TotalCost");}
        }

// ReSharper disable ConvertToAutoProperty
        public ObservableCollection<Product> Products
// ReSharper restore ConvertToAutoProperty
        {
            get { return _products; }
            set { _products = value; }
        }

        public bool IsPinned
        {
            get
            {
                _isPinned = SecondaryTile.Exists(_uniqueId); return _isPinned; }
            set { _isPinned = value; OnPropertyChanged("IsPinned");}
        }

        public int Count
        {
            get
            {
                _count = Products.Count; return _count; }
            set { _count = value; OnPropertyChanged("Count");}
        }

        public string UniqueID
        {
            get { return _uniqueId; }
            set { _uniqueId = value; OnPropertyChanged("UniqueID");}
        }

        public Double Budget
        {
            get { return _budget; }
            set { _budget = value; OnPropertyChanged("Budget");}
        }

        public string UIBudget
        {
            get
            {
                _uiBudget = Budget + " " + Utils.GetCountryInfo().CurrencySymbol;
                return _uiBudget; 
            }
            set { _uiBudget = value; OnPropertyChanged("UIBudget");}
        }

        public int InlistCount
        {
            get
            {
                int s = Products.Count(product => product.IsBought == false);
                _inlistCount = s;
                return _inlistCount;
            }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value;
                OnPropertyChanged("Name");
            }
        }

        public DateTime CreatedTime
        {
            get { return _createdTime; }
            set { _createdTime = value;
                OnPropertyChanged("CreatedTime");
            }
        }

        public DateTime ReminderTime
        {
            get { return _reminderTime; }
            set { _reminderTime = value;
                OnPropertyChanged("ReminderTime");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string ToHtml()
        {
            StringBuilder htmlList = new StringBuilder();
            
            htmlList.Append("<h1>" + Name + "</h1><br />");
            htmlList.Append("<ol>");
            foreach (var product in Products)
            {
                htmlList.AppendLine("<li>");
                htmlList.Append(product.Title);
                
                
                if (Math.Abs(product.Price - 0) > 0)
                {

                    htmlList.Append(product.Price.ToString() + RegionInfo.CurrentRegion.CurrencySymbol);
                }
                if(Math.Abs(product.Quantity - 0) > 0)
                {
                    htmlList.AppendLine("&nbsp;&nbsp;");
                    htmlList.Append(product.Quantity.ToString());
                    if (product.QuantityType != QuantityType.Default)
                    {
                        htmlList.Append("&nbsp;" + product.QuantityType.ToString());
                    }
                }
                if(string.IsNullOrEmpty(product.ShopName))
                {
                    htmlList.AppendLine("From: ");
                    htmlList.Append(product.ShopName);
                    htmlList.Append("<br />");
                }
                htmlList.Append("</li>");
            }
            htmlList.Append("</ol>");
            
            return htmlList.ToString();
        }

        public override string ToString()
        {
            StringBuilder stringList = new StringBuilder();

            stringList.Append(Name);
            stringList.AppendLine();
            foreach (var product in Products)
            {
                stringList.Append(product.Title + " :   ");


                if (Math.Abs(product.Price - 0) > 0)
                {

                    stringList.Append(product.Price.ToString() + " " + RegionInfo.CurrentRegion.CurrencySymbol + ", ");
                }
                if (Math.Abs(product.Quantity - 0) > 0)
                {
                    stringList.Append(product.Quantity.ToString() + " ");
                    if (product.QuantityType != QuantityType.Default)
                    {
                        stringList.Append(" " + product.QuantityType.ToString() + ", ");
                    }
                }
                if (string.IsNullOrEmpty(product.ShopName) == false)
                {
                    stringList.Append("From: ");
                    stringList.Append(product.ShopName);
                }
                stringList.AppendLine("|");
            }
            

            return stringList.ToString();
        }
    }
}
