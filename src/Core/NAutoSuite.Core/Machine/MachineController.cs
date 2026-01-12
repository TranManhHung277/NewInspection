using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.IO;
using NAutoSuite.Core.Data;
using Serilog;

namespace NAutoSuite.Core.Machine;

/// <summary>
/// Standard machine controller for ALL automation machines
/// Handles common operations: EMG, Start, Stop, Reset, Init
/// Each machine uses this controller - just map IO and data
/// </summary>
public class MachineController
{
    private readonly IOMap _ioMap;
    private readonly MachineData _machineData;
    private readonly IIO? _io;
    private readonly ILogger _logger;
    private readonly List<IAxis> _axes = new();
    private readonly List<IOutput> _outputs = new();

    // State
    private bool _emergencyActive;
    private bool _previousEmgState;
    private bool _previousStartState;
    private bool _previousStopState;
    private bool _previousResetState;

    public bool EmergencyActive => _emergencyActive;

    public MachineController(
        IOMap ioMap,
        MachineData machineData,
        IIO? io,
        ILogger logger)
    {
        _ioMap = ioMap;
        _machineData = machineData;
        _io = io;
        _logger = logger;
    }

    #region Registration

    /// <summary>
    /// Register axis to be controlled by this controller
    /// </summary>
    public void RegisterAxis(IAxis axis)
    {
        if (!_axes.Contains(axis))
        {
            _axes.Add(axis);
            _logger.Information("MachineController: Registered axis");
        }
    }

    /// <summary>
    /// Register output to be controlled by this controller
    /// </summary>
    public void RegisterOutput(IOutput output)
    {
        if (!_outputs.Contains(output))
        {
            _outputs.Add(output);
            _logger.Information("MachineController: Registered output");
        }
    }

    #endregion

    #region Cyclic Update

