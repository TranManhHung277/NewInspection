using PickAndPlace.Backend.Machine;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Hardware.Simulator;
using Serilog;

// Configure logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/backend-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

Log.Information("=== PickAndPlace Backend Started ===");

// Create hardware (Simulator)
IAxis axisX = new SimulatorAxis("PAP001", "AxisX", Log.Logger);
IAxis axisY = new SimulatorAxis("PAP001", "AxisY", Log.Logger);
IAxis axisZ = new SimulatorAxis("PAP001", "AxisZ", Log.Logger);
IOutput vacuum = new SimulatorOutput("PAP001", "Vacuum", Log.Logger);

// Create machine
var machine = new PickAndPlaceMachine(
    id: "PAP001",
    name: "PickAndPlace Machine",
    axisX: axisX,
    axisY: axisY,
    axisZ: axisZ,
    vacuum: vacuum,
    logger: Log.Logger
);

Log.Information("Machine created: {Name}", machine.GetType().Name);

// Initialize machine
Log.Information("Initializing machine (Homing all axes)...");
var initResult = await machine.InitializeAsync();
if (initResult.IsSuccess)
{
    Log.Information("Machine initialized successfully");
}
else
{
    Log.Error("Initialize failed: {Message}", initResult.Message);
}

// Start AUTO mode
Log.Information("Starting AUTO mode...");
var startResult = await machine.StartAsync();
if (startResult.IsSuccess)
{
    Log.Information("Machine started - Running AUTO sequence");
}
else
{
    Log.Error("Start failed: {Message}", startResult.Message);
}

// Run until user press key
Log.Information("Press any key to stop...");
Console.ReadKey();

// Stop machine
Log.Information("Stopping machine...");
await machine.StopAsync();

Log.Information("=== Backend Stopped ===");
