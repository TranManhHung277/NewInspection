# Migration Guide - Chuyển Từ Cấu Trúc Cũ Sang Mới

## 📋 TỔNG QUAN

Framework NAutoSuite đã được cải tiến để tự động hóa hoàn toàn việc xử lý IO, buttons, và tower lights. Bạn không còn cần:
- Setup CommonIOHandler thủ công
- Setup BackgroundTaskManager thủ công
- Tạo scan timer riêng
- Gọi StartAll() / StopAll()

**TẤT CẢ đã được tích hợp vào MachineBase!**

---

## 🔄 HƯỚNG DẪN MIGRATION

### ❌ CŨ: Setup thủ công (PickAndPlaceMachine)

```csharp
public class PickAndPlaceMachine : MachineBase
{
    // ❌ Phải khai báo các fields này
    private CommonIOHandler? _commonIOHandler;
    private BackgroundTaskManager? _backgroundTaskManager;
    private Timer? _scanTimer;

    public PickAndPlaceMachine(...) : base(id, name, logger)
    {
        // ...

        // ❌ Phải setup thủ công
        SetupConcurrentTasks();
    }

    // ❌ Phải viết method setup (30+ dòng)
    private void SetupConcurrentTasks()
    {
        // 1. Setup CommonIOHandler
        _commonIOHandler = new CommonIOHandler(
            machine: this,
            ioMap: _ioMap,
            readInputFunc: ReadInputAsync,
            writeOutputFunc: WriteOutputAsync,
            logger: _logger
        );

        // 2. Setup BackgroundTaskManager
        _backgroundTaskManager = new BackgroundTaskManager(_logger);

        // 3. Register CommonIOScan task
        _backgroundTaskManager.RegisterTask("CommonIOScan", async ct =>
        {
            await _commonIOHandler!.ScanAsync(ct);
        }, intervalMs: 100);

        // 4. Register custom tasks
        _backgroundTaskManager.RegisterTask("FeederMonitor", FeederMonitorAsync, intervalMs: 50);
    }

    // ❌ Phải viết 2 helper methods
    private async Task<bool> ReadInputAsync(string address, CancellationToken ct)
    {
        if (_plc == null) return false;
        var result = await _plc.ReadBitAsync(address, ct);
        return result.IsSuccess && result.Value;
    }

    private async Task WriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        if (_plc == null) return;
        await _plc.WriteBitAsync(address, value, ct);
    }

    // ❌ Phải nhớ start background tasks
    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        // Phải start thủ công!
        _backgroundTaskManager?.StartAll();

        // Connect hardware...
    }
}
```

### ✅ MỚI: Tự động (chỉ 3 methods override)

```csharp
public class PickAndPlaceMachine : MachineBase
{
    // ✅ KHÔNG CẦN khai báo CommonIOHandler, BackgroundTaskManager

    public PickAndPlaceMachine(...) : base(id, name, logger)
    {
        // ✅ Chỉ cần init IOMap và Data
        _ioMap = new PickAndPlaceIOMap();
        _data = new PickAndPlaceData();

        // ✅ KHÔNG CẦN SetupConcurrentTasks()!
    }

    #region MachineBase Overrides - CHỈ 3 METHODS

    // ✅ 1. Provide IO Map
    protected override IOMap? GetIOMap() => _ioMap;

    // ✅ 2. Read input handler
    protected override async Task<bool> OnReadInputAsync(string address, CancellationToken ct)
    {
        if (_plc == null) return false;
        var result = await _plc.ReadBitAsync(address, ct);
        return result.IsSuccess && result.Value;
    }

    // ✅ 3. Write output handler
    protected override async Task OnWriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        if (_plc == null) return;
        await _plc.WriteBitAsync(address, value, ct);
    }

    #endregion

    // ✅ [TÙY CHỌN] Register custom background tasks
    protected override void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
    {
        // Register feeder monitor nếu cần
        taskManager.RegisterTask("FeederMonitor", async ct =>
        {
            bool hasPart = await OnReadInputAsync(_ioMap.MachineInputs.PartSensorAtPick, ct);
            if (hasPart)
            {
                // Handle part
            }
        }, intervalMs: 50);
    }

    // ✅ KHÔNG CẦN start background tasks - MachineBase tự động!
    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        // Chỉ cần connect hardware
        // Background tasks tự động start!
    }
}
```

