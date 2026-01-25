using EVIInspection.ViewModels;
using EVIInspection.Views;
using NAutoSuite.Core.Model;
using NAutoSuite.UI.Controls.Dialogs;
using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EVIInspection
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly AutoView _autoView;
        private readonly ManualView _manualView;
        private readonly IOView _ioView;
        private readonly SettingView _settingView;
        private readonly LogView _logView;
        private readonly TeachView _teachView;
        private string _lastTab = "Auto";
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;

            // Create all views
            _autoView = new AutoView { DataContext = _viewModel };
            _manualView = new ManualView { DataContext = _viewModel };
            _ioView = new IOView { DataContext = _viewModel };
            _teachView = new TeachView { DataContext = _teachView };
            _settingView = new SettingView { DataContext = _viewModel };
            _logView = new LogView();

            // Show Auto view by default
            _viewModel.NavigateToView(_autoView);

        }

        private void HeaderControl_CloseClicked(object sender, RoutedEventArgs e)
        {
            ModernMessageBox.TitleYesButton = "Có";
            ModernMessageBox.TitleNoButton = "Không";
            var result = ModernMessageBox.Show(
                "Bạn có chắc chắn muốn thoát chương trình không?",
                "Xác nhận đóng chương trình",
                ModernMessageBox.MessageBoxType.Question,
                ModernMessageBox.MessageBoxButtons.YesNo,
                ModernMessageBox.ButtonStyle.Danger);

            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }

        private void HeaderControl_MinimizeClicked(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void HeaderControl_HeaderClicked(object sender, RoutedEventArgs e)
        {
            var hasError = _viewModel.MachineState == NAutoSuite.Core.Machine.MachineState.Error
                || _viewModel.MachineState == NAutoSuite.Core.Machine.MachineState.EmergencyStop;
            var targetTab = hasError
                ? "Log"
                : "Auto";
            FooterControl.SelectTab(targetTab);
        }

        private void FooterControl_TabChanged(object? sender, string tabName)
        {
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
    }
}
