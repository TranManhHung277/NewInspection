using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class StatisticCardControl : UserControl
{
    public enum TrendDirection
    {
        None,
        Up,
        Down
    }

    private static readonly Brush DefaultValueBrush = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly Brush UpTrendBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush DownTrendBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly Brush NeutralTrendBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
    private static readonly Brush DefaultIconBackground = new SolidColorBrush(Color.FromArgb(0x33, 0x21, 0x96, 0xF3));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata("Statistic"));

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(StatisticCardControl),
            new PropertyMetadata(0.0, OnValueChanged));

    public static readonly DependencyProperty UnitProperty =
        DependencyProperty.Register(nameof(Unit), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata(string.Empty, OnUnitChanged));

    public static readonly DependencyProperty FormatProperty =
        DependencyProperty.Register(nameof(Format), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata("N0", OnValueChanged));

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata(string.Empty, OnIconChanged));

    public static readonly DependencyProperty TrendProperty =
        DependencyProperty.Register(nameof(Trend), typeof(TrendDirection), typeof(StatisticCardControl),
            new PropertyMetadata(TrendDirection.None, OnTrendChanged));

    public static readonly DependencyProperty TrendValueProperty =
        DependencyProperty.Register(nameof(TrendValue), typeof(double), typeof(StatisticCardControl),
            new PropertyMetadata(0.0, OnTrendChanged));

    public static readonly DependencyProperty TrendDescriptionProperty =
        DependencyProperty.Register(nameof(TrendDescription), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata("vs last period"));

    public static readonly DependencyProperty IsPositiveTrendGoodProperty =
        DependencyProperty.Register(nameof(IsPositiveTrendGood), typeof(bool), typeof(StatisticCardControl),
            new PropertyMetadata(true, OnTrendChanged));

    public static readonly DependencyProperty ValueFontSizeProperty =
        DependencyProperty.Register(nameof(ValueFontSize), typeof(double), typeof(StatisticCardControl),
            new PropertyMetadata(28.0));

    public static readonly DependencyProperty ValueForegroundProperty =
        DependencyProperty.Register(nameof(ValueForeground), typeof(Brush), typeof(StatisticCardControl),
            new PropertyMetadata(DefaultValueBrush));

    public static readonly DependencyProperty DisplayValueProperty =
        DependencyProperty.Register(nameof(DisplayValue), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata("0"));

    public static readonly DependencyProperty IconVisibilityProperty =
        DependencyProperty.Register(nameof(IconVisibility), typeof(Visibility), typeof(StatisticCardControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty IconBackgroundProperty =
        DependencyProperty.Register(nameof(IconBackground), typeof(Brush), typeof(StatisticCardControl),
            new PropertyMetadata(DefaultIconBackground));

    public static readonly DependencyProperty IconForegroundProperty =
        DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(StatisticCardControl),
            new PropertyMetadata(DefaultValueBrush));

    public static readonly DependencyProperty UnitVisibilityProperty =
        DependencyProperty.Register(nameof(UnitVisibility), typeof(Visibility), typeof(StatisticCardControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty TrendVisibilityProperty =
        DependencyProperty.Register(nameof(TrendVisibility), typeof(Visibility), typeof(StatisticCardControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty TrendIconProperty =
        DependencyProperty.Register(nameof(TrendIcon), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TrendTextProperty =
        DependencyProperty.Register(nameof(TrendText), typeof(string), typeof(StatisticCardControl),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TrendBrushProperty =
        DependencyProperty.Register(nameof(TrendBrush), typeof(Brush), typeof(StatisticCardControl),
            new PropertyMetadata(NeutralTrendBrush));

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

    public string Format
    {
        get => (string)GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public TrendDirection Trend
    {
        get => (TrendDirection)GetValue(TrendProperty);
        set => SetValue(TrendProperty, value);
    }

    public double TrendValue
    {
        get => (double)GetValue(TrendValueProperty);
        set => SetValue(TrendValueProperty, value);
    }

    public string TrendDescription
    {
        get => (string)GetValue(TrendDescriptionProperty);
        set => SetValue(TrendDescriptionProperty, value);
    }

    public bool IsPositiveTrendGood
    {
        get => (bool)GetValue(IsPositiveTrendGoodProperty);
        set => SetValue(IsPositiveTrendGoodProperty, value);
    }

    public double ValueFontSize
    {
        get => (double)GetValue(ValueFontSizeProperty);
        set => SetValue(ValueFontSizeProperty, value);
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

    public Visibility IconVisibility
    {
        get => (Visibility)GetValue(IconVisibilityProperty);
        set => SetValue(IconVisibilityProperty, value);
    }

    public Brush IconBackground
    {
        get => (Brush)GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    public Visibility UnitVisibility
    {
        get => (Visibility)GetValue(UnitVisibilityProperty);
        set => SetValue(UnitVisibilityProperty, value);
    }

    public Visibility TrendVisibility
    {
        get => (Visibility)GetValue(TrendVisibilityProperty);
        set => SetValue(TrendVisibilityProperty, value);
    }

    public string TrendIcon
    {
        get => (string)GetValue(TrendIconProperty);
        set => SetValue(TrendIconProperty, value);
    }

    public string TrendText
    {
        get => (string)GetValue(TrendTextProperty);
        set => SetValue(TrendTextProperty, value);
    }

    public Brush TrendBrush
    {
        get => (Brush)GetValue(TrendBrushProperty);
        set => SetValue(TrendBrushProperty, value);
    }

    public StatisticCardControl()
    {
        InitializeComponent();
        UpdateDisplayValue();
    }

    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatisticCardControl control)
        {
            control.UpdateDisplayValue();
        }
    }

    private static void OnUnitChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatisticCardControl control)
        {
            control.UnitVisibility = string.IsNullOrEmpty(control.Unit) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatisticCardControl control)
        {
            control.IconVisibility = string.IsNullOrEmpty(control.Icon) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnTrendChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatisticCardControl control)
        {
            control.UpdateTrend();
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

    private void UpdateTrend()
    {
        if (Trend == TrendDirection.None)
        {
            TrendVisibility = Visibility.Collapsed;
            return;
        }

        TrendVisibility = Visibility.Visible;

        var isUp = Trend == TrendDirection.Up;
        TrendIcon = isUp ? "\uE74A" : "\uE74B"; // Up/Down arrows
        TrendText = $"{Math.Abs(TrendValue):F1}%";

        // Determine if trend is good or bad
        var isGood = (isUp && IsPositiveTrendGood) || (!isUp && !IsPositiveTrendGood);
        TrendBrush = isGood ? UpTrendBrush : DownTrendBrush;
    }
}
