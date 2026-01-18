using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Alarm;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Devices;
using NAutoSuite.Core.Hardware;
using NAutoSuite.Core.Interlock;
using NAutoSuite.Core.IO;
using NAutoSuite.Core.Machine;
using NAutoSuite.Hardware.Abstractions.EtherCAT;
using Serilog;

namespace PickAndPlace.Machine;

/// <summary>
/// PickAndPlace sample with 3 axes and simulated EtherCAT IO.
/// </summary>
public class PickAndPlaceMachine : MachineBase
{
    private readonly IAxis _axisX;
    private readonly IAxis _axisY;
    private readonly IAxis _axisZ;
    private readonly IEtherCATMaster? _master;
    private readonly PickAndPlaceIOMap _ioMap;
    private readonly Cylinder _rejectCylinder;
    private readonly SensorWaiter _sensorWaiter;

    private readonly double _safeZ = 60;
    private readonly double _pickX = 120;
    private readonly double _pickY = 40;
    private readonly double _pickZ = 5;
    private readonly double _placeOkX = 220;
    private readonly double _placeOkY = 60;
    private readonly double _placeNgX = 260;
    private readonly double _placeNgY = 80;
    private readonly double _placeZ = 5;
    private readonly double _moveSpeed = 100;
    private readonly double _approachSpeed = 40;
    private Task? _autoTask;

    public PickAndPlaceMachine(
        IAxis axisX,
        IAxis axisY,
        IAxis axisZ,
        IEtherCATMaster? master = null,
        ILogger? logger = null)
        : base("PAP_SIM", "PickAndPlace Simulator", logger)
    {
        _axisX = axisX;
        _axisY = axisY;
        _axisZ = axisZ;
        _master = master;
        _ioMap = new PickAndPlaceIOMap();
        _rejectCylinder = new Cylinder(
            IO,
            _ioMap.MachineOutputs.CylinderExtend,
            _ioMap.MachineInputs.CylinderExtended,
            _ioMap.MachineInputs.CylinderRetracted,
            logger);
        _sensorWaiter = new SensorWaiter(logger);

        SetupMachineInterlocks();
    }

