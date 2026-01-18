using System.Windows;
using PickAndPlace.ViewModels;
using PickAndPlace.Views;
using NAutoSuite.UI.Controls.Dialogs;

namespace PickAndPlace;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly AutoView _autoView;
    private readonly ManualView _manualView;
    private readonly SettingView _settingView;
    private readonly DataView _dataView;
    private readonly LogView _logView;
    private string _lastTab = "Auto";

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        // Create Model Management ViewModel for DATA tab
        var modelManagementViewModel = new ModelManagementViewModel(_viewModel.ModelService);

        // Create all views
        _autoView = new AutoView { DataContext = _viewModel };
        _manualView = new ManualView { DataContext = _viewModel };
        _settingView = new SettingView { DataContext = _viewModel };
        _dataView = new DataView(modelManagementViewModel);
        _logView = new LogView();

        // Show Auto view by default
        _viewModel.NavigateToView(_autoView);

        // Hardware initialization is triggered on startup via view model
    }

    private void FooterControl_TabChanged(object? sender, string tabName)
    {
        if (tabName == "Manual")
        {
            _viewModel.SetManualMode();
        }
        else if (_lastTab == "Manual")
        {
            _viewModel.RestoreAutoMode();
        }

        _lastTab = tabName;

        object view = tabName switch
        {
            "Auto" => (object)_autoView,
            "Manual" => (object)_manualView,
            "Setting" => (object)_settingView,
            "Data" => (object)_dataView,
            "Log" => (object)_logView,
            _ => (object)_autoView
        };

        _viewModel.NavigateToView(view);
    }

    private void HeaderControl_CloseClicked(object sender, RoutedEventArgs e)
    {
        var result = ModernMessageBox.Show(
            "Bạn có chắc chắn muốn thoát chương trình không?",
            "Xác nhận thoát ứng dụng",
            ModernMessageBox.MessageBoxType.Question,
            ModernMessageBox.MessageBoxButtons.YesNo,
            ModernMessageBox.ButtonStyle.Danger);  // Yes button is RED (dangerous action)

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }
}
