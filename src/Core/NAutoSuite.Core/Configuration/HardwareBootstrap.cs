using NAutoSuite.Core.Abstractions;

namespace NAutoSuite.Core.Configuration;

public sealed class HardwareBootstrap
{
    public HardwareBootstrap(IReadOnlyList<IDevice> devices, HardwareProfileConfig profile)
    {
        Devices = devices;
        Profile = profile;
    }

    public IReadOnlyList<IDevice> Devices { get; }

    public HardwareProfileConfig Profile { get; }
}
