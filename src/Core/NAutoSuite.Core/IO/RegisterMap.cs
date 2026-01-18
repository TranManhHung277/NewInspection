namespace NAutoSuite.Core.IO;

/// <summary>
/// Central registry for register addresses (read/write).
/// </summary>
public class RegisterMap
{
    public Dictionary<string, string> InputRegisters { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> OutputRegisters { get; } =
        new(StringComparer.OrdinalIgnoreCase);

    public void AddInput(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address)) return;
        InputRegisters[name] = address;
    }

    public void AddOutput(string name, string address)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address)) return;
        OutputRegisters[name] = address;
    }
}
