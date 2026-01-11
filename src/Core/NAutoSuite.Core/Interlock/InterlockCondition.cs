namespace NAutoSuite.Core.Interlock;

/// <summary>
/// Represents a condition that must be met before machine operations
/// </summary>
public class InterlockCondition
{
    /// <summary>
    /// Unique identifier for the condition
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable description of the condition
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Function that evaluates the condition (returns true if satisfied)
    /// </summary>
    public Func<bool> Condition { get; set; } = () => true;

    /// <summary>
    /// Whether this condition must be satisfied (true) or is optional (false)
    /// </summary>
    public bool IsRequired { get; set; } = true;
}
