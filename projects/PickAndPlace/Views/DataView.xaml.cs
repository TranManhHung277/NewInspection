using System.Windows.Controls;
using PickAndPlace.ViewModels;

namespace PickAndPlace.Views;

public partial class DataView : UserControl
{
    public DataView()
    {
        InitializeComponent();
    }

    public DataView(ModelManagementViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
