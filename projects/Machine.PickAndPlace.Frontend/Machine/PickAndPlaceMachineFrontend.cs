using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Alarm;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Interlock;
using NAutoSuite.Core.Machine;
using NAutoSuite.Core.IO;
using Serilog;

namespace PickAndPlace.Frontend.Machine;

/// <summary>
/// PickAndPlace Machine Frontend - Logic mới với 1 trục
/// Pattern:
/// 1. Start → Chạy 10s
/// 2. Chạy đi chạy lại 1 vòng → Lặp 10 lần
/// 3. Chạy ngược lại so với lần đầu
/// 4. Dừng 5s
/// 5. Lặp lại từ đầu
/// </summary>
public class PickAndPlaceMachineFrontend : MachineBase
{
    private readonly IAxis _axis;

    // Motion parameters
    private readonly double _startPosition = 0;
    private readonly double _endPosition = 100;      // 100mm travel
    private readonly double _velocity = 50;          // 50 mm/s
    private readonly double _initialRunTime = 10;    // 10 seconds
    private readonly int _cycleLoops = 10;           // 10 lần
    private readonly double _pauseTime = 5;          // 5 seconds

    public PickAndPlaceMachineFrontend(
        IAxis axis,
        ILogger? logger = null)
        : base("PAP001_FE", "PickAndPlace Frontend", logger)
    {
        _axis = axis;

        // Setup interlocks
        SetupMachineInterlocks();
    }

