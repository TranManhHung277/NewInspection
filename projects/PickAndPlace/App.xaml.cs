using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickAndPlace.ViewModels;
using PickAndPlace.Machine;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Services;
using NAutoSuite.Hardware.Abstractions.EtherCAT;
using NAutoSuite.Hardware.Simulator;
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
        Log.Information("Pick and Place Machine Application started (Simulation Mode)");
        Log.Information("Configuring dependency injection container...");

        // Build host with DI
        _host = Host.CreateDefaultBuilder()
            .UseSerilog() // Use the already configured Log.Logger
            .ConfigureServices((context, services) =>
            {
                Log.Information("Registering services in DI container...");

                // Register Core Services
                services.AddSingleton<TimeService>();

                // ===== HARDWARE CONFIGURATION (Leadshine EtherCAT Simulator) =====

                // Register Serilog ILogger for injection
                services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);

                services.AddSingleton<LeadshineEthercatSimulator>(sp =>
                {
                    var sim = new LeadshineEthercatSimulator();
                    sim.RegisterCylinder("QX1.0", "IX1.3", "IX1.4");
                    sim.RegisterVacuum("QX1.1", "IX1.5");
                    sim.SetInput("IX1.4", true);
                    sim.SetInput("IX0.5", true);
                    sim.ConfigureTestHead("IX1.0", "IX1.1", "IX1.2", cycleMs: 1500, okProbability: 0.7, pulseMs: 300);
                    return sim;
                });

                services.AddSingleton<IEtherCATMaster>(sp => sp.GetRequiredService<LeadshineEthercatSimulator>());
                services.AddSingleton<IIO>(sp => sp.GetRequiredService<LeadshineEthercatSimulator>());

                // Register simulated axes (3 axes)
                services.AddSingleton<IAxis>(sp =>
                {
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new SimulatorAxis("PAP_SIM_X", "AxisX", logger);
                });

                services.AddSingleton<IAxis>(sp =>
                {
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new SimulatorAxis("PAP_SIM_Y", "AxisY", logger);
                });

                services.AddSingleton<IAxis>(sp =>
                {
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new SimulatorAxis("PAP_SIM_Z", "AxisZ", logger);
                });

                // Register PickAndPlace Machine
                services.AddSingleton<PickAndPlaceMachine>(sp =>
                {
                    var axes = sp.GetServices<IAxis>().ToList();
                    var axisX = axes[0];
                    var axisY = axes[1];
                    var axisZ = axes[2];
                    var master = sp.GetRequiredService<IEtherCATMaster>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new PickAndPlaceMachine(axisX, axisY, axisZ, master, logger);
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

        Log.Information("Pick and Place Machine Application started (Simulation Mode)");
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
