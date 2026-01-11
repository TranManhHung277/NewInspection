using Serilog;
using Serilog.Events;

namespace NAutoSuite.Core.Services;

/// <summary>
/// Centralized logging service
/// </summary>
public static class LogService
{
    /// <summary>
    /// Initialize logging with default configuration
    /// </summary>
    public static void Initialize(string logPath = "logs/machine-.log", LogEventLevel minimumLevel = LogEventLevel.Information)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(minimumLevel)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("Logging initialized");
    }

    /// <summary>
    /// Close and flush logs
    /// </summary>
    public static void Close()
    {
        Log.CloseAndFlush();
    }
}
