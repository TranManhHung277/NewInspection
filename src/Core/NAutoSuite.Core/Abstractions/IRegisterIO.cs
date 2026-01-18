using NAutoSuite.Core.Common;

namespace NAutoSuite.Core.Abstractions;

/// <summary>
/// Register IO abstraction for reading/writing register values.
/// </summary>
public interface IRegisterIO : IDevice
{
    Task<Result<double>> ReadRegisterAsync(string address, CancellationToken cancellationToken = default);

    Task<Result> WriteRegisterAsync(string address, double value, CancellationToken cancellationToken = default);
}
