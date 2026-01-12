using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Machine.PickAndPlace.ViewModels;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Services;
using NAutoSuite.Hardware.Simulator;
using Serilog;
using System.Windows;

namespace Machine.PickAndPlace;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logging
        LogService.Initialize("logs/pickandplace-.log");

        // Build host with DI
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                // Register Core Services
                services.AddSingleton<TimeService>();

                // Register Hardware (Simulator) - for UI display only
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("PAP001", "AxisX", Log.Logger));
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("PAP001", "AxisY", Log.Logger));
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("PAP001", "AxisZ", Log.Logger));
                services.AddSingleton<IOutput>(sp => new SimulatorOutput("PAP001", "Vacuum", Log.Logger));

                // NOTE: Machine logic is in Backend project
                // Frontend is UI only

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

        Log.Information("Pick and Place Machine Application started");
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
}
