# Hướng Dẫn: Xử Lý Concurrent Tasks (Chạy Đồng Thời Không Block)

## 🎯 Vấn Đề

### Vấn Đề 1: Code Lặp Lại Mỗi Dự Án
❌ **TRƯỚC ĐÂY**: Mỗi dự án phải viết lại code xử lý:
- Nút EMG, Start, Stop, Reset
- Đèn tháp (Xanh/Đỏ/Vàng/Còi)
- Safety sensors (Door, Air pressure)

### Vấn Đề 2: Task Bị Block
❌ **TRƯỚC ĐÂY**: Khi chờ sensor, tất cả bị block:
```csharp
// BAD: Code này block TẤT CẢ
await cylinder.MoveDownAsync();
await WaitForSensor(downSensor, timeout: 5000); // ← BLOCK 5 giây!
// Trong 5 giây này, không đọc được:
// - Nút EMG
// - Sensor khác (feeder, conveyor)
// - Đèn tháp không nhấp nháy
```

---

## ✅ Giải Pháp: Task-Based Architecture

### Giải Pháp 1: CommonIOHandler (Xử Lý Tự Động Common IO)

**File: `NAutoSuite.Core/Machine/CommonIOHandler.cs`**

Class này xử lý **TỰ ĐỘNG** tất cả common IO:
- ✅ Đọc và xử lý nút EMG, Start, Stop, Reset
- ✅ Cập nhật đèn tháp theo trạng thái máy
- ✅ Kiểm tra safety sensors (Door, Air pressure)
- ✅ **KHÔNG cần viết lại code này mỗi dự án**

```csharp
// Chỉ cần gọi 1 dòng trong scan cycle
await _commonIOHandler.ScanAsync(ct);

// Tất cả common IO được xử lý tự động:
// - EMG → Emergency stop
// - Start button → Start machine
// - Stop button → Stop machine
// - Reset button → Reset machine
// - Tower lights theo state (Idle=Yellow, Running=Green, Error=Red+Buzzer)
// - Safety door → Stop if opened
// - Air pressure → Stop if lost
```

### Giải Pháp 2: BackgroundTaskManager (Chạy Nhiều Tasks Song Song)

**File: `NAutoSuite.Core/Machine/BackgroundTaskManager.cs`**

Cho phép chạy **NHIỀU tasks đồng thời**:

```csharp
var taskManager = new BackgroundTaskManager();

// Task 1: Scan Cycle - Xử lý common IO (100ms)
taskManager.RegisterTask("ScanCycle", async ct =>
{
    await _commonIOHandler.ScanAsync(ct);
}, intervalMs: 100);

// Task 2: Feeder Monitor - Gạt linh kiện vào vị trí (50ms - nhanh hơn)
taskManager.RegisterTask("FeederMonitor", async ct =>
{
    if (await ReadSensor(partDetectSensor))
    {
        await PushCylinder.ExtendAsync();
        await Task.Delay(500, ct);
        await PushCylinder.RetractAsync();
    }
}, intervalMs: 50);

// Task 3: Conveyor Monitor - Theo dõi băng tải (100ms)
taskManager.RegisterTask("ConveyorMonitor", async ct =>
{
    // Monitor logic
}, intervalMs: 100);

// Start TẤT CẢ tasks song song
taskManager.StartAll();
```

**Kết quả**: 3 tasks chạy **SONG SONG**, không block nhau!

### Giải Pháp 3: SensorWaiter (Chờ Sensor Không Block)

**File: `NAutoSuite.Core/Machine/BackgroundTaskManager.cs`**

Chờ sensor mà **KHÔNG block** các task khác:

```csharp
var sensorWaiter = new SensorWaiter();

// VÍ DỤ: Chờ xi lanh xuống
await cylinder.MoveDownAsync();

// Chờ sensor DOWN trong 5 giây (KHÔNG block scan cycle!)
bool success = await sensorWaiter.WaitForSensorAsync(
    sensorName: "Cylinder Down Sensor",
    readFunc: async () => await ReadSensor(cylinderDownSensor),
    timeoutMs: 5000
);

if (!success)
{
    RaiseAlarm(2001, "Cylinder down timeout");
}
```

