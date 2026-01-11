using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NAutoSuite.UI.Controls;

public partial class ShellFooterControl : UserControl
{
    private Border? _selectedTab;

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
            "Setting" => SettingTab,
            "Data" => DataTab,
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
}
