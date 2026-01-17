using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine.PickAndPlace.Services;
using NAutoSuite.Core.Machine;
using PickAndPlace.Frontend.Machine;
using Serilog;
using System.Windows.Threading;

namespace Machine.PickAndPlace.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PickAndPlaceMachineFrontend _machine;
    private readonly DispatcherTimer _updateTimer;
    private readonly SettingsService _settingsService;

    [ObservableProperty]
    private MachineState _machineState = MachineState.Uninitialized;

    [ObservableProperty]
    private int _machineNumber = 1;

    [ObservableProperty]
    private string _projectName = "Pick & Place Machine (EtherCAT)";

    [ObservableProperty]
    private string _alarmMessage = string.Empty;

    [ObservableProperty]
    private bool _hasAlarm;

    [ObservableProperty]
    private long _cycleCount;

    [ObservableProperty]
    private string _runTime = "00:00:00";

    [ObservableProperty]
    private object? _currentView;

    // Tower Lights
    [ObservableProperty]
    private bool _towerLightGreen;

    [ObservableProperty]
    private bool _towerLightYellow;

    [ObservableProperty]
    private bool _towerLightRed;

    [ObservableProperty]
    private bool _buzzer;

    // Axis Status
    [ObservableProperty]
    private double _axisPosition;

    [ObservableProperty]
    private bool _isAxisMoving;

    [ObservableProperty]
    private string _statusMessage = "Initializing...";

    public MainViewModel(PickAndPlaceMachineFrontend machine)
    {
        _machine = machine;

        // Initialize settings service and load saved settings
        _settingsService = new SettingsService(Log.Logger);
        var settings = _settingsService.Load();
        _machineNumber = settings.MachineNumber;
        _projectName = settings.ProjectName;

        // Subscribe to machine state changes
        _machine.StateChanged += OnMachineStateChanged;

        // Setup update timer (100ms)
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _updateTimer.Tick += UpdateTimerTick;
        _updateTimer.Start();
    }

    partial void OnMachineNumberChanged(int value)
    {
        _settingsService.SaveMachineNumber(value);
    }

    partial void OnProjectNameChanged(string value)
    {
        _settingsService.SaveProjectName(value);
    }

    private void OnMachineStateChanged(object? sender, MachineState newState)
    {
        MachineState = newState;
        UpdateTowerLights();
        UpdateStatusMessage();
    }

    private void UpdateTimerTick(object? sender, EventArgs e)
    {
        try
        {
            // Update machine status
            MachineState = _machine.State;
            CycleCount = _machine.Context.CycleCount;
            RunTime = _machine.Context.TotalRunTime.ToString(@"hh\:mm\:ss");

            // Update axis status
            AxisPosition = _machine.CurrentPosition;
            IsAxisMoving = _machine.IsAxisMoving;

            // Update alarms
            HasAlarm = _machine.HasActiveAlarms;
            if (HasAlarm)
            {
                var firstAlarm = _machine.ActiveAlarms.FirstOrDefault();
                AlarmMessage = firstAlarm != null ? $"[{firstAlarm.Code}] {firstAlarm.Message}" : "";
            }
            else
            {
                AlarmMessage = "";
            }

            // Update tower lights
            UpdateTowerLights();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating UI");
        }
    }

    private void UpdateTowerLights()
    {
        // Reset all lights
        TowerLightGreen = false;
        TowerLightYellow = false;
        TowerLightRed = false;
        Buzzer = false;

        switch (MachineState)
        {
            case MachineState.Uninitialized:
            case MachineState.Initializing:
                TowerLightYellow = true;  // Blinking được handle bởi animation trong XAML
                break;

            case MachineState.Idle:
            case MachineState.Stopped:
                TowerLightYellow = true;
                break;

            case MachineState.Running:
                TowerLightGreen = true;
                break;

            case MachineState.Paused:
                TowerLightGreen = true;  // Blinking
                break;

            case MachineState.EmergencyStop:
            case MachineState.Error:
                TowerLightRed = true;  // Blinking
                Buzzer = true;
                break;

            case MachineState.Stopping:
                TowerLightYellow = true;
                TowerLightRed = true;
                break;
        }
    }

    private void UpdateStatusMessage()
    {
        StatusMessage = MachineState switch
        {
            MachineState.Uninitialized => "System ready - Please initialize",
            MachineState.Initializing => "Initializing hardware...",
            MachineState.Idle => "Ready to start",
            MachineState.Running => $"Running - Cycle {CycleCount + 1}",
            MachineState.Paused => "Paused",
            MachineState.Stopping => "Stopping...",
            MachineState.Stopped => "Stopped",
            MachineState.Error => $"Error: {AlarmMessage}",
            MachineState.EmergencyStop => "EMERGENCY STOP!",
            _ => "Unknown state"
        };
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        try
        {
            StatusMessage = "Initializing...";
            var result = await _machine.InitializeAsync();
            if (!result.IsSuccess)
            {
                StatusMessage = $"Initialize failed: {result.Message}";
                Log.Error("Initialize failed: {Message}", result.Message);
            }
            else
            {
                StatusMessage = "Initialized successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Initialize error: {ex.Message}";
            Log.Error(ex, "Initialize error");
        }
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        try
        {
            StatusMessage = "Starting...";
            var result = await _machine.StartAsync();
            if (!result.IsSuccess)
            {
                StatusMessage = $"Start failed: {result.Message}";
                Log.Error("Start failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Start error: {ex.Message}";
            Log.Error(ex, "Start error");
        }
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        try
        {
            StatusMessage = "Stopping...";
            var result = await _machine.StopAsync();
            if (!result.IsSuccess)
            {
                StatusMessage = $"Stop failed: {result.Message}";
                Log.Error("Stop failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Stop error: {ex.Message}";
            Log.Error(ex, "Stop error");
        }
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        try
        {
            StatusMessage = "Resetting...";
            var result = await _machine.ResetAsync();
            if (!result.IsSuccess)
            {
                StatusMessage = $"Reset failed: {result.Message}";
                Log.Error("Reset failed: {Message}", result.Message);
            }
            else
            {
                HasAlarm = false;
                AlarmMessage = string.Empty;
                StatusMessage = "Reset successful";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Reset error: {ex.Message}";
            Log.Error(ex, "Reset error");
        }
    }

    [RelayCommand]
    private async Task HomeAllAsync()
    {
        try
        {
            StatusMessage = "Homing axis...";
            var result = await _machine.HomeAxisAsync();
            if (!result.IsSuccess)
            {
                StatusMessage = $"Homing failed: {result.Message}";
                Log.Error("Homing failed: {Message}", result.Message);
            }
            else
            {
                StatusMessage = "Axis homed successfully";
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Homing error: {ex.Message}";
            Log.Error(ex, "Homing error");
        }
    }

    [RelayCommand]
    private async Task EmergencyStopAsync()
    {
        try
        {
            StatusMessage = "EMERGENCY STOP!";
            var result = await _machine.EmergencyStopAsync();
            if (!result.IsSuccess)
            {
                Log.Error("Emergency stop failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Emergency stop error");
        }
    }

    public void NavigateToView(object view)
    {
        CurrentView = view;
    }
}
