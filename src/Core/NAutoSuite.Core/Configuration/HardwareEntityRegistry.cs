using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace NAutoSuite.Core.Configuration;

/// <summary>
/// Registry for hardware entities - provides centralized access to all hardware configurations
/// </summary>
public sealed class HardwareEntityRegistry
{
    private readonly ILogger _logger;
    private HardwareEntityCollection _entities = HardwareEntityCollection.Empty;
    private bool _isInitialized;

    public HardwareEntityRegistry(ILogger? logger = null)
    {
        _logger = (logger ?? Log.Logger).ForContext<HardwareEntityRegistry>();
    }

    /// <summary>
    /// Initialize registry from config file path
    /// </summary>
    public void Initialize(string configPath)
    {
        var loader = new HardwareConfigLoader(_logger);
        _entities = loader.LoadEntitiesSafe(configPath);
        _isInitialized = true;

        _logger.Information("HardwareEntityRegistry initialized: {AxisCount} axes, {InputCount} inputs, {OutputCount} outputs",
            _entities.Axes.Count, _entities.Inputs.Count, _entities.Outputs.Count);
    }

    /// <summary>
    /// Initialize registry from pre-loaded entity collection
    /// </summary>
    public void Initialize(HardwareEntityCollection entities)
    {
        _entities = entities;
        _isInitialized = true;

        _logger.Information("HardwareEntityRegistry initialized with pre-loaded entities");
    }

    /// <summary>
    /// Check if registry is initialized
    /// </summary>
    public bool IsInitialized => _isInitialized;

    /// <summary>
    /// Get all entities
    /// </summary>
    public HardwareEntityCollection Entities => _entities;

    /// <summary>
    /// Application configuration
    /// </summary>
    public AppConfig App => _entities.App;

    #region Axes

    /// <summary>
    /// Get all axes
    /// </summary>
    public IReadOnlyList<AxisEntity> Axes => _entities.Axes;

    /// <summary>
    /// Get axis by entity ID
    /// </summary>
    public AxisEntity? GetAxis(string entityId) => _entities.GetAxis(entityId);

    /// <summary>
    /// Get axis by entity ID, throws if not found
    /// </summary>
    public AxisEntity RequireAxis(string entityId)
    {
        return GetAxis(entityId)
            ?? throw new KeyNotFoundException($"Axis '{entityId}' not found in registry");
    }

    #endregion

    #region Remote IO

    /// <summary>
    /// Get all remote IO modules
    /// </summary>
    public IReadOnlyList<RemoteIOEntity> RemoteIOs => _entities.RemoteIOs;

    /// <summary>
    /// Get remote IO module by entity ID
    /// </summary>
    public RemoteIOEntity? GetRemoteIO(string entityId) => _entities.GetRemoteIO(entityId);

    #endregion

    #region IO Signals

    /// <summary>
    /// Get all input signals
    /// </summary>
    public IReadOnlyList<IOSignalEntity> Inputs => _entities.Inputs;

    /// <summary>
    /// Get all output signals
    /// </summary>
    public IReadOnlyList<IOSignalEntity> Outputs => _entities.Outputs;

    /// <summary>
    /// Get input signal by entity ID
    /// </summary>
    public IOSignalEntity? GetInput(string entityId) => _entities.GetInput(entityId);

    /// <summary>
    /// Get output signal by entity ID
    /// </summary>
    public IOSignalEntity? GetOutput(string entityId) => _entities.GetOutput(entityId);

    /// <summary>
    /// Get any IO signal by entity ID
    /// </summary>
    public IOSignalEntity? GetSignal(string entityId) => _entities.GetSignal(entityId);

    /// <summary>
    /// Get input signal by entity ID, throws if not found
    /// </summary>
    public IOSignalEntity RequireInput(string entityId)
    {
        return GetInput(entityId)
            ?? throw new KeyNotFoundException($"Input signal '{entityId}' not found in registry");
    }

    /// <summary>
    /// Get output signal by entity ID, throws if not found
    /// </summary>
    public IOSignalEntity RequireOutput(string entityId)
    {
        return GetOutput(entityId)
            ?? throw new KeyNotFoundException($"Output signal '{entityId}' not found in registry");
    }

    /// <summary>
    /// Get all common inputs
    /// </summary>
    public IEnumerable<IOSignalEntity> CommonInputs => _entities.CommonInputs;

    /// <summary>
    /// Get all common outputs
    /// </summary>
    public IEnumerable<IOSignalEntity> CommonOutputs => _entities.CommonOutputs;

    /// <summary>
    /// Get all process inputs
    /// </summary>
    public IEnumerable<IOSignalEntity> ProcessInputs => _entities.ProcessInputs;

