using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Machine;
using Serilog;

namespace Machine.PickAndPlace.Machines;

public class PickAndPlaceMachine : MachineBase
{
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly IAxis? _axisZ;
    private readonly IInput? _partSensor;
    private readonly IOutput? _vacuum;
    private readonly ILogger _machineLogger;

    public PickAndPlaceMachine(
        string id,
        string name,
        IAxis? axisX = null,
        IAxis? axisY = null,
        IAxis? axisZ = null,
        IInput? partSensor = null,
        IOutput? vacuum = null,
        ILogger? logger = null)
        : base(id, name, logger)
    {
        _axisX = axisX;
        _axisY = axisY;
        _axisZ = axisZ;
        _partSensor = partSensor;
        _vacuum = vacuum;
        _machineLogger = logger ?? Log.Logger;
    }

    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        _machineLogger.Information("Initializing Pick and Place Machine...");

        // Connect axes
        if (_axisX != null) await _axisX.ConnectAsync();
        if (_axisY != null) await _axisY.ConnectAsync();
        if (_axisZ != null) await _axisZ.ConnectAsync();

        // Connect I/O
        if (_partSensor != null) await _partSensor.ConnectAsync();
        if (_vacuum != null) await _vacuum.ConnectAsync();

        _machineLogger.Information("Hardware connected");
    }

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        _machineLogger.Information("Starting Pick and Place cycle {CycleCount}", Context.CycleCount + 1);

        try
        {
            // Pick and place sequence
            await PickSequenceAsync();
            await PlaceSequenceAsync();

            _machineLogger.Information("Cycle {CycleCount} completed successfully", Context.CycleCount + 1);
        }
        catch (Exception ex)
        {
            _machineLogger.Error(ex, "Cycle {CycleCount} failed", Context.CycleCount + 1);
            Context.LastError = ex.Message;
        }
    }

    private async Task PickSequenceAsync()
    {
        _machineLogger.Information("Pick sequence started");

        // Move to pick position
        if (_axisX != null) await _axisX.MoveAbsoluteAsync(100);
        if (_axisY != null) await _axisY.MoveAbsoluteAsync(50);

        // Wait for axes to arrive
        await Task.Delay(500);

        // Lower Z
        if (_axisZ != null) await _axisZ.MoveAbsoluteAsync(10);
        await Task.Delay(300);

        // Activate vacuum
        if (_vacuum != null) await _vacuum.WriteAsync(true);
        await Task.Delay(200);

        // Raise Z
        if (_axisZ != null) await _axisZ.MoveAbsoluteAsync(50);
        await Task.Delay(300);

        _machineLogger.Information("Pick sequence completed");
    }

    private async Task PlaceSequenceAsync()
    {
        _machineLogger.Information("Place sequence started");

        // Move to place position
        if (_axisX != null) await _axisX.MoveAbsoluteAsync(200);
        if (_axisY != null) await _axisY.MoveAbsoluteAsync(150);

        await Task.Delay(500);

        // Lower Z
        if (_axisZ != null) await _axisZ.MoveAbsoluteAsync(10);
        await Task.Delay(300);

        // Deactivate vacuum
        if (_vacuum != null) await _vacuum.WriteAsync(false);
        await Task.Delay(200);

        // Raise Z
        if (_axisZ != null) await _axisZ.MoveAbsoluteAsync(50);
        await Task.Delay(300);

        // Return home
        if (_axisX != null) await _axisX.MoveAbsoluteAsync(0);
        if (_axisY != null) await _axisY.MoveAbsoluteAsync(0);

        _machineLogger.Information("Place sequence completed");
    }

    public async Task<Result> HomeAllAxesAsync()
    {
        _machineLogger.Information("Homing all axes...");

        if (_axisX != null && !_axisX.IsHomed)
        {
            var result = await _axisX.HomeAsync();
            if (!result.IsSuccess) return result;
        }

        if (_axisY != null && !_axisY.IsHomed)
        {
            var result = await _axisY.HomeAsync();
            if (!result.IsSuccess) return result;
        }

        if (_axisZ != null && !_axisZ.IsHomed)
        {
            var result = await _axisZ.HomeAsync();
            if (!result.IsSuccess) return result;
        }

        return Result.Success("All axes homed");
    }
}
