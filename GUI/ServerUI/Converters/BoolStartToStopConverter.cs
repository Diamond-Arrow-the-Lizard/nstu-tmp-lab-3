using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ServerUI.Converters;

public class BoolToStartStopConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is bool running ? (running ? "Stop" : "Start") : "Start";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => 
        throw new NotSupportedException();
}