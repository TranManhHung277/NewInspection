using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public class CoalesceBrushConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        foreach (var value in values)
        {
            if (value is Brush brush)
                return brush;
        }
        return null;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

public enum DisplayFieldValueType
{
    String,
    Int32,
    Float,
    Double,
    Decimal,
    DateTime
}

public enum ColorZoneTarget
{
    None,
    Border,
    Background,
    Text
}

public partial class DisplayField : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(DisplayField),
            new PropertyMetadata(null, OnValueChanged));

    public static readonly DependencyProperty ValueTypeProperty =
        DependencyProperty.Register(nameof(ValueType), typeof(DisplayFieldValueType), typeof(DisplayField),
            new PropertyMetadata(DisplayFieldValueType.String, OnValueTypeChanged));

    public static readonly DependencyProperty DisplayDecimalsProperty =
        DependencyProperty.Register(nameof(DisplayDecimals), typeof(int), typeof(DisplayField),
            new PropertyMetadata(-1, OnDisplayFormatChanged));

    public static readonly DependencyProperty FormatStringProperty =
        DependencyProperty.Register(nameof(FormatString), typeof(string), typeof(DisplayField),
            new PropertyMetadata(string.Empty, OnDisplayFormatChanged));

    public static readonly DependencyProperty TextStyleProperty =
        DependencyProperty.Register(nameof(TextStyle), typeof(Style), typeof(DisplayField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(DisplayField),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty InputHeightProperty =
        DependencyProperty.Register(nameof(InputHeight), typeof(double), typeof(DisplayField),
            new PropertyMetadata(36.0, OnFontSizeRelatedPropertyChanged));

    public static readonly DependencyProperty InputFontSizeProperty =
        DependencyProperty.Register(nameof(InputFontSize), typeof(double), typeof(DisplayField),
            new PropertyMetadata(14.0, OnFontSizeRelatedPropertyChanged));

    public static readonly DependencyProperty ActualFontSizeProperty =
        DependencyProperty.Register(nameof(ActualFontSize), typeof(double), typeof(DisplayField),
            new PropertyMetadata(14.0));

    private const double VerticalPadding = 8.0;
    private const double FontSizeRatio = 0.65;

    public static readonly DependencyProperty InputFontFamilyProperty =
        DependencyProperty.Register(nameof(InputFontFamily), typeof(FontFamily), typeof(DisplayField),
            new PropertyMetadata(SystemFonts.MessageFontFamily));

    public static readonly DependencyProperty InputFontWeightProperty =
        DependencyProperty.Register(nameof(InputFontWeight), typeof(FontWeight), typeof(DisplayField),
            new PropertyMetadata(FontWeights.Normal));

    public static readonly DependencyProperty HorizontalTextAlignmentProperty =
        DependencyProperty.Register(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(DisplayField),
            new PropertyMetadata(TextAlignment.Left));

    public static readonly DependencyProperty VerticalTextAlignmentProperty =
        DependencyProperty.Register(nameof(VerticalTextAlignment), typeof(VerticalAlignment), typeof(DisplayField),
            new PropertyMetadata(VerticalAlignment.Center));

    public static readonly DependencyProperty InputBackgroundProperty =
        DependencyProperty.Register(nameof(InputBackground), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InputBorderBrushProperty =
        DependencyProperty.Register(nameof(InputBorderBrush), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InputForegroundProperty =
        DependencyProperty.Register(nameof(InputForeground), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null));

    // Color Zone Properties
    public static readonly DependencyProperty ColorZoneTargetProperty =
        DependencyProperty.Register(nameof(ColorZoneTarget), typeof(ColorZoneTarget), typeof(DisplayField),
            new PropertyMetadata(ColorZoneTarget.None, OnColorZonePropertyChanged));

    public static readonly DependencyProperty Threshold1Property =
        DependencyProperty.Register(nameof(Threshold1), typeof(double?), typeof(DisplayField),
            new PropertyMetadata(null, OnColorZonePropertyChanged));

    public static readonly DependencyProperty Threshold2Property =
        DependencyProperty.Register(nameof(Threshold2), typeof(double?), typeof(DisplayField),
            new PropertyMetadata(null, OnColorZonePropertyChanged));

    public static readonly DependencyProperty Threshold3Property =
        DependencyProperty.Register(nameof(Threshold3), typeof(double?), typeof(DisplayField),
            new PropertyMetadata(null, OnColorZonePropertyChanged));

    public static readonly DependencyProperty ZoneColor1Property =
        DependencyProperty.Register(nameof(ZoneColor1), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null, OnColorZonePropertyChanged));

    public static readonly DependencyProperty ZoneColor2Property =
        DependencyProperty.Register(nameof(ZoneColor2), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null, OnColorZonePropertyChanged));

    public static readonly DependencyProperty ZoneColor3Property =
        DependencyProperty.Register(nameof(ZoneColor3), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null, OnColorZonePropertyChanged));

    public static readonly DependencyProperty ZoneColor4Property =
        DependencyProperty.Register(nameof(ZoneColor4), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null, OnColorZonePropertyChanged));

    public static readonly DependencyProperty ActiveZoneBorderBrushProperty =
        DependencyProperty.Register(nameof(ActiveZoneBorderBrush), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ActiveZoneBackgroundProperty =
        DependencyProperty.Register(nameof(ActiveZoneBackground), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ActiveZoneForegroundProperty =
        DependencyProperty.Register(nameof(ActiveZoneForeground), typeof(Brush), typeof(DisplayField),
            new PropertyMetadata(null));

    public DisplayField()
    {
        InitializeComponent();
        UpdateDisplayText();
        UpdateActualFontSize();
    }

    private static void OnFontSizeRelatedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DisplayField control)
        {
            control.UpdateActualFontSize();
        }
    }

    private void UpdateActualFontSize()
    {
        var maxFontSize = (InputHeight - VerticalPadding) * FontSizeRatio;
        ActualFontSize = Math.Min(InputFontSize, maxFontSize);
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public DisplayFieldValueType ValueType
    {
        get => (DisplayFieldValueType)GetValue(ValueTypeProperty);
        set => SetValue(ValueTypeProperty, value);
    }

    public int DisplayDecimals
    {
        get => (int)GetValue(DisplayDecimalsProperty);
        set => SetValue(DisplayDecimalsProperty, value);
    }

    public string FormatString
    {
        get => (string)GetValue(FormatStringProperty);
        set => SetValue(FormatStringProperty, value);
    }

    public Style? TextStyle
    {
        get => (Style?)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        private set => SetValue(DisplayTextProperty, value);
    }

    public double InputHeight
    {
        get => (double)GetValue(InputHeightProperty);
        set => SetValue(InputHeightProperty, value);
    }

    public double InputFontSize
    {
        get => (double)GetValue(InputFontSizeProperty);
        set => SetValue(InputFontSizeProperty, value);
    }

    public double ActualFontSize
    {
        get => (double)GetValue(ActualFontSizeProperty);
        private set => SetValue(ActualFontSizeProperty, value);
    }

    public FontFamily InputFontFamily
    {
        get => (FontFamily)GetValue(InputFontFamilyProperty);
        set => SetValue(InputFontFamilyProperty, value);
    }

    public FontWeight InputFontWeight
    {
        get => (FontWeight)GetValue(InputFontWeightProperty);
        set => SetValue(InputFontWeightProperty, value);
    }

    public TextAlignment HorizontalTextAlignment
    {
        get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
        set => SetValue(HorizontalTextAlignmentProperty, value);
    }

    public VerticalAlignment VerticalTextAlignment
    {
        get => (VerticalAlignment)GetValue(VerticalTextAlignmentProperty);
        set => SetValue(VerticalTextAlignmentProperty, value);
    }

    public Brush? InputBackground
    {
        get => (Brush?)GetValue(InputBackgroundProperty);
        set => SetValue(InputBackgroundProperty, value);
    }

    public Brush? InputBorderBrush
    {
        get => (Brush?)GetValue(InputBorderBrushProperty);
        set => SetValue(InputBorderBrushProperty, value);
    }

    public Brush? InputForeground
    {
        get => (Brush?)GetValue(InputForegroundProperty);
        set => SetValue(InputForegroundProperty, value);
    }

    public ColorZoneTarget ColorZoneTarget
    {
        get => (ColorZoneTarget)GetValue(ColorZoneTargetProperty);
        set => SetValue(ColorZoneTargetProperty, value);
    }

    public double? Threshold1
    {
        get => (double?)GetValue(Threshold1Property);
        set => SetValue(Threshold1Property, value);
    }

    public double? Threshold2
    {
        get => (double?)GetValue(Threshold2Property);
        set => SetValue(Threshold2Property, value);
    }

    public double? Threshold3
    {
        get => (double?)GetValue(Threshold3Property);
        set => SetValue(Threshold3Property, value);
    }

    public Brush? ZoneColor1
    {
        get => (Brush?)GetValue(ZoneColor1Property);
        set => SetValue(ZoneColor1Property, value);
    }

    public Brush? ZoneColor2
    {
        get => (Brush?)GetValue(ZoneColor2Property);
        set => SetValue(ZoneColor2Property, value);
    }

    public Brush? ZoneColor3
    {
        get => (Brush?)GetValue(ZoneColor3Property);
        set => SetValue(ZoneColor3Property, value);
    }

    public Brush? ZoneColor4
    {
        get => (Brush?)GetValue(ZoneColor4Property);
        set => SetValue(ZoneColor4Property, value);
    }

    public Brush? ActiveZoneBorderBrush
    {
        get => (Brush?)GetValue(ActiveZoneBorderBrushProperty);
        private set => SetValue(ActiveZoneBorderBrushProperty, value);
    }

    public Brush? ActiveZoneBackground
    {
        get => (Brush?)GetValue(ActiveZoneBackgroundProperty);
        private set => SetValue(ActiveZoneBackgroundProperty, value);
    }

    public Brush? ActiveZoneForeground
    {
        get => (Brush?)GetValue(ActiveZoneForegroundProperty);
        private set => SetValue(ActiveZoneForegroundProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DisplayField control)
        {
            control.UpdateDisplayText();
            control.UpdateColorZone();
        }
    }

    private static void OnColorZonePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DisplayField control)
        {
            control.UpdateColorZone();
        }
    }

    private static void OnValueTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DisplayField control)
        {
            control.UpdateDisplayText();
        }
    }

    private static void OnDisplayFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DisplayField control)
        {
            control.UpdateDisplayText();
        }
    }

    private void UpdateDisplayText()
    {
        DisplayText = FormatValue(Value);
    }

    private string FormatValue(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var culture = CultureInfo.CurrentCulture;
        var format = string.IsNullOrWhiteSpace(FormatString) ? null : FormatString;

        if (format != null)
        {
            return string.Format(culture, "{0:" + format + "}", value);
        }

        return ValueType switch
        {
            DisplayFieldValueType.String => value.ToString() ?? string.Empty,
            DisplayFieldValueType.Int32 => Convert.ToInt32(value, culture).ToString(culture),
            DisplayFieldValueType.Float => FormatNumber(Convert.ToSingle(value, culture), DisplayDecimals, culture),
            DisplayFieldValueType.Double => FormatNumber(Convert.ToDouble(value, culture), DisplayDecimals, culture),
            DisplayFieldValueType.Decimal => FormatNumber(Convert.ToDecimal(value, culture), DisplayDecimals, culture),
            DisplayFieldValueType.DateTime => FormatDateTime(value, culture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatNumber<T>(T value, int decimals, CultureInfo culture) where T : struct, IFormattable
    {
        return decimals >= 0
            ? value.ToString($"F{decimals}", culture)
            : value.ToString(null, culture);
    }

    private static string FormatDateTime(object value, CultureInfo culture)
    {
        if (value is DateTime dateTime)
        {
            return dateTime.ToString(culture);
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset.ToString(culture);
        }

        if (DateTime.TryParse(value.ToString(), culture, DateTimeStyles.None, out var parsed))
        {
            return parsed.ToString(culture);
        }

        return value.ToString() ?? string.Empty;
    }

    private void UpdateColorZone()
    {
        // Reset active zone colors
        ActiveZoneBorderBrush = null;
        ActiveZoneBackground = null;
        ActiveZoneForeground = null;

        if (ColorZoneTarget == ColorZoneTarget.None || Value == null)
        {
            return;
        }

        // Try to get numeric value
        if (!TryGetNumericValue(out var numericValue))
        {
            return;
        }

        // Get and sort thresholds
        var thresholds = GetSortedThresholds();
        if (thresholds.Count == 0)
        {
            return;
        }

        // Determine which zone the value falls into
        var zoneColor = DetermineZoneColor(numericValue, thresholds);
        if (zoneColor == null)
        {
            return;
        }

        // Apply color to the target
        switch (ColorZoneTarget)
        {
            case ColorZoneTarget.Border:
                ActiveZoneBorderBrush = zoneColor;
                break;
            case ColorZoneTarget.Background:
                ActiveZoneBackground = zoneColor;
                break;
            case ColorZoneTarget.Text:
                ActiveZoneForeground = zoneColor;
                break;
        }
    }

    private bool TryGetNumericValue(out double numericValue)
    {
        numericValue = 0;

        if (Value == null)
            return false;

        var culture = CultureInfo.CurrentCulture;

        try
        {
            switch (ValueType)
            {
                case DisplayFieldValueType.Int32:
                    numericValue = Convert.ToInt32(Value, culture);
                    return true;
                case DisplayFieldValueType.Float:
                    numericValue = Convert.ToSingle(Value, culture);
                    return !double.IsNaN(numericValue);
                case DisplayFieldValueType.Double:
                    numericValue = Convert.ToDouble(Value, culture);
                    return !double.IsNaN(numericValue);
                case DisplayFieldValueType.Decimal:
                    numericValue = (double)Convert.ToDecimal(Value, culture);
                    return true;
                default:
                    return double.TryParse(Value.ToString(), NumberStyles.Float, culture, out numericValue);
            }
        }
        catch
        {
            return double.TryParse(Value.ToString(), NumberStyles.Float, culture, out numericValue);
        }
    }

    private List<double> GetSortedThresholds()
    {
        var thresholds = new List<double>();

        if (Threshold1.HasValue) thresholds.Add(Threshold1.Value);
        if (Threshold2.HasValue) thresholds.Add(Threshold2.Value);
        if (Threshold3.HasValue) thresholds.Add(Threshold3.Value);

        thresholds.Sort();
        return thresholds;
    }

    private Brush? DetermineZoneColor(double value, List<double> thresholds)
    {
        // Colors array corresponding to zones (before first threshold, between thresholds, after last threshold)
        var colors = new[] { ZoneColor1, ZoneColor2, ZoneColor3, ZoneColor4 };

        // Find the zone index based on thresholds
        var zoneIndex = 0;
        foreach (var threshold in thresholds)
        {
            if (value >= threshold)
                zoneIndex++;
            else
                break;
        }

        // Ensure we don't exceed available colors
        zoneIndex = Math.Min(zoneIndex, colors.Length - 1);

        return colors[zoneIndex];
    }
}
