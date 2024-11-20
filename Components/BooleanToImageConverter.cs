using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiApp3.Components
{
    public class BooleanToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isExpanded)
            {
                return isExpanded ? "collapse.png" : "expand.png"; // Use your image file names
            }

            return "expand.png";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
