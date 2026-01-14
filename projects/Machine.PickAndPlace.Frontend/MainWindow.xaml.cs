using System.Windows;
using Machine.PickAndPlace.ViewModels;
using Machine.PickAndPlace.Views;
using NAutoSuite.UI.Controls.Dialogs;

namespace Machine.PickAndPlace;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly AutoView _autoView;
    private readonly ManualView _manualView;
    private readonly SettingView _settingView;
    private readonly DataView _dataView;
    private readonly LogView _logView;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        // Create all views
        _autoView = new AutoView { DataContext = _viewModel };
        _manualView = new ManualView { DataContext = _viewModel };
        _settingView = new SettingView { DataContext = _viewModel };
        _dataView = new DataView { DataContext = _viewModel };
        _logView = new LogView();

        // Show Auto view by default
        _viewModel.NavigateToView(_autoView);

        // Hardware initialization is done manually via Initialize button
        // Do NOT auto-initialize on window load
    }

    private void FooterControl_TabChanged(object? sender, string tabName)
    {
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
            "Are you sure you want to exit?",
            "Confirm Exit",
            ModernMessageBox.MessageBoxType.Question,
            ModernMessageBox.MessageBoxButtons.YesNo);

        if (result == MessageBoxResult.Yes)
        {
            Application.Current.Shutdown();
        }
    }
}
