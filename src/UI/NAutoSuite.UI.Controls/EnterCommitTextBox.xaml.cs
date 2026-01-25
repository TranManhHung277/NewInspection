using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NAutoSuite.UI.Controls;

public enum EnterCommitValueType
{
    String,
    Int32,
    Float,
    Double,
    Decimal
}

public partial class EnterCommitTextBox : UserControl
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(object),
        typeof(EnterCommitTextBox),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty ValueTypeProperty = DependencyProperty.Register(
        nameof(ValueType),
        typeof(EnterCommitValueType),
        typeof(EnterCommitTextBox),
        new PropertyMetadata(EnterCommitValueType.String));

    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(EnterCommitTextBox),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TextBoxStyleProperty = DependencyProperty.Register(
        nameof(TextBoxStyle),
        typeof(Style),
        typeof(EnterCommitTextBox),
        new PropertyMetadata(null, OnTextBoxStyleChanged));

    public EnterCommitTextBox()
    {
        InitializeComponent();
        ApplyTextBoxStyle();
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public EnterCommitValueType ValueType
    {
        get => (EnterCommitValueType)GetValue(ValueTypeProperty);
        set => SetValue(ValueTypeProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Style? TextBoxStyle
    {
        get => (Style?)GetValue(TextBoxStyleProperty);
        set => SetValue(TextBoxStyleProperty, value);
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EnterCommitTextBox control)
        {
            return;
        }

        control.Text = control.FormatValue(e.NewValue);
    }

    private static void OnTextBoxStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EnterCommitTextBox control)
        {
            return;
        }

        control.ApplyTextBoxStyle();
    }

    private void ApplyTextBoxStyle()
    {
        if (InputBox == null)
        {
            return;
        }

        if (TextBoxStyle != null)
        {
            InputBox.Style = TextBoxStyle;
            return;
        }

        var defaultStyle = TryFindResource("ModernTextBox") as Style;
        if (defaultStyle != null)
        {
            InputBox.Style = defaultStyle;
        }
    }

    private string FormatValue(object? value)
    {
        return value?.ToString() ?? string.Empty;
    }

    private void InputBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        if (!TryConvert(Text, out var converted))
        {
            Text = FormatValue(Value);
            return;
        }

        Value = converted;
    }

    private bool TryConvert(string input, out object? converted)
    {
        converted = null;

        if (ValueType == EnterCommitValueType.String)
        {
            converted = input;
            return true;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var culture = CultureInfo.CurrentCulture;
        switch (ValueType)
        {
            case EnterCommitValueType.Int32:
                if (int.TryParse(input, NumberStyles.Integer, culture, out var intValue))
                {
                    converted = intValue;
                    return true;
                }
                return false;

            case EnterCommitValueType.Float:
                if (float.TryParse(input, NumberStyles.Float, culture, out var floatValue))
                {
                    converted = floatValue;
                    return true;
                }
                return false;

            case EnterCommitValueType.Double:
                if (double.TryParse(input, NumberStyles.Float, culture, out var doubleValue))
                {
                    converted = doubleValue;
                    return true;
                }
                return false;

            case EnterCommitValueType.Decimal:
                if (decimal.TryParse(input, NumberStyles.Number, culture, out var decimalValue))
                {
                    converted = decimalValue;
                    return true;
                }
                return false;

            default:
                return false;
        }
    }
}