**Trong khi chờ 5 giây**:
- ✅ Scan cycle vẫn chạy (đọc EMG, cập nhật đèn)
- ✅ Feeder monitor vẫn hoạt động (gạt linh kiện)
- ✅ Conveyor monitor vẫn theo dõi

---

## 📐 Kiến Trúc Mới

```
┌─────────────────────────────────────────────────────────────┐
│  MACHINE                                                     │
│  ┌────────────────────────────────────────────────────────┐ │
│  │  BackgroundTaskManager (Quản lý tất cả tasks)          │ │
│  │                                                          │ │
│  │  ┌──────────────────────────────────────────────────┐  │ │
│  │  │ Task 1: Scan Cycle (100ms)                       │  │ │
│  │  │   - CommonIOHandler.ScanAsync()                  │  │ │
│  │  │     ✅ Xử lý EMG/Start/Stop/Reset tự động        │  │ │
│  │  │     ✅ Cập nhật đèn tháp tự động                 │  │ │
│  │  │     ✅ Kiểm tra safety sensors                   │  │ │
│  │  └──────────────────────────────────────────────────┘  │ │
│  │                                                          │ │
│  │  ┌──────────────────────────────────────────────────┐  │ │
│  │  │ Task 2: AUTO Sequence (On-demand)               │  │ │
│  │  │   - OnRunningAsync()                             │  │ │
│  │  │   - PickSequenceAsync()                          │  │ │
│  │  │   - PlaceSequenceAsync()                         │  │ │
│  │  │   - Sử dụng SensorWaiter (không block!)          │  │ │
│  │  └──────────────────────────────────────────────────┘  │ │
│  │                                                          │ │
│  │  ┌──────────────────────────────────────────────────┐  │ │
│  │  │ Task 3: Feeder Monitor (50ms)                    │  │ │
│  │  │   - Theo dõi sensor phát hiện linh kiện          │  │ │
│  │  │   - Gạt linh kiện vào vị trí                     │  │ │
│  │  └──────────────────────────────────────────────────┘  │ │
│  │                                                          │ │
│  │  ┌──────────────────────────────────────────────────┐  │ │
│  │  │ Task 4: Conveyor Monitor (100ms)                 │  │ │
│  │  │   - Theo dõi băng tải đầu vào                    │  │ │
│  │  └──────────────────────────────────────────────────┘  │ │
│  └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘

TẤT CẢ CHẠY SONG SONG! ✅
```

---

## 💡 Ví Dụ Thực Tế

### Trước Đây (❌ BAD)

```csharp
private async Task PickSequenceAsync()
{
    // Move to pick position
    await axisX.MoveAbsoluteAsync(100);
    await axisY.MoveAbsoluteAsync(50);

    // Lower Z and wait
    await axisZ.MoveAbsoluteAsync(10);
    Thread.Sleep(500); // ← BLOCK TẤT CẢ! EMG không hoạt động!

    // Activate vacuum and wait
    await vacuum.WriteAsync(true);
    Thread.Sleep(200); // ← BLOCK TẤT CẢ!

    // Trong thời gian chờ (700ms):
    // - Không đọc được nút EMG
    // - Không xử lý được feeder
    // - Đèn tháp không nhấp nháy
}
```

### Bây Giờ (✅ GOOD)

