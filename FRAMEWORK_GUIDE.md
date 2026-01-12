# NAutoSuite Framework - Hướng Dẫn Sử Dụng

## 📋 TỔNG QUAN

**NAutoSuite Framework** là bộ khung lập trình chung cho **TẤT CẢ** các máy tự động hóa.

### ✅ Platform Core xử lý SẴN:
- ✅ EMG (Emergency Stop) - Dừng khẩn cấp
- ✅ Start - Khởi động
- ✅ Stop - Dừng
- ✅ Reset - Reset lỗi
- ✅ Init - Khởi tạo (Home all axes)
- ✅ Tower Light - Đèn tháp
- ✅ Buzzer - Còi báo
- ✅ Alarm Management
- ✅ Interlock Management
- ✅ State Machine

### ✅ Khi tạo dự án MỚI, bạn CHỈ CẦN:
1. **Map IO** - Định nghĩa địa chỉ IO
2. **Định nghĩa Data** - Vị trí, tốc độ, settings
3. **Viết Logic** - Trong 1 FILE duy nhất

---

## 🚀 CÁCH TẠO DỰ ÁN MỚI

### Bước 1: Tạo IO Map

Tạo file `<TênMáy>IOMap.cs`:

```csharp
using NAutoSuite.Core.IO;

namespace Machine.YourMachine.Core;

/// <summary>
/// IO Map cho máy của bạn
/// Chỉ cần định nghĩa IO riêng của máy này
/// IO chung (EMG, Start, Stop) đã có sẵn trong IOMap base class
/// </summary>
public class YourMachineIOMap : IOMap
{
    public YourMachineIOMap()
    {
        Inputs = new YourMachineInputMap();
        Outputs = new YourMachineOutputMap();

        // (Tùy chọn) Override địa chỉ IO chung nếu cần
        // CommonInputs.EmergencyStop = "IX0.0";  // Mặc định đã có
        // CommonInputs.StartButton = "IX0.1";
        // CommonInputs.StopButton = "IX0.2";
        // CommonInputs.ResetButton = "IX0.3";
    }
}

/// <summary>
/// Inputs riêng của máy bạn
/// </summary>
public class YourMachineInputMap : InputMap
{
    // Cảm biến, công tắc, etc.
    public string Sensor1 { get; set; } = "IX1.0";
    public string Sensor2 { get; set; } = "IX1.1";
    public string LimitSwitch { get; set; } = "IX1.2";
    // ... thêm các input khác
}

/// <summary>
/// Outputs riêng của máy bạn
/// </summary>
public class YourMachineOutputMap : OutputMap
{
    // Van, xi lanh, đèn, etc.
    public string Valve1 { get; set; } = "QX1.0";
    public string Cylinder1 { get; set; } = "QX1.1";
    public string Light1 { get; set; } = "QX1.2";
    // ... thêm các output khác
}
```

### Bước 2: Tạo Data/Settings

Tạo file `<TênMáy>Data.cs`:

```csharp
using NAutoSuite.Core.Data;
using System.Text.Json;

namespace Machine.YourMachine.Core;

/// <summary>
/// Data/Settings cho máy của bạn
/// Common settings (MachineId, CycleTimeout, etc.) đã có sẵn
/// Chỉ cần định nghĩa settings riêng
/// </summary>
public class YourMachineData : MachineData
{
    // Vị trí
    public PositionData Position1 { get; set; } = new(0, 0, 0);
    public PositionData Position2 { get; set; } = new(100, 100, 50);

    // Tốc độ
    public SpeedData FastSpeed { get; set; } = new(100, 500, 500);
    public SpeedData SlowSpeed { get; set; } = new(10, 100, 100);

    // Các thông số khác
    public double Timeout { get; set; } = 5.0;
    public bool EnableFeatureX { get; set; } = true;

    public YourMachineData()
    {
        // Set common settings
        Common.MachineId = "YM001";
        Common.MachineName = "Your Machine Name";
        Common.CycleTimeout = 30.0;
    }

    // Load/Save JSON
    public override void Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Save(filePath);
            return;
        }

        var json = File.ReadAllText(filePath);
        var data = JsonSerializer.Deserialize<YourMachineData>(json);

        if (data != null)
        {
            // Copy properties từ data vào this
            Common = data.Common;
            Position1 = data.Position1;
            Position2 = data.Position2;
            // ... copy các properties khác
        }
    }

    public override void Save(string filePath)
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(this, options);
        File.WriteAllText(filePath, json);
    }
}
```

