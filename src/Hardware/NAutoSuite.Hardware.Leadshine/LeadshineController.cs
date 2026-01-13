using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using NAutoSuite.Hardware.Abstractions.Motion;
using Serilog;

namespace NAutoSuite.Hardware.Leadshine;

/// <summary>
/// Leadshine motion controller
/// </summary>
public class LeadshineController : IMotionController
{
    private readonly int _cardId;
    private readonly ILogger _logger;
    private readonly List<LeadshineAxis> _axes;
    private bool _isConnected;

    public string Id { get; }
    public string Name { get; }
    public bool IsConnected => _isConnected;
    public int AxisCount => _axes.Count;

    public LeadshineController(string id, string name, int cardId, int axisCount, ILogger? logger = null)
    {
        Guard.AgainstNullOrEmpty(id, nameof(id));
        Guard.AgainstNullOrEmpty(name, nameof(name));

        Id = id;
        Name = name;
        _cardId = cardId;
        _logger = logger ?? Log.Logger;
        _axes = new List<LeadshineAxis>();

        // Create axes
        for (int i = 0; i < axisCount; i++)
        {
            _axes.Add(new LeadshineAxis((ushort)_cardId, (ushort)i, $"Axis{i}", $"Axis {i}", null, _logger));
        }
    }

    public IAxis? GetAxis(int index)
    {
        if (index < 0 || index >= _axes.Count)
            return null;
        return _axes[index];
    }

    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Connecting to Leadshine controller {Name}", Name);

            foreach (var axis in _axes)
            {
                var result = await axis.ConnectAsync(cancellationToken);
                if (!result.IsSuccess)
                    return result;
            }

            _isConnected = true;
            _logger.Information("Leadshine controller {Name} connected successfully", Name);
            return Result.Success("Controller connected");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to connect controller {Name}", Name);
            return Result.Failure("Connection failed", ex);
        }
    }

    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var axis in _axes)
            {
                await axis.DisconnectAsync(cancellationToken);
            }

            _isConnected = false;
            _logger.Information("Leadshine controller {Name} disconnected", Name);
            return Result.Success("Disconnected");
        }
        catch (Exception ex)
        {
            return Result.Failure("Disconnect failed", ex);
        }
    }

    public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Resetting controller {Name}", Name);
        return Task.FromResult(Result.Success("Reset complete"));
    }

    public async Task<Result> InitializeAxesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Initializing all axes");

            foreach (var axis in _axes)
            {
                var result = await axis.SetServoAsync(true, cancellationToken);
                if (!result.IsSuccess)
                    return result;
            }

            return Result.Success("All axes initialized");
        }
        catch (Exception ex)
        {
            return Result.Failure("Initialization failed", ex);
        }
    }

    public async Task<Result> HomeAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Homing all axes");

            foreach (var axis in _axes)
            {
                var result = await axis.HomeAsync(cancellationToken);
                if (!result.IsSuccess)
                    return result;
            }

            return Result.Success("All axes homed");
        }
        catch (Exception ex)
        {
            return Result.Failure("Homing failed", ex);
        }
    }

    public async Task<Result> StopAllAsync(bool emergency = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Warning("Stopping all axes (emergency={Emergency})", emergency);

            foreach (var axis in _axes)
            {
                await axis.StopAsync(emergency, cancellationToken);
            }

            return Result.Success("All axes stopped");
        }
        catch (Exception ex)
        {
            return Result.Failure("Stop failed", ex);
        }
    }
}
