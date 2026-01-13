using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Alarm;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Interlock;
using NAutoSuite.Core.IO;
using Serilog;
using Stateless;

namespace NAutoSuite.Core.Machine;

/// <summary>
/// Base class for all machines with integrated:
/// - Alarm and interlock management
/// - Automatic IO scan cycle (EMG, Start, Stop, Reset, Tower Lights)
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

    public string Id { get; }
    public string Name { get; }
    public MachineState State => _stateMachine.State;
    public MachineContext Context { get; }

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

    protected MachineBase(string id, string name, ILogger? logger = null)
    {
        Guard.AgainstNullOrEmpty(id, nameof(id));
        Guard.AgainstNullOrEmpty(name, nameof(name));

        Id = id;
        Name = name;
        Context = new MachineContext();
        _logger = logger ?? Log.Logger;

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
            .OnEntryAsync(OnInitializingAsync)
            .Permit(MachineTrigger.Complete, MachineState.Idle)
            .Permit(MachineTrigger.Reset, MachineState.Error);

        _stateMachine.Configure(MachineState.Idle)
            .Permit(MachineTrigger.Start, MachineState.Running)
            .Permit(MachineTrigger.Reset, MachineState.Uninitialized);

        _stateMachine.Configure(MachineState.Running)
            .OnEntryAsync(OnRunningAsync)
            .OnExitAsync(OnExitRunningAsync)
            .Permit(MachineTrigger.Stop, MachineState.Stopping)
            .Permit(MachineTrigger.Pause, MachineState.Paused)
            .Permit(MachineTrigger.EmergencyStop, MachineState.EmergencyStop)
            .Permit(MachineTrigger.Complete, MachineState.Idle)
            .Permit(MachineTrigger.CancelAuto, MachineState.Idle);

        _stateMachine.Configure(MachineState.Paused)
            .Permit(MachineTrigger.Resume, MachineState.Running)
            .Permit(MachineTrigger.Stop, MachineState.Stopping);

        _stateMachine.Configure(MachineState.Stopping)
            .OnEntryAsync(OnStoppingAsync)
            .Permit(MachineTrigger.Complete, MachineState.Stopped);

        _stateMachine.Configure(MachineState.Stopped)
            .Permit(MachineTrigger.Reset, MachineState.Idle);

        _stateMachine.Configure(MachineState.Error)
            .Permit(MachineTrigger.ClearError, MachineState.Idle)
            .Permit(MachineTrigger.Reset, MachineState.Uninitialized);

        _stateMachine.Configure(MachineState.EmergencyStop)
            .OnEntryAsync(OnEmergencyStopAsync)
            .Permit(MachineTrigger.Reset, MachineState.Idle);

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
            return Result.Failure("Initialization failed", ex);
        }
    }

    /// <summary>
    /// Start the main loop - automatically scans IO and handles buttons/lights
    /// Called automatically in InitializeAsync()
    /// </summary>
    private void StartMainLoop()
    {
        if (_mainLoopCts != null)
        {
            _logger.Warning("Main loop already running");
            return;
        }

        // Get IO Map from derived class
        var ioMap = GetIOMap();
        if (ioMap == null)
        {
            _logger.Warning("No IO Map provided - Main loop will not handle common IO");
            return;
        }

        // Setup CommonIOHandler
        _commonIOHandler = new CommonIOHandler(
            machine: this,
            ioMap: ioMap,
            readInputFunc: OnReadInputAsync,
            writeOutputFunc: OnWriteOutputAsync,
            logger: _logger
        );

        // Setup BackgroundTaskManager
        _backgroundTaskManager = new BackgroundTaskManager(_logger);

        // Register CommonIOScan task (100ms cycle)
        _backgroundTaskManager.RegisterTask("CommonIOScan", async ct =>
        {
            await _commonIOHandler!.ScanAsync(ct);
        }, intervalMs: 100);

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
    private void StopMainLoop()
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
            if (!_stateMachine.CanFire(MachineTrigger.Start))
                return Result.Failure($"Cannot start from state {State}");

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
            Context.Reset();
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

    protected virtual Task OnRunningAsync()
    {
        _logger.Information("Machine {Name} started running", Name);
        Context.CycleStartTime = DateTime.Now;
        return Task.CompletedTask;
    }

    protected virtual Task OnExitRunningAsync()
    {
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
        // Default: Cannot start if there are critical alarms
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "NO_CRITICAL_ALARMS",
            Description = "No critical alarms present",
            Condition = () => !ActiveAlarms.Any(a => a.Severity == AlarmSeverity.Critical),
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

    /// <summary>
    /// Called when an alarm is raised
    /// </summary>
    private void OnAlarmRaised(object? sender, Alarm.Alarm alarm)
    {
        _logger.Warning("Alarm raised: {AlarmCode} - {AlarmMessage} (Severity: {Severity})",
            alarm.Code, alarm.Message, alarm.Severity);

        // Auto stop if critical alarm and machine is running
        if (alarm.Severity == AlarmSeverity.Critical && State == MachineState.Running)
        {
            _logger.Warning("Critical alarm triggered, stopping machine {Name}", Name);
            _ = StopAsync(); // Fire and forget
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
    /// Override this to provide IO Map for automatic IO scanning
    /// Return null if you don't want automatic IO handling
    /// </summary>
    protected virtual IOMap? GetIOMap() => null;

    /// <summary>
    /// Override this to handle reading inputs from PLC/hardware
    /// Called by CommonIOHandler for reading buttons and sensors
    /// </summary>
    protected virtual Task<bool> OnReadInputAsync(string address, CancellationToken ct) => Task.FromResult(false);

    /// <summary>
    /// Override this to handle writing outputs to PLC/hardware
    /// Called by CommonIOHandler for controlling tower lights, buzzer, etc.
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

        // Cleanup alarm manager
        AlarmManager.AlarmRaised -= OnAlarmRaised;
        AlarmManager.AlarmCleared -= OnAlarmCleared;

        _logger.Information("Machine {Name} disposed", Name);
        await Task.CompletedTask;
    }

    #endregion
}
