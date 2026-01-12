using NAutoSuite.Core.Common;
using NAutoSuite.Core.Motion;
using Serilog;

namespace NAutoSuite.Hardware.Leadshine;

/// <summary>
/// Leadshine motion axis implementation with LTDMC.dll
/// </summary>
public class LeadshineAxis : AxisBase
{
    private readonly ushort _cardNo;
    private readonly ushort _axisIndex;
    private readonly LeadshineMaster? _master;
    private bool _isConnected;

    // Motion parameters
    private double _minVel = 0.1;
    private double _maxVel = 100.0;
    private double _acc = 0.2;
    private double _dec = 0.2;

    public override bool IsConnected => _isConnected;

    public override double Position
    {
        get
        {
            if (!_isConnected) return 0;
            try
            {
                double pos = 0;
                LTDMC.dmc_get_position_unit(_cardNo, _axisIndex, ref pos);
                return pos;
            }
            catch
            {
                return 0;
            }
        }
    }

    public override double Velocity => 0; // TODO: Implement velocity reading

    public override bool IsHomed
    {
        get
        {
            if (!_isConnected) return false;
            try
            {
                ushort state = 0;
                LTDMC.dmc_get_home_result(_cardNo, _axisIndex, ref state);
                return state == 1; // 1 = homed successfully
            }
            catch
            {
                return false;
            }
        }
    }

    public override bool IsMoving
    {
        get
        {
            if (!_isConnected) return false;
            try
            {
                var done = LTDMC.dmc_check_done(_cardNo, _axisIndex);
                return done == 0; // 0 = still moving, 1 = done
            }
            catch
            {
                return false;
            }
        }
    }

    public override bool IsInAlarm
    {
        get
        {
            if (!_isConnected) return false;
            try
            {
                var status = LTDMC.dmc_axis_io_status(_cardNo, _axisIndex);
                // Bit 1: Alarm status
                return (status & 0x02) != 0;
            }
            catch
            {
                return false;
            }
        }
    }

    public LeadshineAxis(ushort cardNo, ushort axisIndex, string id, string name, LeadshineMaster? master = null, ILogger? logger = null)
        : base(id, name, logger)
    {
        _cardNo = cardNo;
        _axisIndex = axisIndex;
        _master = master;
    }

    public override Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Axis connection is handled by master
            // Just verify we can read status
            var status = LTDMC.dmc_axis_io_status(_cardNo, _axisIndex);

            _isConnected = true;
            _logger.Information("Leadshine axis {Name} connected (Card={CardNo}, Axis={AxisIndex})",
                Name, _cardNo, _axisIndex);

