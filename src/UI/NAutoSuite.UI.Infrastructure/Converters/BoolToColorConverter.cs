using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace NAutoSuite.UI.Infrastructure.Converters;

/// <summary>
/// Converts a boolean value to a Color based on parameter
/// Parameter can be: 'Red', 'Yellow', 'Green', 'Orange', etc.
/// True = Full color, False = Dark/Off color
/// </summary>
public class BoolToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool isOn)
            return GetOffColor();

        var colorName = parameter?.ToString()?.ToUpperInvariant() ?? "RED";

        if (!isOn)
            return GetOffColor();

        return colorName switch
        {
            "RED" => Colors.Red,
            "YELLOW" => Colors.Yellow,
            "GREEN" => Colors.LimeGreen,
            "ORANGE" => Colors.Orange,
            "BLUE" => Colors.Blue,
            _ => Colors.Gray
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private static Color GetOffColor()
    {
        // Dark gray for "off" state
        return Color.FromRgb(40, 40, 40);
    }
}
