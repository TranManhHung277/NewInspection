using NAutoSuite.Core.Common;

namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Generic IO interface for reading inputs and writing outputs
/// Used by MachineController to handle common IOs (EMG, Start, Stop, etc.)
/// </summary>
public interface IIO : IDevice
{
    /// <summary>
    /// Read digital input
    /// </summary>
    Task<Result<bool>> ReadInputAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write digital output
    /// </summary>
    Task<Result> WriteOutputAsync(string address, bool value, CancellationToken cancellationToken = default);
}
