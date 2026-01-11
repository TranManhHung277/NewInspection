namespace NAutoSuite.Core.Machine;

/// <summary>
/// Machine execution context
/// </summary>
public class MachineContext
{
    /// <summary>
    /// Machine cycle count
    /// </summary>
    public long CycleCount { get; set; }

    /// <summary>
    /// Total running time
    /// </summary>
    public TimeSpan TotalRunTime { get; set; }

    /// <summary>
    /// Current cycle start time
    /// </summary>
    public DateTime? CycleStartTime { get; set; }

    /// <summary>
    /// Last error message
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Additional context data
    /// </summary>
    public Dictionary<string, object> Data { get; } = new();

    /// <summary>
    /// Reset context
    /// </summary>
    public void Reset()
    {
        CycleCount = 0;
        TotalRunTime = TimeSpan.Zero;
        CycleStartTime = null;
        LastError = null;
        Data.Clear();
    }
}
