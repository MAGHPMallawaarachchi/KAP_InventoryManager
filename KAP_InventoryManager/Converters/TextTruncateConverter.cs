using System;
using System.Globalization;
using System.Windows.Data;

namespace KAP_InventoryManager.Converters
{
    public class TextTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string text && parameter is string lengthStr && int.TryParse(lengthStr, out int maxLength))
            {
                if (text.Length > maxLength)
                {
                    return text.Substring(0, maxLength) + "...";
                }
                return text;
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
