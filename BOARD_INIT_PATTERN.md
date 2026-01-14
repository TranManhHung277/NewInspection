# Board Initialization Pattern - LeadshineMaster

## 🎯 Yêu cầu

> **LTDMC.dmc_board_init() chỉ dùng 1 lần khi chạy chương trình và khi card bị lỗi sau khi reset lại sẽ init card lại**

## ✅ Giải pháp đã implement

### Kiến trúc

```
Program Start
    ↓
LeadshineMaster Constructor
    ↓
dmc_board_init() ← ONCE ONLY!
    ↓
[Program Running]
    ↓
    ├─→ ConnectAsync() → Get card info (NO init!)
    ├─→ Axis.ConnectAsync() → Config axis (NO board init!)
    └─→ ResetAsync() → Reset + Reinit board
```

---

## 📋 Chi tiết Implementation

### 1. Constructor - Board Init (1 lần)

**File:** `LeadshineMaster.cs`

```csharp
public LeadshineMaster(ushort cardNo, string? ipAddress = null, ILogger? logger = null)
{
    _cardNo = cardNo;
    _ipAddress = ipAddress;
    _logger = logger ?? Log.Logger;
    Id = $"Leadshine_Card{cardNo}";
    Name = $"Leadshine EtherCAT Master #{cardNo}";

    // Initialize board ONCE in constructor
    short result;
    if (!string.IsNullOrEmpty(_ipAddress))
    {
        _logger.Information("Initializing Leadshine Master {CardNo} via Ethernet at {IpAddress}",
            _cardNo, _ipAddress);
        result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
    }
    else
    {
        _logger.Information("Initializing Leadshine Master {CardNo} locally (dmc_board_init)",
            _cardNo);
        result = LTDMC.dmc_board_init();
    }

    // Check result: <= 0 means failure
    if (result <= 0)
    {
        var errorMsg = $"Failed to initialize Leadshine card {_cardNo}, error code: {result}";
        _logger.Error(errorMsg);
        throw new InvalidOperationException(errorMsg);
    }

    _logger.Information("Leadshine Master {CardNo} board initialized successfully", _cardNo);
}
```

**Khi nào gọi:**
- ✅ Khi tạo instance `LeadshineMaster` lần đầu (program start)
- ❌ KHÔNG gọi khi Connect/Reconnect axis
- ❌ KHÔNG gọi mỗi lần axis.ConnectAsync()

**Exception:**
- Nếu init failed → throw `InvalidOperationException`
- Program sẽ không start nếu board không init được

---

### 2. ConnectAsync() - Get Card Info (NO Init!)

```csharp
public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
{
    try
    {
        // Board init is done in constructor (only once)
        // ConnectAsync just gets card information and sets connected flag

        // Get card information
        _totalAxes = 0;
        var result = LTDMC.dmc_get_total_axes(_cardNo, ref _totalAxes);
        if (result != 0)
        {
            _logger.Warning("Failed to get total axes");
        }

        _totalInputs = 0;
        _totalOutputs = 0;
        result = LTDMC.dmc_get_total_ionum(_cardNo, ref _totalInputs, ref _totalOutputs);
        if (result != 0)
        {
            _logger.Warning("Failed to get IO count");
        }

        uint cardVersion = 0;
        result = LTDMC.dmc_get_card_version(_cardNo, ref cardVersion);
        if (result == 0)
        {
            _logger.Information("Leadshine Card Version: 0x{Version:X}", cardVersion);
        }

        _isConnected = true;
        _logger.Information("Master connected - Axes: {Axes}, IO: {In}/{Out}",
            _totalAxes, _totalInputs, _totalOutputs);

        return Result.Success($"Connected (Axes: {_totalAxes})");
    }
    catch (Exception ex)
    {
        return Result.Failure("Connection failed", ex);
    }
}
```

**Khi nào gọi:**
- ✅ Sau khi LeadshineMaster được tạo (DI inject)
- ✅ Từ Machine.OnInitializingAsync()
- ✅ Khi cần refresh card info

**Không gọi:**
- ❌ dmc_board_init() (đã init trong constructor rồi!)

---

### 3. ResetAsync() - Reset + Reinit Board

