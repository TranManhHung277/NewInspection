using System.Diagnostics;
using System.Threading.Channels;
using Serilog;

namespace NAutoSuite.Core.Services;

/// <summary>
/// Realtime task scheduler for cyclic operations
/// </summary>
public class Scheduler : IDisposable
{
    private readonly ILogger _logger;
    private readonly Channel<Func<Task>> _taskChannel;
    private readonly CancellationTokenSource _cts;
    private Task? _workerTask;
    private readonly PeriodicTimer? _timer;
    private readonly TimeSpan _cycleTime;

    public bool IsRunning { get; private set; }
    public long CycleCount { get; private set; }
    public TimeSpan AverageCycleTime { get; private set; }

    public Scheduler(TimeSpan cycleTime, ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
        _cycleTime = cycleTime;
        _cts = new CancellationTokenSource();
        _taskChannel = Channel.CreateUnbounded<Func<Task>>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        if (cycleTime > TimeSpan.Zero)
        {
            _timer = new PeriodicTimer(cycleTime);
        }
    }

    /// <summary>
    /// Start the scheduler
    /// </summary>
    public void Start(Func<CancellationToken, Task> cyclicTask)
    {
        if (IsRunning) return;

        IsRunning = true;
        CycleCount = 0;

        if (_timer != null)
        {
            _workerTask = RunPeriodicAsync(cyclicTask, _cts.Token);
        }
        else
        {
            _workerTask = RunContinuousAsync(cyclicTask, _cts.Token);
        }

        _logger.Information("Scheduler started with cycle time: {CycleTime}ms", _cycleTime.TotalMilliseconds);
    }

    /// <summary>
    /// Stop the scheduler
    /// </summary>
    public async Task StopAsync()
    {
        if (!IsRunning) return;

        IsRunning = false;
        _cts.Cancel();

        if (_workerTask != null)
        {
            await _workerTask;
        }

        _logger.Information("Scheduler stopped. Total cycles: {CycleCount}", CycleCount);
    }

    private async Task RunPeriodicAsync(Func<CancellationToken, Task> cyclicTask, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var totalTime = TimeSpan.Zero;

        try
        {
            while (await _timer!.WaitForNextTickAsync(cancellationToken))
            {
                var cycleStart = sw.Elapsed;

                try
                {
                    await cyclicTask(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in cyclic task");
                }

                var cycleTime = sw.Elapsed - cycleStart;
                totalTime += cycleTime;
                CycleCount++;
                AverageCycleTime = TimeSpan.FromTicks(totalTime.Ticks / CycleCount);

                if (cycleTime > _cycleTime)
                {
                    _logger.Warning("Cycle time exceeded: {ActualTime}ms > {TargetTime}ms",
                        cycleTime.TotalMilliseconds, _cycleTime.TotalMilliseconds);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Scheduler cancelled");
        }
    }

    private async Task RunContinuousAsync(Func<CancellationToken, Task> cyclicTask, CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        var totalTime = TimeSpan.Zero;

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var cycleStart = sw.Elapsed;

                try
                {
                    await cyclicTask(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error in cyclic task");
                }

                var cycleTime = sw.Elapsed - cycleStart;
                totalTime += cycleTime;
                CycleCount++;
                AverageCycleTime = TimeSpan.FromTicks(totalTime.Ticks / CycleCount);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Information("Scheduler cancelled");
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _cts.Dispose();
    }
}
