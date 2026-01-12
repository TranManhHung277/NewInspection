using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Machine.PickAndPlace.Core;
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

                // Register Hardware (Simulator)
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("AxisX", "X Axis", Log.Logger));
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("AxisY", "Y Axis", Log.Logger));
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("AxisZ", "Z Axis", Log.Logger));
                services.AddSingleton<IInput>(sp => new SimulatorInput("PartSensor", "Part Sensor", Log.Logger));
                services.AddSingleton<IOutput>(sp => new SimulatorOutput("Vacuum", "Vacuum", Log.Logger));

                // Register Machine
                services.AddSingleton<IMachine>(sp =>
                {
                    var axes = sp.GetServices<IAxis>().ToArray();
                    var partSensor = sp.GetServices<IInput>().FirstOrDefault();
                    var vacuum = sp.GetServices<IOutput>().FirstOrDefault();

                    return new PickAndPlaceMachine(
                        "PNP_001",
                        "Pick and Place Machine",
                        axes.ElementAtOrDefault(0), // X
                        axes.ElementAtOrDefault(1), // Y
                        axes.ElementAtOrDefault(2), // Z
                        partSensor,
                        vacuum,
                        Log.Logger);
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
