using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace Machine.PickAndPlace.Core.Manual;

/// <summary>
/// Manual control for Pick and Place machine
/// Provides individual axis and IO control
/// </summary>
public class ManualController
{
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly IAxis? _axisZ;
    private readonly IOutput? _vacuum;
    private readonly ILogger _logger;

    // JOG speeds
    private const double JOG_SPEED_LOW = 10.0;      // mm/s
    private const double JOG_SPEED_MEDIUM = 50.0;   // mm/s
    private const double JOG_SPEED_HIGH = 100.0;    // mm/s

    public enum JogSpeed
    {
        Low,
        Medium,
        High
    }

    public ManualController(
        IAxis? axisX,
        IAxis? axisY,
        IAxis? axisZ,
        IOutput? vacuum,
        ILogger logger)
    {
        _axisX = axisX;
        _axisY = axisY;
        _axisZ = axisZ;
        _vacuum = vacuum;
        _logger = logger;
    }

    #region Axis Movement

    /// <summary>
    /// Move axis to absolute position
    /// </summary>
    public async Task<Result> MoveToPositionAsync(string axisName, double position, CancellationToken ct = default)
    {
        var axis = GetAxis(axisName);
        if (axis == null)
            return Result.Failure($"Axis {axisName} not found");

        if (!axis.IsHomed)
            return Result.Failure($"Axis {axisName} must be homed first");

        _logger.Information("Manual: Moving {Axis} to position {Position}", axisName, position);

        var result = await axis.MoveAbsoluteAsync(position, cancellationToken: ct);
        if (!result.IsSuccess)
        {
            _logger.Error("Manual: Move {Axis} failed - {Message}", axisName, result.Message);
        }

        return result;
    }

    /// <summary>
    /// JOG axis (continuous movement)
    /// Call StopAxis() to stop
    /// </summary>
    public Result JogAxis(string axisName, bool positiveDirection, JogSpeed speed = JogSpeed.Medium)
    {
        var axis = GetAxis(axisName);
        if (axis == null)
            return Result.Failure($"Axis {axisName} not found");

        if (!axis.IsHomed)
            return Result.Failure($"Axis {axisName} must be homed first");

        double jogSpeed = speed switch
        {
            JogSpeed.Low => JOG_SPEED_LOW,
            JogSpeed.Medium => JOG_SPEED_MEDIUM,
            JogSpeed.High => JOG_SPEED_HIGH,
            _ => JOG_SPEED_MEDIUM
        };

        // Set velocity
        // Note: This requires IAxis to have SetVelocity method
        // For now, we'll use MoveRelativeAsync with large distance

        _logger.Information("Manual: JOG {Axis} {Direction} at speed {Speed}",
            axisName, positiveDirection ? "+" : "-", speed);

        // Alternative: Use a background task for continuous movement
        var distance = positiveDirection ? 10000.0 : -10000.0; // Large distance
        _ = axis.MoveRelativeAsync(distance, jogSpeed);

        return Result.Success("JOG started");
    }

    /// <summary>
    /// Stop axis movement
    /// </summary>
    public async Task<Result> StopAxisAsync(string axisName, bool emergency = false)
    {
        var axis = GetAxis(axisName);
        if (axis == null)
            return Result.Failure($"Axis {axisName} not found");

        _logger.Information("Manual: Stop {Axis} (emergency={Emergency})", axisName, emergency);

        return await axis.StopAsync(emergency);
    }

    /// <summary>
    /// Home single axis
    /// </summary>
    public async Task<Result> HomeAxisAsync(string axisName, CancellationToken ct = default)
    {
        var axis = GetAxis(axisName);
        if (axis == null)
            return Result.Failure($"Axis {axisName} not found");

        _logger.Information("Manual: Homing {Axis}", axisName);

        var result = await axis.HomeAsync(ct);
        if (result.IsSuccess)
        {
            _logger.Information("Manual: {Axis} homed successfully", axisName);
        }
        else
        {
            _logger.Error("Manual: {Axis} homing failed - {Message}", axisName, result.Message);
        }

        return result;
    }

    #endregion

    #region IO Control

    /// <summary>
    /// Control vacuum output
    /// </summary>
    public async Task<Result> SetVacuumAsync(bool on, CancellationToken ct = default)
    {
        if (_vacuum == null)
            return Result.Failure("Vacuum output not configured");

        _logger.Information("Manual: Vacuum {State}", on ? "ON" : "OFF");

        return await _vacuum.WriteAsync(on, ct);
    }

    #endregion

    #region Preset Positions

    /// <summary>
    /// Move to pick position
    /// </summary>
    public async Task<Result> MoveToPickPositionAsync(CancellationToken ct = default)
    {
        _logger.Information("Manual: Moving to pick position");

        try
        {
            if (_axisX != null) await _axisX.MoveAbsoluteAsync(100, cancellationToken: ct);
            if (_axisY != null) await _axisY.MoveAbsoluteAsync(50, cancellationToken: ct);
            if (_axisZ != null) await _axisZ.MoveAbsoluteAsync(50, cancellationToken: ct);

            return Result.Success("Moved to pick position");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Manual: Failed to move to pick position");
            return Result.Failure("Move failed", ex);
        }
    }

    /// <summary>
    /// Move to place position
    /// </summary>
    public async Task<Result> MoveToPlacePositionAsync(CancellationToken ct = default)
    {
        _logger.Information("Manual: Moving to place position");

        try
        {
            if (_axisX != null) await _axisX.MoveAbsoluteAsync(200, cancellationToken: ct);
            if (_axisY != null) await _axisY.MoveAbsoluteAsync(150, cancellationToken: ct);
            if (_axisZ != null) await _axisZ.MoveAbsoluteAsync(50, cancellationToken: ct);

            return Result.Success("Moved to place position");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Manual: Failed to move to place position");
            return Result.Failure("Move failed", ex);
        }
    }

    /// <summary>
    /// Move to home position
    /// </summary>
    public async Task<Result> MoveToHomePositionAsync(CancellationToken ct = default)
    {
        _logger.Information("Manual: Moving to home position");

        try
        {
            if (_axisX != null) await _axisX.MoveAbsoluteAsync(0, cancellationToken: ct);
            if (_axisY != null) await _axisY.MoveAbsoluteAsync(0, cancellationToken: ct);
            if (_axisZ != null) await _axisZ.MoveAbsoluteAsync(0, cancellationToken: ct);

            return Result.Success("Moved to home position");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Manual: Failed to move to home position");
            return Result.Failure("Move failed", ex);
        }
    }

    #endregion

    #region Status

    /// <summary>
    /// Get axis position
    /// </summary>
    public double GetPosition(string axisName)
    {
        var axis = GetAxis(axisName);
        return axis?.Position ?? 0;
    }

    /// <summary>
    /// Check if axis is homed
    /// </summary>
    public bool IsAxisHomed(string axisName)
    {
        var axis = GetAxis(axisName);
        return axis?.IsHomed ?? false;
    }

    /// <summary>
    /// Check if axis is moving
    /// </summary>
    public bool IsAxisMoving(string axisName)
    {
        var axis = GetAxis(axisName);
        return axis?.IsMoving ?? false;
    }

    #endregion

    #region Helpers

    private IAxis? GetAxis(string axisName) => axisName.ToUpper() switch
    {
        "X" or "AXISX" => _axisX,
        "Y" or "AXISY" => _axisY,
        "Z" or "AXISZ" => _axisZ,
        _ => null
    };

    #endregion
}
