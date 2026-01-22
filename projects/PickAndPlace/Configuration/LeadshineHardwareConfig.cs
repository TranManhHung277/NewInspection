using System.IO;
using System.Linq;
using System.Globalization;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PickAndPlace.Configuration;

public class LeadshineHardwareConfig
{
    public int CardNo { get; set; }
    public bool UseCard { get; set; } = true;
    public string? EthercatIp { get; set; }
    public List<AxisConfig> Axes { get; set; } = new();
    public List<IoPointConfig> IoPoints { get; set; } = new();
    public RegisterConfig Registers { get; set; } = new();

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

public class LeadshineHardwareSettings
{
    public string HardwareType { get; set; } = "EtherCATCardMaster_Leadshine";
    public List<LeadshineHardwareConfig> Cards { get; set; } = new();

    public static LeadshineHardwareSettings Load(string path)
    {
        var yaml = File.ReadAllText(path);
        return DeserializeYaml(yaml);
    }

    public static LeadshineHardwareSettings LoadFromYamlNode(YamlNode node)
    {
        var stream = new YamlStream(new YamlDocument(node));
        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        stream.Save(writer, false);

        return DeserializeYaml(writer.ToString());
    }

    public IReadOnlyList<LeadshineHardwareConfig> ResolveUsedCards()
    {
        if (Cards.Count == 0)
        {
            throw new InvalidOperationException("Hardware config contains no cards");
        }

        var usedCards = Cards.Where(card => card.UseCard).ToList();
        if (usedCards.Count == 0)
        {
            throw new InvalidOperationException("Hardware config contains no enabled cards");
        }

        return usedCards;
    }

    public LeadshineHardwareConfig BuildMergedConfig()
    {
        var usedCards = ResolveUsedCards();
        return BuildMergedConfig(usedCards);
    }

    public static LeadshineHardwareConfig BuildMergedConfig(IEnumerable<LeadshineHardwareConfig> cards)
    {
        var usedCards = cards.ToList();
        if (usedCards.Count == 0)
        {
            throw new InvalidOperationException("Hardware config contains no enabled cards");
        }

        var merged = new LeadshineHardwareConfig
        {
            CardNo = usedCards[0].CardNo,
            EthercatIp = usedCards[0].EthercatIp
        };

        foreach (var card in usedCards)
        {
            merged.Axes.AddRange(card.Axes);
            merged.IoPoints.AddRange(card.IoPoints);
            MergeRegisters(merged.Registers.Inputs, card.Registers.Inputs);
            MergeRegisters(merged.Registers.Outputs, card.Registers.Outputs);
        }

        return merged;
    }

    private static void MergeRegisters(Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var entry in source)
        {
            if (target.ContainsKey(entry.Key))
            {
                throw new InvalidOperationException($"Duplicate register key '{entry.Key}' across cards");
            }
            target[entry.Key] = entry.Value;
        }
    }

    private static LeadshineHardwareSettings DeserializeYaml(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        var settings = deserializer.Deserialize<LeadshineHardwareSettings>(new StringReader(yaml));
        if (settings == null)
        {
            throw new InvalidOperationException("Failed to load Leadshine hardware config");
        }

        return settings;
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
