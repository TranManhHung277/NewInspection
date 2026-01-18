using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PickAndPlace.Services;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Machine;
using NAutoSuite.Core.Model;
using NAutoSuite.UI.Controls;
using PickAndPlace.Machine;
using PickAndPlace.Model;
using NAutoSuite.Hardware.Simulator;
using Serilog;
using System.Windows.Threading;

namespace PickAndPlace.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PickAndPlaceMachine _machine;
    private readonly LeadshineEthercatSimulator _ioSimulator;
    private readonly SimulatorAxis? _axisXSim;
    private readonly SimulatorAxis? _axisYSim;
    private readonly SimulatorAxis? _axisZSim;
    private bool _isSyncingSimPosition;
    private bool _autoInitAttempted;
    private DispatcherTimer? _homeHoldTimer;
    private readonly DispatcherTimer _updateTimer;
    private readonly SettingsService _settingsService;
    private readonly MachineSettings _machineSettings;
    private readonly ModelService<PickAndPlaceModel> _modelService;

    [ObservableProperty]
    private MachineState _machineState = MachineState.Uninitialized;

    [ObservableProperty]
    private int _machineNumber = 1;

    [ObservableProperty]
    private string _projectName = "Pick & Place Machine (EtherCAT)";

    [ObservableProperty]
    private HeaderStatusLevel _headerStatusLevel = HeaderStatusLevel.Ok;

    [ObservableProperty]
    private MachineRunMode _runMode = MachineRunMode.Auto;

    private MachineRunMode _lastAutoMode = MachineRunMode.Auto;

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

    [ObservableProperty]
    private double _axisXPosition;

    [ObservableProperty]
    private double _axisYPosition;

    [ObservableProperty]
    private double _axisZPosition;

    [ObservableProperty]
    private string _autoStep = string.Empty;

    [ObservableProperty]
    private double _simulatedXPosition;

    [ObservableProperty]
    private double _simulatedYPosition;

    [ObservableProperty]
    private double _simulatedZPosition;

    [ObservableProperty]
    private double _simulatedSpeed = 50;

    // IO Inputs
    [ObservableProperty]
    private bool _partPresent;

    [ObservableProperty]
    private bool _testOk;

    [ObservableProperty]
    private bool _testNg;

    [ObservableProperty]
    private bool _cylinderExtended;

    [ObservableProperty]
    private bool _cylinderRetracted;

    [ObservableProperty]
    private bool _vacuumOk;

    // IO Outputs
    [ObservableProperty]
    private bool _cylinderExtendOutput;

    [ObservableProperty]
    private bool _vacuumOnOutput;

    [ObservableProperty]
    private bool _isAxisMoving;

    [ObservableProperty]
    private string _statusMessage = "Initializing...";

    // Model
    [ObservableProperty]
    private string _modelName = "No Model";

    /// <summary>
    /// Current loaded model
    /// </summary>
    public PickAndPlaceModel? CurrentModel => _modelService.CurrentModel;

    /// <summary>
    /// Model service for save/load models
    /// </summary>
    public ModelService<PickAndPlaceModel> ModelService => _modelService;

    public MainViewModel(
        PickAndPlaceMachine machine,
        LeadshineEthercatSimulator ioSimulator,
        IEnumerable<IAxis> axes)
    {
        _machine = machine;
        _ioSimulator = ioSimulator;
        var axisList = axes.ToList();
        _axisXSim = axisList.FirstOrDefault(a => a.Name == "AxisX") as SimulatorAxis;
        _axisYSim = axisList.FirstOrDefault(a => a.Name == "AxisY") as SimulatorAxis;
        _axisZSim = axisList.FirstOrDefault(a => a.Name == "AxisZ") as SimulatorAxis;

        // Initialize settings service and load saved settings
        _settingsService = new SettingsService(Log.Logger);
        _machineSettings = _settingsService.Load();
        _machineNumber = _machineSettings.MachineNumber;
        _projectName = _machineSettings.ProjectName;

        // Initialize model service
        _modelService = new ModelService<PickAndPlaceModel>("PickAndPlace", Log.Logger);
        _modelService.ModelChanged += OnModelChanged;

        // Try to load default model
        _ = LoadDefaultModelAsync();

        // Subscribe to machine state changes
        _machine.StateChanged += OnMachineStateChanged;

        // Setup update timer (100ms)
        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _updateTimer.Tick += UpdateTimerTick;
        _updateTimer.Start();

        _ = AutoInitializeAsync();
    }

    private void OnModelChanged(object? sender, ModelChangedEventArgs<PickAndPlaceModel> e)
    {
        ModelName = e.NewModel?.Name ?? "No Model";
        OnPropertyChanged(nameof(CurrentModel));
        _machineSettings.LastModelName = e.NewModel?.Name;
        _settingsService.Save(_machineSettings);
        Log.Information("Model changed: {OldModel} -> {NewModel}",
            e.OldModel?.Name ?? "None",
            e.NewModel?.Name ?? "None");
    }

    private async Task AutoInitializeAsync()
    {
        if (_autoInitAttempted) return;
        _autoInitAttempted = true;

        try
        {
            var result = await _machine.InitializeAsync();
            if (!result.IsSuccess)
            {
                StatusMessage = $"Auto init failed: {result.Message}";
                Log.Error("Auto init failed: {Message}", result.Message);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Auto init error: {ex.Message}";
            Log.Error(ex, "Auto init error");
        }
    }

    private async Task LoadDefaultModelAsync()
    {
        // First try to load last used model
        if (!string.IsNullOrWhiteSpace(_machineSettings.LastModelName) &&
            _modelService.ModelExists(_machineSettings.LastModelName))
        {
            await _modelService.LoadModelAsync(_machineSettings.LastModelName);
            return;
        }

        // Try to load most recently modified model
        var model = await _modelService.LoadMostRecentModelAsync();
        if (model == null)
        {
            // No models exist, create a new one
            var modelNames = _modelService.GetModelNames();
            if (modelNames.Count == 0)
            {
                var defaultModel = _modelService.CreateNewModel("Default");
                await _modelService.SaveModelAsync(defaultModel);
                _machineSettings.LastModelName = defaultModel.Name;
                _settingsService.Save(_machineSettings);
                Log.Information("Created default model");
            }
        }
    }

    partial void OnMachineNumberChanged(int value)
    {
        _settingsService.SaveMachineNumber(value);
    }

    partial void OnProjectNameChanged(string value)
    {
        _settingsService.SaveProjectName(value);
    }

    partial void OnRunModeChanged(MachineRunMode value)
    {
        _machine.SetRunMode(value);
    }

    [RelayCommand]
    private void SetAutoMode()
    {
        _lastAutoMode = MachineRunMode.Auto;
        RunMode = MachineRunMode.Auto;
    }

    [RelayCommand]
    private void SetDryRunMode()
    {
        _lastAutoMode = MachineRunMode.DryRun;
        RunMode = MachineRunMode.DryRun;
    }

    public void SetManualMode()
    {
        RunMode = MachineRunMode.Manual;
    }

    public void RestoreAutoMode()
    {
        RunMode = _lastAutoMode;
    }

    private void OnMachineStateChanged(object? sender, MachineState newState)
    {
        MachineState = newState;
        UpdateTowerLights();
        UpdateStatusMessage();
    }

    [RelayCommand]
    private void HomeHoldStart()
    {
        if (_homeHoldTimer != null) return;

        StatusMessage = "Hold HOME for 3s to start homing";

        _homeHoldTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _homeHoldTimer.Tick += async (_, _) =>
        {
            _homeHoldTimer?.Stop();
            _homeHoldTimer = null;
            await HomeAllAsync();
        };
        _homeHoldTimer.Start();
    }

    [RelayCommand]
    private void HomeHoldEnd()
    {
        if (_homeHoldTimer == null) return;
        _homeHoldTimer.Stop();
        _homeHoldTimer = null;
        StatusMessage = "Home cancelled";
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
            AxisXPosition = _machine.AxisXPosition;
            AxisYPosition = _machine.AxisYPosition;
            AxisZPosition = _machine.AxisZPosition;
            IsAxisMoving = _machine.IsAxisMoving;
            AutoStep = _machine.AutoStep;

            _isSyncingSimPosition = true;
            SimulatedXPosition = AxisXPosition;
            SimulatedYPosition = AxisYPosition;
            SimulatedZPosition = AxisZPosition;
            _isSyncingSimPosition = false;

            // Update header status based on machine state
            UpdateHeaderStatus();

            // Update tower lights
            UpdateTowerLights();

            UpdateIOStates();
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
        var alarmMessage = _machine.ActiveAlarms.FirstOrDefault()?.Message ?? "";
        StatusMessage = MachineState switch
        {
            MachineState.Uninitialized => "System ready - Please initialize",
            MachineState.Initializing => "Initializing hardware...",
            MachineState.Idle => "Ready to start",
            MachineState.Running => $"Running - Cycle {CycleCount + 1}",
            MachineState.Paused => "Paused",
            MachineState.Stopping => "Stopping...",
            MachineState.Stopped => "Stopped",
            MachineState.Error => $"Error: {alarmMessage}",
            MachineState.EmergencyStop => "EMERGENCY STOP!",
            _ => "Unknown state"
        };
    }

    private void UpdateHeaderStatus()
    {
        // Error states
        if (MachineState == MachineState.EmergencyStop || MachineState == MachineState.Error)
        {
            HeaderStatusLevel = HeaderStatusLevel.Error;
            return;
        }

        // Warning states - has alarms but not critical
        if (_machine.HasActiveAlarms)
        {
            HeaderStatusLevel = HeaderStatusLevel.Warning;
            return;
        }

        // OK state
        HeaderStatusLevel = HeaderStatusLevel.Ok;
    }

    partial void OnSimulatedXPositionChanged(double value)
    {
        if (_isSyncingSimPosition) return;
        _axisXSim?.SetSimulatedPosition(value);
    }

    partial void OnSimulatedYPositionChanged(double value)
    {
        if (_isSyncingSimPosition) return;
        _axisYSim?.SetSimulatedPosition(value);
    }

    partial void OnSimulatedZPositionChanged(double value)
    {
        if (_isSyncingSimPosition) return;
        _axisZSim?.SetSimulatedPosition(value);
    }

    partial void OnSimulatedSpeedChanged(double value)
    {
        if (_isSyncingSimPosition) return;
        _axisXSim?.SetSimulatedVelocity(value);
        _axisYSim?.SetSimulatedVelocity(value);
        _axisZSim?.SetSimulatedVelocity(value);
    }

    private void UpdateIOStates()
    {
        var map = _machine.IOMap;

        PartPresent = _ioSimulator.GetInput(map.MachineInputs.PartPresent);
        TestOk = _ioSimulator.GetInput(map.MachineInputs.TestOk);
        TestNg = _ioSimulator.GetInput(map.MachineInputs.TestNg);
        CylinderExtended = _ioSimulator.GetInput(map.MachineInputs.CylinderExtended);
        CylinderRetracted = _ioSimulator.GetInput(map.MachineInputs.CylinderRetracted);
        VacuumOk = _ioSimulator.GetInput(map.MachineInputs.VacuumOk);

        CylinderExtendOutput = _ioSimulator.GetOutput(map.MachineOutputs.CylinderExtend);
        VacuumOnOutput = _ioSimulator.GetOutput(map.MachineOutputs.VacuumOn);
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
                HeaderStatusLevel = HeaderStatusLevel.Ok;
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
            StatusMessage = "Homing axes...";
            var result = await _machine.HomeAllAxesAsync();
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

    [RelayCommand]
    private void TogglePartPresent()
    {
        var address = _machine.IOMap.MachineInputs.PartPresent;
        _ioSimulator.SetInput(address, !PartPresent);
        UpdateIOStates();
    }

    [RelayCommand]
    private void ToggleTestOk()
    {
        var address = _machine.IOMap.MachineInputs.TestOk;
        _ioSimulator.SetInput(address, !TestOk);
        UpdateIOStates();
    }

    [RelayCommand]
    private void ToggleTestNg()
    {
        var address = _machine.IOMap.MachineInputs.TestNg;
        _ioSimulator.SetInput(address, !TestNg);
        UpdateIOStates();
    }

    public void NavigateToView(object view)
    {
        CurrentView = view;
    }
}
