namespace PickAndPlace.Configuration;

public sealed class HardwareProfileConfig
{
    public List<AxisConfig> Axes { get; } = new();
    public List<IoPointConfig> IoPoints { get; } = new();
    public RegisterConfig Registers { get; } = new();

    public void AddLeadshine(LeadshineHardwareConfig config)
    {
        Axes.AddRange(config.Axes);
        IoPoints.AddRange(config.IoPoints);
        MergeRegisters(Registers.Inputs, config.Registers.Inputs);
        MergeRegisters(Registers.Outputs, config.Registers.Outputs);
    }

    public void AddKeyence(KeyencePlcSettings config)
    {
        IoPoints.AddRange(config.IoPoints);
        MergeRegisters(Registers.Inputs, config.Registers.Inputs);
        MergeRegisters(Registers.Outputs, config.Registers.Outputs);
    }

    private static void MergeRegisters(Dictionary<string, string> target, Dictionary<string, string> source)
    {
        foreach (var entry in source)
        {
            if (target.ContainsKey(entry.Key))
            {
                throw new InvalidOperationException($"Duplicate register key '{entry.Key}' across hardware");
            }
            target[entry.Key] = entry.Value;
        }
    }
}
