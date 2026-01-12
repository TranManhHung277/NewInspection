# Hướng Dẫn Sử Dụng IO Map và Scan Cycle

## 📍 Nơi Khai Báo Địa Chỉ IO

### File: `PickAndPlaceIOMap.cs`

Đây là nơi **DUY NHẤT** bạn khai báo tất cả địa chỉ IO của máy.

```csharp
// Khai báo inputs của máy
public class PickAndPlaceInputMap : InputMap
{
    // Sensors
    public string PartSensorAtPick { get; set; } = "IX1.0";
    public string PartSensorAtPlace { get; set; } = "IX1.1";
    public string VacuumSensor { get; set; } = "IX1.2";

    // Thêm sensor mới tại đây
    public string DoorSensor { get; set; } = "IX1.3";
    public string PressureSensor { get; set; } = "IX1.4";
}

// Khai báo outputs của máy
public class PickAndPlaceOutputMap : OutputMap
{
    // Actuators
    public string VacuumValve { get; set; } = "QX1.0";
    public string BlowOffValve { get; set; } = "QX1.1";

    // Thêm output mới tại đây
    public string Cylinder1 { get; set; } = "QX1.3";
    public string Cylinder2 { get; set; } = "QX1.4";
}
```

---

## 🔄 Vòng Quét Máy (Scan Cycle)

### File: `PickAndPlaceMachine.cs`

Vòng quét được khai báo trong constructor của máy:

```csharp
public PickAndPlaceMachine(...)
{
    // ...

    // Setup scan cycle timer
    SetupScanCycle();
}

private void SetupScanCycle()
{
    _scanTimer = new System.Timers.Timer(SCAN_CYCLE_MS);
    _scanTimer.Elapsed += async (sender, e) => await ScanCycleAsync();
    _scanTimer.AutoReset = true;
}
```

### Chu Kỳ Quét

```
SCAN_CYCLE_MS = 100ms  →  Tần số quét = 10Hz (10 lần/giây)
```

Bạn có thể thay đổi chu kỳ quét:
- **50ms** = 20Hz (nhanh hơn, dùng cho máy cần phản ứng nhanh)
- **100ms** = 10Hz (tiêu chuẩn)
- **200ms** = 5Hz (chậm hơn, dùng cho máy không cần phản ứng nhanh)

---

## 📝 Cách Sử Dụng IO Map Trong Logic

### 1. Truy Cập IO Map

```csharp
// Trong PickAndPlaceMachine.cs
public class PickAndPlaceMachine : MachineBase
{
    private readonly PickAndPlaceIOMap _ioMap;

    public PickAndPlaceMachine(...)
    {
        _ioMap = new PickAndPlaceIOMap();
    }
}
```

### 2. Sử Dụng Địa Chỉ IO Trong Logic

#### Ví Dụ 1: Đọc Input (Sensor)

```csharp
// Đọc part sensor tại vị trí pick
string address = _ioMap.MachineInputs.PartSensorAtPick;  // "IX1.0"
bool hasPart = await plc.ReadInputAsync(address);

if (hasPart)
{
    Log.Information("Part detected at pick position");
}
```

#### Ví Dụ 2: Ghi Output (Valve)

```csharp
// Bật vacuum valve
string address = _ioMap.MachineOutputs.VacuumValve;  // "QX1.0"
await plc.WriteOutputAsync(address, true);

// Tắt vacuum valve
await plc.WriteOutputAsync(address, false);
```

#### Ví Dụ 3: Đọc Common Input (Nút EMG)

```csharp
// Đọc nút Emergency Stop
string address = _ioMap.CommonInputs.EmergencyStop;  // "IX0.0"
bool emgPressed = await plc.ReadInputAsync(address);

if (emgPressed)
{
    await HandleEmergencyStopAsync();
}
```

#### Ví Dụ 4: Ghi Common Output (Đèn tháp)

```csharp
// Bật đèn đỏ khi có lỗi
await plc.WriteOutputAsync(_ioMap.CommonOutputs.RedLight, true);
await plc.WriteOutputAsync(_ioMap.CommonOutputs.GreenLight, false);
await plc.WriteOutputAsync(_ioMap.CommonOutputs.Buzzer, true);
```

---

## 🔁 Scan Cycle - Vòng Quét Chính

### Nhiệm Vụ Của Scan Cycle

