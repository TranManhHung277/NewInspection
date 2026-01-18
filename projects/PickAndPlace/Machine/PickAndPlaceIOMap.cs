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
    public string PartPresent { get; set; } = "IX1.0";
    public string TestOk { get; set; } = "IX1.1";
    public string TestNg { get; set; } = "IX1.2";
    public string CylinderExtended { get; set; } = "IX1.3";
    public string CylinderRetracted { get; set; } = "IX1.4";
    public string VacuumOk { get; set; } = "IX1.5";
}

public class PickAndPlaceOutputs : OutputMap
{
    public string CylinderExtend { get; set; } = "QX1.0";
    public string VacuumOn { get; set; } = "QX1.1";
}
