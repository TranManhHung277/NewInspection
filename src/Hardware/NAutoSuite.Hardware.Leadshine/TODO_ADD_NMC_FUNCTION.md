# TODO: Thêm function nmc_set_axis_run_mode vào LTDMC.cs

## Vấn đề
Function `nmc_set_axis_run_mode` cần thiết để set CSP mode (mode 8) cho EtherCAT nhưng chưa có trong wrapper LTDMC.cs

## Giải pháp

### Bước 1: Mở file LTDMC.cs
Đường dẫn: `src/Hardware/NAutoSuite.Hardware.Leadshine/LTDMC.cs`

### Bước 2: Thêm function declaration

Tìm vị trí phù hợp (gần các function axis khác) và thêm:

```csharp
/// <summary>
/// Set axis run mode for EtherCAT
/// Mode 8 = CSP (Cyclic Synchronous Position)
/// </summary>
[DllImport("LTDMC.dll", EntryPoint = "nmc_set_axis_run_mode", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
public static extern short nmc_set_axis_run_mode(ushort CardNo, ushort axis, ushort mode);
```

### Bước 3: Uncomment code trong LeadshineAxis.cs

Sau khi thêm function vào LTDMC.cs, mở file:
`src/Hardware/NAutoSuite.Hardware.Leadshine/LeadshineAxis.cs`

Tìm dòng ~138 và uncomment đoạn code:

```csharp
// Uncomment below when nmc_set_axis_run_mode is added to LTDMC.cs:
result = LTDMC.nmc_set_axis_run_mode(_cardNo, _axisIndex, 8);
if (result != 0)
{
    _logger.Warning("Failed to set run mode to CSP for axis {Name}, error code: {ErrorCode}", Name, result);
}
else
{
    _logger.Information("Axis {Name} run mode set to CSP (mode 8)", Name);
}
```

### Bước 4: Xóa dòng log tạm thời

Xóa dòng:
```csharp
_logger.Information("Axis {Name} initialized (CSP mode should be configured in driver)", Name);
```

## Note

Hiện tại code vẫn chạy được mà không cần function này, vì:
- CSP mode có thể đã được configure sẵn trong driver
- Hoặc mode mặc định đã phù hợp

Nhưng để đảm bảo 100%, nên thêm function này vào.

## Tham khảo

Code mẫu từ bạn:
```csharp
// 3. Đưa Driver về chế độ CSP (Quan trọng nhất cho EtherCAT)
LTDMC.nmc_set_axis_run_mode(cardNo, axis, 8);
```
