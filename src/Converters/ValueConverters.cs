using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace FortnitePerformanceOptimizer.Converters;

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is bool isRunning)
        {
            return isRunning ? System.Windows.Media.Brushes.LimeGreen : System.Windows.Media.Brushes.Red;
        }
        return System.Windows.Media.Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

public class MBToGBConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is long mb)
        {
            return $"{mb / 1024} GB";
        }
        return "0 GB";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

public class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        if (value is int count && count > 0)
        {
            return Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}

public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo? culture)
    {
        throw new NotImplementedException();
    }
}
