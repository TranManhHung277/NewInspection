using NAutoSuite.Core.Common;
using NAutoSuite.Core.Machine;

namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Base machine interface
/// </summary>
public interface IMachine : IAsyncDisposable
{
    /// <summary>
    /// Machine identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Machine name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Current machine state
    /// </summary>
    MachineState State { get; }

    /// <summary>
    /// Initialize machine
    /// </summary>
    Task<Result> InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Start machine operation
    /// </summary>
    Task<Result> StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop machine operation
    /// </summary>
    Task<Result> StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Pause machine operation
    /// </summary>
    Task<Result> PauseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Resume machine operation
    /// </summary>
    Task<Result> ResumeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset machine to initial state
    /// </summary>
    Task<Result> ResetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Emergency stop
    /// </summary>
    Task<Result> EmergencyStopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// State changed event
    /// </summary>
    event EventHandler<MachineState>? StateChanged;
}
