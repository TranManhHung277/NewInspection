using NAutoSuite.Core.Common;
using NAutoSuite.Core.Hardware;

namespace NAutoSuite.Core.IO;

/// <summary>
/// Scan service that updates the register image from registered hardware and flushes outputs.
/// </summary>
public class RegisterScanService
{
    private readonly HardwareManager _hardwareManager;
    private readonly RegisterImage _image;
    private readonly IReadOnlyList<string> _inputAddresses;
    private readonly IReadOnlyList<string> _outputAddresses;

    public RegisterScanService(RegisterMap registerMap, HardwareManager hardwareManager, RegisterImage image)
    {
        _hardwareManager = hardwareManager ?? throw new ArgumentNullException(nameof(hardwareManager));
        _image = image ?? throw new ArgumentNullException(nameof(image));

        if (registerMap == null) throw new ArgumentNullException(nameof(registerMap));

        _inputAddresses = registerMap.InputRegisters.Values.ToList();
        _outputAddresses = registerMap.OutputRegisters.Values.ToList();

        _image.EnsureInputs(_inputAddresses);
        _image.EnsureOutputs(_outputAddresses);
    }

    public async Task UpdateInputsAsync(CancellationToken ct = default)
    {
        if (_inputAddresses.Count == 0) return;

        foreach (var address in _inputAddresses)
        {
            var result = await ReadRegisterAsync(address, ct);
            if (result.IsSuccess)
            {
                _image.SetInput(address, result.Value);
            }
        }
    }

    public async Task FlushOutputsAsync(CancellationToken ct = default)
    {
        if (_outputAddresses.Count == 0) return;

        foreach (var address in _outputAddresses)
        {
            var value = _image.GetOutput(address);
            await WriteRegisterAsync(address, value, ct);
        }
    }

    private async Task<Result<double>> ReadRegisterAsync(string address, CancellationToken ct)
    {
        foreach (var io in _hardwareManager.RegisterDevices)
        {
            var result = await io.ReadRegisterAsync(address, ct);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Failure<double>($"No register device available for input {address}");
    }

    private async Task<Result> WriteRegisterAsync(string address, double value, CancellationToken ct)
    {
        foreach (var io in _hardwareManager.RegisterDevices)
        {
            var result = await io.WriteRegisterAsync(address, value, ct);
            if (result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Failure($"No register device available for output {address}");
    }
}
