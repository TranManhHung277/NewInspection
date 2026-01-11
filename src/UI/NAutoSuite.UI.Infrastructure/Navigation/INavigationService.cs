namespace NAutoSuite.UI.Infrastructure.Navigation;

/// <summary>
/// Navigation service for view switching
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Current view model
    /// </summary>
    object? CurrentView { get; }

    /// <summary>
    /// Navigate to a view
    /// </summary>
    void NavigateTo<TViewModel>() where TViewModel : class;

    /// <summary>
    /// Navigate to a view with parameter
    /// </summary>
    void NavigateTo<TViewModel>(object parameter) where TViewModel : class;

    /// <summary>
    /// Go back to previous view
    /// </summary>
    bool GoBack();

    /// <summary>
    /// Check if can go back
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Clear navigation history
    /// </summary>
    void ClearHistory();

    /// <summary>
    /// Navigation occurred event
    /// </summary>
    event EventHandler<object?>? Navigated;
}
