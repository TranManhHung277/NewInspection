using System.Windows;
using System.Windows.Controls;

namespace NAutoSuite.UI.Controls;

/// <summary>
/// Reusable log panel control with filtering, export, and clear functionality
/// </summary>
public partial class LogPanel : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(LogPanel),
            new PropertyMetadata("System Logs"));

    /// <summary>
    /// Gets or sets the title displayed at the top of the log panel
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public LogPanel()
    {
        InitializeComponent();
    }
}
