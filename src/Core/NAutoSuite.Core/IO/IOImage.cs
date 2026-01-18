using System.Collections.Concurrent;

namespace NAutoSuite.Core.IO;

/// <summary>
/// In-memory IO image updated by the scan cycle.
/// </summary>
public class IOImage
{
    private readonly ConcurrentDictionary<string, bool> _inputs =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, bool> _outputs =
        new(StringComparer.OrdinalIgnoreCase);

    public void SetInput(string address, bool value)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        _inputs[address] = value;
    }

    public bool GetInput(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;
        return _inputs.TryGetValue(address, out var value) && value;
    }

    public void SetOutput(string address, bool value)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        _outputs[address] = value;
    }

    public bool GetOutput(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;
        return _outputs.TryGetValue(address, out var value) && value;
    }

    public IReadOnlyDictionary<string, bool> Inputs => _inputs;

    public IReadOnlyDictionary<string, bool> Outputs => _outputs;

    public void EnsureInputs(IEnumerable<string> addresses)
    {
        foreach (var address in addresses)
        {
            if (string.IsNullOrWhiteSpace(address)) continue;
            _inputs.TryAdd(address, false);
        }
    }

    public void EnsureOutputs(IEnumerable<string> addresses)
    {
        foreach (var address in addresses)
        {
            if (string.IsNullOrWhiteSpace(address)) continue;
            _outputs.TryAdd(address, false);
        }
    }
}
