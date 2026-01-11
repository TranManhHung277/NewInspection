using NAutoSuite.Core.Common;

namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Base interface for all hardware devices
/// </summary>
public interface IDevice
{
    /// <summary>
    /// Device unique identifier
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Device name
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Is device connected and ready
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connect to device
    /// </summary>
    Task<Result> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from device
    /// </summary>
    Task<Result> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset device to initial state
    /// </summary>
    Task<Result> ResetAsync(CancellationToken cancellationToken = default);
}
