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
    /// Number of slaves detected (for IEtherCATMaster interface)
    /// </summary>
    public int SlaveCount => (int)_totalAxes;

    /// <summary>
    /// Is in operational state (for IEtherCATMaster interface)
    /// </summary>
    public bool IsOperational => _isConnected;

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

    private bool _boardInitialized = false;

    public LeadshineMaster(ushort cardNo, string? ipAddress = null, ILogger? logger = null)
    {
        _cardNo = cardNo;
        _ipAddress = ipAddress;
        _logger = logger ?? Log.Logger;
        Id = $"Leadshine_Card{cardNo}";
        Name = $"Leadshine EtherCAT Master #{cardNo}";

        // Do NOT initialize board in constructor
        // Board initialization will be done in ConnectAsync() when user is ready
        _logger.Information("LeadshineMaster instance created (Card {CardNo}). Board will be initialized when ConnectAsync is called.", _cardNo);
    }

    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Initialize board (only once)
            if (!_boardInitialized)
            {
                _logger.Information("Initializing Leadshine board (Card {CardNo})...", _cardNo);

                short initResult;
                if (!string.IsNullOrEmpty(_ipAddress))
                {
                    // Connect via Ethernet
                    _logger.Information("Initializing via Ethernet at {IpAddress}", _ipAddress);
                    initResult = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
                }
                else
                {
                    // Connect via local
                    _logger.Information("Initializing locally (dmc_board_init)");
                    initResult = LTDMC.dmc_board_init();
                }

                // Check initialization result: dmc_board_init returns <= 0 on failure
                if (initResult <= 0)
                {
                    var errorMsg = $"Failed to initialize Leadshine card {_cardNo}, error code: {initResult}. Kiểm tra kết nối EtherCAT.";
                    _logger.Error(errorMsg);
                    return Result.Failure(errorMsg);
                }

                _boardInitialized = true;
                _logger.Information("Board initialized successfully (result={Result})", initResult);
            }
            else
            {
                _logger.Information("Board already initialized, skipping init");
            }

            // Step 2: Get card information
            _totalAxes = 0;
            var result = LTDMC.dmc_get_total_axes(_cardNo, ref _totalAxes);
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
            _logger.Warning("Resetting Leadshine Master {CardNo} - This will reinitialize the board", _cardNo);

            // Reset the board
            var result = LTDMC.dmc_board_reset();
            if (result != 0)
            {
                var errorMsg = $"Failed to reset Leadshine card {_cardNo}, error code: {result}";
                _logger.Error(errorMsg);
                return Result.Failure(errorMsg);
            }

            _isConnected = false;
            await Task.Delay(500, cancellationToken); // Wait for board to reset

            // Reinitialize board after reset
            _logger.Information("Reinitializing board after reset...");
            if (!string.IsNullOrEmpty(_ipAddress))
            {
                result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
            }
            else
            {
                result = LTDMC.dmc_board_init();
            }

            if (result <= 0)
            {
                var errorMsg = $"Failed to reinitialize board after reset, error code: {result}";
                _logger.Error(errorMsg);
                return Result.Failure(errorMsg);
            }

            _logger.Information("Leadshine Master {CardNo} reset and reinitialized successfully", _cardNo);

            // Reconnect to get card info
            return await ConnectAsync(cancellationToken);
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

    #region IEtherCATMaster Interface Implementation

    /// <summary>
    /// Scan for slaves (for Leadshine, this is done during ConnectAsync)
    /// </summary>
    public Task<Result> ScanAsync(CancellationToken cancellationToken = default)
    {
        // For Leadshine, scanning is done automatically during initialization
        // Return the current connection state
        if (_isConnected)
        {
            return Task.FromResult(Result.Success($"Leadshine Master has {_totalAxes} axes"));
        }
        return Task.FromResult(Result.Failure("Not connected. Call ConnectAsync first."));
    }

    /// <summary>
    /// Go to operational state (for Leadshine, this happens automatically)
    /// </summary>
    public Task<Result> GoOperationalAsync(CancellationToken cancellationToken = default)
    {
        // Leadshine goes operational automatically after successful connection
        if (_isConnected)
        {
            return Task.FromResult(Result.Success("Already operational"));
        }
        return Task.FromResult(Result.Failure("Not connected. Call ConnectAsync first."));
    }

    /// <summary>
    /// Read PDO (Process Data Object) - Not directly supported by Leadshine API
    /// </summary>
    public Task<Result<byte[]>> ReadPDOAsync(int slaveIndex, int offset, int length, CancellationToken cancellationToken = default)
    {
        // Leadshine doesn't expose direct PDO access
        // Use specific read methods like ReadInput, ReadInputPort instead
        _logger.Warning("Direct PDO read not supported by Leadshine. Use ReadInput/ReadInputPort methods.");
        return Task.FromResult(Result.Failure<byte[]>("Direct PDO access not supported by Leadshine API"));
    }

    /// <summary>
    /// Write PDO - Not directly supported by Leadshine API
    /// </summary>
    public Task<Result> WritePDOAsync(int slaveIndex, int offset, byte[] data, CancellationToken cancellationToken = default)
    {
        // Leadshine doesn't expose direct PDO access
        // Use specific write methods like WriteOutput, WriteOutputPort instead
        _logger.Warning("Direct PDO write not supported by Leadshine. Use WriteOutput/WriteOutputPort methods.");
        return Task.FromResult(Result.Failure("Direct PDO access not supported by Leadshine API"));
    }

    /// <summary>
    /// Get slave info
    /// </summary>
    public Task<Result<SlaveInfo>> GetSlaveInfoAsync(int slaveIndex, CancellationToken cancellationToken = default)
    {
        try
        {
            if (slaveIndex < 0 || slaveIndex >= _totalAxes)
            {
                return Task.FromResult(Result.Failure<SlaveInfo>($"Invalid slave index: {slaveIndex}"));
            }

            var slaveInfo = new SlaveInfo
            {
                Index = slaveIndex,
                Name = $"Leadshine Axis {slaveIndex}",
                VendorId = 0x00000A88, // Leadshine vendor ID
                ProductCode = 0,
                State = _isConnected ? "Operational" : "Init"
            };

            return Task.FromResult(Result.Success(slaveInfo, "Slave info retrieved"));
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get slave info for index {SlaveIndex}", slaveIndex);
            return Task.FromResult(Result.Failure<SlaveInfo>("Failed to get slave info", ex));
        }
    }

    #endregion
}