```csharp
public async Task<Result> ResetAsync(CancellationToken cancellationToken = default)
{
    try
    {
        _logger.Warning("Resetting Leadshine Master {CardNo} - This will reinitialize the board",
            _cardNo);

        // 1. Reset the board
        var result = LTDMC.dmc_board_reset();
        if (result != 0)
        {
            var errorMsg = $"Failed to reset card {_cardNo}, error code: {result}";
            _logger.Error(errorMsg);
            return Result.Failure(errorMsg);
        }

        _isConnected = false;

        // 2. Wait for board to reset
        await Task.Delay(500, cancellationToken);

        // 3. Reinitialize board after reset
        _logger.Information("Reinitializing board after reset...");
        if (!string.IsNullOrEmpty(_ipAddress))
        {
            result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
        }
        else
        {
            result = LTDMC.dmc_board_init();
        }

        if (result <= 0)
        {
            var errorMsg = $"Failed to reinitialize board, error code: {result}";
            _logger.Error(errorMsg);
            return Result.Failure(errorMsg);
        }

        _logger.Information("Board reset and reinitialized successfully");

        // 4. Reconnect to get card info
        return await ConnectAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        return Result.Failure("Reset failed", ex);
    }
}
```

**Khi nào gọi:**
- ✅ Khi card bị lỗi và cần reset
- ✅ Khi user nhấn nút Reset trong UI
- ✅ Khi cần recover từ error state

**Flow:**
1. `dmc_board_reset()` - Reset hardware
2. Wait 500ms - Đợi board reset xong
3. `dmc_board_init()` - Reinitialize board
4. `ConnectAsync()` - Get card info lại

---

## 🔄 Lifecycle Flow

### Program Start
```
1. App starts
2. DI Container creates LeadshineMaster
   └─> Constructor calls dmc_board_init() ← ONCE!
3. LeadshineMaster instance injected to Machine
4. Machine.OnInitializingAsync()
   └─> LeadshineMaster.ConnectAsync() ← Get info only
   └─> Axis.ConnectAsync() ← Config axis
5. Ready to run!
```

### Normal Operation
```
[Running]
  ├─> Axis moving
  ├─> IO reading/writing
  └─> NO board init!
```

### Error Recovery
```
[Error detected]
  ↓
User clicks Reset
  ↓
Machine.ResetAsync()
  ↓
LeadshineMaster.ResetAsync()
  ├─> dmc_board_reset()
  ├─> Wait 500ms
  ├─> dmc_board_init() ← REINIT!
  └─> ConnectAsync()
  ↓
[Ready again]
```

---

## 📊 Comparison

| Scenario | Old (Wrong) | New (Correct) |
|----------|-------------|---------------|
| Program Start | Init in ConnectAsync | ✅ Init in Constructor |
| Axis Connect | Call board init again ❌ | ✅ No board init |
| Multiple axes | Init N times ❌ | ✅ Init once |
| Reset needed | Manual reinit | ✅ Auto reinit in ResetAsync |

---

## ⚠️ Important Notes

### DO ✅
- Call `dmc_board_init()` in LeadshineMaster constructor
- Call `ConnectAsync()` to get card info
- Call `ResetAsync()` when card error occurs
- Check `result <= 0` for failure

### DON'T ❌
- Don't call `dmc_board_init()` in ConnectAsync
- Don't call `dmc_board_init()` in Axis.ConnectAsync
- Don't call `dmc_board_init()` multiple times
- Don't forget to reinit after reset

---

## 🐛 Troubleshooting

### Problem: "Board init called multiple times"
**Symptom:** Log shows multiple "Initializing Leadshine Master" messages

**Cause:** Old code was calling init in ConnectAsync

**Solution:** ✅ Fixed - Init only in constructor now

---

### Problem: "Card not working after reset"
**Symptom:** After reset, axis doesn't respond

**Cause:** Forgot to reinitialize board after reset

**Solution:** ✅ Fixed - ResetAsync now auto-reinits board

---

### Problem: "Constructor throws exception"
**Symptom:** Program crashes on start with InvalidOperationException

**Cause:** Board init failed (card not connected, driver issue)

**Solution:**
1. Check EtherCAT cable connection
2. Check LTDMC.dll installed correctly
3. Check Device Manager for card
4. Try different USB port
5. Check card power

---

## 🎯 Summary

**Key Points:**
1. ✅ `dmc_board_init()` → Constructor ONLY (once per program)
2. ✅ `ConnectAsync()` → Get card info ONLY (no init)
3. ✅ `ResetAsync()` → Reset + Reinit when error
4. ✅ Check `result <= 0` for failure

**Benefits:**
- ✅ Board init chỉ 1 lần như yêu cầu
- ✅ Auto reinit khi reset
- ✅ Clean lifecycle management
- ✅ Easy to debug
- ✅ Follows best practices

**Pattern này đảm bảo:**
> "dmc_board_init() chỉ dùng 1 lần khi chạy chương trình và khi card bị lỗi sau khi reset lại sẽ init card lại" ✅
