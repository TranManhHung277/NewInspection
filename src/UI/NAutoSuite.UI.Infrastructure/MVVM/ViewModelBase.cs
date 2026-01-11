using CommunityToolkit.Mvvm.ComponentModel;

namespace NAutoSuite.UI.Infrastructure.MVVM;

/// <summary>
/// Base class for all ViewModels using CommunityToolkit.Mvvm
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
    private bool _isBusy;
    private string _title = string.Empty;

    /// <summary>
    /// Indicates if the ViewModel is performing an operation
    /// </summary>
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    /// <summary>
    /// Title for the View
    /// </summary>
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    /// <summary>
    /// Called when the view is loaded
    /// </summary>
    public virtual Task OnLoadedAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the view is unloaded
    /// </summary>
    public virtual Task OnUnloadedAsync()
    {
        return Task.CompletedTask;
    }
}
