using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace IpChanger.Converters;

/// <summary>
/// Logikai értéket alakít ecsetté. True → TrueBrush, False → FalseBrush.
/// Státuszsor színezéséhez (hiba = piros, egyébként semleges).
/// </summary>
public sealed class BoolToBrushConverter : IValueConverter
{
    public Brush TrueBrush { get; set; } = Brushes.Red;
    public Brush FalseBrush { get; set; } = Brushes.Gray;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? TrueBrush : FalseBrush;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
