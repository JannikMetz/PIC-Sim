using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace PicSimulator.Converters
{
    public class IntToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                var format = parameter as string ?? "X2"; // Default ist "X2"
                return intValue.ToString(format);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is "")
            {
                return 0;
            }
            
            if (value is string strValue && int.TryParse(strValue, NumberStyles.HexNumber, culture, out int intValue))
            {
                return intValue;
            }
            return 0;
        }
    }
}