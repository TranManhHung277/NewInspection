namespace NAutoSuite.Core.IO;

/// <summary>
/// Access to the cached IO image for machine logic.
/// </summary>
public class MachineIO
{
    private readonly IOImage _image;

    public MachineIO(IOImage image)
    {
        _image = image ?? throw new ArgumentNullException(nameof(image));
    }

    public bool ReadInput(string address) => _image.GetInput(address);

    public bool ReadOutput(string address) => _image.GetOutput(address);

    public void WriteOutput(string address, bool value) => _image.SetOutput(address, value);
}
