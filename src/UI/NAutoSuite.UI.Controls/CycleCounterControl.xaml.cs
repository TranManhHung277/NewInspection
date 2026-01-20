using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class CycleCounterControl : UserControl
{
    private static readonly Brush GoodYieldBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush MediumYieldBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));
    private static readonly Brush BadYieldBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(CycleCounterControl),
            new PropertyMetadata("Production Counter"));

    public static readonly DependencyProperty TotalCountProperty =
        DependencyProperty.Register(nameof(TotalCount), typeof(int), typeof(CycleCounterControl),
            new PropertyMetadata(0, OnCountChanged));

    public static readonly DependencyProperty OkCountProperty =
        DependencyProperty.Register(nameof(OkCount), typeof(int), typeof(CycleCounterControl),
            new PropertyMetadata(0, OnCountChanged));

    public static readonly DependencyProperty NgCountProperty =
        DependencyProperty.Register(nameof(NgCount), typeof(int), typeof(CycleCounterControl),
            new PropertyMetadata(0, OnCountChanged));

    public static readonly DependencyProperty YieldRateProperty =
        DependencyProperty.Register(nameof(YieldRate), typeof(double), typeof(CycleCounterControl),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty YieldBrushProperty =
        DependencyProperty.Register(nameof(YieldBrush), typeof(Brush), typeof(CycleCounterControl),
            new PropertyMetadata(GoodYieldBrush));

    public static readonly DependencyProperty YieldWidthProperty =
        DependencyProperty.Register(nameof(YieldWidth), typeof(double), typeof(CycleCounterControl),
            new PropertyMetadata(0.0));

    public static readonly DependencyProperty GoodYieldThresholdProperty =
        DependencyProperty.Register(nameof(GoodYieldThreshold), typeof(double), typeof(CycleCounterControl),
            new PropertyMetadata(95.0));

    public static readonly DependencyProperty MediumYieldThresholdProperty =
        DependencyProperty.Register(nameof(MediumYieldThreshold), typeof(double), typeof(CycleCounterControl),
            new PropertyMetadata(80.0));

    public static readonly DependencyProperty ShowResetButtonProperty =
        DependencyProperty.Register(nameof(ShowResetButton), typeof(bool), typeof(CycleCounterControl),
            new PropertyMetadata(true, OnShowResetButtonChanged));

    public static readonly DependencyProperty ResetButtonVisibilityProperty =
        DependencyProperty.Register(nameof(ResetButtonVisibility), typeof(Visibility), typeof(CycleCounterControl),
            new PropertyMetadata(Visibility.Visible));

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(CycleCounterControl),
            new PropertyMetadata(null));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public int TotalCount
    {
        get => (int)GetValue(TotalCountProperty);
        set => SetValue(TotalCountProperty, value);
    }

    public int OkCount
    {
        get => (int)GetValue(OkCountProperty);
        set => SetValue(OkCountProperty, value);
    }

    public int NgCount
    {
        get => (int)GetValue(NgCountProperty);
        set => SetValue(NgCountProperty, value);
    }

    public double YieldRate
    {
        get => (double)GetValue(YieldRateProperty);
        set => SetValue(YieldRateProperty, value);
    }

    public Brush YieldBrush
    {
        get => (Brush)GetValue(YieldBrushProperty);
        set => SetValue(YieldBrushProperty, value);
    }

    public double YieldWidth
    {
        get => (double)GetValue(YieldWidthProperty);
        set => SetValue(YieldWidthProperty, value);
    }

    public double GoodYieldThreshold
    {
        get => (double)GetValue(GoodYieldThresholdProperty);
        set => SetValue(GoodYieldThresholdProperty, value);
    }

    public double MediumYieldThreshold
    {
        get => (double)GetValue(MediumYieldThresholdProperty);
        set => SetValue(MediumYieldThresholdProperty, value);
    }

    public bool ShowResetButton
    {
        get => (bool)GetValue(ShowResetButtonProperty);
        set => SetValue(ShowResetButtonProperty, value);
    }

    public Visibility ResetButtonVisibility
    {
        get => (Visibility)GetValue(ResetButtonVisibilityProperty);
        set => SetValue(ResetButtonVisibilityProperty, value);
    }

    public ICommand? ResetCommand
    {
        get => (ICommand?)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public CycleCounterControl()
    {
        InitializeComponent();
        SizeChanged += (s, e) => UpdateYieldWidth();
    }

    private static void OnCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CycleCounterControl control)
        {
            control.UpdateYieldRate();
        }
    }

    private static void OnShowResetButtonChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is CycleCounterControl control)
        {
            control.ResetButtonVisibility = control.ShowResetButton ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateYieldRate()
    {
        if (TotalCount > 0)
        {
            YieldRate = (double)OkCount / TotalCount * 100;
        }
        else
        {
            YieldRate = 0;
        }

        YieldBrush = YieldRate >= GoodYieldThreshold ? GoodYieldBrush :
                     YieldRate >= MediumYieldThreshold ? MediumYieldBrush :
                     BadYieldBrush;

        UpdateYieldWidth();
    }

    private void UpdateYieldWidth()
    {
        var containerWidth = ActualWidth - 32; // Padding
        YieldWidth = Math.Max(0, containerWidth * YieldRate / 100);
    }
}
