using NAutoSuite.Core.IO;

namespace NAutoSuite.Core.Configuration;

/// <summary>
/// Project-level hardware profile: IO map, register map, and axes.
/// </summary>
public class MachineProfile
{
    public MachineProfile(IOMap ioMap)
        : this(ioMap, new RegisterMap(), Array.Empty<AxisDefinition>())
    {
    }

    public MachineProfile(IOMap ioMap, RegisterMap registerMap, IEnumerable<AxisDefinition> axes)
    {
        IOMap = ioMap ?? throw new ArgumentNullException(nameof(ioMap));
        RegisterMap = registerMap ?? throw new ArgumentNullException(nameof(registerMap));
        var list = axes?.ToList() ?? new List<AxisDefinition>();
        Axes = list.AsReadOnly();
    }

    public IOMap IOMap { get; }

    public RegisterMap RegisterMap { get; }

    public IReadOnlyList<AxisDefinition> Axes { get; }
}

/// <summary>
/// Axis definition used by project profiles.
/// </summary>
public class AxisDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}
