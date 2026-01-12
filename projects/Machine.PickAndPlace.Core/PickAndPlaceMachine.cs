using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Alarm;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Interlock;
using NAutoSuite.Core.Machine;
using Machine.PickAndPlace.Core.Manual;
using Serilog;

namespace Machine.PickAndPlace.Core;

public class PickAndPlaceMachine : MachineBase
{
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly IAxis? _axisZ;
    private readonly IInput? _partSensor;
    private readonly IOutput? _vacuum;

    /// <summary>
    /// Manual controller for manual mode operations
    /// </summary>
    public ManualController Manual { get; }

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

        // Initialize manual controller
        Manual = new ManualController(_axisX, _axisY, _axisZ, _vacuum, Log.Logger);

        // Setup machine-specific interlocks
        SetupMachineInterlocks();
    }

    private void SetupMachineInterlocks()
    {
        // Add interlock: All axes must be homed before starting
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "AXES_HOMED",
            Description = "All axes must be homed",
            Condition = () => (_axisX?.IsHomed ?? true) &&
                            (_axisY?.IsHomed ?? true) &&
                            (_axisZ?.IsHomed ?? true),
            IsRequired = true
        });

        // Add interlock: No axes in alarm state
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "NO_AXIS_ALARM",
            Description = "No axis in alarm state",
            Condition = () => !(_axisX?.IsInAlarm ?? false) &&
                            !(_axisY?.IsInAlarm ?? false) &&
                            !(_axisZ?.IsInAlarm ?? false),
            IsRequired = true
        });
    }

    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        Log.Logger.Information("Initializing Pick and Place Machine...");

        try
        {
            // Connect axes
            if (_axisX != null)
            {
                var result = await _axisX.ConnectAsync();
                if (!result.IsSuccess)
                {
                    RaiseAlarm(1001, $"Failed to connect Axis X: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            if (_axisY != null)
            {
                var result = await _axisY.ConnectAsync();
                if (!result.IsSuccess)
                {
                    RaiseAlarm(1002, $"Failed to connect Axis Y: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            if (_axisZ != null)
            {
                var result = await _axisZ.ConnectAsync();
                if (!result.IsSuccess)
                {
                    RaiseAlarm(1003, $"Failed to connect Axis Z: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            // Connect I/O
            if (_partSensor != null)
            {
                var result = await _partSensor.ConnectAsync();
                if (!result.IsSuccess)
                {
                    RaiseAlarm(1004, $"Failed to connect part sensor: {result.Message}", AlarmSeverity.Warning);
                }
            }

            if (_vacuum != null)
            {
                var result = await _vacuum.ConnectAsync();
                if (!result.IsSuccess)
                {
                    RaiseAlarm(1005, $"Failed to connect vacuum: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            Log.Logger.Information("Hardware connected successfully");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Hardware initialization failed");
            throw;
        }
    }

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        Log.Logger.Information("Starting Pick and Place cycle {CycleCount}", Context.CycleCount + 1);

        try
        {
            // Pick and place sequence
            await PickSequenceAsync();
            await PlaceSequenceAsync();

            Log.Logger.Information("Cycle {CycleCount} completed successfully", Context.CycleCount + 1);
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Cycle {CycleCount} failed", Context.CycleCount + 1);
            RaiseAlarm(2001, $"Cycle failed: {ex.Message}", AlarmSeverity.Critical);
            throw;
        }
    }

    private async Task PickSequenceAsync()
    {
        Log.Logger.Information("Pick sequence started");

        try
        {
            // Move to pick position
            if (_axisX != null)
            {
                var result = await _axisX.MoveAbsoluteAsync(100);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2101, $"Axis X move failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            if (_axisY != null)
            {
                var result = await _axisY.MoveAbsoluteAsync(50);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2102, $"Axis Y move failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            // Wait for axes to arrive
            await Task.Delay(500);

            // Lower Z
            if (_axisZ != null)
            {
                var result = await _axisZ.MoveAbsoluteAsync(10);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2103, $"Axis Z down failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }
            await Task.Delay(300);

            // Activate vacuum
            if (_vacuum != null)
            {
                var result = await _vacuum.WriteAsync(true);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2104, $"Vacuum activation failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }
            await Task.Delay(200);

            // Raise Z
            if (_axisZ != null)
            {
                var result = await _axisZ.MoveAbsoluteAsync(50);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2105, $"Axis Z up failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }
            await Task.Delay(300);

            Log.Logger.Information("Pick sequence completed");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Pick sequence failed");
            throw;
        }
    }

    private async Task PlaceSequenceAsync()
    {
        Log.Logger.Information("Place sequence started");

        try
        {
            // Move to place position
            if (_axisX != null)
            {
                var result = await _axisX.MoveAbsoluteAsync(200);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2201, $"Axis X move to place failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            if (_axisY != null)
            {
                var result = await _axisY.MoveAbsoluteAsync(150);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2202, $"Axis Y move to place failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            await Task.Delay(500);

            // Lower Z
            if (_axisZ != null)
            {
                var result = await _axisZ.MoveAbsoluteAsync(10);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2203, $"Axis Z down at place failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }
            await Task.Delay(300);

            // Deactivate vacuum
            if (_vacuum != null)
            {
                var result = await _vacuum.WriteAsync(false);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2204, "Vacuum deactivation failed", AlarmSeverity.Warning);
                }
            }
            await Task.Delay(200);

            // Raise Z
            if (_axisZ != null)
            {
                var result = await _axisZ.MoveAbsoluteAsync(50);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2205, $"Axis Z up from place failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }
            await Task.Delay(300);

            // Return home
            if (_axisX != null)
            {
                var result = await _axisX.MoveAbsoluteAsync(0);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2206, $"Axis X return home failed: {result.Message}", AlarmSeverity.Warning);
                }
            }

            if (_axisY != null)
            {
                var result = await _axisY.MoveAbsoluteAsync(0);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2207, $"Axis Y return home failed: {result.Message}", AlarmSeverity.Warning);
                }
            }

            Log.Logger.Information("Place sequence completed");
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Place sequence failed");
            throw;
        }
    }

    public async Task<Result> HomeAllAxesAsync()
    {
        Log.Logger.Information("Homing all axes...");

        if (_axisX != null && !_axisX.IsHomed)
        {
            var result = await _axisX.HomeAsync();
            if (!result.IsSuccess)
            {
                RaiseAlarm(1101, $"Axis X homing failed: {result.Message}", AlarmSeverity.Critical);
                return result;
            }
        }

        if (_axisY != null && !_axisY.IsHomed)
        {
            var result = await _axisY.HomeAsync();
            if (!result.IsSuccess)
            {
                RaiseAlarm(1102, $"Axis Y homing failed: {result.Message}", AlarmSeverity.Critical);
                return result;
            }
        }

        if (_axisZ != null && !_axisZ.IsHomed)
        {
            var result = await _axisZ.HomeAsync();
            if (!result.IsSuccess)
            {
                RaiseAlarm(1103, $"Axis Z homing failed: {result.Message}", AlarmSeverity.Critical);
                return result;
            }
        }

        Log.Logger.Information("All axes homed successfully");
        return Result.Success("All axes homed");
    }
}
