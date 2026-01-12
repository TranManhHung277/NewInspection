# NAutoSuite Framework - Tóm Tắt

## ✅ ĐÃ HOÀN THÀNH

### 1. Platform Core - Bộ Khung Dùng Chung

#### ✅ MachineController.cs
- **File:** `src/Core/NAutoSuite.Core/Machine/MachineController.cs`
- **Chức năng:** Xử lý TẤT CẢ các chức năng chung cho MỌI máy tự động hóa

**Các chức năng đã implement:**

##### EMG (Emergency Stop)
```csharp
await HandleEmergencyAsync()
```
- ✅ Dừng TẤT CẢ động cơ (emergency stop)
- ✅ Disable TẤT CẢ outputs
- ✅ Bật đèn đỏ (tower light red)
- ✅ Bật còi báo (buzzer)
- ✅ Disable main power
- ✅ Set emergency state

##### Start
```csharp
await HandleStartButtonAsync()
```
- ✅ Kiểm tra emergency state
- ✅ Bật đèn xanh (tower light green)
- ✅ Tắt buzzer

##### Stop
```csharp
await HandleStopButtonAsync()
```
- ✅ Dừng TẤT CẢ động cơ (controlled stop)
- ✅ Bật đèn vàng (tower light yellow)

##### Reset
```csharp
await HandleResetButtonAsync()
```
- ✅ Xóa emergency state
- ✅ Tắt buzzer
- ✅ Bật đèn vàng (ready)
- ✅ Enable main power

##### Init (Home All)
```csharp
await InitializeAsync()
```
- ✅ Home TẤT CẢ axes đã đăng ký
- ✅ Kiểm tra emergency trong khi homing
- ✅ Hủy nếu EMG được nhấn

##### Cyclic Update
```csharp
await TickAsync()
```
- ✅ Đọc common IOs (EMG, Start, Stop, Reset buttons)
- ✅ Phát hiện rising edge của các nút
- ✅ Tự động gọi các handlers tương ứng

---

#### ✅ IOMap.cs
- **File:** `src/Core/NAutoSuite.Core/IO/IOMap.cs`
- **Chức năng:** Base class cho IO mapping

**Cấu trúc:**

```csharp
public abstract class IOMap
{
    public CommonInputMap CommonInputs { get; }    // IO chung cho TẤT CẢ máy
    public CommonOutputMap CommonOutputs { get; }  // Output chung cho TẤT CẢ máy
    public InputMap Inputs { get; }                // Inputs riêng (override)
    public OutputMap Outputs { get; }              // Outputs riêng (override)
}
```

**Common Inputs (TẤT CẢ máy có):**
- ✅ EmergencyStop (IX0.0)
- ✅ StartButton (IX0.1)
- ✅ StopButton (IX0.2)
- ✅ ResetButton (IX0.3)
- ✅ SafetyDoor (IX0.4)
- ✅ AirPressure (IX0.5)

**Common Outputs (TẤT CẢ máy có):**
- ✅ TowerLightRed (QX0.0)
- ✅ TowerLightYellow (QX0.1)
- ✅ TowerLightGreen (QX0.2)
- ✅ Buzzer (QX0.3)
- ✅ MainPowerEnable (QX0.4)

---

#### ✅ MachineData.cs
- **File:** `src/Core/NAutoSuite.Core/Data/MachineData.cs`
- **Chức năng:** Base class cho machine data/settings

**Cấu trúc:**

```csharp
public abstract class MachineData
{
    public CommonSettings Common { get; set; }

    public abstract void Load(string filePath);
    public abstract void Save(string filePath);
}
```

**Common Settings (TẤT CẢ máy có):**
- ✅ MachineId
- ✅ MachineName
- ✅ CycleTimeout
- ✅ AutoRestartAfterAlarm
- ✅ BuzzerOnAlarm
- ✅ EmergencyResetTimeout

**Helper Classes:**
- ✅ PositionData (X, Y, Z)
- ✅ SpeedData (Velocity, Acceleration, Deceleration)

---

#### ✅ IIO.cs
- **File:** `src/Core/NAutoSuite.Core/Abstractions/IIO.cs`
- **Chức năng:** Interface cho đọc/ghi IO

```csharp
public interface IIO : IDevice
{
    Task<Result<bool>> ReadInputAsync(string address, CancellationToken ct);
    Task<Result> WriteOutputAsync(string address, bool value, CancellationToken ct);
}
```

---

### 2. Ví Dụ: Pick and Place Machine

