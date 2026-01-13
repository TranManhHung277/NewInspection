# NAutoSuite Framework - Hướng Dẫn Mới (Đã Cải Tiến)

## 🎉 CẢI TIẾN MỚI NHẤT

**MachineBase giờ đây TỰ ĐỘNG xử lý TẤT CẢ:**
- ✅ Vòng lặp IO scan (100ms cycle)
- ✅ EMG / Start / Stop / Reset buttons
- ✅ Tower lights (đèn tháp)
- ✅ Buzzer (còi báo)
- ✅ Safety sensors (cửa, áp suất khí)
- ✅ Background task manager
- ✅ Alarm & Interlock system

**Khi tạo dự án mới, bạn CHỈ CẦN:**
1. Kế thừa từ `MachineBase`
2. Override 3 methods chính
3. Viết logic AUTO trong `OnRunningAsync()`
4. **XONG!** - Không cần setup CommonIOHandler, BackgroundTaskManager, scan timer!

---

## 📋 CẤU TRÚC DỰ ÁN MỚI

### Bước 1: Tạo IO Map

```csharp
using NAutoSuite.Core.IO;

namespace Machine.YourMachine.Core;

public class YourMachineIOMap : IOMap
{
    // Properties để truy cập nhanh
    public YourMachineInputMap MachineInputs => (YourMachineInputMap)Inputs;
    public YourMachineOutputMap MachineOutputs => (YourMachineOutputMap)Outputs;

    public YourMachineIOMap()
    {
        Inputs = new YourMachineInputMap();
        Outputs = new YourMachineOutputMap();
    }
}

public class YourMachineInputMap : InputMap
{
    public string Sensor1 { get; set; } = "IX1.0";
    public string Sensor2 { get; set; } = "IX1.1";
}

public class YourMachineOutputMap : OutputMap
{
    public string Valve1 { get; set; } = "QX1.0";
    public string Cylinder1 { get; set; } = "QX1.1";
}
```

---

### Bước 2: Tạo Data/Settings

```csharp
using NAutoSuite.Core.Data;

namespace Machine.YourMachine.Core;

public class YourMachineData : MachineData
{
    // Positions
    public PositionData HomePosition { get; set; } = new(0, 0, 0);
    public PositionData WorkPosition { get; set; } = new(100, 100, 50);

    // Speeds
    public SpeedData FastSpeed { get; set; } = new(100, 500, 500);
    public SpeedData SlowSpeed { get; set; } = new(10, 100, 100);

    // Settings
    public double PickDelay { get; set; } = 0.5;
    public double PlaceDelay { get; set; } = 0.3;

    public override void Load(string filePath) { /* JSON load */ }
    public override void Save(string filePath) { /* JSON save */ }
}
```

---

### Bước 3: Tạo Machine Class - CHỈ 3 METHODS BẮT BUỘC!

