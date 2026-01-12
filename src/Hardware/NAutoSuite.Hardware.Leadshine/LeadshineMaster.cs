using NAutoSuite.Core.Common;
using NAutoSuite.Hardware.Abstractions.EtherCAT;
using Serilog;

namespace NAutoSuite.Hardware.Leadshine;

/// <summary>
/// Leadshine EtherCAT Master controller implementation
/// </summary>
public class LeadshineMaster : IEtherCATMaster
{
    private readonly ushort _cardNo;
    private readonly string? _ipAddress;
    private readonly ILogger _logger;
    private bool _isConnected;
    private uint _totalAxes;
    private ushort _totalInputs;
    private ushort _totalOutputs;

    public string Id { get; }
    public string Name { get; }
    public bool IsConnected => _isConnected;

    /// <summary>
    /// Total number of axes available on this master
    /// </summary>
    public int TotalAxes => (int)_totalAxes;

    /// <summary>
    /// Total number of inputs
    /// </summary>
    public int TotalInputs => _totalInputs;

    /// <summary>
    /// Total number of outputs
    /// </summary>
    public int TotalOutputs => _totalOutputs;

    public LeadshineMaster(ushort cardNo, string? ipAddress = null, ILogger? logger = null)
    {
        _cardNo = cardNo;
        _ipAddress = ipAddress;
        _logger = logger ?? Log.Logger;
        Id = $"Leadshine_Card{cardNo}";
        Name = $"Leadshine EtherCAT Master #{cardNo}";
    }

    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            short result;

            if (!string.IsNullOrEmpty(_ipAddress))
            {
                // Connect via Ethernet
                _logger.Information("Connecting to Leadshine Master {CardNo} via Ethernet at {IpAddress}", _cardNo, _ipAddress);
                result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
            }
            else
            {
                // Connect via local
                _logger.Information("Connecting to Leadshine Master {CardNo} locally", _cardNo);
                result = LTDMC.dmc_board_init();
            }

            if (result != 0)
            {
                var errorMsg = $"Failed to initialize Leadshine card {_cardNo}, error code: {result}";
                _logger.Error(errorMsg);
                return Result.Failure(errorMsg);
            }

            // Get card information
            _totalAxes = 0;
            result = LTDMC.dmc_get_total_axes(_cardNo, ref _totalAxes);
            if (result != 0)
            {
                _logger.Warning("Failed to get total axes, error code: {ErrorCode}", result);
            }

            _totalInputs = 0;
            _totalOutputs = 0;
            result = LTDMC.dmc_get_total_ionum(_cardNo, ref _totalInputs, ref _totalOutputs);
            if (result != 0)
            {
                _logger.Warning("Failed to get IO count, error code: {ErrorCode}", result);
            }

            // Get card version
            uint cardVersion = 0;
            result = LTDMC.dmc_get_card_version(_cardNo, ref cardVersion);
            if (result == 0)
            {
                _logger.Information("Leadshine Card Version: 0x{Version:X}", cardVersion);
            }

            _isConnected = true;
            _logger.Information("Leadshine Master {CardNo} connected successfully. Axes: {Axes}, Inputs: {Inputs}, Outputs: {Outputs}",
                _cardNo, _totalAxes, _totalInputs, _totalOutputs);

