using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Machine;
using NAutoSuite.UI.Controls;
using NAutoSuite.UI.Controls.Dialogs;
using System.Windows;
using System.Windows.Media.Imaging;
using EVIInspection.Camera;

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
        

        public IRelayCommand StartCommand { get; }
        public IRelayCommand StopCommand { get; }
        public IRelayCommand ResetCommand { get; }
        public IRelayCommand HomeHoldStartCommand { get; }
        public IRelayCommand HomeHoldEndCommand { get; }
        [ObservableProperty]
        private ImageViewModel _imageVM;



        public MainViewModel(HardwareMinimalConfig config, EVIInspection.Camera.ImageViewModel imageVM)
        {
            _imageVM = imageVM;
            StartCommand = new RelayCommand(OnStart);
            StopCommand = new RelayCommand(OnStop);
            ResetCommand = new RelayCommand(OnReset);
            HomeHoldStartCommand = new RelayCommand(OnHomeHoldStart);
            HomeHoldEndCommand = new RelayCommand(OnHomeHoldEnd);

            
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

        private void OnStart()
        {
            ModernMessageBox.Show("bạn đã nhấn nút start!");
        }

        private void OnStop()
        {
        }

        private void OnReset()
        {
        }

        private void OnHomeHoldStart()
        {
        }

        private void OnHomeHoldEnd()
        {
            ModernMessageBox.Show(
                "Ban da nhan giu du thoi gian.",
                "Hold Completed",
                ModernMessageBox.MessageBoxType.Success,
                ModernMessageBox.MessageBoxButtons.OK,
                ModernMessageBox.ButtonStyle.Primary);
        }
        
    }
}