```csharp
using NAutoSuite.Core.Machine;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.IO;
using Serilog;

namespace Machine.YourMachine.Core;

public class YourMachine : MachineBase
{
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly IOutput? _vacuum;
    private readonly IPlc? _plc;

    private readonly YourMachineIOMap _ioMap;
    private readonly YourMachineData _data;

    public YourMachine(
        string id,
        string name,
        IAxis? axisX = null,
        IAxis? axisY = null,
        IOutput? vacuum = null,
        IPlc? plc = null,
        ILogger? logger = null)
        : base(id, name, logger)
    {
        _axisX = axisX;
        _axisY = axisY;
        _vacuum = vacuum;
        _plc = plc;

        // Initialize IO Map và Data
        _ioMap = new YourMachineIOMap();
        _data = new YourMachineData();

        // Setup interlocks
        SetupInterlocks();
    }

    private void SetupInterlocks()
    {
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "AXES_HOMED",
            Description = "All axes must be homed",
            Condition = () => (_axisX?.IsHomed ?? true) && (_axisY?.IsHomed ?? true),
            IsRequired = true
        });
    }

    #region MachineBase Overrides - CHỈ 3 METHODS NÀY LÀ BẮT BUỘC!

    /// <summary>
    /// [BẮT BUỘC] Provide IO Map cho automatic IO scanning
    /// </summary>
    protected override IOMap? GetIOMap() => _ioMap;

    /// <summary>
    /// [BẮT BUỘC] Handle reading inputs từ PLC/hardware
    /// </summary>
    protected override async Task<bool> OnReadInputAsync(string address, CancellationToken ct)
    {
        if (_plc == null) return false;

        try
        {
            var result = await _plc.ReadBitAsync(address, ct);
            return result.IsSuccess && result.Value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// [BẮT BUỘC] Handle writing outputs tới PLC/hardware
    /// </summary>
    protected override async Task OnWriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        if (_plc == null) return;

        try
        {
            await _plc.WriteBitAsync(address, value, ct);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to write {Address}", address);
        }
    }

    #endregion

    #region Optional Overrides

    /// <summary>
    /// [TÙY CHỌN] Initialize hardware - Connect axes, sensors
    /// </summary>
    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        _logger.Information("Initializing YourMachine...");

        // Connect axes
        if (_axisX != null)
        {
            await _axisX.ConnectAsync();
        }

        if (_axisY != null)
        {
            await _axisY.ConnectAsync();
        }

        _logger.Information("Hardware connected");
    }

    /// <summary>
    /// [TÙY CHỌN] Register additional background tasks
    /// </summary>
    protected override void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
    {
        // Example: Feeder monitor task (runs every 50ms)
        taskManager.RegisterTask("FeederMonitor", async ct =>
        {
            bool partDetected = await OnReadInputAsync(_ioMap.MachineInputs.Sensor1, ct);
            if (partDetected)
            {
                _logger.Information("Part detected!");
            }
        }, intervalMs: 50);
    }

    #endregion

    #region AUTO Logic - VIẾT LOGIC CỦA BẠN Ở ĐÂY

    /// <summary>
    /// Main AUTO sequence - Logic chính của máy
    /// Method này được gọi liên tục khi máy đang ở state Running
    /// </summary>
    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        _logger.Information("Starting cycle {Count}", Context.CycleCount + 1);

        // =======================================
        // VIẾT LOGIC AUTO CỦA BẠN Ở ĐÂY
        // =======================================

        // Ví dụ: Di chuyển đến vị trí làm việc
        if (_axisX != null)
        {
            await _axisX.MoveAbsoluteAsync(_data.WorkPosition.X);
        }

        if (_axisY != null)
        {
            await _axisY.MoveAbsoluteAsync(_data.WorkPosition.Y);
        }

        // Ví dụ: Chờ đến vị trí (không block IO scan!)
        await Task.Delay(TimeSpan.FromSeconds(1));

        // Ví dụ: Bật vacuum
        if (_vacuum != null)
        {
            await _vacuum.WriteAsync(true);
        }

        await Task.Delay(TimeSpan.FromSeconds(_data.PickDelay));

        // Ví dụ: Tắt vacuum
        if (_vacuum != null)
        {
            await _vacuum.WriteAsync(false);
        }

        // Ví dụ: Return home
        if (_axisX != null)
        {
            await _axisX.MoveAbsoluteAsync(0);
        }

        _logger.Information("Cycle {Count} completed", Context.CycleCount + 1);

        // =======================================
        // HẾT LOGIC AUTO
        // =======================================
    }

    #endregion
}
```

---

## 🔄 CÁCH HOẠT ĐỘNG TỰ ĐỘNG

### 1. Khi gọi `machine.InitializeAsync()`:

