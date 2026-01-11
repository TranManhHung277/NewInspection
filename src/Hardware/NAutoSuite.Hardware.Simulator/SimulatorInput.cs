using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using Serilog;

namespace NAutoSuite.Hardware.Simulator;

/// <summary>
/// Simulated digital input
/// </summary>
public class SimulatorInput : IInput
{
    private bool _value;
    private readonly ILogger _logger;

    public string Id { get; }
    public string Name { get; }
    public bool IsConnected => true;
    public bool Value => _value;

    public event EventHandler<bool>? ValueChanged;

    public SimulatorInput(string id, string name, ILogger? logger = null)
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

    public Task<bool> ReadAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_value);
    }

    /// <summary>
    /// Simulate input change (for testing)
    /// </summary>
    public void Simulate(bool value)
    {
        if (_value != value)
        {
            _value = value;
            _logger.Debug("Simulator input {Name} changed to {Value}", Name, value);
            ValueChanged?.Invoke(this, value);
        }
    }
}
