using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NAutoSuite.UI.Controls.ViewModels;

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

    /// <summary>
    /// Gets the ViewModel for this LogPanel
    /// </summary>
    public LogPanelViewModel? ViewModel => DataContext as LogPanelViewModel;

    public LogPanel()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
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

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AttachViewModel(DataContext as LogPanelViewModel);
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        DetachViewModel(DataContext as LogPanelViewModel);
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        DetachViewModel(e.OldValue as LogPanelViewModel);
        AttachViewModel(e.NewValue as LogPanelViewModel);
    }

    private void AttachViewModel(LogPanelViewModel? vm)
    {
        if (vm == null) return;
        vm.FilteredLogs.CollectionChanged += OnLogsChanged;
        vm.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void DetachViewModel(LogPanelViewModel? vm)
    {
        if (vm == null) return;
        vm.FilteredLogs.CollectionChanged -= OnLogsChanged;
        vm.PropertyChanged -= OnViewModelPropertyChanged;
    }

    private void OnLogsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ViewModel?.AutoScrollEnabled != true)
        {
            return;
        }

        Dispatcher.BeginInvoke(() => LogScrollViewer.ScrollToTop());
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(LogPanelViewModel.AutoScrollEnabled) &&
            ViewModel?.AutoScrollEnabled == true)
        {
            Dispatcher.BeginInvoke(() => LogScrollViewer.ScrollToTop());
        }
    }
}
