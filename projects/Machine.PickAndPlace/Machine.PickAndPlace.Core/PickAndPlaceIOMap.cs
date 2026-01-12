using NAutoSuite.Core.IO;

namespace Machine.PickAndPlace.Core;

/// <summary>
/// IO Map for Pick and Place machine
/// This class ONLY defines machine-specific IOs
/// Common IOs (EMG, Start, Stop, etc.) are already in IOMap base class
/// </summary>
public class PickAndPlaceIOMap : IOMap
{
    public PickAndPlaceIOMap()
    {
        // Initialize machine-specific inputs and outputs
        Inputs = new PickAndPlaceInputMap();
        Outputs = new PickAndPlaceOutputMap();

        // Override common IO addresses if needed (optional)
        // CommonInputs.EmergencyStop = "IX0.0";  // Already default
        // CommonInputs.StartButton = "IX0.1";     // Already default
        // etc.
    }
}

/// <summary>
/// Machine-specific INPUTS for Pick and Place
/// </summary>
public class PickAndPlaceInputMap : InputMap
{
    // Sensors
    public string PartSensorAtPick { get; set; } = "IX1.0";
    public string PartSensorAtPlace { get; set; } = "IX1.1";
    public string VacuumSensor { get; set; } = "IX1.2";

    // Axis limit switches (if using hardware limits)
    public string AxisXHomeSensor { get; set; } = "IX2.0";
    public string AxisYHomeSensor { get; set; } = "IX2.1";
    public string AxisZHomeSensor { get; set; } = "IX2.2";

    public string AxisXPositiveLimit { get; set; } = "IX2.3";
    public string AxisXNegativeLimit { get; set; } = "IX2.4";
    public string AxisYPositiveLimit { get; set; } = "IX2.5";
    public string AxisYNegativeLimit { get; set; } = "IX2.6";
    public string AxisZPositiveLimit { get; set; } = "IX2.7";
    public string AxisZNegativeLimit { get; set; } = "IX3.0";
}

/// <summary>
/// Machine-specific OUTPUTS for Pick and Place
/// </summary>
public class PickAndPlaceOutputMap : OutputMap
{
    // Vacuum system
    public string VacuumValve { get; set; } = "QX1.0";
    public string BlowOffValve { get; set; } = "QX1.1";

    // Optional: Lighting, etc.
    public string WorkLight { get; set; } = "QX1.2";
}
