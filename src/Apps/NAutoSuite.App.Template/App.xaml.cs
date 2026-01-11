using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NAutoSuite.App.Template.Machine;
using NAutoSuite.App.Template.ViewModels;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Services;
using NAutoSuite.Hardware.Simulator;
using Serilog;
using System.Windows;

namespace NAutoSuite.App.Template;

public partial class App : Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize logging
        LogService.Initialize("logs/app-.log");

        // Build host with DI
        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((context, services) =>
            {
                // Register Core Services
                services.AddSingleton<TimeService>();

                // Register Hardware (using Simulator for template)
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("AxisX", "X Axis", Log.Logger));
                services.AddSingleton<IAxis>(sp => new SimulatorAxis("AxisY", "Y Axis", Log.Logger));

                // Register Machine
                services.AddSingleton<IMachine>(sp =>
                {
                    var axes = sp.GetServices<IAxis>().ToArray();
                    return new TemplateMachine(
                        "MACHINE_001",
                        "Template Machine",
                        axes.ElementAtOrDefault(0),
                        axes.ElementAtOrDefault(1),
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
        var mainViewModel = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.DataContext = mainViewModel;
        mainWindow.Show();

        Log.Information("Application started");
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
