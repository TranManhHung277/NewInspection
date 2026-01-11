using CommunityToolkit.Mvvm.Input;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Machine;
using NAutoSuite.UI.Infrastructure.MVVM;
using AsyncRelayCommand = CommunityToolkit.Mvvm.Input.AsyncRelayCommand;

namespace NAutoSuite.App.Template.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly IMachine _machine;
    private MachineState _machineState;

    public MachineState MachineState
    {
        get => _machineState;
        set => SetProperty(ref _machineState, value);
    }

    public IAsyncRelayCommand InitializeCommand { get; }
    public IAsyncRelayCommand StartCommand { get; }
    public IAsyncRelayCommand StopCommand { get; }
    public IAsyncRelayCommand ResetCommand { get; }

    public MainViewModel(IMachine machine)
    {
        _machine = machine;
        Title = "NAutoSuite - Machine Control";

        _machine.StateChanged += OnMachineStateChanged;
        MachineState = _machine.State;

        InitializeCommand = new AsyncRelayCommand(InitializeAsync);
        StartCommand = new AsyncRelayCommand(StartAsync, CanStart);
        StopCommand = new AsyncRelayCommand(StopAsync, CanStop);
        ResetCommand = new AsyncRelayCommand(ResetAsync);
    }

    private void OnMachineStateChanged(object? sender, MachineState state)
    {
        MachineState = state;
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    private async Task InitializeAsync()
    {
        IsBusy = true;
        await _machine.InitializeAsync();
        IsBusy = false;
    }

    private bool CanStart() => MachineState == MachineState.Idle;

    private async Task StartAsync()
    {
        IsBusy = true;
        await _machine.StartAsync();
        IsBusy = false;
    }

    private bool CanStop() => MachineState == MachineState.Running;

    private async Task StopAsync()
    {
        IsBusy = true;
        await _machine.StopAsync();
        IsBusy = false;
    }

    private async Task ResetAsync()
    {
        IsBusy = true;
        await _machine.ResetAsync();
        IsBusy = false;
    }
}
