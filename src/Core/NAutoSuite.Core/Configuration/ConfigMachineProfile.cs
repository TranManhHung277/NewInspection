using NAutoSuite.Core.IO;

namespace NAutoSuite.Core.Configuration;

public sealed class ConfigMachineProfile : MachineProfile
{
    private readonly DynamicIOMap _ioMap;

    public ConfigMachineProfile()
        : this(new HardwareProfileConfig())
    {
    }

    public ConfigMachineProfile(HardwareProfileConfig config)
        : base(BuildIoMap(config, out var ioMap), BuildRegisterMap(config), BuildAxes(config), BuildDefaultIoValues(config))
    {
        _ioMap = ioMap;
    }

    public void ApplyConfig(HardwareProfileConfig config)
    {
        _ioMap.Reset();
        UpdateDefaultIoValues(BuildDefaultIoValues(config));
        ApplyIoPoints(_ioMap, config.IoPoints);

        var registerMap = RegisterMap;
        registerMap.Clear();
        foreach (var entry in config.Registers.Inputs)
        {
            registerMap.AddInput(entry.Key, entry.Value);
        }
        foreach (var entry in config.Registers.Outputs)
        {
            registerMap.AddOutput(entry.Key, entry.Value);
        }
    }

    private static IOMap BuildIoMap(HardwareProfileConfig config, out DynamicIOMap ioMap)
    {
        ioMap = new DynamicIOMap();
        ApplyIoPoints(ioMap, config.IoPoints);
        return ioMap;
    }

    private static void ApplyIoPoints(DynamicIOMap map, IEnumerable<IoPointConfig> points)
    {
        foreach (var point in points)
        {
            var address = IoPointConfig.ResolveAddress(point);
            var isOutput = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase);
            if (isOutput)
            {
                map.AddOutput(address);
            }
            else
            {
                map.AddInput(address);
            }

            var normalized = NormalizeIoMapName(point.Id);
            if (isOutput)
            {
                map.TrySetCommonOutput(point.Id, address);
                map.TrySetCommonOutput(normalized, address);
            }
            else
            {
                map.TrySetCommonInput(point.Id, address);
                map.TrySetCommonInput(normalized, address);
            }
        }
    }

    private static RegisterMap BuildRegisterMap(HardwareProfileConfig config)
    {
        var map = new RegisterMap();
        foreach (var entry in config.Registers.Inputs)
        {
            map.AddInput(entry.Key, entry.Value);
        }
        foreach (var entry in config.Registers.Outputs)
        {
            map.AddOutput(entry.Key, entry.Value);
        }
        return map;
    }

    private static IReadOnlyList<AxisDefinition> BuildAxes(HardwareProfileConfig config)
    {
        return config.Axes.Select(axis => new AxisDefinition
        {
            Id = axis.Id,
            Name = axis.Name,
            Description = axis.Description,
            AxisIndex = axis.AxisIndex
        }).ToList();
    }

    private static IReadOnlyDictionary<string, bool> BuildDefaultIoValues(HardwareProfileConfig config)
    {
        var defaults = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        foreach (var point in config.IoPoints)
        {
            var address = IoPointConfig.ResolveAddress(point);
            if (!defaults.ContainsKey(address))
            {
                defaults[address] = point.DefaultValue;
            }
        }
        return defaults;
    }

    private static string NormalizeIoMapName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return id;
        }

        var parts = id.Split(new[] { '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return id;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var part in parts)
        {
            if (part.Length == 0) continue;
            sb.Append(char.ToUpperInvariant(part[0]));
            if (part.Length > 1)
            {
                sb.Append(part.AsSpan(1));
            }
        }
        return sb.ToString();
    }
}
