using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace NAutoSuite.UI.Controls;

public partial class SwitchToggleControl : UserControl
{
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(SwitchToggleControl),
            new PropertyMetadata(false));

    public static readonly DependencyProperty OnLabelProperty =
        DependencyProperty.Register(nameof(OnLabel), typeof(string), typeof(SwitchToggleControl),
            new PropertyMetadata("ON"));

    public static readonly DependencyProperty OffLabelProperty =
        DependencyProperty.Register(nameof(OffLabel), typeof(string), typeof(SwitchToggleControl),
            new PropertyMetadata("OFF"));

    public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SwitchToggleControl),
            new PropertyMetadata(null));

    public static readonly DependencyProperty CommandParameterProperty =
        DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(SwitchToggleControl),
            new PropertyMetadata(null));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public string OnLabel
    {
        get => (string)GetValue(OnLabelProperty);
        set => SetValue(OnLabelProperty, value);
    }

    public string OffLabel
    {
        get => (string)GetValue(OffLabelProperty);
        set => SetValue(OffLabelProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public SwitchToggleControl()
    {
        InitializeComponent();
    }
}
