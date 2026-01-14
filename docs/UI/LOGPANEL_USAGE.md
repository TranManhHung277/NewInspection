# LogPanel UserControl - Usage Guide

## Overview

`LogPanel` là một UserControl có thể tái sử dụng để hiển thị system logs với các tính năng:
- ✅ **Filter theo log level** (Debug, Info, Warning, Error, Fatal)
- ✅ **Search/tìm kiếm** logs theo nội dung
- ✅ **Export** logs ra file .txt
- ✅ **Clear** all logs
- ✅ **Tự động giới hạn 1000 records** (xóa logs cũ nhất khi vượt quá)
- ✅ **Real-time display** với color-coded log levels
- ✅ **Thread-safe** với WPF Dispatcher

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     Serilog Logger                       │
│           (configured in App.xaml.cs)                    │
└─────────────────┬───────────────────────────────────────┘
                  │
                  ▼
          ┌───────────────┐
          │    UISink     │  (Custom Serilog sink)
          └───────┬───────┘
                  │
                  ▼
     ┌────────────────────────┐
     │   UILogService         │  (Singleton service)
     │   - Event-based         │
     │   - Multiple subscribers│
     └────────┬───────────────┘
              │
              ▼
  ┌────────────────────────────┐
  │  LogPanelViewModel         │  (Auto-subscribes)
  │  - Filter logic             │
  │  - Export/Clear commands    │
  │  - 1000 entry limit         │
  └────────┬───────────────────┘
           │
           ▼
    ┌─────────────┐
    │  LogPanel   │  (UserControl)
    │  - XAML UI  │
    └─────────────┘
```

## Quick Start

### 1. Add LogPanel to Your View

```xml
<UserControl xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <!-- Simply add the LogPanel control -->
    <controls:LogPanel/>

</UserControl>
```

### 2. Configure Serilog with UISink

In your `App.xaml.cs` or startup code:

```csharp
using Machine.PickAndPlace.Services; // For UISink extension
using Serilog;
using Serilog.Events;

// Initialize Serilog with UI sink
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(LogEventLevel.Debug)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.UISink() // 👈 Add this line to forward logs to UI
    .CreateLogger();
```

### 3. Log Messages Anywhere

```csharp
using Serilog;

// Logs will automatically appear in LogPanel
Log.Debug("This is a debug message");
Log.Information("Application started successfully");
Log.Warning("Configuration file not found, using defaults");
Log.Error("Failed to connect to database: {Error}", ex.Message);
Log.Fatal("Critical system failure!");
```

## Features in Detail

### 1. Log Level Filtering

LogPanel hiển thị checkboxes để filter theo từng log level:
- **Debug** (Gray badge)
- **Information** (Blue badge)
- **Warning** (Orange badge)
- **Error** (Red badge)
- **Fatal** (Dark Red badge)

Mặc định tất cả log levels đều được hiển thị. Uncheck để ẩn level đó.

### 2. Search/Filter

Textbox search cho phép tìm kiếm theo:
- Message content
- Log level name
- Exception text (nếu có)

Search không phân biệt hoa/thường.

### 3. Export to File

Button **Export** sẽ:
1. Mở Save File Dialog
2. Export tất cả **filtered logs** (chỉ logs đang hiển thị)
3. Format: `logs_yyyyMMdd_HHmmss.txt`
4. Content format:
```
===================================
Log Export - 2026-01-14 15:30:45
Total Entries: 125
===================================

[2026-01-14 15:30:12.456] [Information] Application started
[2026-01-14 15:30:15.789] [Warning] Configuration file not found
Exception: System.IO.FileNotFoundException: ...
```

### 4. Clear Logs

Button **Clear** sẽ:
1. Hiển thị confirmation dialog
2. Xóa tất cả logs khỏi UI (không ảnh hưởng đến file logs)

### 5. Automatic 1000 Entry Limit

LogPanelViewModel tự động duy trì maximum 1000 log entries:
```csharp
// When adding new log entry
AllLogs.Add(entry);

// Keep only last 1000 entries
while (AllLogs.Count > MaxLogEntries)
{
    AllLogs.RemoveAt(0); // Remove oldest
}
```

Điều này đảm bảo:
- ✅ Performance tốt với long-running applications
- ✅ Memory usage không tăng không giới hạn
- ✅ UI vẫn responsive

## Advanced Usage

### Using Multiple LogPanels

Bạn có thể sử dụng nhiều `LogPanel` instances trong cùng một application:

```xml
<!-- Main Log View -->
<controls:LogPanel Grid.Row="0"/>

