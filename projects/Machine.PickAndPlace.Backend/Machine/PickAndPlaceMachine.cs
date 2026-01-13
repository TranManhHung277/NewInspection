using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Alarm;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Interlock;
using NAutoSuite.Core.Machine;
using NAutoSuite.Hardware.Abstractions.PLC;
using PickAndPlace.Backend.Manual;
using Serilog;

namespace PickAndPlace.Backend.Machine;

public class PickAndPlaceMachine : MachineBase
{
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly IAxis? _axisZ;
    private readonly IInput? _partSensor;
    private readonly IOutput? _vacuum;

    // IO Map and Data
    private readonly PickAndPlaceIOMap _ioMap;
    private readonly PickAndPlaceData _data;
    private readonly IPlc? _plc;

    // Sensor Waiter - Chờ sensor không block
    private readonly SensorWaiter _sensorWaiter;

    /// <summary>
    /// Manual controller for manual mode operations
    /// </summary>
    public ManualController Manual { get; }

    /// <summary>
    /// IO Map - Danh sách địa chỉ IO
    /// </summary>
    public PickAndPlaceIOMap IOMap => _ioMap;

    /// <summary>
    /// Machine Data - Cài đặt và thông số
    /// </summary>
    public PickAndPlaceData Data => _data;

    public PickAndPlaceMachine(
        string id,
        string name,
        IAxis? axisX = null,
        IAxis? axisY = null,
        IAxis? axisZ = null,
        IInput? partSensor = null,
        IOutput? vacuum = null,
        IPlc? plc = null,
        ILogger? logger = null)
        : base(id, name, logger)
    {
        _axisX = axisX;
        _axisY = axisY;
        _axisZ = axisZ;
        _partSensor = partSensor;
        _vacuum = vacuum;
        _plc = plc;

        // Initialize IO Map and Data
        _ioMap = new PickAndPlaceIOMap();
        _data = new PickAndPlaceData();

        // Initialize manual controller
        Manual = new ManualController(_axisX, _axisY, _axisZ, _vacuum, _logger);

        // Initialize Sensor Waiter
        _sensorWaiter = new SensorWaiter(_logger);

        // Setup machine-specific interlocks
        SetupMachineInterlocks();
    }

    #region MachineBase Overrides - REQUIRED

    /// <summary>
    /// Provide IO Map for automatic IO scanning by MachineBase
    /// </summary>
    protected override NAutoSuite.Core.IO.IOMap? GetIOMap() => _ioMap;

    /// <summary>
    /// Handle reading inputs from PLC
    /// Called automatically by MachineBase's CommonIOHandler
    /// </summary>
    protected override async Task<bool> OnReadInputAsync(string address, CancellationToken ct)
    {
        try
        {
            if (_plc == null) return false;
            var result = await _plc.ReadBitAsync(address, ct);
            return result.IsSuccess && result.Value;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read input {Address}", address);
            return false;
        }
    }

    /// <summary>
    /// Handle writing outputs to PLC
    /// Called automatically by MachineBase's CommonIOHandler
    /// </summary>
    protected override async Task OnWriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        try
        {
            if (_plc == null) return;
            await _plc.WriteBitAsync(address, value, ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write output {Address}", address);
        }
    }

    /// <summary>
    /// Register additional background tasks (optional)
    /// Example: Feeder monitor, vision processing, etc.
    /// </summary>
    protected override void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
    {
        // Example: Register feeder monitor task that runs every 50ms
        // taskManager.RegisterTask("FeederMonitor", async ct => {
        //     bool partDetected = await OnReadInputAsync(_ioMap.MachineInputs.PartSensorAtPick, ct);
        //     if (partDetected) {
        //         // Handle part detection
        //     }
        // }, intervalMs: 50);
    }

    #endregion

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

        _logger.Information("Initializing Pick and Place Machine...");

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

            _logger.Information("Hardware connected successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Hardware initialization failed");
            throw;
        }
    }

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        _logger.Information("Starting Pick and Place cycle {CycleCount}", Context.CycleCount + 1);

        try
        {
            // Pick and place sequence
            await PickSequenceAsync();
            await PlaceSequenceAsync();

            _logger.Information("Cycle {CycleCount} completed successfully", Context.CycleCount + 1);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cycle {CycleCount} failed", Context.CycleCount + 1);
            RaiseAlarm(2001, $"Cycle failed: {ex.Message}", AlarmSeverity.Critical);
            throw;
        }
    }

    private async Task PickSequenceAsync()
    {
        _logger.Information("Pick sequence started");

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

            // ✅ DEMO: Chờ XY đến vị trí (dùng Task.Delay thay vì Thread.Sleep để không block)
            // Trong production: dùng SensorWaiter với real sensor
            await Task.Delay(500);

            // Lower Z
            if (_axisZ != null)
            {
                var result = await _axisZ.MoveAbsoluteAsync(10);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2104, $"Axis Z down failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            // ✅ DEMO: Chờ Z xuống - dùng Task.Delay (không block scan cycle)
            await Task.Delay(300);

            // Activate vacuum
            if (_vacuum != null)
            {
                var result = await _vacuum.WriteAsync(true);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2106, $"Vacuum activation failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            // ✅ DEMO: Chờ vacuum - SensorWaiter KHÔNG block scan cycle!
            // Trong khi chờ 1 giây, EMG/Start/Stop/Reset vẫn hoạt động bình thường
            bool hasVacuum = await _sensorWaiter!.WaitForSensorAsync(
                sensorName: "Vacuum Sensor",
                readFunc: async () => await ReadInputAsync(_ioMap.MachineInputs.VacuumSensor, default),
                timeoutMs: 1000,
                ct: default
            );

            if (!hasVacuum)
            {
                RaiseAlarm(2107, "Vacuum pressure not detected", AlarmSeverity.Critical);
                throw new Exception("Vacuum sensor timeout");
            }

            // Raise Z
            if (_axisZ != null)
            {
                var result = await _axisZ.MoveAbsoluteAsync(50);
                if (!result.IsSuccess)
                {
                    RaiseAlarm(2108, $"Axis Z up failed: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
            }

            // ✅ DEMO: Chờ Z lên - dùng Task.Delay (không block scan cycle)
            await Task.Delay(300);

            _logger.Information("Pick sequence completed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Pick sequence failed");
            throw;
        }
    }

    private async Task PlaceSequenceAsync()
    {
        _logger.Information("Place sequence started");

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

            _logger.Information("Place sequence completed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Place sequence failed");
            throw;
        }
    }

    public async Task<Result> HomeAllAxesAsync()
    {
        _logger.Information("Homing all axes...");

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

        _logger.Information("All axes homed successfully");
        return Result.Success("All axes homed");
    }
}