---

## 📝 BƯỚC MIGRATION CHI TIẾT

### Bước 1: Xóa các fields không cần thiết

**XÓA:**
```csharp
❌ private CommonIOHandler? _commonIOHandler;
❌ private BackgroundTaskManager? _backgroundTaskManager;
❌ private Timer? _scanTimer;
```

**GIỮ LẠI:** (nếu cần dùng trực tiếp)
```csharp
✅ private SensorWaiter? _sensorWaiter;  // Nếu dùng WaitForSensorAsync()
```

---

### Bước 2: Xóa method SetupConcurrentTasks()

**XÓA TOÀN BỘ:**
```csharp
❌ private void SetupConcurrentTasks()
   {
       _commonIOHandler = new CommonIOHandler(...);
       _backgroundTaskManager = new BackgroundTaskManager(...);
       _backgroundTaskManager.RegisterTask(...);
   }
```

---

### Bước 3: Chuyển ReadInputAsync & WriteOutputAsync thành overrides

**CŨ:**
```csharp
❌ private async Task<bool> ReadInputAsync(string address, CancellationToken ct)
   {
       if (_plc == null) return false;
       var result = await _plc.ReadBitAsync(address, ct);
       return result.IsSuccess && result.Value;
   }

❌ private async Task WriteOutputAsync(string address, bool value, CancellationToken ct)
   {
       if (_plc == null) return;
       await _plc.WriteBitAsync(address, value, ct);
   }
```

**MỚI:**
```csharp
✅ protected override async Task<bool> OnReadInputAsync(string address, CancellationToken ct)
   {
       if (_plc == null) return false;
       var result = await _plc.ReadBitAsync(address, ct);
       return result.IsSuccess && result.Value;
   }

✅ protected override async Task OnWriteOutputAsync(string address, bool value, CancellationToken ct)
   {
       if (_plc == null) return;
       await _plc.WriteBitAsync(address, value, ct);
   }
```

**Thay đổi:**
- `private` → `protected override`
- `ReadInputAsync` → `OnReadInputAsync`
- `WriteOutputAsync` → `OnWriteOutputAsync`

---

### Bước 4: Thêm GetIOMap() override

**THÊM METHOD MỚI:**
```csharp
✅ protected override IOMap? GetIOMap() => _ioMap;
```

**Lưu ý:** Method này bắt buộc để MachineBase biết cần scan IO nào!

---

### Bước 5: Chuyển custom background tasks

**CŨ:**
```csharp
❌ private void SetupConcurrentTasks()
   {
       // ...
       _backgroundTaskManager.RegisterTask("FeederMonitor", FeederMonitorAsync, intervalMs: 50);
   }

   private async Task FeederMonitorAsync(CancellationToken ct)
   {
       // Logic
   }
```

**MỚI:**
```csharp
✅ protected override void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
   {
       taskManager.RegisterTask("FeederMonitor", async ct =>
       {
           // Logic (inline hoặc gọi method riêng)
       }, intervalMs: 50);
   }
```

---

### Bước 6: Xóa gọi StartAll() / StopAll()

**XÓA:**
```csharp
❌ protected override async Task OnInitializingAsync()
   {
       await base.OnInitializingAsync();

       // XÓA dòng này!
       _backgroundTaskManager?.StartAll();  ❌

       // Connect hardware...
   }
```

**MỚI:**
```csharp
✅ protected override async Task OnInitializingAsync()
   {
       await base.OnInitializingAsync();

       // KHÔNG CẦN StartAll() - tự động!

       // Chỉ connect hardware
       if (_axisX != null)
       {
           await _axisX.ConnectAsync();
       }
   }
```

---

### Bước 7: Xóa Constructor calls

**XÓA trong constructor:**
```csharp
❌ SetupConcurrentTasks();  // Xóa!
```

---

## 🎯 CHECKLIST MIGRATION

