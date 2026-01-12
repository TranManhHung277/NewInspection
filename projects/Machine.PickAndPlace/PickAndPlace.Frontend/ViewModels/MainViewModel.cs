using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Machine;
using Machine.PickAndPlace.Core;

namespace Machine.PickAndPlace.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IMachine _machine;
    private readonly PickAndPlaceMachine _pickAndPlaceMachine;

    [ObservableProperty]
    private MachineState _machineState;

    [ObservableProperty]
    private int _machineNumber = 1;

    [ObservableProperty]
    private string _projectName = "Pick & Place Machine";

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

    public MainViewModel(IMachine machine)
    {
        _machine = machine;
        _pickAndPlaceMachine = (PickAndPlaceMachine)machine;
        _machineState = machine.State;

        _machine.StateChanged += OnMachineStateChanged;

        // Update cycle info periodically
        var timer = new System.Windows.Threading.DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(0.5);
        timer.Tick += (s, e) => UpdateMachineInfo();
        timer.Start();
    }

    private void OnMachineStateChanged(object? sender, MachineState state)
    {
        MachineState = state;
    }

    private void UpdateMachineInfo()
    {
        CycleCount = _pickAndPlaceMachine.Context.CycleCount;
        RunTime = _pickAndPlaceMachine.Context.TotalRunTime.ToString(@"hh\:mm\:ss");

        if (!string.IsNullOrEmpty(_pickAndPlaceMachine.Context.LastError))
        {
            AlarmMessage = _pickAndPlaceMachine.Context.LastError;
            HasAlarm = true;
        }
        else
        {
            AlarmMessage = string.Empty;
            HasAlarm = false;
        }
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await _machine.InitializeAsync();
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        await _machine.StartAsync();
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        await _machine.StopAsync();
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        await _machine.ResetAsync();
        HasAlarm = false;
        AlarmMessage = string.Empty;
    }

    [RelayCommand]
    private async Task HomeAllAsync()
    {
        await _pickAndPlaceMachine.HomeAllAxesAsync();
    }

    public void NavigateToView(object view)
    {
        CurrentView = view;
    }
}
