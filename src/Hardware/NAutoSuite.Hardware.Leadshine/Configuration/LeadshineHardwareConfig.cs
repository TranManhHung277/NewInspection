using System.Globalization;
using NAutoSuite.Core.Configuration;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NAutoSuite.Hardware.Leadshine.Configuration;

public class LeadshineHardwareConfig
{
    public int CardNo { get; set; }
    public string Name { get; set; } = "Leadshine";
    public List<LeadshineAxisConfig> Axes { get; set; } = new();
    public List<LeadshineDataConfig> Data { get; set; } = new();
    public List<LeadshineIoConfig> CommonIo { get; set; } = new();
    public List<LeadshineIoConfig> Io { get; set; } = new();

    public static LeadshineHardwareConfig LoadFromYamlNode(YamlNode node)
    {
        var stream = new YamlStream(new YamlDocument(node));
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        stream.Save(writer, false);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var settings = deserializer.Deserialize<LeadshineHardwareConfig>(new StringReader(writer.ToString()));
        if (settings == null)
        {
            throw new InvalidOperationException("Failed to load Leadshine hardware config");
        }

        return settings;
    }
}

public class LeadshineAxisConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int AxisIndex { get; set; }
    public double MinVel { get; set; } = 0.1;
    public double MaxVel { get; set; } = 100.0;
    public double Acc { get; set; } = 0.2;
    public double Dec { get; set; } = 0.2;
    public double StopVel { get; set; } = 0.0;
    public double JogVel { get; set; } = 50.0;
    public double GearEquiv { get; set; } = 0.0;
    public ushort RunMode { get; set; } = 8;
    public bool ServoOnConnect { get; set; } = true;
    public bool EnableGear { get; set; }
    public ushort GearMasterType { get; set; }
    public ushort GearMasterIndex { get; set; }
    public int GearMasterEven { get; set; }
    public int GearSlaveEven { get; set; }
    public uint GearMasterSlope { get; set; }
    public ushort HomeDir { get; set; }
    public double HomeVel { get; set; } = 10.0;
    public ushort HomeMode { get; set; }
    public ushort HomeEzCount { get; set; }
}

public class LeadshineDataConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int Axis { get; set; }
    public string Type { get; set; } = "int32";
    public string Default { get; set; } = string.Empty;
}

public class LeadshineIoConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int NodeId { get; set; }
    public int IoBit { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Direction { get; set; } = "Input";
    public string Type { get; set; } = "bit";
    public bool Default { get; set; }
}
