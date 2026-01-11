using NAutoSuite.Core.Common;

namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Interface for motion axis control
/// </summary>
public interface IAxis : IDevice
{
    /// <summary>
    /// Current position in user units
    /// </summary>
    double Position { get; }

    /// <summary>
    /// Current velocity
    /// </summary>
    double Velocity { get; }

    /// <summary>
    /// Is axis homed
    /// </summary>
    bool IsHomed { get; }

    /// <summary>
    /// Is axis moving
    /// </summary>
    bool IsMoving { get; }

    /// <summary>
    /// Is axis in alarm state
    /// </summary>
    bool IsInAlarm { get; }

    /// <summary>
    /// Perform homing sequence
    /// </summary>
    Task<Result> HomeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Move to absolute position
    /// </summary>
    Task<Result> MoveAbsoluteAsync(double position, double? velocity = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Move relative from current position
    /// </summary>
    Task<Result> MoveRelativeAsync(double distance, double? velocity = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop axis motion
    /// </summary>
    Task<Result> StopAsync(bool emergency = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable/disable servo
    /// </summary>
    Task<Result> SetServoAsync(bool enabled, CancellationToken cancellationToken = default);
}
