using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Configuration;
using NAutoSuite.Core.Entities;
using NAutoSuite.Hardware.Abstractions.Hardware;
using NAutoApp.Configuration;
using NAutoApp.Machine;
using Serilog;

namespace NAutoApp.Services;

public sealed class ConfigReloadService
{
    private readonly IEnumerable<IHardwareModule> _modules;
    private readonly ConfigMachineProfile _profile;
    private readonly EntityRegistry _entityRegistry;
    private readonly DataEntityRegistry _dataRegistry;
    private readonly NAutoAppMachine _machine;
    private readonly ILogger _logger;

    public ConfigReloadService(
        IEnumerable<IHardwareModule> modules,
        ConfigMachineProfile profile,
        EntityRegistry entityRegistry,
        DataEntityRegistry dataRegistry,
        NAutoAppMachine machine,
        ILogger logger)
    {
        _modules = modules;
        _profile = profile;
        _entityRegistry = entityRegistry;
        _dataRegistry = dataRegistry;
        _machine = machine;
        _logger = logger.ForContext<ConfigReloadService>()
            .ForContext("Layer", "Config");
    }

    public Task<Result> ReloadAsync(CancellationToken ct = default)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, "Configs", "hardware_config.yaml");
        HardwareCatalogSettings catalog;
        try
        {
            catalog = HardwareCatalogSettings.Load(configPath);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Reload failed. Invalid config {Path}", configPath);
            return Task.FromResult(Result.Failure("Reload failed. Invalid config file."));
        }

        var profile = new HardwareProfileConfig();

        foreach (var entry in catalog.Hardware)
        {
            var module = _modules.FirstOrDefault(m =>
                string.Equals(m.Type, entry.Type, StringComparison.OrdinalIgnoreCase));
            if (module == null)
            {
                _logger.Warning("No hardware module registered for '{Type}'", entry.Type);
                continue;
            }

            var result = module.LoadFromConfig(entry.ConfigNode);
            profile.AddAxes(result.Axes);
            profile.AddIoPoints(entry.Name, result.IoPoints);
            profile.AddDataPoints(entry.Name, result.DataPoints);
            profile.MergeRegisters(result.Registers);
        }

        _profile.ApplyConfig(profile);
        _entityRegistry.Reload(profile.IoPoints);
        _dataRegistry.Reload(profile.DataPoints);

        _machine.ReloadIoScan();
        _machine.RefreshEntityAddresses();

        _logger.Information("Hardware config reloaded successfully");
        return Task.FromResult(Result.Success("Config reloaded"));
    }
}

