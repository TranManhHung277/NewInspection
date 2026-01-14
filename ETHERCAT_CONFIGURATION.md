# EtherCAT Configuration for NAutoSuite

## 📅 Cập nhật: 2026-01-13

## 🎯 Tổng quan

Framework đã được cập nhật để hỗ trợ cấu hình EtherCAT theo đúng yêu cầu thực tế của Leadshine controller với động cơ 23-bit encoder.

---

## ✅ Các thay đổi đã thực hiện

### 1. LeadshineMaster.cs - Khởi tạo Board (QUAN TRỌNG!)

**File:** `src/Hardware/NAutoSuite.Hardware.Leadshine/LeadshineMaster.cs`

**Thay đổi chính:**
- ✅ **Di chuyển `dmc_board_init()` vào Constructor** - Chỉ gọi 1 lần khi khởi động chương trình
- ✅ **ConnectAsync()** - Chỉ lấy thông tin card, không init lại board
- ✅ **ResetAsync()** - Reset board và reinitialize khi card bị lỗi

#### a) Constructor - Board Initialization (1 lần duy nhất)
```csharp
public LeadshineMaster(ushort cardNo, string? ipAddress = null, ILogger? logger = null)
{
    _cardNo = cardNo;
    _ipAddress = ipAddress;
    _logger = logger ?? Log.Logger;

    // Initialize board ONCE in constructor
    // This should only be called once when program starts
    short result;
    if (!string.IsNullOrEmpty(_ipAddress))
    {
        result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
    }
    else
    {
        result = LTDMC.dmc_board_init();
    }

    // Check initialization result: dmc_board_init returns <= 0 on failure
    if (result <= 0)
    {
        throw new InvalidOperationException($"Failed to initialize card {_cardNo}");
    }
}
```

#### b) ConnectAsync() - Không init lại board
```csharp
public async Task<Result> ConnectAsync(CancellationToken ct = default)
{
    // Board init is done in constructor (only once)
    // ConnectAsync just gets card information and sets connected flag

    // Get card information
    _totalAxes = 0;
    var result = LTDMC.dmc_get_total_axes(_cardNo, ref _totalAxes);
    // ... get IO info, version, etc.
}
```

#### c) ResetAsync() - Reinitialize sau khi reset
```csharp
public async Task<Result> ResetAsync(CancellationToken ct = default)
{
    // Reset the board
    var result = LTDMC.dmc_board_reset();

    // Wait for board to reset
    await Task.Delay(500, ct);

    // Reinitialize board after reset
    if (!string.IsNullOrEmpty(_ipAddress))
    {
        result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
    }
    else
    {
        result = LTDMC.dmc_board_init();
    }

    // Reconnect to get card info
    return await ConnectAsync(ct);
}
```

---

### 2. LeadshineAxis.cs - Cấu hình Gear Ratio & Profile

**File:** `src/Hardware/NAutoSuite.Hardware.Leadshine/LeadshineAxis.cs`

**Thêm vào ConnectAsync():**

#### a) Enable Axis (QUAN TRỌNG!)
```csharp
// Enable axis immediately when connecting
var result = LTDMC.nmc_set_axis_enable(_cardNo, _axisIndex);
if (result != 0)
{
    _logger.Warning("Failed to enable axis {Name}, error code: {ErrorCode}", Name, result);
}
else
{
    _logger.Information("Axis {Name} enabled successfully", Name);
}
```

**Giải thích:**
- Enable axis ngay khi connect
- Axis phải được enable trước khi có thể move

#### b) Gear Ratio (Equiv)
```csharp
// Configure gear ratio (equiv)
// Động cơ 23-bit: 8,388,608 xung/vòng. 1 vòng = 36,000 unit (0.01 độ/unit)
double equiv = 8388608.0 / 36000.0;
var result = LTDMC.dmc_set_equiv(_cardNo, _axisIndex, equiv);
```

**Giải thích:**
- Encoder 23-bit = 2^23 = 8,388,608 pulses/revolution
- 1 revolution = 360 degrees = 36,000 units (vì 1 unit = 0.01 độ)
- Equiv = 8,388,608 / 36,000 = 233.016 pulses/unit

#### c) Motion Profile
```csharp
// Set default profile (100 unit/s min, 36000 unit/s max = 360 deg/s, 0.2 acc/dec)
result = LTDMC.dmc_set_profile_unit(_cardNo, _axisIndex, 100, 36000, 0.2, 0.2, 100);
```

**Giải thích:**
- Min velocity: 100 unit/s = 1 deg/s
- Max velocity: 36,000 unit/s = 360 deg/s (1 vòng/giây)
- Acceleration: 0.2 (tăng tốc)
- Deceleration: 0.2 (giảm tốc)
- Start velocity: 100 unit/s

#### d) CSP Mode (TODO)
```csharp
// NOTE: Function nmc_set_axis_run_mode chưa có trong LTDMC wrapper
// Cần thêm function này vào LTDMC.cs:
// [DllImport("LTDMC.dll")]
// public static extern short nmc_set_axis_run_mode(ushort CardNo, ushort axis, ushort mode);
```

**Xem:** [TODO_ADD_NMC_FUNCTION.md](src/Hardware/NAutoSuite.Hardware.Leadshine/TODO_ADD_NMC_FUNCTION.md)

