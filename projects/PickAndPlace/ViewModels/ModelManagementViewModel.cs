using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAutoSuite.Core.Model;
using PickAndPlace.Model;
using Serilog;
using System.Collections.ObjectModel;

namespace PickAndPlace.ViewModels;

/// <summary>
/// ViewModel for Model Management (DATA tab)
/// </summary>
public partial class ModelManagementViewModel : ObservableObject
{
    private readonly ModelService<PickAndPlaceModel> _modelService;

    #region Observable Properties

    [ObservableProperty]
    private ObservableCollection<ModelListItem> _modelList = new();

    [ObservableProperty]
    private ModelListItem? _selectedModelItem;

    [ObservableProperty]
    private PickAndPlaceModel? _currentModel;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    // Editable model properties
    [ObservableProperty]
    private string _editModelName = string.Empty;

    [ObservableProperty]
    private string _editDescription = string.Empty;

    // Positions
    [ObservableProperty]
    private double _pickPositionX;

    [ObservableProperty]
    private double _pickPositionY;

    [ObservableProperty]
    private double _pickPositionZ;

    [ObservableProperty]
    private double _placePositionX;

    [ObservableProperty]
    private double _placePositionY;

    [ObservableProperty]
    private double _placePositionZ;

    [ObservableProperty]
    private double _homePositionX;

    [ObservableProperty]
    private double _homePositionY;

    [ObservableProperty]
    private double _homePositionZ;

    // Speeds
    [ObservableProperty]
    private double _moveSpeed;

    [ObservableProperty]
    private double _approachSpeed;

    [ObservableProperty]
    private double _pickSpeed;

    [ObservableProperty]
    private double _placeSpeed;

    // Delays
    [ObservableProperty]
    private int _vacuumOnDelay;

    [ObservableProperty]
    private int _vacuumOffDelay;

    [ObservableProperty]
    private int _pickDelay;

    [ObservableProperty]
    private int _placeDelay;

    // Other settings
    [ObservableProperty]
    private int _vacuumThreshold;

    [ObservableProperty]
    private int _pickRetryCount;

    [ObservableProperty]
    private bool _enablePartCheck;

    [ObservableProperty]
    private double _targetCycleTime;

    #endregion

    public ModelManagementViewModel(ModelService<PickAndPlaceModel> modelService)
    {
        _modelService = modelService;
        _modelService.ModelChanged += OnModelServiceModelChanged;

        // Load initial data
        _ = RefreshModelListAsync();
        LoadCurrentModelToEdit();
    }

    private void OnModelServiceModelChanged(object? sender, ModelChangedEventArgs<PickAndPlaceModel> e)
    {
        CurrentModel = e.NewModel;
        LoadCurrentModelToEdit();
    }

    partial void OnSelectedModelItemChanged(ModelListItem? value)
    {
        if (value != null && !IsEditing)
        {
            _ = LoadModelAsync(value.Name);
        }
    }

    #region Commands

