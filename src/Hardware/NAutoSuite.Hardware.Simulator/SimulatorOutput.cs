using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Hardware.Simulator;

/// <summary>
/// Simulated digital output
/// </summary>
public class SimulatorOutput : IOutput
{
    private bool _value;
    private readonly ILogger _logger;

    public string Id { get; }
    public string Name { get; }
    public bool IsConnected => true;
    public bool Value => _value;

    public SimulatorOutput(string id, string name, ILogger? logger = null)
    {
        Id = id;
        Name = name;
        _logger = logger ?? Log.Logger;
    }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success("Connected"));
    }

    public Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result.Success("Disconnected"));
    }

    public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        _value = false;
        return Task.FromResult(Result.Success("Reset"));
    }

    public Task<Result> WriteAsync(bool value, CancellationToken cancellationToken = default)
    {
        _value = value;
        _logger.Debug("Simulator output {Name} set to {Value}", Name, value);
        return Task.FromResult(Result.Success($"Output set to {value}"));
    }

    public async Task<Result> ToggleAsync(CancellationToken cancellationToken = default)
    {
        return await WriteAsync(!_value, cancellationToken);
    }

    public async Task<Result> PulseAsync(TimeSpan duration, CancellationToken cancellationToken = default)
    {
        await WriteAsync(true, cancellationToken);
        await Task.Delay(duration, cancellationToken);
        await WriteAsync(false, cancellationToken);
        return Result.Success($"Pulsed for {duration.TotalMilliseconds}ms");
    }
}
