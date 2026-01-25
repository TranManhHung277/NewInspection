using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using NAutoSuite.UI.Controls.Dialogs;

namespace NAutoSuite.UI.Controls;

public enum InputFieldValidationMode
{
    None,
    Uppercase,
    Phone,
    IpAddress,
    MacAddress,
    Email
}

public partial class InputField : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(nameof(Text), typeof(string), typeof(InputField),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public static readonly DependencyProperty PendingTextProperty =
        DependencyProperty.Register(nameof(PendingText), typeof(string), typeof(InputField),
            new PropertyMetadata(string.Empty, OnPendingTextChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(object), typeof(InputField),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty ValueTypeProperty =
        DependencyProperty.Register(nameof(ValueType), typeof(InputFieldValueType), typeof(InputField),
            new PropertyMetadata(InputFieldValueType.String));

    public static readonly DependencyProperty DisplayDecimalsProperty =
        DependencyProperty.Register(nameof(DisplayDecimals), typeof(int), typeof(InputField),
            new PropertyMetadata(-1, OnDisplayDecimalsChanged));

    public static readonly DependencyProperty KeyboardTypeProperty =
        DependencyProperty.Register(nameof(KeyboardType), typeof(InputFieldKeyboardType), typeof(InputField),
            new PropertyMetadata(InputFieldKeyboardType.Text));

    public static readonly DependencyProperty AutoShowOnFocusProperty =
        DependencyProperty.Register(nameof(AutoShowOnFocus), typeof(bool), typeof(InputField),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ShowKeyboardButtonProperty =
        DependencyProperty.Register(nameof(ShowKeyboardButton), typeof(bool), typeof(InputField),
            new PropertyMetadata(false));

    public static readonly DependencyProperty AllowDecimalProperty =
        DependencyProperty.Register(nameof(AllowDecimal), typeof(bool), typeof(InputField),
            new PropertyMetadata(true));

    public static readonly DependencyProperty AllowNegativeProperty =
        DependencyProperty.Register(nameof(AllowNegative), typeof(bool), typeof(InputField),
            new PropertyMetadata(true));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(InputField),
            new PropertyMetadata(false));

    public static readonly DependencyProperty CommitOnEnterProperty =
        DependencyProperty.Register(nameof(CommitOnEnter), typeof(bool), typeof(InputField),
            new PropertyMetadata(true, OnCommitOnEnterChanged));

    public static readonly DependencyProperty TextBoxStyleProperty =
        DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(InputField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InputHeightProperty =
        DependencyProperty.Register(nameof(InputHeight), typeof(double), typeof(InputField),
            new PropertyMetadata(36.0, OnFontSizeRelatedPropertyChanged));

    public static readonly DependencyProperty InputFontSizeProperty =
        DependencyProperty.Register(nameof(InputFontSize), typeof(double), typeof(InputField),
            new PropertyMetadata(14.0, OnFontSizeRelatedPropertyChanged));

    public static readonly DependencyProperty ActualFontSizeProperty =
        DependencyProperty.Register(nameof(ActualFontSize), typeof(double), typeof(InputField),
            new PropertyMetadata(14.0));

    private const double VerticalPadding = 8.0;
    private const double FontSizeRatio = 0.65;

    public static readonly DependencyProperty InputFontFamilyProperty =
        DependencyProperty.Register(nameof(InputFontFamily), typeof(FontFamily), typeof(InputField),
            new PropertyMetadata(SystemFonts.MessageFontFamily));

    public static readonly DependencyProperty InputFontWeightProperty =
        DependencyProperty.Register(nameof(InputFontWeight), typeof(FontWeight), typeof(InputField),
            new PropertyMetadata(FontWeights.Normal));

    public static readonly DependencyProperty HorizontalTextAlignmentProperty =
        DependencyProperty.Register(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(InputField),
            new PropertyMetadata(TextAlignment.Left));

    public static readonly DependencyProperty VerticalTextAlignmentProperty =
        DependencyProperty.Register(nameof(VerticalTextAlignment), typeof(VerticalAlignment), typeof(InputField),
            new PropertyMetadata(VerticalAlignment.Center));

    public static readonly DependencyProperty InputBackgroundProperty =
        DependencyProperty.Register(nameof(InputBackground), typeof(Brush), typeof(InputField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InputBorderBrushProperty =
        DependencyProperty.Register(nameof(InputBorderBrush), typeof(Brush), typeof(InputField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty InputForegroundProperty =
        DependencyProperty.Register(nameof(InputForeground), typeof(Brush), typeof(InputField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ValidationModeProperty =
        DependencyProperty.Register(nameof(ValidationMode), typeof(InputFieldValidationMode), typeof(InputField),
            new PropertyMetadata(InputFieldValidationMode.None));

    public static readonly DependencyProperty MinValueProperty =
        DependencyProperty.Register(nameof(MinValue), typeof(double?), typeof(InputField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty MaxValueProperty =
        DependencyProperty.Register(nameof(MaxValue), typeof(double?), typeof(InputField),
            new PropertyMetadata(null));

    public static readonly DependencyProperty IsValidProperty =
        DependencyProperty.Register(nameof(IsValid), typeof(bool), typeof(InputField),
            new PropertyMetadata(true));

    public static readonly DependencyProperty ValidationErrorMessageProperty =
        DependencyProperty.Register(nameof(ValidationErrorMessage), typeof(string), typeof(InputField),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty InvalidBorderBrushProperty =
        DependencyProperty.Register(nameof(InvalidBorderBrush), typeof(Brush), typeof(InputField),
            new PropertyMetadata(null));

    private bool _isDialogOpen;
    private bool _suppressNextFocus;
    private bool _isSyncing;
    private bool _suppressValueFormat;
    private string _lastCommittedText = string.Empty;

    public InputField()
    {
        InitializeComponent();
        UpdateActualFontSize();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Validate initial value and store it
        _lastCommittedText = PendingText;
        ValidateInput();
    }

    private static void OnFontSizeRelatedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is InputField control)
        {
            control.UpdateActualFontSize();
        }
    }

    private void UpdateActualFontSize()
    {
        var maxFontSize = (InputHeight - VerticalPadding) * FontSizeRatio;
        ActualFontSize = Math.Min(InputFontSize, maxFontSize);
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

    public InputFieldValueType ValueType
    {
        get => (InputFieldValueType)GetValue(ValueTypeProperty);
        set => SetValue(ValueTypeProperty, value);
    }

    public int DisplayDecimals
    {
        get => (int)GetValue(DisplayDecimalsProperty);
        set => SetValue(DisplayDecimalsProperty, value);
    }

    public InputFieldKeyboardType KeyboardType
    {
        get => (InputFieldKeyboardType)GetValue(KeyboardTypeProperty);
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

    public InputFieldValidationMode ValidationMode
    {
        get => (InputFieldValidationMode)GetValue(ValidationModeProperty);
        set => SetValue(ValidationModeProperty, value);
    }

    public double? MinValue
    {
        get => (double?)GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public double? MaxValue
    {
        get => (double?)GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        private set => SetValue(IsValidProperty, value);
    }

    public string ValidationErrorMessage
    {
        get => (string)GetValue(ValidationErrorMessageProperty);
        private set => SetValue(ValidationErrorMessageProperty, value);
    }

    public Brush? InvalidBorderBrush
    {
        get => (Brush?)GetValue(InvalidBorderBrushProperty);
        set => SetValue(InvalidBorderBrushProperty, value);
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InputField control || control._isSyncing)
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
        control._lastCommittedText = control.PendingText;
        control.ValidateInput();
    }

    private static void OnPendingTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not InputField control || control._isSyncing)
        {
            return;
        }

        // Apply text transformation (e.g., uppercase)
        control.ApplyTextTransformation();

        // Validate input
        var isValid = control.ValidateInput();

        if (!isValid && !control.CommitOnEnter)
        {
            control.RevertToLastCommitted();
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
        if (d is not InputField control || control._isSyncing)
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
        if (d is not InputField control)
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
        if (d is not InputField control)
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

        if (!AutoShowOnFocus || KeyboardType != InputFieldKeyboardType.Text || IsReadOnly || _isDialogOpen)
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
            string? result = KeyboardType == InputFieldKeyboardType.Numeric
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
        if (!ValidateInput(PendingText))
        {
            RevertToLastCommitted();
            return;
        }

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
            if (!formatAfterCommit)
            {
                _lastCommittedText = PendingText;
            }
            return;
        }

        _isSyncing = true;
        Text = converted?.ToString() ?? string.Empty;
        _isSyncing = false;
        _lastCommittedText = Text;
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
        _lastCommittedText = formatted;
        ValidateInput();
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
            InputFieldValueType.String => value.ToString() ?? string.Empty,
            InputFieldValueType.Int32 => Convert.ToInt32(value, culture).ToString(culture),
            InputFieldValueType.Float => FormatNumber(Convert.ToSingle(value, culture), decimals, culture),
            InputFieldValueType.Double => FormatNumber(Convert.ToDouble(value, culture), decimals, culture),
            InputFieldValueType.Decimal => FormatNumber(Convert.ToDecimal(value, culture), decimals, culture),
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

        if (ValueType == InputFieldValueType.String)
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
            case InputFieldValueType.Int32:
                if (int.TryParse(input, NumberStyles.Integer, culture, out var intValue))
                {
                    converted = intValue;
                    return true;
                }
                return false;

            case InputFieldValueType.Float:
                if (float.TryParse(input, NumberStyles.Float, culture, out var floatValue))
                {
                    converted = floatValue;
                    return true;
                }
                return false;

            case InputFieldValueType.Double:
                if (double.TryParse(input, NumberStyles.Float, culture, out var doubleValue))
                {
                    converted = doubleValue;
                    return true;
                }
                return false;

            case InputFieldValueType.Decimal:
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

    private bool ValidateInput()
    {
        return ValidateInput(PendingText);
    }

    private bool ValidateInput(string text)
    {
        // Pattern validation for string types
        if (ValueType == InputFieldValueType.String)
        {
            var (isValid, errorMessage) = ValidatePattern(text);
            IsValid = isValid;
            ValidationErrorMessage = errorMessage;
            return isValid;
        }

        // Numeric range validation
        if (ValueType != InputFieldValueType.String && !string.IsNullOrWhiteSpace(text))
        {
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var numericValue))
            {
                IsValid = false;
                ValidationErrorMessage = "Invalid number format";
                return false;
            }

            var (isValid, errorMessage) = ValidateNumericRange(numericValue);
            IsValid = isValid;
            ValidationErrorMessage = errorMessage;
            return isValid;
        }

        IsValid = true;
        ValidationErrorMessage = string.Empty;
        return true;
    }

    private (bool isValid, string errorMessage) ValidatePattern(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return (true, string.Empty);
        }

        return ValidationMode switch
        {
            InputFieldValidationMode.Phone => ValidatePhone(text),
            InputFieldValidationMode.IpAddress => ValidateIpAddress(text),
            InputFieldValidationMode.MacAddress => ValidateMacAddress(text),
            InputFieldValidationMode.Email => ValidateEmail(text),
            _ => (true, string.Empty)
        };
    }

    private static (bool isValid, string errorMessage) ValidatePhone(string text)
    {
        // Supports formats: +84123456789, 0123456789, 123-456-7890, (123) 456-7890
        var pattern = @"^[\+]?[(]?[0-9]{1,4}[)]?[-\s\.]?[0-9]{1,4}[-\s\.]?[0-9]{1,9}$";
        var isValid = Regex.IsMatch(text, pattern);
        return (isValid, isValid ? string.Empty : "Invalid phone number format");
    }

    private static (bool isValid, string errorMessage) ValidateIpAddress(string text)
    {
        // IPv4: 192.168.1.1
        var ipv4Pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
        var isValid = Regex.IsMatch(text, ipv4Pattern);
        return (isValid, isValid ? string.Empty : "Invalid IP address format");
    }

    private static (bool isValid, string errorMessage) ValidateMacAddress(string text)
    {
        // Supports: AA:BB:CC:DD:EE:FF, AA-BB-CC-DD-EE-FF, AABBCCDDEEFF
        var pattern = @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$|^([0-9A-Fa-f]{12})$";
        var isValid = Regex.IsMatch(text, pattern);
        return (isValid, isValid ? string.Empty : "Invalid MAC address format");
    }

    private static (bool isValid, string errorMessage) ValidateEmail(string text)
    {
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        var isValid = Regex.IsMatch(text, pattern);
        return (isValid, isValid ? string.Empty : "Invalid email format");
    }

    private (bool isValid, string errorMessage) ValidateNumericRange(double value)
    {
        var min = MinValue;
        var max = MaxValue;

        // No validation if both are null
        if (!min.HasValue && !max.HasValue)
            return (true, string.Empty);

        // Only Max: value <= Max
        if (!min.HasValue)
            return value <= max!.Value ? (true, string.Empty) : (false, $"Value must be ≤ {max.Value}");

        // Only Min: value >= Min
        if (!max.HasValue)
            return value >= min.Value ? (true, string.Empty) : (false, $"Value must be ≥ {min.Value}");

        // Both Min and Max - Normal range: value in [Min, Max]
        if (max.Value >= min.Value)
            return value >= min.Value && value <= max.Value
                ? (true, string.Empty)
                : (false, $"Value must be between {min.Value} and {max.Value}");

        // Inverted range: value outside (Max, Min) - i.e., value <= Max OR value >= Min
        return value <= max.Value || value >= min.Value
            ? (true, string.Empty)
            : (false, $"Value must be ≤ {max.Value} or ≥ {min.Value}");
    }

    private void ApplyTextTransformation()
    {
        if (ValidationMode == InputFieldValidationMode.Uppercase && ValueType == InputFieldValueType.String)
        {
            var currentText = PendingText;
            var upperText = currentText.ToUpperInvariant();
            if (currentText != upperText)
            {
                var caretIndex = InputBox.CaretIndex;
                _isSyncing = true;
                PendingText = upperText;
                _isSyncing = false;
                InputBox.CaretIndex = caretIndex;
            }
        }
    }

    private void RevertToLastCommitted()
    {
        _isSyncing = true;
        PendingText = _lastCommittedText;
        if (!BindingOperations.IsDataBound(this, TextProperty))
        {
            Text = _lastCommittedText;
        }
        _isSyncing = false;
        ValidateInput();
    }
}

public enum InputFieldKeyboardType
{
    Text,
    Numeric
}

public enum InputFieldValueType
{
    String,
    Int32,
    Float,
    Double,
    Decimal
}
