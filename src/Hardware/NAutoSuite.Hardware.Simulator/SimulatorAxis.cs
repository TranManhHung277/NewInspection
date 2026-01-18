using NAutoSuite.Core.Common;
using NAutoSuite.Core.Motion;
using Serilog;

namespace NAutoSuite.Hardware.Simulator;

/// <summary>
/// Simulated motion axis for testing without hardware
/// </summary>
public class SimulatorAxis : AxisBase
{
    private double _position;
    private double _velocity;
    private bool _isHomed;
    private bool _isMoving;
    private bool _servoOn;
    private readonly Random _random = new();

    public override bool IsConnected => true;
    public override double Position => _position + (_random.NextDouble() - 0.5) * 0.001; // Add noise
    public override double Velocity => _velocity;
    public override bool IsHomed => _isHomed;
    public override bool IsMoving => _isMoving;
    public override bool IsInAlarm => false;

    public SimulatorAxis(string id, string name, ILogger? logger = null)
        : base(id, name, logger)
    {
    }

    public override Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Simulator axis {Name} connected", Name);
        return Task.FromResult(Result.Success("Connected"));
    }

    public override Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Simulator axis {Name} disconnected", Name);
        return Task.FromResult(Result.Success("Disconnected"));
    }

    public override Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        _position = 0;
        _velocity = 0;
        _isHomed = false;
        _isMoving = false;
        _logger.Information("Simulator axis {Name} reset", Name);
        return Task.FromResult(Result.Success("Reset"));
    }

    public override async Task<Result> HomeAsync(CancellationToken cancellationToken = default)
    {
        _logger.Information("Homing simulator axis {Name}", Name);
        _isMoving = true;

        await Task.Delay(500, cancellationToken); // Simulate homing time

        _position = 0;
        _isHomed = true;
        _isMoving = false;

        _logger.Information("Simulator axis {Name} homed", Name);
        return Result.Success("Homed");
    }

    public override async Task<Result> MoveAbsoluteAsync(double position, double? velocity = null, CancellationToken cancellationToken = default)
    {
        if (!_isHomed)
            return Result.Failure("Axis not homed");

        if (!_servoOn)
            return Result.Failure("Servo not enabled");

        if (velocity.HasValue)
            _velocity = velocity.Value;

        _logger.Information("Moving simulator axis {Name} to {Position}", Name, position);
        _isMoving = true;

        var distance = Math.Abs(position - _position);
        var moveTime = (int)(distance * 10); // Simulate proportional time
        await Task.Delay(Math.Min(moveTime, 2000), cancellationToken);

        _position = position;
        _isMoving = false;
        _velocity = 0;

        return Result.Success($"Moved to {position}");
    }

    public override async Task<Result> MoveRelativeAsync(double distance, double? velocity = null, CancellationToken cancellationToken = default)
    {
        if (!_isHomed)
            return Result.Failure("Axis not homed");

        if (!_servoOn)
            return Result.Failure("Servo not enabled");

        if (velocity.HasValue)
            _velocity = velocity.Value;

        var targetPosition = _position + distance;
        _logger.Information("Moving simulator axis {Name} by {Distance} to {Target}", Name, distance, targetPosition);

        _isMoving = true;
        var moveTime = (int)(Math.Abs(distance) * 10);
        await Task.Delay(Math.Min(moveTime, 2000), cancellationToken);

        _position = targetPosition;
        _isMoving = false;
        _velocity = 0;

        return Result.Success($"Moved by {distance}");
    }

    public override Task<Result> StopAsync(bool emergency = false, CancellationToken cancellationToken = default)
    {
        _isMoving = false;
        _velocity = 0;
        _logger.Information("Simulator axis {Name} stopped (emergency={Emergency})", Name, emergency);
        return Task.FromResult(Result.Success("Stopped"));
    }

    public void SetSimulatedPosition(double position)
    {
        if (_isMoving) return;
        _position = position;
    }

    public void SetSimulatedVelocity(double velocity)
    {
        if (_isMoving) return;
        _velocity = velocity;
    }

    public override Task<Result> SetServoAsync(bool enabled, CancellationToken cancellationToken = default)
    {
        _servoOn = enabled;
        _logger.Information("Simulator axis {Name} servo {State}", Name, enabled ? "ON" : "OFF");
        return Task.FromResult(Result.Success($"Servo {(enabled ? "enabled" : "disabled")}"));
    }
}