### Bước 3: Viết Logic trong 1 FILE

Tạo file `<TênMáy>Machine.cs`:

```csharp
using NAutoSuite.Core.Machine;
using NAutoSuite.Core.Common;
using NAutoSuite.Core.Abstractions;
using Serilog;

namespace Machine.YourMachine.Core;

/// <summary>
/// Logic chính cho máy của bạn
/// Tất cả logic AUTO viết ở đây - TRONG 1 FILE
/// </summary>
public class YourMachineMachine : MachineBase
{
    // IO Map và Data
    private readonly YourMachineIOMap _ioMap;
    private readonly YourMachineData _data;
    private readonly MachineController _controller;

    // Hardware
    private readonly IPlc? _plc;
    private readonly IAxis? _axisX;
    private readonly IAxis? _axisY;
    private readonly IOutput? _output1;

    public YourMachineMachine(
        IPlc? plc,
        IAxis? axisX,
        IAxis? axisY,
        IOutput? output1)
        : base("YourMachine", Log.Logger)
    {
        _plc = plc;
        _axisX = axisX;
        _axisY = axisY;
        _output1 = output1;

        // Tạo IO Map và Data
        _ioMap = new YourMachineIOMap();
        _data = new YourMachineData();

        // Tạo MachineController - xử lý EMG/Start/Stop/Reset/Init
        _controller = new MachineController(_ioMap, _data, _plc, Log.Logger);

        // Đăng ký axes và outputs với controller
        if (_axisX != null) _controller.RegisterAxis(_axisX);
        if (_axisY != null) _controller.RegisterAxis(_axisY);
        if (_output1 != null) _controller.RegisterOutput(_output1);

        // Load settings
        _data.Load("YourMachineSettings.json");

        // Setup interlocks
        SetupInterlocks();
    }

    private void SetupInterlocks()
    {
        // Các điều kiện phải thỏa trước khi Start
        Interlocks.Add("AllAxesHomed", () => _controller.AreAllAxesHomed());
        Interlocks.Add("NoMoving", () => !_controller.IsAnyAxisMoving());
    }

    /// <summary>
    /// Cyclic update - gọi mỗi scan cycle
    /// </summary>
    protected override async Task OnIdleAsync(CancellationToken cancellationToken)
    {
        // MachineController tự động xử lý:
        // - Đọc nút EMG, Start, Stop, Reset
        // - Xử lý EMG: dừng motor, bật đèn đỏ, buzzer
        // - Xử lý Start: bật đèn xanh
        // - Xử lý Stop: dừng motor, bật đèn vàng
        // - Xử lý Reset: tắt alarm, bật đèn vàng
        await _controller.TickAsync(cancellationToken);

        await base.OnIdleAsync(cancellationToken);
    }

    /// <summary>
    /// Init - Home all axes
    /// Platform Core tự động xử lý
    /// </summary>
    protected override async Task<Result> OnInitializingAsync(CancellationToken cancellationToken)
    {
        return await _controller.InitializeAsync(cancellationToken);
    }

    /// <summary>
    /// AUTO logic - VIẾT Ở ĐÂY
    /// Đây là logic chạy tự động của máy bạn
    /// </summary>
    protected override async Task OnRunningAsync(CancellationToken cancellationToken)
    {
        // ===================================
        // VIẾT LOGIC AUTO CỦA BẠN Ở ĐÂY
        // ===================================

        Log.Information("Starting auto cycle");

        // Ví dụ: Di chuyển đến vị trí 1
        if (_axisX != null)
        {
            await _axisX.MoveAbsoluteAsync(
                _data.Position1.X,
                _data.FastSpeed.Velocity,
                cancellationToken: cancellationToken);
        }

        // Ví dụ: Bật output
        if (_output1 != null)
        {
            await _output1.WriteAsync(true, cancellationToken);
        }

        // Ví dụ: Đợi
        await Task.Delay(TimeSpan.FromSeconds(_data.Timeout), cancellationToken);

        // Ví dụ: Tắt output
        if (_output1 != null)
        {
            await _output1.WriteAsync(false, cancellationToken);
        }

        // Ví dụ: Di chuyển về home
        if (_axisX != null)
        {
            await _axisX.MoveAbsoluteAsync(0, _data.FastSpeed.Velocity, cancellationToken: cancellationToken);
        }

        Log.Information("Auto cycle completed");

        // ===================================
        // HẾT LOGIC AUTO
        // ===================================
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    public override async Task DisposeAsync()
    {
        _data.Save("YourMachineSettings.json");
        await base.DisposeAsync();
    }
}
```

