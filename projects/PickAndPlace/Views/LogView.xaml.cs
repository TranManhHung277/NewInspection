using System.Windows.Controls;
using NAutoSuite.UI.Controls;

namespace PickAndPlace.Views;

public partial class LogView : UserControl
{
    public LogPanel? LogPanel => logPanel;

    public LogView()
    {
        InitializeComponent();
    }
}
