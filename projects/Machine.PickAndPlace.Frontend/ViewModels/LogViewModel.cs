using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using Machine.PickAndPlace.Services;

namespace Machine.PickAndPlace.ViewModels;

public partial class LogViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<LogEntry> _logs = new();

    public LogViewModel()
    {
        // Register with log service to receive log entries
        UILogService.Instance.LogReceived += OnLogReceived;
    }

    private void OnLogReceived(LogEntry entry)
    {
        // Ensure UI thread update
        Application.Current?.Dispatcher.Invoke(() =>
        {
            Logs.Add(entry);

            // Keep only last 1000 entries to prevent memory issues
            while (Logs.Count > 1000)
            {
                Logs.RemoveAt(0);
            }
        });
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
