using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class ProgressIndicatorControl : UserControl
{
    public enum ProgressMode
    {
        Circular,
        Linear
    }

    private static readonly Brush DefaultProgressBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));

    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(ProgressMode), typeof(ProgressIndicatorControl),
            new PropertyMetadata(ProgressMode.Circular, OnModeChanged));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(nameof(Maximum), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(100.0, OnValueChanged));

    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(nameof(Minimum), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(nameof(Label), typeof(string), typeof(ProgressIndicatorControl),
            new PropertyMetadata(string.Empty, OnLabelChanged));

    public static readonly DependencyProperty SizeProperty =
        DependencyProperty.Register(nameof(Size), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(100.0, OnSizeChanged));

    public static readonly DependencyProperty StrokeWidthProperty =
        DependencyProperty.Register(nameof(StrokeWidth), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(8.0, OnValueChanged));

    public static readonly DependencyProperty BarHeightProperty =
        DependencyProperty.Register(nameof(BarHeight), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(8.0));

    public static readonly DependencyProperty ProgressBrushProperty =
        DependencyProperty.Register(nameof(ProgressBrush), typeof(Brush), typeof(ProgressIndicatorControl),
            new PropertyMetadata(DefaultProgressBrush));

    public static readonly DependencyProperty ShowPercentageProperty =
        DependencyProperty.Register(nameof(ShowPercentage), typeof(bool), typeof(ProgressIndicatorControl),
            new PropertyMetadata(true));

    public static readonly DependencyProperty PercentageTextProperty =
        DependencyProperty.Register(nameof(PercentageText), typeof(string), typeof(ProgressIndicatorControl),
            new PropertyMetadata("0%"));

    public static readonly DependencyProperty ArcDataProperty =
        DependencyProperty.Register(nameof(ArcData), typeof(Geometry), typeof(ProgressIndicatorControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CircularVisibilityProperty =
        DependencyProperty.Register(nameof(CircularVisibility), typeof(Visibility), typeof(ProgressIndicatorControl),
            new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty LinearVisibilityProperty =
        DependencyProperty.Register(nameof(LinearVisibility), typeof(Visibility), typeof(ProgressIndicatorControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty LabelVisibilityProperty =
        DependencyProperty.Register(nameof(LabelVisibility), typeof(Visibility), typeof(ProgressIndicatorControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty ValueFontSizeProperty =
        DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(20.0));

    public static readonly DependencyProperty ProgressWidthProperty =
        DependencyProperty.Register(nameof(ProgressWidth), typeof(double), typeof(ProgressIndicatorControl),
            new PropertyMetadata(0.0));

    public ProgressMode Mode
    {
        get => (ProgressMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
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

    public double StrokeWidth
    {
        get => (double)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    public Brush ProgressBrush
    {
        get => (Brush)GetValue(ProgressBrushProperty);
        set => SetValue(ProgressBrushProperty, value);
    }

    public bool ShowPercentage
    {
        get => (bool)GetValue(ShowPercentageProperty);
        set => SetValue(ShowPercentageProperty, value);
    }

    public string PercentageText
    {
        get => (string)GetValue(PercentageTextProperty);
        set => SetValue(PercentageTextProperty, value);
    }

    public Geometry ArcData
    {
        get => (Geometry)GetValue(ArcDataProperty);
        set => SetValue(ArcDataProperty, value);
    }

    public Visibility CircularVisibility
    {
        get => (Visibility)GetValue(CircularVisibilityProperty);
        set => SetValue(CircularVisibilityProperty, value);
    }

    public Visibility LinearVisibility
    {
        get => (Visibility)GetValue(LinearVisibilityProperty);
        set => SetValue(LinearVisibilityProperty, value);
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

    public double ProgressWidth
    {
        get => (double)GetValue(ProgressWidthProperty);
        set => SetValue(ProgressWidthProperty, value);
    }

    public ProgressIndicatorControl()
    {
        InitializeComponent();
        UpdateProgress();
        SizeChanged += (s, e) => UpdateProgress();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressIndicatorControl control)
        {
            control.UpdateProgress();
        }
    }

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressIndicatorControl control)
        {
            control.CircularVisibility = control.Mode == ProgressMode.Circular ? Visibility.Visible : Visibility.Collapsed;
            control.LinearVisibility = control.Mode == ProgressMode.Linear ? Visibility.Visible : Visibility.Collapsed;
            control.UpdateProgress();
        }
    }

    private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressIndicatorControl control)
        {
            control.ValueFontSize = control.Size / 5;
            control.UpdateProgress();
        }
    }

    private static void OnLabelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ProgressIndicatorControl control)
        {
            control.LabelVisibility = string.IsNullOrEmpty(control.Label) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void UpdateProgress()
    {
        var range = Maximum - Minimum;
        var percentage = range > 0 ? (Value - Minimum) / range * 100 : 0;
        percentage = Math.Max(0, Math.Min(100, percentage));

        PercentageText = $"{percentage:F0}%";

        if (Mode == ProgressMode.Circular)
        {
            UpdateCircularProgress(percentage);
        }
        else
        {
            UpdateLinearProgress(percentage);
        }
    }

    private void UpdateCircularProgress(double percentage)
    {
        var radius = (Size - StrokeWidth) / 2;
        var center = Size / 2;
        var angle = percentage / 100 * 360;

        if (angle <= 0)
        {
            ArcData = Geometry.Empty;
            return;
        }

        if (angle >= 360)
        {
            angle = 359.99;
        }

        var startAngle = -90;
        var endAngle = startAngle + angle;

        var startX = center + radius * Math.Cos(startAngle * Math.PI / 180);
        var startY = center + radius * Math.Sin(startAngle * Math.PI / 180);
        var endX = center + radius * Math.Cos(endAngle * Math.PI / 180);
        var endY = center + radius * Math.Sin(endAngle * Math.PI / 180);

        var largeArc = angle > 180 ? 1 : 0;

        var pathData = string.Format(CultureInfo.InvariantCulture,
            "M {0},{1} A {2},{2} 0 {3} 1 {4},{5}",
            startX, startY, radius, largeArc, endX, endY);

        ArcData = Geometry.Parse(pathData);
    }

    private void UpdateLinearProgress(double percentage)
    {
        ProgressWidth = ActualWidth * percentage / 100;
    }
}
