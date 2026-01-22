using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Core.Entities;

public sealed class DataStateService
{
    private readonly IReadOnlyList<IRegisterIO> _registerDevices;
    private readonly DataEntityRegistry _registry;
    private readonly DataEventBus _bus;
    private readonly ILogger _logger;
    private readonly Dictionary<string, double> _lastValues = new(StringComparer.OrdinalIgnoreCase);

    public DataStateService(IEnumerable<IRegisterIO> registerDevices, DataEntityRegistry registry, DataEventBus bus, ILogger? logger = null)
    {
        _registerDevices = registerDevices?.ToList() ?? throw new ArgumentNullException(nameof(registerDevices));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger ?? Log.Logger;
    }

    public async Task UpdateAsync(CancellationToken ct = default)
    {
        foreach (var entity in _registry.All)
        {
            var result = await ReadRegisterAsync(entity.Address, ct);
            var value = result.IsSuccess ? result.Value : entity.DefaultValue;
            if (_lastValues.TryGetValue(entity.SystemName, out var lastValue) && Math.Abs(lastValue - value) < 0.0001)
            {
                continue;
            }

            _lastValues[entity.SystemName] = value;
            _bus.PublishDataChanged(new DataChangedEventArgs(entity.SystemName, value, entity.DataType, DateTime.UtcNow));
        }
    }

    private async Task<Result<double>> ReadRegisterAsync(string address, CancellationToken ct)
    {
        foreach (var io in _registerDevices)
        {
            var result = await io.ReadRegisterAsync(address, ct);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        _logger.Debug("No register device available for address {Address}", address);
        return Result.Failure<double>($"No register device available for {address}");
    }
}
