using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class AxisPositionControl : UserControl
{
    private static readonly Brush DefaultPositionBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly Brush InactiveBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
    private static readonly Brush HomeBrushActive = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush AlarmBrushActive = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));

    public static readonly DependencyProperty AxisNameProperty =
        DependencyProperty.Register(nameof(AxisName), typeof(string), typeof(AxisPositionControl),
            new PropertyMetadata("Axis"));

    public static readonly DependencyProperty PositionProperty =
        DependencyProperty.Register(nameof(Position), typeof(double), typeof(AxisPositionControl),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(AxisPositionControl),
            new PropertyMetadata("mm"));

    public static readonly DependencyProperty IsHomedProperty =
        DependencyProperty.Register(nameof(IsHomed), typeof(bool), typeof(AxisPositionControl),
            new PropertyMetadata(false, OnStatusChanged));

    public static readonly DependencyProperty HasAlarmProperty =
        DependencyProperty.Register(nameof(HasAlarm), typeof(bool), typeof(AxisPositionControl),
            new PropertyMetadata(false, OnStatusChanged));

    public static readonly DependencyProperty PositionForegroundProperty =
        DependencyProperty.Register(nameof(PositionForeground), typeof(Brush), typeof(AxisPositionControl),
            new PropertyMetadata(DefaultPositionBrush));

    public static readonly DependencyProperty HomeBrushProperty =
        DependencyProperty.Register(nameof(HomeBrush), typeof(Brush), typeof(AxisPositionControl),
            new PropertyMetadata(InactiveBrush));

    public static readonly DependencyProperty AlarmBrushProperty =
        DependencyProperty.Register(nameof(AlarmBrush), typeof(Brush), typeof(AxisPositionControl),
            new PropertyMetadata(InactiveBrush));

    public string AxisName
    {
        get => (string)GetValue(AxisNameProperty);
        set => SetValue(AxisNameProperty, value);
    }

    public double Position
    {
        get => (double)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public bool IsHomed
    {
        get => (bool)GetValue(IsHomedProperty);
        set => SetValue(IsHomedProperty, value);
    }

    public bool HasAlarm
    {
        get => (bool)GetValue(HasAlarmProperty);
        set => SetValue(HasAlarmProperty, value);
    }

    public Brush PositionForeground
    {
        get => (Brush)GetValue(PositionForegroundProperty);
        set => SetValue(PositionForegroundProperty, value);
    }

    public Brush HomeBrush
    {
        get => (Brush)GetValue(HomeBrushProperty);
        set => SetValue(HomeBrushProperty, value);
    }

    public Brush AlarmBrush
    {
        get => (Brush)GetValue(AlarmBrushProperty);
        set => SetValue(AlarmBrushProperty, value);
    }

    public AxisPositionControl()
    {
        InitializeComponent();
    }

    private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AxisPositionControl control)
        {
            control.UpdateStatusIndicators();
        }
    }

    private void UpdateStatusIndicators()
    {
        HomeBrush = IsHomed ? HomeBrushActive : InactiveBrush;
        AlarmBrush = HasAlarm ? AlarmBrushActive : InactiveBrush;
    }
}
