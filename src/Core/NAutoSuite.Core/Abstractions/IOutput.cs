using NAutoSuite.Core.Common;

namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Digital output interface
/// </summary>
public interface IOutput : IDevice
{
    /// <summary>
    /// Current output state
    /// </summary>
    bool Value { get; }

    /// <summary>
    /// Set output value
    /// </summary>
    Task<Result> WriteAsync(bool value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggle output value
    /// </summary>
    Task<Result> ToggleAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pulse output for specified duration
    /// </summary>
    Task<Result> PulseAsync(TimeSpan duration, CancellationToken cancellationToken = default);
}
