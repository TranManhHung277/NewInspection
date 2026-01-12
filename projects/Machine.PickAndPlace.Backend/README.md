# Pick and Place Machine - Backend

## 📍 Nơi Khai Báo Địa Chỉ IO

### File: `Machine/PickAndPlaceIOMap.cs`

Đây là nơi **DUY NHẤT** bạn khai báo tất cả địa chỉ IO của máy.

```csharp
public class PickAndPlaceIOMap : IOMap
{
    // Properties để truy cập nhanh
    public PickAndPlaceInputMap MachineInputs => (PickAndPlaceInputMap)Inputs;
    public PickAndPlaceOutputMap MachineOutputs => (PickAndPlaceOutputMap)Outputs;

    // Methods để lấy danh sách địa chỉ (dùng trong vòng quét)
    public List<string> GetAllInputAddresses()  // Tất cả inputs
    public List<string> GetAllOutputAddresses() // Tất cả outputs
}
```

#### Common Inputs (Có sẵn trong tất cả máy)
```csharp
CommonInputs.EmergencyStop  // IX0.0 - Nút EMG
CommonInputs.StartButton    // IX0.1 - Nút Start
CommonInputs.StopButton     // IX0.2 - Nút Stop
CommonInputs.ResetButton    // IX0.3 - Nút Reset
CommonInputs.SafetyDoor     // IX0.4 - Cửa an toàn
CommonInputs.AirPressure    // IX0.5 - Cảm biến khí nén
```

#### Machine-Specific Inputs (Riêng cho máy này)
```csharp
MachineInputs.PartSensorAtPick      // IX1.0 - Cảm biến linh kiện tại pick
MachineInputs.PartSensorAtPlace     // IX1.1 - Cảm biến linh kiện tại place
MachineInputs.VacuumSensor          // IX1.2 - Cảm biến chân không
MachineInputs.AxisXHomeSensor       // IX2.0 - Home X
MachineInputs.AxisYHomeSensor       // IX2.1 - Home Y
MachineInputs.AxisZHomeSensor       // IX2.2 - Home Z
MachineInputs.AxisXPositiveLimit    // IX2.3 - Limit + X
MachineInputs.AxisXNegativeLimit    // IX2.4 - Limit - X
// ... các limit khác
```

#### Common Outputs (Có sẵn trong tất cả máy)
```csharp
CommonOutputs.TowerLightRed     // QX0.0 - Đèn đỏ (Lỗi/EMG)
CommonOutputs.TowerLightYellow  // QX0.1 - Đèn vàng (Cảnh báo/Idle)
CommonOutputs.TowerLightGreen   // QX0.2 - Đèn xanh (Chạy)
CommonOutputs.Buzzer            // QX0.3 - Còi báo động
CommonOutputs.MainPowerEnable   // QX0.4 - Nguồn chính
```

#### Machine-Specific Outputs (Riêng cho máy này)
```csharp
MachineOutputs.VacuumValve   // QX1.0 - Van chân không
MachineOutputs.BlowOffValve  // QX1.1 - Van thổi khí
MachineOutputs.WorkLight     // QX1.2 - Đèn làm việc
```

---

## 🔄 Vòng Quét Máy (Scan Cycle)

### File: `Machine/PickAndPlaceMachine.cs`

```csharp
public class PickAndPlaceMachine : MachineBase
{
    private System.Timers.Timer? _scanTimer;
    private const int SCAN_CYCLE_MS = 100; // 100ms = 10Hz (10 lần/giây)

    // IO Map - Truy cập địa chỉ IO
    public PickAndPlaceIOMap IOMap => _ioMap;

    // Machine Data - Cài đặt và thông số
    public PickAndPlaceData Data => _data;
}
```

### Các Method Quan Trọng

#### 1. `SetupScanCycle()` - Khởi tạo vòng quét
```csharp
private void SetupScanCycle()
{
    _scanTimer = new System.Timers.Timer(SCAN_CYCLE_MS);
    _scanTimer.Elapsed += async (sender, e) => await ScanCycleAsync();
    _scanTimer.AutoReset = true;
}
```

#### 2. `StartScanCycle()` - Bật vòng quét
```csharp
public void StartScanCycle()
{
    _scanTimer?.Start();
    Log.Logger.Information("Scan cycle started (Interval: {Interval}ms)", SCAN_CYCLE_MS);
}
```

Gọi method này trong `Program.cs` sau khi khởi tạo máy:
```csharp
var machine = new PickAndPlaceMachine(...);
await machine.InitializeAsync();
machine.StartScanCycle();  // ← Bật vòng quét
```

#### 3. `StopScanCycle()` - Tắt vòng quét
```csharp
public void StopScanCycle()
{
    _scanTimer?.Stop();
    Log.Logger.Information("Scan cycle stopped");
}
```

#### 4. `ScanCycleAsync()` - Vòng quét chính
```csharp
private async Task ScanCycleAsync()
{
    // 1. Đọc tất cả inputs từ PLC
    // 2. Kiểm tra EMG, Start, Stop, Reset
    // 3. Kiểm tra sensors, limit switches
    // 4. Cập nhật đèn tháp theo trạng thái
    // 5. Ghi tất cả outputs ra PLC
}
```

**CHI TIẾT IMPLEMENTATION**: Xem file `IOMAP_USAGE_GUIDE.md`

---

## 🎯 Cách Sử Dụng IO Map Trong Logic

