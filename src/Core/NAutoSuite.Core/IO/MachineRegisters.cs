namespace NAutoSuite.Core.IO;

/// <summary>
/// Access to the cached register image for machine logic.
/// </summary>
public class MachineRegisters
{
    private readonly RegisterImage _image;

    public MachineRegisters(RegisterImage image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
    }

    public double ReadInput(string address) => _image.GetInput(address);

    public double ReadOutput(string address) => _image.GetOutput(address);

    public void WriteOutput(string address, double value) => _image.SetOutput(address, value);
}
