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
        if (!TryParseAxisAddress(address, out var axis, out var kind, out var extra))
        {
            return Task.FromResult(Result.Failure<double>($"Unsupported Leadshine register '{address}'"));
        }

        try
        {
            switch (kind)
            {
                case "pos":
                {
                    double pos = 0;
                    LTDMC.dmc_get_position_unit(_cardNo, axis, ref pos);
                    return Task.FromResult(Result.Success(pos));
                }
                case "target_pos":
                {
                    double pos = 0;
                    LTDMC.dmc_get_target_position_unit(_cardNo, axis, ref pos);
                    return Task.FromResult(Result.Success(pos));
                }
                case "cmd_pos":
                {
                    double pos = 0;
                    LTDMC.dmc_get_cmd_position(_cardNo, axis, ref pos);
                    return Task.FromResult(Result.Success(pos));
                }
                case "speed":
                {
                    double speed = 0;
                    LTDMC.dmc_read_current_speed_unit(_cardNo, axis, ref speed);
                    return Task.FromResult(Result.Success(speed));
                }
                case "enc_pos":
                {
                    double pos = 0;
                    LTDMC.dmc_get_encoder_unit(_cardNo, axis, ref pos);
                    return Task.FromResult(Result.Success(pos));
                }
                case "io_status":
                {
                    var status = LTDMC.dmc_axis_io_status(_cardNo, axis);
                    return Task.FromResult(Result.Success((double)status));
                }
                case "status_word":
                {
                    var status = 0;
                    LTDMC.nmc_get_axis_statusword(_cardNo, axis, ref status);
                    return Task.FromResult(Result.Success((double)status));
                }
                case "state_machine":
                {
                    ushort state = 0;
                    LTDMC.nmc_get_axis_state_machine(_cardNo, axis, ref state);
                    return Task.FromResult(Result.Success((double)state));
                }
                case "error_code":
                {
                    ushort code = 0;
                    LTDMC.nmc_get_axis_errcode(_cardNo, axis, ref code);
                    return Task.FromResult(Result.Success((double)code));
                }
                case "done":
                {
                    var done = LTDMC.dmc_check_done(_cardNo, axis);
                    return Task.FromResult(Result.Success(done == 1 ? 1.0 : 0.0));
                }
                case "io_status_bit":
                {
                    if (!int.TryParse(extra, out var bitIndex))
                    {
                        return Task.FromResult(Result.Failure<double>($"Invalid bit index '{extra}'"));
                    }
                    var status = LTDMC.dmc_axis_io_status(_cardNo, axis);
                    var value = (status & (1u << bitIndex)) != 0;
                    return Task.FromResult(Result.Success(value ? 1.0 : 0.0));
                }
                case "status_word_bit":
                {
                    if (!int.TryParse(extra, out var bitIndex))
                    {
                        return Task.FromResult(Result.Failure<double>($"Invalid bit index '{extra}'"));
                    }
                    var status = 0;
                    LTDMC.nmc_get_axis_statusword(_cardNo, axis, ref status);
                    var value = ((status >> bitIndex) & 0x1) != 0;
                    return Task.FromResult(Result.Success(value ? 1.0 : 0.0));
                }
            }

            return Task.FromResult(Result.Failure<double>($"Unsupported Leadshine register '{address}'"));
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Failed to read Leadshine register {Address}", address);
            return Task.FromResult(Result.Failure<double>($"Failed to read Leadshine register '{address}'", ex));
        }
    }

    public Task<Result> WriteRegisterAsync(string address, double value, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure("Leadshine register IO is read-only"));

    private static bool TryParseAxisAddress(string address, out ushort axis, out string kind, out string? extra)
    {
        axis = 0;
        kind = string.Empty;
        extra = null;
        if (string.IsNullOrWhiteSpace(address))
        {
            return false;
        }

        var parts = address.Split(':', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 3)
        {
            return false;
        }

        if (!string.Equals(parts[0], "axis", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!ushort.TryParse(parts[1], out axis))
        {
            return false;
        }

        kind = parts[2].Trim().ToLowerInvariant();
        if (parts.Length > 3)
        {
            extra = parts[3].Trim();
        }

        return true;
    }
}
