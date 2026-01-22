using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Serilog;
using YamlDotNet.RepresentationModel;

namespace PickAndPlace.Configuration;

public sealed class HardwareCatalogSettings
{
    public IReadOnlyList<HardwareDefinition> Hardware { get; }

    public static HardwareCatalogSettings Empty { get; } =
        new HardwareCatalogSettings(new List<HardwareDefinition>());

    private HardwareCatalogSettings(List<HardwareDefinition> hardware)
    {
        Hardware = hardware;
    }

    public static HardwareCatalogSettings Load(string path)
    {
        var yaml = File.ReadAllText(path);
        var yamlStream = new YamlStream();
        using var reader = new StringReader(yaml);
        yamlStream.Load(reader);

        if (yamlStream.Documents.Count == 0)
        {
            throw new InvalidOperationException("Hardware config is empty");
        }

        var root = yamlStream.Documents[0].RootNode as YamlMappingNode
            ?? throw new InvalidOperationException("Hardware config root must be a mapping");

        if (!root.Children.TryGetValue(new YamlScalarNode("hardware"), out var hardwareNode))
        {
            throw new InvalidOperationException("Hardware config missing 'hardware' section");
        }

        if (hardwareNode is not YamlMappingNode hardwareMap)
        {
            throw new InvalidOperationException("'hardware' section must be a mapping");
        }

        ValidateHardwareIds(hardwareMap, path);

        var definitions = new List<HardwareDefinition>();
        foreach (var entry in hardwareMap.Children)
        {
            var name = (entry.Key as YamlScalarNode)?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            if (entry.Value is not YamlMappingNode entryMap)
            {
                throw new InvalidOperationException($"Hardware '{name}' entry must be a mapping");
            }

            var type = GetRequiredScalar(entryMap, "type", name);
            var config = GetRequiredNode(entryMap, "config", name);

            definitions.Add(new HardwareDefinition(name, type, config));
        }

        if (definitions.Count == 0)
        {
            throw new InvalidOperationException("Hardware config contains no entries");
        }

        return new HardwareCatalogSettings(definitions);
    }

    public static HardwareCatalogSettings LoadSafe(string path, ILogger? logger = null)
    {
        try
        {
            return Load(path);
        }
        catch (Exception ex)
        {
            var log = (logger ?? Log.Logger)
                .ForContext<HardwareCatalogSettings>()
                .ForContext("Layer", "Config");
            log.Error(ex, "Failed to load hardware config {Path}", path);
            return Empty;
        }
    }

    public IEnumerable<HardwareDefinition> FindByType(string type)
    {
        return Hardware.Where(entry => string.Equals(entry.Type, type, StringComparison.OrdinalIgnoreCase));
    }

    private static string GetRequiredScalar(YamlMappingNode node, string key, string hardwareName)
    {
        if (!node.Children.TryGetValue(new YamlScalarNode(key), out var valueNode))
        {
            throw new InvalidOperationException($"Hardware '{hardwareName}' missing '{key}'");
        }

        var value = (valueNode as YamlScalarNode)?.Value;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Hardware '{hardwareName}' '{key}' must be a non-empty scalar");
        }

        return value;
    }

    private static YamlNode GetRequiredNode(YamlMappingNode node, string key, string hardwareName)
    {
        if (!node.Children.TryGetValue(new YamlScalarNode(key), out var valueNode))
        {
            throw new InvalidOperationException($"Hardware '{hardwareName}' missing '{key}'");
        }

        return valueNode;
    }

    private static void ValidateHardwareIds(YamlMappingNode hardwareMap, string path)
    {
        var logger = Log.ForContext<HardwareCatalogSettings>()
            .ForContext("Layer", "Config");

        var errors = new List<string>();
        var ioIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var entry in hardwareMap.Children)
        {
            var hardwareName = (entry.Key as YamlScalarNode)?.Value ?? string.Empty;
            if (entry.Value is not YamlMappingNode entryMap)
            {
                errors.Add($"Hardware '{hardwareName}' entry must be a mapping");
                continue;
            }

            if (!entryMap.Children.TryGetValue(new YamlScalarNode("config"), out var configNode) ||
                configNode is not YamlMappingNode configMap)
            {
                errors.Add($"Hardware '{hardwareName}' missing or invalid 'config' mapping");
                continue;
            }

            ValidateIdList(configMap, hardwareName, "common_io", ioIds, errors, allowGlobalDuplicates: false);
            ValidateIdList(configMap, hardwareName, "io", ioIds, errors, allowGlobalDuplicates: false);

            var dataIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            ValidateIdList(configMap, hardwareName, "data", dataIds, errors, allowGlobalDuplicates: true);
        }

        if (errors.Count == 0)
        {
            return;
        }

        logger.Error("Hardware config validation failed for {Path}", path);
        foreach (var error in errors)
        {
            logger.Error("{Error}", error);
        }

        throw new InvalidOperationException("Invalid hardware_config.yaml. Check logs for details.");
    }

    private static void ValidateIdList(
        YamlMappingNode configMap,
        string hardwareName,
        string sectionKey,
        HashSet<string> idSet,
        List<string> errors,
        bool allowGlobalDuplicates)
    {
        if (!configMap.Children.TryGetValue(new YamlScalarNode(sectionKey), out var listNode))
        {
            return;
        }

        if (listNode is not YamlSequenceNode sequence)
        {
            errors.Add($"Hardware '{hardwareName}' section '{sectionKey}' must be a list");
            return;
        }

        var index = 0;
        foreach (var item in sequence.Children)
        {
            index++;
            if (item is not YamlMappingNode mapping)
            {
                errors.Add($"Hardware '{hardwareName}' section '{sectionKey}' item #{index} must be a mapping");
                continue;
            }

            if (!mapping.Children.TryGetValue(new YamlScalarNode("id"), out var idNode))
            {
                errors.Add($"Hardware '{hardwareName}' section '{sectionKey}' item #{index} missing 'id'");
                continue;
            }

            var id = (idNode as YamlScalarNode)?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(id))
            {
                errors.Add($"Hardware '{hardwareName}' section '{sectionKey}' item #{index} has empty 'id'");
                continue;
            }

            if (id.Any(char.IsWhiteSpace))
            {
                errors.Add($"Hardware '{hardwareName}' section '{sectionKey}' id '{id}' contains whitespace");
            }

            if (!Regex.IsMatch(id, "^[A-Za-z0-9_]+$"))
            {
                errors.Add($"Hardware '{hardwareName}' section '{sectionKey}' id '{id}' contains invalid characters");
            }

            if (!allowGlobalDuplicates && !idSet.Add(id))
            {
                errors.Add($"Duplicate IO id '{id}' found across hardware definitions");
            }
            else if (allowGlobalDuplicates && !idSet.Add(id))
            {
                errors.Add($"Hardware '{hardwareName}' section '{sectionKey}' has duplicate id '{id}'");
            }
        }
    }

}

public sealed class HardwareDefinition
{
    public string Name { get; }
    public string Type { get; }
    public YamlNode ConfigNode { get; }

    public HardwareDefinition(string name, string type, YamlNode configNode)
    {
        Name = name;
        Type = type;
        ConfigNode = configNode;
    }
}
