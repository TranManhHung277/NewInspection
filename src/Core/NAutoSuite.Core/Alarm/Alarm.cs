namespace NAutoSuite.Core.Alarm;

/// <summary>
/// Alarm severity levels
/// </summary>
public enum AlarmSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Represents a machine alarm
/// </summary>
public class Alarm
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public AlarmSeverity Severity { get; set; }
    public DateTime Timestamp { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ClearedAt { get; set; }
    public string? Source { get; set; }
    public Dictionary<string, object> Data { get; } = new();

    public Alarm()
    {
        Timestamp = DateTime.Now;
        IsActive = true;
    }

    public void Clear()
    {
        IsActive = false;
        ClearedAt = DateTime.Now;
    }

    public override string ToString()
    {
        return $"[{Severity}] {Code}: {Message} ({Timestamp:yyyy-MM-dd HH:mm:ss})";
    }
}
