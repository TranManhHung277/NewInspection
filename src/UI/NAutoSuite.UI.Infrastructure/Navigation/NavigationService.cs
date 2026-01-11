using Microsoft.Extensions.DependencyInjection;

namespace NAutoSuite.UI.Infrastructure.Navigation;

/// <summary>
/// Default navigation service implementation
/// </summary>
public class NavigationService : INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<object> _navigationStack = new();
    private object? _currentView;

    public object? CurrentView
    {
        get => _currentView;
        private set
        {
            _currentView = value;
            Navigated?.Invoke(this, _currentView);
        }
    }

    public bool CanGoBack => _navigationStack.Count > 0;

    public event EventHandler<object?>? Navigated;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void NavigateTo<TViewModel>() where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        Navigate(viewModel);
    }

    public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();

        // If ViewModel has a parameter property, set it
        var parameterProperty = typeof(TViewModel).GetProperty("Parameter");
        parameterProperty?.SetValue(viewModel, parameter);

        Navigate(viewModel);
    }

    public bool GoBack()
    {
        if (_navigationStack.Count == 0)
            return false;

        var previousView = _navigationStack.Pop();
        CurrentView = previousView;
        return true;
    }

    public void ClearHistory()
    {
        _navigationStack.Clear();
    }

    private void Navigate(object viewModel)
    {
        if (CurrentView != null)
        {
            _navigationStack.Push(CurrentView);
        }

        CurrentView = viewModel;
    }
}
