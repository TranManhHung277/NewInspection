using System.Collections.Concurrent;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using NAutoSuite.Hardware.Abstractions.EtherCAT;

namespace NAutoSuite.Hardware.Simulator;

/// <summary>
/// Simulated Leadshine EtherCAT master with digital IO.
/// </summary>
public class LeadshineEthercatSimulator : IEtherCATMaster, IIO, IHardwareDataProvider
{
    private readonly ConcurrentDictionary<string, bool> _inputs =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, bool> _outputs =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ScheduledSignal> _scheduled = new();
    private readonly object _lock = new();
    private readonly List<CylinderLink> _cylinders = new();
    private readonly List<VacuumLink> _vacuumLinks = new();
    private readonly Random _random = new();
    private bool _testHeadEnabled;
    private string _testPartPresent = "IX1.0";
    private string _testOk = "IX1.1";
    private string _testNg = "IX1.2";
    private int _testCycleMs = 1500;
    private int _testPulseMs = 300;
    private double _testOkProbability = 0.7;
    private DateTime _nextTestAt = DateTime.UtcNow;

    public string Id { get; }
    public string Name { get; }
    public bool IsConnected { get; private set; }
    public int SlaveCount { get; private set; }
    public bool IsOperational => IsConnected;

    public LeadshineEthercatSimulator(string id = "SIM_ECAT", string name = "Leadshine EtherCAT Simulator")
    {
        Id = id;
        Name = name;
    }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        SlaveCount = 3;
        EnsureDefaultCommonInputs();
        return Task.FromResult(Result.Success("Simulator connected"));
    }

    public Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = false;
        return Task.FromResult(Result.Success("Simulator disconnected"));
    }

    public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        _inputs.Clear();
        _outputs.Clear();
        EnsureDefaultCommonInputs();
        return Task.FromResult(Result.Success("Simulator reset"));
    }

    public Task<Result> ScanAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success("Scan complete"));

    public Task<Result> GoOperationalAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success("Operational"));

    public Task<Result<byte[]>> ReadPDOAsync(int slaveIndex, int offset, int length, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(new byte[length]));

    public Task<Result> WritePDOAsync(int slaveIndex, int offset, byte[] data, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success("PDO written"));

    public Task<Result<SlaveInfo>> GetSlaveInfoAsync(int slaveIndex, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Success(new SlaveInfo
        {
            Index = slaveIndex,
            Name = $"SimSlave{slaveIndex}",
            VendorId = 0,
            ProductCode = 0,
            State = "OP"
        }));

    public Task<Result<bool>> ReadInputAsync(string address, CancellationToken cancellationToken = default)
    {
        var value = _inputs.TryGetValue(address, out var current) && current;
        return Task.FromResult(Result.Success(value));
    }

    public Task<Result> WriteOutputAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        _outputs[address] = value;
        return Task.FromResult(Result.Success("Output updated"));
    }

    public void SetInput(string address, bool value)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        _inputs[address] = value;
    }

    public bool GetInput(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;
        return _inputs.TryGetValue(address, out var value) && value;
    }

    public bool GetOutput(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;
        return _outputs.TryGetValue(address, out var value) && value;
    }

    public void RegisterCylinder(string extendOutput, string extendedInput, string retractedInput, int delayMs = 150)
    {
        _cylinders.Add(new CylinderLink(extendOutput, extendedInput, retractedInput, delayMs));
    }

    public void RegisterVacuum(string vacuumOutput, string vacuumSensor, int delayMs = 120)
    {
        _vacuumLinks.Add(new VacuumLink(vacuumOutput, vacuumSensor, delayMs));
    }

    public void ConfigureTestHead(
        string partPresentInput,
        string okInput,
        string ngInput,
        int cycleMs = 1500,
        double okProbability = 0.7,
        int pulseMs = 300)
    {
        _testPartPresent = partPresentInput;
        _testOk = okInput;
        _testNg = ngInput;
        _testCycleMs = cycleMs;
        _testOkProbability = okProbability;
        _testPulseMs = pulseMs;
        _testHeadEnabled = true;
        _nextTestAt = DateTime.UtcNow.AddMilliseconds(_testCycleMs);
    }

    public Task UpdateAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var cylinder in _cylinders)
        {
            var output = GetOutput(cylinder.ExtendOutput);
            if (output == cylinder.LastOutput) continue;

            cylinder.LastOutput = output;
            if (output)
            {
                ScheduleInput(cylinder.ExtendedInput, true, cylinder.DelayMs);
                ScheduleInput(cylinder.RetractedInput, false, cylinder.DelayMs);
            }
            else
            {
                ScheduleInput(cylinder.ExtendedInput, false, cylinder.DelayMs);
                ScheduleInput(cylinder.RetractedInput, true, cylinder.DelayMs);
            }
        }

        foreach (var vacuum in _vacuumLinks)
        {
            var output = GetOutput(vacuum.VacuumOutput);
            if (output == vacuum.LastOutput) continue;

            vacuum.LastOutput = output;
            ScheduleInput(vacuum.VacuumSensor, output, vacuum.DelayMs);
        }

        if (_testHeadEnabled && now >= _nextTestAt)
        {
            var partPresent = GetInput(_testPartPresent);
            if (partPresent)
            {
                var isOk = _random.NextDouble() <= _testOkProbability;
                ScheduleInput(_testOk, isOk, 0);
                ScheduleInput(_testNg, !isOk, 0);
                ScheduleInput(_testOk, false, _testPulseMs);
                ScheduleInput(_testNg, false, _testPulseMs);
            }

            _nextTestAt = now.AddMilliseconds(_testCycleMs);
        }

        List<ScheduledSignal> due;
        lock (_lock)
        {
            due = _scheduled.Where(s => s.DueAt <= now).ToList();
            _scheduled.RemoveAll(s => s.DueAt <= now);
        }

        foreach (var signal in due)
        {
            _inputs[signal.Address] = signal.Value;
        }

        return Task.CompletedTask;
    }

    private void ScheduleInput(string address, bool value, int delayMs)
    {
        if (string.IsNullOrWhiteSpace(address)) return;
        lock (_lock)
        {
            _scheduled.Add(new ScheduledSignal(address, value, DateTime.UtcNow.AddMilliseconds(delayMs)));
        }
    }

    private void EnsureDefaultCommonInputs()
    {
        _inputs.TryAdd("IX0.0", false); // EMG
        _inputs.TryAdd("IX0.1", false); // Start
        _inputs.TryAdd("IX0.2", false); // Stop
        _inputs.TryAdd("IX0.3", false); // Reset
        _inputs.TryAdd("IX0.4", false); // Safety door open
        _inputs.TryAdd("IX0.5", true);  // Air pressure OK
    }

    private sealed class ScheduledSignal
    {
        public ScheduledSignal(string address, bool value, DateTime dueAt)
        {
            Address = address;
            Value = value;
            DueAt = dueAt;
        }

        public string Address { get; }
        public bool Value { get; }
        public DateTime DueAt { get; }
    }

    private sealed class CylinderLink
    {
        public CylinderLink(string extendOutput, string extendedInput, string retractedInput, int delayMs)
        {
            ExtendOutput = extendOutput;
            ExtendedInput = extendedInput;
            RetractedInput = retractedInput;
            DelayMs = delayMs;
        }

        public string ExtendOutput { get; }
        public string ExtendedInput { get; }
        public string RetractedInput { get; }
        public int DelayMs { get; }
        public bool LastOutput { get; set; }
    }

    private sealed class VacuumLink
    {
        public VacuumLink(string vacuumOutput, string vacuumSensor, int delayMs)
        {
            VacuumOutput = vacuumOutput;
            VacuumSensor = vacuumSensor;
            DelayMs = delayMs;
        }

        public string VacuumOutput { get; }
        public string VacuumSensor { get; }
        public int DelayMs { get; }
        public bool LastOutput { get; set; }
    }
}
