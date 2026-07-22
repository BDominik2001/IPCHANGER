using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace IpChanger.Converters;

/// <summary>
/// Nem üres szöveg → Visible, üres/null → Collapsed.
/// </summary>
public sealed class StringToVisibilityConverter : IValueConverter
{
    public static readonly StringToVisibilityConverter Instance = new();

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Nem null → Visible, null → Collapsed. Az InstanceInverse a fordítottja.
/// </summary>
public sealed class NullToVisibilityConverter : IValueConverter
{
    public static readonly NullToVisibilityConverter Instance = new() { Invert = false };
    public static readonly NullToVisibilityConverter InstanceInverse = new() { Invert = true };

    public bool Invert { get; init; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasValue = value is not null;
        if (Invert)
            hasValue = !hasValue;
        return hasValue ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
