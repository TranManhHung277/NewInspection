using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class NumericDisplayControl : UserControl
{
    private static readonly Brush DefaultValueBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(NumericDisplayControl),
            new PropertyMetadata("Value"));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(NumericDisplayControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(NumericDisplayControl),
            new PropertyMetadata(string.Empty, OnUnitChanged));

    public static readonly DependencyProperty PrefixProperty =
        DependencyProperty.Register(nameof(Prefix), typeof(string), typeof(NumericDisplayControl),
            new PropertyMetadata(string.Empty, OnPrefixChanged));

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(nameof(Format), typeof(string), typeof(NumericDisplayControl),
            new PropertyMetadata("N0", OnValueChanged));

    public static readonly DependencyProperty ValueFontSizeProperty =
        DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(NumericDisplayControl),
            new PropertyMetadata(36.0));

    public static readonly DependencyProperty UnitFontSizeProperty =
        DependencyProperty.Register(nameof(UnitFontSize), typeof(double), typeof(NumericDisplayControl),
            new PropertyMetadata(14.0));

    public static readonly DependencyProperty ValueForegroundProperty =
        DependencyProperty.Register(nameof(ValueForeground), typeof(Brush), typeof(NumericDisplayControl),
            new PropertyMetadata(DefaultValueBrush));

    public static readonly DependencyProperty DisplayValueProperty =
        DependencyProperty.Register(nameof(DisplayValue), typeof(string), typeof(NumericDisplayControl),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty UnitVisibilityProperty =
        DependencyProperty.Register(nameof(UnitVisibility), typeof(Visibility), typeof(NumericDisplayControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty PrefixVisibilityProperty =
        DependencyProperty.Register(nameof(PrefixVisibility), typeof(Visibility), typeof(NumericDisplayControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty ValueAlignmentProperty =
        DependencyProperty.Register(nameof(ValueAlignment), typeof(HorizontalAlignment), typeof(NumericDisplayControl),
            new PropertyMetadata(HorizontalAlignment.Left));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
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

    public string Prefix
    {
        get => (string)GetValue(PrefixProperty);
        set => SetValue(PrefixProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public double ValueFontSize
    {
        get => (double)GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    public double UnitFontSize
    {
        get => (double)GetValue(UnitFontSizeProperty);
        set => SetValue(UnitFontSizeProperty, value);
    }

    public Brush ValueForeground
    {
        get => (Brush)GetValue(ValueForegroundProperty);
        set => SetValue(ValueForegroundProperty, value);
    }

    public string DisplayValue
    {
        get => (string)GetValue(DisplayValueProperty);
        set => SetValue(DisplayValueProperty, value);
    }

    public Visibility UnitVisibility
    {
        get => (Visibility)GetValue(UnitVisibilityProperty);
        set => SetValue(UnitVisibilityProperty, value);
    }

    public Visibility PrefixVisibility
    {
        get => (Visibility)GetValue(PrefixVisibilityProperty);
        set => SetValue(PrefixVisibilityProperty, value);
    }

    public HorizontalAlignment ValueAlignment
    {
        get => (HorizontalAlignment)GetValue(ValueAlignmentProperty);
        set => SetValue(ValueAlignmentProperty, value);
    }

    public NumericDisplayControl()
    {
        InitializeComponent();
        UpdateDisplayValue();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericDisplayControl control)
        {
            control.UpdateDisplayValue();
        }
    }

    private static void OnUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericDisplayControl control)
        {
            control.UnitVisibility = string.IsNullOrEmpty(control.Unit) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnPrefixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is NumericDisplayControl control)
        {
            control.PrefixVisibility = string.IsNullOrEmpty(control.Prefix) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void UpdateDisplayValue()
    {
        try
        {
            DisplayValue = Value.ToString(Format, CultureInfo.CurrentCulture);
        }
        catch
        {
            DisplayValue = Value.ToString(CultureInfo.CurrentCulture);
        }
    }
}
