using System.Collections.Generic;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace NAutoSuite.UI.Controls.Services;

/// <summary>
/// Custom Serilog sink that forwards log events to the UI via UILogService.
/// This sink is shared across all projects using NAutoSuite.UI.Controls.
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
        var callerType = GetScalarProperty(logEvent, "CallerType");
        var prefix = BuildPrefix(callerType);
        var message = prefix + logEvent.RenderMessage(_formatProvider);
        var level = logEvent.Level.ToString();
        var timestamp = logEvent.Timestamp.DateTime;
        var exception = logEvent.Exception?.ToString();

        // Forward to UILogService
        UILogService.Instance.AddLog(timestamp, level, message, exception);
    }

    private static string? GetScalarProperty(LogEvent logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var value) &&
            value is ScalarValue scalar &&
            scalar.Value != null)
        {
            return scalar.Value.ToString();
        }

        return null;
    }

    private static string BuildPrefix(string? callerType)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(callerType))
        {
            parts.Add(callerType);
        }

        return parts.Count == 0 ? string.Empty : $"[{string.Join("][", parts)}] ";
    }
}

/// <summary>
/// Extension method to configure UISink in Serilog pipeline.
/// Usage: .WriteTo.UISink()
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
