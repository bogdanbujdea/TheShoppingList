using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace TheShoppingList.Common
{
    class IntToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (value is int && (int)value % 2 == 0) ? new SolidColorBrush(Colors.DarkGray) : new SolidColorBrush(Colors.DimGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value is SolidColorBrush && (SolidColorBrush)value == new SolidColorBrush(Colors.DarkGray);
        }
    }
}