```
MachineBase.InitializeAsync()
  ↓
StartMainLoop() - TỰ ĐỘNG
  ├─ Lấy IOMap từ GetIOMap()
  ├─ Tạo CommonIOHandler
  ├─ Tạo BackgroundTaskManager
  ├─ Đăng ký task "CommonIOScan" (100ms)
  └─ Gọi OnRegisterBackgroundTasks() - Cho phép bạn thêm tasks
  ↓
BackgroundTaskManager.StartAll() - Tất cả tasks chạy song song!
  ├─ Task 1: CommonIOScan (100ms) - Quét buttons, cập nhật lights
  ├─ Task 2: FeederMonitor (50ms) - Nếu bạn đăng ký
  └─ Task N: YourCustomTask - Bất kỳ task nào bạn muốn
  ↓
OnInitializingAsync() - Bạn override để connect hardware
  ↓
State = Idle - Sẵn sàng Start!
```

### 2. Vòng lặp IO scan (tự động, 100ms):

```
CommonIOHandler.ScanAsync() - Chạy mỗi 100ms
  ├─ Đọc EMG button
  │  └─ Nếu pressed → machine.EmergencyStopAsync()
  ├─ Đọc Start button
  │  └─ Nếu pressed & state=Idle → machine.StartAsync()
  ├─ Đọc Stop button
  │  └─ Nếu pressed & state=Running → machine.StopAsync()
  ├─ Đọc Reset button
  │  └─ Nếu pressed → machine.ResetAsync()
  ├─ Cập nhật tower lights theo state
  │  ├─ Idle → Yellow
  │  ├─ Running → Green
  │  ├─ Error/EMG → Red blinking + Buzzer
  │  └─ Initializing → Yellow blinking
  └─ Kiểm tra safety sensors
     ├─ Safety door open → Stop
     └─ Air pressure lost → Stop
```

**LƯU Ý QUAN TRỌNG:**
- Vòng lặp này chạy SONG SONG với OnRunningAsync()
- EMG có thể được nhấn BẤT CỨ LÚC NÀO - ngay cả khi đang trong AUTO sequence
- Tower lights tự động cập nhật - bạn KHÔNG CẦN code gì!

### 3. Khi nhấn Start button (hoặc gọi `machine.StartAsync()`):

```
MachineBase.StartAsync()
  ├─ Check interlocks (axes homed? no alarms?)
  ├─ State → Running
  └─ OnRunningAsync() - Logic AUTO của bạn
     ↓
     Trong khi OnRunningAsync() đang chạy:
       - CommonIOScan vẫn chạy (100ms)
       - EMG có thể dừng bất cứ lúc nào
       - Tower lights tự động cập nhật
     ↓
     Khi OnRunningAsync() kết thúc:
       - State → Idle (nếu không có lỗi)
       - Hoặc State → Error (nếu có exception)
```

---

## ✅ SO SÁNH: CŨ vs MỚI

### ❌ CŨ - Phức tạp (100+ dòng setup):

```csharp
public class OldMachine : MachineBase
{
    private CommonIOHandler? _commonIOHandler;
    private BackgroundTaskManager? _backgroundTaskManager;
    private Timer? _scanTimer;

    public OldMachine()
    {
        SetupCommonIOHandler();     // 20 dòng
        SetupBackgroundTasks();     // 30 dòng
        SetupScanTimer();           // 15 dòng
        // ... rất nhiều boilerplate code
    }

    private void SetupCommonIOHandler()
    {
        _commonIOHandler = new CommonIOHandler(
            machine: this,
            ioMap: _ioMap,
            readInputFunc: ReadInputAsync,
            writeOutputFunc: WriteOutputAsync,
            logger: _logger
        );
    }

    private void SetupBackgroundTasks()
    {
        _backgroundTaskManager = new BackgroundTaskManager(_logger);
        _backgroundTaskManager.RegisterTask("CommonIOScan", async ct =>
        {
            await _commonIOHandler!.ScanAsync(ct);
        }, intervalMs: 100);
    }

    private void SetupScanTimer()
    {
        _scanTimer = new Timer(100);
        _scanTimer.Elapsed += async (s, e) => await ScanCycleAsync();
        _scanTimer.Start();
    }

    protected override async Task OnInitializingAsync()
    {
        _backgroundTaskManager?.StartAll();  // Phải nhớ start!
        // ...
    }

    // ... và nhiều code khác
}
```

### ✅ MỚI - Đơn giản (chỉ 3 methods):

