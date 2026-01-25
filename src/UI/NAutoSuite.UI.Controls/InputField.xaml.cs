using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NAutoSuite.UI.Controls.Dialogs;

namespace NAutoSuite.UI.Controls;

public partial class KeyboardInput : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(KeyboardInput),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty PendingTextProperty =
        DependencyProperty.Register(nameof(PendingText), typeof(string), typeof(KeyboardInput),
            new PropertyMetadata(string.Empty, OnPendingTextChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(KeyboardInput),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty ValueTypeProperty =
        DependencyProperty.Register(nameof(ValueType), typeof(KeyboardInputValueType), typeof(KeyboardInput),
            new PropertyMetadata(KeyboardInputValueType.String));

    public static readonly DependencyProperty DisplayDecimalsProperty =
        DependencyProperty.Register(nameof(DisplayDecimals), typeof(int), typeof(KeyboardInput),
            new PropertyMetadata(-1, OnDisplayDecimalsChanged));

    public static readonly DependencyProperty KeyboardTypeProperty =
        DependencyProperty.Register(nameof(KeyboardType), typeof(KeyboardInputType), typeof(KeyboardInput),
            new PropertyMetadata(KeyboardInputType.Text));

    public static readonly DependencyProperty AutoShowOnFocusProperty =
        DependencyProperty.Register(nameof(AutoShowOnFocus), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowKeyboardButtonProperty =
        DependencyProperty.Register(nameof(ShowKeyboardButton), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(false));

    public static readonly DependencyProperty AllowDecimalProperty =
        DependencyProperty.Register(nameof(AllowDecimal), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AllowNegativeProperty =
        DependencyProperty.Register(nameof(AllowNegative), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CommitOnEnterProperty =
        DependencyProperty.Register(nameof(CommitOnEnter), typeof(bool), typeof(KeyboardInput),
            new PropertyMetadata(true, OnCommitOnEnterChanged));

    public static readonly DependencyProperty TextBoxStyleProperty =
        DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(KeyboardInput),
            new PropertyMetadata(null));

    private bool _isDialogOpen;
    private bool _suppressNextFocus;
    private bool _isSyncing;
    private bool _suppressValueFormat;

    public KeyboardInput()
    {
        InitializeComponent();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string PendingText
    {
        get => (string)GetValue(PendingTextProperty);
        set => SetValue(PendingTextProperty, value);
    }

    public object? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public KeyboardInputValueType ValueType
    {
        get => (KeyboardInputValueType)GetValue(ValueTypeProperty);
        set => SetValue(ValueTypeProperty, value);
    }

    public int DisplayDecimals
    {
        get => (int)GetValue(DisplayDecimalsProperty);
        set => SetValue(DisplayDecimalsProperty, value);
    }

    public KeyboardInputType KeyboardType
    {
        get => (KeyboardInputType)GetValue(KeyboardTypeProperty);
        set => SetValue(KeyboardTypeProperty, value);
    }

    public bool AutoShowOnFocus
    {
        get => (bool)GetValue(AutoShowOnFocusProperty);
        set => SetValue(AutoShowOnFocusProperty, value);
    }

    public bool ShowKeyboardButton
    {
        get => (bool)GetValue(ShowKeyboardButtonProperty);
        set => SetValue(ShowKeyboardButtonProperty, value);
    }

    public bool AllowDecimal
    {
        get => (bool)GetValue(AllowDecimalProperty);
        set => SetValue(AllowDecimalProperty, value);
    }

    public bool AllowNegative
    {
        get => (bool)GetValue(AllowNegativeProperty);
        set => SetValue(AllowNegativeProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool CommitOnEnter
    {
        get => (bool)GetValue(CommitOnEnterProperty);
        set => SetValue(CommitOnEnterProperty, value);
    }

    public Style? TextBoxStyle
    {
        get => (Style?)GetValue(TextBoxStyleProperty);
        set => SetValue(TextBoxStyleProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control || control._isSyncing)
        {
            return;
        }

        if (BindingOperations.IsDataBound(control, ValueProperty))
        {
            return;
        }

        control._isSyncing = true;
        control.PendingText = e.NewValue?.ToString() ?? string.Empty;
        control._isSyncing = false;
    }

    private static void OnPendingTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control || control._isSyncing)
        {
            return;
        }

        if (control.CommitOnEnter)
        {
            return;
        }

        if (BindingOperations.IsDataBound(control, ValueProperty))
        {
            control.CommitPendingText(formatAfterCommit: false);
            return;
        }

        control._isSyncing = true;
        control.Text = e.NewValue?.ToString() ?? string.Empty;
        control._isSyncing = false;
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control || control._isSyncing)
        {
            return;
        }

        if (control._suppressValueFormat)
        {
            return;
        }

        control.SetPendingFromValue(e.NewValue);
    }

    private static void OnDisplayDecimalsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control)
        {
            return;
        }

        if (!BindingOperations.IsDataBound(control, ValueProperty))
        {
            return;
        }

        control.SetPendingFromValue(control.Value);
    }

    private static void OnCommitOnEnterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not KeyboardInput control)
        {
            return;
        }

        if (!(bool)e.NewValue)
        {
            control._isSyncing = true;
            control.Text = control.PendingText;
            control._isSyncing = false;
        }
        else
        {
            control._isSyncing = true;
            control.PendingText = control.Text;
            control._isSyncing = false;
        }
    }

    private void InputBox_GotKeyboardFocus(object sender, RoutedEventArgs e)
    {
        if (_suppressNextFocus)
        {
            _suppressNextFocus = false;
            return;
        }

        if (!AutoShowOnFocus || KeyboardType != KeyboardInputType.Text || IsReadOnly || _isDialogOpen)
        {
            return;
        }

        OpenKeyboard();
    }

    private void InputBox_OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (!CommitOnEnter || e.Key != System.Windows.Input.Key.Enter)
        {
            return;
        }

        CommitPendingText(formatAfterCommit: true);
        e.Handled = true;
    }

    private void KeyboardButton_Click(object sender, RoutedEventArgs e)
    {
        if (_isDialogOpen)
        {
            return;
        }

        OpenKeyboard();
    }

    private void OpenKeyboard()
    {
        _isDialogOpen = true;

        try
        {
            var owner = Window.GetWindow(this);
            string? result = KeyboardType == KeyboardInputType.Numeric
                ? NumericKeyboardDialog.Show(owner, PendingText, AllowDecimal, AllowNegative)
                : TextKeyboardDialog.Show(owner, PendingText);

            if (result != null)
            {
                PendingText = result;

                CommitPendingText(formatAfterCommit: true);
            }
        }
        finally
        {
            _isDialogOpen = false;
            _suppressNextFocus = true;
        }
    }

    private void CommitPendingText(bool formatAfterCommit)
    {
        if (!TryConvert(PendingText, out var converted))
        {
            if (BindingOperations.IsDataBound(this, ValueProperty))
            {
                SetPendingFromValue(Value);
            }
            return;
        }

        if (BindingOperations.IsDataBound(this, ValueProperty))
        {
            _suppressValueFormat = !formatAfterCommit;
            Value = converted;
            _suppressValueFormat = false;
            return;
        }

        _isSyncing = true;
        Text = converted?.ToString() ?? string.Empty;
        _isSyncing = false;
    }

    private void SetPendingFromValue(object? value)
    {
        var formatted = FormatValue(value);

        _isSyncing = true;
        PendingText = formatted;
        if (!BindingOperations.IsDataBound(this, TextProperty))
        {
            Text = formatted;
        }
        _isSyncing = false;
    }

    private string FormatValue(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        var culture = CultureInfo.CurrentCulture;
        var decimals = DisplayDecimals;

        return ValueType switch
        {
            KeyboardInputValueType.String => value.ToString() ?? string.Empty,
            KeyboardInputValueType.Int32 => Convert.ToInt32(value, culture).ToString(culture),
            KeyboardInputValueType.Float => FormatNumber(Convert.ToSingle(value, culture), decimals, culture),
            KeyboardInputValueType.Double => FormatNumber(Convert.ToDouble(value, culture), decimals, culture),
            KeyboardInputValueType.Decimal => FormatNumber(Convert.ToDecimal(value, culture), decimals, culture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string FormatNumber<T>(T value, int decimals, CultureInfo culture) where T : struct, IFormattable
    {
        return decimals >= 0
            ? value.ToString($"F{decimals}", culture)
            : value.ToString(null, culture);
    }

    private bool TryConvert(string input, out object? converted)
    {
        converted = null;

        if (ValueType == KeyboardInputValueType.String)
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
            case KeyboardInputValueType.Int32:
                if (int.TryParse(input, NumberStyles.Integer, culture, out var intValue))
                {
                    converted = intValue;
                    return true;
                }
                return false;

            case KeyboardInputValueType.Float:
                if (float.TryParse(input, NumberStyles.Float, culture, out var floatValue))
                {
                    converted = floatValue;
                    return true;
                }
                return false;

            case KeyboardInputValueType.Double:
                if (double.TryParse(input, NumberStyles.Float, culture, out var doubleValue))
                {
                    converted = doubleValue;
                    return true;
                }
                return false;

            case KeyboardInputValueType.Decimal:
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

public enum KeyboardInputType
{
    Text,
    Numeric
}

public enum KeyboardInputValueType
{
    String,
    Int32,
    Float,
    Double,
    Decimal
}
