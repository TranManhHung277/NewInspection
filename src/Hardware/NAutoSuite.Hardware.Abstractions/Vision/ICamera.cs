using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;

namespace NAutoSuite.Hardware.Abstractions.Vision;

/// <summary>
/// Camera interface for machine vision
/// </summary>
public interface ICamera : IDevice
{
    /// <summary>
    /// Camera resolution width
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Camera resolution height
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Is camera streaming
    /// </summary>
    bool IsStreaming { get; }

    /// <summary>
    /// Current exposure time (microseconds)
    /// </summary>
    double ExposureTime { get; set; }

    /// <summary>
    /// Current gain
    /// </summary>
    double Gain { get; set; }

    /// <summary>
    /// Start continuous acquisition
    /// </summary>
    Task<Result> StartStreamingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stop continuous acquisition
    /// </summary>
    Task<Result> StopStreamingAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Grab single image
    /// </summary>
    Task<Result<byte[]>> GrabImageAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Trigger software trigger
    /// </summary>
    Task<Result> TriggerAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Image received event
    /// </summary>
    event EventHandler<byte[]>? ImageReceived;
}