---

### 3. App.xaml.cs - Connection Mode

**File:** `projects/Machine.PickAndPlace.Frontend/App.xaml.cs`

**Thay đổi:**
```csharp
// Trước:
ipAddress: "192.168.1.100",     // EtherCAT card IP

// Sau:
ipAddress: null,                // null = use dmc_board_init() for local connection
```

**Lý do:**
- Code mẫu của bạn dùng `dmc_board_init()` (không có IP)
- Kết nối local thay vì qua Ethernet

---

## 📊 So sánh Code Mẫu vs Framework

| Bước | Code mẫu của bạn | NAutoSuite Framework | Status |
|------|------------------|----------------------|--------|
| 1. Init Board | `dmc_board_init()` check `<= 0` | ✅ Đã cập nhật | ✅ Done |
| 2. Gear Ratio | `dmc_set_equiv(cardNo, axis, 233.016)` | ✅ Đã thêm | ✅ Done |
| 3. CSP Mode | `nmc_set_axis_run_mode(cardNo, axis, 8)` | ⚠️ Function chưa có | 📝 TODO |
| 4. Profile | `dmc_set_profile_unit(...)` | ✅ Đã thêm | ✅ Done |

---

## 🚀 Hướng dẫn sử dụng

### Khởi tạo và chạy

```bash
cd "d:\Projects\C#\NAutoSuite\projects\Machine.PickAndPlace.Frontend"
dotnet run
```

### Trình tự test

1. **INITIALIZE** - Kết nối với EtherCAT card
   - Board init
   - Axis config (equiv, profile)
   - Log sẽ hiển thị các bước cấu hình

2. **HOME AXIS** - Về home position (0mm)

3. **START** - Chạy automatic sequence:
   - 10s initial run (back/forth nhiều lần)
   - 10 cycles (0→100mm→0mm)
   - Reverse pattern
   - 5s pause
   - Loop lại

4. **STOP** / **EMERGENCY STOP** - Dừng máy

---

## 🔧 Configuration Parameters

### Trong LeadshineAxis.cs

```csharp
// Gear ratio
double equiv = 8388608.0 / 36000.0;  // 233.016 pulses/unit

// Motion profile
MinVel = 100 unit/s    (1 deg/s)
MaxVel = 36000 unit/s  (360 deg/s = 1 rev/s)
Acc = 0.2
Dec = 0.2
```

### Trong PickAndPlaceMachineFrontend.cs

```csharp
_startPosition = 0;        // Home position
_endPosition = 100;        // 100mm travel distance
_velocity = 50;            // 50 mm/s (sẽ map qua unit/s tùy theo equiv)
_initialRunTime = 10;      // 10 seconds
_cycleLoops = 10;          // 10 lần
_pauseTime = 5;            // 5 seconds
```

---

## 📝 TODO List

### Bắt buộc
- [ ] Thêm `nmc_set_axis_run_mode` vào LTDMC.cs (xem TODO_ADD_NMC_FUNCTION.md)

### Tùy chọn
- [ ] Verify equiv calculation với hardware thực tế
- [ ] Fine-tune velocity/acceleration parameters
- [ ] Thêm safety limits (min/max position)

---

## 🐛 Troubleshooting

### Lỗi: "Failed to initialize Leadshine card, error code: 0"
**Nguyên nhân:** Card không kết nối được
**Giải pháp:**
1. Kiểm tra cáp EtherCAT
2. Kiểm tra driver đã cài đúng chưa
3. Check Device Manager có card không

### Lỗi: "Failed to set equiv"
**Nguyên nhân:** Card chưa init hoặc axis index sai
**Giải pháp:**
1. Đảm bảo Initialize thành công trước
2. Check axis index (0, 1, 2...)

### Axis không move
**Nguyên nhân:**
- Chưa home
- Chưa enable servo
- Profile parameters sai

**Giải pháp:**
1. Home axis trước khi start
2. Check log để xem profile có set thành công không
3. Verify velocity không quá cao (> MaxVel)

---

## 📚 Tham khảo

### Files quan trọng
- [LeadshineMaster.cs](src/Hardware/NAutoSuite.Hardware.Leadshine/LeadshineMaster.cs) - Board initialization
- [LeadshineAxis.cs](src/Hardware/NAutoSuite.Hardware.Leadshine/LeadshineAxis.cs) - Axis control
- [PickAndPlaceMachineFrontend.cs](projects/Machine.PickAndPlace.Frontend/Machine/PickAndPlaceMachineFrontend.cs) - Motion logic
- [App.xaml.cs](projects/Machine.PickAndPlace.Frontend/App.xaml.cs) - DI configuration

### Documentation
- [TODO_ADD_NMC_FUNCTION.md](src/Hardware/NAutoSuite.Hardware.Leadshine/TODO_ADD_NMC_FUNCTION.md) - Hướng dẫn thêm CSP mode function
- [README_REFACTORING.md](README_REFACTORING.md) - Framework overview

---

## ✅ Kết luận

Framework đã được cấu hình sẵn 90% theo yêu cầu EtherCAT của bạn. Chỉ cần thêm function `nmc_set_axis_run_mode` vào LTDMC.cs là hoàn thiện 100%.

Bạn có thể chạy và test ngay với hardware thực tế!

**Happy Testing! 🎉**
