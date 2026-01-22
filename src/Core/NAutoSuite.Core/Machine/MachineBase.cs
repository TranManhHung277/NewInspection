using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Alarm;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Interlock;
using NAutoSuite.Core.IO;
using NAutoSuite.Core.Hardware;
using Serilog;
using Stateless;

namespace NAutoSuite.Core.Machine;

/// <summary>
/// Base class for all machines with integrated:
/// - Alarm and interlock management
/// - Hardware registry for device connections
/// - Automatic IO scan cycle with cached IO image (EMG, Start, Stop, Reset, Tower Lights)
/// - Background task manager
/// - State machine
///
/// NEW PROJECTS ONLY NEED TO:
/// 1. Inherit from MachineBase
/// 2. Override OnRunningAsync() to write AUTO logic
/// 3. Done! All common IO, buttons, lights are handled automatically
/// </summary>
public abstract class MachineBase : IMachine
{
    private readonly StateMachine<MachineState, MachineTrigger> _stateMachine;
    protected readonly ILogger _logger;
    private readonly HardwareManager _hardwareManager;
    private readonly IOImage _ioImage;
    private readonly MachineIO _machineIO;
    private readonly RegisterImage _registerImage;
    private readonly MachineRegisters _machineRegisters;
    private IOScanService? _ioScanService;
    private RegisterScanService? _registerScanService;
    private bool _hardwareRegistered;
    private CancellationTokenSource? _runCts;
    private readonly SensorWaiter _sensorWaiter;
    private readonly MachineProfile? _profile;
    private bool _autoRestoreRequired;

    public string Id { get; }
    public string Name { get; }
    public MachineState State => _stateMachine.State;
    public MachineContext Context { get; }
    public MachineIO IO => _machineIO;
    public MachineRegisters Reg => _machineRegisters;
    public RegisterMap? Registers => _profile?.RegisterMap;
    public IReadOnlyList<AxisDefinition>? ProfileAxes => _profile?.Axes;
    protected HardwareManager Hardware => _hardwareManager;
    public MachineRunMode RunMode { get; private set; } = MachineRunMode.Auto;
    protected CancellationToken RunCancellationToken => _runCts?.Token ?? CancellationToken.None;

    /// <summary>
    /// Alarm manager for handling machine alarms
    /// </summary>
    protected readonly AlarmManager AlarmManager;

    /// <summary>
    /// Interlock manager for checking start conditions
    /// </summary>
    protected readonly InterlockManager InterlockManager;

    /// <summary>
    /// Background task manager - runs multiple tasks in parallel
    /// </summary>
    private BackgroundTaskManager? _backgroundTaskManager;

    /// <summary>
    /// Common IO handler - automatically handles EMG/Start/Stop/Reset/Tower Lights
    /// </summary>
    private CommonIOHandler? _commonIOHandler;

    /// <summary>
    /// Main loop cancellation token source
    /// </summary>
    private CancellationTokenSource? _mainLoopCts;

    /// <summary>
    /// Whether machine has any active alarms
    /// </summary>
    public bool HasActiveAlarms => AlarmManager.HasActiveAlarms;

    /// <summary>
    /// Gets all active alarms
    /// </summary>
    public IEnumerable<Alarm.Alarm> ActiveAlarms => AlarmManager.ActiveAlarms;

    public event EventHandler<MachineState>? StateChanged;

    protected MachineBase(string id, string name, MachineProfile? profile = null, ILogger? logger = null)
    {
        Guard.AgainstNullOrEmpty(id, nameof(id));
        Guard.AgainstNullOrEmpty(name, nameof(name));

        Id = id;
        Name = name;
        _profile = profile;
        Context = new MachineContext();
        _logger = logger ?? Log.Logger;
        _hardwareManager = new HardwareManager();
        _ioImage = new IOImage();
        _machineIO = new MachineIO(_ioImage);
        _registerImage = new RegisterImage();
        _machineRegisters = new MachineRegisters(_registerImage);
        _sensorWaiter = new SensorWaiter(_logger);

        // Initialize alarm and interlock managers
        AlarmManager = new AlarmManager(_logger);
        AlarmManager.AlarmRaised += OnAlarmRaised;
        AlarmManager.AlarmCleared += OnAlarmCleared;

        InterlockManager = new InterlockManager(_logger);

        _stateMachine = new StateMachine<MachineState, MachineTrigger>(MachineState.Uninitialized);
        ConfigureStateMachine();

        // Setup default interlocks
        SetupDefaultInterlocks();
    }

