using Serilog;

namespace NAutoSuite.Core.Interlock;

/// <summary>
/// Manages interlock conditions that must be satisfied before machine operations
/// </summary>
public class InterlockManager
{
    private readonly List<InterlockCondition> _conditions = new();
    private readonly ILogger _logger;

    public InterlockManager(ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
    }

    /// <summary>
    /// Adds an interlock condition
    /// </summary>
    public void AddCondition(InterlockCondition condition)
    {
        if (string.IsNullOrEmpty(condition.Id))
            throw new ArgumentException("Condition ID cannot be empty", nameof(condition));

        // Remove existing condition with same ID
        _conditions.RemoveAll(c => c.Id == condition.Id);
        _conditions.Add(condition);

        _logger.Debug("Interlock condition added: {Id} - {Description}", condition.Id, condition.Description);
    }

    /// <summary>
    /// Removes an interlock condition by ID
    /// </summary>
    public void RemoveCondition(string id)
    {
        var removed = _conditions.RemoveAll(c => c.Id == id);
        if (removed > 0)
        {
            _logger.Debug("Interlock condition removed: {Id}", id);
        }
    }

    /// <summary>
    /// Removes all interlock conditions
    /// </summary>
    public void ClearAll()
    {
        _conditions.Clear();
        _logger.Debug("All interlock conditions cleared");
    }

    /// <summary>
    /// Checks all required interlock conditions
    /// </summary>
    /// <returns>Tuple of (CanStart, List of failed condition descriptions)</returns>
    public (bool CanStart, List<string> FailedConditions) CheckStartConditions()
    {
        var failed = new List<string>();

        foreach (var condition in _conditions.Where(c => c.IsRequired))
        {
            try
            {
                if (!condition.Condition())
                {
                    failed.Add(condition.Description);
                    _logger.Warning("Interlock failed: {Id} - {Description}", condition.Id, condition.Description);
                }
            }
            catch (Exception ex)
            {
                failed.Add($"{condition.Description} (Error: {ex.Message})");
                _logger.Error(ex, "Error evaluating interlock condition: {Id}", condition.Id);
            }
        }

        return (failed.Count == 0, failed);
    }

    /// <summary>
    /// Gets all registered conditions
    /// </summary>
    public IReadOnlyList<InterlockCondition> GetConditions() => _conditions.AsReadOnly();
}
