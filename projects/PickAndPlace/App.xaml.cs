using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickAndPlace.ViewModels;
using PickAndPlace.Machine;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Services;
using NAutoSuite.Hardware.Leadshine;
using NAutoSuite.UI.Controls.Services;
using Serilog;
using Serilog.Events;
using System.Windows;

namespace PickAndPlace;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logging with UI sink FIRST
        InitializeLogging("logs/pickandplace-.log");

        // Test logs at different levels
        Log.Debug("App startup - Debug level test");
        Log.Information("Pick and Place Machine Application started (EtherCAT Mode)");
        Log.Information("Configuring dependency injection container...");

        // Build host with DI
        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // Use the already configured Log.Logger
            .ConfigureServices((context, services) =>
            {
                Log.Information("Registering services in DI container...");

                // Register Core Services
                services.AddSingleton<TimeService>();

                // ===== HARDWARE CONFIGURATION (Leadshine EtherCAT) =====

                // Register Serilog ILogger for injection
                services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);

                // Register Leadshine Master (EtherCAT Card)
                // Note: Using local connection (no IP address) - dmc_board_init()
                services.AddSingleton<LeadshineMaster>(sp =>
                {
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new LeadshineMaster(
                        cardNo: 0,                      // Card number (0, 1, 2...)
                        ipAddress: null,                // null = use dmc_board_init() for local connection
                        logger: logger
                    );
                });

                // Register Real Axis (Only 1 axis for this demo)
                services.AddSingleton<IAxis>(sp =>
                {
                    var master = sp.GetRequiredService<LeadshineMaster>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new LeadshineAxis(
                        cardNo: 0,
                        axisIndex: 0,           // First axis (X)
                        id: "PAP001_X",
                        name: "AxisX",
                        master: master,
                        logger: logger
                    );
                });

                // Register PickAndPlace Machine
                services.AddSingleton<PickAndPlaceMachine>(sp =>
                {
                    var axis = sp.GetRequiredService<IAxis>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new PickAndPlaceMachine(axis, logger);
                });

                // Register ViewModels
                services.AddSingleton<MainViewModel>();

                // Register MainWindow
                services.AddSingleton<MainWindow>();
            })
            .Build();

        Log.Information("DI container built successfully");

        await _host.StartAsync();

        Log.Information("Host started, creating MainWindow...");

        // Show main window
        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();

        Log.Information("MainWindow displayed. Application ready. Hardware NOT initialized yet - waiting for user to click Initialize button.");

        Log.Information("Pick and Place Machine Application started (EtherCAT Mode)");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        Log.Information("Application shutting down");

        if (_host != null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        LogService.Close();
        base.OnExit(e);
    }

    /// <summary>
    /// Initialize Serilog with console, file, and UI sinks
    /// </summary>
    private void InitializeLogging(string logPath)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Debug) // Show all logs including Debug
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.UISink() // Forward logs to UI
            .CreateLogger();

        Log.Information("Logging initialized with UI sink");
    }
}