- [ ] Xóa fields: `_commonIOHandler`, `_backgroundTaskManager`, `_scanTimer`
- [ ] Xóa method: `SetupConcurrentTasks()`
- [ ] Chuyển `ReadInputAsync()` → `OnReadInputAsync()` với `protected override`
- [ ] Chuyển `WriteOutputAsync()` → `OnWriteOutputAsync()` với `protected override`
- [ ] Thêm `protected override IOMap? GetIOMap() => _ioMap;`
- [ ] Chuyển custom tasks → `OnRegisterBackgroundTasks()`
- [ ] Xóa gọi `StartAll()` trong `OnInitializingAsync()`
- [ ] Xóa gọi `SetupConcurrentTasks()` trong constructor
- [ ] Test: Initialize, Start, EMG, Stop, Reset
- [ ] Verify: Tower lights tự động đổi màu
- [ ] Verify: Background tasks vẫn chạy

---

## 🔍 SO SÁNH TRƯỚC/SAU

| Aspect | CŨ | MỚI |
|--------|-----|-----|
| **Fields** | 3+ fields (CommonIOHandler, BackgroundTaskManager, Timer) | 0 fields cần thiết |
| **Setup methods** | SetupConcurrentTasks() (30+ dòng) | Không cần |
| **Helper methods** | ReadInputAsync(), WriteOutputAsync() (private) | OnReadInputAsync(), OnWriteOutputAsync() (override) |
| **Background tasks** | RegisterTask trong SetupConcurrentTasks() | RegisterTask trong OnRegisterBackgroundTasks() |
| **Start/Stop** | Phải gọi StartAll() / StopAll() thủ công | Tự động |
| **GetIOMap()** | Không có | Thêm override |
| **Dòng code** | ~100 dòng setup | ~20 dòng (3 overrides) |

---

## 📦 VÍ DỤ HOÀN CHỈNH

### File cũ: OldMachine.cs (~150 dòng)

```csharp
public class OldMachine : MachineBase
{
    private readonly IOldIOMap _ioMap;
    private readonly IPlc? _plc;

    private CommonIOHandler? _commonIOHandler;
    private BackgroundTaskManager? _backgroundTaskManager;
    private Timer? _scanTimer;

    public OldMachine(...) : base(id, name, logger)
    {
        _ioMap = new OldIOMap();
        _plc = plc;

        SetupConcurrentTasks();  // ❌
    }

    private void SetupConcurrentTasks()  // ❌ ~30 dòng
    {
        _commonIOHandler = new CommonIOHandler(
            machine: this,
            ioMap: _ioMap,
            readInputFunc: ReadInputAsync,
            writeOutputFunc: WriteOutputAsync,
            logger: _logger
        );

        _backgroundTaskManager = new BackgroundTaskManager(_logger);

        _backgroundTaskManager.RegisterTask("CommonIOScan", async ct =>
        {
            await _commonIOHandler!.ScanAsync(ct);
        }, intervalMs: 100);
    }

    private async Task<bool> ReadInputAsync(string address, CancellationToken ct)  // ❌
    {
        if (_plc == null) return false;
        var result = await _plc.ReadBitAsync(address, ct);
        return result.IsSuccess && result.Value;
    }

    private async Task WriteOutputAsync(string address, bool value, CancellationToken ct)  // ❌
    {
        if (_plc == null) return;
        await _plc.WriteBitAsync(address, value, ct);
    }

    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();
        _backgroundTaskManager?.StartAll();  // ❌
        // ...
    }

    protected override async Task OnRunningAsync()
    {
        // AUTO logic
    }
}
```

### File mới: NewMachine.cs (~80 dòng)

```csharp
public class NewMachine : MachineBase
{
    private readonly NewIOMap _ioMap;
    private readonly IPlc? _plc;

    public NewMachine(...) : base(id, name, logger)
    {
        _ioMap = new NewIOMap();
        _plc = plc;
        // ✅ KHÔNG CẦN setup gì!
    }

    // ✅ 3 overrides bắt buộc
    protected override IOMap? GetIOMap() => _ioMap;

    protected override async Task<bool> OnReadInputAsync(string address, CancellationToken ct)
    {
        if (_plc == null) return false;
        var result = await _plc.ReadBitAsync(address, ct);
        return result.IsSuccess && result.Value;
    }

    protected override async Task OnWriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        if (_plc == null) return;
        await _plc.WriteBitAsync(address, value, ct);
    }

    // ✅ [Optional] Custom background tasks
    protected override void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
    {
        // Register nếu cần
    }

    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();
        // ✅ KHÔNG CẦN StartAll()!
        // ...
    }

    protected override async Task OnRunningAsync()
    {
        // AUTO logic (giống cũ)
    }
}
```

