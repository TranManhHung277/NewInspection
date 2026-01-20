using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAutoSuite.Core.Model;
using PickAndPlace.Model;
using Serilog;
using System.Windows;
using NAutoSuite.UI.Controls.Dialogs;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Data;

namespace PickAndPlace.ViewModels;

/// <summary>
/// ViewModel for Model Management (DATA tab)
/// </summary>
public partial class ModelManagementViewModel : ObservableObject
{
    private readonly ModelService<PickAndPlaceModel> _modelService;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

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
        if (value == null)
        {
            return;
        }

        if (CurrentModel != null && string.Equals(value.Name, CurrentModel.Name, StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = $"Selected active model: {value.Name}";
            return;
        }

        StatusMessage = $"Selected model: {value.Name}. Press Load to activate.";
    }

    #region Commands

    [RelayCommand]
    private async Task RefreshModelListAsync()
    {
        await _refreshLock.WaitAsync();

        try
        {
            var models = await _modelService.GetAllModelsAsync(forceRefresh: true);
            var uniqueModels = models
                .GroupBy(m => m.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(m => m.ModifiedAt ?? m.CreatedAt).First())
                .ToList();

            ModelList.Clear();

            foreach (var model in uniqueModels
                .OrderByDescending(m => _modelService.CurrentModel?.Id == m.Id)
                .ThenByDescending(m => m.ModifiedAt ?? m.CreatedAt))
            {
                ModelList.Add(new ModelListItem
                {
                    Name = model.Name,
                    Description = model.Description,
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
        finally
        {
            _refreshLock.Release();
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
    private async Task LoadSelectedModelAsync()
    {
        if (SelectedModelItem == null)
        {
            StatusMessage = "Select a model to load";
            return;
        }

        if (CurrentModel != null &&
            string.Equals(SelectedModelItem.Name, CurrentModel.Name, StringComparison.OrdinalIgnoreCase))
        {
            StatusMessage = $"Model already active: {SelectedModelItem.Name}";
            return;
        }

        var result = ModernMessageBox.Show(
            $"Load model '{SelectedModelItem.Name}' and apply all parameters?",
            "Load Model",
            ModernMessageBox.MessageBoxType.Question,
            ModernMessageBox.MessageBoxButtons.YesNo,
            ModernMessageBox.ButtonStyle.Warning);

        if (result != MessageBoxResult.Yes)
        {
            StatusMessage = "Load canceled";
            return;
        }

        if (IsEditing)
        {
            IsEditing = false;
            StatusMessage = "Editing canceled - loading new model";
        }

        await LoadModelAsync(SelectedModelItem.Name);
    }

    [RelayCommand]
    private async Task CreateNewModelAsync()
    {
        try
        {
            var owner = Application.Current?.MainWindow;
            var suggestedName = CurrentModel == null ? "New Model" : $"{CurrentModel.Name} Backup";
            var name = TextKeyboardDialog.Show(owner, suggestedName)?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                StatusMessage = "Model creation canceled";
                return;
            }

            if (_modelService.ModelExists(name))
            {
                StatusMessage = $"Model name already exists: {name}";
                return;
            }

            PickAndPlaceModel model;
            if (CurrentModel != null)
            {
                model = (PickAndPlaceModel)CurrentModel.Clone(name);
                model.Description = string.Empty;
            }
            else
            {
                model = new PickAndPlaceModel { Name = name, CreatedAt = DateTime.Now };
            }

            await _modelService.SaveModelAsync(model);
            _modelService.SetCurrentModel(model);

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
            // Validate model name is not empty
            if (string.IsNullOrWhiteSpace(EditModelName))
            {
                ModernMessageBox.Show(
                    "Model name cannot be empty.",
                    "Validation Error",
                    ModernMessageBox.MessageBoxType.Warning,
                    ModernMessageBox.MessageBoxButtons.OK,
                    ModernMessageBox.ButtonStyle.Warning);
                StatusMessage = "Save failed: Model name is required";
                return;
            }

            var oldName = CurrentModel.Name;

            // Check for duplicate name (only if name changed)
            if (!string.Equals(EditModelName, oldName, StringComparison.OrdinalIgnoreCase) &&
                _modelService.ModelExists(EditModelName))
            {
                ModernMessageBox.Show(
                    $"A model with name '{EditModelName}' already exists.\n\nPlease choose a different name.",
                    "Duplicate Name",
                    ModernMessageBox.MessageBoxType.Warning,
                    ModernMessageBox.MessageBoxButtons.OK,
                    ModernMessageBox.ButtonStyle.Warning);
                StatusMessage = $"Save failed: Name '{EditModelName}' already exists";
                return;
            }

            // Update model from edit fields
            ApplyEditToModel();

            // Validate model data
            var errors = CurrentModel.Validate();
            if (errors.Count > 0)
            {
                var errorMessage = string.Join("\n", errors.Take(3));
                if (errors.Count > 3)
                {
                    errorMessage += $"\n... and {errors.Count - 3} more errors";
                }
                ModernMessageBox.Show(
                    $"Please fix the following errors:\n\n{errorMessage}",
                    "Validation Error",
                    ModernMessageBox.MessageBoxType.Warning,
                    ModernMessageBox.MessageBoxButtons.OK,
                    ModernMessageBox.ButtonStyle.Warning);
                StatusMessage = $"Save failed: {errors[0]}";
                return;
            }

            // Save
            var success = await _modelService.SaveModelAsync(CurrentModel);

            if (success)
            {
                IsEditing = false;
                _modelService.SetCurrentModel(CurrentModel);
                if (!string.Equals(oldName, CurrentModel.Name, StringComparison.OrdinalIgnoreCase))
                {
                    _modelService.DeleteModel(oldName);
                }
                await RefreshModelListAsync();
                StatusMessage = $"Model saved: {CurrentModel.Name}";
            }
            else
            {
                ModernMessageBox.Show(
                    "Failed to save model. Please check the logs for details.",
                    "Save Failed",
                    ModernMessageBox.MessageBoxType.Error,
                    ModernMessageBox.MessageBoxButtons.OK,
                    ModernMessageBox.ButtonStyle.Danger);
                StatusMessage = "Failed to save model";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save model");
            ModernMessageBox.Show(
                $"Error saving model:\n\n{ex.Message}",
                "Save Error",
                ModernMessageBox.MessageBoxType.Error,
                ModernMessageBox.MessageBoxButtons.OK,
                ModernMessageBox.ButtonStyle.Danger);
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

            // Confirm before delete
            var result = ModernMessageBox.Show(
                $"Are you sure you want to delete model '{modelName}'?\n\nThis action cannot be undone.",
                "Delete Model",
                ModernMessageBox.MessageBoxType.Warning,
                ModernMessageBox.MessageBoxButtons.YesNo,
                ModernMessageBox.ButtonStyle.Danger);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "Delete canceled";
                return;
            }

            var success = _modelService.DeleteModel(modelName);

            if (success)
            {
                await RefreshModelListAsync();

                if (CurrentModel == null)
                {
                    // Load most recent model or first available
                    var recentModel = await _modelService.LoadMostRecentModelAsync();
                    if (recentModel == null && ModelList.Count > 0)
                    {
                        await LoadModelAsync(ModelList[0].Name);
                    }
                    else if (recentModel == null)
                    {
                        ClearEditFields();
                    }
                }

                StatusMessage = $"Model deleted: {modelName}";
            }
            else
            {
                ModernMessageBox.Show(
                    $"Failed to delete model '{modelName}'.",
                    "Delete Failed",
                    ModernMessageBox.MessageBoxType.Error,
                    ModernMessageBox.MessageBoxButtons.OK,
                    ModernMessageBox.ButtonStyle.Danger);
                StatusMessage = $"Failed to delete model: {modelName}";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to delete model");
            ModernMessageBox.Show(
                $"Error deleting model: {ex.Message}",
                "Delete Error",
                ModernMessageBox.MessageBoxType.Error,
                ModernMessageBox.MessageBoxButtons.OK,
                ModernMessageBox.ButtonStyle.Danger);
            StatusMessage = $"Error deleting model: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task CloneModelAsync()
    {
        if (CurrentModel == null) return;

        try
        {
            // Ask for new name
            var owner = Application.Current?.MainWindow;
            var suggestedName = $"{CurrentModel.Name} (Copy)";
            var name = TextKeyboardDialog.Show(owner, suggestedName)?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                StatusMessage = "Clone canceled";
                return;
            }

            // Check if name already exists
            if (_modelService.ModelExists(name))
            {
                ModernMessageBox.Show(
                    $"A model with name '{name}' already exists.\n\nPlease choose a different name.",
                    "Name Already Exists",
                    ModernMessageBox.MessageBoxType.Warning,
                    ModernMessageBox.MessageBoxButtons.OK,
                    ModernMessageBox.ButtonStyle.Warning);
                StatusMessage = $"Clone failed: Name already exists";
                return;
            }

            // Confirm clone
            var result = ModernMessageBox.Show(
                $"Clone model '{CurrentModel.Name}' as '{name}'?",
                "Clone Model",
                ModernMessageBox.MessageBoxType.Question,
                ModernMessageBox.MessageBoxButtons.YesNo,
                ModernMessageBox.ButtonStyle.Primary);

            if (result != MessageBoxResult.Yes)
            {
                StatusMessage = "Clone canceled";
                return;
            }

            var clone = await _modelService.CloneModelAsync(CurrentModel.Name, name);

            if (clone != null)
            {
                await RefreshModelListAsync();
                await LoadModelAsync(name);
                IsEditing = true;
                StatusMessage = $"Model cloned: {name}";
            }
            else
            {
                ModernMessageBox.Show(
                    $"Failed to clone model.",
                    "Clone Failed",
                    ModernMessageBox.MessageBoxType.Error,
                    ModernMessageBox.MessageBoxButtons.OK,
                    ModernMessageBox.ButtonStyle.Danger);
                StatusMessage = "Clone failed";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to clone model");
            ModernMessageBox.Show(
                $"Error cloning model: {ex.Message}",
                "Clone Error",
                ModernMessageBox.MessageBoxType.Error,
                ModernMessageBox.MessageBoxButtons.OK,
                ModernMessageBox.ButtonStyle.Danger);
            StatusMessage = $"Error cloning model: {ex.Message}";
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
public partial class ModelListItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private DateTime _modifiedAt;

    [ObservableProperty]
    private bool _isCurrentModel;
}
