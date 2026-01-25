using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public enum DisplayFieldValueType
{
    String,
    Int32,
    Float,
    Double,
    Decimal,
    DateTime
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

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DisplayField control)
        {
            control.UpdateDisplayText();
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
}
