using System.Windows;
using NAutoApp.ViewModels;
using NAutoApp.Views;
using NAutoSuite.UI.Controls.Dialogs;

namespace NAutoApp;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly AutoView _autoView;
    private readonly ManualView _manualView;
    private readonly IOView _ioView;
    private readonly TeachView _teachView;
    private readonly SettingView _settingView;
    private readonly LogView _logView;
    private string _lastTab = "Auto";
    private bool _suppressTabChange;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        // Create Model Management ViewModel for TEACH tab
        var modelManagementViewModel = new ModelManagementViewModel(_viewModel.ModelService);

        // Create all views
        _autoView = new AutoView { DataContext = _viewModel };
        _manualView = new ManualView { DataContext = _viewModel };
        _ioView = new IOView { DataContext = _viewModel };
        _teachView = new TeachView(modelManagementViewModel);
        _settingView = new SettingView { DataContext = _viewModel };
        _logView = new LogView();

        // Show Auto view by default
        _viewModel.NavigateToView(_autoView);

        // Hardware initialization is triggered on startup via view model
    }

    private void FooterControl_TabChanged(object? sender, string tabName)
    {
        if (_suppressTabChange)
        {
            return;
        }

        if (_viewModel.MachineState == NAutoSuite.Core.Machine.MachineState.Running &&
            _viewModel.RunMode == NAutoSuite.Core.Machine.MachineRunMode.Auto &&
            tabName is "Manual" or "IO" or "Teach" or "Setting")
        {
            _viewModel.StatusMessage = "Stop machine before switching tabs";
            _suppressTabChange = true;
            FooterControl.SelectTab("Auto");
            _suppressTabChange = false;
            return;
        }

        _lastTab = tabName;

        object view = tabName switch
        {
            "Auto" => (object)_autoView,
            "Manual" => (object)_manualView,
            "IO" => (object)_ioView,
            "Teach" => (object)_teachView,
            "Setting" => (object)_settingView,
            "Log" => (object)_logView,
            _ => (object)_autoView
        };

        _viewModel.NavigateToView(view);
    }

    private void HeaderControl_CloseClicked(object sender, RoutedEventArgs e)
    {
        ModernMessageBox.TitleYesButton = "Có";
        ModernMessageBox.TitleNoButton = "Không";
        var result = ModernMessageBox.Show(
            "B?n có ch?c ch?n mu?n thoát chương tr?nh không?",
            "Xác nh?n thoát ?ng d?ng",
            ModernMessageBox.MessageBoxType.Question,
            ModernMessageBox.MessageBoxButtons.YesNo,
            ModernMessageBox.ButtonStyle.Danger);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }

    private void HeaderControl_HeaderClicked(object sender, RoutedEventArgs e)
    {
        var hasError = _viewModel.MachineState == NAutoSuite.Core.Machine.MachineState.Error
            || _viewModel.MachineState == NAutoSuite.Core.Machine.MachineState.EmergencyStop
            || _viewModel.HasActiveAlarms
            || _viewModel.HeaderStatusLevel == NAutoSuite.UI.Controls.HeaderStatusLevel.Error;
        var targetTab = hasError
            ? "Log"
            : "Auto";
        FooterControl.SelectTab(targetTab);
    }
}



