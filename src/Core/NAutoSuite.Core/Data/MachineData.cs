namespace NAutoSuite.Core.Data;

/// <summary>
/// Base class for machine data/settings
/// Each machine defines its own data by inheriting this
/// </summary>
public abstract class MachineData
{
    /// <summary>
    /// Common settings for all machines
    /// </summary>
    public CommonSettings Common { get; set; } = new();

    /// <summary>
    /// Load data from file (JSON, XML, etc.)
    /// </summary>
    public abstract void Load(string filePath);

    /// <summary>
    /// Save data to file
    /// </summary>
    public abstract void Save(string filePath);
}

/// <summary>
/// Common settings present in all machines
/// </summary>
public class CommonSettings
{
    /// <summary>
    /// Machine ID
    /// </summary>
    public string MachineId { get; set; } = "M001";

    /// <summary>
    /// Machine name
    /// </summary>
    public string MachineName { get; set; } = "Automation Machine";

    /// <summary>
    /// Cycle timeout (seconds)
    /// </summary>
    public double CycleTimeout { get; set; } = 60.0;

    /// <summary>
    /// Auto restart after alarm clear
    /// </summary>
    public bool AutoRestartAfterAlarm { get; set; } = false;

    /// <summary>
    /// Enable buzzer on alarm
    /// </summary>
    public bool BuzzerOnAlarm { get; set; } = true;

    /// <summary>
    /// EMG reset timeout (seconds)
    /// </summary>
    public double EmergencyResetTimeout { get; set; } = 5.0;
}

/// <summary>
/// Position data (for motion)
/// </summary>
public class PositionData
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public PositionData() { }

    public PositionData(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override string ToString() => $"X:{X:F2}, Y:{Y:F2}, Z:{Z:F2}";
}

/// <summary>
/// Speed data (for motion)
/// </summary>
public class SpeedData
{
    public double Velocity { get; set; }
    public double Acceleration { get; set; }
    public double Deceleration { get; set; }

    public SpeedData() { }

    public SpeedData(double vel, double acc, double dec)
    {
        Velocity = vel;
        Acceleration = acc;
        Deceleration = dec;
    }
}
