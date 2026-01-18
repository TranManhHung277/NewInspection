using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;

namespace NAutoSuite.Core.Hardware;

/// <summary>
/// Central registry for hardware connections.
/// </summary>
public class HardwareManager
{
    private readonly List<IDevice> _devices = new();
    private readonly List<IIO> _ioDevices = new();
    private readonly List<IHardwareDataProvider> _dataProviders = new();

    public IReadOnlyList<IDevice> Devices => _devices.AsReadOnly();

    public IReadOnlyList<IIO> IODevices => _ioDevices.AsReadOnly();

    public IReadOnlyList<IHardwareDataProvider> DataProviders => _dataProviders.AsReadOnly();

    public void RegisterDevice(IDevice device)
    {
        if (device == null) return;
        if (_devices.Contains(device)) return;
        _devices.Add(device);

        if (device is IIO ioDevice)
        {
            if (!_ioDevices.Contains(ioDevice))
            {
                _ioDevices.Add(ioDevice);
            }
        }

        if (device is IHardwareDataProvider provider)
        {
            RegisterDataProvider(provider);
        }
    }

    public void RegisterIO(IIO ioDevice)
    {
        if (ioDevice == null) return;
        RegisterDevice(ioDevice);
    }

    public void RegisterDataProvider(IHardwareDataProvider provider)
    {
        if (provider == null) return;
        if (_dataProviders.Contains(provider)) return;
        _dataProviders.Add(provider);
    }

    public async Task UpdateDataAsync(CancellationToken ct = default)
    {
        foreach (var provider in _dataProviders)
        {
            await provider.UpdateAsync(ct);
        }
    }

    public async Task<Result> ConnectAllAsync(CancellationToken ct = default)
    {
        foreach (var device in _devices)
        {
            if (device.IsConnected) continue;
            var result = await device.ConnectAsync(ct);
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Success("All devices connected");
    }

    public async Task<Result> DisconnectAllAsync(CancellationToken ct = default)
    {
        foreach (var device in _devices)
        {
            if (!device.IsConnected) continue;
            await device.DisconnectAsync(ct);
        }

        return Result.Success("All devices disconnected");
    }
}
