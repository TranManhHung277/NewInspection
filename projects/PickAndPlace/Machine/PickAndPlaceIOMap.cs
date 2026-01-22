using NAutoSuite.Core.IO;

namespace PickAndPlace.Machine;

public class PickAndPlaceIOMap : IOMap
{
    public PickAndPlaceInputs MachineInputs => (PickAndPlaceInputs)Inputs;
    public PickAndPlaceOutputs MachineOutputs => (PickAndPlaceOutputs)Outputs;

    public PickAndPlaceIOMap()
    {
        Inputs = new PickAndPlaceInputs();
        Outputs = new PickAndPlaceOutputs();
    }
}

public class PickAndPlaceInputs : InputMap
{
    public string PartPresent { get; set; } = string.Empty;
    public string TestOk { get; set; } = string.Empty;
    public string TestNg { get; set; } = string.Empty;
    public string CylinderExtended { get; set; } = string.Empty;
    public string CylinderRetracted { get; set; } = string.Empty;
    public string VacuumOk { get; set; } = string.Empty;
}

public class PickAndPlaceOutputs : OutputMap
{
    public string CylinderExtend { get; set; } = string.Empty;
    public string VacuumOn { get; set; } = string.Empty;
}
