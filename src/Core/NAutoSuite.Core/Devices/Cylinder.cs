using NAutoSuite.Core.Machine;
using NAutoSuite.Core.IO;
using Serilog;

namespace NAutoSuite.Core.Devices;

/// <summary>
/// Simple pneumatic cylinder abstraction using cached IO.
/// </summary>
public class Cylinder
{
    private readonly MachineIO _io;
    private readonly SensorWaiter _waiter;
    private readonly ILogger _logger;
    private readonly string _extendOutput;
    private readonly string _extendedSensor;
    private readonly string _retractedSensor;

    public Cylinder(
        MachineIO io,
        string extendOutput,
        string extendedSensor,
        string retractedSensor,
        ILogger? logger = null)
    {
        _io = io ?? throw new ArgumentNullException(nameof(io));
        _extendOutput = extendOutput;
        _extendedSensor = extendedSensor;
        _retractedSensor = retractedSensor;
        _logger = logger ?? Log.Logger;
        _waiter = new SensorWaiter(_logger);
    }

    public bool IsExtended => _io.ReadInput(_extendedSensor);

    public bool IsRetracted => _io.ReadInput(_retractedSensor);

    public void SetExtend(bool value)
    {
        _io.WriteOutput(_extendOutput, value);
    }

    public async Task<bool> ExtendAsync(int timeoutMs = 1000, CancellationToken ct = default)
    {
        SetExtend(true);
        return await _waiter.WaitForSensorAsync(
            "CylinderExtended",
            () => Task.FromResult(_io.ReadInput(_extendedSensor)),
            timeoutMs,
            ct);
    }

    public async Task<bool> RetractAsync(int timeoutMs = 1000, CancellationToken ct = default)
    {
        SetExtend(false);
        return await _waiter.WaitForSensorAsync(
            "CylinderRetracted",
            () => Task.FromResult(_io.ReadInput(_retractedSensor)),
            timeoutMs,
            ct);
    }
}
