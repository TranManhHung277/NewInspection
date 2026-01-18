using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NAutoSuite.Core.Machine;

namespace NAutoSuite.UI.Controls;

public partial class RunModeControl : UserControl
{
    public static readonly DependencyProperty ModeProperty =
        DependencyProperty.Register(nameof(Mode), typeof(MachineRunMode), typeof(RunModeControl),
            new PropertyMetadata(MachineRunMode.Auto, OnModeChanged));

    public MachineRunMode Mode
    {
        get => (MachineRunMode)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public RunModeControl()
    {
        InitializeComponent();
        UpdateMode();
    }

    private static void OnModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is RunModeControl control)
        {
            control.UpdateMode();
        }
    }

    private void UpdateMode()
    {
        ModeText.Text = Mode.ToString();

        ModeText.Foreground = Mode switch
        {
            MachineRunMode.Auto => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
            MachineRunMode.Manual => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
            MachineRunMode.DryRun => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
            _ => new SolidColorBrush(Color.FromRgb(128, 128, 128))
        };
    }
}
