using NAutoSuite.Core.IO;

namespace NAutoSuite.Core.Configuration;

/// <summary>
/// Project-level hardware profile: IO map, register map, and axes.
/// </summary>
public class MachineProfile
{
    private readonly Dictionary<string, bool> _defaultIoValues;

    public MachineProfile(IOMap ioMap)
        : this(ioMap, new RegisterMap(), Array.Empty<AxisDefinition>(), null)
    {
    }

    public MachineProfile(
        IOMap ioMap,
        RegisterMap registerMap,
        IEnumerable<AxisDefinition> axes,
        IReadOnlyDictionary<string, bool>? defaultIoValues)
    {
        IOMap = ioMap ?? throw new ArgumentNullException(nameof(ioMap));
        RegisterMap = registerMap ?? throw new ArgumentNullException(nameof(registerMap));
        _defaultIoValues = defaultIoValues != null
            ? new Dictionary<string, bool>(defaultIoValues, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        var list = axes?.ToList() ?? new List<AxisDefinition>();
        Axes = list.AsReadOnly();
    }

    public IOMap IOMap { get; }

    public RegisterMap RegisterMap { get; }

    public IReadOnlyList<AxisDefinition> Axes { get; }

    public IReadOnlyDictionary<string, bool> DefaultIoValues => _defaultIoValues;

    public void UpdateDefaultIoValues(IReadOnlyDictionary<string, bool> values)
    {
        _defaultIoValues.Clear();
        foreach (var entry in values)
        {
            _defaultIoValues[entry.Key] = entry.Value;
        }
    }
}

/// <summary>
/// Axis definition used by project profiles.
/// </summary>
public class AxisDefinition
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AxisIndex { get; set; }
}