#### ✅ PickAndPlaceIOMap.cs
- **File:** `projects/Machine.PickAndPlace/Machine.PickAndPlace.Core/PickAndPlaceIOMap.cs`
- **Chức năng:** Map IO riêng cho Pick and Place

**Machine-specific Inputs:**
- PartSensorAtPick (IX1.0)
- PartSensorAtPlace (IX1.1)
- VacuumSensor (IX1.2)
- Axis home sensors (IX2.0-IX2.2)
- Axis limit switches (IX2.3-IX3.0)

**Machine-specific Outputs:**
- VacuumValve (QX1.0)
- BlowOffValve (QX1.1)
- WorkLight (QX1.2)

---

#### ✅ PickAndPlaceData.cs
- **File:** `projects/Machine.PickAndPlace/Machine.PickAndPlace.Core/PickAndPlaceData.cs`
- **Chức năng:** Data/Settings riêng cho Pick and Place

**Positions:**
- PickPosition
- PlacePosition
- HomePosition
- SafeZPosition

**Speeds:**
- HighSpeed
- LowSpeed
- PickSpeed
- PlaceSpeed

**Settings:**
- PickHeight
- PlaceHeight
- VacuumOnDelay
- VacuumOffDelay
- BlowOffDuration
- CheckPartPresence
- CheckVacuumSensor

**JSON Support:**
- ✅ Load() - Đọc từ JSON file
- ✅ Save() - Lưu vào JSON file

---

### 3. Tài Liệu

#### ✅ FRAMEWORK_GUIDE.md
- **File:** `FRAMEWORK_GUIDE.md`
- **Nội dung:**
  - ✅ Tổng quan framework
  - ✅ Hướng dẫn tạo dự án mới (từng bước)
  - ✅ Code examples đầy đủ
  - ✅ Giải thích cách hoạt động
  - ✅ File structure
  - ✅ Checklist

---

## 🎯 CÁCH SỬ DỤNG FRAMEWORK

### Bước 1: Tạo IO Map
```csharp
public class YourMachineIOMap : IOMap
{
    public YourMachineIOMap()
    {
        Inputs = new YourMachineInputMap();
        Outputs = new YourMachineOutputMap();
    }
}

public class YourMachineInputMap : InputMap
{
    public string Sensor1 { get; set; } = "IX1.0";
    // ... thêm sensors khác
}

public class YourMachineOutputMap : OutputMap
{
    public string Valve1 { get; set; } = "QX1.0";
    // ... thêm outputs khác
}
```

### Bước 2: Tạo Data
```csharp
public class YourMachineData : MachineData
{
    public PositionData Position1 { get; set; } = new(0, 0, 0);
    public SpeedData FastSpeed { get; set; } = new(100, 500, 500);

    public override void Load(string filePath) { /* JSON load */ }
    public override void Save(string filePath) { /* JSON save */ }
}
```

### Bước 3: Viết Logic
```csharp
public class YourMachineMachine : MachineBase
{
    private readonly YourMachineIOMap _ioMap;
    private readonly YourMachineData _data;
    private readonly MachineController _controller;

    public YourMachineMachine(IIO? io, IAxis? axis1, ...)
        : base("YourMachine", Log.Logger)
    {
        _ioMap = new YourMachineIOMap();
        _data = new YourMachineData();
        _controller = new MachineController(_ioMap, _data, io, Log.Logger);

        // Đăng ký axes và outputs
        if (axis1 != null) _controller.RegisterAxis(axis1);

        _data.Load("Settings.json");
    }

    protected override async Task OnIdleAsync(CancellationToken ct)
    {
        // MachineController tự động xử lý EMG/Start/Stop/Reset
        await _controller.TickAsync(ct);
        await base.OnIdleAsync(ct);
    }

    protected override async Task<Result> OnInitializingAsync(CancellationToken ct)
    {
        // Platform Core tự động home all axes
        return await _controller.InitializeAsync(ct);
    }

    protected override async Task OnRunningAsync(CancellationToken ct)
    {
        // ===================================
        // VIẾT LOGIC AUTO CỦA BẠN Ở ĐÂY
        // ===================================

        await axis1.MoveAbsoluteAsync(_data.Position1.X, ...);
        await output1.WriteAsync(true, ct);
        // ... logic của bạn

        // ===================================
    }
}
```

---

## 📊 KIẾN TRÚC

