using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PickAndPlace.ViewModels;
using PickAndPlace.Machine;
using PickAndPlace.Configuration;
using PickAndPlace.Services;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Services;
using NAutoSuite.Hardware.Abstractions.EtherCAT;
using NAutoSuite.Hardware.Leadshine;
using NAutoSuite.Hardware.Keyence;
using NAutoSuite.UI.Controls.Services;
using Serilog;
using Serilog.Events;
using System.IO;
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

                // ===== HARDWARE CONFIGURATION (Leadshine EtherCAT) =====

                // Register Serilog ILogger for injection
                services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);

                services.AddSingleton(sp =>
                {
                    var configPath = Path.Combine(AppContext.BaseDirectory, "Configs", "hardware_config.yaml");
                    return HardwareCatalogSettings.Load(configPath);
                });

                services.AddSingleton<IReadOnlyList<LeadshineHardwareSettings>>(sp =>
                {
                    var catalog = sp.GetRequiredService<HardwareCatalogSettings>();
                    var entries = catalog.FindByType("leadshine").ToList();
                    if (entries.Count == 0)
                    {
                        throw new InvalidOperationException("No leadshine hardware configured");
                    }

                    return entries
                        .Select(entry => LeadshineHardwareSettings.LoadFromYamlNode(entry.ConfigNode))
                        .ToList();
                });

                services.AddSingleton<IReadOnlyList<KeyencePlcSettings>>(sp =>
                {
                    var catalog = sp.GetRequiredService<HardwareCatalogSettings>();
                    var entries = catalog.FindByType("keyence").ToList();
                    if (entries.Count == 0)
                    {
                        return Array.Empty<KeyencePlcSettings>();
                    }

                    return entries
                        .Select(entry => KeyencePlcSettings.LoadFromYamlNode(entry.ConfigNode))
                        .ToList();
                });

                services.AddSingleton<IReadOnlyList<LeadshineHardwareConfig>>(sp =>
                {
                    var settings = sp.GetRequiredService<IReadOnlyList<LeadshineHardwareSettings>>();
                    var cards = settings.SelectMany(entry => entry.ResolveUsedCards()).ToList();
                    if (cards.Count == 0)
                    {
                        throw new InvalidOperationException("Hardware config contains no enabled cards");
                    }

                    return cards;
                });

                services.AddSingleton(sp =>
                {
                    var cards = sp.GetRequiredService<IReadOnlyList<LeadshineHardwareConfig>>();
                    return LeadshineHardwareSettings.BuildMergedConfig(cards);
                });

                services.AddSingleton<PickAndPlaceProfile>(sp =>
                {
                    var profileConfig = new HardwareProfileConfig();
                    var leadshineCards = sp.GetRequiredService<IReadOnlyList<LeadshineHardwareConfig>>();
                    var keyenceConfigs = sp.GetRequiredService<IReadOnlyList<KeyencePlcSettings>>();

                    foreach (var card in leadshineCards)
                    {
                        profileConfig.AddLeadshine(card);
                    }

                    foreach (var plc in keyenceConfigs.Where(plc => plc.UsePlc))
                    {
                        profileConfig.AddKeyence(plc);
                    }

                    return new PickAndPlaceProfile(profileConfig);
                });

                services.AddSingleton<IEnumerable<IEtherCATMaster>>(sp =>
                {
                    var cards = sp.GetRequiredService<IReadOnlyList<LeadshineHardwareConfig>>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return cards.Select(card =>
                        (IEtherCATMaster)new LeadshineMaster((ushort)card.CardNo, card.EthercatIp, logger)).ToList();
                });

                services.AddSingleton<IEnumerable<IIO>>(sp =>
                {
                    var cards = sp.GetRequiredService<IReadOnlyList<LeadshineHardwareConfig>>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    var ioDevices = new List<IIO>();
                    ioDevices.AddRange(cards.Select(card =>
                    {
                        var points = card.IoPoints.Select(point =>
                        {
                            var address = LeadshineHardwareConfig.ResolveAddress(point);
                            return new RemoteIoPoint
                            {
                                Address = address,
                                NodeId = (ushort)point.NodeId,
                                IoBit = (ushort)point.IoBit,
                                IsOutput = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase)
                            };
                        }).ToList();

                        return (IIO)new LeadshineRemoteIO(card.CardNo, points, logger);
                    }));

                    var keyenceConfigs = sp.GetRequiredService<IReadOnlyList<KeyencePlcSettings>>();
                    foreach (var plcConfig in keyenceConfigs.Where(plc => plc.UsePlc))
                    {
                        ioDevices.Add(new KeyencePlc(plcConfig.Ip, plcConfig.Port, plcConfig.Name, logger));
                    }

                    return ioDevices;
                });
                services.AddSingleton<ISimulatedIO, NullSimulatedIO>();

                // Register simulated axes from profile
                services.AddSingleton<IEnumerable<IAxis>>(sp =>
                {
                    var cards = sp.GetRequiredService<IReadOnlyList<LeadshineHardwareConfig>>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return cards.SelectMany(card => card.Axes.Select(axis =>
                            (IAxis)new LeadshineAxis((ushort)card.CardNo,
                                (ushort)axis.AxisIndex,
                                axis.Id,
                                axis.Name,
                                null,
                                logger)))
                        .ToList();
                });

                services.AddSingleton<PickAndPlaceMachine>(sp =>
                {
                    var axes = sp.GetRequiredService<IEnumerable<IAxis>>().ToList();
                    var axisMap = axes.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
                    var axisX = axisMap["AxisX"];
                    var axisY = axisMap["AxisY"];
                    var axisZ = axisMap["AxisZ"];
                    var masters = sp.GetRequiredService<IEnumerable<IEtherCATMaster>>();
                    var ioDevices = sp.GetRequiredService<IEnumerable<IIO>>();
                    var profile = sp.GetRequiredService<PickAndPlaceProfile>();
                    var logger = sp.GetRequiredService<Serilog.ILogger>();
                    return new PickAndPlaceMachine(axisX, axisY, axisZ, masters, ioDevices, profile, logger);
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
