namespace NAutoSuite.UI.Controls.Services;

/// <summary>
/// Singleton service for collecting and distributing log entries to UI components
/// Thread-safe implementation supporting multiple subscribers with log caching
/// </summary>
public class UILogService
{
    private static readonly Lazy<UILogService> _instance = new(() => new UILogService());
    public static UILogService Instance => _instance.Value;

    private readonly Queue<(DateTime, string, string, string?)> _cachedLogs = new();
    private const int MaxCachedLogs = 200;
    private readonly object _lock = new();

    private event Action<DateTime, string, string, string?>? _logReceived;

    /// <summary>
    /// Event that replays cached logs to new subscribers
    /// </summary>
    public event Action<DateTime, string, string, string?>? LogReceived
    {
        add
        {
            lock (_lock)
            {
                // Replay cached logs to new subscriber
                foreach (var log in _cachedLogs)
                {
                    value?.Invoke(log.Item1, log.Item2, log.Item3, log.Item4);
                }
                _logReceived += value;
            }
        }
        remove
        {
            lock (_lock)
            {
                _logReceived -= value;
            }
        }
    }

    private UILogService()
    {
    }

    /// <summary>
    /// Add a log entry, cache it, and notify all subscribers
    /// </summary>
    public void AddLog(DateTime timestamp, string level, string message, string? exception = null)
    {
        lock (_lock)
        {
            // Cache log for future subscribers
            _cachedLogs.Enqueue((timestamp, level, message, exception));

            // Keep only last N logs
            while (_cachedLogs.Count > MaxCachedLogs)
            {
                _cachedLogs.Dequeue();
            }

            // Notify current subscribers
            _logReceived?.Invoke(timestamp, level, message, exception);
        }
    }
}