    private void SetupMachineInterlocks()
    {
        // Interlock: Axis must be homed before starting
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "AXIS_HOMED",
            Description = "Axis must be homed",
            Condition = () => _axis?.IsHomed ?? true,
            IsRequired = true
        });

        // Interlock: No axis alarm
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "NO_AXIS_ALARM",
            Description = "No axis in alarm state",
            Condition = () => !(_axis?.IsInAlarm ?? false),
            IsRequired = true
        });
    }

    #region MachineBase Overrides

    /// <summary>
    /// No IO Map for this demo (no PLC, just motion controller)
    /// </summary>
    protected override IOMap? GetIOMap() => null;

    /// <summary>
    /// No PLC input reading
    /// </summary>
    protected override Task<bool> OnReadInputAsync(string address, CancellationToken ct)
        => Task.FromResult(false);

    /// <summary>
    /// No PLC output writing
    /// </summary>
    protected override Task OnWriteOutputAsync(string address, bool value, CancellationToken ct)
        => Task.CompletedTask;

    #endregion

    #region Initialization

    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        _logger.Information("Initializing PickAndPlace Machine Frontend...");

        try
        {
            // Connect axis
            if (_axis != null)
            {
                var result = await _axis.ConnectAsync();
                if (!result.IsSuccess)
                {
                    RaiseAlarm(1001, $"Failed to connect axis: {result.Message}", AlarmSeverity.Critical);
                    throw new Exception(result.Message);
                }
                _logger.Information("Axis connected successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Hardware initialization failed");
            throw;
        }
    }

    #endregion

    #region AUTO Logic - Pattern mới

    /// <summary>
    /// Main AUTO sequence với pattern mới:
    /// 1. Chạy 10s (initial run)
    /// 2. Chạy đi chạy lại 1 vòng (start→end→start) x 10 lần
    /// 3. Chạy ngược (start→end) - đối lập với lần đầu
    /// 4. Dừng 5s
    /// 5. Lặp lại
    /// </summary>
    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        _logger.Information("=== Starting Cycle {Count} ===", Context.CycleCount + 1);

        try
        {
            // ===== STEP 1: Initial Run - Chạy 10 giây =====
            _logger.Information("Step 1: Initial run for {Time}s", _initialRunTime);
            await InitialRunAsync();

            // ===== STEP 2: Chạy đi chạy lại 10 lần =====
            _logger.Information("Step 2: Running back and forth {Loops} times", _cycleLoops);
            for (int i = 0; i < _cycleLoops; i++)
            {
                _logger.Information("  Loop {Current}/{Total}", i + 1, _cycleLoops);

                // Forward: Start → End
                await MoveToAsync(_endPosition, $"Loop {i + 1}: Forward");

                // Backward: End → Start
                await MoveToAsync(_startPosition, $"Loop {i + 1}: Backward");
            }

            // ===== STEP 3: Chạy ngược lại so với lần đầu =====
            _logger.Information("Step 3: Running reverse pattern");
            await ReverseRunAsync();

            // ===== STEP 4: Dừng 5 giây =====
            _logger.Information("Step 4: Pausing for {Time}s", _pauseTime);
            await Task.Delay(TimeSpan.FromSeconds(_pauseTime));

            _logger.Information("=== Cycle {Count} completed successfully ===", Context.CycleCount + 1);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Cycle {Count} failed", Context.CycleCount + 1);
            RaiseAlarm(2001, $"Cycle failed: {ex.Message}", AlarmSeverity.Critical);
            throw;
        }
    }

    /// <summary>
    /// Step 1: Initial run - Chạy liên tục trong 10 giây
    /// Di chuyển từ start → end nhiều lần trong 10s
    /// </summary>
    private async Task InitialRunAsync()
    {
        var startTime = DateTime.Now;
        var endTime = startTime.AddSeconds(_initialRunTime);
        int moves = 0;

        while (DateTime.Now < endTime)
        {
            // Forward
            await MoveToAsync(_endPosition, $"InitialRun move {moves + 1}: Forward");
            moves++;

            if (DateTime.Now >= endTime) break;

            // Backward
            await MoveToAsync(_startPosition, $"InitialRun move {moves + 1}: Backward");
            moves++;
        }

        _logger.Information("InitialRun completed: {Moves} moves in {Time}s", moves, _initialRunTime);
    }

    /// <summary>
    /// Step 3: Reverse run - Chạy ngược lại so với initial run
    /// Nếu initial run kết thúc ở end, thì reverse bắt đầu từ end → start
    /// </summary>
    private async Task ReverseRunAsync()
    {
        // Get current position
        var currentPosition = _axis?.Position ?? 0;

        _logger.Information("Current position: {Pos}, reversing...", currentPosition);

        // Nếu đang ở end, chạy về start
        if (Math.Abs(currentPosition - _endPosition) < 1.0)
        {
            await MoveToAsync(_startPosition, "Reverse: End → Start");
        }
        // Nếu đang ở start, chạy về end
        else
        {
            await MoveToAsync(_endPosition, "Reverse: Start → End");
        }
    }

    /// <summary>
    /// Helper: Move to target position
    /// </summary>
    private async Task MoveToAsync(double targetPosition, string description)
    {
        _logger.Information("{Description} → Position: {Target}mm", description, targetPosition);

        if (_axis != null)
        {
            var result = await _axis.MoveAbsoluteAsync(targetPosition, _velocity);
            if (!result.IsSuccess)
            {
                RaiseAlarm(2101, $"Move failed: {result.Message}", AlarmSeverity.Critical);
                throw new Exception(result.Message);
            }

            // Wait for motion complete (với timeout protection)
            var timeout = DateTime.Now.AddSeconds(30);
            while (_axis.IsMoving && DateTime.Now < timeout)
            {
                await Task.Delay(100);
            }

            if (_axis.IsMoving)
            {
                _logger.Warning("Motion timeout - stopping axis");
                await _axis.StopAsync(emergency: false);
            }
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Home axis - Gọi trước khi Start
    /// </summary>
    public async Task<Result> HomeAxisAsync()
    {
        _logger.Information("Homing axis...");

        if (_axis != null && !_axis.IsHomed)
        {
            var result = await _axis.HomeAsync();
            if (!result.IsSuccess)
            {
                RaiseAlarm(1101, $"Axis homing failed: {result.Message}", AlarmSeverity.Critical);
                return result;
            }
            _logger.Information("Axis homed successfully");
        }

        return Result.Success("Axis homed");
    }

    /// <summary>
    /// Get current axis position
    /// </summary>
    public double CurrentPosition => _axis?.Position ?? 0;

    /// <summary>
    /// Check if axis is moving
    /// </summary>
    public bool IsAxisMoving => _axis?.IsMoving ?? false;

    #endregion
}
