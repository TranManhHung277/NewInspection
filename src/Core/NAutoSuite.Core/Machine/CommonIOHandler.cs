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
    private bool _prevHomeState;
    private bool _prevAutoSwitchState;
    private bool _prevManualSwitchState;
    private bool _prevMaterialLowState;

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
        var emgAddr = _ioMap.CommonInputs.EmergencyStop;
        var startAddr = _ioMap.CommonInputs.StartButton;
        var stopAddr = _ioMap.CommonInputs.StopButton;
        var resetAddr = _ioMap.CommonInputs.ResetButton;
        var homeAddr = _ioMap.CommonInputs.HomeButton;
        var autoAddr = _ioMap.CommonInputs.AutoModeSwitch;
        var manualAddr = _ioMap.CommonInputs.ManualModeSwitch;

        bool emg = !string.IsNullOrWhiteSpace(emgAddr) && _ioImage.GetInput(emgAddr);
        bool start = !string.IsNullOrWhiteSpace(startAddr) && _ioImage.GetInput(startAddr);
        bool stop = !string.IsNullOrWhiteSpace(stopAddr) && _ioImage.GetInput(stopAddr);
        bool reset = !string.IsNullOrWhiteSpace(resetAddr) && _ioImage.GetInput(resetAddr);
        bool home = !string.IsNullOrWhiteSpace(homeAddr) && _ioImage.GetInput(homeAddr);
        bool autoSwitch = !string.IsNullOrWhiteSpace(autoAddr) && _ioImage.GetInput(autoAddr);
        bool manualSwitch = !string.IsNullOrWhiteSpace(manualAddr) && _ioImage.GetInput(manualAddr);

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

        // ===== HOME BUTTON =====
        if (home && !_prevHomeState)
        {
            if (_machine.State == MachineState.Idle || _machine.State == MachineState.Stopped)
            {
                _logger.Information("Home button pressed - Homing machine");
                await _machine.HomeAsync(ct);
            }
            else
            {
                _logger.Warning("Home button pressed but machine is in {State}", _machine.State);
            }
        }

        // ===== AUTO/MANUAL SWITCH =====
        if (_machine is MachineBase machineBase)
        {
            var hasSwitchInput = !string.IsNullOrWhiteSpace(autoAddr) || !string.IsNullOrWhiteSpace(manualAddr);

            if (!hasSwitchInput)
            {
                if (machineBase.RunMode != MachineRunMode.Manual)
                {
                    machineBase.SetRunMode(MachineRunMode.Manual);
                }
            }
            else if (autoSwitch != _prevAutoSwitchState || manualSwitch != _prevManualSwitchState)
            {
                if (autoSwitch && !manualSwitch)
                {
                    machineBase.SetRunMode(MachineRunMode.Auto);
                }
                else if (manualSwitch && !autoSwitch)
                {
                    machineBase.SetRunMode(MachineRunMode.Manual);
                }
                else if (!autoSwitch && !manualSwitch)
                {
                    machineBase.SetRunMode(MachineRunMode.Manual);
                }
            }
        }

        // Save current states for next cycle
        _prevStartState = start;
        _prevStopState = stop;
        _prevResetState = reset;
        _prevHomeState = home;
        _prevAutoSwitchState = autoSwitch;
        _prevManualSwitchState = manualSwitch;
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
        var safetyAddr = _ioMap.CommonInputs.SafetyDoor;
        bool doorOpen = !string.IsNullOrWhiteSpace(safetyAddr) && _ioImage.GetInput(safetyAddr);
        if (!string.IsNullOrWhiteSpace(safetyAddr) && doorOpen && _machine.State == MachineState.Running)
        {
            _logger.Warning("Safety door opened during operation - Stopping machine");
            await _machine.StopAsync(ct);
        }

        // Kiểm tra Air Pressure
        var airAddr = _ioMap.CommonInputs.AirPressure;
        bool airOk = !string.IsNullOrWhiteSpace(airAddr) && _ioImage.GetInput(airAddr);
        if (!string.IsNullOrWhiteSpace(airAddr) && !airOk && _machine.State == MachineState.Running)
        {
            _logger.Warning("Air pressure lost during operation - Stopping machine");
            await _machine.StopAsync(ct);
        }

        var materialAddr = _ioMap.CommonInputs.MaterialLow;
        bool materialLow = !string.IsNullOrWhiteSpace(materialAddr) && _ioImage.GetInput(materialAddr);
        if (!string.IsNullOrWhiteSpace(materialAddr) && materialLow && !_prevMaterialLowState)
        {
            _logger.Warning("Material low during operation");
        }
        else if (!string.IsNullOrWhiteSpace(materialAddr) && !materialLow && _prevMaterialLowState)
        {
            _logger.Information("Material restored");
        }

        _prevMaterialLowState = materialLow;
    }
}
