namespace NAutoApp.Services;

public sealed class NullSimulatedIO : ISimulatedIO
{
    public void SetInput(string address, bool value)
    {
        // No-op for real hardware mode.
    }
}

