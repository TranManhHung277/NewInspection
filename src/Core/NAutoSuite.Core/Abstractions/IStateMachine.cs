namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// State machine interface
/// </summary>
public interface IStateMachine<TState, TTrigger>
    where TState : Enum
    where TTrigger : Enum
{
    /// <summary>
    /// Current state
    /// </summary>
    TState State { get; }

    /// <summary>
    /// Fire a trigger
    /// </summary>
    Task FireAsync(TTrigger trigger);

    /// <summary>
    /// Check if trigger can be fired
    /// </summary>
    bool CanFire(TTrigger trigger);

    /// <summary>
    /// Get permitted triggers for current state
    /// </summary>
    IEnumerable<TTrigger> PermittedTriggers { get; }
}
