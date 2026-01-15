using System.Windows;
using System.Windows.Controls;
using NAutoSuite.UI.Controls.ViewModels;

namespace NAutoSuite.UI.Controls;

/// <summary>
/// Reusable log panel control with filtering, export, and clear functionality
/// </summary>
public partial class LogPanel : UserControl
{
    private int _testLogCounter = 0;

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

    /// <summary>
    /// Gets the ViewModel for this LogPanel
    /// </summary>
    public LogPanelViewModel? ViewModel => DataContext as LogPanelViewModel;

    public LogPanel()
    {
        InitializeComponent();
    }

    private void TestLog_Click(object sender, RoutedEventArgs e)
    {
        _testLogCounter++;

        if (DataContext is LogPanelViewModel vm)
        {
            vm.AddLog(DateTime.Now, "Information", $"Test log message #{_testLogCounter}");
        }
    }

    /// <summary>
    /// Add a log entry directly to this panel
    /// </summary>
    public void AddLog(string level, string message, string? exception = null)
    {
        if (DataContext is LogPanelViewModel vm)
        {
            vm.AddLog(DateTime.Now, level, message, exception);
        }
    }

    /// <summary>
    /// Add an Information log
    /// </summary>
    public void LogInfo(string message) => AddLog("Information", message);

    /// <summary>
    /// Add a Warning log
    /// </summary>
    public void LogWarning(string message) => AddLog("Warning", message);

    /// <summary>
    /// Add an Error log
    /// </summary>
    public void LogError(string message, string? exception = null) => AddLog("Error", message, exception);

    /// <summary>
    /// Add a Debug log
    /// </summary>
    public void LogDebug(string message) => AddLog("Debug", message);
}
