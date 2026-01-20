using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class ParameterInputControl : UserControl
{
    private static readonly Brush NormalBorder = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x42));
    private static readonly Brush FocusBorder = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly Brush ErrorBorder = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(ParameterInputControl),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ParameterInputControl),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(ParameterInputControl),
            new PropertyMetadata(string.Empty, OnUnitChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(ParameterInputControl),
            new PropertyMetadata(double.MinValue));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(ParameterInputControl),
            new PropertyMetadata(double.MaxValue));

    public static readonly DependencyProperty IncrementProperty =
        DependencyProperty.Register(nameof(Increment), typeof(double), typeof(ParameterInputControl),
            new PropertyMetadata(1.0));

    public static readonly DependencyProperty DecimalPlacesProperty =
        DependencyProperty.Register(nameof(DecimalPlaces), typeof(int), typeof(ParameterInputControl),
            new PropertyMetadata(2));

    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(ParameterInputControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty ShowSpinButtonsProperty =
        DependencyProperty.Register(nameof(ShowSpinButtons), typeof(bool), typeof(ParameterInputControl),
            new PropertyMetadata(true, OnShowSpinButtonsChanged));

    public static readonly DependencyProperty TextAlignmentProperty =
        DependencyProperty.Register(nameof(TextAlignment), typeof(TextAlignment), typeof(ParameterInputControl),
            new PropertyMetadata(TextAlignment.Right));

    public static readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(ParameterInputControl),
            new PropertyMetadata(NormalBorder));

    public static readonly DependencyProperty LabelVisibilityProperty =
        DependencyProperty.Register(nameof(LabelVisibility), typeof(Visibility), typeof(ParameterInputControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty UnitVisibilityProperty =
        DependencyProperty.Register(nameof(UnitVisibility), typeof(Visibility), typeof(ParameterInputControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty SpinButtonsVisibilityProperty =
        DependencyProperty.Register(nameof(SpinButtonsVisibility), typeof(Visibility), typeof(ParameterInputControl),
            new PropertyMetadata(Visibility.Visible));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Increment
    {
        get => (double)GetValue(IncrementProperty);
        set => SetValue(IncrementProperty, value);
    }

    public int DecimalPlaces
    {
        get => (int)GetValue(DecimalPlacesProperty);
        set => SetValue(DecimalPlacesProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    public bool ShowSpinButtons
    {
        get => (bool)GetValue(ShowSpinButtonsProperty);
        set => SetValue(ShowSpinButtonsProperty, value);
    }

    public TextAlignment TextAlignment
    {
        get => (TextAlignment)GetValue(TextAlignmentProperty);
        set => SetValue(TextAlignmentProperty, value);
    }

    public new Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Visibility LabelVisibility
    {
        get => (Visibility)GetValue(LabelVisibilityProperty);
        set => SetValue(LabelVisibilityProperty, value);
    }

    public Visibility UnitVisibility
    {
        get => (Visibility)GetValue(UnitVisibilityProperty);
        set => SetValue(UnitVisibilityProperty, value);
    }

    public Visibility SpinButtonsVisibility
    {
        get => (Visibility)GetValue(SpinButtonsVisibilityProperty);
        set => SetValue(SpinButtonsVisibilityProperty, value);
    }

    public ParameterInputControl()
    {
        InitializeComponent();
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ParameterInputControl control)
        {
            control.LabelVisibility = string.IsNullOrEmpty(control.Label) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ParameterInputControl control)
        {
            control.UnitVisibility = string.IsNullOrEmpty(control.Unit) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ParameterInputControl control)
        {
            control.UpdateDisplayValue();
        }
    }

    private static void OnShowSpinButtonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ParameterInputControl control)
        {
            control.SpinButtonsVisibility = control.ShowSpinButtons ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateDisplayValue()
    {
        if (!ValueInput.IsFocused)
        {
            ValueInput.Text = Value.ToString($"F{DecimalPlaces}", CultureInfo.CurrentCulture);
        }
    }

    private void ValueInput_GotFocus(object sender, RoutedEventArgs e)
    {
        BorderBrush = FocusBorder;
        ValueInput.SelectAll();
    }

    private void ValueInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (double.TryParse(ValueInput.Text, NumberStyles.Any, CultureInfo.CurrentCulture, out var value))
        {
            Value = Math.Max(Minimum, Math.Min(Maximum, value));
            BorderBrush = NormalBorder;
        }
        else
        {
            BorderBrush = ErrorBorder;
        }
        UpdateDisplayValue();
    }

    private void ValueInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        // Allow digits, decimal separator, and minus sign
        var regex = new Regex(@"^[0-9\-\.,]$");
        e.Handled = !regex.IsMatch(e.Text);
    }

    private void ValueInput_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ValueInput_LostFocus(sender, e);
            Keyboard.ClearFocus();
        }
        else if (e.Key == Key.Up)
        {
            IncrementValue();
            e.Handled = true;
        }
        else if (e.Key == Key.Down)
        {
            DecrementValue();
            e.Handled = true;
        }
    }

    private void IncrementButton_Click(object sender, RoutedEventArgs e)
    {
        IncrementValue();
    }

    private void DecrementButton_Click(object sender, RoutedEventArgs e)
    {
        DecrementValue();
    }

    private void IncrementValue()
    {
        Value = Math.Min(Maximum, Math.Round(Value + Increment, DecimalPlaces));
    }

    private void DecrementValue()
    {
        Value = Math.Max(Minimum, Math.Round(Value - Increment, DecimalPlaces));
    }
}