    /// <summary>
    /// Get all process outputs
    /// </summary>
    public IEnumerable<IOSignalEntity> ProcessOutputs => _entities.ProcessOutputs;

    #endregion

    #region Status Bits

    /// <summary>
    /// Get all status bits
    /// </summary>
    public IReadOnlyList<StatusBitEntity> StatusBits => _entities.StatusBits;

    /// <summary>
    /// Get status bit by entity ID
    /// </summary>
    public StatusBitEntity? GetStatusBit(string entityId) => _entities.GetStatusBit(entityId);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Check if an entity exists by ID
    /// </summary>
    public bool HasEntity(string entityId)
    {
        return GetAxis(entityId) != null
            || GetRemoteIO(entityId) != null
            || GetSignal(entityId) != null
            || GetStatusBit(entityId) != null;
    }

    /// <summary>
    /// Get all entity IDs
    /// </summary>
    public IEnumerable<string> GetAllEntityIds()
    {
        foreach (var axis in Axes)
        {
            yield return axis.EntityId;
            foreach (var bit in axis.StatusBits)
                yield return bit.EntityId;
            foreach (var bit in axis.ServoStatusword)
                yield return bit.EntityId;
        }

        foreach (var io in RemoteIOs)
        {
            yield return io.EntityId;
        }

        foreach (var input in Inputs)
            yield return input.EntityId;

        foreach (var output in Outputs)
            yield return output.EntityId;
    }

    /// <summary>
    /// Print summary to logger
    /// </summary>
    public void LogSummary()
    {
        _logger.Information("=== Hardware Entity Registry Summary ===");
        _logger.Information("App: {AppName} v{Version}", App.Name, App.Version);
        _logger.Information("Machine: {Machine}", App.Machine);

        _logger.Information("--- Axes ({Count}) ---", Axes.Count);
        foreach (var axis in Axes)
        {
            _logger.Information("  {EntityId}: Card={CardNo}, Axis={AxisNo}, StatusBits={StatusCount}, ServoWord={ServoCount}",
                axis.EntityId, axis.CardNo, axis.AxisNo,
                axis.StatusBits.Count, axis.ServoStatusword.Count);
        }

        _logger.Information("--- Remote IO Modules ({Count}) ---", RemoteIOs.Count);
        foreach (var io in RemoteIOs)
        {
            _logger.Information("  {EntityId}: Card={CardNo}, Node={NodeId}, Inputs={InputCount}, Outputs={OutputCount}",
                io.EntityId, io.CardNo, io.NodeId,
                io.Inputs.Count, io.Outputs.Count);
        }

        _logger.Information("--- IO Signals ---");
        _logger.Information("  Common Inputs: {Count}", CommonInputs.Count());
        _logger.Information("  Process Inputs: {Count}", ProcessInputs.Count());
        _logger.Information("  Common Outputs: {Count}", CommonOutputs.Count());
        _logger.Information("  Process Outputs: {Count}", ProcessOutputs.Count());
    }

    #endregion
}

/// <summary>
/// Extension methods for dependency injection
/// </summary>
public static class HardwareConfigServiceExtensions
{
    /// <summary>
    /// Add hardware configuration services to DI container
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configPath">Path to hardware_config.yaml</param>
    public static IServiceCollection AddHardwareConfig(this IServiceCollection services, string configPath)
    {
        // Register loader
        services.AddSingleton<HardwareConfigLoader>();

        // Register entity collection
        services.AddSingleton(sp =>
        {
            var logger = sp.GetService<ILogger>();
            var loader = new HardwareConfigLoader(logger);
            return loader.LoadEntitiesSafe(configPath);
        });

        // Register registry
        services.AddSingleton(sp =>
        {
            var logger = sp.GetService<ILogger>();
            var entities = sp.GetRequiredService<HardwareEntityCollection>();
            var registry = new HardwareEntityRegistry(logger);
            registry.Initialize(entities);
            return registry;
        });

        return services;
    }

    /// <summary>
    /// Add hardware configuration services with custom loader
    /// </summary>
    public static IServiceCollection AddHardwareConfig(this IServiceCollection services, Func<IServiceProvider, HardwareEntityCollection> entityFactory)
    {
        services.AddSingleton<HardwareConfigLoader>();
        services.AddSingleton(entityFactory);
        services.AddSingleton(sp =>
        {
            var logger = sp.GetService<ILogger>();
            var entities = sp.GetRequiredService<HardwareEntityCollection>();
            var registry = new HardwareEntityRegistry(logger);
            registry.Initialize(entities);
            return registry;
        });

        return services;
    }
}
