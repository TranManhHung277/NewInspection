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
    public List<AxisDefinition> Axes { get; set; } = new();
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

public class LeadshineDataConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Axis { get; set; }
    public string Type { get; set; } = "int32";
    public double Default { get; set; }
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