    /// <summary>
    /// Cyclic update - call this every scan cycle
    /// Reads common IOs and handles button presses
    /// </summary>
    public async Task TickAsync(CancellationToken ct = default)
    {
        if (_io == null) return;

        try
        {
            // Read common inputs
            bool emgPressed = await ReadInputAsync(_ioMap.CommonInputs.EmergencyStop, ct);
            bool startPressed = await ReadInputAsync(_ioMap.CommonInputs.StartButton, ct);
            bool stopPressed = await ReadInputAsync(_ioMap.CommonInputs.StopButton, ct);
            bool resetPressed = await ReadInputAsync(_ioMap.CommonInputs.ResetButton, ct);

            // EMG - Rising edge detection
            if (emgPressed && !_previousEmgState)
            {
                await HandleEmergencyAsync(ct);
            }

            // Start button - Rising edge
            if (startPressed && !_previousStartState)
            {
                await HandleStartButtonAsync(ct);
            }

            // Stop button - Rising edge
            if (stopPressed && !_previousStopState)
            {
                await HandleStopButtonAsync(ct);
            }

            // Reset button - Rising edge
            if (resetPressed && !_previousResetState)
            {
                await HandleResetButtonAsync(ct);
            }

            // Save previous states
            _previousEmgState = emgPressed;
            _previousStartState = startPressed;
            _previousStopState = stopPressed;
            _previousResetState = resetPressed;

            // Update tower lights based on state
            await UpdateTowerLightsAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "MachineController: Error in TickAsync");
        }
    }

    #endregion

    #region Standard Handlers

    /// <summary>
    /// Handle emergency stop
    /// - Stop all motors
    /// - Show alarm
    /// - Cancel init state
    /// - Disable outputs
    /// - Turn on red tower light
    /// - Sound buzzer
    /// </summary>
    private async Task HandleEmergencyAsync(CancellationToken ct)
    {
        _logger.Warning("MachineController: EMERGENCY STOP activated!");
        _emergencyActive = true;

        // 1. Stop all motors immediately
        foreach (var axis in _axes)
        {
            try
            {
                await axis.StopAsync(emergency: true);
                _logger.Information("MachineController: Stopped axis in emergency");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "MachineController: Failed to stop axis in emergency");
            }
        }

        // 2. Disable all outputs (except tower lights and buzzer)
        foreach (var output in _outputs)
        {
            try
            {
                await output.WriteAsync(false, ct);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "MachineController: Failed to disable output in emergency");
            }
        }

        // 3. Turn on red tower light
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightRed, true, ct);
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightYellow, false, ct);
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightGreen, false, ct);

        // 4. Sound buzzer
        if (_machineData.Common.BuzzerOnAlarm)
        {
            await WriteOutputAsync(_ioMap.CommonOutputs.Buzzer, true, ct);
        }

        // 5. Disable main power (if configured)
        await WriteOutputAsync(_ioMap.CommonOutputs.MainPowerEnable, false, ct);

        _logger.Information("MachineController: Emergency stop completed");
    }

    /// <summary>
    /// Handle Start button press
    /// Override this in derived class to implement custom start logic
    /// </summary>
    protected virtual async Task HandleStartButtonAsync(CancellationToken ct)
    {
        _logger.Information("MachineController: Start button pressed");

        // Check if emergency is active
        if (_emergencyActive)
        {
            _logger.Warning("MachineController: Cannot start - emergency is active");
            return;
        }

        // Turn on green tower light
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightGreen, true, ct);
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightRed, false, ct);
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightYellow, false, ct);

        // Turn off buzzer
        await WriteOutputAsync(_ioMap.CommonOutputs.Buzzer, false, ct);

        _logger.Information("MachineController: Start handled");
    }

    /// <summary>
    /// Handle Stop button press
    /// </summary>
    protected virtual async Task HandleStopButtonAsync(CancellationToken ct)
    {
        _logger.Information("MachineController: Stop button pressed");

        // Stop all axes (controlled stop, not emergency)
        foreach (var axis in _axes)
        {
            try
            {
                await axis.StopAsync(emergency: false);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "MachineController: Failed to stop axis");
            }
        }

        // Turn on yellow tower light
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightYellow, true, ct);
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightGreen, false, ct);

        _logger.Information("MachineController: Stop handled");
    }

    /// <summary>
    /// Handle Reset button press
    /// - Clear alarms
    /// - Reset emergency state
    /// - Turn off buzzer
    /// </summary>
    protected virtual async Task HandleResetButtonAsync(CancellationToken ct)
    {
        _logger.Information("MachineController: Reset button pressed");

        // Clear emergency state
        _emergencyActive = false;

        // Turn off buzzer
        await WriteOutputAsync(_ioMap.CommonOutputs.Buzzer, false, ct);

        // Turn on yellow tower light (ready state)
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightYellow, true, ct);
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightRed, false, ct);
        await WriteOutputAsync(_ioMap.CommonOutputs.TowerLightGreen, false, ct);

        // Enable main power
        await WriteOutputAsync(_ioMap.CommonOutputs.MainPowerEnable, true, ct);

        _logger.Information("MachineController: Reset completed - system ready");
    }

    /// <summary>
    /// Initialize machine - home all axes
    /// </summary>
    public async Task<Result> InitializeAsync(CancellationToken ct = default)
    {
        _logger.Information("MachineController: Initializing machine - homing all axes");

        if (_emergencyActive)
        {
            return Result.Failure("Cannot initialize - emergency is active");
        }

        try
        {
            // Home all axes
            foreach (var axis in _axes)
            {
                _logger.Information("MachineController: Homing axis...");
                var result = await axis.HomeAsync(ct);

                if (!result.IsSuccess)
                {
                    _logger.Error("MachineController: Failed to home axis - {Message}", result.Message);
                    return Result.Failure($"Homing failed: {result.Message}");
                }

                // Check if emergency was triggered during homing
                if (_emergencyActive)
                {
                    _logger.Warning("MachineController: Homing cancelled - emergency activated");
                    return Result.Failure("Homing cancelled by emergency");
                }
            }

            _logger.Information("MachineController: All axes homed successfully");
            return Result.Success("Initialization complete");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "MachineController: Initialization failed");
            return Result.Failure("Initialization failed", ex);
        }
    }

    #endregion

    #region Tower Lights

    /// <summary>
    /// Update tower lights based on current state
    /// </summary>
    private async Task UpdateTowerLightsAsync(CancellationToken ct)
    {
        // This is called every cycle, but we only update on state changes
        // The specific states are handled in the button handlers above
    }

    #endregion

    #region IO Helpers

    /// <summary>
    /// Read input from PLC using IO map address
    /// </summary>
    private async Task<bool> ReadInputAsync(string address, CancellationToken ct)
    {
        if (_io == null || string.IsNullOrEmpty(address))
            return false;

        try
        {
            var result = await _io.ReadInputAsync(address, ct);
            return result.IsSuccess && result.Value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Write output to PLC using IO map address
    /// </summary>
    private async Task WriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        if (_io == null || string.IsNullOrEmpty(address))
            return;

        try
        {
            await _io.WriteOutputAsync(address, value, ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "MachineController: Failed to write output {Address}", address);
        }
    }

    #endregion

    #region Public Status

    /// <summary>
    /// Check if all axes are homed
    /// </summary>
    public bool AreAllAxesHomed()
    {
        return _axes.All(axis => axis.IsHomed);
    }

    /// <summary>
    /// Check if any axis is moving
    /// </summary>
    public bool IsAnyAxisMoving()
    {
        return _axes.Any(axis => axis.IsMoving);
    }

    /// <summary>
    /// Get all registered axes
    /// </summary>
    public IReadOnlyList<IAxis> GetAxes() => _axes.AsReadOnly();

    #endregion
}
