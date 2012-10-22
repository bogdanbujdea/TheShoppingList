using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace TheShoppingList.Classes
{
    public class ShoppingList : INotifyPropertyChanged
    {
        public ShoppingList()
        {
            _products = new ObservableCollection<Product>();
            
           Image = @"Assets/cart.png";
        }
        private ObservableCollection<Product> _products;
        private string _name;
        private DateTime _createdTime;
        private DateTime _finishedTime;
        private double _balance;
        private double _totalCost;
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

        public Double Balance
        {
            get { return _balance; }
            set { _balance = value; OnPropertyChanged("Balance");}
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

        public DateTime FinishedTime
        {
            get { return _finishedTime; }
            set { _finishedTime = value;
                OnPropertyChanged("FinishedTime");
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