            return Result.Success($"Connected to Leadshine Master (Axes: {_totalAxes}, IO: {_totalInputs}/{_totalOutputs})");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to connect Leadshine Master {CardNo}", _cardNo);
            return Result.Failure("Connection failed", ex);
        }
    }

    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = LTDMC.dmc_board_close();
            if (result != 0)
            {
                _logger.Warning("Disconnect returned error code: {ErrorCode}", result);
            }

            _isConnected = false;
            _logger.Information("Leadshine Master {CardNo} disconnected", _cardNo);
            return Result.Success("Disconnected");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to disconnect Leadshine Master {CardNo}", _cardNo);
            return Result.Failure("Disconnection failed", ex);
        }
    }

    public async Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Resetting Leadshine Master {CardNo}", _cardNo);
            var result = LTDMC.dmc_board_reset();

            if (result != 0)
            {
                var errorMsg = $"Failed to reset Leadshine card {_cardNo}, error code: {result}";
                _logger.Error(errorMsg);
                return Result.Failure(errorMsg);
            }

            _logger.Information("Leadshine Master {CardNo} reset successfully", _cardNo);
            return Result.Success("Reset complete");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to reset Leadshine Master {CardNo}", _cardNo);
            return Result.Failure("Reset failed", ex);
        }
    }

    public int GetSlaveCount()
    {
        // For Leadshine, slaves are the axes
        return (int)_totalAxes;
    }

    /// <summary>
    /// Soft reset the controller
    /// </summary>
    public Result SoftReset()
    {
        try
        {
            var result = LTDMC.dmc_soft_reset(_cardNo);
            if (result != 0)
            {
                return Result.Failure($"Soft reset failed, error code: {result}");
            }
            _logger.Information("Leadshine Master {CardNo} soft reset", _cardNo);
            return Result.Success("Soft reset complete");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Soft reset failed for Leadshine Master {CardNo}", _cardNo);
            return Result.Failure("Soft reset failed", ex);
        }
    }

    /// <summary>
    /// Emergency stop all axes
    /// </summary>
    public Result EmergencyStopAll()
    {
        try
        {
            var result = LTDMC.dmc_emg_stop(_cardNo);
            if (result != 0)
            {
                return Result.Failure($"Emergency stop failed, error code: {result}");
            }
            _logger.Warning("Emergency stop executed for all axes on card {CardNo}", _cardNo);
            return Result.Success("Emergency stop executed");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Emergency stop failed for card {CardNo}", _cardNo);
            return Result.Failure("Emergency stop failed", ex);
        }
    }

    /// <summary>
    /// Read input bit
    /// </summary>
    public bool ReadInput(ushort bitNo)
    {
        try
        {
            var value = LTDMC.dmc_read_inbit(_cardNo, bitNo);
            return value != 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read input bit {BitNo} on card {CardNo}", bitNo, _cardNo);
            return false;
        }
    }

    /// <summary>
    /// Write output bit
    /// </summary>
    public Result WriteOutput(ushort bitNo, bool value)
    {
        try
        {
            var result = LTDMC.dmc_write_outbit(_cardNo, bitNo, (ushort)(value ? 1 : 0));
            if (result != 0)
            {
                return Result.Failure($"Failed to write output bit {bitNo}, error code: {result}");
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write output bit {BitNo} on card {CardNo}", bitNo, _cardNo);
            return Result.Failure("Write output failed", ex);
        }
    }

    /// <summary>
    /// Read output bit state
    /// </summary>
    public bool ReadOutput(ushort bitNo)
    {
        try
        {
            var value = LTDMC.dmc_read_outbit(_cardNo, bitNo);
            return value != 0;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read output bit {BitNo} on card {CardNo}", bitNo, _cardNo);
            return false;
        }
    }

    /// <summary>
    /// Read input port (8 bits)
    /// </summary>
    public uint ReadInputPort(ushort portNo)
    {
        try
        {
            return LTDMC.dmc_read_inport(_cardNo, portNo);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read input port {PortNo} on card {CardNo}", portNo, _cardNo);
            return 0;
        }
    }

    /// <summary>
    /// Write output port (8 bits)
    /// </summary>
    public Result WriteOutputPort(ushort portNo, uint value)
    {
        try
        {
            var result = LTDMC.dmc_write_outport(_cardNo, portNo, value);
            if (result != 0)
            {
                return Result.Failure($"Failed to write output port {portNo}, error code: {result}");
            }
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write output port {PortNo} on card {CardNo}", portNo, _cardNo);
            return Result.Failure("Write output port failed", ex);
        }
    }
}
