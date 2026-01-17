using System.Text.Json;
using Serilog;

namespace NAutoSuite.Core.Model;

/// <summary>
/// Service for managing machine models (save, load, list, delete)
/// Models are stored as JSON files in the models directory
/// </summary>
public class ModelService<TModel> where TModel : ModelBase, new()
{
    private readonly string _modelsDirectory;
    private readonly ILogger _logger;
    private TModel? _currentModel;
    private readonly List<TModel> _cachedModels = new();

    /// <summary>
    /// Event fired when the current model changes
    /// </summary>
    public event EventHandler<ModelChangedEventArgs<TModel>>? ModelChanged;

    /// <summary>
    /// Event fired when a model is saved
    /// </summary>
    public event EventHandler<TModel>? ModelSaved;

    /// <summary>
    /// The currently loaded model
    /// </summary>
    public TModel? CurrentModel
    {
        get => _currentModel;
        private set
        {
            var oldModel = _currentModel;
            _currentModel = value;
            ModelChanged?.Invoke(this, new ModelChangedEventArgs<TModel>(oldModel, value));
        }
    }

    /// <summary>
    /// Name of the current model (or "No Model" if none loaded)
    /// </summary>
    public string CurrentModelName => CurrentModel?.Name ?? "No Model";

    /// <summary>
    /// Whether a model is currently loaded
    /// </summary>
    public bool HasModel => CurrentModel != null;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Create a new ModelService
    /// </summary>
    /// <param name="machineName">Machine/project name for organizing models</param>
    /// <param name="logger">Optional logger</param>
    public ModelService(string machineName, ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;

        // Store models in AppData/Local/NAutoSuite/{MachineName}/Models/
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        _modelsDirectory = Path.Combine(appDataPath, "NAutoSuite", machineName, "Models");

        // Ensure directory exists
        Directory.CreateDirectory(_modelsDirectory);

        _logger.Debug("ModelService initialized. Models directory: {Path}", _modelsDirectory);
    }

    /// <summary>
    /// Create a new ModelService with custom path
    /// </summary>
    public ModelService(string machineName, string customPath, ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
        _modelsDirectory = customPath;
        Directory.CreateDirectory(_modelsDirectory);
        _logger.Debug("ModelService initialized with custom path: {Path}", _modelsDirectory);
    }

    /// <summary>
    /// Get the file path for a model
    /// </summary>
    private string GetModelFilePath(string modelName)
    {
        // Sanitize filename
        var safeName = string.Join("_", modelName.Split(Path.GetInvalidFileNameChars()));
        return Path.Combine(_modelsDirectory, $"{safeName}.json");
    }

    /// <summary>
    /// Save the current model to file
    /// </summary>
    public async Task<bool> SaveCurrentModelAsync()
    {
        if (CurrentModel == null)
        {
            _logger.Warning("No model to save");
            return false;
        }

        return await SaveModelAsync(CurrentModel);
    }

    /// <summary>
    /// Save a model to file
    /// </summary>
    public async Task<bool> SaveModelAsync(TModel model)
    {
        try
        {
            model.MarkModified();

            var filePath = GetModelFilePath(model.Name);
            var json = JsonSerializer.Serialize(model, JsonOptions);
            await File.WriteAllTextAsync(filePath, json);

            _logger.Information("Model saved: {Name} -> {Path}", model.Name, filePath);

            // Update cache
            var existingIndex = _cachedModels.FindIndex(m => m.Id == model.Id);
            if (existingIndex >= 0)
            {
                _cachedModels[existingIndex] = model;
            }
            else
            {
                _cachedModels.Add(model);
            }

            ModelSaved?.Invoke(this, model);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save model: {Name}", model.Name);
            return false;
        }
    }

