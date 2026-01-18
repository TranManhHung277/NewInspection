using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.IO;

namespace PickAndPlace.Machine;

/// <summary>
/// Project profile: IO map, register map, and axis definitions.
/// </summary>
public class PickAndPlaceProfile : MachineProfile
{
    public PickAndPlaceProfile()
        : base(CreateIoMapWithCommon(), CreateRegisterMap(), CreateAxes())
    {
    }

    private static IOMap CreateIoMapWithCommon()
    {
        var map = new PickAndPlaceIOMap();
        map.CommonInputs.EmergencyStop = "IX0.0";
        map.CommonInputs.StartButton = "IX0.1";
        map.CommonInputs.StopButton = "IX0.2";
        map.CommonInputs.ResetButton = "IX0.3";
        map.CommonInputs.SafetyDoor = "IX0.4";
        map.CommonInputs.AirPressure = "IX0.5";
        map.CommonInputs.HomeButton = "IX0.6";
        map.CommonInputs.AutoModeSwitch = "IX0.7";
        map.CommonInputs.ManualModeSwitch = "IX0.8";
        map.CommonInputs.MaterialLow = "IX0.9";

        map.CommonOutputs.TowerLightRed = "QX0.0";
        map.CommonOutputs.TowerLightYellow = "QX0.1";
        map.CommonOutputs.TowerLightGreen = "QX0.2";
        map.CommonOutputs.Buzzer = "QX0.3";
        map.CommonOutputs.MainPowerEnable = "QX0.4";
        return map;
    }

    private static RegisterMap CreateRegisterMap()
    {
        var map = new RegisterMap();
        // Example registers (extend as needed)
        map.AddInput("PRODUCT_COUNT", "R1000");
        map.AddInput("LAST_RESULT", "R1001");
        map.AddOutput("TARGET_RECIPE", "R2000");
        map.AddOutput("START_BATCH", "R2001");
        return map;
    }

    private static IReadOnlyList<AxisDefinition> CreateAxes()
    {
        return new List<AxisDefinition>
        {
            new() { Id = "AXIS_X", Name = "AxisX", Description = "Pick X axis" },
            new() { Id = "AXIS_Y", Name = "AxisY", Description = "Pick Y axis" },
            new() { Id = "AXIS_Z", Name = "AxisZ", Description = "Pick Z axis" }
        };
    }
}
