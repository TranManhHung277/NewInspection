using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Core.Motion;

/// <summary>
/// Base implementation for motion axis
/// </summary>
public abstract class AxisBase : IAxis
{
    protected readonly ILogger _logger;

    public string Id { get; }
    public string Name { get; }
    public abstract bool IsConnected { get; }
    public abstract double Position { get; }
    public abstract double Velocity { get; }
    public abstract bool IsHomed { get; }
    public abstract bool IsMoving { get; }
    public abstract bool IsInAlarm { get; }

    protected AxisBase(string id, string name, ILogger? logger = null)
    {
        Guard.AgainstNullOrEmpty(id, nameof(id));
        Guard.AgainstNullOrEmpty(name, nameof(name));

        Id = id;
        Name = name;
        _logger = logger ?? Log.Logger;
    }

    public abstract Task<Result> ConnectAsync(CancellationToken cancellationToken = default);
    public abstract Task<Result> DisconnectAsync(CancellationToken cancellationToken = default);
    public abstract Task<Result> ResetAsync(CancellationToken cancellationToken = default);
    public abstract Task<Result> HomeAsync(CancellationToken cancellationToken = default);
    public abstract Task<Result> MoveAbsoluteAsync(double position, double? velocity = null, CancellationToken cancellationToken = default);
    public abstract Task<Result> MoveRelativeAsync(double distance, double? velocity = null, CancellationToken cancellationToken = default);
    public abstract Task<Result> StopAsync(bool emergency = false, CancellationToken cancellationToken = default);
    public abstract Task<Result> SetServoAsync(bool enabled, CancellationToken cancellationToken = default);
}
