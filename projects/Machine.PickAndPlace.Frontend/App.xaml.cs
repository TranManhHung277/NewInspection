using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Machine.PickAndPlace.ViewModels;
using PickAndPlace.Frontend.Machine;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Services;
using NAutoSuite.Hardware.Leadshine;
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

                // ===== HARDWARE CONFIGURATION (Leadshine EtherCAT) =====

                // Register Leadshine Master (EtherCAT Card)
                // Note: Using local connection (no IP address) - dmc_board_init()
                services.AddSingleton<LeadshineMaster>(sp =>
                    new LeadshineMaster(
                        cardNo: 0,                      // Card number (0, 1, 2...)
                        ipAddress: null,                // null = use dmc_board_init() for local connection
                        logger: Log.Logger
                    )
                );

                // Register Real Axis (Only 1 axis for this demo)
                services.AddSingleton<IAxis>(sp =>
                {
                    var master = sp.GetRequiredService<LeadshineMaster>();
                    return new LeadshineAxis(
                        cardNo: 0,
                        axisIndex: 0,           // First axis (X)
                        id: "PAP001_X",
                        name: "AxisX",
                        master: master,
                        logger: Log.Logger
                    );
                });

                // Register PickAndPlace Machine with new logic
                services.AddSingleton<PickAndPlaceMachineFrontend>();

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
}
