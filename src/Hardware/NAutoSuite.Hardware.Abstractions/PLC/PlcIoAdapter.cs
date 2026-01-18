using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;

namespace NAutoSuite.Hardware.Abstractions.PLC;

/// <summary>
/// Adapter to expose an IPlc as a generic IO device.
/// </summary>
public class PlcIoAdapter : IIO
{
    private readonly IPlc _plc;

    public PlcIoAdapter(IPlc plc)
    {
        _plc = plc ?? throw new ArgumentNullException(nameof(plc));
    }

    public string Id => _plc.Id;

    public string Name => _plc.Name;

    public bool IsConnected => _plc.IsConnected;

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
        => _plc.ConnectAsync(cancellationToken);

    public Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
        => _plc.DisconnectAsync(cancellationToken);

    public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
        => _plc.ResetAsync(cancellationToken);

    public Task<Result<bool>> ReadInputAsync(string address, CancellationToken cancellationToken = default)
        => _plc.ReadBitAsync(address, cancellationToken);

    public Task<Result> WriteOutputAsync(string address, bool value, CancellationToken cancellationToken = default)
        => _plc.WriteBitAsync(address, value, cancellationToken);
}
