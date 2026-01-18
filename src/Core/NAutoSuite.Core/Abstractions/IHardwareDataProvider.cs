namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Optional interface for hardware data providers that need cyclic updates.
/// </summary>
public interface IHardwareDataProvider
{
    Task UpdateAsync(CancellationToken cancellationToken = default);
}
