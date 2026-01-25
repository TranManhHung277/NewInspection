using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace NAutoSuite.UI.Controls;

public enum ValueDisplayType
{
    String,
    Int32,
    Float,
    Double,
    Decimal,
    DateTime
}

public partial class ValueDisplay : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(ValueDisplay),
            new PropertyMetadata(null, OnValueChanged));

    public static readonly DependencyProperty ValueTypeProperty =
        DependencyProperty.Register(nameof(ValueType), typeof(ValueDisplayType), typeof(ValueDisplay),
            new PropertyMetadata(ValueDisplayType.String, OnValueTypeChanged));

    public static readonly DependencyProperty DisplayDecimalsProperty =
        DependencyProperty.Register(nameof(DisplayDecimals), typeof(int), typeof(ValueDisplay),
            new PropertyMetadata(-1, OnDisplayFormatChanged));

    public static readonly DependencyProperty FormatStringProperty =
        DependencyProperty.Register(nameof(FormatString), typeof(string), typeof(ValueDisplay),
            new PropertyMetadata(string.Empty, OnDisplayFormatChanged));

    public static readonly DependencyProperty TextStyleProperty =
        DependencyProperty.Register(nameof(TextStyle), typeof(Style), typeof(ValueDisplay),
            new PropertyMetadata(null));

    public static readonly DependencyProperty DisplayTextProperty =
        DependencyProperty.Register(nameof(DisplayText), typeof(string), typeof(ValueDisplay),
            new PropertyMetadata(string.Empty));

    public ValueDisplay()
    {
        InitializeComponent();
        UpdateDisplayText();
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public ValueDisplayType ValueType
    {
        get => (ValueDisplayType)GetValue(ValueTypeProperty);
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

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValueDisplay control)
        {
            control.UpdateDisplayText();
        }
    }

    private static void OnValueTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValueDisplay control)
        {
            control.UpdateDisplayText();
        }
    }

    private static void OnDisplayFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ValueDisplay control)
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
            ValueDisplayType.String => value.ToString() ?? string.Empty,
            ValueDisplayType.Int32 => Convert.ToInt32(value, culture).ToString(culture),
            ValueDisplayType.Float => FormatNumber(Convert.ToSingle(value, culture), DisplayDecimals, culture),
            ValueDisplayType.Double => FormatNumber(Convert.ToDouble(value, culture), DisplayDecimals, culture),
            ValueDisplayType.Decimal => FormatNumber(Convert.ToDecimal(value, culture), DisplayDecimals, culture),
            ValueDisplayType.DateTime => FormatDateTime(value, culture),
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
