using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Configuration;

namespace PickAndPlace.Configuration;

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
