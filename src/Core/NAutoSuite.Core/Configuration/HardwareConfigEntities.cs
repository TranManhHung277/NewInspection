using YamlDotNet.Serialization;

namespace NAutoSuite.Core.Configuration;

#region Root Config

/// <summary>
/// Root configuration containing app info and hardware definitions
/// </summary>
public sealed class HardwareRootConfig
{
    [YamlMember(Alias = "app")]
    public AppConfig App { get; set; } = new();

    [YamlMember(Alias = "hardware")]
    public Dictionary<string, HardwareTypeConfig> Hardware { get; set; } = new();
}

/// <summary>
/// Application metadata
/// </summary>
public sealed class AppConfig
{
    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;

    [YamlMember(Alias = "version")]
    public string Version { get; set; } = "1.0.0";

    [YamlMember(Alias = "machine")]
    public int Machine { get; set; }
}

#endregion

#region Minimal Config (Axis + Signals)

/// <summary>
/// Minimal config that focuses on axis and IO signals only.
/// </summary>
public sealed class HardwareMinimalConfig
{
    [YamlMember(Alias = "app")]
    public AppConfig App { get; set; } = new();

    [YamlMember(Alias = "axis")]
    public List<AxisMinimalConfig> Axes { get; set; } = new();

    [YamlMember(Alias = "signal")]
    public SignalGroupConfig Signals { get; set; } = new();
}

/// <summary>
/// Minimal axis config for motion and home.
/// </summary>
public sealed class AxisMinimalConfig
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "node_id")]
    public ushort NodeId { get; set; }

    [YamlMember(Alias = "axis_no")]
    public ushort AxisNo { get; set; }

    [YamlMember(Alias = "config")]
    public AxisMotionConfig? Config { get; set; }

    [YamlMember(Alias = "home")]
    public AxisHomeConfig? Home { get; set; }

    [YamlMember(Alias = "status_bits")]
    public List<StatusBitConfig>? StatusBits { get; set; }

    [YamlMember(Alias = "servo_statusword")]
    public List<StatusBitConfig>? ServoStatusword { get; set; }
}

/// <summary>
/// Grouped signals for input/output.
/// </summary>
public sealed class SignalGroupConfig
{
    [YamlMember(Alias = "input")]
    public List<SignalPointConfig> Inputs { get; set; } = new();

    [YamlMember(Alias = "output")]
    public List<SignalPointConfig> Outputs { get; set; } = new();
}

/// <summary>
/// Minimal IO signal config.
/// </summary>
public sealed class SignalPointConfig
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "node_id")]
    public ushort NodeId { get; set; }

    [YamlMember(Alias = "port_no")]
    public ushort PortNo { get; set; }

    [YamlMember(Alias = "bit")]
    public int Bit { get; set; }

    [YamlMember(Alias = "category")]
    public string Category { get; set; } = "process";

    [YamlMember(Alias = "default")]
    public bool Default { get; set; }
}

#endregion

#region Hardware Type Config

/// <summary>
/// Hardware type configuration (e.g., leadshine)
/// </summary>
public sealed class HardwareTypeConfig
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;

    [YamlMember(Alias = "cards")]
    public List<CardConfig> Cards { get; set; } = new();
}

/// <summary>
/// Card configuration (e.g., EtherCAT card)
/// </summary>
public sealed class CardConfig
{
    [YamlMember(Alias = "card_no")]
    public ushort CardNo { get; set; }

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "cycle_time_ms")]
    public string CycleTimeMs { get; set; } = string.Empty;

    [YamlMember(Alias = "clients")]
    public List<ClientConfig> Clients { get; set; } = new();
}

#endregion

#region Client Config (Axis or Remote IO)

/// <summary>
/// Base client configuration - can be axis or remote_io
/// </summary>
public sealed class ClientConfig
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;

    [YamlMember(Alias = "node_id")]
    public ushort NodeId { get; set; }

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    // Axis-specific properties
    [YamlMember(Alias = "axis_no")]
    public ushort AxisNo { get; set; }

    [YamlMember(Alias = "config")]
    public AxisMotionConfig? Config { get; set; }

    [YamlMember(Alias = "home")]
    public AxisHomeConfig? Home { get; set; }

    [YamlMember(Alias = "status_bits")]
    public List<StatusBitConfig>? StatusBits { get; set; }

    [YamlMember(Alias = "servo_statusword")]
    public List<StatusBitConfig>? ServoStatusword { get; set; }

    // Remote IO-specific properties
    [YamlMember(Alias = "inputs")]
    public List<PortConfig>? Inputs { get; set; }

    [YamlMember(Alias = "outputs")]
    public List<PortConfig>? Outputs { get; set; }

    /// <summary>
    /// Check if this client is an axis
    /// </summary>
    public bool IsAxis => string.Equals(Type, "axis", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Check if this client is a remote IO module
    /// </summary>
    public bool IsRemoteIO => string.Equals(Type, "remote_io", StringComparison.OrdinalIgnoreCase);
}

