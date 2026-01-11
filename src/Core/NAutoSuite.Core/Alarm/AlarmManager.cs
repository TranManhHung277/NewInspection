using System.Collections.Concurrent;
using Serilog;

namespace NAutoSuite.Core.Alarm;

/// <summary>
/// Manages machine alarms
/// </summary>
public class AlarmManager
{
    private readonly ConcurrentDictionary<int, Alarm> _activeAlarms = new();
    private readonly List<Alarm> _alarmHistory = new();
    private readonly ILogger _logger;
    private readonly int _maxHistorySize;

    public event EventHandler<Alarm>? AlarmRaised;
    public event EventHandler<Alarm>? AlarmCleared;

    public IEnumerable<Alarm> ActiveAlarms => _activeAlarms.Values;
    public IEnumerable<Alarm> AlarmHistory => _alarmHistory;

    public AlarmManager(ILogger? logger = null, int maxHistorySize = 1000)
    {
        _logger = logger ?? Log.Logger;
        _maxHistorySize = maxHistorySize;
    }

    /// <summary>
    /// Raise an alarm
    /// </summary>
    public void Raise(int code, string message, AlarmSeverity severity = AlarmSeverity.Warning, string? source = null)
    {
        var alarm = new Alarm
        {
            Code = code,
            Message = message,
            Severity = severity,
            Source = source
        };

        if (_activeAlarms.TryAdd(code, alarm))
        {
            _logger.Warning("Alarm raised: {Alarm}", alarm);
            AddToHistory(alarm);
            AlarmRaised?.Invoke(this, alarm);
        }
    }

    /// <summary>
    /// Clear an alarm by code
    /// </summary>
    public bool Clear(int code)
    {
        if (_activeAlarms.TryRemove(code, out var alarm))
        {
            alarm.Clear();
            _logger.Information("Alarm cleared: {Code}", code);
            AlarmCleared?.Invoke(this, alarm);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Clear all alarms
    /// </summary>
    public void ClearAll()
    {
        foreach (var alarm in _activeAlarms.Values)
        {
            alarm.Clear();
            AlarmCleared?.Invoke(this, alarm);
        }
        _activeAlarms.Clear();
        _logger.Information("All alarms cleared");
    }

    /// <summary>
    /// Check if any alarm is active
    /// </summary>
    public bool HasActiveAlarms => !_activeAlarms.IsEmpty;

    /// <summary>
    /// Check if specific alarm is active
    /// </summary>
    public bool IsAlarmActive(int code) => _activeAlarms.ContainsKey(code);

    /// <summary>
    /// Get alarm by code
    /// </summary>
    public Alarm? GetAlarm(int code)
    {
        _activeAlarms.TryGetValue(code, out var alarm);
        return alarm;
    }

    private void AddToHistory(Alarm alarm)
    {
        lock (_alarmHistory)
        {
            _alarmHistory.Add(alarm);
            if (_alarmHistory.Count > _maxHistorySize)
            {
                _alarmHistory.RemoveAt(0);
            }
        }
    }
}
