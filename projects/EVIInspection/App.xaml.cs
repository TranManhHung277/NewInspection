using EVIInspection.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            // Initialize logging with UI sink FIRST
            InitializeLogging("logs/eviinspection-.log");
            // Build host with DI
            _host = Host.CreateDefaultBuilder()
                .UseSerilog() // Use the already configured Log.Logger
                .ConfigureServices((context, services) =>
                {
                    Log.Information("Registering services in DI container...");

                    // Register Serilog ILogger for injection
                    services.AddSingleton<Serilog.ILogger>(sp => Log.Logger);

                    // Load configurations
                    services.AddSingleton(sp =>
                    {
                        var configPath = Path.Combine(AppContext.BaseDirectory, "Configs", "hardware_config.yaml");
                        var logger = sp.GetRequiredService<Serilog.ILogger>();
                        return HardwareCatalogSettings.LoadSafe(configPath, logger);
                    });

                    // Register ViewModels
                    services.AddSingleton<MainViewModel>();

                    // Register MainWindow
                    services.AddSingleton<MainWindow>();
                })
                .Build();
            await _host.StartAsync();
            // Show main window
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
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

