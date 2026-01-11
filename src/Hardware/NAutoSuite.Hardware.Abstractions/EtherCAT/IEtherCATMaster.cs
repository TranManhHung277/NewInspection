using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;

namespace NAutoSuite.Hardware.Abstractions.EtherCAT;

/// <summary>
/// EtherCAT master interface
/// </summary>
public interface IEtherCATMaster : IDevice
{
    /// <summary>
    /// Number of slaves detected
    /// </summary>
    int SlaveCount { get; }

    /// <summary>
    /// Is in operational state
    /// </summary>
    bool IsOperational { get; }

    /// <summary>
    /// Scan for slaves
    /// </summary>
    Task<Result> ScanAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Go to operational state
    /// </summary>
    Task<Result> GoOperationalAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Read PDO (Process Data Object)
    /// </summary>
    Task<Result<byte[]>> ReadPDOAsync(int slaveIndex, int offset, int length, CancellationToken cancellationToken = default);

    /// <summary>
    /// Write PDO
    /// </summary>
    Task<Result> WritePDOAsync(int slaveIndex, int offset, byte[] data, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get slave info
    /// </summary>
    Task<Result<SlaveInfo>> GetSlaveInfoAsync(int slaveIndex, CancellationToken cancellationToken = default);
}

/// <summary>
/// EtherCAT slave information
/// </summary>
public class SlaveInfo
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint VendorId { get; set; }
    public uint ProductCode { get; set; }
    public string State { get; set; } = string.Empty;
}
