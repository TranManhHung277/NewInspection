using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NAutoSuite.Core.Machine;

namespace NAutoSuite.UI.Controls;

public partial class MachineStateControl : UserControl
{
    public static readonly DependencyProperty StateProperty =
        DependencyProperty.Register(nameof(State), typeof(MachineState), typeof(MachineStateControl),
            new PropertyMetadata(MachineState.Uninitialized, OnStateChanged));

    public MachineState State
    {
        get => (MachineState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public MachineStateControl()
    {
        InitializeComponent();
        UpdateState();
    }

    private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MachineStateControl control)
        {
            control.UpdateState();
        }
    }

    private void UpdateState()
    {
        StateText.Text = State.ToString();

        StateIndicator.Fill = State switch
        {
            MachineState.Idle => new SolidColorBrush(Color.FromRgb(33, 150, 243)), // Blue
            MachineState.Running => new SolidColorBrush(Color.FromRgb(76, 175, 80)), // Green
            MachineState.Paused => new SolidColorBrush(Color.FromRgb(255, 193, 7)), // Yellow
            MachineState.Error => new SolidColorBrush(Color.FromRgb(244, 67, 54)), // Red
            MachineState.EmergencyStop => new SolidColorBrush(Color.FromRgb(156, 39, 176)), // Purple
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128)) // Gray
        };
    }
}
