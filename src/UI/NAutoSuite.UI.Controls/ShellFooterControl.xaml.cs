using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class ShellFooterControl : UserControl
{
    private Border? _selectedTab;

    public static readonly DependencyProperty StartCommandProperty =
        DependencyProperty.Register(nameof(StartCommand), typeof(ICommand), typeof(ShellFooterControl));

    public static readonly DependencyProperty StopCommandProperty =
        DependencyProperty.Register(nameof(StopCommand), typeof(ICommand), typeof(ShellFooterControl));

    public static readonly DependencyProperty ResetCommandProperty =
        DependencyProperty.Register(nameof(ResetCommand), typeof(ICommand), typeof(ShellFooterControl));

    public static readonly DependencyProperty HomeHoldStartCommandProperty =
        DependencyProperty.Register(nameof(HomeHoldStartCommand), typeof(ICommand), typeof(ShellFooterControl));

    public static readonly DependencyProperty HomeHoldEndCommandProperty =
        DependencyProperty.Register(nameof(HomeHoldEndCommand), typeof(ICommand), typeof(ShellFooterControl));

    public event EventHandler<string>? TabChanged;

    public ShellFooterControl()
    {
        InitializeComponent();
        _selectedTab = AutoTab;
    }

    private void Tab_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border clickedTab && clickedTab.Tag is string tabName)
        {
            SelectTab(clickedTab, tabName);
        }
    }

    public void SelectTab(string tabName)
    {
        var tab = tabName switch
        {
            "Auto" => AutoTab,
            "Manual" => ManualTab,
            "Data" => TeachTab,
            "Camera" => CameraTab,
            "Setting" => SettingTab,
            "Log" => LogTab,
            _ => AutoTab
        };

        SelectTab(tab, tabName);
    }

    private void SelectTab(Border tab, string tabName)
    {
        // Deselect previous
        if (_selectedTab != null)
        {
            _selectedTab.Background = Brushes.Transparent;
            SetTabTextColor(_selectedTab, Color.FromRgb(176, 176, 176)); // Gray
        }

        // Select new
        _selectedTab = tab;
        _selectedTab.Background = new SolidColorBrush(Color.FromRgb(37, 37, 38));
        SetTabTextColor(_selectedTab, Color.FromRgb(33, 150, 243)); // Blue

        TabChanged?.Invoke(this, tabName);
    }

    private void SetTabTextColor(Border tab, Color color)
    {
        if (tab.Child is StackPanel stack)
        {
            foreach (var child in stack.Children)
            {
                if (child is TextBlock text)
                {
                    text.Foreground = new SolidColorBrush(color);
                }
            }
        }
    }

    private void HomeHoldStart(object sender, MouseButtonEventArgs e)
    {
        if (HomeHoldStartCommand?.CanExecute(null) == true)
        {
            HomeHoldStartCommand.Execute(null);
        }
    }

    private void HomeHoldEnd(object sender, MouseButtonEventArgs e)
    {
        if (HomeHoldEndCommand?.CanExecute(null) == true)
        {
            HomeHoldEndCommand.Execute(null);
        }
    }

    public ICommand? StartCommand
    {
        get => (ICommand?)GetValue(StartCommandProperty);
        set => SetValue(StartCommandProperty, value);
    }

    public ICommand? StopCommand
    {
        get => (ICommand?)GetValue(StopCommandProperty);
        set => SetValue(StopCommandProperty, value);
    }

    public ICommand? ResetCommand
    {
        get => (ICommand?)GetValue(ResetCommandProperty);
        set => SetValue(ResetCommandProperty, value);
    }

    public ICommand? HomeHoldStartCommand
    {
        get => (ICommand?)GetValue(HomeHoldStartCommandProperty);
        set => SetValue(HomeHoldStartCommandProperty, value);
    }

    public ICommand? HomeHoldEndCommand
    {
        get => (ICommand?)GetValue(HomeHoldEndCommandProperty);
        set => SetValue(HomeHoldEndCommandProperty, value);
    }
}
