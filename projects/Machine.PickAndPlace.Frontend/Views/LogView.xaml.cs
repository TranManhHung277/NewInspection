using System.Windows.Controls;
using NAutoSuite.UI.Controls;

namespace Machine.PickAndPlace.Views;

public partial class LogView : UserControl
{
    public LogPanel? LogPanel => logPanel;

    public LogView()
    {
        InitializeComponent();
    }
}