    /// <summary>
    /// Load a model by name
    /// </summary>
    public async Task<TModel?> LoadModelAsync(string modelName)
    {
        try
        {
            var filePath = GetModelFilePath(modelName);

            if (!File.Exists(filePath))
            {
                _logger.Warning("Model file not found: {Path}", filePath);
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            var model = JsonSerializer.Deserialize<TModel>(json, JsonOptions);

            if (model != null)
            {
                CurrentModel = model;
                _logger.Information("Model loaded: {Name}", model.Name);
            }

            return model;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load model: {Name}", modelName);
            return null;
        }
    }

    /// <summary>
    /// Set the current model without loading from file
    /// </summary>
    public void SetCurrentModel(TModel model)
    {
        CurrentModel = model;
        _logger.Information("Current model set to: {Name}", model.Name);
    }

    /// <summary>
    /// Create a new model with default values
    /// </summary>
    public TModel CreateNewModel(string name)
    {
        var model = new TModel
        {
            Name = name,
            CreatedAt = DateTime.Now
        };

        CurrentModel = model;
        _logger.Information("New model created: {Name}", name);
        return model;
    }

    /// <summary>
    /// Get list of all available models
    /// </summary>
    public async Task<List<TModel>> GetAllModelsAsync(bool forceRefresh = false)
    {
        if (!forceRefresh && _cachedModels.Count > 0)
        {
            return _cachedModels;
        }

        _cachedModels.Clear();

        try
        {
            var files = Directory.GetFiles(_modelsDirectory, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var model = JsonSerializer.Deserialize<TModel>(json, JsonOptions);
                    if (model != null)
                    {
                        _cachedModels.Add(model);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warning(ex, "Failed to load model from file: {File}", file);
                }
            }

            _logger.Debug("Loaded {Count} models from directory", _cachedModels.Count);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to enumerate models");
        }

        return _cachedModels;
    }

    /// <summary>
    /// Get list of model names only (faster than loading all models)
    /// </summary>
    public List<string> GetModelNames()
    {
        try
        {
            var files = Directory.GetFiles(_modelsDirectory, "*.json");
            return files.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to get model names");
            return new List<string>();
        }
    }

    /// <summary>
    /// Delete a model by name
    /// </summary>
    public bool DeleteModel(string modelName)
    {
        try
        {
            var filePath = GetModelFilePath(modelName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _cachedModels.RemoveAll(m => m.Name == modelName);

                // Clear current model if it was deleted
                if (CurrentModel?.Name == modelName)
                {
                    CurrentModel = null;
                }

                _logger.Information("Model deleted: {Name}", modelName);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to delete model: {Name}", modelName);
            return false;
        }
    }

    /// <summary>
    /// Clone an existing model with a new name
    /// </summary>
    public async Task<TModel?> CloneModelAsync(string sourceName, string newName)
    {
        var source = await LoadModelAsync(sourceName);
        if (source == null) return null;

        var clone = (TModel)source.Clone(newName);
        await SaveModelAsync(clone);

        return clone;
    }

    /// <summary>
    /// Check if a model exists
    /// </summary>
    public bool ModelExists(string modelName)
    {
        return File.Exists(GetModelFilePath(modelName));
    }

    /// <summary>
    /// Load the most recently modified model (or first available if none modified)
    /// </summary>
    public async Task<TModel?> LoadMostRecentModelAsync()
    {
        var models = await GetAllModelsAsync(forceRefresh: true);

        if (models.Count == 0)
        {
            _logger.Information("No models found to load");
            return null;
        }

        // Get the most recently modified model
        var recentModel = models
            .OrderByDescending(m => m.ModifiedAt ?? m.CreatedAt)
            .First();

        CurrentModel = recentModel;
        _logger.Information("Most recent model loaded: {Name} (Modified: {Date})",
            recentModel.Name, recentModel.ModifiedAt ?? recentModel.CreatedAt);

        return recentModel;
    }


    /// <summary>
    /// Get the models directory path
    /// </summary>
    public string ModelsDirectory => _modelsDirectory;
}

/// <summary>
/// Event args for model changed events
/// </summary>
public class ModelChangedEventArgs<TModel> : EventArgs where TModel : ModelBase
{
    public TModel? OldModel { get; }
    public TModel? NewModel { get; }

    public ModelChangedEventArgs(TModel? oldModel, TModel? newModel)
    {
        OldModel = oldModel;
        NewModel = newModel;
    }
}
