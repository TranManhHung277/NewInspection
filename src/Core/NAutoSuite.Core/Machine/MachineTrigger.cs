namespace NAutoSuite.Core.Machine;

/// <summary>
/// Standard machine triggers
/// </summary>
public enum MachineTrigger
{
    Initialize,
    Start,
    Stop,
    Error,
    Pause,
    Resume,
    Reset,
    EmergencyStop,
    ClearError,
    Complete,
    CancelAuto
}
