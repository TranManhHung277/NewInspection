using System.Reflection;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.IO;

namespace PickAndPlace.Machine;

/// <summary>
/// Project profile: IO map, register map, and axis definitions.
/// </summary>
public class PickAndPlaceProfile : MachineProfile
{
    public PickAndPlaceProfile()
        : this(new HardwareProfileConfig())
    {
    }

    public PickAndPlaceProfile(HardwareProfileConfig config)
        : base(CreateIoMapFromConfig(config), CreateRegisterMapFromConfig(config), CreateAxesFromConfig(config), CreateDefaultIoValues(config))
    {
    }

    private static IOMap CreateIoMapFromConfig(HardwareProfileConfig config)
    {
        var map = new PickAndPlaceIOMap();
        foreach (var point in config.IoPoints)
        {
            ApplyIoPoint(map, point);
        }
        return map;
    }

    private static RegisterMap CreateRegisterMapFromConfig(HardwareProfileConfig config)
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

    private static IReadOnlyList<AxisDefinition> CreateAxesFromConfig(HardwareProfileConfig config)
    {
        return config.Axes.Select(axis => new AxisDefinition
        {
            Id = axis.Id,
            Name = axis.Name,
            Description = axis.Description,
            AxisIndex = axis.AxisIndex
        }).ToList();
    }

    private static IReadOnlyDictionary<string, bool> CreateDefaultIoValues(HardwareProfileConfig config)
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

    public void ApplyConfig(HardwareProfileConfig config)
    {
        var defaults = CreateDefaultIoValues(config);
        UpdateDefaultIoValues(defaults);
        ResetIoMap();
        foreach (var point in config.IoPoints)
        {
            ApplyIoPoint((PickAndPlaceIOMap)IOMap, point);
        }

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

    private static void ApplyIoPoint(PickAndPlaceIOMap map, IoPointConfig point)
    {
        var address = IoPointConfig.ResolveAddress(point);
        var isOutput = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase);
        var name = point.Id;
        var normalized = NormalizeIoMapName(name);

        object target = isOutput ? map.CommonOutputs : map.CommonInputs;
        if (!TrySetAddress(target, name, address) && !TrySetAddress(target, normalized, address))
        {
            target = isOutput ? map.MachineOutputs : map.MachineInputs;
            TrySetAddress(target, name, address);
            TrySetAddress(target, normalized, address);
        }
    }

    private static bool TrySetAddress(object target, string name, string address)
    {
        var prop = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (prop == null || prop.PropertyType != typeof(string)) return false;
        prop.SetValue(target, address);
        return true;
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

    private void ResetIoMap()
    {
        ResetStringProperties(IOMap.CommonInputs);
        ResetStringProperties(IOMap.CommonOutputs);
        ResetStringProperties(((PickAndPlaceIOMap)IOMap).MachineInputs);
        ResetStringProperties(((PickAndPlaceIOMap)IOMap).MachineOutputs);
    }

    private static void ResetStringProperties(object target)
    {
        var props = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            if (prop.PropertyType == typeof(string) && prop.CanWrite)
            {
                prop.SetValue(target, string.Empty);
            }
        }
    }
}
