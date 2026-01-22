using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Core.Entities;

public sealed class EntityStateService
{
    private readonly IReadOnlyList<IIO> _ioDevices;
    private readonly EntityRegistry _registry;
    private readonly EntityEventBus _bus;
    private readonly ILogger _logger;
    private readonly Dictionary<string, bool> _lastStates = new(StringComparer.OrdinalIgnoreCase);

    public EntityStateService(IEnumerable<IIO> ioDevices, EntityRegistry registry, EntityEventBus bus, ILogger? logger = null)
    {
        _ioDevices = ioDevices?.ToList() ?? throw new ArgumentNullException(nameof(ioDevices));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger ?? Log.Logger;
    }

    public async Task UpdateAsync(CancellationToken ct = default)
    {
        foreach (var entity in _registry.All)
        {
            if (entity.Direction != EntityDirection.Input)
            {
                continue;
            }

            var result = await ReadInputAsync(entity.Address, ct);
            var value = result.IsSuccess ? result.Value : entity.DefaultValue;
            if (_lastStates.TryGetValue(entity.SystemName, out var lastValue) && lastValue == value)
            {
                continue;
            }

            _lastStates[entity.SystemName] = value;
            _bus.PublishStateChanged(new EntityStateChangedEventArgs(entity.SystemName, value, entity.Direction, DateTime.UtcNow));
        }
    }

    private async Task<Result<bool>> ReadInputAsync(string address, CancellationToken ct)
    {
        foreach (var io in _ioDevices)
        {
            var result = await io.ReadInputAsync(address, ct);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        _logger.Debug("No IO device available for input {Address}", address);
        return Result.Failure<bool>($"No IO device available for input {address}");
    }
}
