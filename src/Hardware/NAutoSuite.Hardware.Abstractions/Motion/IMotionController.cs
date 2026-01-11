using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;

namespace NAutoSuite.Hardware.Abstractions.Motion;

/// <summary>
/// Motion controller interface
/// </summary>
public interface IMotionController : IDevice
{
    /// <summary>
    /// Number of axes supported
    /// </summary>
    int AxisCount { get; }

    /// <summary>
    /// Get axis by index
    /// </summary>
    IAxis? GetAxis(int index);

    /// <summary>
    /// Initialize all axes
    /// </summary>
    Task<Result> InitializeAxesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Home all axes
    /// </summary>
    Task<Result> HomeAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop all axes
    /// </summary>
    Task<Result> StopAllAsync(bool emergency = false, CancellationToken cancellationToken = default);
}
