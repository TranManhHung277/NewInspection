using CommunityToolkit.Mvvm.ComponentModel;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Machine;
using NAutoSuite.UI.Controls;

namespace EVIInspection.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private MachineState _machineState = MachineState.Uninitialized;
        [ObservableProperty]
        private bool _hasActiveAlarms;
        [ObservableProperty]
        private object? _currentView;
        [ObservableProperty]
        private int _machineNumber = 22;
        [ObservableProperty]
        private string _projectName = "EVI Inspection";
        [ObservableProperty]
        private string _modelName = "Default";
        [ObservableProperty]
        private MachineRunMode _runMode = MachineRunMode.Auto;
        [ObservableProperty]
        private HeaderStatusLevel _headerStatusLevel = HeaderStatusLevel.Ok;

        public MainViewModel(HardwareMinimalConfig config)
        {
            ApplyAppConfig(config?.App);
            UpdateHeaderStatusLevel();
        }

        public void NavigateToView(object view)
        {
            CurrentView = view;
        }

        partial void OnMachineStateChanged(MachineState value)
        {
            UpdateHeaderStatusLevel();
        }

        partial void OnHasActiveAlarmsChanged(bool value)
        {
            UpdateHeaderStatusLevel();
        }

        private void UpdateHeaderStatusLevel()
        {
            if (MachineState == MachineState.Error || MachineState == MachineState.EmergencyStop)
            {
                HeaderStatusLevel = HeaderStatusLevel.Error;
                return;
            }

            HeaderStatusLevel = HasActiveAlarms
                ? HeaderStatusLevel.Warning
                : HeaderStatusLevel.Ok;
        }

        private void ApplyAppConfig(AppConfig? app)
        {
            if (app == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(app.Name))
            {
                ProjectName = app.Name;
            }

            if (app.Machine > 0)
            {
                MachineNumber = app.Machine;
            }
        }
    }
}
