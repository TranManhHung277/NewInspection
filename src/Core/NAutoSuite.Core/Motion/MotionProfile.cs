namespace NAutoSuite.Core.Motion;

/// <summary>
/// Motion profile parameters
/// </summary>
public class MotionProfile
{
    /// <summary>
    /// Maximum velocity (units/sec)
    /// </summary>
    public double MaxVelocity { get; set; }

    /// <summary>
    /// Acceleration (units/sec²)
    /// </summary>
    public double Acceleration { get; set; }

    /// <summary>
    /// Deceleration (units/sec²)
    /// </summary>
    public double Deceleration { get; set; }

    /// <summary>
    /// Jerk (units/sec³)
    /// </summary>
    public double Jerk { get; set; }

    /// <summary>
    /// Create default profile
    /// </summary>
    public static MotionProfile Default => new()
    {
        MaxVelocity = 100,
        Acceleration = 1000,
        Deceleration = 1000,
        Jerk = 10000
    };

    /// <summary>
    /// Validate profile parameters
    /// </summary>
    public bool IsValid()
    {
        return MaxVelocity > 0 &&
               Acceleration > 0 &&
               Deceleration > 0 &&
               Jerk > 0;
    }
}
