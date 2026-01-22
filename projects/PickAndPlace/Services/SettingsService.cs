using System;
using Serilog;
using System.Globalization;
using System.IO;
using YamlDotNet.RepresentationModel;

namespace PickAndPlace.Services;

/// <summary>
/// Service to load/save machine settings in hardware_config.yaml
/// </summary>
public class SettingsService
{
    private readonly string _settingsPath;
    private readonly ILogger _logger;
    private MachineSettings _settings;

    public SettingsService(ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
        _settingsPath = Path.Combine(AppContext.BaseDirectory, "Configs", "hardware_config.yaml");
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
                var yaml = File.ReadAllText(_settingsPath);
                var stream = new YamlStream();
                using var reader = new StringReader(yaml);
                stream.Load(reader);

                if (stream.Documents.Count > 0 &&
                    stream.Documents[0].RootNode is YamlMappingNode root &&
                    root.Children.TryGetValue(new YamlScalarNode("app"), out var appNode) &&
                    appNode is YamlMappingNode appMap)
                {
                    _settings.ProjectName = GetScalar(appMap, "name", _settings.ProjectName);
                    _settings.Version = GetScalar(appMap, "version", _settings.Version);
                    _settings.MachineNumber = GetInt(appMap, "machine", _settings.MachineNumber);
                    _settings.LastModelName = GetOptionalScalar(appMap, "lastModelName");
                }

                _logger.Information("Settings loaded: MachineNumber={MachineNumber}, ProjectName={ProjectName}",
                    _settings.MachineNumber, _settings.ProjectName);
                return _settings;
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
            var stream = new YamlStream();
            YamlMappingNode root;

            if (File.Exists(_settingsPath))
            {
                var yaml = File.ReadAllText(_settingsPath);
                using var reader = new StringReader(yaml);
                stream.Load(reader);
            }

            if (stream.Documents.Count == 0 || stream.Documents[0].RootNode is not YamlMappingNode)
            {
                root = new YamlMappingNode();
                stream.Documents.Clear();
                stream.Documents.Add(new YamlDocument(root));
            }
            else
            {
                root = (YamlMappingNode)stream.Documents[0].RootNode;
            }

            if (!root.Children.TryGetValue(new YamlScalarNode("app"), out var appNode) ||
                appNode is not YamlMappingNode appMap)
            {
                appMap = new YamlMappingNode();
                root.Children[new YamlScalarNode("app")] = appMap;
            }

            SetScalar(appMap, "name", _settings.ProjectName);
            SetScalar(appMap, "version", _settings.Version);
            SetScalar(appMap, "machine", _settings.MachineNumber.ToString(CultureInfo.InvariantCulture));
            SetOptionalScalar(appMap, "lastModelName", _settings.LastModelName);

            using var writer = new StringWriter(CultureInfo.InvariantCulture);
            stream.Save(writer, false);
            File.WriteAllText(_settingsPath, writer.ToString());

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

    private static string GetScalar(YamlMappingNode node, string key, string fallback)
    {
        if (!node.Children.TryGetValue(new YamlScalarNode(key), out var valueNode))
        {
            return fallback;
        }

        var value = (valueNode as YamlScalarNode)?.Value;
        return string.IsNullOrWhiteSpace(value) ? fallback : value;
    }

    private static string? GetOptionalScalar(YamlMappingNode node, string key)
    {
        if (!node.Children.TryGetValue(new YamlScalarNode(key), out var valueNode))
        {
            return null;
        }

        var value = (valueNode as YamlScalarNode)?.Value;
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static int GetInt(YamlMappingNode node, string key, int fallback)
    {
        var value = GetScalar(node, key, fallback.ToString(CultureInfo.InvariantCulture));
        return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : fallback;
    }

    private static void SetScalar(YamlMappingNode node, string key, string value)
    {
        node.Children[new YamlScalarNode(key)] = new YamlScalarNode(value);
    }

    private static void SetOptionalScalar(YamlMappingNode node, string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            node.Children.Remove(new YamlScalarNode(key));
            return;
        }

        SetScalar(node, key, value);
    }
}
