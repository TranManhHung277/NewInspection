using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace PickAndPlace.Backend.Stations;

/// <summary>
/// Station responsible for placing parts
/// </summary>
public class PlaceStation
{
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly IAxis? _axisZ;
    private readonly IOutput? _vacuum;
    private readonly ILogger _logger;

    // Place position coordinates
    private const double PlaceX = 200.0;
    private const double PlaceY = 150.0;
    private const double PlaceZDown = 10.0;
    private const double PlaceZUp = 50.0;

    // Home position
    private const double HomeX = 0.0;
    private const double HomeY = 0.0;

    public PlaceStation(
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

    public async Task<Result> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Place sequence started");

        try
        {
            // Move to place position
            var result = await MoveToPlacePositionAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to move to place position: {Message}", result.Message);
                return result;
            }

            // Lower Z
            result = await LowerZAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to lower Z: {Message}", result.Message);
                return result;
            }

            // Deactivate vacuum
            result = await DeactivateVacuumAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to deactivate vacuum: {Message}", result.Message);
                return result;
            }

            // Raise Z
            result = await RaiseZAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to raise Z: {Message}", result.Message);
                return result;
            }

            // Return home
            result = await ReturnHomeAsync(cancellationToken);
            if (!result.IsSuccess)
            {
                _logger.Error("Failed to return home: {Message}", result.Message);
                return result;
            }

            _logger.Information("Place sequence completed successfully");
            return Result.Success("Place completed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Place sequence failed");
            return Result.Failure("Place sequence failed", ex);
        }
    }

    private async Task<Result> MoveToPlacePositionAsync(CancellationToken cancellationToken)
    {
        if (_axisX != null)
        {
            var result = await _axisX.MoveAbsoluteAsync(PlaceX, cancellationToken: cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure($"Axis X move failed: {result.Message}");
        }

        if (_axisY != null)
        {
            var result = await _axisY.MoveAbsoluteAsync(PlaceY, cancellationToken: cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure($"Axis Y move failed: {result.Message}");
        }

        // Wait for axes to settle
        await Task.Delay(500, cancellationToken);

        return Result.Success("Moved to place position");
    }

    private async Task<Result> LowerZAsync(CancellationToken cancellationToken)
    {
        if (_axisZ != null)
        {
            var result = await _axisZ.MoveAbsoluteAsync(PlaceZDown, cancellationToken: cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure($"Z axis down failed: {result.Message}");

            await Task.Delay(300, cancellationToken);
        }

        return Result.Success("Z lowered");
    }

    private async Task<Result> RaiseZAsync(CancellationToken cancellationToken)
    {
        if (_axisZ != null)
        {
            var result = await _axisZ.MoveAbsoluteAsync(PlaceZUp, cancellationToken: cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure($"Z axis up failed: {result.Message}");

            await Task.Delay(300, cancellationToken);
        }

        return Result.Success("Z raised");
    }

    private async Task<Result> DeactivateVacuumAsync(CancellationToken cancellationToken)
    {
        if (_vacuum != null)
        {
            var result = await _vacuum.WriteAsync(false, cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure($"Vacuum deactivation failed: {result.Message}");

            await Task.Delay(200, cancellationToken);
        }

        return Result.Success("Vacuum deactivated");
    }

    private async Task<Result> ReturnHomeAsync(CancellationToken cancellationToken)
    {
        if (_axisX != null)
        {
            var result = await _axisX.MoveAbsoluteAsync(HomeX, cancellationToken: cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure($"Axis X return home failed: {result.Message}");
        }

        if (_axisY != null)
        {
            var result = await _axisY.MoveAbsoluteAsync(HomeY, cancellationToken: cancellationToken);
            if (!result.IsSuccess)
                return Result.Failure($"Axis Y return home failed: {result.Message}");
        }

        return Result.Success("Returned home");
    }
}
