using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NAutoApp.ViewModels;
using NAutoApp.Machine;
using NAutoApp.Configuration;
using NAutoApp.Services;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Entities;
using NAutoSuite.Core.Services;
using NAutoSuite.Hardware.Abstractions.Hardware;
using NAutoSuite.Hardware.Abstractions.EtherCAT;
using NAutoSuite.Hardware.Keyence;
using NAutoSuite.Hardware.Leadshine;
using NAutoSuite.Hardware.Simulator;
using NAutoSuite.UI.Controls.Services;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Windows;

namespace NAutoApp;

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

                // ===== HARDWARE CONFIGURATION (Leadshine EtherCAT) =====

                // Register Serilog ILogger for injection
                services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);

                services.AddSingleton(sp =>
                {
                    var configPath = Path.Combine(AppContext.BaseDirectory, "Configs", "hardware_config.yaml");
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return HardwareCatalogSettings.LoadSafe(configPath, logger);
                });

                services.AddSingleton<IEnumerable<IHardwareModule>>(sp =>
                {
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new IHardwareModule[]
                    {
                        new LeadshineHardwareModule(logger),
                        new KeyenceHardwareModule(logger)
                    };
                });

                services.AddSingleton<HardwareBootstrap>(sp =>
                {
                    var catalog = sp.GetRequiredService<HardwareCatalogSettings>();
                    var modules = sp.GetRequiredService<IEnumerable<IHardwareModule>>().ToList();
                    var devices = new List<IDevice>();
                    var profile = new HardwareProfileConfig();

                    foreach (var entry in catalog.Hardware)
                    {
                        var module = modules.FirstOrDefault(m =>
                            string.Equals(m.Type, entry.Type, StringComparison.OrdinalIgnoreCase));
                        if (module == null)
                        {
                            throw new InvalidOperationException($"No hardware module registered for '{entry.Type}'");
                        }

                        var result = module.LoadFromConfig(entry.ConfigNode);
                        devices.AddRange(result.Devices);
                        profile.AddAxes(result.Axes);
                        profile.AddIoPoints(entry.Name, result.IoPoints);
                        profile.AddDataPoints(entry.Name, result.DataPoints);
                        profile.MergeRegisters(result.Registers);
                    }

                    return new HardwareBootstrap(devices, profile);
                });

                services.AddSingleton<EntityRegistry>(sp =>
                {
                    var bootstrap = sp.GetRequiredService<HardwareBootstrap>();
                    return new EntityRegistry(bootstrap.Profile.IoPoints);
                });

                services.AddSingleton<EntityEventBus>();
                services.AddSingleton<EntityStateService>();
                services.AddSingleton<EntityCommandService>();
                services.AddSingleton<DataEntityRegistry>(sp =>
                {
                    var bootstrap = sp.GetRequiredService<HardwareBootstrap>();
                    return new DataEntityRegistry(bootstrap.Profile.DataPoints);
                });
                services.AddSingleton<DataEventBus>();
                services.AddSingleton<DataStateService>();
                services.AddSingleton<ConfigReloadService>();

                services.AddSingleton<ConfigMachineProfile>(sp =>
                {
                    var bootstrap = sp.GetRequiredService<HardwareBootstrap>();
                    return new ConfigMachineProfile(bootstrap.Profile);
                });

                services.AddSingleton<IEnumerable<IEtherCATMaster>>(sp =>
                {
                    var bootstrap = sp.GetRequiredService<HardwareBootstrap>();
                    return bootstrap.Devices.OfType<IEtherCATMaster>().ToList();
                });

                services.AddSingleton<IEnumerable<IIO>>(sp =>
                {
                    var bootstrap = sp.GetRequiredService<HardwareBootstrap>();
                    return bootstrap.Devices.OfType<IIO>().ToList();
                });
                services.AddSingleton<IEnumerable<IRegisterIO>>(sp =>
                {
                    var bootstrap = sp.GetRequiredService<HardwareBootstrap>();
                    return bootstrap.Devices.OfType<IRegisterIO>().ToList();
                });
                services.AddSingleton<ISimulatedIO, NullSimulatedIO>();

                // Register simulated axes from profile
                services.AddSingleton<IEnumerable<IAxis>>(sp =>
                {
                    var bootstrap = sp.GetRequiredService<HardwareBootstrap>();
                    var axes = bootstrap.Devices.OfType<IAxis>().ToList();
                    if (axes.Count == 0)
                    {
                        return new List<IAxis>
                        {
                            new SimulatorAxis("AxisX", "AxisX", Log.Logger),
                            new SimulatorAxis("AxisY", "AxisY", Log.Logger),
                            new SimulatorAxis("AxisZ", "AxisZ", Log.Logger)
                        };
                    }

                    return axes;
                });

                services.AddSingleton<NAutoAppMachine>(sp =>
                {
                    var axes = sp.GetRequiredService<IEnumerable<IAxis>>().ToList();
                    var axisMap = axes.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
                    var axisX = axisMap["AxisX"];
                    var axisY = axisMap["AxisY"];
                    var axisZ = axisMap["AxisZ"];
                    var masters = sp.GetRequiredService<IEnumerable<IEtherCATMaster>>();
                    var ioDevices = sp.GetRequiredService<IEnumerable<IIO>>();
                    var registerDevices = sp.GetRequiredService<IEnumerable<IRegisterIO>>();
                    var profile = sp.GetRequiredService<ConfigMachineProfile>();
                    var entityStateService = sp.GetRequiredService<EntityStateService>();
                    var dataStateService = sp.GetRequiredService<DataStateService>();
                    var entityRegistry = sp.GetRequiredService<EntityRegistry>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new NAutoAppMachine(axisX, axisY, axisZ, masters, ioDevices, registerDevices, profile, entityRegistry, entityStateService, dataStateService, logger);
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

        Log.Information("MainWindow displayed. Application ready (auto-initialize enabled).");

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
            .Enrich.With<CallerTypeEnricher>()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CallerType}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} {Level:u3}] [{CallerType}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.UISink() // Forward logs to UI
            .CreateLogger();

        Log.Information("Logging initialized with UI sink");
    }
}

