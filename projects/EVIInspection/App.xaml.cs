using EVIInspection.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Services;
using NAutoSuite.UI.Controls.Services;
using Serilog;
using Serilog.Events;
using System.IO;
using System.Threading;
using System.Windows;

namespace EVIInspection
{
    public partial class App : Application
    {
        private const string HardwareConfigFileName = "hardware_config.yaml";
        private const string SingleInstanceMutexName = "NAutoSuite.EVIInspection.SingleInstance";
        private IHost? _host;
        private Mutex? _singleInstanceMutex;

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            if (!AcquireSingleInstance())
            {
                return;
            }

            InitializeLogging("logs/eviinspection-.log");
            _host = BuildHost();

            await _host.StartAsync();
            ForceLoadHardwareConfiguration(_host.Services);
            ShowMainWindow(_host.Services);
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

        private static string GetHardwareConfigPath()
        {
            return Path.Combine(AppContext.BaseDirectory, "Configs", HardwareConfigFileName);
        }

        private static IHost BuildHost()
        {
            return Host.CreateDefaultBuilder()
                .UseSerilog() // Use the already configured Log.Logger
                .ConfigureServices((context, services) =>
                {
                    Log.Information("Registering services in DI container...");
                    RegisterCoreServices(services);
                    RegisterHardwareServices(services);
                    RegisterUiServices(services);
                })
                .Build();
        }

        private static void RegisterCoreServices(IServiceCollection services)
        {
            services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);
            services.AddSingleton(sp =>
            {
                var logger = sp.GetRequiredService<Serilog.ILogger>();
                return new HardwareConfigLoader(logger);
            });

            services.AddSingleton(sp =>
            {
                var loader = sp.GetRequiredService<HardwareConfigLoader>();
                return loader.LoadMinimalSafe(GetHardwareConfigPath());
            });

            services.AddSingleton(sp =>
            {
                var loader = sp.GetRequiredService<HardwareConfigLoader>();
                return loader.LoadMinimalEntitiesSafe(GetHardwareConfigPath());
            });
        }

        private static void RegisterHardwareServices(IServiceCollection services)
        {
            services.AddSingleton(sp =>
            {
                var config = sp.GetRequiredService<HardwareMinimalConfig>();
                return BuildHardwareBootstrap(config);
            });

            services.AddSingleton(sp => sp.GetRequiredService<HardwareBootstrap>().Profile);
            services.AddSingleton(sp => sp.GetRequiredService<HardwareBootstrap>().Devices);
        }

        private static void RegisterUiServices(IServiceCollection services)
        {
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }

        private static void ForceLoadHardwareConfiguration(IServiceProvider services)
        {
            _ = services.GetRequiredService<HardwareMinimalConfig>();
            _ = services.GetRequiredService<HardwareEntityCollection>();
            _ = services.GetRequiredService<HardwareBootstrap>();
        }

        private static void ShowMainWindow(IServiceProvider services)
        {
            var mainWindow = services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        private static HardwareBootstrap BuildHardwareBootstrap(HardwareMinimalConfig config)
        {
            var profile = new HardwareProfileConfig();
            var devices = new List<IDevice>();

            foreach (var axis in config.Axes)
            {
                profile.AddAxes(new[]
                {
                    new AxisDefinition
                    {
                        Id = axis.Id,
                        Name = axis.Name,
                        AxisIndex = axis.AxisNo
                    }
                });
            }

            var inputPoints = BuildIoPoints(config.Signals.Inputs, "Input");
            var outputPoints = BuildIoPoints(config.Signals.Outputs, "Output");
            profile.AddIoPoints("signal", inputPoints.Concat(outputPoints));

            return new HardwareBootstrap(devices, profile);
        }

        private static IEnumerable<IoPointConfig> BuildIoPoints(IEnumerable<SignalPointConfig> signals, string direction)
        {
            foreach (var signal in signals)
            {
                var addressPrefix = string.Equals(direction, "Output", StringComparison.OrdinalIgnoreCase) ? "O" : "I";
                var address = $"{addressPrefix}{signal.NodeId}:{signal.PortNo}:{signal.Bit}";
                yield return new IoPointConfig
                {
                    Id = signal.Id,
                    Name = signal.Name,
                    Address = address,
                    Direction = direction,
                    NodeId = signal.NodeId,
                    IoBit = signal.Bit,
                    DataType = "bit",
                    DefaultValue = signal.Default
                };
            }
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
            ReleaseSingleInstance();
            base.OnExit(e);
        }

        private bool AcquireSingleInstance()
        {
            _singleInstanceMutex = new Mutex(true, SingleInstanceMutexName, out var createdNew);
            if (createdNew)
            {
                return true;
            }

            try
            {
                _singleInstanceMutex.Dispose();
                _singleInstanceMutex = null;
            }
            catch
            {
                // Ignore dispose errors during shutdown.
            }

            MessageBox.Show(
                "EVIInspection is already running.",
                "EVIInspection",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            Shutdown();
            return false;
        }

        private void ReleaseSingleInstance()
        {
            if (_singleInstanceMutex == null)
            {
                return;
            }

            try
            {
                _singleInstanceMutex.ReleaseMutex();
                _singleInstanceMutex.Dispose();
            }
            finally
            {
                _singleInstanceMutex = null;
            }
        }
    }
}

