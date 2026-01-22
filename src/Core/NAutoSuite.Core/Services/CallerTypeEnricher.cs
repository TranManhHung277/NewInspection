using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace NAutoSuite.Core.Services;

public sealed class CallerTypeEnricher : ILogEventEnricher
{
    private const string PropertyName = "CallerType";

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        if (logEvent.Properties.ContainsKey(PropertyName))
        {
            return;
        }

        var caller = ResolveCallerTypeName();
        if (string.IsNullOrWhiteSpace(caller))
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty(PropertyName, caller));
    }

    private static string? ResolveCallerTypeName()
    {
        var trace = new StackTrace();
        var frames = trace.GetFrames();
        if (frames == null)
        {
            return null;
        }

        foreach (var frame in frames)
        {
            var method = frame.GetMethod();
            var type = method?.DeclaringType;
            if (type == null)
            {
                continue;
            }

            var name = type.FullName ?? string.Empty;
            if (name.StartsWith("Serilog.", StringComparison.Ordinal) ||
                name.StartsWith("System.", StringComparison.Ordinal) ||
                name.StartsWith("Microsoft.", StringComparison.Ordinal) ||
                name.StartsWith("NAutoSuite.UI.Controls.Services.", StringComparison.Ordinal) ||
                name.EndsWith(".CallerTypeEnricher", StringComparison.Ordinal))
            {
                continue;
            }

            return type.Name;
        }

        return null;
    }
}
