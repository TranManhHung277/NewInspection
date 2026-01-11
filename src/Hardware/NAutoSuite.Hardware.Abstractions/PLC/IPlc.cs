using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;

namespace NAutoSuite.Hardware.Abstractions.PLC;

/// <summary>
/// PLC communication interface
/// </summary>
public interface IPlc : IDevice
{
    /// <summary>
    /// Read bit from PLC
    /// </summary>
    Task<Result<bool>> ReadBitAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write bit to PLC
    /// </summary>
    Task<Result> WriteBitAsync(string address, bool value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read word (16-bit) from PLC
    /// </summary>
    Task<Result<short>> ReadWordAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write word to PLC
    /// </summary>
    Task<Result> WriteWordAsync(string address, short value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read double word (32-bit) from PLC
    /// </summary>
    Task<Result<int>> ReadDWordAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write double word to PLC
    /// </summary>
    Task<Result> WriteDWordAsync(string address, int value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read float from PLC
    /// </summary>
    Task<Result<float>> ReadFloatAsync(string address, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write float to PLC
    /// </summary>
    Task<Result> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Read bytes from PLC
    /// </summary>
    Task<Result<byte[]>> ReadBytesAsync(string address, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write bytes to PLC
    /// </summary>
    Task<Result> WriteBytesAsync(string address, byte[] data, CancellationToken cancellationToken = default);
}
