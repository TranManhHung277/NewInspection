using System.Windows.Controls;
using NAutoApp.ViewModels;

namespace NAutoApp.Views;

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

