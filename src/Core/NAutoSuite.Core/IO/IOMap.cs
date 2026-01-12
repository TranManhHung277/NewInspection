namespace NAutoSuite.Core.IO;

/// <summary>
/// Base class for IO mapping
/// Each machine defines its own IO map by inheriting this
/// </summary>
public abstract class IOMap
{
    /// <summary>
    /// Machine-specific inputs
    /// </summary>
    public InputMap Inputs { get; protected set; } = new();

    /// <summary>
    /// Machine-specific outputs
    /// </summary>
    public OutputMap Outputs { get; protected set; } = new();

    /// <summary>
    /// Common inputs (EMG, Start, Stop, Reset buttons)
    /// </summary>
    public CommonInputMap CommonInputs { get; } = new();

    /// <summary>
    /// Common outputs (Tower light, buzzer, etc.)
    /// </summary>
    public CommonOutputMap CommonOutputs { get; } = new();
}

/// <summary>
/// Common inputs present in all machines
/// </summary>
public class CommonInputMap
{
    /// <summary>
    /// Emergency stop button
    /// </summary>
    public string EmergencyStop { get; set; } = "IX0.0";

    /// <summary>
    /// Start button
    /// </summary>
    public string StartButton { get; set; } = "IX0.1";

    /// <summary>
    /// Stop button
    /// </summary>
    public string StopButton { get; set; } = "IX0.2";

    /// <summary>
    /// Reset button
    /// </summary>
    public string ResetButton { get; set; } = "IX0.3";

    /// <summary>
    /// Safety door sensor
    /// </summary>
    public string SafetyDoor { get; set; } = "IX0.4";

    /// <summary>
    /// Air pressure sensor
    /// </summary>
    public string AirPressure { get; set; } = "IX0.5";
}

/// <summary>
/// Common outputs present in all machines
/// </summary>
public class CommonOutputMap
{
    /// <summary>
    /// Tower light - Red (Error/EMG)
    /// </summary>
    public string TowerLightRed { get; set; } = "QX0.0";

    /// <summary>
    /// Tower light - Yellow (Warning)
    /// </summary>
    public string TowerLightYellow { get; set; } = "QX0.1";

    /// <summary>
    /// Tower light - Green (Running)
    /// </summary>
    public string TowerLightGreen { get; set; } = "QX0.2";

    /// <summary>
    /// Buzzer for alarms
    /// </summary>
    public string Buzzer { get; set; } = "QX0.3";

    /// <summary>
    /// Main power enable
    /// </summary>
    public string MainPowerEnable { get; set; } = "QX0.4";
}

/// <summary>
/// Machine-specific inputs (override this)
/// </summary>
public class InputMap
{
    // Override in machine-specific map
}

/// <summary>
/// Machine-specific outputs (override this)
/// </summary>
public class OutputMap
{
    // Override in machine-specific map
}