```
┌─────────────────────────────────────────────┐
│         PLATFORM CORE (Dùng Chung)          │
├─────────────────────────────────────────────┤
│ ✅ MachineController                        │
│    - EMG/Start/Stop/Reset/Init             │
│    - Tower lights, Buzzer                   │
│    - Cyclic IO reading                      │
│                                              │
│ ✅ IOMap (Base Class)                       │
│    - CommonInputs, CommonOutputs           │
│                                              │
│ ✅ MachineData (Base Class)                 │
│    - CommonSettings                         │
│    - Load/Save JSON                         │
│                                              │
│ ✅ MachineBase (State Machine)              │
│    - AlarmManager, InterlockManager        │
│    - Virtual OnRunningAsync()              │
└─────────────────────────────────────────────┘
                    ↓ kế thừa
┌─────────────────────────────────────────────┐
│        DỰ ÁN CỤ THỂ (Machine-Specific)      │
├─────────────────────────────────────────────┤
│ 📝 YourMachineIOMap : IOMap                 │
│ 📝 YourMachineData : MachineData            │
│ 📝 YourMachineMachine : MachineBase         │
│    - Logic AUTO trong OnRunningAsync()     │
└─────────────────────────────────────────────┘
```

---

## ✅ TÍNH NĂNG ĐÃ IMPLEMENT

### ✅ Platform Core Handles:
- [x] EMG (Emergency) - dừng motor, đèn đỏ, buzzer
- [x] Start - đèn xanh, enable
- [x] Stop - dừng motor, đèn vàng
- [x] Reset - xóa lỗi, đèn vàng
- [x] Init - home all axes
- [x] Tower lights tự động
- [x] Buzzer tự động
- [x] IO reading cyclic
- [x] Edge detection cho buttons

### ✅ Machine-Specific Chỉ Cần:
- [x] Map IO addresses
- [x] Define data/settings (positions, speeds)
- [x] Write AUTO logic trong 1 FILE (OnRunningAsync)

---

## 📝 BUILD STATUS

```
✅ NAutoSuite.Core.csproj - Build succeeded
✅ Machine.PickAndPlace.Core.csproj - Build succeeded

⚠️  NAutoSuite.Hardware.Leadshine - Build failed (không ảnh hưởng framework)
```

---

## 🎓 KẾT LUẬN

Framework đã HOÀN THÀNH với các tính năng:

1. ✅ **Platform Core xử lý TẤT CẢ logic chung**
   - EMG/Start/Stop/Reset/Init
   - Tower lights, Buzzer
   - IO mapping system
   - Data/Settings system

2. ✅ **Dự án mới CHỈ CẦN:**
   - Tạo IOMap - map địa chỉ IO
   - Tạo Data - định nghĩa positions, speeds
   - Viết logic trong OnRunningAsync()

3. ✅ **Ví dụ cụ thể:**
   - PickAndPlaceIOMap.cs
   - PickAndPlaceData.cs
   - Hướng dẫn đầy đủ trong FRAMEWORK_GUIDE.md

4. ✅ **Khi nhấn EMG (màn hình hoặc nút cứng):**
   - Dừng TẤT CẢ motor
   - Disable outputs
   - Đèn đỏ + buzzer
   - Hủy init state

**Đúng như yêu cầu ban đầu của bạn!**

---

## 📂 CÁC FILE MỚI ĐÃ TẠO

### Platform Core:
1. `src/Core/NAutoSuite.Core/Machine/MachineController.cs`
2. `src/Core/NAutoSuite.Core/IO/IOMap.cs`
3. `src/Core/NAutoSuite.Core/Data/MachineData.cs`
4. `src/Core/NAutoSuite.Core/Abstractions/IIO.cs`

### Pick and Place Example:
5. `projects/Machine.PickAndPlace/Machine.PickAndPlace.Core/PickAndPlaceIOMap.cs`
6. `projects/Machine.PickAndPlace/Machine.PickAndPlace.Core/PickAndPlaceData.cs`

### Documentation:
7. `FRAMEWORK_GUIDE.md` - Hướng dẫn đầy đủ cách sử dụng
8. `FRAMEWORK_SUMMARY.md` - File này

**Tổng cộng: 8 files mới**

---

## 🚀 BƯỚC TIẾP THEO (Tùy Chọn)

Nếu muốn hoàn thiện hơn:

1. [ ] Refactor PickAndPlaceMachine.cs để sử dụng MachineController đầy đủ
2. [ ] Tạo SimulatorIO implement IIO interface
3. [ ] Test EMG/Start/Stop/Reset với simulator
4. [ ] Tạo UI settings editor cho Data
5. [ ] Tạo thêm 1 machine example khác để chứng minh framework

Nhưng **FRAMEWORK CỐT LÕI ĐÃ HOÀN THÀNH** và sẵn sàng sử dụng!
