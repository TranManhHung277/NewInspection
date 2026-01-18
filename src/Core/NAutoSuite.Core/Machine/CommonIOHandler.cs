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
    private readonly IOImage _ioImage;

    // Previous button states (để phát hiện rising edge)
    private bool _prevStartState;
    private bool _prevStopState;
    private bool _prevResetState;

    // Tower light flash timer
    private int _flashCounter;
    private const int FLASH_CYCLE = 5; // Flash every 5 scan cycles (500ms if scan = 100ms)

    public CommonIOHandler(
        IMachine machine,
        IOMap ioMap,
        IOImage ioImage,
        ILogger? logger = null)
    {
        _machine = machine;
        _ioMap = ioMap;
        _ioImage = ioImage;
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
        // Đọc tất cả buttons
        bool emg = _ioImage.GetInput(_ioMap.CommonInputs.EmergencyStop);
        bool start = _ioImage.GetInput(_ioMap.CommonInputs.StartButton);
        bool stop = _ioImage.GetInput(_ioMap.CommonInputs.StopButton);
        bool reset = _ioImage.GetInput(_ioMap.CommonInputs.ResetButton);

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
        _prevStartState = start;
        _prevStopState = stop;
        _prevResetState = reset;
    }

    /// <summary>
    /// Cập nhật tower lights tự động theo trạng thái máy
    /// </summary>
    private Task UpdateTowerLightsAsync(CancellationToken ct)
    {
        _flashCounter++;
        bool flashOn = (_flashCounter % FLASH_CYCLE) < (FLASH_CYCLE / 2);

        switch (_machine.State)
        {
            case MachineState.Uninitialized:
            case MachineState.Initializing:
                // Đèn vàng nhấp nháy khi đang khởi tạo
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightGreen, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightYellow, flashOn);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightRed, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.Buzzer, false);
                break;

            case MachineState.Idle:
            case MachineState.Stopped:
                // Đèn vàng sáng khi Idle
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightGreen, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightYellow, true);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightRed, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.Buzzer, false);
                break;

            case MachineState.Running:
                // Đèn xanh sáng khi đang chạy
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightGreen, true);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightYellow, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightRed, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.Buzzer, false);
                break;

            case MachineState.Paused:
                // Đèn xanh nhấp nháy khi tạm dừng
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightGreen, flashOn);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightYellow, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightRed, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.Buzzer, false);
                break;

            case MachineState.EmergencyStop:
            case MachineState.Error:
                // Đèn đỏ nhấp nháy + còi khi lỗi/EMG
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightGreen, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightYellow, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightRed, flashOn);
                _ioImage.SetOutput(_ioMap.CommonOutputs.Buzzer, flashOn);
                break;

            case MachineState.Stopping:
                // Đèn vàng + đỏ khi đang dừng
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightGreen, false);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightYellow, true);
                _ioImage.SetOutput(_ioMap.CommonOutputs.TowerLightRed, true);
                _ioImage.SetOutput(_ioMap.CommonOutputs.Buzzer, false);
                break;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Kiểm tra safety sensors (Safety door, Air pressure)
    /// </summary>
    private async Task CheckSafetySensorsAsync(CancellationToken ct)
    {
        // Kiểm tra Safety Door
        bool doorOpen = _ioImage.GetInput(_ioMap.CommonInputs.SafetyDoor);
        if (doorOpen && _machine.State == MachineState.Running)
        {
            _logger.Warning("Safety door opened during operation - Stopping machine");
            await _machine.StopAsync(ct);
        }

        // Kiểm tra Air Pressure
        bool airOk = _ioImage.GetInput(_ioMap.CommonInputs.AirPressure);
        if (!airOk && _machine.State == MachineState.Running)
        {
            _logger.Warning("Air pressure lost during operation - Stopping machine");
            await _machine.StopAsync(ct);
        }
    }
}