```csharp
public class NewMachine : MachineBase
{
    public NewMachine() : base("id", "name")
    {
        // Chỉ cần khởi tạo IOMap, Data
        _ioMap = new YourMachineIOMap();
        _data = new YourMachineData();
    }

    // CHỈ 3 METHODS BẮT BUỘC:
    protected override IOMap? GetIOMap() => _ioMap;

    protected override async Task<bool> OnReadInputAsync(string address, CancellationToken ct)
    {
        // Đọc từ PLC
    }

    protected override async Task OnWriteOutputAsync(string address, bool value, CancellationToken ct)
    {
        // Ghi ra PLC
    }

    // Viết logic AUTO:
    protected override async Task OnRunningAsync()
    {
        // Logic của bạn ở đây
    }
}
```

**Giảm từ 100+ dòng setup → CHỈ 3 methods!**

---

## 🎯 CÁC TÍNH NĂNG TỰ ĐỘNG

### 1. Buttons - Tự động xử lý

| Button | Địa chỉ mặc định | Hành động tự động |
|--------|-----------------|-------------------|
| EMG | IX0.0 | → machine.EmergencyStopAsync() |
| Start | IX0.1 | → machine.StartAsync() (nếu Idle) |
| Stop | IX0.2 | → machine.StopAsync() (nếu Running) |
| Reset | IX0.3 | → machine.ResetAsync() |

### 2. Tower Lights - Tự động cập nhật

| State | Đèn xanh | Đèn vàng | Đèn đỏ | Buzzer |
|-------|---------|---------|--------|---------|
| Uninitialized/Initializing | ❌ | 💡 Blinking | ❌ | ❌ |
| Idle/Stopped | ❌ | ✅ ON | ❌ | ❌ |
| Running | ✅ ON | ❌ | ❌ | ❌ |
| Paused | 💡 Blinking | ❌ | ❌ | ❌ |
| Error/EMG | ❌ | ❌ | 🔴 Blinking | 🔊 Blinking |

### 3. Safety Sensors - Tự động kiểm tra

| Sensor | Địa chỉ | Hành động khi trigger |
|--------|---------|----------------------|
| Safety Door | IX0.4 | Stop machine nếu đang Running |
| Air Pressure | IX0.5 | Stop machine nếu đang Running |

---

## 🔧 TÙY CHỈNH NÂNG CAO

### Override địa chỉ Common IO (nếu cần):

```csharp
public class CustomIOMap : IOMap
{
    public CustomIOMap()
    {
        // Override địa chỉ mặc định
        CommonInputs.EmergencyStop = "IX10.0";  // Thay vì IX0.0
        CommonInputs.StartButton = "IX10.1";    // Thay vì IX0.1

        CommonOutputs.TowerLightRed = "QX5.0";  // Thay vì QX0.0
    }
}
```

### Thêm Background Tasks:

```csharp
protected override void OnRegisterBackgroundTasks(BackgroundTaskManager taskManager)
{
    // Task 1: Monitor feeder (50ms)
    taskManager.RegisterTask("FeederMonitor", async ct =>
    {
        bool hasPart = await OnReadInputAsync("IX1.5", ct);
        if (hasPart && !_feederBusy)
        {
            _logger.Information("Part detected - Pushing to position");
            await PushPartAsync();
        }
    }, intervalMs: 50);

    // Task 2: Vision processing (200ms)
    taskManager.RegisterTask("VisionCheck", async ct =>
    {
        if (_camera != null && State == MachineState.Running)
        {
            var image = await _camera.GrabAsync();
            var result = await _vision.InspectAsync(image);
            if (!result.IsOK)
            {
                RaiseAlarm(3001, "Vision inspection failed", AlarmSeverity.Warning);
            }
        }
    }, intervalMs: 200);

    // Task 3: Data logging (1000ms)
    taskManager.RegisterTask("DataLogger", async ct =>
    {
        await LogMachineDataAsync();
    }, intervalMs: 1000);
}
```

---

