using System.Globalization;
using System.Windows.Data;

namespace TradeSystem.App.Converters;

/// <summary>
/// 涨跌幅颜色转换：正数红色，负数绿色
/// </summary>
public class ChangePctColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is decimal pct)
        {
            if (pct > 0) return System.Windows.Media.Brushes.Red;
            if (pct < 0) return System.Windows.Media.Brushes.ForestGreen;
        }
        return System.Windows.Media.Brushes.Gray;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
