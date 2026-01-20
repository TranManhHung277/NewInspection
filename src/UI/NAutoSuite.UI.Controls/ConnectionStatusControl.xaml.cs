using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace NAutoSuite.UI.Controls;

public partial class ConnectionStatusControl : UserControl
{
    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Error
    }

    private static readonly Brush DisconnectedBrush = new SolidColorBrush(Color.FromRgb(0x6E, 0x6E, 0x6E));
    private static readonly Brush ConnectingBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));
    private static readonly Brush ConnectedBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));
    private static readonly Brush GoodLatencyBrush = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50));
    private static readonly Brush MediumLatencyBrush = new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07));
    private static readonly Brush BadLatencyBrush = new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36));

    private static readonly Color DisconnectedColor = Color.FromRgb(0x6E, 0x6E, 0x6E);
    private static readonly Color ConnectingColor = Color.FromRgb(0xFF, 0xC1, 0x07);
    private static readonly Color ConnectedColor = Color.FromRgb(0x4C, 0xAF, 0x50);
    private static readonly Color ErrorColor = Color.FromRgb(0xF4, 0x43, 0x36);

    public static readonly DependencyProperty DeviceNameProperty =
        DependencyProperty.Register(nameof(DeviceName), typeof(string), typeof(ConnectionStatusControl),
            new PropertyMetadata("Device"));

    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(ConnectionState), typeof(ConnectionStatusControl),
            new PropertyMetadata(ConnectionState.Disconnected, OnStateChanged));

    public static readonly DependencyProperty LatencyProperty =
        DependencyProperty.Register(nameof(Latency), typeof(int), typeof(ConnectionStatusControl),
            new PropertyMetadata(0, OnLatencyChanged));

    public static readonly DependencyProperty ShowLatencyProperty =
        DependencyProperty.Register(nameof(ShowLatency), typeof(bool), typeof(ConnectionStatusControl),
            new PropertyMetadata(true, OnShowLatencyChanged));

    public static readonly DependencyProperty GoodLatencyThresholdProperty =
        DependencyProperty.Register(nameof(GoodLatencyThreshold), typeof(int), typeof(ConnectionStatusControl),
            new PropertyMetadata(50));

    public static readonly DependencyProperty MediumLatencyThresholdProperty =
        DependencyProperty.Register(nameof(MediumLatencyThreshold), typeof(int), typeof(ConnectionStatusControl),
            new PropertyMetadata(150));

    public static readonly DependencyProperty StatusTextProperty =
        DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(ConnectionStatusControl),
            new PropertyMetadata("Disconnected"));

    public static readonly DependencyProperty StatusBrushProperty =
        DependencyProperty.Register(nameof(StatusBrush), typeof(Brush), typeof(ConnectionStatusControl),
            new PropertyMetadata(DisconnectedBrush));

    public static readonly DependencyProperty StatusColorProperty =
        DependencyProperty.Register(nameof(StatusColor), typeof(Color), typeof(ConnectionStatusControl),
            new PropertyMetadata(DisconnectedColor));

    public static readonly DependencyProperty LatencyBrushProperty =
        DependencyProperty.Register(nameof(LatencyBrush), typeof(Brush), typeof(ConnectionStatusControl),
            new PropertyMetadata(GoodLatencyBrush));

    public static readonly DependencyProperty LatencyVisibilityProperty =
        DependencyProperty.Register(nameof(LatencyVisibility), typeof(Visibility), typeof(ConnectionStatusControl),
            new PropertyMetadata(Visibility.Collapsed));

    public string DeviceName
    {
        get => (string)GetValue(DeviceNameProperty);
        set => SetValue(DeviceNameProperty, value);
    }

    public ConnectionState State
    {
        get => (ConnectionState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public int Latency
    {
        get => (int)GetValue(LatencyProperty);
        set => SetValue(LatencyProperty, value);
    }

    public bool ShowLatency
    {
        get => (bool)GetValue(ShowLatencyProperty);
        set => SetValue(ShowLatencyProperty, value);
    }

    public int GoodLatencyThreshold
    {
        get => (int)GetValue(GoodLatencyThresholdProperty);
        set => SetValue(GoodLatencyThresholdProperty, value);
    }

    public int MediumLatencyThreshold
    {
        get => (int)GetValue(MediumLatencyThresholdProperty);
        set => SetValue(MediumLatencyThresholdProperty, value);
    }

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
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

    public Brush LatencyBrush
    {
        get => (Brush)GetValue(LatencyBrushProperty);
        set => SetValue(LatencyBrushProperty, value);
    }

    public Visibility LatencyVisibility
    {
        get => (Visibility)GetValue(LatencyVisibilityProperty);
        set => SetValue(LatencyVisibilityProperty, value);
    }

    public ConnectionStatusControl()
    {
        InitializeComponent();
        UpdateStatus();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConnectionStatusControl control)
        {
            control.UpdateStatus();
        }
    }

    private static void OnLatencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConnectionStatusControl control)
        {
            control.UpdateLatencyBrush();
        }
    }

    private static void OnShowLatencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ConnectionStatusControl control)
        {
            control.UpdateLatencyVisibility();
        }
    }

    private void UpdateStatus()
    {
        var storyboard = (Storyboard)Resources["PulseAnimation"];

        switch (State)
        {
            case ConnectionState.Connected:
                StatusText = "Connected";
                StatusBrush = ConnectedBrush;
                StatusColor = ConnectedColor;
                storyboard.Stop();
                break;
            case ConnectionState.Connecting:
                StatusText = "Connecting...";
                StatusBrush = ConnectingBrush;
                StatusColor = ConnectingColor;
                storyboard.Begin();
                break;
            case ConnectionState.Error:
                StatusText = "Error";
                StatusBrush = ErrorBrush;
                StatusColor = ErrorColor;
                storyboard.Stop();
                break;
            default:
                StatusText = "Disconnected";
                StatusBrush = DisconnectedBrush;
                StatusColor = DisconnectedColor;
                storyboard.Stop();
                break;
        }

        UpdateLatencyVisibility();
    }

    private void UpdateLatencyBrush()
    {
        if (Latency <= GoodLatencyThreshold)
        {
            LatencyBrush = GoodLatencyBrush;
        }
        else if (Latency <= MediumLatencyThreshold)
        {
            LatencyBrush = MediumLatencyBrush;
        }
        else
        {
            LatencyBrush = BadLatencyBrush;
        }
    }

    private void UpdateLatencyVisibility()
    {
        LatencyVisibility = ShowLatency && State == ConnectionState.Connected
            ? Visibility.Visible
            : Visibility.Collapsed;
    }
}
