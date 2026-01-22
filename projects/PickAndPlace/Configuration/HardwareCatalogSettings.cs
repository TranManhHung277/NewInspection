using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.RepresentationModel;

namespace PickAndPlace.Configuration;

public sealed class HardwareCatalogSettings
{
    public IReadOnlyList<HardwareDefinition> Hardware { get; }

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
