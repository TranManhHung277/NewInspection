using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class SensorStatusControl : UserControl
{
    private static readonly Brush OnBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush OffBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
    private static readonly Color OnColor = Color.FromRgb(0x4C, 0xAF, 0x50);
    private static readonly Color OffColor = Color.FromRgb(0x6E, 0x6E, 0x6E);

    public static readonly DependencyProperty SensorNameProperty =
        DependencyProperty.Register(nameof(SensorName), typeof(string), typeof(SensorStatusControl),
            new PropertyMetadata("Sensor"));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(SensorStatusControl),
            new PropertyMetadata(false, OnIsActiveChanged));

    public static readonly DependencyProperty OnTextProperty =
        DependencyProperty.Register(nameof(OnText), typeof(string), typeof(SensorStatusControl),
            new PropertyMetadata("ON"));

    public static readonly DependencyProperty OffTextProperty =
        DependencyProperty.Register(nameof(OffText), typeof(string), typeof(SensorStatusControl),
            new PropertyMetadata("OFF"));

    public static readonly DependencyProperty ShowStatusTextProperty =
        DependencyProperty.Register(nameof(ShowStatusText), typeof(bool), typeof(SensorStatusControl),
            new PropertyMetadata(true, OnShowStatusTextChanged));

    public static readonly DependencyProperty ActiveColorProperty =
        DependencyProperty.Register(nameof(ActiveColor), typeof(Color), typeof(SensorStatusControl),
            new PropertyMetadata(OnColor, OnColorChanged));

    public static readonly DependencyProperty InactiveColorProperty =
        DependencyProperty.Register(nameof(InactiveColor), typeof(Color), typeof(SensorStatusControl),
            new PropertyMetadata(OffColor, OnColorChanged));

    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(nameof(StatusBrush), typeof(Brush), typeof(SensorStatusControl),
            new PropertyMetadata(OffBrush));

    public static readonly DependencyProperty StatusColorProperty =
        DependencyProperty.Register(nameof(StatusColor), typeof(Color), typeof(SensorStatusControl),
            new PropertyMetadata(OffColor));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(SensorStatusControl),
            new PropertyMetadata("OFF"));

    public static readonly DependencyProperty StatusTextVisibilityProperty =
        DependencyProperty.Register(nameof(StatusTextVisibility), typeof(Visibility), typeof(SensorStatusControl),
            new PropertyMetadata(Visibility.Visible));

    public string SensorName
    {
        get => (string)GetValue(SensorNameProperty);
        set => SetValue(SensorNameProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public string OnText
    {
        get => (string)GetValue(OnTextProperty);
        set => SetValue(OnTextProperty, value);
    }

    public string OffText
    {
        get => (string)GetValue(OffTextProperty);
        set => SetValue(OffTextProperty, value);
    }

    public bool ShowStatusText
    {
        get => (bool)GetValue(ShowStatusTextProperty);
        set => SetValue(ShowStatusTextProperty, value);
    }

    public Color ActiveColor
    {
        get => (Color)GetValue(ActiveColorProperty);
        set => SetValue(ActiveColorProperty, value);
    }

    public Color InactiveColor
    {
        get => (Color)GetValue(InactiveColorProperty);
        set => SetValue(InactiveColorProperty, value);
    }

    public Brush StatusBrush
    {
        get => (Brush)GetValue(StatusBrushProperty);
        set => SetValue(StatusBrushProperty, value);
    }

    public Color StatusColor
    {
        get => (Color)GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public Visibility StatusTextVisibility
    {
        get => (Visibility)GetValue(StatusTextVisibilityProperty);
        set => SetValue(StatusTextVisibilityProperty, value);
    }

    public SensorStatusControl()
    {
        InitializeComponent();
        UpdateStatus();
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SensorStatusControl control)
        {
            control.UpdateStatus();
        }
    }

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SensorStatusControl control)
        {
            control.UpdateStatus();
        }
    }

    private static void OnShowStatusTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SensorStatusControl control)
        {
            control.StatusTextVisibility = control.ShowStatusText ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateStatus()
    {
        var color = IsActive ? ActiveColor : InactiveColor;
        StatusColor = color;
        StatusBrush = new SolidColorBrush(color);
        StatusText = IsActive ? OnText : OffText;
    }
}