```csharp
private async Task ScanCycleAsync()
{
    try
    {
        // ========== BƯỚC 1: ĐỌC TẤT CẢ INPUTS ==========
        // Đọc từ PLC vào buffer
        foreach (var address in _ioMap.GetAllInputAddresses())
        {
            bool value = await plc.ReadInputAsync(address);
            inputBuffer[address] = value;  // Lưu vào buffer
        }

        // ========== BƯỚC 2: KIỂM TRA CÁC NÚT BẤM ==========

        // Emergency Stop
        if (inputBuffer[_ioMap.CommonInputs.EmergencyStop])
        {
            await HandleEmergencyStopAsync();
            return;  // Dừng ngay lập tức
        }

        // Start Button
        if (inputBuffer[_ioMap.CommonInputs.StartButton] &&
            State == MachineState.Idle)
        {
            await StartAsync();
        }

        // Stop Button
        if (inputBuffer[_ioMap.CommonInputs.StopButton] &&
            State == MachineState.Running)
        {
            await StopAsync();
        }

        // Reset Button
        if (inputBuffer[_ioMap.CommonInputs.ResetButton] &&
            HasAlarm)
        {
            await ResetAsync();
        }

        // ========== BƯỚC 3: KIỂM TRA SENSORS ==========

        // Kiểm tra vacuum sensor
        if (!inputBuffer[_ioMap.MachineInputs.VacuumSensor] &&
            outputBuffer[_ioMap.MachineOutputs.VacuumValve])
        {
            // Vacuum ON nhưng không có áp suất → Lỗi
            RaiseAlarm(3001, "Vacuum pressure lost", AlarmSeverity.Critical);
        }

        // Kiểm tra limit switches
        if (inputBuffer[_ioMap.MachineInputs.AxisXPositiveLimit])
        {
            RaiseAlarm(3002, "Axis X positive limit triggered", AlarmSeverity.Warning);
        }

        // ========== BƯỚC 4: CẬP NHẬT ĐÈN THÁP ==========

        switch (State)
        {
            case MachineState.Idle:
                outputBuffer[_ioMap.CommonOutputs.GreenLight] = false;
                outputBuffer[_ioMap.CommonOutputs.YellowLight] = true;  // Vàng
                outputBuffer[_ioMap.CommonOutputs.RedLight] = false;
                break;

            case MachineState.Running:
                outputBuffer[_ioMap.CommonOutputs.GreenLight] = true;   // Xanh
                outputBuffer[_ioMap.CommonOutputs.YellowLight] = false;
                outputBuffer[_ioMap.CommonOutputs.RedLight] = false;
                break;

            case MachineState.Alarm:
                outputBuffer[_ioMap.CommonOutputs.GreenLight] = false;
                outputBuffer[_ioMap.CommonOutputs.YellowLight] = false;
                outputBuffer[_ioMap.CommonOutputs.RedLight] = true;     // Đỏ
                outputBuffer[_ioMap.CommonOutputs.Buzzer] = true;       // Kêu
                break;
        }

        // ========== BƯỚC 5: GHI TẤT CẢ OUTPUTS ==========
        // Ghi từ buffer ra PLC
        foreach (var address in _ioMap.GetAllOutputAddresses())
        {
            bool value = outputBuffer[address];
            await plc.WriteOutputAsync(address, value);
        }
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Scan cycle error");
    }
}
```

---

## 🎯 Luồng Hoạt Động Của Máy

```
┌─────────────────────────────────────────────────────┐
│  Program.cs (Entry Point)                           │
│  - Khởi tạo PLC/Simulator                          │
│  - Tạo PickAndPlaceMachine                         │
│  - Gọi machine.InitializeAsync()                   │
│  - Gọi machine.StartScanCycle()  ← BẬT VÒNG QUÉT  │
└─────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────┐
│  Scan Cycle (Chạy liên tục mỗi 100ms)              │
│  ┌───────────────────────────────────────────────┐ │
│  │ 1. Đọc tất cả inputs từ PLC                   │ │
│  │ 2. Kiểm tra EMG, Start, Stop, Reset           │ │
│  │ 3. Kiểm tra sensors, limit switches           │ │
│  │ 4. Cập nhật đèn tháp theo trạng thái máy      │ │
│  │ 5. Ghi tất cả outputs ra PLC                  │ │
│  └───────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────────────────┐
│  AUTO Mode Logic (Khi nhấn Start)                  │
│  - OnRunningAsync() được gọi                       │
│  - PickSequenceAsync()                             │
│  - PlaceSequenceAsync()                            │
│  - Lặp lại cho đến khi Stop                        │
└─────────────────────────────────────────────────────┘
```

---

## 📋 Checklist: Tạo Dự Án Mới

### Bước 1: Khai Báo IO Map
✅ Vào `YourMachineIOMap.cs`
✅ Thêm tất cả inputs vào `YourMachineInputMap`
✅ Thêm tất cả outputs vào `YourMachineOutputMap`
✅ Implement `GetAllInputAddresses()` và `GetAllOutputAddresses()`

### Bước 2: Khai Báo Máy
✅ Vào `YourMachine.cs`
✅ Thêm `_ioMap` và `_data` fields
✅ Khởi tạo trong constructor
✅ Gọi `SetupScanCycle()` trong constructor

### Bước 3: Implement Scan Cycle
✅ Viết `ScanCycleAsync()` method
✅ Đọc inputs → Xử lý logic → Ghi outputs

### Bước 4: Implement AUTO Logic
✅ Override `OnRunningAsync()`
✅ Viết sequence logic (Pick, Place, etc.)
✅ Sử dụng `_ioMap` để truy cập địa chỉ IO

### Bước 5: Test
✅ Build Backend
✅ Run Backend
✅ Kiểm tra scan cycle chạy đúng
✅ Kiểm tra AUTO mode hoạt động

---

## 💡 Tips

1. **KHÔNG hardcode địa chỉ IO trong logic**
   ```csharp
   ❌ BAD:  await plc.ReadInputAsync("IX1.0");
   ✅ GOOD: await plc.ReadInputAsync(_ioMap.MachineInputs.PartSensorAtPick);
   ```

2. **Sử dụng GetAllInputAddresses() để quét tất cả**
   ```csharp
   foreach (var address in _ioMap.GetAllInputAddresses())
   {
       bool value = await plc.ReadInputAsync(address);
   }
   ```

3. **Scan Cycle CHỈ nên quét IO và kiểm tra trạng thái**
   - KHÔNG viết logic AUTO trong scan cycle
   - Logic AUTO nên ở `OnRunningAsync()`

4. **Tần số quét phù hợp**
   - 100ms (10Hz) là tiêu chuẩn cho hầu hết máy
   - Nếu cần phản ứng nhanh hơn → giảm xuống 50ms
   - Nếu PLC chậm → tăng lên 200ms

---

## 🎓 Ví Dụ Hoàn Chỉnh

Xem các file sau để tham khảo:
- `PickAndPlaceIOMap.cs` - Cách khai báo IO
- `PickAndPlaceMachine.cs` - Cách sử dụng IO Map và Scan Cycle
- `Program.cs` - Cách khởi tạo và chạy máy
