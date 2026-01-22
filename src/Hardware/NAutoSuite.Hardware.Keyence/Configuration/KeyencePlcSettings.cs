using System.Globalization;
using NAutoSuite.Core.Configuration;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NAutoSuite.Hardware.Keyence.Configuration;

public class KeyencePlcSettings
{
    public string Ip { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8501;
    public string Name { get; set; } = "KeyencePLC";
    public List<KeyenceDataConfig> Data { get; set; } = new();
    public List<KeyenceIoConfig> CommonIo { get; set; } = new();
    public List<KeyenceIoConfig> Io { get; set; } = new();

    public static KeyencePlcSettings LoadFromYamlNode(YamlNode node)
    {
        var stream = new YamlStream(new YamlDocument(node));
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        stream.Save(writer, false);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var settings = deserializer.Deserialize<KeyencePlcSettings>(new StringReader(writer.ToString()));
        if (settings == null)
        {
            throw new InvalidOperationException("Failed to load Keyence PLC config");
        }

        return settings;
    }
}

public class KeyenceDataConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Type { get; set; } = "int32";
    public string Default { get; set; } = string.Empty;
}

public class KeyenceIoConfig
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Direction { get; set; } = "Input";
    public string Type { get; set; } = "bit";
    public bool Default { get; set; }
}
