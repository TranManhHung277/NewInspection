using Machine.PickAndPlace.ViewModels;

namespace Machine.PickAndPlace.Services;

/// <summary>
/// Singleton service for collecting and distributing log entries to UI
/// </summary>
public class UILogService
{
    private static readonly Lazy<UILogService> _instance = new(() => new UILogService());
    public static UILogService Instance => _instance.Value;

    public event Action<LogEntry>? LogReceived;

    private UILogService()
    {
    }

    public void AddLog(DateTime timestamp, string level, string message, string? exception = null)
    {
        var entry = new LogEntry
        {
            Timestamp = timestamp,
            Level = level,
            Message = message,
            Exception = exception
        };

        LogReceived?.Invoke(entry);
    }
}
