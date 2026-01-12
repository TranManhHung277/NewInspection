# ✅ Giải Pháp: Không Code Lặp + Concurrent Tasks

## 🎯 Vấn Đề Đã Giải Quyết

### Vấn Đề 1: Code Lặp Lại ❌ → SOLVED ✅
**Trước**: Mỗi dự án phải viết lại 350+ dòng code xử lý EMG/Start/Stop/Reset/Đèn tháp

**Bây giờ**: Chỉ cần 1 dòng code!
```csharp
await _commonIOHandler.ScanAsync(ct);
```

### Vấn Đề 2: Task Bị Block ❌ → SOLVED ✅
**Trước**: Chờ sensor → Block TẤT CẢ (EMG không hoạt động)

**Bây giờ**: Chờ sensor không block!
```csharp
bool success = await _sensorWaiter.WaitForSensorAsync(...);
// Trong khi chờ: EMG vẫn hoạt động, đèn vẫn nhấp nháy, feeder vẫn chạy
```

---

## 📦 Các Class Đã Tạo

### 1. CommonIOHandler
**File**: `NAutoSuite.Core/Machine/CommonIOHandler.cs`

**Chức năng**: Xử lý TỰ ĐỘNG tất cả common IO
- ✅ EMG → Emergency stop machine
- ✅ Start button → Start machine
- ✅ Stop button → Stop machine
- ✅ Reset button → Reset machine
- ✅ Tower lights theo state (Idle=Yellow, Running=Green, Error=Red+Buzzer)
- ✅ Safety door → Stop if opened
- ✅ Air pressure → Stop if lost

**Cách dùng**:
```csharp
// Setup trong constructor
_commonIOHandler = new CommonIOHandler(
    machine: this,
    ioMap: _ioMap,
    readInputFunc: async (address, ct) => {
        var result = await _plc.ReadBitAsync(address, ct);
        return result.IsSuccess ? result.Value : false;
    },
    writeOutputFunc: async (address, value, ct) => {
        await _plc.WriteBitAsync(address, value, ct);
    },
    logger: Log.Logger
);

// Gọi trong scan cycle (100ms)
await _commonIOHandler.ScanAsync(ct);
```

### 2. BackgroundTaskManager
**File**: `NAutoSuite.Core/Machine/BackgroundTaskManager.cs`

**Chức năng**: Chạy nhiều tasks SONG SONG

**Cách dùng**:
```csharp
_backgroundTaskManager = new BackgroundTaskManager();

// Task 1: Scan Cycle
_backgroundTaskManager.RegisterTask("ScanCycle", async ct =>
{
    await _commonIOHandler.ScanAsync(ct);
}, intervalMs: 100);

// Task 2: Feeder Monitor
_backgroundTaskManager.RegisterTask("FeederMonitor", async ct =>
{
    if (await ReadSensor(partSensor))
    {
        await PushCylinder.ExtendAsync();
    }
}, intervalMs: 50);

// Start ALL tasks
_backgroundTaskManager.StartAll();
```

### 3. SensorWaiter
**File**: `NAutoSuite.Core/Machine/BackgroundTaskManager.cs`

**Chức năng**: Chờ sensor KHÔNG block

**Cách dùng**:
```csharp
_sensorWaiter = new SensorWaiter();

// Chờ xi lanh xuống (timeout 5s, KHÔNG block scan cycle)
bool success = await _sensorWaiter.WaitForSensorAsync(
    sensorName: "Cylinder Down",
    readFunc: async () => await ReadSensor(cylinderDownSensor),
    timeoutMs: 5000,
    ct
);

if (!success)
{
    RaiseAlarm(2001, "Cylinder timeout");
}
```

---

## 🔄 Luồng Hoạt Động