#endregion

#region Axis Config

/// <summary>
/// Axis motion configuration
/// </summary>
public sealed class AxisMotionConfig
{
    [YamlMember(Alias = "min_vel")]
    public double MinVel { get; set; }

    [YamlMember(Alias = "max_vel")]
    public double MaxVel { get; set; }

    [YamlMember(Alias = "acc")]
    public double Acc { get; set; }

    [YamlMember(Alias = "dec")]
    public double Dec { get; set; }

    [YamlMember(Alias = "stop_vel")]
    public double StopVel { get; set; }

    [YamlMember(Alias = "jog_vel")]
    public double JogVel { get; set; }

    [YamlMember(Alias = "run_mode")]
    public int RunMode { get; set; }

    [YamlMember(Alias = "gear_equiv")]
    public double GearEquiv { get; set; }

    [YamlMember(Alias = "servo_on_connect")]
    public bool ServoOnConnect { get; set; }

    [YamlMember(Alias = "enable_gear")]
    public bool EnableGear { get; set; }
}

/// <summary>
/// Axis homing configuration
/// </summary>
public sealed class AxisHomeConfig
{
    [YamlMember(Alias = "dir")]
    public int Dir { get; set; }

    [YamlMember(Alias = "vel")]
    public double Vel { get; set; }

    [YamlMember(Alias = "mode")]
    public int Mode { get; set; }

    [YamlMember(Alias = "ez_count")]
    public int EzCount { get; set; }
}

/// <summary>
/// Status bit configuration (for axis IO status or servo statusword)
/// </summary>
public sealed class StatusBitConfig
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "bit")]
    public int Bit { get; set; }

    [YamlMember(Alias = "description")]
    public string Description { get; set; } = string.Empty;
}

#endregion

#region Remote IO Config

/// <summary>
/// IO Port configuration
/// </summary>
public sealed class PortConfig
{
    [YamlMember(Alias = "port_no")]
    public ushort PortNo { get; set; }

    [YamlMember(Alias = "signals")]
    public List<SignalConfig> Signals { get; set; } = new();
}

/// <summary>
/// IO Signal configuration
/// </summary>
public sealed class SignalConfig
{
    [YamlMember(Alias = "id")]
    public string Id { get; set; } = string.Empty;

    [YamlMember(Alias = "name")]
    public string Name { get; set; } = string.Empty;

    [YamlMember(Alias = "bit")]
    public int Bit { get; set; }

    [YamlMember(Alias = "category")]
    public string Category { get; set; } = "process";

    [YamlMember(Alias = "default")]
    public bool Default { get; set; }
}

#endregion

#region Flattened Entity Models (for easy access)

/// <summary>
/// Flattened axis entity with full context
/// </summary>
public sealed class AxisEntity
{
    public string EntityId { get; init; } = string.Empty;
    public string HardwareName { get; init; } = string.Empty;
    public ushort CardNo { get; init; }
    public ushort NodeId { get; init; }
    public ushort AxisNo { get; init; }
    public string Name { get; init; } = string.Empty;
    public AxisMotionConfig Motion { get; init; } = new();
    public AxisHomeConfig Home { get; init; } = new();
    public List<StatusBitEntity> StatusBits { get; init; } = new();
    public List<StatusBitEntity> ServoStatusword { get; init; } = new();
}

/// <summary>
/// Flattened status bit entity
/// </summary>
public sealed class StatusBitEntity
{
    public string EntityId { get; init; } = string.Empty;
    public string ParentId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Bit { get; init; }
    public string Description { get; init; } = string.Empty;
    public StatusBitType BitType { get; init; }
}

public enum StatusBitType
{
    AxisIO,
    ServoStatusword
}

/// <summary>
/// Flattened remote IO module entity
/// </summary>
public sealed class RemoteIOEntity
{
    public string EntityId { get; init; } = string.Empty;
    public string HardwareName { get; init; } = string.Empty;
    public ushort CardNo { get; init; }
    public ushort NodeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public List<IOSignalEntity> Inputs { get; init; } = new();
    public List<IOSignalEntity> Outputs { get; init; } = new();
}

/// <summary>
/// Flattened IO signal entity
/// </summary>
public sealed class IOSignalEntity
{
    public string EntityId { get; init; } = string.Empty;
    public string ParentId { get; init; } = string.Empty;
    public string HardwareName { get; init; } = string.Empty;
    public ushort CardNo { get; init; }
    public ushort NodeId { get; init; }
    public ushort PortNo { get; init; }
    public int Bit { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Category { get; init; } = "process";
    public bool Default { get; init; }
    public IODirection Direction { get; init; }

    /// <summary>
    /// Generate address string for register mapping (e.g., "I2:0:5" for input, node 2, port 0, bit 5)
    /// </summary>
    public string Address => $"{(Direction == IODirection.Input ? "I" : "O")}{NodeId}:{PortNo}:{Bit}";
}

public enum IODirection
{
    Input,
    Output
}

#endregion
