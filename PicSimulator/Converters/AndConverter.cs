using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;

namespace PicSimulator.Converters
{
    public class AndConverter : IMultiValueConverter
    {
        // Correct implementation of the required Convert method
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null) return false;
            return values.All(v => v is bool b && b);
        }

        // Optional: Remove this other Convert method to avoid confusion 
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return false;
            return values.All(v => v is bool b && b);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}