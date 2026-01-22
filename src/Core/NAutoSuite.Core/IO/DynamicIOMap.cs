using System.Reflection;

namespace NAutoSuite.Core.IO;

public interface IDynamicIOMap
{
    IReadOnlyCollection<string> InputAddresses { get; }
    IReadOnlyCollection<string> OutputAddresses { get; }
}

public sealed class DynamicIOMap : IOMap, IDynamicIOMap
{
    private readonly HashSet<string> _inputs = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _outputs = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyCollection<string> InputAddresses => _inputs;

    public IReadOnlyCollection<string> OutputAddresses => _outputs;

    public void Reset()
    {
        _inputs.Clear();
        _outputs.Clear();
        ResetStringProperties(CommonInputs);
        ResetStringProperties(CommonOutputs);
    }

    public void AddInput(string address)
    {
        if (!string.IsNullOrWhiteSpace(address))
        {
            _inputs.Add(address);
        }
    }

    public void AddOutput(string address)
    {
        if (!string.IsNullOrWhiteSpace(address))
        {
            _outputs.Add(address);
        }
    }

    public bool TrySetCommonInput(string name, string address)
        => TrySetAddress(CommonInputs, name, address);

    public bool TrySetCommonOutput(string name, string address)
        => TrySetAddress(CommonOutputs, name, address);

    private static bool TrySetAddress(object target, string name, string address)
    {
        var prop = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (prop == null || prop.PropertyType != typeof(string))
        {
            return false;
        }

        prop.SetValue(target, address);
        return true;
    }

    private static void ResetStringProperties(object target)
    {
        var props = target.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            if (prop.PropertyType == typeof(string) && prop.CanWrite)
            {
                prop.SetValue(target, string.Empty);
            }
        }
    }
}