---

## 📝 VÍ DỤ CỤ THỂ: Pick and Place Machine

Xem các file mẫu:

### IO Map:
- File: `projects/Machine.PickAndPlace/Machine.PickAndPlace.Core/PickAndPlaceIOMap.cs`
- Kế thừa: `IOMap`
- Nội dung: Định nghĩa sensors, vacuum valve, blow off valve

### Data:
- File: `projects/Machine.PickAndPlace/Machine.PickAndPlace.Core/PickAndPlaceData.cs`
- Kế thừa: `MachineData`
- Nội dung: Pick position, Place position, speeds, timeouts

### Logic:
- File: `projects/Machine.PickAndPlace/Machine.PickAndPlace.Core/PickAndPlaceMachine.cs`
- Method: `OnRunningAsync()` - line 132
- Nội dung: Logic pick -> place -> return home

---

## 🎯 KIẾN TRÚC FRAMEWORK

```
┌─────────────────────────────────────────────────────────────────┐
│                     PLATFORM CORE (Dùng Chung)                  │
├─────────────────────────────────────────────────────────────────┤
│  ✅ MachineController                                           │
│     - HandleEmergency() ← Dừng motor, đèn đỏ, buzzer           │
│     - HandleStart()      ← Đèn xanh, enable                     │
│     - HandleStop()       ← Dừng motor, đèn vàng                 │
│     - HandleReset()      ← Xóa lỗi, đèn vàng                    │
│     - Initialize()       ← Home all axes                        │
│                                                                  │
│  ✅ IOMap (Base Class)                                          │
│     - CommonInputs:  EMG, Start, Stop, Reset, SafetyDoor        │
│     - CommonOutputs: Tower Lights, Buzzer, MainPower            │
│     - InputMap:      (override cho máy cụ thể)                  │
│     - OutputMap:     (override cho máy cụ thể)                  │
│                                                                  │
│  ✅ MachineData (Base Class)                                    │
│     - CommonSettings: MachineId, CycleTimeout, BuzzerOnAlarm    │
│     - PositionData, SpeedData helper classes                    │
│     - Abstract Load/Save methods                                │
│                                                                  │
│  ✅ MachineBase (State Machine)                                 │
│     - 8 States: Stopped, Idle, Initializing, Ready, etc.        │
│     - AlarmManager, InterlockManager                            │
│     - Virtual methods: OnRunningAsync, OnInitializingAsync      │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│               DỰ ÁN CỤ THỂ (Machine-Specific)                   │
├─────────────────────────────────────────────────────────────────┤
│  📝 YourMachineIOMap : IOMap                                    │
│     - Map địa chỉ IO riêng của máy này                          │
│                                                                  │
│  📝 YourMachineData : MachineData                               │
│     - Định nghĩa vị trí, tốc độ, settings riêng                 │
│                                                                  │
│  📝 YourMachineMachine : MachineBase                            │
│     - Viết logic AUTO trong OnRunningAsync()                    │
│     - Chỉ cần 1 FILE duy nhất                                   │
└─────────────────────────────────────────────────────────────────┘
```

