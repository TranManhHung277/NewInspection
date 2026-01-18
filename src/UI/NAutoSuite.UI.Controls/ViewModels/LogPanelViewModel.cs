using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using NAutoSuite.UI.Controls.Dialogs;
using NAutoSuite.UI.Controls.Services;

namespace NAutoSuite.UI.Controls.ViewModels;

/// <summary>
/// ViewModel for LogPanel with filtering, export, and management features
/// </summary>
public partial class LogPanelViewModel : ObservableObject
{
    private const int MaxLogEntries = 1000;

    [ObservableProperty]
    private ObservableCollection<LogEntry> _allLogs = new();

    [ObservableProperty]
    private ObservableCollection<LogEntry> _filteredLogs = new();

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private bool _showDebug = true;

    [ObservableProperty]
    private bool _showInformation = true;

    [ObservableProperty]
    private bool _showWarning = true;

    [ObservableProperty]
    private bool _showError = true;

    [ObservableProperty]
    private bool _showFatal = true;

    [ObservableProperty]
    private bool _autoScrollEnabled = true;

    public LogPanelViewModel()
    {
        // Apply filter whenever any filter property changes
        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(SearchText) ||
                e.PropertyName == nameof(ShowDebug) ||
                e.PropertyName == nameof(ShowInformation) ||
                e.PropertyName == nameof(ShowWarning) ||
                e.PropertyName == nameof(ShowError) ||
                e.PropertyName == nameof(ShowFatal))
            {
                ApplyFilter();
            }
        };

        // Add initial log to confirm panel is ready
        AddLogDirect(DateTime.Now, "Information", "LogPanel initialized and ready to receive logs", null);

        // Subscribe to UILogService to receive logs from Serilog
        UILogService.Instance.LogReceived += OnLogReceived;
    }

    private void OnLogReceived(DateTime timestamp, string level, string message, string? exception)
    {
        AddLog(timestamp, level, message, exception);
    }

    /// <summary>
    /// Add log directly (used for initial log before dispatcher is ready)
    /// </summary>
    private void AddLogDirect(DateTime timestamp, string level, string message, string? exception)
    {
        var entry = new LogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Message = message,
            Exception = exception
        };

        AllLogs.Insert(0, entry);
        FilteredLogs.Insert(0, entry);
    }

    /// <summary>
    /// Add a log entry (automatically manages 1000 entry limit)
    /// </summary>
    public void AddLog(DateTime timestamp, string level, string message, string? exception = null)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher == null)
            return;

        if (dispatcher.CheckAccess())
        {
            AddLogInternal(timestamp, level, message, exception);
        }
        else
        {
            dispatcher.BeginInvoke(() => AddLogInternal(timestamp, level, message, exception));
        }
    }

    private void AddLogInternal(DateTime timestamp, string level, string message, string? exception)
    {
        var entry = new LogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Message = message,
            Exception = exception
        };

        AllLogs.Insert(0, entry);

        // Keep only last 1000 entries
        while (AllLogs.Count > MaxLogEntries)
        {
            AllLogs.RemoveAt(AllLogs.Count - 1);
        }

        // Check if entry matches current filter
        if (MatchesFilter(entry))
        {
            FilteredLogs.Insert(0, entry);

            while (FilteredLogs.Count > MaxLogEntries)
            {
                FilteredLogs.RemoveAt(FilteredLogs.Count - 1);
            }
        }
    }

    /// <summary>
    /// Check if a log entry matches current filter
    /// </summary>
    private bool MatchesFilter(LogEntry log)
    {
        // Check level filter
        var matchesLevel = log.Level switch
        {
            "Debug" => ShowDebug,
            "Information" => ShowInformation,
            "Warning" => ShowWarning,
            "Error" => ShowError,
            "Fatal" => ShowFatal,
            _ => true
        };

        if (!matchesLevel)
            return false;

        // Check search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.ToLower();
            var matchesSearch = log.Message.ToLower().Contains(searchLower) ||
                               log.Level.ToLower().Contains(searchLower) ||
                               (log.Exception?.ToLower().Contains(searchLower) ?? false);

            if (!matchesSearch)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Apply filter based on level checkboxes and search text
    /// </summary>
    private void ApplyFilter()
    {
        FilteredLogs.Clear();

        for (var i = 0; i < AllLogs.Count; i++)
        {
            var log = AllLogs[i];
            // Check level filter
            var matchesLevel = log.Level switch
            {
                "Debug" => ShowDebug,
                "Information" => ShowInformation,
                "Warning" => ShowWarning,
                "Error" => ShowError,
                "Fatal" => ShowFatal,
                _ => true
            };

            if (!matchesLevel)
                continue;

            // Check search text
            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var searchLower = SearchText.ToLower();
                var matchesSearch = log.Message.ToLower().Contains(searchLower) ||
                                   log.Level.ToLower().Contains(searchLower) ||
                                   (log.Exception?.ToLower().Contains(searchLower) ?? false);

                if (!matchesSearch)
                    continue;
            }

            FilteredLogs.Add(log);
        }
    }

    /// <summary>
    /// Clear all logs
    /// </summary>
    [RelayCommand]
    private void ClearLogs()
    {
        var result = ModernMessageBox.Show(
            "Are you sure you want to clear all logs?",
            "Clear Logs",
            ModernMessageBox.MessageBoxType.Question,
            ModernMessageBox.MessageBoxButtons.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            AllLogs.Clear();
            FilteredLogs.Clear();
        }
    }

    /// <summary>
    /// Export filtered logs to text file
    /// </summary>
    [RelayCommand]
    private void ExportLogs()
    {
        if (FilteredLogs.Count == 0)
        {
            ModernMessageBox.Show(
                "No logs to export.",
                "Export Logs",
                ModernMessageBox.MessageBoxType.Information,
                ModernMessageBox.MessageBoxButtons.OK);
            return;
        }

        var saveFileDialog = new SaveFileDialog
        {
            Title = "Export Logs",
            Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
            FileName = $"logs_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
            DefaultExt = "txt"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("===================================");
                sb.AppendLine($"Log Export - {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                sb.AppendLine($"Total Entries: {FilteredLogs.Count}");
                sb.AppendLine("===================================");
                sb.AppendLine();

                foreach (var log in FilteredLogs)
                {
                    sb.AppendLine($"[{log.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{log.Level}] {log.Message}");
                    if (!string.IsNullOrEmpty(log.Exception))
                    {
                        sb.AppendLine($"Exception: {log.Exception}");
                    }
                    sb.AppendLine();
                }

                File.WriteAllText(saveFileDialog.FileName, sb.ToString(), Encoding.UTF8);

                ModernMessageBox.Show(
                    $"Logs exported successfully to:\n{saveFileDialog.FileName}",
                    "Export Complete",
                    ModernMessageBox.MessageBoxType.Success,
                    ModernMessageBox.MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                ModernMessageBox.Show(
                    $"Failed to export logs:\n{ex.Message}",
                    "Export Error",
                    ModernMessageBox.MessageBoxType.Error,
                    ModernMessageBox.MessageBoxButtons.OK);
            }
        }
    }
}

/// <summary>
/// Log entry model
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }

    public string FormattedMessage =>
        $"[{Timestamp:HH:mm:ss.fff}] [{Level}] {Message}{(Exception != null ? $"\n{Exception}" : "")}";
}