**Kết quả:**
- Giảm ~70 dòng code
- Đơn giản hóa 50%
- Không còn boilerplate
- Dễ đọc, dễ maintain

---

## ⚠️ LƯU Ý QUAN TRỌNG

### 1. Thay đổi Log.Logger → _logger

Framework mới expose `_logger` từ MachineBase, nên bạn có thể dùng trực tiếp:

**CŨ:**
```csharp
❌ Log.Logger.Information("Message");
```

**MỚI:**
```csharp
✅ _logger.Information("Message");
```

### 2. CommonIOScan đã tự động

Bạn KHÔNG CẦN register task "CommonIOScan" nữa - MachineBase tự làm!

**XÓA:**
```csharp
❌ _backgroundTaskManager.RegisterTask("CommonIOScan", async ct =>
   {
       await _commonIOHandler!.ScanAsync(ct);
   }, intervalMs: 100);
```

### 3. GetIOMap() bắt buộc!

Nếu không override `GetIOMap()`, CommonIOHandler sẽ không chạy!

**BẮT BUỘC:**
```csharp
✅ protected override IOMap? GetIOMap() => _ioMap;
```

### 4. Background tasks vẫn hoạt động

Custom background tasks của bạn vẫn chạy song song - chỉ cần chuyển sang `OnRegisterBackgroundTasks()`:

```csharp
protected override void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
{
    taskManager.RegisterTask("YourTask", YourTaskAsync, intervalMs: 50);
}
```

---

## ✅ TESTING SAU KHI MIGRATE

### 1. Test cơ bản

```csharp
var machine = new YourMachine(...);

// 1. Initialize
await machine.InitializeAsync();
// ✅ Verify: IO scan cycle started
// ✅ Verify: Background tasks started

// 2. Start
await machine.StartAsync();
// ✅ Verify: Tower light GREEN
// ✅ Verify: OnRunningAsync() được gọi

// 3. EMG (nhấn button hoặc gọi API)
await machine.EmergencyStopAsync();
// ✅ Verify: Tower light RED blinking
// ✅ Verify: Buzzer ON
// ✅ Verify: Machine stopped

// 4. Reset
await machine.ResetAsync();
// ✅ Verify: Tower light YELLOW
// ✅ Verify: Buzzer OFF

// 5. Cleanup
await machine.DisposeAsync();
// ✅ Verify: IO scan cycle stopped
// ✅ Verify: Background tasks stopped
```

### 2. Test tower lights

Kiểm tra tower lights tự động đổi màu:

| Action | Expected Light |
|--------|---------------|
| InitializeAsync() | Yellow blinking |
| Initialize complete | Yellow solid |
| StartAsync() | Green solid |
| PauseAsync() | Green blinking |
| EmergencyStopAsync() | Red blinking + Buzzer |
| ResetAsync() | Yellow solid |

### 3. Test buttons (nếu có hardware)

- Nhấn EMG → Machine dừng ngay lập tức
- Nhấn Start (khi Idle) → Machine chạy
- Nhấn Stop (khi Running) → Machine dừng
- Nhấn Reset → Clear alarms

---

## 🎓 KẾT LUẬN

**Migration này mang lại:**
- ✅ Giảm 50-70% boilerplate code
- ✅ Không còn lo setup CommonIOHandler, BackgroundTaskManager
- ✅ Không còn lo start/stop tasks
- ✅ Code sạch hơn, dễ đọc hơn
- ✅ Dễ maintain, test
- ✅ Ít bug hơn (logic chung ở MachineBase)

**Thời gian migration:**
- Dự án nhỏ (< 500 dòng): **15-30 phút**
- Dự án vừa (500-2000 dòng): **1-2 giờ**
- Dự án lớn (> 2000 dòng): **2-4 giờ**

**Bắt đầu migration ngay!** 🚀
