using System.Linq;
using NAutoSuite.Core.Configuration;

namespace NAutoSuite.Core.Entities;

public sealed class DataEntityDescriptor
{
    public DataEntityDescriptor(string systemName, string address, string dataType, double defaultValue)
    {
        SystemName = systemName;
        Address = address;
        DataType = dataType;
        DefaultValue = defaultValue;
    }

    public string SystemName { get; }
    public string Address { get; }
    public string DataType { get; }
    public double DefaultValue { get; }
}

public sealed class DataEntityRegistry
{
    private readonly Dictionary<string, DataEntityDescriptor> _entities =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    public DataEntityRegistry(IEnumerable<DataPointConfig> points)
    {
        Reload(points);
    }

    public IReadOnlyCollection<DataEntityDescriptor> All
    {
        get
        {
            lock (_sync)
            {
                return _entities.Values.ToList();
            }
        }
    }

    public bool TryGet(string systemName, out DataEntityDescriptor descriptor)
    {
        lock (_sync)
        {
            return _entities.TryGetValue(systemName, out descriptor!);
        }
    }

    public void Reload(IEnumerable<DataPointConfig> points)
    {
        lock (_sync)
        {
            _entities.Clear();
            foreach (var point in points)
            {
                if (string.IsNullOrWhiteSpace(point.Id))
                {
                    continue;
                }

                var hardwareName = NormalizeSegment(point.HardwareName);
                var id = NormalizeSegment(point.Id);
                var systemName = string.IsNullOrWhiteSpace(hardwareName)
                    ? id
                    : $"{hardwareName}.data.{id}";

                if (_entities.ContainsKey(systemName))
                {
                    throw new InvalidOperationException($"Duplicate data systemName '{systemName}'");
                }

                _entities[systemName] = new DataEntityDescriptor(systemName, point.Address, point.DataType, point.DefaultValue);
            }
        }
    }

    private static string NormalizeSegment(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var sb = new System.Text.StringBuilder();
        foreach (var ch in value.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch == '_')
            {
                sb.Append(char.ToLowerInvariant(ch));
            }
            else if (char.IsWhiteSpace(ch) || ch == '.' || ch == '-')
            {
                sb.Append('_');
            }
        }

        return sb.ToString();
    }
}
