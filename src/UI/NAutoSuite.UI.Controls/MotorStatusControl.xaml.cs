using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NAutoSuite.UI.Controls;

public partial class MotorStatusControl : UserControl
{
    public enum MotorState
    {
        Stopped,
        Running,
        Error,
        Warning
    }

    private static readonly Brush StoppedBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
    private static readonly Brush RunningBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly Brush WarningBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));

    public static readonly DependencyProperty MotorNameProperty =
        DependencyProperty.Register(nameof(MotorName), typeof(string), typeof(MotorStatusControl),
            new PropertyMetadata("Motor"));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(MotorState), typeof(MotorStatusControl),
            new PropertyMetadata(MotorState.Stopped, OnStateChanged));

    public static readonly DependencyProperty SpeedProperty =
        DependencyProperty.Register(nameof(Speed), typeof(double), typeof(MotorStatusControl),
            new PropertyMetadata(0.0, OnSpeedChanged));

    public static readonly DependencyProperty ShowSpeedProperty =
        DependencyProperty.Register(nameof(ShowSpeed), typeof(bool), typeof(MotorStatusControl),
            new PropertyMetadata(true, OnShowSpeedChanged));

    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(nameof(StatusBrush), typeof(Brush), typeof(MotorStatusControl),
            new PropertyMetadata(StoppedBrush));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(MotorStatusControl),
            new PropertyMetadata("STOPPED"));

    public static readonly DependencyProperty SpeedVisibilityProperty =
        DependencyProperty.Register(nameof(SpeedVisibility), typeof(Visibility), typeof(MotorStatusControl),
            new PropertyMetadata(Visibility.Visible));

    public string MotorName
    {
        get => (string)GetValue(MotorNameProperty);
        set => SetValue(MotorNameProperty, value);
    }

    public MotorState State
    {
        get => (MotorState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public double Speed
    {
        get => (double)GetValue(SpeedProperty);
        set => SetValue(SpeedProperty, value);
    }

    public bool ShowSpeed
    {
        get => (bool)GetValue(ShowSpeedProperty);
        set => SetValue(ShowSpeedProperty, value);
    }

    public Brush StatusBrush
    {
        get => (Brush)GetValue(StatusBrushProperty);
        set => SetValue(StatusBrushProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public Visibility SpeedVisibility
    {
        get => (Visibility)GetValue(SpeedVisibilityProperty);
        set => SetValue(SpeedVisibilityProperty, value);
    }

    public MotorStatusControl()
    {
        InitializeComponent();
        UpdateStatus();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MotorStatusControl control)
        {
            control.UpdateStatus();
        }
    }

    private static void OnSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MotorStatusControl control)
        {
            control.UpdateAnimation();
        }
    }

    private static void OnShowSpeedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MotorStatusControl control)
        {
            control.SpeedVisibility = control.ShowSpeed ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateStatus()
    {
        switch (State)
        {
            case MotorState.Running:
                StatusBrush = RunningBrush;
                StatusText = "RUNNING";
                break;
            case MotorState.Error:
                StatusBrush = ErrorBrush;
                StatusText = "ERROR";
                break;
            case MotorState.Warning:
                StatusBrush = WarningBrush;
                StatusText = "WARNING";
                break;
            default:
                StatusBrush = StoppedBrush;
                StatusText = "STOPPED";
                break;
        }

        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        var storyboard = (Storyboard)Resources["RotateAnimation"];

        if (State == MotorState.Running && Speed > 0)
        {
            // Adjust animation speed based on motor speed
            var duration = Speed > 0 ? 60.0 / Speed : 1.0; // Convert RPM to seconds per rotation
            duration = System.Math.Max(0.1, System.Math.Min(5.0, duration)); // Clamp between 0.1 and 5 seconds

            var animation = (DoubleAnimation)storyboard.Children[0];
            animation.Duration = new Duration(System.TimeSpan.FromSeconds(duration));

            storyboard.Begin();
        }
        else
        {
            storyboard.Stop();
        }
    }
}
