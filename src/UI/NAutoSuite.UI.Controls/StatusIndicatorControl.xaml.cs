using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class StatusIndicatorControl : UserControl
{
    public enum IndicatorState
    {
        Off,
        On,
        Error,
        Warning,
        Info
    }

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(IndicatorState), typeof(StatusIndicatorControl),
            new PropertyMetadata(IndicatorState.Off, OnStateChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(StatusIndicatorControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IndicatorSizeProperty =
        DependencyProperty.Register(nameof(IndicatorSize), typeof(double), typeof(StatusIndicatorControl),
            new PropertyMetadata(16.0));

    public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(StatusIndicatorControl),
            new PropertyMetadata(false, OnIsActiveChanged));

    public static readonly DependencyProperty ActiveColorProperty =
        DependencyProperty.Register(nameof(ActiveColor), typeof(Color), typeof(StatusIndicatorControl),
            new PropertyMetadata(Color.FromRgb(0x4C, 0xAF, 0x50), OnColorChanged));

    public static readonly DependencyProperty InactiveColorProperty =
        DependencyProperty.Register(nameof(InactiveColor), typeof(Color), typeof(StatusIndicatorControl),
            new PropertyMetadata(Color.FromRgb(0x6E, 0x6E, 0x6E), OnColorChanged));

    public static readonly DependencyProperty IndicatorBrushProperty =
        DependencyProperty.Register(nameof(IndicatorBrush), typeof(Brush), typeof(StatusIndicatorControl),
            new PropertyMetadata(new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E))));

    public static readonly DependencyProperty IndicatorColorProperty =
        DependencyProperty.Register(nameof(IndicatorColor), typeof(Color), typeof(StatusIndicatorControl),
            new PropertyMetadata(Color.FromRgb(0x6E, 0x6E, 0x6E)));

    public static readonly DependencyProperty LabelForegroundProperty =
        DependencyProperty.Register(nameof(LabelForeground), typeof(Brush), typeof(StatusIndicatorControl),
            new PropertyMetadata(new SolidColorBrush(Colors.White)));

    public static readonly DependencyProperty LabelVisibilityProperty =
        DependencyProperty.Register(nameof(LabelVisibility), typeof(Visibility), typeof(StatusIndicatorControl),
            new PropertyMetadata(Visibility.Visible));

    public IndicatorState State
    {
        get => (IndicatorState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public double IndicatorSize
    {
        get => (double)GetValue(IndicatorSizeProperty);
        set => SetValue(IndicatorSizeProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
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

    public Brush IndicatorBrush
    {
        get => (Brush)GetValue(IndicatorBrushProperty);
        set => SetValue(IndicatorBrushProperty, value);
    }

    public Color IndicatorColor
    {
        get => (Color)GetValue(IndicatorColorProperty);
        set => SetValue(IndicatorColorProperty, value);
    }

    public Brush LabelForeground
    {
        get => (Brush)GetValue(LabelForegroundProperty);
        set => SetValue(LabelForegroundProperty, value);
    }

    public Visibility LabelVisibility
    {
        get => (Visibility)GetValue(LabelVisibilityProperty);
        set => SetValue(LabelVisibilityProperty, value);
    }

    public StatusIndicatorControl()
    {
        InitializeComponent();
        UpdateIndicatorColor();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusIndicatorControl control)
        {
            control.UpdateIndicatorFromState();
        }
    }

    private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusIndicatorControl control)
        {
            control.UpdateIndicatorColor();
        }
    }

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusIndicatorControl control)
        {
            control.UpdateIndicatorColor();
        }
    }

    private void UpdateIndicatorFromState()
    {
        var color = State switch
        {
            IndicatorState.On => Color.FromRgb(0x4C, 0xAF, 0x50),      // Green
            IndicatorState.Error => Color.FromRgb(0xF4, 0x43, 0x36),   // Red
            IndicatorState.Warning => Color.FromRgb(0xFF, 0xC1, 0x07), // Yellow
            IndicatorState.Info => Color.FromRgb(0x21, 0x96, 0xF3),    // Blue
            _ => Color.FromRgb(0x6E, 0x6E, 0x6E)                        // Gray (Off)
        };

        IndicatorColor = color;
        IndicatorBrush = new SolidColorBrush(color);
    }

    private void UpdateIndicatorColor()
    {
        var color = IsActive ? ActiveColor : InactiveColor;
        IndicatorColor = color;
        IndicatorBrush = new SolidColorBrush(color);
    }
}