```csharp
private async Task PickSequenceAsync(CancellationToken ct)
{
    // Move to pick position
    await axisX.MoveAbsoluteAsync(100);
    await axisY.MoveAbsoluteAsync(50);

    // Lower Z
    await axisZ.MoveAbsoluteAsync(10);

    // Chờ Z xuống (KHÔNG block!)
    bool zDown = await _sensorWaiter.WaitForSensorAsync(
        "Z Down Sensor",
        async () => await ReadSensor(zDownSensor),
        timeoutMs: 2000,
        ct
    );

    if (!zDown)
    {
        RaiseAlarm(2001, "Z axis timeout");
        return;
    }

    // Activate vacuum
    await vacuum.WriteAsync(true);

    // Chờ vacuum (KHÔNG block!)
    bool hasVacuum = await _sensorWaiter.WaitForSensorAsync(
        "Vacuum Sensor",
        async () => await ReadSensor(vacuumSensor),
        timeoutMs: 1000,
        ct
    );

    if (!hasVacuum)
    {
        RaiseAlarm(2002, "Vacuum pressure not detected");
        return;
    }

    // Trong thời gian chờ (tối đa 3 giây):
    // ✅ Scan cycle vẫn chạy (EMG hoạt động)
    // ✅ Feeder monitor vẫn gạt linh kiện
    // ✅ Đèn tháp vẫn nhấp nháy
}
```

---

## 📝 Cách Sử Dụng Trong Dự Án

### Bước 1: Setup trong Constructor

```csharp
public PickAndPlaceMachine(...)
{
    // ...

    // Setup Common IO Handler (TỰ ĐỘNG xử lý common IO)
    _commonIOHandler = new CommonIOHandler(this, _ioMap, _plc, Log.Logger);

    // Setup Sensor Waiter
    _sensorWaiter = new SensorWaiter(Log.Logger);

    // Setup Background Task Manager
    _backgroundTaskManager = new BackgroundTaskManager(Log.Logger);

    // Đăng ký Task 1: Scan Cycle
    _backgroundTaskManager.RegisterTask("ScanCycle", async ct =>
    {
        await _commonIOHandler.ScanAsync(ct);
    }, intervalMs: 100);

    // Đăng ký Task 2: Feeder Monitor (nếu có)
    _backgroundTaskManager.RegisterTask("FeederMonitor",
        FeederMonitorAsync,
        intervalMs: 50
    );
}
```

### Bước 2: Start/Stop Background Tasks

```csharp
protected override async Task OnInitializingAsync()
{
    await base.OnInitializingAsync();

    // Start all background tasks
    _backgroundTaskManager?.StartAll();

    // Initialize hardware...
}

protected override async Task OnExitRunningAsync()
{
    await base.OnExitRunningAsync();

    // Stop all background tasks when stopping
    _backgroundTaskManager?.StopAll();
}
```

### Bước 3: Viết AUTO Logic (Dùng SensorWaiter)

```csharp
protected override async Task OnRunningAsync()
{
    await base.OnRunningAsync();

    while (State == MachineState.Running)
    {
        await PickSequenceAsync(CancellationToken.None);
        await PlaceSequenceAsync(CancellationToken.None);

        Context.CycleCount++;
    }
}

private async Task PickSequenceAsync(CancellationToken ct)
{
    // Move axes
    await axisX.MoveAbsoluteAsync(pickX);
    await axisY.MoveAbsoluteAsync(pickY);
    await axisZ.MoveAbsoluteAsync(pickZ);

    // Chờ Z xuống (KHÔNG block scan cycle)
    bool zReady = await _sensorWaiter.WaitForSensorAsync(
        "Z at pick position",
        async () => axisZ.IsInPosition,
        timeoutMs: 2000,
        ct
    );

    if (!zReady)
    {
        RaiseAlarm(2001, "Z axis timeout at pick");
        return;
    }

    // Vacuum ON
    await vacuum.WriteAsync(true);

    // Chờ vacuum (KHÔNG block)
    bool hasVacuum = await _sensorWaiter.WaitForSensorAsync(
        "Vacuum sensor",
        async () => await ReadSensor(_ioMap.MachineInputs.VacuumSensor),
        timeoutMs: 1000,
        ct
    );

    if (!hasVacuum)
    {
        RaiseAlarm(2002, "Vacuum not detected");
        return;
    }

    // Raise Z
    await axisZ.MoveAbsoluteAsync(safeZ);
}
```

