using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Hardware;
using NAutoSuite.Core.Machine;
using Serilog;

namespace NAutoSuite.App.Template.Machine;

/// <summary>
/// Template machine implementation - customize for your needs
/// </summary>
public class TemplateMachine : MachineBase
{
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly ILogger _machineLogger;

    public TemplateMachine(
        string id,
        string name,
        IAxis? axisX = null,
        IAxis? axisY = null,
        ILogger? logger = null)
        : base(id, name, null, logger)
    {
        _axisX = axisX;
        _axisY = axisY;
        _machineLogger = logger ?? Log.Logger;
    }

    protected override void OnRegisterHardware(HardwareManager hardwareManager)
    {
        if (_axisX != null)
        {
            hardwareManager.RegisterDevice(_axisX);
        }

        if (_axisY != null)
        {
            hardwareManager.RegisterDevice(_axisY);
        }
    }

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        _machineLogger.Information("Starting production cycle {CycleCount}", Context.CycleCount + 1);

        // TODO: Implement your machine cycle here
        await Task.Delay(1000);

        _machineLogger.Information("Cycle {CycleCount} completed", Context.CycleCount + 1);
    }

    public async Task<Result> HomeAllAxesAsync()
    {
        _machineLogger.Information("Homing all axes...");

        if (_axisX != null)
        {
            var result = await _axisX.HomeAsync();
            if (!result.IsSuccess)
                return result;
        }

        if (_axisY != null)
        {
            var result = await _axisY.HomeAsync();
            if (!result.IsSuccess)
                return result;
        }

        return Result.Success("All axes homed");
    }
}
