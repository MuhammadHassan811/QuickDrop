using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace QuickDrop.Converters
{
    public class InvertedBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
                return !booleanValue; // True ise False, False ise True yapar

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
                return !booleanValue;

            return false;
        }
    }
}