<!-- Diagnostic Panel (in another window/view) -->
<controls:LogPanel Grid.Row="1"/>
```

Mỗi LogPanel sẽ:
- Subscribe độc lập vào `UILogService`
- Có filter settings riêng
- Có 1000 entry buffer riêng

### Customizing Max Entry Limit

Nếu muốn thay đổi limit từ 1000:

```csharp
// Edit LogPanelViewModel.cs
private const int MaxLogEntries = 2000; // Change from 1000 to 2000
```

### Styling the LogPanel

LogPanel sử dụng inline styles, nhưng bạn có thể override:

```xml
<controls:LogPanel>
    <controls:LogPanel.Resources>
        <!-- Override colors -->
        <SolidColorBrush x:Key="InfoColor" Color="#00BCD4"/>
        <SolidColorBrush x:Key="ErrorColor" Color="#FF5252"/>
    </controls:LogPanel.Resources>
</controls:LogPanel>
```

## File Structure

```
src/UI/NAutoSuite.UI.Controls/
├── LogPanel.xaml                    # UI definition
├── LogPanel.xaml.cs                 # Code-behind (minimal)
├── ViewModels/
│   └── LogPanelViewModel.cs         # Full logic (filter, export, clear)
└── Services/
    └── UILogService.cs              # Singleton event broker

projects/Machine.PickAndPlace.Frontend/
└── Services/
    ├── UISink.cs                    # Serilog sink
    └── UISinkExtensions.cs          # .WriteTo.UISink() extension
```

## Benefits

### ✅ Reusable
- Drop-in component cho bất kỳ WPF page nào
- Không cần viết logging UI lại

### ✅ Self-Contained
- ViewModel tự động subscribe vào UILogService
- Không cần manual wiring trong DI

### ✅ Performance
- Automatic 1000 entry limit
- Efficient filtering với ObservableCollection
- Thread-safe với Dispatcher.Invoke

### ✅ User-Friendly
- Filter by level (checkbox)
- Search (real-time)
- Export (filtered logs only)
- Clear (with confirmation)
- Color-coded levels

## Example: Adding to Any View

### Before (Old LogView - tightly coupled)
```xml
<UserControl>
    <!-- Custom log implementation per view -->
    <ItemsControl ItemsSource="{Binding Logs}">
        <!-- 100+ lines of XAML... -->
    </ItemsControl>
</UserControl>
```

### After (New LogPanel - reusable)
```xml
<UserControl xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <!-- One line! -->
    <controls:LogPanel/>

</UserControl>
```

## Troubleshooting

### Logs not appearing in UI

**Check 1**: UISink đã được configure?
```csharp
.WriteTo.UISink() // Must be present
```

**Check 2**: LogPanel có ở trong Visual Tree?
```xml
<controls:LogPanel/> <!-- Must be visible in UI -->
```

**Check 3**: Log level có bị filter?
- Check các checkboxes (Debug, Info, Warning, Error, Fatal)
- Clear search box

### Export file empty

- Export chỉ export **filtered logs** (logs đang hiển thị)
- Check filter checkboxes và search box
- Nếu không có logs hiển thị → file sẽ trống

### Performance issues

- LogPanel tự động giới hạn 1000 entries
- Nếu vẫn chậm, check:
  - Log frequency (quá nhiều logs/second?)
  - UI thread blocking (long operations on UI thread?)

## Migration from Old LogView

### Step 1: Remove old LogViewModel registration
```csharp
// App.xaml.cs - REMOVE
services.AddSingleton<LogViewModel>(); // ❌ Delete this
```

### Step 2: Update LogView.xaml
```xml
<!-- OLD -->
<ItemsControl ItemsSource="{Binding Logs}">...</ItemsControl>

<!-- NEW -->
<controls:LogPanel/>
```

### Step 3: Remove LogViewModel parameter from MainWindow
```csharp
// OLD
public MainWindow(MainViewModel viewModel, LogViewModel logViewModel) // ❌

// NEW
public MainWindow(MainViewModel viewModel) // ✅
```

### Step 4: Update LogView instantiation
```csharp
// OLD
_logView = new LogView { DataContext = _logViewModel }; // ❌

// NEW
_logView = new LogView(); // ✅ LogPanel has its own ViewModel
```

Done! 🎉

## Future Enhancements

Possible improvements for LogPanel:
- [ ] Auto-scroll to bottom option
- [ ] Pause/Resume logging
- [ ] Log level color customization via DependencyProperty
- [ ] Copy selected log entry to clipboard
- [ ] Filter by date/time range
- [ ] Regex search support
- [ ] Dark/Light theme switching
