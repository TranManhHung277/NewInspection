namespace PickAndPlace.Services;

/// <summary>
/// Machine settings that persist across sessions
/// </summary>
public class MachineSettings
{
    public int MachineNumber { get; set; } = 1;
    public string ProjectName { get; set; } = "Pick & Place Machine (EtherCAT)";
}
