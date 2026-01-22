using System.Reflection;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.IO;
using PickAndPlace.Configuration;

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

    public PickAndPlaceProfile(LeadshineHardwareConfig config)
        : this(CreateFromLeadshine(config))
    {
    }

    public PickAndPlaceProfile(HardwareProfileConfig config)
        : base(CreateIoMapFromConfig(config), CreateRegisterMapFromConfig(config), CreateAxesFromConfig(config))
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

    private static HardwareProfileConfig CreateFromLeadshine(LeadshineHardwareConfig config)
    {
        var profile = new HardwareProfileConfig();
        profile.AddLeadshine(config);
        return profile;
    }

    private static void ApplyIoPoint(PickAndPlaceIOMap map, IoPointConfig point)
    {
        var address = LeadshineHardwareConfig.ResolveAddress(point);
        var isOutput = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase);

        object target = isOutput ? map.CommonOutputs : map.CommonInputs;
        if (!TrySetAddress(target, point.Name, address))
        {
            target = isOutput ? map.MachineOutputs : map.MachineInputs;
            TrySetAddress(target, point.Name, address);
        }
    }

    private static bool TrySetAddress(object target, string name, string address)
    {
        var prop = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (prop == null || prop.PropertyType != typeof(string)) return false;
        prop.SetValue(target, address);
        return true;
    }
}
