using System.Collections.Concurrent;

namespace NAutoSuite.Core.IO;

/// <summary>
/// In-memory register image updated by the scan cycle.
/// </summary>
public class RegisterImage
{
    private readonly ConcurrentDictionary<string, double> _inputs =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, double> _outputs =
        new(StringComparer.OrdinalIgnoreCase);

    public void SetInput(string address, double value)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        _inputs[address] = value;
    }

    public double GetInput(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return 0;
        return _inputs.TryGetValue(address, out var value) ? value : 0;
    }

    public void SetOutput(string address, double value)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        _outputs[address] = value;
    }

    public double GetOutput(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return 0;
        return _outputs.TryGetValue(address, out var value) ? value : 0;
    }

    public IReadOnlyDictionary<string, double> Inputs => _inputs;

    public IReadOnlyDictionary<string, double> Outputs => _outputs;

    public void EnsureInputs(IEnumerable<string> addresses)
    {
        foreach (var address in addresses)
        {
            if (string.IsNullOrWhiteSpace(address)) continue;
            _inputs.TryAdd(address, 0);
        }
    }

    public void EnsureOutputs(IEnumerable<string> addresses)
    {
        foreach (var address in addresses)
        {
            if (string.IsNullOrWhiteSpace(address)) continue;
            _outputs.TryAdd(address, 0);
        }
    }
}