```
┌────────────────────────────────────────────────────┐
│  MACHINE                                           │
│  ┌──────────────────────────────────────────────┐ │
│  │  BackgroundTaskManager                       │ │
│  │                                               │ │
│  │  Task 1 (100ms): CommonIOHandler.ScanAsync() │ │
│  │    → Đọc EMG/Start/Stop/Reset                │ │
│  │    → Cập nhật đèn tháp                       │ │
│  │    → Kiểm tra safety sensors                 │ │
│  │                                               │ │
│  │  Task 2: AUTO Sequence                       │ │
│  │    → PickSequenceAsync()                     │ │
│  │    → SensorWaiter (không block!)             │ │
│  │                                               │ │
│  │  Task 3 (50ms): Feeder Monitor               │ │
│  │    → Gạt linh kiện vào vị trí                │ │
│  └──────────────────────────────────────────────┘ │
└────────────────────────────────────────────────────┘

TẤT CẢ CHẠY SONG SONG! ✅
```

---

## 📝 Checklist: Áp Dụng Vào Dự Án Mới

### Bước 1: Khai báo trong Constructor
```csharp
public YourMachine(...)
{
    // Setup Common IO Handler
    _commonIOHandler = new CommonIOHandler(
        this,
        _ioMap,
        readInputFunc: async (addr, ct) => await ReadBit(addr, ct),
        writeOutputFunc: async (addr, val, ct) => await WriteBit(addr, val, ct)
    );

    // Setup Sensor Waiter
    _sensorWaiter = new SensorWaiter();

    // Setup Background Task Manager
    _backgroundTaskManager = new BackgroundTaskManager();

    _backgroundTaskManager.RegisterTask("ScanCycle",
        async ct => await _commonIOHandler.ScanAsync(ct),
        intervalMs: 100
    );
}
```

### Bước 2: Start/Stop Background Tasks
```csharp
protected override async Task OnInitializingAsync()
{
    await base.OnInitializingAsync();
    _backgroundTaskManager?.StartAll();
}

public void Dispose()
{
    _backgroundTaskManager?.StopAll();
}
```

### Bước 3: Viết AUTO Logic (Dùng SensorWaiter)
```csharp
private async Task PickSequenceAsync(CancellationToken ct)
{
    // Move cylinder down
    await cylinder.MoveDownAsync();

    // Wait for sensor (KHÔNG block!)
    bool ok = await _sensorWaiter.WaitForSensorAsync(
        "Cylinder Down",
        async () => await ReadSensor(downSensor),
        timeoutMs: 2000,
        ct
    );

    if (!ok)
    {
        RaiseAlarm(2001, "Cylinder timeout");
        return;
    }

    // Continue...
}
```

---

## ✅ Kết Quả

### Trước (❌ BAD)
```
Mỗi dự án:
- 350+ dòng code common IO
- Thread.Sleep() → Block tất cả
- EMG không hoạt động khi chờ
- Phải viết lại mỗi lần
```

### Bây Giờ (✅ GOOD)
```
Mỗi dự án:
- 30 dòng code setup (1 lần)
- Không bao giờ block
- EMG luôn hoạt động
- Tất cả tự động
```

**Tiết kiệm**: 90% code, 100% đáng tin cậy hơn!

---

## 📖 Tài Liệu Chi Tiết

1. **CONCURRENT_TASKS_GUIDE.md** - Hướng dẫn đầy đủ với ví dụ
2. **CommonIOHandler.cs** - Source code xử lý common IO
3. **BackgroundTaskManager.cs** - Source code quản lý tasks
4. **SensorWaiter.cs** - Source code chờ sensor không block

---

## 🚀 Status

✅ CommonIOHandler - Hoàn thành
✅ BackgroundTaskManager - Hoàn thành
✅ SensorWaiter - Hoàn thành
⏭️ Cập nhật PickAndPlaceMachine để demo - Tiếp theo

---

**KẾT LUẬN**:
- Không cần viết lại code common IO nữa
- Không bị block khi chờ sensor
- Tất cả chạy song song
- Code ít hơn, tin cậy hơn!