    [RelayCommand]
    private async Task RefreshModelListAsync()
    {
        try
        {
            var models = await _modelService.GetAllModelsAsync(forceRefresh: true);
            ModelList.Clear();

            foreach (var model in models.OrderBy(m => m.Name))
            {
                ModelList.Add(new ModelListItem
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsDefault = model.IsDefault,
                    ModifiedAt = model.ModifiedAt ?? model.CreatedAt,
                    IsCurrentModel = _modelService.CurrentModel?.Id == model.Id
                });
            }

            // Update selected item
            if (_modelService.CurrentModel != null)
            {
                SelectedModelItem = ModelList.FirstOrDefault(m => m.Name == _modelService.CurrentModel.Name);
            }

            StatusMessage = $"Loaded {ModelList.Count} models";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to refresh model list");
            StatusMessage = "Failed to load models";
        }
    }

    [RelayCommand]
    private async Task LoadModelAsync(string modelName)
    {
        try
        {
            StatusMessage = $"Loading model: {modelName}...";
            var model = await _modelService.LoadModelAsync(modelName);

            if (model != null)
            {
                CurrentModel = model;
                LoadCurrentModelToEdit();
                await RefreshModelListAsync();
                StatusMessage = $"Model loaded: {modelName}";
            }
            else
            {
                StatusMessage = $"Failed to load model: {modelName}";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load model: {Name}", modelName);
            StatusMessage = $"Error loading model: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CreateNewModelAsync()
    {
        try
        {
            // Generate unique name
            var baseName = "New Model";
            var name = baseName;
            var counter = 1;

            while (_modelService.ModelExists(name))
            {
                name = $"{baseName} {counter++}";
            }

            var model = _modelService.CreateNewModel(name);
            await _modelService.SaveModelAsync(model);

            await RefreshModelListAsync();
            LoadCurrentModelToEdit();
            IsEditing = true;

            StatusMessage = $"Created new model: {name}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create new model");
            StatusMessage = $"Error creating model: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SaveModelAsync()
    {
        if (CurrentModel == null) return;

        try
        {
            // Update model from edit fields
            ApplyEditToModel();

            // Validate
            var errors = CurrentModel.Validate();
            if (errors.Count > 0)
            {
                StatusMessage = $"Validation error: {errors[0]}";
                return;
            }

            // Save
            var success = await _modelService.SaveModelAsync(CurrentModel);

            if (success)
            {
                IsEditing = false;
                await RefreshModelListAsync();
                StatusMessage = $"Model saved: {CurrentModel.Name}";
            }
            else
            {
                StatusMessage = "Failed to save model";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save model");
            StatusMessage = $"Error saving model: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task DeleteModelAsync()
    {
        if (CurrentModel == null) return;

        try
        {
            var modelName = CurrentModel.Name;
            var success = _modelService.DeleteModel(modelName);

            if (success)
            {
                await RefreshModelListAsync();

                // Load another model if available
                if (ModelList.Count > 0)
                {
                    await LoadModelAsync(ModelList[0].Name);
                }
                else
                {
                    CurrentModel = null;
                    ClearEditFields();
                }

                StatusMessage = $"Model deleted: {modelName}";
            }
            else
            {
                StatusMessage = $"Failed to delete model: {modelName}";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete model");
            StatusMessage = $"Error deleting model: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CloneModelAsync()
    {
        if (CurrentModel == null) return;

        try
        {
            var baseName = $"{CurrentModel.Name} (Copy)";
            var name = baseName;
            var counter = 1;

            while (_modelService.ModelExists(name))
            {
                name = $"{CurrentModel.Name} (Copy {counter++})";
            }

            var clone = await _modelService.CloneModelAsync(CurrentModel.Name, name);

            if (clone != null)
            {
                await RefreshModelListAsync();
                await LoadModelAsync(name);
                IsEditing = true;
                StatusMessage = $"Model cloned: {name}";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to clone model");
            StatusMessage = $"Error cloning model: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SetAsDefaultAsync()
    {
        if (CurrentModel == null) return;

        try
        {
            await _modelService.SetDefaultModelAsync(CurrentModel.Name);
            await RefreshModelListAsync();
            StatusMessage = $"Set as default: {CurrentModel.Name}";
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to set default model");
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void StartEditing()
    {
        IsEditing = true;
        StatusMessage = "Editing mode - make changes and save";
    }

    [RelayCommand]
    private void CancelEditing()
    {
        IsEditing = false;
        LoadCurrentModelToEdit();
        StatusMessage = "Changes discarded";
    }

    #endregion

    #region Helper Methods

    private void LoadCurrentModelToEdit()
    {
        CurrentModel = _modelService.CurrentModel;

        if (CurrentModel == null)
        {
            ClearEditFields();
            return;
        }

        // Basic info
        EditModelName = CurrentModel.Name;
        EditDescription = CurrentModel.Description;

        // Positions
        var pickPos = CurrentModel.PickPosition;
        PickPositionX = pickPos.X ?? 0;
        PickPositionY = pickPos.Y ?? 0;
        PickPositionZ = pickPos.Z ?? 0;

        var placePos = CurrentModel.PlacePosition;
        PlacePositionX = placePos.X ?? 0;
        PlacePositionY = placePos.Y ?? 0;
        PlacePositionZ = placePos.Z ?? 0;

        var homePos = CurrentModel.HomePosition;
        HomePositionX = homePos.X ?? 0;
        HomePositionY = homePos.Y ?? 0;
        HomePositionZ = homePos.Z ?? 0;

        // Speeds
        MoveSpeed = CurrentModel.MoveSpeed;
        ApproachSpeed = CurrentModel.ApproachSpeed;
        PickSpeed = CurrentModel.SpeedSettings.GetValueOrDefault(PickAndPlaceModel.SPEED_PICK, 20);
        PlaceSpeed = CurrentModel.SpeedSettings.GetValueOrDefault(PickAndPlaceModel.SPEED_PLACE, 20);

        // Delays
        VacuumOnDelay = CurrentModel.VacuumOnDelay;
        VacuumOffDelay = CurrentModel.VacuumOffDelay;
        PickDelay = CurrentModel.DelaySettings.GetValueOrDefault(PickAndPlaceModel.DELAY_PICK, 300);
        PlaceDelay = CurrentModel.DelaySettings.GetValueOrDefault(PickAndPlaceModel.DELAY_PLACE, 300);

        // Other settings
        VacuumThreshold = CurrentModel.VacuumThreshold;
        PickRetryCount = CurrentModel.PickRetryCount;
        EnablePartCheck = CurrentModel.EnablePartCheck;
        TargetCycleTime = CurrentModel.TargetCycleTime;
    }

    private void ApplyEditToModel()
    {
        if (CurrentModel == null) return;

        // Basic info
        CurrentModel.Name = EditModelName;
        CurrentModel.Description = EditDescription;

        // Positions
        CurrentModel.PickPosition = new TeachPosition(PickAndPlaceModel.POS_PICK, PickPositionX, PickPositionY, PickPositionZ);
        CurrentModel.PlacePosition = new TeachPosition(PickAndPlaceModel.POS_PLACE, PlacePositionX, PlacePositionY, PlacePositionZ);
        CurrentModel.HomePosition = new TeachPosition(PickAndPlaceModel.POS_HOME, HomePositionX, HomePositionY, HomePositionZ);

        // Speeds
        CurrentModel.MoveSpeed = MoveSpeed;
        CurrentModel.ApproachSpeed = ApproachSpeed;
        CurrentModel.SpeedSettings[PickAndPlaceModel.SPEED_PICK] = PickSpeed;
        CurrentModel.SpeedSettings[PickAndPlaceModel.SPEED_PLACE] = PlaceSpeed;

        // Delays
        CurrentModel.DelaySettings[PickAndPlaceModel.DELAY_VACUUM_ON] = VacuumOnDelay;
        CurrentModel.DelaySettings[PickAndPlaceModel.DELAY_VACUUM_OFF] = VacuumOffDelay;
        CurrentModel.DelaySettings[PickAndPlaceModel.DELAY_PICK] = PickDelay;
        CurrentModel.DelaySettings[PickAndPlaceModel.DELAY_PLACE] = PlaceDelay;

        // Other settings
        CurrentModel.VacuumThreshold = VacuumThreshold;
        CurrentModel.PickRetryCount = PickRetryCount;
        CurrentModel.EnablePartCheck = EnablePartCheck;
        CurrentModel.TargetCycleTime = TargetCycleTime;
    }

    private void ClearEditFields()
    {
        EditModelName = string.Empty;
        EditDescription = string.Empty;
        PickPositionX = PickPositionY = PickPositionZ = 0;
        PlacePositionX = PlacePositionY = PlacePositionZ = 0;
        HomePositionX = HomePositionY = HomePositionZ = 0;
        MoveSpeed = ApproachSpeed = PickSpeed = PlaceSpeed = 0;
        VacuumOnDelay = VacuumOffDelay = PickDelay = PlaceDelay = 0;
        VacuumThreshold = PickRetryCount = 0;
        EnablePartCheck = false;
        TargetCycleTime = 0;
    }

    #endregion
}

/// <summary>
/// Item for model list display
/// </summary>
public class ModelListItem
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public DateTime ModifiedAt { get; set; }
    public bool IsCurrentModel { get; set; }
}