    private void ConfigureStateMachine()
    {
        _stateMachine.Configure(MachineState.Uninitialized)
            .Permit(MachineTrigger.Initialize, MachineState.Initializing);

        _stateMachine.Configure(MachineState.Initializing)
            .OnEntryAsync(OnInitializingInternalAsync)
            .Permit(MachineTrigger.Complete, MachineState.Idle)
            .Permit(MachineTrigger.Error, MachineState.Error)
            .Permit(MachineTrigger.Reset, MachineState.Uninitialized);

        _stateMachine.Configure(MachineState.Idle)
            .Permit(MachineTrigger.Start, MachineState.Running)
            .Permit(MachineTrigger.Error, MachineState.Error)
            .Permit(MachineTrigger.Reset, MachineState.Uninitialized);

        _stateMachine.Configure(MachineState.Running)
            .OnEntryAsync(OnRunningAsync)
            .OnExitAsync(OnExitRunningAsync)
            .Permit(MachineTrigger.Stop, MachineState.Stopping)
            .Permit(MachineTrigger.Pause, MachineState.Paused)
            .Permit(MachineTrigger.Error, MachineState.Error)
            .Permit(MachineTrigger.EmergencyStop, MachineState.EmergencyStop)
            .Permit(MachineTrigger.Complete, MachineState.Idle)
            .Permit(MachineTrigger.CancelAuto, MachineState.Idle);

        _stateMachine.Configure(MachineState.Paused)
            .Permit(MachineTrigger.Resume, MachineState.Running)
            .Permit(MachineTrigger.Error, MachineState.Error)
            .Permit(MachineTrigger.Reset, MachineState.Idle)
            .Permit(MachineTrigger.Stop, MachineState.Stopping);

        _stateMachine.Configure(MachineState.Stopping)
            .OnEntryAsync(OnStoppingAsync)
            .Permit(MachineTrigger.Error, MachineState.Error)
            .Permit(MachineTrigger.Complete, MachineState.Stopped);

        _stateMachine.Configure(MachineState.Stopped)
            .Permit(MachineTrigger.Reset, MachineState.Idle)
            .Permit(MachineTrigger.Error, MachineState.Error);

        _stateMachine.Configure(MachineState.Error)
            .Permit(MachineTrigger.ClearError, MachineState.Idle)
            .Permit(MachineTrigger.Reset, MachineState.Idle);

        _stateMachine.Configure(MachineState.EmergencyStop)
            .OnEntryAsync(OnEmergencyStopAsync)
            .Permit(MachineTrigger.Reset, MachineState.Uninitialized);

        _stateMachine.OnTransitioned(transition =>
        {
            _logger.Information("Machine {Name} transitioned from {Source} to {Destination} via {Trigger}",
                Name, transition.Source, transition.Destination, transition.Trigger);
            StateChanged?.Invoke(this, transition.Destination);
        });
    }

    public virtual async Task<Result> InitializeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _stateMachine.FireAsync(MachineTrigger.Initialize);

            // Start the main loop (IO scan cycle)
            StartMainLoop();

