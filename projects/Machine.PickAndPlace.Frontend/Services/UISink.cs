using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using NAutoSuite.UI.Controls.Services;

namespace Machine.PickAndPlace.Services;

/// <summary>
/// Custom Serilog sink that forwards log events to the UI
/// </summary>
public class UISink : ILogEventSink
{
    private readonly IFormatProvider? _formatProvider;

    public UISink(IFormatProvider? formatProvider)
    {
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        var message = logEvent.RenderMessage(_formatProvider);
        var level = logEvent.Level.ToString();
        var timestamp = logEvent.Timestamp.DateTime;
        var exception = logEvent.Exception?.ToString();

        // Forward to UILogService
        UILogService.Instance.AddLog(timestamp, level, message, exception);
    }
}

/// <summary>
/// Extension method to configure UISink
/// </summary>
public static class UISinkExtensions
{
    public static LoggerConfiguration UISink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider? formatProvider = null)
    {
        return loggerConfiguration.Sink(new UISink(formatProvider));
    }
}