## 📦 TEMPLATE ĐẦY ĐỦ - COPY & PASTE

```csharp
// File: YourMachineIOMap.cs
using NAutoSuite.Core.IO;

namespace Machine.YourMachine.Core;

public class YourMachineIOMap : IOMap
{
    public YourMachineInputMap MachineInputs => (YourMachineInputMap)Inputs;
    public YourMachineOutputMap MachineOutputs => (YourMachineOutputMap)Outputs;

    public YourMachineIOMap()
    {
        Inputs = new YourMachineInputMap();
        Outputs = new YourMachineOutputMap();
    }
}

public class YourMachineInputMap : InputMap
{
    public string PartSensor { get; set; } = "IX1.0";
}

public class YourMachineOutputMap : OutputMap
{
    public string Vacuum { get; set; } = "QX1.0";
}
```

```csharp
// File: YourMachineData.cs
using NAutoSuite.Core.Data;

namespace Machine.YourMachine.Core;

public class YourMachineData : MachineData
{
    public PositionData PickPosition { get; set; } = new(100, 50, 10);
    public double PickDelay { get; set; } = 0.5;

    public override void Load(string filePath) { /* JSON */ }
    public override void Save(string filePath) { /* JSON */ }
}
```

```csharp
// File: YourMachine.cs
using NAutoSuite.Core.Machine;
using NAutoSuite.Core.Abstractions;
using NAutoSuite.Core.IO;
using Serilog;

namespace Machine.YourMachine.Core;

public class YourMachine : MachineBase
{
    private readonly YourMachineIOMap _ioMap;
    private readonly YourMachineData _data;
    private readonly IPlc? _plc;

    public YourMachine(
        string id,
        string name,
        IPlc? plc = null,
        ILogger? logger = null)
        : base(id, name, logger)
    {
        _plc = plc;
        _ioMap = new YourMachineIOMap();
        _data = new YourMachineData();
    }

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

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        // YOUR AUTO LOGIC HERE
        _logger.Information("Cycle {Count} started", Context.CycleCount + 1);

        // Move, activate, wait, etc.

        _logger.Information("Cycle {Count} completed", Context.CycleCount + 1);
    }
}
```

```csharp
// File: Program.cs
var machine = new YourMachine("YM001", "YourMachine", plc: myPlc);

await machine.InitializeAsync();  // Tự động start IO scan cycle!

await machine.StartAsync();  // Start AUTO

Console.ReadKey();

await machine.StopAsync();
await machine.DisposeAsync();  // Tự động stop IO scan cycle!
```

---

## 🎓 KẾT LUẬN

**ƯU ĐIỂM CỦA FRAMEWORK MỚI:**

✅ **Đơn giản hóa cực kỳ** - Chỉ 3 methods thay vì 100+ dòng setup
✅ **Tự động hóa hoàn toàn** - EMG/buttons/lights được xử lý tự động
✅ **Không cần nhớ start/stop** - MachineBase tự quản lý lifecycle
✅ **Background tasks dễ dàng** - Chỉ cần override OnRegisterBackgroundTasks()
✅ **An toàn hơn** - IO scan chạy song song, EMG luôn hoạt động
✅ **Dễ maintain** - Tất cả logic chung ở MachineBase, không trùng lặp
✅ **Dễ testing** - Mock GetIOMap(), OnReadInputAsync(), OnWriteOutputAsync()

**KHI TẠO DỰ ÁN MỚI:**
1. Copy template ở trên
2. Đổi tên `YourMachine` → tên máy của bạn
3. Thêm IO addresses, positions, speeds
4. Viết logic AUTO trong `OnRunningAsync()`
5. **DONE!**

**Không cần lo:**
- ❌ Setup CommonIOHandler
- ❌ Setup BackgroundTaskManager
- ❌ Setup scan timer
- ❌ Start/stop background tasks
- ❌ Xử lý EMG/Start/Stop/Reset
- ❌ Cập nhật tower lights
- ❌ Kiểm tra safety sensors

**TẤT CẢ đã tự động!** 🎉
