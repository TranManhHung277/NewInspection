using System.Windows.Controls;
using System.Windows.Input;

namespace PickAndPlace.Views;

public partial class AutoView : UserControl
{
    public AutoView()
    {
        InitializeComponent();
    }

    private void HomeHoldStart(object sender, MouseButtonEventArgs e)
    {
        ExecuteCommand("HomeHoldStartCommand");
    }

    private void HomeHoldEnd(object sender, MouseButtonEventArgs e)
    {
        ExecuteCommand("HomeHoldEndCommand");
    }

    private void ExecuteCommand(string commandName)
    {
        if (DataContext == null) return;
        var property = DataContext.GetType().GetProperty(commandName);
        if (property?.GetValue(DataContext) is ICommand command && command.CanExecute(null))
        {
            command.Execute(null);
        }
    }
}
