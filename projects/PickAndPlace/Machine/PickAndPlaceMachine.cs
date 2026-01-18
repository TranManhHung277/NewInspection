using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Alarm;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Configuration;
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
    private readonly PickAndPlaceProfile _profile;
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
    private AutoResumeSnapshot? _autoSnapshot;

    public PickAndPlaceMachine(
        IAxis axisX,
        IAxis axisY,
        IAxis axisZ,
        IEtherCATMaster? master = null,
        PickAndPlaceProfile? profile = null,
        ILogger? logger = null)
        : base("PAP_SIM", "PickAndPlace Simulator", profile ?? new PickAndPlaceProfile(), logger)
    {
        _profile = profile ?? new PickAndPlaceProfile();
        _axisX = axisX;
        _axisY = axisY;
        _axisZ = axisZ;
        _master = master;
        _ioMap = (PickAndPlaceIOMap)_profile.IOMap;
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
    }

    protected override void OnRegisterProfileAxes(HardwareManager hardwareManager, IReadOnlyList<AxisDefinition> axes)
    {
        foreach (var axis in axes)
        {
            if (string.Equals(axis.Name, _axisX.Name, StringComparison.OrdinalIgnoreCase))
            {
                hardwareManager.RegisterDevice(_axisX);
            }
            else if (string.Equals(axis.Name, _axisY.Name, StringComparison.OrdinalIgnoreCase))
            {
                hardwareManager.RegisterDevice(_axisY);
            }
            else if (string.Equals(axis.Name, _axisZ.Name, StringComparison.OrdinalIgnoreCase))
            {
                hardwareManager.RegisterDevice(_axisZ);
            }
            else
            {
                _logger.Warning("Axis definition {Name} not matched to hardware", axis.Name);
            }
        }
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
                var step = AutoStep;
                if (string.IsNullOrWhiteSpace(step) || step == "IDLE")
                {
                    step = "WAIT_PART";
                    SetStep(step);
                }

                switch (step)
                {
                    case "WAIT_PART":
                    {
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
                            return;
                        }

                        SetStep("MOVE_TO_PICK");
                        break;
                    }

                    case "MOVE_TO_PICK":
                        await MoveToAsync(_pickX, _pickY, _safeZ, _moveSpeed, ct);
                        await MoveZAsync(_pickZ, _approachSpeed, ct);
                        SetStep("VACUUM_ON");
                        break;

                    case "VACUUM_ON":
                        if (!dryRun)
                        {
                            IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, true);
                        }
                        SetStep("WAIT_VACUUM");
                        break;

                    case "WAIT_VACUUM":
                    {
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
                            return;
                        }

                        await MoveZAsync(_safeZ, _moveSpeed, ct);
                        SetStep("WAIT_TEST_RESULT");
                        break;
                    }

                    case "WAIT_TEST_RESULT":
                    {
                        var testResult = await WaitForTestResultAsync(TimeSpan.FromSeconds(5), dryRun, ct);
                        if (testResult == TestResult.None)
                        {
                            RaiseAlarm(2003, "Test result timeout", AlarmSeverity.Error);
                            IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, false);
                            await Task.Delay(200, ct);
                            return;
                        }

                        SetStep(testResult == TestResult.Ok ? "PLACE_OK" : "PLACE_NG");
                        break;
                    }

                    case "PLACE_OK":
                        await MoveToAsync(_placeOkX, _placeOkY, _safeZ, _moveSpeed, ct);
                        await MoveZAsync(_placeZ, _approachSpeed, ct);
                        IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, false);
                        await Task.Delay(200, ct);
                        await MoveZAsync(_safeZ, _moveSpeed, ct);
                        SetStep("RETURN_HOME");
                        break;

                    case "PLACE_NG":
                    {
                        await MoveToAsync(_placeNgX, _placeNgY, _safeZ, _moveSpeed, ct);
                        await MoveZAsync(_placeZ, _approachSpeed, ct);
                        IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, false);
                        await Task.Delay(200, ct);
                        var extendOk = await _rejectCylinder.ExtendAsync(800, ct);
                        if (!extendOk)
                        {
                            RaiseAlarm(2004, "Cylinder extend timeout", AlarmSeverity.Error);
                            return;
                        }
                        await Task.Delay(200, ct);
                        var retractOk = await _rejectCylinder.RetractAsync(800, ct);
                        if (!retractOk)
                        {
                            RaiseAlarm(2005, "Cylinder retract timeout", AlarmSeverity.Error);
                            return;
                        }
                        await MoveZAsync(_safeZ, _moveSpeed, ct);
                        SetStep("RETURN_HOME");
                        break;
                    }

                    case "RETURN_HOME":
                        await MoveToAsync(0, 0, _safeZ, _moveSpeed, ct);
                        await MoveZAsync(0, _moveSpeed, ct);
                        SetStep("WAIT_PART");
                        break;

                    default:
                        SetStep("WAIT_PART");
                        break;
                }
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

        MarkAutoRestoreComplete();
        return Result.Success("All axes homed");
    }

    public override Task<Result> HomeAsync(CancellationToken cancellationToken = default)
    {
        if (RunMode != MachineRunMode.Manual)
        {
            return Task.FromResult(Result.Failure("Home only allowed in Manual mode"));
        }

        return HomeAllAxesAsync();
    }

    public void CaptureAutoSnapshot()
    {
        _autoSnapshot = new AutoResumeSnapshot
        {
            X = _axisX.Position,
            Y = _axisY.Position,
            Z = _axisZ.Position,
            CylinderExtend = IO.ReadOutput(_ioMap.MachineOutputs.CylinderExtend),
            VacuumOn = IO.ReadOutput(_ioMap.MachineOutputs.VacuumOn),
            Step = AutoStep
        };
    }

    public async Task<Result> RestoreAutoSnapshotAsync(CancellationToken ct = default)
    {
        if (_autoSnapshot == null)
        {
            return Result.Failure("No auto snapshot available");
        }

        if (State == MachineState.Running)
        {
            return Result.Failure("Cannot restore while running");
        }

        IO.WriteOutput(_ioMap.MachineOutputs.CylinderExtend, _autoSnapshot.CylinderExtend);
        IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, _autoSnapshot.VacuumOn);
        if (!string.IsNullOrWhiteSpace(_autoSnapshot.Step))
        {
            SetStep(_autoSnapshot.Step);
        }

        await MoveToAsync(_autoSnapshot.X, _autoSnapshot.Y, _autoSnapshot.Z, _moveSpeed, ct);
        MarkAutoRestoreComplete();
        return Result.Success("Auto snapshot restored");
    }

    private sealed class AutoResumeSnapshot
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public bool CylinderExtend { get; set; }
        public bool VacuumOn { get; set; }
        public string Step { get; set; } = string.Empty;
    }

    public double AxisXPosition => _axisX.Position;
    public double AxisYPosition => _axisY.Position;
    public double AxisZPosition => _axisZ.Position;
    public bool IsAxisMoving => _axisX.IsMoving || _axisY.IsMoving || _axisZ.IsMoving;
    public bool AxisXIsMoving => _axisX.IsMoving;
    public bool AxisXIsHomed => _axisX.IsHomed;
    public bool AxisXInAlarm => _axisX.IsInAlarm;
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

    #region Manual Actions

    public Task<Result> JogXAsync(double distance, double speed, CancellationToken ct = default)
        => _axisX.MoveRelativeAsync(distance, speed, ct);

    public Task<Result> JogYAsync(double distance, double speed, CancellationToken ct = default)
        => _axisY.MoveRelativeAsync(distance, speed, ct);

    public Task<Result> JogZAsync(double distance, double speed, CancellationToken ct = default)
        => _axisZ.MoveRelativeAsync(distance, speed, ct);

    public Task<Result> MoveAxisXAsync(double position, double speed, CancellationToken ct = default)
        => _axisX.MoveAbsoluteAsync(position, speed, ct);

    public Task<Result> HomeAxisXAsync(CancellationToken ct = default)
        => _axisX.HomeAsync(ct);

    public Task<Result> HomeAxisYAsync(CancellationToken ct = default)
        => _axisY.HomeAsync(ct);

    public Task<Result> HomeAxisZAsync(CancellationToken ct = default)
        => _axisZ.HomeAsync(ct);

    public async Task<Result> MoveToPickAsync(CancellationToken ct = default)
    {
        await MoveToAsync(_pickX, _pickY, _safeZ, _moveSpeed, ct);
        return Result.Success("Moved to pick");
    }

    public async Task<Result> MoveToPlaceAsync(CancellationToken ct = default)
    {
        await MoveToAsync(_placeOkX, _placeOkY, _safeZ, _moveSpeed, ct);
        return Result.Success("Moved to place");
    }

    public async Task<Result> MoveToHomeAsync(CancellationToken ct = default)
    {
        await MoveToAsync(0, 0, _safeZ, _moveSpeed, ct);
        return Result.Success("Moved to home");
    }

    public async Task<Result> StopAllAxesAsync(CancellationToken ct = default)
    {
        await _axisX.StopAsync(false, ct);
        await _axisY.StopAsync(false, ct);
        await _axisZ.StopAsync(false, ct);
        return Result.Success("Stop requested");
    }

    public void SetVacuum(bool on)
    {
        IO.WriteOutput(_ioMap.MachineOutputs.VacuumOn, on);
    }

    #endregion
}
