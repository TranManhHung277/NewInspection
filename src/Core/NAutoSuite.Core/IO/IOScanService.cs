using NAutoSuite.Core.Common;
using NAutoSuite.Core.Hardware;

namespace NAutoSuite.Core.IO;

/// <summary>
/// Scan service that updates the IO image from registered hardware and flushes outputs.
/// </summary>
public class IOScanService
{
    private readonly HardwareManager _hardwareManager;
    private readonly IOImage _ioImage;
    private readonly IReadOnlyList<string> _inputAddresses;
    private readonly IReadOnlyList<string> _outputAddresses;
    private readonly IReadOnlyDictionary<string, bool> _defaultValues;

    public IOScanService(
        IOMap ioMap,
        HardwareManager hardwareManager,
        IOImage ioImage,
        IReadOnlyDictionary<string, bool>? defaultValues = null)
    {
        _hardwareManager = hardwareManager ?? throw new ArgumentNullException(nameof(hardwareManager));
        _ioImage = ioImage ?? throw new ArgumentNullException(nameof(ioImage));
        _defaultValues = defaultValues ?? new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        if (ioMap == null) throw new ArgumentNullException(nameof(ioMap));

        _inputAddresses = ioMap.GetInputAddresses();
        _outputAddresses = ioMap.GetOutputAddresses();

        _ioImage.EnsureInputs(_inputAddresses);
        _ioImage.EnsureOutputs(_outputAddresses);

        foreach (var address in _inputAddresses)
        {
            if (_defaultValues.TryGetValue(address, out var value))
            {
                _ioImage.SetInput(address, value);
            }
        }

        foreach (var address in _outputAddresses)
        {
            if (_defaultValues.TryGetValue(address, out var value))
            {
                _ioImage.SetOutput(address, value);
            }
        }
    }

    public async Task UpdateInputsAsync(CancellationToken ct = default)
    {
        if (_inputAddresses.Count == 0) return;

        foreach (var address in _inputAddresses)
        {
            var result = await ReadInputAsync(address, ct);
            if (result.IsSuccess)
            {
                _ioImage.SetInput(address, result.Value);
            }
            else if (_defaultValues.TryGetValue(address, out var fallback))
            {
                _ioImage.SetInput(address, fallback);
            }
        }
    }

    public async Task FlushOutputsAsync(CancellationToken ct = default)
    {
        if (_outputAddresses.Count == 0) return;

        foreach (var address in _outputAddresses)
        {
            var value = _ioImage.GetOutput(address);
            await WriteOutputAsync(address, value, ct);
        }
    }

    private async Task<Result<bool>> ReadInputAsync(string address, CancellationToken ct)
    {
        foreach (var io in _hardwareManager.IODevices)
        {
            var result = await io.ReadInputAsync(address, ct);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Failure<bool>($"No IO device available for input {address}");
    }

    private async Task<Result> WriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        foreach (var io in _hardwareManager.IODevices)
        {
            var result = await io.WriteOutputAsync(address, value, ct);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Failure($"No IO device available for output {address}");
    }
}
