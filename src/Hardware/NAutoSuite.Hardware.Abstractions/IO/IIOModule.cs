using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;

namespace NAutoSuite.Hardware.Abstractions.IO;

/// <summary>
/// Digital IO module interface
/// </summary>
public interface IIOModule : IDevice
{
    /// <summary>
    /// Number of input channels
    /// </summary>
    int InputCount { get; }

    /// <summary>
    /// Number of output channels
    /// </summary>
    int OutputCount { get; }

    /// <summary>
    /// Get input by index
    /// </summary>
    IInput? GetInput(int index);

    /// <summary>
    /// Get output by index
    /// </summary>
    IOutput? GetOutput(int index);

    /// <summary>
    /// Read all inputs
    /// </summary>
    Task<Result<bool[]>> ReadAllInputsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Read all outputs
    /// </summary>
    Task<Result<bool[]>> ReadAllOutputsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Write all outputs
    /// </summary>
    Task<Result> WriteAllOutputsAsync(bool[] values, CancellationToken cancellationToken = default);
}
