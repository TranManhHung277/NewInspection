using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Configuration;
using YamlDotNet.RepresentationModel;

namespace NAutoSuite.Hardware.Abstractions.Hardware;

public interface IHardwareModule
{
    string Type { get; }

    HardwareModuleResult LoadFromConfig(YamlNode configNode);
}

public sealed class HardwareModuleResult
{
    public HardwareModuleResult(IEnumerable<IDevice> devices,
        IEnumerable<AxisDefinition> axes,
        IEnumerable<IoPointConfig> ioPoints,
        IEnumerable<DataPointConfig> dataPoints,
        RegisterConfig registers)
    {
        Devices = devices.ToList().AsReadOnly();
        Axes = axes.ToList().AsReadOnly();
        IoPoints = ioPoints.ToList().AsReadOnly();
        DataPoints = dataPoints.ToList().AsReadOnly();
        Registers = registers ?? throw new ArgumentNullException(nameof(registers));
    }

    public IReadOnlyList<IDevice> Devices { get; }
    public IReadOnlyList<AxisDefinition> Axes { get; }
    public IReadOnlyList<IoPointConfig> IoPoints { get; }
    public IReadOnlyList<DataPointConfig> DataPoints { get; }
    public RegisterConfig Registers { get; }
}
