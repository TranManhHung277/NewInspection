using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.Common;
using NAutoSuite.Hardware.Abstractions.PLC;
using Serilog;

namespace NAutoSuite.Hardware.Keyence;

public sealed class KeyencePlc : IPlc, IIO, IRegisterIO, IDisposable
{
    private const int ConnectionTimeoutMs = 3000;
    private const int ReadTimeoutMs = 2000;
    private const int WriteTimeoutMs = 2000;

    private readonly object _commandLock = new();
    private readonly System.Timers.Timer _reconnectTimer;
    private readonly ILogger _logger;
    private readonly string _ip;
    private readonly int _port;

    private TcpClient? _client;
    private NetworkStream? _stream;
    private bool _isReconnecting;
    private bool _disposed;

    public KeyencePlc(string ip, int port, string name, ILogger logger)
    {
        _ip = ip;
        _port = port;
        Name = name;
        Id = $"keyence:{_ip}:{_port}";
        _logger = logger;

        _reconnectTimer = new System.Timers.Timer(3000)
        {
            AutoReset = false
        };
        _reconnectTimer.Elapsed += (_, _) => Reconnect();
    }

    public string Id { get; }

    public string Name { get; }

    public bool IsConnected { get; private set; }

    public Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                Open();
                return IsConnected
                    ? Result.Success("PLC connected")
                    : Result.Failure("PLC connection failed");
            }
            catch (Exception ex)
            {
                return Result.Failure($"PLC connection failed: {ex.Message}", ex);
            }
        }, cancellationToken);
    }

    public Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            Close();
            return Result.Success("PLC disconnected");
        }, cancellationToken);
    }

    public Task<Result> ResetAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(async () =>
        {
            await DisconnectAsync(cancellationToken);
            return await ConnectAsync(cancellationToken);
        }, cancellationToken);
    }

    public Task<Result<bool>> ReadInputAsync(string address, CancellationToken cancellationToken = default)
        => ReadBitAsync(address, cancellationToken);

    public Task<Result> WriteOutputAsync(string address, bool value, CancellationToken cancellationToken = default)
        => WriteBitAsync(address, value, cancellationToken);

    public Task<Result<bool>> ReadBitAsync(string address, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var response = SendCommand($"RD {address}");
            return response switch
            {
                "1" => Result.Success(true),
                "0" => Result.Success(false),
                _ => Result.Failure<bool>($"PLC read bit failed: {response}")
            };
        }, cancellationToken);
    }

    public Task<Result> WriteBitAsync(string address, bool value, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var command = value ? $"ST {address}" : $"RS {address}";
            var response = SendCommand(command);
            return ParseResponse(response) == TcpReceiveCmd.Ok
                ? Result.Success()
                : Result.Failure($"PLC write bit failed: {response}");
        }, cancellationToken);
    }

    public Task<Result<short>> ReadWordAsync(string address, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var response = SendCommand($"RD {address}.S");
            if (short.TryParse(response, out var value))
            {
                return Result.Success(value);
            }

            return Result.Failure<short>($"PLC read word failed: {response}");
        }, cancellationToken);
    }

    public Task<Result> WriteWordAsync(string address, short value, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var response = SendCommand($"WR {address}.S {value}");
            return ParseResponse(response) == TcpReceiveCmd.Ok
                ? Result.Success()
                : Result.Failure($"PLC write word failed: {response}");
        }, cancellationToken);
    }

    public Task<Result<int>> ReadDWordAsync(string address, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var response = SendCommand($"RD {address}.L");
            if (int.TryParse(response, out var value))
            {
                return Result.Success(value);
            }

            return Result.Failure<int>($"PLC read dword failed: {response}");
        }, cancellationToken);
    }

    public Task<Result> WriteDWordAsync(string address, int value, CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            var response = SendCommand($"WR {address}.L {value}");
            return ParseResponse(response) == TcpReceiveCmd.Ok
                ? Result.Success()
                : Result.Failure($"PLC write dword failed: {response}");
        }, cancellationToken);
    }

    public Task<Result<float>> ReadFloatAsync(string address, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<float>("PLC float read not supported"));

    public Task<Result> WriteFloatAsync(string address, float value, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure("PLC float write not supported"));

    public Task<Result<byte[]>> ReadBytesAsync(string address, int length, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure<byte[]>("PLC byte read not supported"));

    public Task<Result> WriteBytesAsync(string address, byte[] data, CancellationToken cancellationToken = default)
        => Task.FromResult(Result.Failure("PLC byte write not supported"));

    public async Task<Result<double>> ReadRegisterAsync(string address, CancellationToken cancellationToken = default)
    {
        var (baseAddress, format) = SplitRegisterFormat(address);
        return format switch
        {
            RegisterFormat.DWord => await ToDoubleAsync(ReadDWordAsync(baseAddress, cancellationToken), cancellationToken),
            RegisterFormat.UInt16 => await ToDoubleAsync(ReadUInt16Async(baseAddress, cancellationToken), cancellationToken),
            _ => await ToDoubleAsync(ReadWordAsync(baseAddress, cancellationToken), cancellationToken)
        };
    }

    public async Task<Result> WriteRegisterAsync(string address, double value, CancellationToken cancellationToken = default)
    {
        var (baseAddress, format) = SplitRegisterFormat(address);
        return format switch
        {
            RegisterFormat.DWord => await WriteDWordAsync(baseAddress, Convert.ToInt32(value), cancellationToken),
            RegisterFormat.UInt16 => await WriteUInt16Async(baseAddress, Convert.ToUInt16(value), cancellationToken),
            _ => await WriteWordAsync(baseAddress, Convert.ToInt16(value), cancellationToken)
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _reconnectTimer.Stop();
        _reconnectTimer.Dispose();
        Close();
    }

    private void Open()
    {
        lock (_commandLock)
        {
            Close();
            _client = new TcpClient();
            var connectTask = _client.ConnectAsync(_ip, _port);
            if (!connectTask.Wait(ConnectionTimeoutMs))
            {
                _client.Close();
                _client = null;
                throw new TimeoutException($"PLC connection timeout after {ConnectionTimeoutMs}ms");
            }

            if (!_client.Connected)
            {
                throw new InvalidOperationException("PLC connection failed");
            }

            _stream = _client.GetStream();
            _stream.ReadTimeout = ReadTimeoutMs;
            _stream.WriteTimeout = WriteTimeoutMs;
            IsConnected = true;
        }
    }

    private void Close()
    {
        lock (_commandLock)
        {
            try
            {
                _stream?.Close();
                _client?.Close();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "PLC close error");
            }
            finally
            {
                _stream = null;
                _client = null;
                IsConnected = false;
            }
        }
    }

    private string SendCommand(string command)
    {
        lock (_commandLock)
        {
            if (_stream == null || _client == null || !IsConnected)
            {
                return "EX:PLC is not connected";
            }

            try
            {
                var fullCommand = command.Trim() + "\r";
                var sendBytes = Encoding.ASCII.GetBytes(fullCommand);
                _stream.Write(sendBytes, 0, sendBytes.Length);

                var buffer = new byte[256];
                var bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                {
                    return "EX:Connection closed by PLC";
                }

                var raw = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim();
                var parts = raw.Split(new[] { '\r', '\n', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                {
                    return "EX:NO RESPONSE";
                }

                return parts.Length > 1 ? parts[1] : parts[0];
            }
            catch (Exception ex) when (ex is IOException || ex is SocketException)
            {
                _logger.Warning(ex, "PLC socket error");
                IsConnected = false;
                StartReconnectTimer();
                return $"EX:{ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "PLC command error");
                IsConnected = false;
                StartReconnectTimer();
                return $"EX:{ex.Message}";
            }
        }
    }

    private void StartReconnectTimer()
    {
        if (_isReconnecting || _disposed) return;
        _isReconnecting = true;
        _reconnectTimer.Stop();
        _reconnectTimer.Start();
    }

    private void StopReconnectTimer()
    {
        _isReconnecting = false;
        _reconnectTimer.Stop();
    }

    private void Reconnect()
    {
        if (_disposed) return;

        try
        {
            _isReconnecting = false;
            Open();
            if (IsConnected)
            {
                StopReconnectTimer();
                _logger.Information("PLC reconnected to {Ip}:{Port}", _ip, _port);
            }
            else
            {
                StartReconnectTimer();
            }
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "PLC reconnect failed");
            StartReconnectTimer();
        }
    }

    private Task<Result<ushort>> ReadUInt16Async(string address, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var response = SendCommand($"RD {address}.U");
            if (ushort.TryParse(response, out var value))
            {
                return Result.Success(value);
            }

            return Result.Failure<ushort>($"PLC read ushort failed: {response}");
        }, cancellationToken);
    }

    private Task<Result> WriteUInt16Async(string address, ushort value, CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            var response = SendCommand($"WR {address}.U {value}");
            return ParseResponse(response) == TcpReceiveCmd.Ok
                ? Result.Success()
                : Result.Failure($"PLC write ushort failed: {response}");
        }, cancellationToken);
    }

    private static (string BaseAddress, RegisterFormat Format) SplitRegisterFormat(string address)
    {
        if (address.EndsWith(".L", StringComparison.OrdinalIgnoreCase) ||
            address.EndsWith(".D", StringComparison.OrdinalIgnoreCase))
        {
            return (address[..^2], RegisterFormat.DWord);
        }

        if (address.EndsWith(".U", StringComparison.OrdinalIgnoreCase))
        {
            return (address[..^2], RegisterFormat.UInt16);
        }

        if (address.EndsWith(".S", StringComparison.OrdinalIgnoreCase))
        {
            return (address[..^2], RegisterFormat.Int16);
        }

        return (address, RegisterFormat.Int16);
    }

    private static TcpReceiveCmd ParseResponse(string response)
    {
        if (response == "OK" || response == "CC" || response == "00000") return TcpReceiveCmd.Ok;
        if (response == "E0") return TcpReceiveCmd.E0;
        if (response == "E1") return TcpReceiveCmd.E1;
        if (response.StartsWith("EX:", StringComparison.OrdinalIgnoreCase)) return TcpReceiveCmd.CheckCodeWrong;
        return TcpReceiveCmd.Unknown;
    }

    private enum RegisterFormat
    {
        Int16,
        UInt16,
        DWord
    }

    private enum TcpReceiveCmd
    {
        Ok,
        E0,
        E1,
        Unknown,
        CheckCodeWrong
    }

    private static async Task<Result<double>> ToDoubleAsync<T>(Task<Result<T>> task, CancellationToken ct)
        where T : struct
    {
        var result = await task.WaitAsync(ct);
        return result.IsSuccess
            ? Result.Success(Convert.ToDouble(result.Value, CultureInfo.InvariantCulture))
            : Result.Failure<double>(result.Message, result.Exception);
    }
}
