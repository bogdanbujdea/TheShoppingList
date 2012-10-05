using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace TheShoppingList.Common
{
    class BooleanToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is bool && (bool)value) ? 0.6 : 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is double && (double)value == 1.0;
        }
    }
}
