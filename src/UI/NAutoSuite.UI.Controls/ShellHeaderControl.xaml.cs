using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using NAutoSuite.Core.Machine;

namespace NAutoSuite.UI.Controls;

public partial class ShellHeaderControl : UserControl
{
    private readonly DispatcherTimer _timer;

    public static readonly DependencyProperty MachineNumberProperty =
        DependencyProperty.Register(nameof(MachineNumberValue), typeof(int), typeof(ShellHeaderControl),
            new PropertyMetadata(1, OnMachineNumberChanged));

    public static readonly DependencyProperty MachineStateProperty =
        DependencyProperty.Register(nameof(MachineState), typeof(MachineState), typeof(ShellHeaderControl),
            new PropertyMetadata(MachineState.Uninitialized, OnMachineStateChanged));

    public static readonly DependencyProperty ProjectNameProperty =
        DependencyProperty.Register(nameof(ProjectNameValue), typeof(string), typeof(ShellHeaderControl),
            new PropertyMetadata("NAutoSuite Project", OnProjectNameChanged));

    public static readonly DependencyProperty AlarmMessageProperty =
        DependencyProperty.Register(nameof(AlarmMessage), typeof(string), typeof(ShellHeaderControl),
            new PropertyMetadata(string.Empty, OnAlarmMessageChanged));

    public static readonly DependencyProperty HasAlarmProperty =
        DependencyProperty.Register(nameof(HasAlarm), typeof(bool), typeof(ShellHeaderControl),
            new PropertyMetadata(false, OnHasAlarmChanged));

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

    public string AlarmMessage
    {
        get => (string)GetValue(AlarmMessageProperty);
        set => SetValue(AlarmMessageProperty, value);
    }

    public bool HasAlarm
    {
        get => (bool)GetValue(HasAlarmProperty);
        set => SetValue(HasAlarmProperty, value);
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
        CurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
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

    private static void OnAlarmMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShellHeaderControl control)
        {
            var message = e.NewValue?.ToString();
            control.AlarmText.Text = string.IsNullOrEmpty(message) ? "No Alarm" : message;
        }
    }

    private static void OnHasAlarmChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ShellHeaderControl control && e.NewValue is bool hasAlarm)
        {
            if (hasAlarm)
            {
                control.AlarmIndicator.Fill = new SolidColorBrush(Color.FromRgb(244, 67, 54)); // Red
                control.AlarmBorder.Background = new SolidColorBrush(Color.FromArgb(40, 244, 67, 54));
            }
            else
            {
                control.AlarmIndicator.Fill = new SolidColorBrush(Color.FromRgb(128, 128, 128)); // Gray
                control.AlarmBorder.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));
            }
        }
    }
}
