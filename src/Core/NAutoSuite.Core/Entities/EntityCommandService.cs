using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Core.Entities;

public sealed class EntityCommandService
{
    private readonly IReadOnlyList<IIO> _ioDevices;
    private readonly EntityRegistry _registry;
    private readonly EntityEventBus _bus;
    private readonly ILogger _logger;

    public EntityCommandService(IEnumerable<IIO> ioDevices, EntityRegistry registry, EntityEventBus bus, ILogger? logger = null)
    {
        _ioDevices = ioDevices?.ToList() ?? throw new ArgumentNullException(nameof(ioDevices));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        _logger = logger ?? Log.Logger;
    }

    public async Task<Result> WriteOutputAsync(string systemName, bool value, CancellationToken ct = default)
    {
        if (!_registry.TryGet(systemName, out var entity))
        {
            return Result.Failure($"Unknown entity '{systemName}'");
        }

        if (entity.Direction != EntityDirection.Output)
        {
            return Result.Failure($"Entity '{systemName}' is not an output");
        }

        foreach (var io in _ioDevices)
        {
            var result = await io.WriteOutputAsync(entity.Address, value, ct);
            if (result.IsSuccess)
            {
                _bus.PublishStateChanged(new EntityStateChangedEventArgs(entity.SystemName, value, entity.Direction, DateTime.UtcNow));
                return result;
            }
        }

        _logger.Debug("No IO device available for output {Address}", entity.Address);
        return Result.Failure($"No IO device available for output {entity.Address}");
    }
}