### Ví Dụ 1: Đọc Sensor
```csharp
// Trong PickSequenceAsync()
string address = _ioMap.MachineInputs.PartSensorAtPick;
bool hasPart = await _partSensor.ReadAsync(); // hoặc plc.ReadInputAsync(address)

if (hasPart)
{
    Log.Information("Part detected at pick position");
    // Tiếp tục logic pick
}
else
{
    RaiseAlarm(2001, "No part at pick position", AlarmSeverity.Warning);
}
```

### Ví Dụ 2: Điều Khiển Output
```csharp
// Bật vacuum
await _vacuum.WriteAsync(true);  // hoặc plc.WriteOutputAsync(_ioMap.MachineOutputs.VacuumValve, true)
Log.Information("Vacuum activated");

await Task.Delay(200); // Chờ vacuum ổn định

// Kiểm tra vacuum sensor
bool hasVacuum = await plc.ReadInputAsync(_ioMap.MachineInputs.VacuumSensor);
if (!hasVacuum)
{
    RaiseAlarm(2002, "Vacuum pressure not detected", AlarmSeverity.Critical);
}
```

### Ví Dụ 3: Kiểm Tra Common Input
```csharp
// Kiểm tra cửa an toàn
bool doorOpen = await plc.ReadInputAsync(_ioMap.CommonInputs.SafetyDoor);
if (doorOpen)
{
    await StopAsync();
    RaiseAlarm(1001, "Safety door is open", AlarmSeverity.Critical);
    return;
}
```

### Ví Dụ 4: Điều Khiển Đèn Tháp
```csharp
// Đèn xanh khi chạy
await plc.WriteOutputAsync(_ioMap.CommonOutputs.TowerLightGreen, true);
await plc.WriteOutputAsync(_ioMap.CommonOutputs.TowerLightYellow, false);
await plc.WriteOutputAsync(_ioMap.CommonOutputs.TowerLightRed, false);

// Đèn đỏ + còi khi lỗi
await plc.WriteOutputAsync(_ioMap.CommonOutputs.TowerLightRed, true);
await plc.WriteOutputAsync(_ioMap.CommonOutputs.TowerLightGreen, false);
await plc.WriteOutputAsync(_ioMap.CommonOutputs.Buzzer, true);
```

---

## 📂 Cấu Trúc File

```
Machine.PickAndPlace.Backend/
├── Program.cs                      ← Entry point, khởi tạo máy, chạy AUTO
├── Machine/
│   ├── PickAndPlaceMachine.cs      ← Logic máy, AUTO sequence
│   ├── PickAndPlaceData.cs         ← Data/Settings
│   └── PickAndPlaceIOMap.cs        ← ★ IO MAPPING (Khai báo địa chỉ IO)
├── Manual/
│   └── ManualController.cs         ← Manual operations
├── Stations/
│   ├── PickStation.cs
│   └── PlaceStation.cs
├── README.md                       ← File này
└── IOMAP_USAGE_GUIDE.md           ← Hướng dẫn chi tiết
```

---

## 🚀 Build và Run

```bash
cd Machine.PickAndPlace.Backend
dotnet build
dotnet run
```

### Output Mẫu
```
=== PickAndPlace Backend Started ===
Machine created: PickAndPlaceMachine
Scan cycle started (Interval: 100ms)
Initializing machine (Homing all axes)...
Machine initialized successfully
Starting AUTO mode...
Pick sequence started
Pick sequence completed
Place sequence completed
Cycle 1 completed successfully
Press any key to stop...
```

---

## 💡 Tips Quan Trọng

### 1. KHÔNG hardcode địa chỉ IO trong logic
```csharp
❌ BAD:  await plc.ReadInputAsync("IX1.0");
✅ GOOD: await plc.ReadInputAsync(_ioMap.MachineInputs.PartSensorAtPick);
```

### 2. Sử dụng `GetAllInputAddresses()` trong scan cycle
```csharp
// Quét tất cả inputs
foreach (var address in _ioMap.GetAllInputAddresses())
{
    bool value = await plc.ReadInputAsync(address);
    inputBuffer[address] = value;
}
```

### 3. Scan Cycle CHỈ quét IO, KHÔNG viết logic AUTO
- Scan cycle: Đọc inputs, xử lý nút bấm, cập nhật đèn tháp
- AUTO logic: Viết trong `OnRunningAsync()`, `PickSequenceAsync()`, `PlaceSequenceAsync()`

### 4. Tần số quét phù hợp
- **50ms (20Hz)**: Máy cần phản ứng rất nhanh
- **100ms (10Hz)**: Tiêu chuẩn (khuyến nghị) ✅
- **200ms (5Hz)**: Máy chậm, PLC chậm

---

## 📖 Tài Liệu Tham Khảo

- **IOMAP_USAGE_GUIDE.md** - Hướng dẫn chi tiết về IO Map và Scan Cycle
- **PICKANDPLACE_STRUCTURE.md** - Cấu trúc tổng quan của dự án

---

## ✅ Checklist Khi Viết Logic Mới

- [ ] Khai báo IO trong `PickAndPlaceIOMap.cs`
- [ ] Thêm địa chỉ vào `GetAllInputAddresses()` hoặc `GetAllOutputAddresses()`
- [ ] Sử dụng `_ioMap.MachineInputs.TênSensor` trong logic
- [ ] KHÔNG hardcode địa chỉ ("IX1.0")
- [ ] Kiểm tra sensor trước khi thực hiện động tác
- [ ] Raise alarm khi có lỗi
- [ ] Log các bước quan trọng
- [ ] Test với Simulator trước
