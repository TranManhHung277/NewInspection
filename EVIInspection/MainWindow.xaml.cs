using EVIInspection.ViewModels;
using EVIInspection.Views;
using NAutoSuite.Core.Model;
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
        private readonly MainViewMdel _viewModel;
        private readonly AutoView _autoView;
        private readonly ManualView _manualView;
        private readonly IOView _ioView;
        private readonly SettingView _settingView;
        private readonly LogView _logView;
        private readonly TeachView _teachView;
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}