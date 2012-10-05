using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TheShoppingList.Common
{
    class BooleanToColorConverter : IValueConverter
    {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                return (value is bool && (bool)value) ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.White);
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                return value is SolidColorBrush && (SolidColorBrush)value == new SolidColorBrush(Colors.White);
            }
    }
}
