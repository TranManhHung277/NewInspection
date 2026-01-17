using Serilog;
using System.IO;
using System.Text.Json;

namespace Machine.PickAndPlace.Services;

/// <summary>
/// Service to load/save machine settings to JSON file
/// Settings are stored in AppData/Local/NAutoSuite/PickAndPlace/settings.json
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private readonly ILogger _logger;
    private MachineSettings _settings;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsService(ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;

        // Store settings in AppData/Local/NAutoSuite/PickAndPlace/
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var settingsFolder = Path.Combine(appDataPath, "NAutoSuite", "PickAndPlace");

        // Ensure directory exists
        Directory.CreateDirectory(settingsFolder);

        _settingsPath = Path.Combine(settingsFolder, "settings.json");
        _settings = new MachineSettings();

        _logger.Debug("Settings path: {Path}", _settingsPath);
    }

    /// <summary>
    /// Load settings from file. Returns default settings if file doesn't exist.
    /// </summary>
    public MachineSettings Load()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<MachineSettings>(json, _jsonOptions);

                if (settings != null)
                {
                    _settings = settings;
                    _logger.Information("Settings loaded: MachineNumber={MachineNumber}, ProjectName={ProjectName}",
                        _settings.MachineNumber, _settings.ProjectName);
                    return _settings;
                }
            }

            _logger.Information("No settings file found, using defaults");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load settings, using defaults");
        }

        return _settings;
    }

    /// <summary>
    /// Save settings to file
    /// </summary>
    public void Save(MachineSettings settings)
    {
        try
        {
            _settings = settings;
            var json = JsonSerializer.Serialize(_settings, _jsonOptions);
            File.WriteAllText(_settingsPath, json);

            _logger.Information("Settings saved: MachineNumber={MachineNumber}, ProjectName={ProjectName}",
                _settings.MachineNumber, _settings.ProjectName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save settings");
        }
    }

    /// <summary>
    /// Save individual setting values
    /// </summary>
    public void SaveMachineNumber(int machineNumber)
    {
        _settings.MachineNumber = machineNumber;
        Save(_settings);
    }

    /// <summary>
    /// Save individual setting values
    /// </summary>
    public void SaveProjectName(string projectName)
    {
        _settings.ProjectName = projectName;
        Save(_settings);
    }
}
