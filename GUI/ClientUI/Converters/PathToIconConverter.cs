using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace ClientUI.Converters
{
    public class PathToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string path)
            {
                return path.EndsWith("/") ? "📁" : "📄"; // Example icons
            }
            return "❓";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}