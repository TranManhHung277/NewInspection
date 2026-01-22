namespace NAutoSuite.Core.Configuration;

public class IoPointConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Direction { get; set; } = "Input";
    public int NodeId { get; set; }
    public int IoBit { get; set; }
    public string DataType { get; set; } = "bit";
    public bool DefaultValue { get; set; }
    public string HardwareName { get; set; } = string.Empty;

    public static string ResolveAddress(IoPointConfig point)
    {
        if (!string.IsNullOrWhiteSpace(point.Address) && point.Address != "0")
        {
            return point.Address;
        }

        var prefix = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase)
            ? "O"
            : "I";
        return $"{prefix}{point.NodeId}:{point.IoBit}";
    }
}

public class RegisterConfig
{
    public Dictionary<string, string> Inputs { get; set; } = new();
    public Dictionary<string, string> Outputs { get; set; } = new();
}

public sealed class HardwareProfileConfig
{
    public List<AxisDefinition> Axes { get; } = new();
    public List<IoPointConfig> IoPoints { get; } = new();
    public List<DataPointConfig> DataPoints { get; } = new();
    public RegisterConfig Registers { get; } = new();

    public void AddAxes(IEnumerable<AxisDefinition> axes)
    {
        Axes.AddRange(axes);
    }

    public void AddIoPoints(string hardwareName, IEnumerable<IoPointConfig> points)
    {
        foreach (var point in points)
        {
            IoPoints.Add(new IoPointConfig
            {
                Id = point.Id,
                Name = point.Name,
                Address = point.Address,
                Direction = point.Direction,
                NodeId = point.NodeId,
                IoBit = point.IoBit,
                DataType = point.DataType,
                DefaultValue = point.DefaultValue,
                HardwareName = hardwareName
            });
        }
    }

    public void AddDataPoints(string hardwareName, IEnumerable<DataPointConfig> points)
    {
        foreach (var point in points)
        {
            DataPoints.Add(new DataPointConfig
            {
                Id = point.Id,
                Name = point.Name,
                Address = point.Address,
                Axis = point.Axis,
                DataType = point.DataType,
                DefaultValue = point.DefaultValue,
                HardwareName = hardwareName
            });
        }
    }

    public void MergeRegisters(RegisterConfig registers)
    {
        MergeRegisters(Registers.Inputs, registers.Inputs);
        MergeRegisters(Registers.Outputs, registers.Outputs);
    }

    private static void MergeRegisters(Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var entry in source)
        {
            if (target.ContainsKey(entry.Key))
            {
                throw new InvalidOperationException($"Duplicate register key '{entry.Key}' across hardware");
            }
            target[entry.Key] = entry.Value;
        }
    }

}

public class DataPointConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Axis { get; set; }
    public string DataType { get; set; } = "int32";
    public double DefaultValue { get; set; }
    public string HardwareName { get; set; } = string.Empty;
}
