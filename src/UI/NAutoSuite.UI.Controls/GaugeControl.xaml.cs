using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NAutoSuite.UI.Controls;

public partial class GaugeControl : UserControl
{
    private static readonly Brush DefaultValueBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly Brush DefaultNeedleBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly Color DefaultValueColor = Color.FromRgb(0x21, 0x96, 0xF3);

    private const double StartAngle = 135; // Start angle in degrees (bottom left)
    private const double EndAngle = 405;   // End angle in degrees (bottom right, going clockwise)
    private const double TotalAngle = 270; // Total arc angle

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(0.0, OnRangeChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(100.0, OnRangeChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(GaugeControl),
            new PropertyMetadata(string.Empty, OnUnitChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(GaugeControl),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(150.0, OnSizeChanged));

    public static readonly DependencyProperty ArcThicknessProperty =
        DependencyProperty.Register(nameof(ArcThickness), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(10.0, OnValueChanged));

    public static readonly DependencyProperty ShowNeedleProperty =
        DependencyProperty.Register(nameof(ShowNeedle), typeof(bool), typeof(GaugeControl),
            new PropertyMetadata(false, OnShowNeedleChanged));

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(nameof(Format), typeof(string), typeof(GaugeControl),
            new PropertyMetadata("F0", OnValueChanged));

    public static readonly DependencyProperty ValueBrushProperty =
        DependencyProperty.Register(nameof(ValueBrush), typeof(Brush), typeof(GaugeControl),
            new PropertyMetadata(DefaultValueBrush));

    public static readonly DependencyProperty ValueColorProperty =
        DependencyProperty.Register(nameof(ValueColor), typeof(Color), typeof(GaugeControl),
            new PropertyMetadata(DefaultValueColor));

    public static readonly DependencyProperty DisplayValueProperty =
        DependencyProperty.Register(nameof(DisplayValue), typeof(string), typeof(GaugeControl),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty BackgroundArcDataProperty =
        DependencyProperty.Register(nameof(BackgroundArcData), typeof(Geometry), typeof(GaugeControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty ValueArcDataProperty =
        DependencyProperty.Register(nameof(ValueArcData), typeof(Geometry), typeof(GaugeControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty NeedleDataProperty =
        DependencyProperty.Register(nameof(NeedleData), typeof(Geometry), typeof(GaugeControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty NeedleAngleProperty =
        DependencyProperty.Register(nameof(NeedleAngle), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty NeedleBrushProperty =
        DependencyProperty.Register(nameof(NeedleBrush), typeof(Brush), typeof(GaugeControl),
            new PropertyMetadata(DefaultNeedleBrush));

    public static readonly DependencyProperty NeedleVisibilityProperty =
        DependencyProperty.Register(nameof(NeedleVisibility), typeof(Visibility), typeof(GaugeControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty UnitVisibilityProperty =
        DependencyProperty.Register(nameof(UnitVisibility), typeof(Visibility), typeof(GaugeControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty LabelVisibilityProperty =
        DependencyProperty.Register(nameof(LabelVisibility), typeof(Visibility), typeof(GaugeControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty ValueFontSizeProperty =
        DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(GaugeControl),
            new PropertyMetadata(24.0));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
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

    public string Unit
    {
        get => (string)GetValue(UnitProperty);
        set => SetValue(UnitProperty, value);
    }

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public double ArcThickness
    {
        get => (double)GetValue(ArcThicknessProperty);
        set => SetValue(ArcThicknessProperty, value);
    }

    public bool ShowNeedle
    {
        get => (bool)GetValue(ShowNeedleProperty);
        set => SetValue(ShowNeedleProperty, value);
    }

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public Brush ValueBrush
    {
        get => (Brush)GetValue(ValueBrushProperty);
        set => SetValue(ValueBrushProperty, value);
    }

    public Color ValueColor
    {
        get => (Color)GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    public string DisplayValue
    {
        get => (string)GetValue(DisplayValueProperty);
        set => SetValue(DisplayValueProperty, value);
    }

    public Geometry BackgroundArcData
    {
        get => (Geometry)GetValue(BackgroundArcDataProperty);
        set => SetValue(BackgroundArcDataProperty, value);
    }

    public Geometry ValueArcData
    {
        get => (Geometry)GetValue(ValueArcDataProperty);
        set => SetValue(ValueArcDataProperty, value);
    }

    public Geometry NeedleData
    {
        get => (Geometry)GetValue(NeedleDataProperty);
        set => SetValue(NeedleDataProperty, value);
    }

    public double NeedleAngle
    {
        get => (double)GetValue(NeedleAngleProperty);
        set => SetValue(NeedleAngleProperty, value);
    }

    public Brush NeedleBrush
    {
        get => (Brush)GetValue(NeedleBrushProperty);
        set => SetValue(NeedleBrushProperty, value);
    }

    public Visibility NeedleVisibility
    {
        get => (Visibility)GetValue(NeedleVisibilityProperty);
        set => SetValue(NeedleVisibilityProperty, value);
    }

    public Visibility UnitVisibility
    {
        get => (Visibility)GetValue(UnitVisibilityProperty);
        set => SetValue(UnitVisibilityProperty, value);
    }

    public Visibility LabelVisibility
    {
        get => (Visibility)GetValue(LabelVisibilityProperty);
        set => SetValue(LabelVisibilityProperty, value);
    }

    public double ValueFontSize
    {
        get => (double)GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
    }

    public GaugeControl()
    {
        InitializeComponent();
        Loaded += (s, e) => UpdateGauge();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeControl control)
        {
            control.UpdateGauge();
        }
    }

    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeControl control)
        {
            control.UpdateGauge();
        }
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeControl control)
        {
            control.ValueFontSize = control.Size / 6;
            control.UpdateGauge();
        }
    }

    private static void OnUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeControl control)
        {
            control.UnitVisibility = string.IsNullOrEmpty(control.Unit) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeControl control)
        {
            control.LabelVisibility = string.IsNullOrEmpty(control.Label) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnShowNeedleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is GaugeControl control)
        {
            control.NeedleVisibility = control.ShowNeedle ? Visibility.Visible : Visibility.Collapsed;
            control.UpdateGauge();
        }
    }

    private void UpdateGauge()
    {
        var center = Size / 2;
        var radius = (Size - ArcThickness) / 2 - 5;

        // Update display value
        try
        {
            DisplayValue = Value.ToString(Format, CultureInfo.CurrentCulture);
        }
        catch
        {
            DisplayValue = Value.ToString(CultureInfo.CurrentCulture);
        }

        // Calculate percentage
        var range = Maximum - Minimum;
        var percentage = range > 0 ? Math.Max(0, Math.Min(1, (Value - Minimum) / range)) : 0;

        // Create background arc (full arc)
        BackgroundArcData = CreateArc(center, radius, StartAngle, EndAngle - 0.01);

        // Create value arc
        var valueEndAngle = StartAngle + (TotalAngle * percentage);
        if (percentage > 0)
        {
            ValueArcData = CreateArc(center, radius, StartAngle, valueEndAngle);
        }
        else
        {
            ValueArcData = Geometry.Empty;
        }

        // Update needle
        if (ShowNeedle)
        {
            NeedleAngle = StartAngle - 90 + (TotalAngle * percentage);
            UpdateNeedleGeometry(center, radius);
            UpdateCenterCirclePosition(center);
        }
    }

    private Geometry CreateArc(double center, double radius, double startAngle, double endAngle)
    {
        var startRad = startAngle * Math.PI / 180;
        var endRad = endAngle * Math.PI / 180;

        var startX = center + radius * Math.Cos(startRad);
        var startY = center + radius * Math.Sin(startRad);
        var endX = center + radius * Math.Cos(endRad);
        var endY = center + radius * Math.Sin(endRad);

        var angleDiff = endAngle - startAngle;
        var largeArc = angleDiff > 180 ? 1 : 0;

        var pathData = string.Format(CultureInfo.InvariantCulture,
            "M {0},{1} A {2},{2} 0 {3} 1 {4},{5}",
            startX, startY, radius, largeArc, endX, endY);

        return Geometry.Parse(pathData);
    }

    private void UpdateNeedleGeometry(double center, double radius)
    {
        var needleLength = radius - 10;
        var needleWidth = 6;

        var pathData = string.Format(CultureInfo.InvariantCulture,
            "M {0},{1} L {2},{3} L {4},{5} Z",
            center, center - needleLength,          // Top point
            center - needleWidth / 2, center + 5,   // Bottom left
            center + needleWidth / 2, center + 5);  // Bottom right

        NeedleData = Geometry.Parse(pathData);
    }

    private void UpdateCenterCirclePosition(double center)
    {
        Canvas.SetLeft(CenterCircle, center - 6);
        Canvas.SetTop(CenterCircle, center - 6);
    }
}
