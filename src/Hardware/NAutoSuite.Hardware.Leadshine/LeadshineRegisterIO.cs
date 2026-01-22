using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Hardware.Leadshine;

public sealed class LeadshineRegisterIO : IRegisterIO
{
    private readonly ushort _cardNo;
    private readonly ILogger _logger;

    public LeadshineRegisterIO(ushort cardNo, ILogger logger)
    {
        _cardNo = cardNo;
        _logger = logger;
        Id = $"LeadshineRegister:{cardNo}";
        Name = $"Leadshine Register IO #{cardNo}";
    }

    public string Id { get; }

    public string Name { get; }

    public bool IsConnected { get; private set; }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.FromResult(Result.Success("Leadshine register IO ready"));
    }

    public Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = false;
        return Task.FromResult(Result.Success("Leadshine register IO disconnected"));
    }

    public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success("Leadshine register IO reset"));

    public Task<Result<double>> ReadRegisterAsync(string address, CancellationToken cancellationToken = default)
    {
        if (!TryParseAxisPosition(address, out var axis))
        {
            return Task.FromResult(Result.Failure<double>($"Unsupported Leadshine register '{address}'"));
        }

        try
        {
            double pos = 0;
            LTDMC.dmc_get_position_unit(_cardNo, axis, ref pos);
            return Task.FromResult(Result.Success(pos));
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read Leadshine register {Address}", address);
            return Task.FromResult(Result.Failure<double>($"Failed to read Leadshine register '{address}'", ex));
        }
    }

    public Task<Result> WriteRegisterAsync(string address, double value, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure("Leadshine register IO is read-only"));

    private static bool TryParseAxisPosition(string address, out ushort axis)
    {
        axis = 0;
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        var parts = address.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
        {
            return false;
        }

        if (!string.Equals(parts[0], "axis", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!string.Equals(parts[2], "pos", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return ushort.TryParse(parts[1], out axis);
    }
}
