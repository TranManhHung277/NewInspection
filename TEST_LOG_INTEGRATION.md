# Test Log Integration - Debug Guide

## Problem
Logs không hiển thị trong LogPanel khi chạy app.

## Possible Causes

### 1. **Timing Issue** (Most Likely)
LogPanel được tạo AFTER các logs đã được emit:

```
Timeline:
[0ms]  App.OnStartup() → InitializeLogging() → Log.Information("Logging initialized")
[100ms] Host.StartAsync()
[200ms] MainWindow.Show()
[300ms] User clicks "Log" tab → LogView created → LogPanel created → Subscribe to UILogService
       ❌ BUT logs from [0ms] đã bị miss!
```

**Solution**: LogPanel chỉ nhận logs TỪ LÚC NÓ SUBSCRIBE. Các logs trước đó bị mất.

### 2. **Event Not Firing**
UILogService.LogReceived event không fire.

**Debug**: Thêm breakpoint trong UISink.Emit() và LogPanelViewModel.OnLogReceived()

### 3. **Dispatcher Issue**
Application.Current là null khi AddLog được gọi.

**Debug**: Check Application.Current?.Dispatcher

## Quick Test

### Test 1: Check if LogPanel is created
Khi chạy app, LogPanel constructor thêm 1 test log:
```csharp
AddLog(DateTime.Now, "Information", "LogPanel initialized and ready to receive logs", null);
```

✅ **Expected**: Tab Log hiển thị 1 entry "LogPanel initialized..."
❌ **If not showing**: LogPanel không được khởi tạo hoặc UI binding bị lỗi

### Test 2: Generate new logs after LogPanel is ready
Click Initialize button → Sẽ tạo nhiều logs từ LeadshineMaster và LeadshineAxis

✅ **Expected**: Logs xuất hiện trong LogPanel
❌ **If not showing**: Event subscription bị lỗi

## Solutions

### Solution 1: Cache Initial Logs (Recommended)
Modify UILogService để cache logs cho đến khi LogPanel subscribe:

```csharp
public class UILogService
{
    private readonly Queue<(DateTime, string, string, string?)> _cachedLogs = new();
    private const int MaxCachedLogs = 100;

    public event Action<DateTime, string, string, string?>? LogReceived
    {
        add
        {
            // When subscriber added, replay cached logs
            lock (_cachedLogs)
            {
                foreach (var log in _cachedLogs)
                {
                    value?.Invoke(log.Item1, log.Item2, log.Item3, log.Item4);
                }
            }
            _logReceived += value;
        }
        remove => _logReceived -= value;
    }

    private event Action<DateTime, string, string, string?>? _logReceived;

    public void AddLog(DateTime timestamp, string level, string message, string? exception = null)
    {
        // Cache log
        lock (_cachedLogs)
        {
            _cachedLogs.Enqueue((timestamp, level, message, exception));
            while (_cachedLogs.Count > MaxCachedLogs)
                _cachedLogs.Dequeue();
        }

        // Fire event
        _logReceived?.Invoke(timestamp, level, message, exception);
    }
}
```

### Solution 2: Create LogPanel Earlier
Create LogPanel trong MainWindow constructor, BEFORE showing window:

```csharp
public MainWindow(MainViewModel viewModel)
{
    InitializeComponent();

    _viewModel = viewModel;
    DataContext = _viewModel;

    // Create all views IMMEDIATELY (not lazy)
    _autoView = new AutoView { DataContext = _viewModel };
    _manualView = new ManualView { DataContext = _viewModel };
    _settingView = new SettingView { DataContext = _viewModel };
    _dataView = new DataView { DataContext = _viewModel };
    _logView = new LogView(); // 👈 LogPanel subscribes NOW

    // Show Auto view by default
    _viewModel.NavigateToView(_autoView);
}
```

✅ This is ALREADY implemented! So logs AFTER MainWindow creation should appear.

### Solution 3: Add Test Button
Add button để generate test logs:

```xml
<Button Content="Test Logs" Command="{Binding TestLogsCommand}"/>
```

```csharp
[RelayCommand]
private void TestLogs()
{
    Log.Debug("This is a DEBUG message");
    Log.Information("This is an INFO message");
    Log.Warning("This is a WARNING message");
    Log.Error("This is an ERROR message");
}
```

## Verification Steps

1. **Run app**
2. **Click Log tab** immediately
3. **Check for**: "LogPanel initialized and ready to receive logs"
4. **If YES**: LogPanel UI is working, but missed early logs
5. **If NO**: LogPanel not created or UI binding issue

6. **Click Initialize button** (on Auto tab)
7. **Go back to Log tab**
8. **Check for**: Logs from LeadshineMaster.ConnectAsync() and LeadshineAxis.ConnectAsync()
9. **If YES**: ✅ Log system working! Early logs were just missed.
10. **If NO**: ❌ Event subscription broken

## Current Implementation Status

✅ UISink configured in App.xaml.cs
✅ UILogService singleton
✅ LogPanelViewModel auto-subscribes
✅ LogView uses LogPanel
✅ MainWindow creates LogView in constructor

⚠️ **Known Issue**: Logs emitted BEFORE MainWindow constructor will be lost
- Log.Information("Logging initialized with UI sink") → LOST
- Log.Information("Pick and Place Machine Application started") → LOST

✅ **Works For**: Logs AFTER MainWindow created
- LeadshineMaster.ConnectAsync() logs → ✅ Should appear
- LeadshineAxis.ConnectAsync() logs → ✅ Should appear
- Any logs after app fully loaded → ✅ Should appear

## Recommended Fix

Implement **Solution 1** (Cache Initial Logs) để không mất logs từ app startup.