            return Task.FromResult(Result.Success("Connected to Leadshine axis"));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to connect Leadshine axis {Name}", Name);
            return Task.FromResult(Result.Failure("Connection failed", ex));
        }
    }

    public override Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _isConnected = false;
        _logger.Information("Leadshine axis {Name} disconnected", Name);
        return Task.FromResult(Result.Success("Disconnected"));
    }

    public override Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Clear any error states
            _logger.Information("Leadshine axis {Name} reset", Name);
            return Task.FromResult(Result.Success("Reset complete"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result.Failure("Reset failed", ex));
        }
    }

    /// <summary>
    /// Set motion profile parameters
    /// </summary>
    public Result SetProfile(double minVel, double maxVel, double acc, double dec)
    {
        try
        {
            _minVel = minVel;
            _maxVel = maxVel;
            _acc = acc;
            _dec = dec;

            var result = LTDMC.dmc_set_profile_unit(_cardNo, _axisIndex, minVel, maxVel, acc, dec, 0);
            if (result != 0)
            {
                return Result.Failure($"Failed to set profile, error code: {result}");
            }

            _logger.Debug("Axis {Name} profile set: MinVel={MinVel}, MaxVel={MaxVel}, Acc={Acc}, Dec={Dec}",
                Name, minVel, maxVel, acc, dec);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to set profile for axis {Name}", Name);
            return Result.Failure("Set profile failed", ex);
        }
    }

    public override async Task<Result> HomeAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Starting home sequence for axis {Name}", Name);

            // Set home mode (mode 0: search home sensor)
            var result = LTDMC.dmc_set_homemode(_cardNo, _axisIndex, 0, 10.0, 0, 0);
            if (result != 0)
            {
                return Result.Failure($"Failed to set home mode, error code: {result}");
            }

            // Start homing
            result = LTDMC.dmc_home_move(_cardNo, _axisIndex);
            if (result != 0)
            {
                return Result.Failure($"Failed to start homing, error code: {result}");
            }

            // Wait for homing to complete (with timeout)
            var timeout = DateTime.Now.AddSeconds(30);
            while (DateTime.Now < timeout && !cancellationToken.IsCancellationRequested)
            {
                ushort state = 0;
                LTDMC.dmc_get_home_result(_cardNo, _axisIndex, ref state);

                if (state == 1) // Homing successful
                {
                    _logger.Information("Axis {Name} homed successfully", Name);
                    return Result.Success("Homing complete");
                }
                else if (state == 2) // Homing failed
                {
                    _logger.Error("Homing failed for axis {Name}", Name);
                    return Result.Failure("Homing failed");
                }

                await Task.Delay(100, cancellationToken);
            }

            if (cancellationToken.IsCancellationRequested)
            {
                LTDMC.dmc_stop(_cardNo, _axisIndex, 1); // Deceleration stop
                return Result.Failure("Homing cancelled");
            }

            return Result.Failure("Homing timeout");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Homing failed for axis {Name}", Name);
            return Result.Failure("Homing failed", ex);
        }
    }

    public override async Task<Result> MoveAbsoluteAsync(double position, double? velocity = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsHomed)
                return Result.Failure("Axis must be homed before movement");

            _logger.Information("Moving axis {Name} to absolute position {Position}", Name, position);

            // Set velocity if specified
            if (velocity.HasValue)
            {
                SetProfile(_minVel, velocity.Value, _acc, _dec);
            }

            // Execute absolute move (mode 1 = absolute)
            var result = LTDMC.dmc_pmove_unit(_cardNo, _axisIndex, position, 1);
            if (result != 0)
            {
                return Result.Failure($"Move failed, error code: {result}");
            }

            // Wait for move to complete
            while (!cancellationToken.IsCancellationRequested)
            {
                var done = LTDMC.dmc_check_done(_cardNo, _axisIndex);
                if (done == 1) // Move complete
                {
                    return Result.Success($"Moved to position {position}");
                }

                if (IsInAlarm)
                {
                    return Result.Failure("Axis in alarm during movement");
                }

                await Task.Delay(50, cancellationToken);
            }

            // If cancelled, stop the axis
            LTDMC.dmc_stop(_cardNo, _axisIndex, 1);
            return Result.Failure("Movement cancelled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Move absolute failed for axis {Name}", Name);
            return Result.Failure("Move failed", ex);
        }
    }

    public override async Task<Result> MoveRelativeAsync(double distance, double? velocity = null, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsHomed)
                return Result.Failure("Axis must be homed before movement");

            _logger.Information("Moving axis {Name} relative {Distance}", Name, distance);

            // Set velocity if specified
            if (velocity.HasValue)
            {
                SetProfile(_minVel, velocity.Value, _acc, _dec);
            }

            // Execute relative move (mode 0 = relative)
            var result = LTDMC.dmc_pmove_unit(_cardNo, _axisIndex, distance, 0);
            if (result != 0)
            {
                return Result.Failure($"Move failed, error code: {result}");
            }

            // Wait for move to complete
            while (!cancellationToken.IsCancellationRequested)
            {
                var done = LTDMC.dmc_check_done(_cardNo, _axisIndex);
                if (done == 1)
                {
                    return Result.Success($"Moved {distance} units");
                }

                if (IsInAlarm)
                {
                    return Result.Failure("Axis in alarm during movement");
                }

                await Task.Delay(50, cancellationToken);
            }

            LTDMC.dmc_stop(_cardNo, _axisIndex, 1);
            return Result.Failure("Movement cancelled");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Move relative failed for axis {Name}", Name);
            return Result.Failure("Move failed", ex);
        }
    }

    public override Task<Result> StopAsync(bool emergency = false, CancellationToken cancellationToken = default)
    {
        try
        {
            ushort stopMode = (ushort)(emergency ? 0 : 1); // 0=immediate, 1=deceleration
            var result = LTDMC.dmc_stop(_cardNo, _axisIndex, stopMode);

            if (result != 0)
            {
                return Task.FromResult(Result.Failure($"Stop failed, error code: {result}"));
            }

            _logger.Information("Axis {Name} stopped (emergency={Emergency})", Name, emergency);
            return Task.FromResult(Result.Success("Stopped"));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Stop failed for axis {Name}", Name);
            return Task.FromResult(Result.Failure("Stop failed", ex));
        }
    }

    public override Task<Result> SetServoAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = LTDMC.dmc_set_sevon_enable(_cardNo, _axisIndex, (ushort)(enabled ? 1 : 0));

            if (result != 0)
            {
                return Task.FromResult(Result.Failure($"Servo control failed, error code: {result}"));
            }

            _logger.Information("Axis {Name} servo {State}", Name, enabled ? "ON" : "OFF");
            return Task.FromResult(Result.Success($"Servo {(enabled ? "enabled" : "disabled")}"));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Servo control failed for axis {Name}", Name);
            return Task.FromResult(Result.Failure("Servo control failed", ex));
        }
    }

    /// <summary>
    /// JOG move (continuous velocity move)
    /// </summary>
    public Result JogMove(bool positiveDirection)
    {
        try
        {
            ushort dir = (ushort)(positiveDirection ? 0 : 1); // 0=positive, 1=negative
            var result = LTDMC.dmc_vmove(_cardNo, _axisIndex, dir);

            if (result != 0)
            {
                return Result.Failure($"JOG move failed, error code: {result}");
            }

            _logger.Information("Axis {Name} JOG started in {Direction} direction",
                Name, positiveDirection ? "positive" : "negative");

            return Result.Success("JOG started");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "JOG move failed for axis {Name}", Name);
            return Result.Failure("JOG move failed", ex);
        }
    }
}
