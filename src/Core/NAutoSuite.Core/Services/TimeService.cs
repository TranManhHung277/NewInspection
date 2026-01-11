using System.Diagnostics;

namespace NAutoSuite.Core.Services;

/// <summary>
/// High-resolution time service for realtime operations
/// </summary>
public class TimeService
{
    private readonly Stopwatch _stopwatch;
    private readonly long _startTimestamp;

    public TimeService()
    {
        _stopwatch = Stopwatch.StartNew();
        _startTimestamp = DateTime.Now.Ticks;
    }

    /// <summary>
    /// Get elapsed time since service start
    /// </summary>
    public TimeSpan Elapsed => _stopwatch.Elapsed;

    /// <summary>
    /// Get current timestamp with high resolution
    /// </summary>
    public long GetTimestampMicroseconds()
    {
        return _stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency;
    }

    /// <summary>
    /// Get current timestamp in milliseconds
    /// </summary>
    public long GetTimestampMilliseconds()
    {
        return _stopwatch.ElapsedMilliseconds;
    }

    /// <summary>
    /// Get absolute DateTime
    /// </summary>
    public DateTime GetCurrentTime()
    {
        return new DateTime(_startTimestamp + _stopwatch.ElapsedTicks);
    }

    /// <summary>
    /// Reset the timer
    /// </summary>
    public void Reset()
    {
        _stopwatch.Restart();
    }
}