---

## 🔧 CÁCH HOẠT ĐỘNG

### 1. Khi nhấn nút EMG (màn hình hoặc nút cứng):
```
MachineController.TickAsync()
  ↓ đọc IO
  ↓ phát hiện EMG pressed
  ↓
MachineController.HandleEmergencyAsync()
  ✅ Stop all motors (emergency stop)
  ✅ Disable all outputs
  ✅ Turn on RED tower light
  ✅ Sound buzzer
  ✅ Disable main power
  ✅ Set _emergencyActive = true
```

### 2. Khi nhấn nút Start:
```
MachineController.TickAsync()
  ↓ đọc IO
  ↓ phát hiện Start pressed
  ↓
MachineController.HandleStartButtonAsync()
  ✅ Check emergency state
  ✅ Turn on GREEN tower light
  ✅ Turn off buzzer
  ✅ (Machine state machine tự chuyển sang Running)
```

### 3. Khi nhấn nút Reset:
```
MachineController.TickAsync()
  ↓ đọc IO
  ↓ phát hiện Reset pressed
  ↓
MachineController.HandleResetButtonAsync()
  ✅ Clear emergency state
  ✅ Turn off buzzer
  ✅ Turn on YELLOW tower light (ready)
  ✅ Enable main power
```

### 4. Khi Init (Home):
```
MachineController.InitializeAsync()
  ✅ Check emergency state
  ✅ Home all registered axes
  ✅ Cancel if EMG pressed during homing
```

---

## 📦 FILE STRUCTURE

```
NAutoSuite/
│
├── src/Core/NAutoSuite.Core/          ← PLATFORM CORE
│   ├── Machine/
│   │   ├── MachineBase.cs             ← State machine, alarms, interlocks
│   │   └── MachineController.cs       ← ✅ EMG/Start/Stop/Reset/Init handler
│   ├── IO/
│   │   └── IOMap.cs                   ← ✅ Base class cho IO mapping
│   └── Data/
│       └── MachineData.cs             ← ✅ Base class cho machine data
│
└── projects/
    └── Machine.YourMachine/            ← DỰ ÁN CỤ THỂ
        └── Machine.YourMachine.Core/
            ├── YourMachineIOMap.cs     ← Map IO của máy này
            ├── YourMachineData.cs      ← Data/settings của máy này
            └── YourMachineMachine.cs   ← Logic AUTO trong 1 FILE
```

---

## ✅ CHECKLIST TẠO DỰ ÁN MỚI

- [ ] 1. Tạo `<TênMáy>IOMap.cs` - Map địa chỉ IO
- [ ] 2. Tạo `<TênMáy>Data.cs` - Định nghĩa vị trí, tốc độ, settings
- [ ] 3. Tạo `<TênMáy>Machine.cs` - Viết logic trong OnRunningAsync()
- [ ] 4. Đăng ký axes/outputs với MachineController
- [ ] 5. Setup interlocks nếu cần
- [ ] 6. Test EMG, Start, Stop, Reset
- [ ] 7. Test Init (Home)
- [ ] 8. Chạy Auto

---

## 🎓 KẾT LUẬN

Với framework này:
- ✅ **Không cần** viết lại logic EMG/Start/Stop/Reset cho mỗi dự án
- ✅ **Không cần** lo về tower light, buzzer, alarm
- ✅ **Chỉ cần** map IO và viết logic AUTO
- ✅ **Tất cả** dự án dùng chung 1 kiến trúc
- ✅ **Dễ dàng** deploy, maintain, train người mới

**Tất cả logic AUTO chỉ trong 1 FILE: `OnRunningAsync()`**
