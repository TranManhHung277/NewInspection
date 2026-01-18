using System.Reflection;

namespace NAutoSuite.Core.IO;

public static class IOMapExtensions
{
    public static IReadOnlyList<string> GetInputAddresses(this IOMap ioMap)
    {
        var addresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAddresses(addresses, ioMap.CommonInputs);
        AddAddresses(addresses, ioMap.Inputs);
        return addresses.ToList();
    }

    public static IReadOnlyList<string> GetOutputAddresses(this IOMap ioMap)
    {
        var addresses = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        AddAddresses(addresses, ioMap.CommonOutputs);
        AddAddresses(addresses, ioMap.Outputs);
        return addresses.ToList();
    }

    private static void AddAddresses(HashSet<string> addresses, object map)
    {
        if (map == null) return;

        var props = map.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            if (prop.PropertyType != typeof(string)) continue;
            if (prop.GetValue(map) is not string value) continue;
            if (string.IsNullOrWhiteSpace(value)) continue;
            addresses.Add(value);
        }
    }
}
