using Serilog;

namespace NAutoSuite.Core.Machine;

/// <summary>
/// Quản lý các background tasks chạy song song với AUTO sequence
/// VÍ DỤ:
/// - Task 1 (Scan Cycle): Quét IO, xử lý buttons, đèn tháp
/// - Task 2 (AUTO): Chạy sequence chính (pick, place, etc.)
/// - Task 3 (Feeder Monitor): Theo dõi và gạt linh kiện vào vị trí
/// - Task 4 (Conveyor Monitor): Theo dõi băng tải đầu vào
///
/// TẤT CẢ chạy song song, không block nhau!
/// </summary>
public class BackgroundTaskManager : IDisposable
{
    private readonly ILogger _logger;
    private readonly List<BackgroundTask> _tasks = new();
    private readonly CancellationTokenSource _cts = new();
    private bool _isRunning;

    public BackgroundTaskManager(ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
    }

    /// <summary>
    /// Đăng ký một background task
    /// </summary>
    /// <param name="name">Tên task (để debug)</param>
    /// <param name="taskFunc">Function chạy liên tục</param>
    /// <param name="intervalMs">Chu kỳ chạy (ms)</param>
    public void RegisterTask(string name, Func<CancellationToken, Task> taskFunc, int intervalMs = 100)
    {
        _tasks.Add(new BackgroundTask
        {
            Name = name,
            TaskFunc = taskFunc,
            IntervalMs = intervalMs
        });

        _logger.Information("Registered background task: {TaskName} (Interval: {Interval}ms)", name, intervalMs);
    }

    /// <summary>
    /// Start tất cả background tasks
    /// </summary>
    public void StartAll()
    {
        if (_isRunning)
        {
            _logger.Warning("Background tasks already running");
            return;
        }

        _isRunning = true;
        _logger.Information("Starting {Count} background tasks", _tasks.Count);

        foreach (var task in _tasks)
        {
            _ = RunTaskAsync(task, _cts.Token);
        }
    }

    /// <summary>
    /// Stop tất cả background tasks
    /// </summary>
    public void StopAll()
    {
        if (!_isRunning) return;

        _logger.Information("Stopping all background tasks");
        _isRunning = false;
        _cts.Cancel();
    }

    /// <summary>
    /// Chạy một task trong background loop
    /// </summary>
    private async Task RunTaskAsync(BackgroundTask task, CancellationToken ct)
    {
        _logger.Information("Background task started: {TaskName}", task.Name);

        try
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    // Chạy task
                    await task.TaskFunc(ct);

                    // Chờ interval trước khi chạy lại
                    await Task.Delay(task.IntervalMs, ct);
                }
                catch (OperationCanceledException)
                {
                    // Normal cancellation
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in background task {TaskName}", task.Name);
                    // Không break, tiếp tục chạy
                    await Task.Delay(1000, ct); // Delay 1s trước khi retry
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when stopping
        }

        _logger.Information("Background task stopped: {TaskName}", task.Name);
    }

    public void Dispose()
    {
        StopAll();
        _cts.Dispose();
    }

    private class BackgroundTask
    {
        public string Name { get; set; } = string.Empty;
        public Func<CancellationToken, Task> TaskFunc { get; set; } = null!;
        public int IntervalMs { get; set; }
    }
}

/// <summary>
/// Helper class để chờ sensor với timeout mà KHÔNG block scan cycle
/// </summary>
public class SensorWaiter
{
    private readonly ILogger _logger;

    public SensorWaiter(ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
    }

    /// <summary>
    /// Chờ sensor ON với timeout
    /// Method này KHÔNG block scan cycle vì dùng Task.Delay thay vì Thread.Sleep
    /// </summary>
    /// <param name="sensorName">Tên sensor (để log)</param>
    /// <param name="readFunc">Function đọc sensor</param>
    /// <param name="timeoutMs">Timeout (ms)</param>
    /// <param name="ct">CancellationToken</param>
    /// <returns>True nếu sensor ON, False nếu timeout</returns>
    public async Task<bool> WaitForSensorAsync(
        string sensorName,
        Func<Task<bool>> readFunc,
        int timeoutMs = 5000,
        CancellationToken ct = default)
    {
        _logger.Debug("Waiting for sensor: {SensorName} (Timeout: {Timeout}ms)", sensorName, timeoutMs);

        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
        {
            if (ct.IsCancellationRequested)
            {
                _logger.Warning("Wait for sensor {SensorName} cancelled", sensorName);
                return false;
            }

            try
            {
                bool value = await readFunc();
                if (value)
                {
                    _logger.Debug("Sensor {SensorName} detected (after {Duration}ms)",
                        sensorName, (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reading sensor {SensorName}", sensorName);
            }

            // Delay ngắn trước khi đọc lại (KHÔNG block các task khác)
            await Task.Delay(50, ct);
        }

        _logger.Warning("Timeout waiting for sensor {SensorName} (Timeout: {Timeout}ms)", sensorName, timeoutMs);
        return false;
    }

    /// <summary>
    /// Chờ sensor OFF với timeout
    /// </summary>
    public async Task<bool> WaitForSensorOffAsync(
        string sensorName,
        Func<Task<bool>> readFunc,
        int timeoutMs = 5000,
        CancellationToken ct = default)
    {
        _logger.Debug("Waiting for sensor OFF: {SensorName} (Timeout: {Timeout}ms)", sensorName, timeoutMs);

        var startTime = DateTime.UtcNow;

        while ((DateTime.UtcNow - startTime).TotalMilliseconds < timeoutMs)
        {
            if (ct.IsCancellationRequested)
            {
                _logger.Warning("Wait for sensor OFF {SensorName} cancelled", sensorName);
                return false;
            }

            try
            {
                bool value = await readFunc();
                if (!value) // Chờ sensor OFF
                {
                    _logger.Debug("Sensor {SensorName} OFF (after {Duration}ms)",
                        sensorName, (DateTime.UtcNow - startTime).TotalMilliseconds);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error reading sensor {SensorName}", sensorName);
            }

            await Task.Delay(50, ct);
        }

        _logger.Warning("Timeout waiting for sensor OFF {SensorName} (Timeout: {Timeout}ms)", sensorName, timeoutMs);
        return false;
    }
}
