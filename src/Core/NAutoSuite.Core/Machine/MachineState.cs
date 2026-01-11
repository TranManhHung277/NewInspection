namespace NAutoSuite.Core.Machine;

/// <summary>
/// Standard machine states
/// </summary>
public enum MachineState
{
    /// <summary>
    /// Machine is not initialized
    /// </summary>
    Uninitialized,

    /// <summary>
    /// Machine is initializing
    /// </summary>
    Initializing,

    /// <summary>
    /// Machine is idle and ready
    /// </summary>
    Idle,

    /// <summary>
    /// Machine is running
    /// </summary>
    Running,

    /// <summary>
    /// Machine is paused
    /// </summary>
    Paused,

    /// <summary>
    /// Machine is stopping
    /// </summary>
    Stopping,

    /// <summary>
    /// Machine is stopped
    /// </summary>
    Stopped,

    /// <summary>
    /// Machine is in error state
    /// </summary>
    Error,

    /// <summary>
    /// Machine is in emergency stop
    /// </summary>
    EmergencyStop
}
