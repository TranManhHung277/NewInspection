using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NAutoSuite.Core.Configuration;

/// <summary>
/// Service to load and parse hardware configuration from YAML files
/// </summary>
public sealed class HardwareConfigLoader
{
    private readonly ILogger _logger;
    private readonly IDeserializer _deserializer;

    public HardwareConfigLoader(ILogger? logger = null)
    {
        _logger = (logger ?? Log.Logger).ForContext<HardwareConfigLoader>();
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    /// Load configuration from YAML file
    /// </summary>
    public HardwareRootConfig Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Hardware config file not found: {path}");
        }

        var yaml = File.ReadAllText(path);
        var config = _deserializer.Deserialize<HardwareRootConfig>(yaml);

        if (config == null)
        {
            throw new InvalidOperationException("Failed to deserialize hardware config");
        }

        _logger.Information("Loaded hardware config from {Path}", path);
        _logger.Debug("App: {AppName} v{Version}", config.App.Name, config.App.Version);

        return config;
    }

    /// <summary>
    /// Load minimal axis/signal configuration from YAML file.
    /// </summary>
    public HardwareMinimalConfig LoadMinimal(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Hardware config file not found: {path}");
        }

        var yaml = File.ReadAllText(path);
        var config = _deserializer.Deserialize<HardwareMinimalConfig>(yaml);

        if (config == null)
        {
            throw new InvalidOperationException("Failed to deserialize minimal hardware config");
        }

        _logger.Information("Loaded minimal hardware config from {Path}", path);
        _logger.Debug("App: {AppName} v{Version}", config.App.Name, config.App.Version);

        return config;
    }

    /// <summary>
    /// Load configuration safely, returning empty config on error
    /// </summary>
    public HardwareRootConfig LoadSafe(string path)
    {
        try
        {
            return Load(path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load hardware config from {Path}", path);
            return new HardwareRootConfig();
        }
    }

    /// <summary>
    /// Load minimal configuration safely, returning empty config on error.
    /// </summary>
    public HardwareMinimalConfig LoadMinimalSafe(string path)
    {
        try
        {
            return LoadMinimal(path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load minimal hardware config from {Path}", path);
            return new HardwareMinimalConfig();
        }
    }

    /// <summary>
    /// Load and flatten configuration into entity collections
    /// </summary>
    public HardwareEntityCollection LoadEntities(string path)
    {
        var config = Load(path);
        return FlattenToEntities(config);
    }

    /// <summary>
    /// Load and flatten configuration safely
    /// </summary>
    public HardwareEntityCollection LoadEntitiesSafe(string path)
    {
        try
        {
            return LoadEntities(path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load hardware entities from {Path}", path);
            return HardwareEntityCollection.Empty;
        }
    }

    /// <summary>
    /// Flatten hierarchical config into entity collections
    /// </summary>
    public HardwareEntityCollection FlattenToEntities(HardwareRootConfig config)
    {
        var axes = new List<AxisEntity>();
        var remoteIOs = new List<RemoteIOEntity>();
        var allInputs = new List<IOSignalEntity>();
        var allOutputs = new List<IOSignalEntity>();
        var allStatusBits = new List<StatusBitEntity>();

        foreach (var (hardwareName, hardwareConfig) in config.Hardware)
        {
            foreach (var card in hardwareConfig.Cards)
            {
                foreach (var client in card.Clients)
                {
                    if (client.IsAxis)
                    {
                        var axisEntity = CreateAxisEntity(hardwareName, card.CardNo, client);
                        axes.Add(axisEntity);
                        allStatusBits.AddRange(axisEntity.StatusBits);
                        allStatusBits.AddRange(axisEntity.ServoStatusword);

                        _logger.Debug("Registered axis: {EntityId} (CardNo={CardNo}, AxisNo={AxisNo})",
                            axisEntity.EntityId, axisEntity.CardNo, axisEntity.AxisNo);
                    }
                    else if (client.IsRemoteIO)
                    {
                        var ioEntity = CreateRemoteIOEntity(hardwareName, card.CardNo, client);
                        remoteIOs.Add(ioEntity);
                        allInputs.AddRange(ioEntity.Inputs);
                        allOutputs.AddRange(ioEntity.Outputs);

                        _logger.Debug("Registered remote IO: {EntityId} (CardNo={CardNo}, NodeId={NodeId}, Inputs={InputCount}, Outputs={OutputCount})",
                            ioEntity.EntityId, ioEntity.CardNo, ioEntity.NodeId,
                            ioEntity.Inputs.Count, ioEntity.Outputs.Count);
                    }
                }
            }
        }

        _logger.Information("Flattened config: {AxisCount} axes, {IOModuleCount} IO modules, {InputCount} inputs, {OutputCount} outputs",
            axes.Count, remoteIOs.Count, allInputs.Count, allOutputs.Count);

        return new HardwareEntityCollection(config.App, axes, remoteIOs, allInputs, allOutputs, allStatusBits);
    }

    /// <summary>
    /// Load and flatten minimal configuration into entity collections.
    /// </summary>
    public HardwareEntityCollection LoadMinimalEntities(string path)
    {
        var config = LoadMinimal(path);
        return FlattenMinimalToEntities(config);
    }

    /// <summary>
    /// Load and flatten minimal configuration safely.
    /// </summary>
    public HardwareEntityCollection LoadMinimalEntitiesSafe(string path)
    {
        try
        {
            return LoadMinimalEntities(path);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load minimal hardware entities from {Path}", path);
            return HardwareEntityCollection.Empty;
        }
    }

    /// <summary>
    /// Flatten minimal axis/signal config into entity collections.
    /// </summary>
    public HardwareEntityCollection FlattenMinimalToEntities(HardwareMinimalConfig config)
    {
        var axes = new List<AxisEntity>();
        var remoteIOs = new List<RemoteIOEntity>();
        var allInputs = new List<IOSignalEntity>();
        var allOutputs = new List<IOSignalEntity>();
        var allStatusBits = new List<StatusBitEntity>();
        var remoteIoByNode = new Dictionary<ushort, RemoteIOEntity>();

        foreach (var axis in config.Axes)
        {
            var axisEntity = new AxisEntity
            {
                EntityId = axis.Id,
                HardwareName = "axis",
                CardNo = 0,
                NodeId = axis.NodeId,
                AxisNo = axis.AxisNo,
                Name = axis.Name,
                Motion = axis.Config ?? new AxisMotionConfig(),
                Home = axis.Home ?? new AxisHomeConfig(),
                StatusBits = BuildStatusBits(axis.Id, axis.StatusBits, StatusBitType.AxisIO),
                ServoStatusword = BuildStatusBits(axis.Id, axis.ServoStatusword, StatusBitType.ServoStatusword)
            };

            axes.Add(axisEntity);
            allStatusBits.AddRange(axisEntity.StatusBits);
            allStatusBits.AddRange(axisEntity.ServoStatusword);
        }

        AddSignals(config.Signals.Inputs, IODirection.Input, remoteIoByNode, remoteIOs, allInputs);
        AddSignals(config.Signals.Outputs, IODirection.Output, remoteIoByNode, remoteIOs, allOutputs);

        _logger.Information(
            "Flattened minimal config: {AxisCount} axes, {IOModuleCount} IO modules, {InputCount} inputs, {OutputCount} outputs",
            axes.Count, remoteIOs.Count, allInputs.Count, allOutputs.Count);

        return new HardwareEntityCollection(config.App, axes, remoteIOs, allInputs, allOutputs, allStatusBits);
    }

    private static void AddSignals(
        IEnumerable<SignalPointConfig> signals,
        IODirection direction,
        Dictionary<ushort, RemoteIOEntity> remoteIoByNode,
        List<RemoteIOEntity> remoteIOs,
        List<IOSignalEntity> allSignals)
    {
        foreach (var signal in signals)
        {
            if (!remoteIoByNode.TryGetValue(signal.NodeId, out var ioEntity))
            {
                ioEntity = new RemoteIOEntity
                {
                    EntityId = $"io_{signal.NodeId}",
                    HardwareName = "signal",
                    CardNo = 0,
                    NodeId = signal.NodeId,
                    Name = $"Remote IO {signal.NodeId}"
                };
                remoteIoByNode[signal.NodeId] = ioEntity;
                remoteIOs.Add(ioEntity);
            }

            var entity = new IOSignalEntity
            {
                EntityId = $"{(direction == IODirection.Input ? "input" : "output")}.{signal.Id}",
                ParentId = ioEntity.EntityId,
                HardwareName = "signal",
                CardNo = 0,
                NodeId = signal.NodeId,
                PortNo = signal.PortNo,
                Bit = signal.Bit,
                Name = signal.Name,
                Category = signal.Category,
                Default = signal.Default,
                Direction = direction
            };

            allSignals.Add(entity);
            if (direction == IODirection.Input)
            {
                ioEntity.Inputs.Add(entity);
            }
            else
            {
                ioEntity.Outputs.Add(entity);
            }
        }
    }

    private static List<StatusBitEntity> BuildStatusBits(
        string axisId,
        List<StatusBitConfig>? bits,
        StatusBitType bitType)
    {
        var result = new List<StatusBitEntity>();
        if (bits == null)
        {
            return result;
        }

        foreach (var bit in bits)
        {
            result.Add(new StatusBitEntity
            {
                EntityId = $"{axisId}.{(bitType == StatusBitType.AxisIO ? "status" : "servo")}.{bit.Id}",
                ParentId = axisId,
                Name = bit.Name,
                Bit = bit.Bit,
                Description = bit.Description,
                BitType = bitType
            });
        }

        return result;
    }

    private static AxisEntity CreateAxisEntity(string hardwareName, ushort cardNo, ClientConfig client)
    {
        var statusBits = new List<StatusBitEntity>();
        var servoStatusword = new List<StatusBitEntity>();

        if (client.StatusBits != null)
        {
            foreach (var bit in client.StatusBits)
            {
                statusBits.Add(new StatusBitEntity
                {
                    EntityId = $"{client.Id}.status.{bit.Id}",
                    ParentId = client.Id,
                    Name = bit.Name,
                    Bit = bit.Bit,
                    Description = bit.Description,
                    BitType = StatusBitType.AxisIO
                });
            }
        }

        if (client.ServoStatusword != null)
        {
            foreach (var bit in client.ServoStatusword)
            {
                servoStatusword.Add(new StatusBitEntity
                {
                    EntityId = $"{client.Id}.servo.{bit.Id}",
                    ParentId = client.Id,
                    Name = bit.Name,
                    Bit = bit.Bit,
                    Description = bit.Description,
                    BitType = StatusBitType.ServoStatusword
                });
            }
        }

        return new AxisEntity
        {
            EntityId = client.Id,
            HardwareName = hardwareName,
            CardNo = cardNo,
            NodeId = client.NodeId,
            AxisNo = client.AxisNo,
            Name = client.Name,
            Motion = client.Config ?? new AxisMotionConfig(),
            Home = client.Home ?? new AxisHomeConfig(),
            StatusBits = statusBits,
            ServoStatusword = servoStatusword
        };
    }

    private static RemoteIOEntity CreateRemoteIOEntity(string hardwareName, ushort cardNo, ClientConfig client)
    {
        var inputs = new List<IOSignalEntity>();
        var outputs = new List<IOSignalEntity>();

        if (client.Inputs != null)
        {
            foreach (var port in client.Inputs)
            {
                foreach (var signal in port.Signals)
                {
                    inputs.Add(new IOSignalEntity
                    {
                        EntityId = $"{client.Id}.input.{signal.Id}",
                        ParentId = client.Id,
                        HardwareName = hardwareName,
                        CardNo = cardNo,
                        NodeId = client.NodeId,
                        PortNo = port.PortNo,
                        Bit = signal.Bit,
                        Name = signal.Name,
                        Category = signal.Category,
                        Default = signal.Default,
                        Direction = IODirection.Input
                    });
                }
            }
        }

        if (client.Outputs != null)
        {
            foreach (var port in client.Outputs)
            {
                foreach (var signal in port.Signals)
                {
                    outputs.Add(new IOSignalEntity
                    {
                        EntityId = $"{client.Id}.output.{signal.Id}",
                        ParentId = client.Id,
                        HardwareName = hardwareName,
                        CardNo = cardNo,
                        NodeId = client.NodeId,
                        PortNo = port.PortNo,
                        Bit = signal.Bit,
                        Name = signal.Name,
                        Category = signal.Category,
                        Default = signal.Default,
                        Direction = IODirection.Output
                    });
                }
            }
        }

        return new RemoteIOEntity
        {
            EntityId = client.Id,
            HardwareName = hardwareName,
            CardNo = cardNo,
            NodeId = client.NodeId,
            Name = client.Name,
            Inputs = inputs,
            Outputs = outputs
        };
    }
}

/// <summary>
/// Collection of all hardware entities flattened from config
/// </summary>
public sealed class HardwareEntityCollection
{
    public static HardwareEntityCollection Empty { get; } = new(
        new AppConfig(),
        new List<AxisEntity>(),
        new List<RemoteIOEntity>(),
        new List<IOSignalEntity>(),
        new List<IOSignalEntity>(),
        new List<StatusBitEntity>());

    public AppConfig App { get; }
    public IReadOnlyList<AxisEntity> Axes { get; }
    public IReadOnlyList<RemoteIOEntity> RemoteIOs { get; }
    public IReadOnlyList<IOSignalEntity> Inputs { get; }
    public IReadOnlyList<IOSignalEntity> Outputs { get; }
    public IReadOnlyList<StatusBitEntity> StatusBits { get; }

    // Lookup dictionaries for fast access
    private readonly Dictionary<string, AxisEntity> _axesById;
    private readonly Dictionary<string, RemoteIOEntity> _remoteIOsById;
    private readonly Dictionary<string, IOSignalEntity> _inputsById;
    private readonly Dictionary<string, IOSignalEntity> _outputsById;
    private readonly Dictionary<string, StatusBitEntity> _statusBitsById;

    public HardwareEntityCollection(
        AppConfig app,
        List<AxisEntity> axes,
        List<RemoteIOEntity> remoteIOs,
        List<IOSignalEntity> inputs,
        List<IOSignalEntity> outputs,
        List<StatusBitEntity> statusBits)
    {
        App = app;
        Axes = axes;
        RemoteIOs = remoteIOs;
        Inputs = inputs;
        Outputs = outputs;
        StatusBits = statusBits;

        _axesById = axes.ToDictionary(a => a.EntityId, StringComparer.OrdinalIgnoreCase);
        _remoteIOsById = remoteIOs.ToDictionary(r => r.EntityId, StringComparer.OrdinalIgnoreCase);
        _inputsById = inputs.ToDictionary(i => i.EntityId, StringComparer.OrdinalIgnoreCase);
        _outputsById = outputs.ToDictionary(o => o.EntityId, StringComparer.OrdinalIgnoreCase);
        _statusBitsById = statusBits.ToDictionary(s => s.EntityId, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get axis by entity ID
    /// </summary>
    public AxisEntity? GetAxis(string entityId)
    {
        return _axesById.TryGetValue(entityId, out var axis) ? axis : null;
    }

    /// <summary>
    /// Get remote IO module by entity ID
    /// </summary>
    public RemoteIOEntity? GetRemoteIO(string entityId)
    {
        return _remoteIOsById.TryGetValue(entityId, out var io) ? io : null;
    }

    /// <summary>
    /// Get input signal by entity ID
    /// </summary>
    public IOSignalEntity? GetInput(string entityId)
    {
        return _inputsById.TryGetValue(entityId, out var input) ? input : null;
    }

    /// <summary>
    /// Get output signal by entity ID
    /// </summary>
    public IOSignalEntity? GetOutput(string entityId)
    {
        return _outputsById.TryGetValue(entityId, out var output) ? output : null;
    }

    /// <summary>
    /// Get any IO signal (input or output) by entity ID
    /// </summary>
    public IOSignalEntity? GetSignal(string entityId)
    {
        return GetInput(entityId) ?? GetOutput(entityId);
    }

    /// <summary>
    /// Get status bit by entity ID
    /// </summary>
    public StatusBitEntity? GetStatusBit(string entityId)
    {
        return _statusBitsById.TryGetValue(entityId, out var bit) ? bit : null;
    }

    /// <summary>
    /// Get all inputs for a specific category
    /// </summary>
    public IEnumerable<IOSignalEntity> GetInputsByCategory(string category)
    {
        return Inputs.Where(i => string.Equals(i.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all outputs for a specific category
    /// </summary>
    public IEnumerable<IOSignalEntity> GetOutputsByCategory(string category)
    {
        return Outputs.Where(o => string.Equals(o.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Get all common inputs
    /// </summary>
    public IEnumerable<IOSignalEntity> CommonInputs => GetInputsByCategory("common");

    /// <summary>
    /// Get all common outputs
    /// </summary>
    public IEnumerable<IOSignalEntity> CommonOutputs => GetOutputsByCategory("common");

    /// <summary>
    /// Get all process inputs
    /// </summary>
    public IEnumerable<IOSignalEntity> ProcessInputs => GetInputsByCategory("process");

    /// <summary>
    /// Get all process outputs
    /// </summary>
    public IEnumerable<IOSignalEntity> ProcessOutputs => GetOutputsByCategory("process");
}
