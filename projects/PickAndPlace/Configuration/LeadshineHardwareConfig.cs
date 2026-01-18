using System.IO;
using System.Text.Json;

namespace PickAndPlace.Configuration;

public class LeadshineHardwareConfig
{
    public int CardNo { get; set; }
    public string? EthercatIp { get; set; }
    public List<AxisConfig> Axes { get; set; } = new();
    public List<IoPointConfig> IoPoints { get; set; } = new();
    public RegisterConfig Registers { get; set; } = new();

    public static LeadshineHardwareConfig Load(string path)
    {
        var json = File.ReadAllText(path);
        var config = JsonSerializer.Deserialize<LeadshineHardwareConfig>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null)
        {
            throw new InvalidOperationException("Failed to load hardware config");
        }

        return config;
    }

    public static string ResolveAddress(IoPointConfig point)
    {
        if (!string.IsNullOrWhiteSpace(point.Address))
        {
            return point.Address;
        }

        var prefix = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase)
            ? "QX"
            : "IX";
        return $"{prefix}{point.NodeId}.{point.IoBit}";
    }
}

public class AxisConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int AxisIndex { get; set; }
    public string? Description { get; set; }
}

public class IoPointConfig
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Direction { get; set; } = "Input";
    public int NodeId { get; set; }
    public int IoBit { get; set; }
}

public class RegisterConfig
{
    public Dictionary<string, string> Inputs { get; set; } = new();
    public Dictionary<string, string> Outputs { get; set; } = new();
}