### Bước 4: Viết Background Task (Optional)

Nếu cần xử lý task phụ (như feeder, conveyor):

```csharp
private async Task FeederMonitorAsync(CancellationToken ct)
{
    // Đọc sensor phát hiện linh kiện
    bool partDetected = await ReadSensor(_ioMap.MachineInputs.PartSensorAtPick);

    if (partDetected && !_partInPosition)
    {
        Log.Information("Part detected - Pushing into position");

        // Gạt linh kiện vào vị trí
        await _pushCylinder.ExtendAsync();
        await Task.Delay(500, ct);
        await _pushCylinder.RetractAsync();

        _partInPosition = true;
    }

    // Task này chạy song song với AUTO sequence!
}
```

---

## ✅ Lợi Ích

### 1. Không Code Lặp Lại
- ✅ Common IO (EMG/Start/Stop/Reset/Đèn) chỉ viết **1 LẦN** trong `CommonIOHandler`
- ✅ Mọi dự án **TỰ ĐỘNG** có sẵn xử lý common IO
- ✅ Chỉ cần khai báo địa chỉ trong `IOMap`

### 2. Không Bị Block
- ✅ Chờ sensor không block scan cycle
- ✅ EMG luôn hoạt động ngay cả khi đang chờ
- ✅ Đèn tháp luôn nhấp nháy đúng

### 3. Chạy Song Song
- ✅ AUTO sequence chạy
- ✅ Scan cycle xử lý buttons
- ✅ Feeder monitor gạt linh kiện
- ✅ Conveyor monitor theo dõi băng tải
- ✅ **TẤT CẢ đồng thời!**

### 4. Dễ Maintain
- ✅ Sửa common IO → sửa `CommonIOHandler` 1 lần, tất cả dự án đều có
- ✅ Logic rõ ràng, tách biệt
- ✅ Dễ debug từng task riêng

---

## 🎓 So Sánh

### TRƯỚC ĐÂY (❌ BAD)
```
Mỗi dự án phải viết:
✗ 200 dòng code xử lý EMG/Start/Stop/Reset
✗ 100 dòng code xử lý đèn tháp
✗ 50 dòng code kiểm tra safety sensors
✗ Thread.Sleep() → Block tất cả
✗ Không chạy song song

= 350 dòng code LẶP LẠI mỗi dự án
```

### BÂY GIỜ (✅ GOOD)
```
Mỗi dự án chỉ cần:
✅ Khai báo địa chỉ IO trong IOMap (20 dòng)
✅ Gọi _commonIOHandler.ScanAsync() (1 dòng)
✅ Sử dụng SensorWaiter.WaitForSensorAsync() (3 dòng/sensor)
✅ Chạy song song tự động

= 30-50 dòng code, còn lại TỰ ĐỘNG!
```

---

## 📖 Tham Khảo

- **CommonIOHandler.cs** - Xử lý tự động common IO
- **BackgroundTaskManager.cs** - Quản lý concurrent tasks
- **SensorWaiter.cs** - Chờ sensor không block
- **PickAndPlaceMachine.cs** - Ví dụ sử dụng

---

## 🚀 Next Steps

1. ✅ Đã tạo `CommonIOHandler` - Xử lý tự động common IO
2. ✅ Đã tạo `BackgroundTaskManager` - Chạy tasks song song
3. ✅ Đã tạo `SensorWaiter` - Chờ sensor không block
4. ⏭️ Cập nhật `PickAndPlaceMachine` để sử dụng các class mới
5. ⏭️ Test và verify

---

**KẾT LUẬN**: Với kiến trúc này, bạn **KHÔNG BAO GIỜ** phải viết lại code xử lý common IO nữa. Chỉ cần khai báo địa chỉ, phần còn lại TỰ ĐỘNG!