    private void SetupMachineInterlocks()
    {
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "AXES_HOMED",
            Description = "All axes must be homed",
            Condition = () => _axisX.IsHomed && _axisY.IsHomed && _axisZ.IsHomed,
            IsRequired = true
        });

        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "NO_AXIS_ALARM",
            Description = "No axis in alarm state",
            Condition = () => !_axisX.IsInAlarm && !_axisY.IsInAlarm && !_axisZ.IsInAlarm,
            IsRequired = true
        });
    }

    #region MachineBase Overrides

    protected override IOMap? GetIOMap() => _ioMap;

    protected override void OnRegisterHardware(HardwareManager hardwareManager)
    {
        if (_master != null)
        {
            hardwareManager.RegisterDevice(_master);
        }

        hardwareManager.RegisterDevice(_axisX);
        hardwareManager.RegisterDevice(_axisY);
        hardwareManager.RegisterDevice(_axisZ);
    }

    protected override Task OnHardwareConnectFailedAsync(Result result)
    {
        RaiseAlarm(1001, $"Failed to connect hardware: {result.Message}", AlarmSeverity.Critical);
        return Task.CompletedTask;
    }

    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        await _axisX.SetServoAsync(true);
        await _axisY.SetServoAsync(true);
        await _axisZ.SetServoAsync(true);
    }

    #endregion

    #region AUTO Logic

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        if (_autoTask is { IsCompleted: false })
        {
            await Task.WhenAny(_autoTask, Task.Delay(200));
            if (_autoTask is { IsCompleted: false })
            {
                _logger.Warning("Auto loop still running, skip starting a new one");
                return;
            }
        }

        _autoTask = Task.Run(() => RunAutoLoopAsync(RunCancellationToken));
    }

    private async Task RunAutoLoopAsync(CancellationToken ct)
    {
        try
        {
            while (State == MachineState.Running && !ct.IsCancellationRequested)
            {
                var dryRun = RunMode == MachineRunMode.DryRun;

                SetStep("WAIT_PART");
                var hasPart = dryRun || await _sensorWaiter.WaitForSensorAsync(
                    "PartPresent",
                    () => Task.FromResult(IO.ReadInput(_ioMap.MachineInputs.PartPresent)),
                    timeoutMs: 10000,
                    ct);

                if (State != MachineState.Running || ct.IsCancellationRequested)
                    break;

                if (!hasPart)
                {
                    RaiseAlarm(2001, "Timeout waiting for part", AlarmSeverity.Error);
                    await Task.Delay(200, ct);
                    break;
                }

                SetStep("MOVE_TO_PICK");
                await MoveToAsync(_pickX, _pickY, _safeZ, _moveSpeed, ct);
                await MoveZAsync(_pickZ, _approachSpeed, ct);

                SetStep("VACUUM_ON");
                if (!dryRun)
                {
                    IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, true);
                }

                var vacuumOk = dryRun || (await WaitForInputOnAsync(
                    _ioMap.MachineInputs.VacuumOk,
                    timeoutMs: 1500,
                    errorMessage: "Vacuum not detected",
                    ct: ct)).IsSuccess;

                if (State != MachineState.Running || ct.IsCancellationRequested)
                    break;

                if (!vacuumOk)
                {
                    RaiseAlarm(2002, "Vacuum not detected", AlarmSeverity.Error);
                    IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, false);
                    await Task.Delay(200, ct);
                    break;
                }

                await MoveZAsync(_safeZ, _moveSpeed, ct);

                SetStep("WAIT_TEST_RESULT");
                var testResult = await WaitForTestResultAsync(TimeSpan.FromSeconds(5), dryRun, ct);
                if (testResult == TestResult.None)
                {
                    RaiseAlarm(2003, "Test result timeout", AlarmSeverity.Error);
                    IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, false);
                    await Task.Delay(200, ct);
                    break;
                }

                if (testResult == TestResult.Ok)
                {
                    SetStep("PLACE_OK");
                    await MoveToAsync(_placeOkX, _placeOkY, _safeZ, _moveSpeed, ct);
                    await MoveZAsync(_placeZ, _approachSpeed, ct);
                    IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, false);
                    await Task.Delay(200, ct);
                    await MoveZAsync(_safeZ, _moveSpeed, ct);
                }
                else
                {
                    SetStep("PLACE_NG");
                    await MoveToAsync(_placeNgX, _placeNgY, _safeZ, _moveSpeed, ct);
                    await MoveZAsync(_placeZ, _approachSpeed, ct);
                    IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, false);
                    await Task.Delay(200, ct);
                    var extendOk = await _rejectCylinder.ExtendAsync(800, ct);
                    if (!extendOk)
                    {
                        RaiseAlarm(2004, "Cylinder extend timeout", AlarmSeverity.Error);
                        break;
                    }
                    await Task.Delay(200, ct);
                    var retractOk = await _rejectCylinder.RetractAsync(800, ct);
                    if (!retractOk)
                    {
                        RaiseAlarm(2005, "Cylinder retract timeout", AlarmSeverity.Error);
                        break;
                    }
                    await MoveZAsync(_safeZ, _moveSpeed, ct);
                }

                SetStep("RETURN_HOME");
                await MoveToAsync(0, 0, _safeZ, _moveSpeed, ct);
                await MoveZAsync(0, _moveSpeed, ct);
                SetStep("IDLE");
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Auto loop canceled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Auto loop error");
            RaiseAlarm(9001, $"Auto loop error: {ex.Message}", AlarmSeverity.Critical);
        }
    }

    #endregion

    #region Public Methods

    public async Task<Result> HomeAllAxesAsync()
    {
        var resultX = await _axisX.HomeAsync();
        if (!resultX.IsSuccess) return resultX;

        var resultY = await _axisY.HomeAsync();
        if (!resultY.IsSuccess) return resultY;

        var resultZ = await _axisZ.HomeAsync();
        if (!resultZ.IsSuccess) return resultZ;

        return Result.Success("All axes homed");
    }

    public double AxisXPosition => _axisX.Position;
    public double AxisYPosition => _axisY.Position;
    public double AxisZPosition => _axisZ.Position;
    public bool IsAxisMoving => _axisX.IsMoving || _axisY.IsMoving || _axisZ.IsMoving;
    public PickAndPlaceIOMap IOMap => _ioMap;

    public string AutoStep => Context.Data.TryGetValue("AutoStep", out var step)
        ? step?.ToString() ?? string.Empty
        : string.Empty;

    #endregion

    private void SetStep(string step)
    {
        Context.Data["AutoStep"] = step;
        _logger.Information("AUTO STEP: {Step}", step);
    }

    private async Task MoveToAsync(double x, double y, double z, double speed, CancellationToken ct)
    {
        await _axisX.MoveAbsoluteAsync(x, speed, ct);
        await _axisY.MoveAbsoluteAsync(y, speed, ct);
        await _axisZ.MoveAbsoluteAsync(z, speed, ct);
    }

    private async Task MoveZAsync(double z, double speed, CancellationToken ct)
    {
        await _axisZ.MoveAbsoluteAsync(z, speed, ct);
    }

    private async Task<TestResult> WaitForTestResultAsync(TimeSpan timeout, bool dryRun, CancellationToken ct)
    {
        if (dryRun)
        {
            await Task.Delay(200, ct);
            return TestResult.Ok;
        }

        var start = DateTime.UtcNow;
        while (DateTime.UtcNow - start < timeout && !ct.IsCancellationRequested)
        {
            if (IO.ReadInput(_ioMap.MachineInputs.TestOk))
            {
                return TestResult.Ok;
            }

            if (IO.ReadInput(_ioMap.MachineInputs.TestNg))
            {
                return TestResult.Ng;
            }

            await Task.Delay(100, ct);
        }

        return TestResult.None;
    }

    private enum TestResult
    {
        None,
        Ok,
        Ng
    }
}
