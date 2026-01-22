using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NAutoSuite.UI.Controls;

public partial class AlarmIndicatorControl : UserControl
{
    public enum AlarmLevel
    {
        None,
        Info,
        Warning,
        Error,
        Critical
    }

    private static readonly Brush NoneBackground = new SolidColorBrush(Color.FromRgb(0x2D, 0x2D, 0x30));
    private static readonly Brush NoneBorder = new SolidColorBrush(Color.FromRgb(0x3E, 0x3E, 0x42));
    private static readonly Brush NoneIcon = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));

    private static readonly Brush InfoBackground = new SolidColorBrush(Color.FromRgb(0x1A, 0x3A, 0x5C));
    private static readonly Brush InfoBorder = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));
    private static readonly Brush InfoIcon = new SolidColorBrush(Color.FromRgb(0x21, 0x96, 0xF3));

    private static readonly Brush WarningBackground = new SolidColorBrush(Color.FromRgb(0x4D, 0x3D, 0x1A));
    private static readonly Brush WarningBorder = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));
    private static readonly Brush WarningIcon = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));

    private static readonly Brush ErrorBackground = new SolidColorBrush(Color.FromRgb(0x4D, 0x1A, 0x1A));
    private static readonly Brush ErrorBorder = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly Brush ErrorIcon = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));

    private static readonly Brush CriticalBackground = new SolidColorBrush(Color.FromRgb(0x6D, 0x1A, 0x1A));
    private static readonly Brush CriticalBorder = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));
    private static readonly Brush CriticalIcon = new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28));

    public static readonly DependencyProperty LevelProperty =
        DependencyProperty.Register(nameof(Level), typeof(AlarmLevel), typeof(AlarmIndicatorControl),
            new PropertyMetadata(AlarmLevel.None, OnLevelChanged));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(AlarmIndicatorControl),
            new PropertyMetadata("No Alarm"));

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(AlarmIndicatorControl),
            new PropertyMetadata(string.Empty, OnMessageChanged));

    public static readonly DependencyProperty ShowAcknowledgeButtonProperty =
        DependencyProperty.Register(nameof(ShowAcknowledgeButton), typeof(bool), typeof(AlarmIndicatorControl),
            new PropertyMetadata(true, OnShowAckChanged));

    public static readonly DependencyProperty EnableBlinkingProperty =
        DependencyProperty.Register(nameof(EnableBlinking), typeof(bool), typeof(AlarmIndicatorControl),
            new PropertyMetadata(true, OnBlinkingChanged));

    public static readonly DependencyProperty AcknowledgeCommandProperty =
        DependencyProperty.Register(nameof(AcknowledgeCommand), typeof(ICommand), typeof(AlarmIndicatorControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty BackgroundBrushProperty =
        DependencyProperty.Register(nameof(BackgroundBrush), typeof(Brush), typeof(AlarmIndicatorControl),
            new PropertyMetadata(NoneBackground));

    public static new readonly DependencyProperty BorderBrushProperty =
        DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(AlarmIndicatorControl),
            new PropertyMetadata(NoneBorder));

    public static readonly DependencyProperty IconBackgroundBrushProperty =
        DependencyProperty.Register(nameof(IconBackgroundBrush), typeof(Brush), typeof(AlarmIndicatorControl),
            new PropertyMetadata(NoneIcon));

    public static readonly DependencyProperty IconForegroundProperty =
        DependencyProperty.Register(nameof(IconForeground), typeof(Brush), typeof(AlarmIndicatorControl),
            new PropertyMetadata(NoneIcon));

    public static readonly DependencyProperty IconTextProperty =
        DependencyProperty.Register(nameof(IconText), typeof(string), typeof(AlarmIndicatorControl),
            new PropertyMetadata("\uE946")); // Info icon

    public static readonly DependencyProperty MessageVisibilityProperty =
        DependencyProperty.Register(nameof(MessageVisibility), typeof(Visibility), typeof(AlarmIndicatorControl),
            new PropertyMetadata(Visibility.Collapsed));

    public static readonly DependencyProperty AckButtonVisibilityProperty =
        DependencyProperty.Register(nameof(AckButtonVisibility), typeof(Visibility), typeof(AlarmIndicatorControl),
            new PropertyMetadata(Visibility.Collapsed));

    public AlarmLevel Level
    {
        get => (AlarmLevel)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public bool ShowAcknowledgeButton
    {
        get => (bool)GetValue(ShowAcknowledgeButtonProperty);
        set => SetValue(ShowAcknowledgeButtonProperty, value);
    }

    public bool EnableBlinking
    {
        get => (bool)GetValue(EnableBlinkingProperty);
        set => SetValue(EnableBlinkingProperty, value);
    }

    public ICommand? AcknowledgeCommand
    {
        get => (ICommand?)GetValue(AcknowledgeCommandProperty);
        set => SetValue(AcknowledgeCommandProperty, value);
    }

    public Brush BackgroundBrush
    {
        get => (Brush)GetValue(BackgroundBrushProperty);
        set => SetValue(BackgroundBrushProperty, value);
    }

    public new Brush BorderBrush
    {
        get => (Brush)GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    public Brush IconBackgroundBrush
    {
        get => (Brush)GetValue(IconBackgroundBrushProperty);
        set => SetValue(IconBackgroundBrushProperty, value);
    }

    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    public string IconText
    {
        get => (string)GetValue(IconTextProperty);
        set => SetValue(IconTextProperty, value);
    }

    public Visibility MessageVisibility
    {
        get => (Visibility)GetValue(MessageVisibilityProperty);
        set => SetValue(MessageVisibilityProperty, value);
    }

    public Visibility AckButtonVisibility
    {
        get => (Visibility)GetValue(AckButtonVisibilityProperty);
        set => SetValue(AckButtonVisibilityProperty, value);
    }

    public AlarmIndicatorControl()
    {
        InitializeComponent();
        UpdateAppearance();
    }

    private static void OnLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AlarmIndicatorControl control)
        {
            control.UpdateAppearance();
        }
    }

    private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AlarmIndicatorControl control)
        {
            control.MessageVisibility = string.IsNullOrEmpty(control.Message) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private static void OnShowAckChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AlarmIndicatorControl control)
        {
            control.UpdateAckButtonVisibility();
        }
    }

    private static void OnBlinkingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AlarmIndicatorControl control)
        {
            control.UpdateBlinking();
        }
    }

    private void UpdateAppearance()
    {
        switch (Level)
        {
            case AlarmLevel.Info:
                BackgroundBrush = InfoBackground;
                BorderBrush = InfoBorder;
                IconBackgroundBrush = InfoIcon;
                IconForeground = InfoIcon;
                IconText = "\uE946"; // Info
                break;
            case AlarmLevel.Warning:
                BackgroundBrush = WarningBackground;
                BorderBrush = WarningBorder;
                IconBackgroundBrush = WarningIcon;
                IconForeground = WarningIcon;
                IconText = "\uE7BA"; // Warning
                break;
            case AlarmLevel.Error:
                BackgroundBrush = ErrorBackground;
                BorderBrush = ErrorBorder;
                IconBackgroundBrush = ErrorIcon;
                IconForeground = ErrorIcon;
                IconText = "\uEA39"; // Error
                break;
            case AlarmLevel.Critical:
                BackgroundBrush = CriticalBackground;
                BorderBrush = CriticalBorder;
                IconBackgroundBrush = CriticalIcon;
                IconForeground = CriticalIcon;
                IconText = "\uEB90"; // Critical
                break;
            default:
                BackgroundBrush = NoneBackground;
                BorderBrush = NoneBorder;
                IconBackgroundBrush = NoneIcon;
                IconForeground = NoneIcon;
                IconText = "\uE930"; // Checkmark
                break;
        }

        UpdateAckButtonVisibility();
        UpdateBlinking();
    }

    private void UpdateAckButtonVisibility()
    {
        AckButtonVisibility = ShowAcknowledgeButton && Level != AlarmLevel.None
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void UpdateBlinking()
    {
        var storyboard = (Storyboard)Resources["BlinkAnimation"];

        if (EnableBlinking && (Level == AlarmLevel.Error || Level == AlarmLevel.Critical))
        {
            storyboard.Begin();
        }
        else
        {
            storyboard.Stop();
        }
    }
}
