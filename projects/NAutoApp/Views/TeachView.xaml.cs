using System.Windows.Controls;
using NAutoApp.ViewModels;

namespace NAutoApp.Views;

public partial class TeachView : UserControl
{
    public TeachView()
    {
        InitializeComponent();
    }

    public TeachView(ModelManagementViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }
}
