using System.Linq;
using NAutoSuite.Core.Configuration;

namespace NAutoSuite.Core.Entities;

public enum EntityDirection
{
    Input,
    Output
}

public sealed class EntityDescriptor
{
    public EntityDescriptor(string systemName, string address, EntityDirection direction, bool defaultValue)
    {
        SystemName = systemName;
        Address = address;
        Direction = direction;
        DefaultValue = defaultValue;
    }

    public string SystemName { get; }

    public string Address { get; }

    public EntityDirection Direction { get; }

    public bool DefaultValue { get; }
}

public sealed class EntityRegistry
{
    private readonly Dictionary<string, EntityDescriptor> _entities =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly object _sync = new();

    public EntityRegistry(IEnumerable<IoPointConfig> points)
    {
        Reload(points);
    }

    public IReadOnlyCollection<EntityDescriptor> All
    {
        get
        {
            lock (_sync)
            {
                return _entities.Values.ToList();
            }
        }
    }

    public bool TryGet(string systemName, out EntityDescriptor descriptor)
    {
        lock (_sync)
        {
            return _entities.TryGetValue(systemName, out descriptor!);
        }
    }

    public void Reload(IEnumerable<IoPointConfig> points)
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
                    : $"{hardwareName}.io.{id}";

                var direction = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase)
                    ? EntityDirection.Output
                    : EntityDirection.Input;

                var address = IoPointConfig.ResolveAddress(point);
                if (_entities.ContainsKey(systemName))
                {
                    throw new InvalidOperationException($"Duplicate entity systemName '{systemName}'");
                }
                _entities[systemName] = new EntityDescriptor(systemName, address, direction, point.DefaultValue);
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