            await _stateMachine.FireAsync(MachineTrigger.Complete);
            return Result.Success("Machine initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize machine {Name}", Name);
            if (_stateMachine.CanFire(MachineTrigger.Reset))
            {
                await _stateMachine.FireAsync(MachineTrigger.Reset);
            }
            return Result.Failure("Initialization failed", ex);
        }
    }

    /// <summary>
    /// Start the main loop - automatically scans IO and handles buttons/lights
    /// Called automatically in InitializeAsync()
    /// </summary>
    protected void StartMainLoop()
    {
        if (_mainLoopCts != null)
        {
            _logger.Warning("Main loop already running");
            return;
        }

        // Setup BackgroundTaskManager
        _backgroundTaskManager = new BackgroundTaskManager(_logger);

        // Get IO Map from derived class
        var ioMap = GetIOMap();
        var registerMap = _profile?.RegisterMap;
        var hasRegisters = registerMap != null &&
            (registerMap.InputRegisters.Count > 0 || registerMap.OutputRegisters.Count > 0);
        if (ioMap == null)
        {
            _logger.Warning("No IO Map provided - Main loop will not handle common IO");

            if (_hardwareManager.DataProviders.Count > 0)
            {
                _backgroundTaskManager.RegisterTask("HardwareDataScan", async ct =>
                {
                    await _hardwareManager.UpdateDataAsync(ct);
                }, intervalMs: 100);
            }
        }
        else
        {
            _ioScanService = new IOScanService(ioMap, _hardwareManager, _ioImage, _profile?.DefaultIoValues);
            _commonIOHandler = new CommonIOHandler(
                machine: this,
                ioMap: ioMap,
                ioImage: _ioImage,
                logger: _logger
            );

            // Register IO scan task (100ms cycle)
            _backgroundTaskManager.RegisterTask("IOSignalScan", async ct =>
            {
                await _ioScanService!.UpdateInputsAsync(ct);
                await _hardwareManager.UpdateDataAsync(ct);
                await _commonIOHandler!.ScanAsync(ct);
                await _ioScanService!.FlushOutputsAsync(ct);
            }, intervalMs: 100);
        }

        if (hasRegisters && registerMap != null)
        {
            _registerScanService = new RegisterScanService(registerMap, _hardwareManager, _registerImage);
            _backgroundTaskManager.RegisterTask("RegisterScan", async ct =>
            {
                await _registerScanService!.UpdateInputsAsync(ct);
                await _registerScanService!.FlushOutputsAsync(ct);
            }, intervalMs: 100);
        }

        // Allow derived classes to register additional background tasks
        OnRegisterBackgroundTasks(_backgroundTaskManager);

        // Start all background tasks
        _mainLoopCts = new CancellationTokenSource();
        _backgroundTaskManager.StartAll();

        _logger.Information("Main loop started - IO scan cycle active");
    }

    /// <summary>
    /// Stop the main loop
    /// Called automatically in Dispose
    /// </summary>
    protected void StopMainLoop()
    {
        if (_mainLoopCts == null) return;

        _logger.Information("Stopping main loop...");

        _backgroundTaskManager?.StopAll();
        _mainLoopCts?.Cancel();
        _mainLoopCts?.Dispose();
        _mainLoopCts = null;

        _logger.Information("Main loop stopped");
    }

    public virtual async Task<Result> StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (RunMode == MachineRunMode.Manual)
            {
                return Result.Failure("Cannot start auto cycle in Manual mode");
            }

            if (_autoRestoreRequired)
            {
                return Result.Failure("Please Home/Restore before starting Auto");
            }

            if (State == MachineState.Paused)
            {
                if (_hardwareManager.Devices.Count > 0 &&
                    _hardwareManager.Devices.Any(device => !device.IsConnected))
                {
                    return Result.Failure("Hardware not connected. Please initialize.");
                }

                var (canResume, failedResumeConditions) = InterlockManager.CheckStartConditions();
                if (!canResume)
                {
                    var message = $"Cannot start: {string.Join(", ", failedResumeConditions)}";
                    _logger.Warning("Start interlock failed for machine {Name}: {Message}", Name, message);
                    return Result.Failure(message);
                }

                return await ResumeAsync(cancellationToken);
            }

            if (!_stateMachine.CanFire(MachineTrigger.Start))
                return Result.Failure($"Cannot start from state {State}");

            if (_hardwareManager.Devices.Count > 0 &&
                _hardwareManager.Devices.Any(device => !device.IsConnected))
            {
                return Result.Failure("Hardware not connected. Please initialize.");
            }

            // Check interlock conditions before starting
            var (canStart, failedConditions) = InterlockManager.CheckStartConditions();
            if (!canStart)
            {
                var message = $"Cannot start: {string.Join(", ", failedConditions)}";
                _logger.Warning("Start interlock failed for machine {Name}: {Message}", Name, message);
                return Result.Failure(message);
            }

            await _stateMachine.FireAsync(MachineTrigger.Start);
            return Result.Success("Machine started");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start machine {Name}", Name);
            return Result.Failure("Start failed", ex);
        }
    }

    public virtual async Task<Result> StopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _runCts?.Cancel();

            if (State == MachineState.Running && _stateMachine.CanFire(MachineTrigger.Pause))
            {
                await _stateMachine.FireAsync(MachineTrigger.Pause);
                return Result.Success("Machine paused");
            }

            if (!_stateMachine.CanFire(MachineTrigger.Stop))
                return Result.Failure($"Cannot stop from state {State}");

            await _stateMachine.FireAsync(MachineTrigger.Stop);
            await _stateMachine.FireAsync(MachineTrigger.Complete);
            return Result.Success("Machine stopped");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to stop machine {Name}", Name);
            return Result.Failure("Stop failed", ex);
        }
    }

    public virtual async Task<Result> PauseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_stateMachine.CanFire(MachineTrigger.Pause))
                return Result.Failure($"Cannot pause from state {State}");

            await _stateMachine.FireAsync(MachineTrigger.Pause);
            return Result.Success("Machine paused");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to pause machine {Name}", Name);
            return Result.Failure("Pause failed", ex);
        }
    }

    public virtual async Task<Result> ResumeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_stateMachine.CanFire(MachineTrigger.Resume))
                return Result.Failure($"Cannot resume from state {State}");

            await _stateMachine.FireAsync(MachineTrigger.Resume);
            return Result.Success("Machine resumed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to resume machine {Name}", Name);
            return Result.Failure("Resume failed", ex);
        }
    }

    public virtual async Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (State == MachineState.Running)
            {
                return Result.Success("Reset ignored while running");
            }

            if (!HasActiveAlarms)
            {
                return Result.Success("Reset ignored (no active alarms)");
            }

            if (State == MachineState.Uninitialized)
            {
                return Result.Success("Reset ignored (already uninitialized)");
            }

            Context.Reset();
            ClearAllAlarms();
            await _stateMachine.FireAsync(MachineTrigger.Reset);
            return Result.Success("Machine reset");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to reset machine {Name}", Name);
            return Result.Failure("Reset failed", ex);
        }
    }

    public virtual async Task<Result> EmergencyStopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _runCts?.Cancel();
            await _stateMachine.FireAsync(MachineTrigger.EmergencyStop);
            return Result.Success("Emergency stop executed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to execute emergency stop for machine {Name}", Name);
            return Result.Failure("Emergency stop failed", ex);
        }
    }

    protected virtual Task OnInitializingAsync()
    {
        _logger.Information("Initializing machine {Name}", Name);
        return Task.CompletedTask;
    }

    public void ReloadIoScan()
    {
        StopMainLoop();
        StartMainLoop();
    }

    private async Task OnInitializingInternalAsync()
    {
        RegisterHardwareIfNeeded();

        var connectResult = await _hardwareManager.ConnectAllAsync();
        if (!connectResult.IsSuccess)
        {
            await OnHardwareConnectFailedAsync(connectResult);
            throw new InvalidOperationException(connectResult.Message);
        }

        await OnInitializingAsync();
    }

    private void RegisterHardwareIfNeeded()
    {
        if (_hardwareRegistered) return;

        OnRegisterHardware(_hardwareManager);

        if (_profile?.Axes is { Count: > 0 })
        {
            OnRegisterProfileAxes(_hardwareManager, _profile.Axes);
        }
        _hardwareRegistered = true;

        if (_hardwareManager.IODevices.Count == 0)
        {
            _hardwareManager.RegisterIO(new MachineIoAdapter(this));
        }
    }

    public virtual Task<Result> HomeAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Failure("Home not supported"));
    }

    protected virtual Task OnRunningAsync()
    {
        _logger.Information("Machine {Name} started running", Name);
        Context.CycleStartTime = DateTime.Now;
        _runCts?.Cancel();
        _runCts?.Dispose();
        _runCts = new CancellationTokenSource();
        return Task.CompletedTask;
    }

    protected virtual Task OnExitRunningAsync()
    {
        _runCts?.Cancel();
        _runCts?.Dispose();
        _runCts = null;

        if (Context.CycleStartTime.HasValue)
        {
            var cycleTime = DateTime.Now - Context.CycleStartTime.Value;
            Context.TotalRunTime += cycleTime;
            Context.CycleCount++;
        }
        return Task.CompletedTask;
    }

    protected virtual Task OnStoppingAsync()
    {
        _logger.Information("Stopping machine {Name}", Name);
        return Task.CompletedTask;
    }

    protected virtual Task OnEmergencyStopAsync()
    {
        _logger.Warning("Emergency stop triggered for machine {Name}", Name);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Cancels the current auto sequence and returns to Idle state
    /// </summary>
    public virtual async Task<Result> CancelAutoAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_stateMachine.CanFire(MachineTrigger.CancelAuto))
                return Result.Failure($"Cannot cancel auto from state {State}");

            await _stateMachine.FireAsync(MachineTrigger.CancelAuto);
            _logger.Information("Auto sequence cancelled for machine {Name}", Name);
            return Result.Success("Auto sequence cancelled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to cancel auto for machine {Name}", Name);
            return Result.Failure("Cancel auto failed", ex);
        }
    }

    /// <summary>
    /// Setup default interlock conditions
    /// </summary>
    protected virtual void SetupDefaultInterlocks()
    {
        // Default: Cannot start if there are active alarms
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "NO_ACTIVE_ALARMS",
            Description = "No active alarms present",
            Condition = () => !HasActiveAlarms,
            IsRequired = true
        });
    }

    /// <summary>
    /// Raises an alarm with code and message
    /// </summary>
    protected void RaiseAlarm(int code, string message, AlarmSeverity severity = AlarmSeverity.Warning, string? source = null)
    {
        AlarmManager.Raise(code, message, severity, source);
        Context.LastError = message;

        if (severity == AlarmSeverity.Error || severity == AlarmSeverity.Critical)
        {
            TriggerPauseOnError();
        }
    }

    private void TriggerPauseOnError()
    {
        if (State == MachineState.EmergencyStop)
        {
            return;
        }

        _runCts?.Cancel();

        if (State == MachineState.Running && _stateMachine.CanFire(MachineTrigger.Pause))
        {
            _ = _stateMachine.FireAsync(MachineTrigger.Pause);
            return;
        }

        if (_stateMachine.CanFire(MachineTrigger.Error))
        {
            _ = _stateMachine.FireAsync(MachineTrigger.Error);
        }
    }

    /// <summary>
    /// Clears a specific alarm by code
    /// </summary>
    protected void ClearAlarm(int code)
    {
        AlarmManager.Clear(code);
    }

    /// <summary>
    /// Clears all alarms
    /// </summary>
    protected void ClearAllAlarms()
    {
        AlarmManager.ClearAll();
        Context.LastError = string.Empty;
    }

    public void SetRunMode(MachineRunMode mode)
    {
        RunMode = mode;
        _logger.Information("Machine {Name} run mode set to {Mode}", Name, mode);
    }

    public void RequireAutoRestore()
    {
        _autoRestoreRequired = true;
    }

    protected void MarkAutoRestoreComplete()
    {
        _autoRestoreRequired = false;
    }

    protected async Task<Result> WaitForInputOnAsync(
        string address,
        int timeoutMs,
        string? errorMessage = null,
        CancellationToken ct = default)
    {
        var ok = await _sensorWaiter.WaitForSensorAsync(
            address,
            () => Task.FromResult(IO.ReadInput(address)),
            timeoutMs,
            ct);

        if (ok) return Result.Success();
        return Result.Failure(errorMessage ?? $"Timeout waiting for {address}");
    }

    protected async Task<Result> WaitForInputOffAsync(
        string address,
        int timeoutMs,
        string? errorMessage = null,
        CancellationToken ct = default)
    {
        var ok = await _sensorWaiter.WaitForSensorOffAsync(
            address,
            () => Task.FromResult(IO.ReadInput(address)),
            timeoutMs,
            ct);

        if (ok) return Result.Success();
        return Result.Failure(errorMessage ?? $"Timeout waiting for {address} OFF");
    }

    /// <summary>
    /// Called when an alarm is raised
    /// </summary>
    private void OnAlarmRaised(object? sender, Alarm.Alarm alarm)
    {
        _logger.Warning("Alarm raised: {AlarmCode} - {AlarmMessage} (Severity: {Severity})",
            alarm.Code, alarm.Message, alarm.Severity);

        // Auto pause on serious alarms
        if (alarm.Severity == AlarmSeverity.Error || alarm.Severity == AlarmSeverity.Critical)
        {
            _logger.Warning("Alarm requires pause, pausing machine {Name}", Name);
            TriggerPauseOnError();
        }
    }

    /// <summary>
    /// Called when an alarm is cleared
    /// </summary>
    private void OnAlarmCleared(object? sender, Alarm.Alarm alarm)
    {
        _logger.Information("Alarm cleared: {AlarmCode}", alarm.Code);
    }

    #region Template Methods for Derived Classes

    /// <summary>
    /// Override this to register hardware devices, IO providers, and data providers.
    /// </summary>
    protected virtual void OnRegisterHardware(HardwareManager hardwareManager)
    {
        // Derived classes can register hardware here
    }

    /// <summary>
    /// Override this to register axes based on the profile definitions.
    /// </summary>
    protected virtual void OnRegisterProfileAxes(HardwareManager hardwareManager, IReadOnlyList<AxisDefinition> axes)
    {
        // Derived classes can register profile axes here
    }

    /// <summary>
    /// Override this to provide IO Map for automatic IO scanning
    /// Return null if you don't want automatic IO handling
    /// </summary>
    protected virtual IOMap? GetIOMap() => _profile?.IOMap;

    /// <summary>
    /// Override this to react to hardware connection failures.
    /// </summary>
    protected virtual Task OnHardwareConnectFailedAsync(Result result)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Override this to handle reading inputs from PLC/hardware
    /// Used by the built-in IO adapter when no IO device is registered
    /// </summary>
    protected virtual Task<bool> OnReadInputAsync(string address, CancellationToken ct) => Task.FromResult(false);

    /// <summary>
    /// Override this to handle writing outputs to PLC/hardware
    /// Used by the built-in IO adapter when no IO device is registered
    /// </summary>
    protected virtual Task OnWriteOutputAsync(string address, bool value, CancellationToken ct) => Task.CompletedTask;

    /// <summary>
    /// Override this to register additional background tasks
    /// Example: Feeder monitor, vision processing, data logging
    /// </summary>
    protected virtual void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
    {
        // Derived classes can register custom background tasks here
    }

    /// <summary>
    /// Override this to perform custom idle operations every scan cycle
    /// Called when machine is in Idle state
    /// </summary>
    protected virtual Task OnIdleAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private sealed class MachineIoAdapter : IIO
    {
        private readonly MachineBase _machine;

        public MachineIoAdapter(MachineBase machine)
        {
            _machine = machine;
        }

        public string Id => _machine.Id;

        public string Name => _machine.Name;

        public bool IsConnected => true;

        public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success("Machine IO adapter ready"));

        public Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success("Machine IO adapter stopped"));

        public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success("Machine IO adapter reset"));

        public async Task<Result<bool>> ReadInputAsync(string address, CancellationToken cancellationToken = default)
        {
            var value = await _machine.OnReadInputAsync(address, cancellationToken);
            return Result.Success(value);
        }

        public async Task<Result> WriteOutputAsync(string address, bool value, CancellationToken cancellationToken = default)
        {
            await _machine.OnWriteOutputAsync(address, value, cancellationToken);
            return Result.Success();
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Dispose machine resources
    /// </summary>
    public virtual async ValueTask DisposeAsync()
    {
        _logger.Information("Disposing machine {Name}...", Name);

        // Stop main loop
        StopMainLoop();

        await _hardwareManager.DisconnectAllAsync();

        // Cleanup alarm manager
        AlarmManager.AlarmRaised -= OnAlarmRaised;
        AlarmManager.AlarmCleared -= OnAlarmCleared;

        _logger.Information("Machine {Name} disposed", Name);
        await Task.CompletedTask;
    }

    #endregion
}
