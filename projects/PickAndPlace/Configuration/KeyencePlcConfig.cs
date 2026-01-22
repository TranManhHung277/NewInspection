using System.Globalization;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PickAndPlace.Configuration;

public class KeyencePlcSettings
{
    public string Ip { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 8501;
    public bool UsePlc { get; set; } = true;
    public string Name { get; set; } = "KeyencePLC";
    public List<IoPointConfig> IoPoints { get; set; } = new();
    public RegisterConfig Registers { get; set; } = new();

    public static KeyencePlcSettings LoadFromYamlNode(YamlNode node)
    {
        var stream = new YamlStream(new YamlDocument(node));
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        stream.Save(writer, false);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
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
