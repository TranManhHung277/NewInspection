using System.Globalization;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Hardware.Abstractions.Hardware;
using NAutoSuite.Hardware.Leadshine.Configuration;
using Serilog;

namespace NAutoSuite.Hardware.Leadshine;

public sealed class LeadshineHardwareModule : IHardwareModule
{
    private readonly ILogger _logger;

    public LeadshineHardwareModule(ILogger logger)
    {
        _logger = logger;
    }

    public string Type => "ethercat_card";

    public HardwareModuleResult LoadFromConfig(YamlDotNet.RepresentationModel.YamlNode configNode)
    {
        var settings = LeadshineHardwareConfig.LoadFromYamlNode(configNode);

        ValidateIds(settings.CommonIo.Concat(settings.Io), "io");
        ValidateDataIds(settings.Data);

        var devices = new List<IDevice>();
        var axes = new List<AxisDefinition>();
        var ioPoints = new List<IoPointConfig>();
        var dataPoints = new List<DataPointConfig>();
        var registers = new RegisterConfig();

        var master = new LeadshineMaster((ushort)settings.CardNo, null, _logger);
        devices.Add(master);

        foreach (var axis in settings.Axes)
        {
            axes.Add(new AxisDefinition
            {
                Id = axis.Id,
                Name = axis.Name,
                Description = axis.Description,
                AxisIndex = axis.AxisIndex
            });

            devices.Add(new LeadshineAxis((ushort)settings.CardNo,
                (ushort)axis.AxisIndex,
                axis.Id,
                axis.Name,
                axis,
                null,
                _logger));
        }

        var allIo = settings.CommonIo.Concat(settings.Io)
            .Select(point => new IoPointConfig
            {
                Id = point.Id,
                Name = point.Name,
                NodeId = point.NodeId,
                IoBit = point.IoBit,
                Address = point.Address,
                Direction = string.IsNullOrWhiteSpace(point.Direction) ? "Input" : point.Direction,
                DataType = string.IsNullOrWhiteSpace(point.Type) ? "bit" : point.Type,
                DefaultValue = point.Default
            })
            .ToList();

        var remotePoints = allIo.Select(point =>
        {
            var address = IoPointConfig.ResolveAddress(point);
            return new RemoteIoPoint
            {
                Address = address,
                NodeId = (ushort)point.NodeId,
                IoBit = (ushort)point.IoBit,
                IsOutput = string.Equals(point.Direction, "Output", StringComparison.OrdinalIgnoreCase)
            };
        }).ToList();
        devices.Add(new LeadshineRemoteIO(settings.CardNo, remotePoints, _logger));

        ioPoints.AddRange(allIo);

        foreach (var item in settings.Data)
        {
            var address = string.IsNullOrWhiteSpace(item.Address)
                ? $"axis:{item.Axis}:pos"
                : item.Address;
            var defaultValue = ParseDefaultValue(item.Default, item.Type, $"leadshine.data.{item.Id}");
            dataPoints.Add(new DataPointConfig
            {
                Id = item.Id,
                Name = item.Name,
                Axis = item.Axis,
                Address = address,
                DataType = item.Type,
                DefaultValue = defaultValue
            });
        }

        devices.Add(new LeadshineRegisterIO((ushort)settings.CardNo, _logger));

        return new HardwareModuleResult(devices, axes, ioPoints, dataPoints, registers);
    }

    private static void ValidateIds(IEnumerable<LeadshineIoConfig> points, string section)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var point in points)
        {
            if (string.IsNullOrWhiteSpace(point.Id))
            {
                throw new InvalidOperationException($"Missing id in leadshine.{section}");
            }

            if (point.Id.Any(char.IsWhiteSpace))
            {
                throw new InvalidOperationException($"Invalid id '{point.Id}' in leadshine.{section}");
            }

            if (!seen.Add(point.Id))
            {
                throw new InvalidOperationException($"Duplicate id '{point.Id}' in leadshine.{section}");
            }
        }
    }

    private static void ValidateDataIds(IEnumerable<LeadshineDataConfig> points)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var point in points)
        {
            if (string.IsNullOrWhiteSpace(point.Id))
            {
                throw new InvalidOperationException("Missing id in leadshine.data");
            }

            if (point.Id.Any(char.IsWhiteSpace))
            {
                throw new InvalidOperationException($"Invalid id '{point.Id}' in leadshine.data");
            }

            if (!seen.Add(point.Id))
            {
                throw new InvalidOperationException($"Duplicate id '{point.Id}' in leadshine.data");
            }
        }
    }

    private double ParseDefaultValue(string rawValue, string dataType, string context)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return 0;
        }

        if (string.Equals(dataType, "bit", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(dataType, "bool", StringComparison.OrdinalIgnoreCase))
        {
            if (bool.TryParse(rawValue, out var boolValue))
            {
                return boolValue ? 1 : 0;
            }

            if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var bitNumber))
            {
                return bitNumber != 0 ? 1 : 0;
            }

            _logger.Warning("Invalid default value '{Value}' for {Context}; using 0", rawValue, context);
            return 0;
        }

        if (double.TryParse(rawValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
        {
            return number;
        }

        _logger.Warning("Invalid default value '{Value}' for {Context}; using 0", rawValue, context);
        return 0;
    }
}
