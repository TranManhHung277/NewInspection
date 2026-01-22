using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Hardware.Leadshine;

/// <summary>
/// Remote IO access via Leadshine EtherCAT (NMC functions).
/// </summary>
public class LeadshineRemoteIO : IIO
{
    private readonly int _cardNo;
    private readonly Dictionary<string, RemoteIoPoint> _points;
    private readonly ILogger _logger;
    private bool _isConnected;

    public LeadshineRemoteIO(int cardNo, IEnumerable<RemoteIoPoint> points, ILogger? logger = null)
    {
        _cardNo = cardNo;
        _logger = logger ?? Log.Logger;
        _points = new Dictionary<string, RemoteIoPoint>(StringComparer.OrdinalIgnoreCase);

        foreach (var point in points)
        {
            if (string.IsNullOrWhiteSpace(point.Address))
            {
                continue;
            }

            if (_points.ContainsKey(point.Address))
            {
                _logger.Warning("Duplicate Leadshine IO address '{Address}' ignored", point.Address);
                continue;
            }

            _points[point.Address] = point;
        }
    }

    public string Id => $"LeadshineRemoteIO_Card{_cardNo}";
    public string Name => $"Leadshine Remote IO (Card {_cardNo})";
    public bool IsConnected => _isConnected;

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        _isConnected = true;
        return Task.FromResult(Result.Success("Remote IO ready"));
    }

    public Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _isConnected = false;
        return Task.FromResult(Result.Success("Remote IO disconnected"));
    }

    public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success("Remote IO reset"));

    public Task<Result<bool>> ReadInputAsync(string address, CancellationToken cancellationToken = default)
    {
        if (!_points.TryGetValue(address, out var point) || point.IsOutput)
        {
            return Task.FromResult(Result.Failure<bool>($"Unknown input address {address}"));
        }

        ushort value = 0;
        var result = LTDMC.nmc_read_inbit((ushort)_cardNo, point.NodeId, point.IoBit, ref value);
        if (result != 0)
        {
            return Task.FromResult(Result.Failure<bool>($"Read input failed {address} (err={result})"));
        }

        return Task.FromResult(Result.Success(value != 0));
    }

    public Task<Result> WriteOutputAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        if (!_points.TryGetValue(address, out var point) || !point.IsOutput)
        {
            return Task.FromResult(Result.Failure($"Unknown output address {address}"));
        }

        var result = LTDMC.nmc_write_outbit((ushort)_cardNo, point.NodeId, point.IoBit, (ushort)(value ? 1 : 0));
        if (result != 0)
        {
            return Task.FromResult(Result.Failure($"Write output failed {address} (err={result})"));
        }

        return Task.FromResult(Result.Success("Output updated"));
    }
}

public class RemoteIoPoint
{
    public string Address { get; set; } = string.Empty;
    public ushort NodeId { get; set; }
    public ushort IoBit { get; set; }
    public bool IsOutput { get; set; }
}
