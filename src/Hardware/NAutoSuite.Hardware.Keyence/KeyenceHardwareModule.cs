using System.Globalization;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Hardware.Abstractions.Hardware;
using NAutoSuite.Hardware.Keyence.Configuration;
using Serilog;

namespace NAutoSuite.Hardware.Keyence;

public sealed class KeyenceHardwareModule : IHardwareModule
{
    private readonly ILogger _logger;

    public KeyenceHardwareModule(ILogger logger)
    {
        _logger = logger;
    }

    public string Type => "keyence";

    public HardwareModuleResult LoadFromConfig(YamlDotNet.RepresentationModel.YamlNode configNode)
    {
        var settings = KeyencePlcSettings.LoadFromYamlNode(configNode);

        ValidateIds(settings.CommonIo.Concat(settings.Io), "io");
        ValidateDataIds(settings.Data);

        var plc = new KeyencePlc(settings.Ip, settings.Port, settings.Name, _logger);
        var ioPoints = settings.CommonIo.Concat(settings.Io)
            .Select(point => new IoPointConfig
            {
                Id = point.Id,
                Name = point.Name,
                Address = point.Address,
                Direction = string.IsNullOrWhiteSpace(point.Direction) ? "Input" : point.Direction,
                DataType = string.IsNullOrWhiteSpace(point.Type) ? "bit" : point.Type,
                DefaultValue = point.Default
            })
            .ToList();

        var dataPoints = settings.Data.Select(item =>
        {
            var defaultValue = ParseDefaultValue(item.Default, item.Type, $"keyence.data.{item.Id}");
            return new DataPointConfig
            {
                Id = item.Id,
                Name = item.Name,
                Address = NormalizeRegisterAddress(item.Address, item.Type),
                DataType = item.Type,
                DefaultValue = defaultValue
            };
        }).ToList();

        return new HardwareModuleResult(
            new IDevice[] { plc },
            Array.Empty<AxisDefinition>(),
            ioPoints,
            dataPoints,
            new RegisterConfig());
    }

    private static void ValidateIds(IEnumerable<KeyenceIoConfig> points, string section)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var point in points)
        {
            if (string.IsNullOrWhiteSpace(point.Id))
            {
                throw new InvalidOperationException($"Missing id in keyence.{section}");
            }

            if (point.Id.Any(char.IsWhiteSpace))
            {
                throw new InvalidOperationException($"Invalid id '{point.Id}' in keyence.{section}");
            }

            if (!seen.Add(point.Id))
            {
                throw new InvalidOperationException($"Duplicate id '{point.Id}' in keyence.{section}");
            }
        }
    }

    private static void ValidateDataIds(IEnumerable<KeyenceDataConfig> points)
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var point in points)
        {
            if (string.IsNullOrWhiteSpace(point.Id))
            {
                throw new InvalidOperationException("Missing id in keyence.data");
            }

            if (point.Id.Any(char.IsWhiteSpace))
            {
                throw new InvalidOperationException($"Invalid id '{point.Id}' in keyence.data");
            }

            if (!seen.Add(point.Id))
            {
                throw new InvalidOperationException($"Duplicate id '{point.Id}' in keyence.data");
            }
        }
    }

    private static string NormalizeRegisterAddress(string address, string type)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return address;
        }

        if (address.Contains('.', StringComparison.OrdinalIgnoreCase))
        {
            return address;
        }

        return type.ToLowerInvariant() switch
        {
            "int32" => $"{address}.L",
            "uint16" => $"{address}.U",
            "int16" => $"{address}.S",
            _ => address
        };
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
