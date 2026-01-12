using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.IO;
using Serilog;

namespace NAutoSuite.Core.Machine;

/// <summary>
/// Xử lý tự động tất cả Common IO (EMG, Start, Stop, Reset, Tower Lights, Buzzer)
/// Class này chạy trong Scan Cycle, tự động xử lý mọi common IO
/// Developer KHÔNG cần viết lại code này cho mỗi project
/// </summary>
public class CommonIOHandler
{
    private readonly ILogger _logger;
    private readonly IOMap _ioMap;
    private readonly IMachine _machine;
    private readonly Func<string, CancellationToken, Task<bool>>? _readInputFunc;
    private readonly Func<string, bool, CancellationToken, Task>? _writeOutputFunc;

    // Previous button states (để phát hiện rising edge)
    private bool _prevEmgState;
    private bool _prevStartState;
    private bool _prevStopState;
    private bool _prevResetState;

    // Tower light flash timer
    private int _flashCounter;
    private const int FLASH_CYCLE = 5; // Flash every 5 scan cycles (500ms if scan = 100ms)

    public CommonIOHandler(
        IMachine machine,
        IOMap ioMap,
        Func<string, CancellationToken, Task<bool>>? readInputFunc = null,
        Func<string, bool, CancellationToken, Task>? writeOutputFunc = null,
        ILogger? logger = null)
    {
        _machine = machine;
        _ioMap = ioMap;
        _readInputFunc = readInputFunc;
        _writeOutputFunc = writeOutputFunc;
        _logger = logger ?? Log.Logger;
    }

    /// <summary>
    /// Scan Cycle Main - Gọi method này trong scan cycle của máy
    /// Method này xử lý TẤT CẢ common IO tự động
    /// </summary>
    public async Task ScanAsync(CancellationToken ct = default)
    {
        try
        {
            // 1. Đọc và xử lý tất cả buttons
            await ProcessButtonsAsync(ct);

            // 2. Cập nhật tower lights theo trạng thái máy
            await UpdateTowerLightsAsync(ct);

            // 3. Kiểm tra safety sensors
            await CheckSafetySensorsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "CommonIOHandler scan error");
        }
    }

    /// <summary>
    /// Đọc và xử lý tất cả common buttons (EMG, Start, Stop, Reset)
    /// </summary>
    private async Task ProcessButtonsAsync(CancellationToken ct)
    {
        if (_readInputFunc == null) return;

        // Đọc tất cả buttons
        bool emg = await ReadInputSafeAsync(_ioMap.CommonInputs.EmergencyStop, ct);
        bool start = await ReadInputSafeAsync(_ioMap.CommonInputs.StartButton, ct);
        bool stop = await ReadInputSafeAsync(_ioMap.CommonInputs.StopButton, ct);
        bool reset = await ReadInputSafeAsync(_ioMap.CommonInputs.ResetButton, ct);

        // ===== EMERGENCY STOP =====
        // EMG có tín hiệu (level trigger, không cần edge)
        if (emg && _machine.State != MachineState.EmergencyStop)
        {
            _logger.Warning("Emergency Stop triggered!");
            await _machine.EmergencyStopAsync(ct);
        }

        // ===== START BUTTON =====
        // Rising edge detection (nút được nhấn mới)
        if (start && !_prevStartState)
        {
            if (_machine.State == MachineState.Idle)
            {
                _logger.Information("Start button pressed - Starting machine");
                await _machine.StartAsync(ct);
            }
            else
            {
                _logger.Warning("Start button pressed but machine is in {State}", _machine.State);
            }
        }

        // ===== STOP BUTTON =====
        // Rising edge detection
        if (stop && !_prevStopState)
        {
            if (_machine.State == MachineState.Running)
            {
                _logger.Information("Stop button pressed - Stopping machine");
                await _machine.StopAsync(ct);
            }
            else
            {
                _logger.Warning("Stop button pressed but machine is in {State}", _machine.State);
            }
        }

        // ===== RESET BUTTON =====
        // Rising edge detection
        if (reset && !_prevResetState)
        {
            _logger.Information("Reset button pressed - Resetting machine");
            await _machine.ResetAsync(ct);
        }

        // Save current states for next cycle
        _prevEmgState = emg;
        _prevStartState = start;
        _prevStopState = stop;
        _prevResetState = reset;
    }

    /// <summary>
    /// Cập nhật tower lights tự động theo trạng thái máy
    /// </summary>
    private async Task UpdateTowerLightsAsync(CancellationToken ct)
    {
        if (_writeOutputFunc == null) return;

        _flashCounter++;
        bool flashOn = (_flashCounter % FLASH_CYCLE) < (FLASH_CYCLE / 2);

        switch (_machine.State)
        {
            case MachineState.Uninitialized:
            case MachineState.Initializing:
                // Đèn vàng nhấp nháy khi đang khởi tạo
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightGreen, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightYellow, flashOn, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightRed, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.Buzzer, false, ct);
                break;

            case MachineState.Idle:
            case MachineState.Stopped:
                // Đèn vàng sáng khi Idle
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightGreen, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightYellow, true, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightRed, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.Buzzer, false, ct);
                break;

            case MachineState.Running:
                // Đèn xanh sáng khi đang chạy
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightGreen, true, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightYellow, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightRed, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.Buzzer, false, ct);
                break;

            case MachineState.Paused:
                // Đèn xanh nhấp nháy khi tạm dừng
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightGreen, flashOn, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightYellow, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightRed, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.Buzzer, false, ct);
                break;

            case MachineState.EmergencyStop:
            case MachineState.Error:
                // Đèn đỏ nhấp nháy + còi khi lỗi/EMG
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightGreen, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightYellow, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightRed, flashOn, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.Buzzer, flashOn, ct);
                break;

            case MachineState.Stopping:
                // Đèn vàng + đỏ khi đang dừng
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightGreen, false, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightYellow, true, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.TowerLightRed, true, ct);
                await WriteOutputSafeAsync(_ioMap.CommonOutputs.Buzzer, false, ct);
                break;
        }
    }

    /// <summary>
    /// Kiểm tra safety sensors (Safety door, Air pressure)
    /// </summary>
    private async Task CheckSafetySensorsAsync(CancellationToken ct)
    {
        if (_readInputFunc == null) return;

        // Kiểm tra Safety Door
        bool doorOpen = await ReadInputSafeAsync(_ioMap.CommonInputs.SafetyDoor, ct);
        if (doorOpen && _machine.State == MachineState.Running)
        {
            _logger.Warning("Safety door opened during operation - Stopping machine");
            await _machine.StopAsync(ct);
        }

        // Kiểm tra Air Pressure
        bool airOk = await ReadInputSafeAsync(_ioMap.CommonInputs.AirPressure, ct);
        if (!airOk && _machine.State == MachineState.Running)
        {
            _logger.Warning("Air pressure lost during operation - Stopping machine");
            await _machine.StopAsync(ct);
        }
    }

    /// <summary>
    /// Helper: Đọc input an toàn (không throw exception)
    /// </summary>
    private async Task<bool> ReadInputSafeAsync(string address, CancellationToken ct)
    {
        try
        {
            if (_readInputFunc == null) return false;
            return await _readInputFunc(address, ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to read input {Address}", address);
            return false;
        }
    }

    /// <summary>
    /// Helper: Ghi output an toàn (không throw exception)
    /// </summary>
    private async Task WriteOutputSafeAsync(string address, bool value, CancellationToken ct)
    {
        try
        {
            if (_writeOutputFunc == null) return;
            await _writeOutputFunc(address, value, ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write output {Address}", address);
        }
    }
}
