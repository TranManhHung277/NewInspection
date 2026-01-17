using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using System.Windows.Threading;
using NAutoSuite.Core.Machine;

namespace NAutoSuite.UI.Controls;

/// <summary>
/// Status level for header background color
/// </summary>
public enum HeaderStatusLevel
{
    Ok,
    Warning,
    Error
}

public partial class ShellHeaderControl : UserControl
{
    private readonly DispatcherTimer _timer;
    private SolidColorBrush _currentStatusBrush = OkBackground;
    private bool _blinkOn;
    private readonly Stopwatch _blinkWatch = new();
    private static readonly TimeSpan BlinkInterval = TimeSpan.FromMilliseconds(500);

    // Background colors for each status level
    private static readonly SolidColorBrush OkBackground = new(Color.FromRgb(0x2D, 0x2D, 0x30));       // #2D2D30
    private static readonly SolidColorBrush WarningBackground = new(Color.FromRgb(0xFF, 0x8C, 0x00));  // DarkOrange
    private static readonly SolidColorBrush ErrorBackground = new(Color.FromRgb(0xD3, 0x2F, 0x2F));    // Red #D32F2F

    public static readonly DependencyProperty MachineNumberProperty =
        DependencyProperty.Register(nameof(MachineNumberValue), typeof(int), typeof(ShellHeaderControl),
            new PropertyMetadata(1, OnMachineNumberChanged));

    public static readonly DependencyProperty MachineStateProperty =
        DependencyProperty.Register(nameof(MachineState), typeof(MachineState), typeof(ShellHeaderControl),
            new PropertyMetadata(MachineState.Uninitialized, OnMachineStateChanged));

    public static readonly DependencyProperty ProjectNameProperty =
        DependencyProperty.Register(nameof(ProjectNameValue), typeof(string), typeof(ShellHeaderControl),
            new PropertyMetadata("NAutoSuite Project", OnProjectNameChanged));

    public static readonly DependencyProperty ModelNameProperty =
        DependencyProperty.Register(nameof(ModelNameValue), typeof(string), typeof(ShellHeaderControl),
            new PropertyMetadata("No Model", OnModelNameChanged));

    public static readonly DependencyProperty StatusLevelProperty =
        DependencyProperty.Register(nameof(StatusLevel), typeof(HeaderStatusLevel), typeof(ShellHeaderControl),
            new PropertyMetadata(HeaderStatusLevel.Ok, OnStatusLevelChanged));

    public int MachineNumberValue
    {
        get => (int)GetValue(MachineNumberProperty);
        set => SetValue(MachineNumberProperty, value);
    }

    public MachineState MachineState
    {
        get => (MachineState)GetValue(MachineStateProperty);
        set => SetValue(MachineStateProperty, value);
    }

    public string ProjectNameValue
    {
        get => (string)GetValue(ProjectNameProperty);
        set => SetValue(ProjectNameProperty, value);
    }

    public string ModelNameValue
    {
        get => (string)GetValue(ModelNameProperty);
        set => SetValue(ModelNameProperty, value);
    }

    /// <summary>
    /// Status level that controls the header background color
    /// Ok = #221C16, Warning = DarkOrange, Error = Red
    /// </summary>
    public HeaderStatusLevel StatusLevel
    {
        get => (HeaderStatusLevel)GetValue(StatusLevelProperty);
        set => SetValue(StatusLevelProperty, value);
    }

    public event RoutedEventHandler? CloseClicked;

    public ShellHeaderControl()
    {
        InitializeComponent();

        // Timer for current time
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();

        CloseButton.Click += (s, e) => CloseClicked?.Invoke(this, e);

        UpdateTime();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        UpdateTime();
    }

    private void UpdateTime()
    {
        CurrentTime.Text = DateTime.Now.ToString("HH:mm:ss dd/MM/yyyy");
    }

    private static void OnMachineNumberChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShellHeaderControl control)
        {
            control.MachineNumber.Text = e.NewValue?.ToString() ?? "1";
        }
    }

    private static void OnMachineStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShellHeaderControl control && e.NewValue is MachineState state)
        {
            control.StateControl.State = state;
        }
    }

    private static void OnProjectNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShellHeaderControl control)
        {
            control.ProjectName.Text = e.NewValue?.ToString() ?? "NAutoSuite Project";
        }
    }

    private static void OnModelNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShellHeaderControl control)
        {
            var modelName = e.NewValue?.ToString();
            control.ModelName.Text = string.IsNullOrEmpty(modelName) ? "No Model" : modelName;
        }
    }

    private static void OnStatusLevelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShellHeaderControl control && e.NewValue is HeaderStatusLevel status)
        {
            control.ApplyStatus(status);
        }
    }

    private void ApplyStatus(HeaderStatusLevel status)
    {
        _currentStatusBrush = status switch
        {
            HeaderStatusLevel.Error => ErrorBackground,
            HeaderStatusLevel.Warning => WarningBackground,
            _ => OkBackground
        };

        if (status == HeaderStatusLevel.Ok)
        {
            StopBlink();
            return;
        }

        StartBlink();
    }

    private void StartBlink()
    {
        _blinkOn = true;
        MainBorder.Background = _currentStatusBrush;
        _blinkWatch.Restart();
        CompositionTarget.Rendering -= OnBlinkRendering;
        CompositionTarget.Rendering += OnBlinkRendering;
    }

    private void StopBlink()
    {
        CompositionTarget.Rendering -= OnBlinkRendering;
        _blinkWatch.Stop();
        _blinkOn = false;
        MainBorder.Background = OkBackground;
    }

    private void OnBlinkRendering(object? sender, EventArgs e)
    {
        if (_blinkWatch.Elapsed < BlinkInterval)
        {
            return;
        }

        _blinkWatch.Restart();
        _blinkOn = !_blinkOn;
        MainBorder.Background = _blinkOn ? _currentStatusBrush : OkBackground;
    }
}
