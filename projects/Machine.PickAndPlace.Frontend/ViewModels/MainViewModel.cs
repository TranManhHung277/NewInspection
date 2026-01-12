using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAutoSuite.Core.Machine;

namespace Machine.PickAndPlace.ViewModels;

public partial class MainViewModel : ObservableObject
{
    // Frontend is UI only - no machine logic
    // This is a placeholder ViewModel for UI display

    [ObservableProperty]
    private MachineState _machineState = MachineState.Idle;

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

    public MainViewModel()
    {
        // UI only - no machine instance
        // Machine logic runs in Backend project
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        // UI only - would connect to Backend via IPC/Network
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StartAsync()
    {
        // UI only - would connect to Backend via IPC/Network
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task StopAsync()
    {
        // UI only - would connect to Backend via IPC/Network
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ResetAsync()
    {
        // UI only - would connect to Backend via IPC/Network
        HasAlarm = false;
        AlarmMessage = string.Empty;
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task HomeAllAsync()
    {
        // UI only - would connect to Backend via IPC/Network
        await Task.CompletedTask;
    }

    public void NavigateToView(object view)
    {
        CurrentView = view;
    }
}
