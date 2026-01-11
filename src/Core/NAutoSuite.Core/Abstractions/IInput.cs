namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Digital input interface
/// </summary>
public interface IInput : IDevice
{
    /// <summary>
    /// Current input state
    /// </summary>
    bool Value { get; }

    /// <summary>
    /// Read input value
    /// </summary>
    Task<bool> ReadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event fired when input changes
    /// </summary>
    event EventHandler<bool>? ValueChanged;
}
