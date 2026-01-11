# KẾ HOẠCH REFACTORING - NAUTOSUITE ARCHITECTURE

## 📋 PHÂN TÍCH HIỆN TẠI

### ✅ Platform Core (NAutoSuite.Core) - Đã có

**MachineBase.cs** đã implement đầy đủ:
- ✅ State Machine với Stateless library
- ✅ States: Uninitialized → Initializing → Idle → Running → Paused → Stopping → Stopped → Error → EmergencyStop
- ✅ Triggers: Initialize, Start, Stop, Pause, Resume, Reset, EmergencyStop, ClearError, Complete
- ✅ Public Methods:
  - `InitializeAsync()` - Khởi tạo máy
  - `StartAsync()` - Bắt đầu auto
  - `StopAsync()` - Dừng máy
  - `PauseAsync()` - Tạm dừng
  - `ResumeAsync()` - Tiếp tục
  - `ResetAsync()` - Reset máy về Idle/Uninitialized
  - `EmergencyStopAsync()` - Dừng khẩn cấp
- ✅ Virtual Hooks (Machine-specific logic override):
  - `OnInitializingAsync()` - Logic khi khởi tạo
  - `OnRunningAsync()` - Logic khi chạy auto
  - `OnExitRunningAsync()` - Logic khi thoát running
  - `OnStoppingAsync()` - Logic khi dừng
  - `OnEmergencyStopAsync()` - Logic khi EMG
- ✅ Context tracking (CycleCount, RunTime, LastError)
- ✅ Event: StateChanged
- ✅ Logging tự động

**MachineContext.cs**:
- ✅ CycleCount, CycleStartTime, TotalRunTime, LastError
- ✅ Reset()

### ❌ THIẾU - Cần bổ sung

1. **Alarm Management trong MachineBase**
   - Hiện tại: AlarmManager tách biệt, không tích hợp
   - Cần: Tích hợp AlarmManager vào MachineBase
   - Cần: Auto transition sang Error state khi có alarm

2. **Cancel Auto Function**
   - Hiện tại: Không có
   - Cần: Thêm `CancelAutoAsync()` - hủy auto đang chạy, về Idle

3. **Interlock System**
   - Hiện tại: Không có
   - Cần: Hệ thống interlock ngăn Start khi điều kiện không đủ

---

## 🔧 KẾ HOẠCH THỰC HIỆN

### PHASE 1: Bổ sung Platform Core ✅

#### Task 1.1: Tích hợp AlarmManager vào MachineBase

**File: `src/Core/NAutoSuite.Core/Machine/MachineBase.cs`**

```csharp
public abstract class MachineBase : IMachine
{
    private readonly StateMachine<MachineState, MachineTrigger> _stateMachine;
    private readonly ILogger _logger;

    // ✅ NEW: Integrated AlarmManager
    protected readonly AlarmManager AlarmManager;

    public string Id { get; }
    public string Name { get; }
    public MachineState State => _stateMachine.State;
    public MachineContext Context { get; }

    // ✅ NEW: Alarm properties
    public bool HasActiveAlarms => AlarmManager.GetActiveAlarms().Any();
    public IEnumerable<Alarm> ActiveAlarms => AlarmManager.GetActiveAlarms();

    protected MachineBase(string id, string name, ILogger? logger = null)
    {
        // ... existing code ...

        // ✅ NEW: Initialize AlarmManager
        AlarmManager = new AlarmManager(logger);
        AlarmManager.AlarmRaised += OnAlarmRaised;
        AlarmManager.AlarmCleared += OnAlarmCleared;
    }

    // ✅ NEW: Alarm handlers
    private void OnAlarmRaised(object? sender, Alarm alarm)
    {
        _logger.Warning("Alarm raised: {AlarmCode} - {AlarmMessage}", alarm.Code, alarm.Message);

        // Auto transition to Error if alarm is critical
        if (alarm.IsCritical && State == MachineState.Running)
        {
            _ = StopAsync(); // Fire and forget
        }
    }

    private void OnAlarmCleared(object? sender, Alarm alarm)
    {
        _logger.Information("Alarm cleared: {AlarmCode}", alarm.Code);
    }

    // ✅ NEW: Alarm management methods
    protected void RaiseAlarm(string code, string message, bool isCritical = false)
    {
        AlarmManager.RaiseAlarm(code, message, isCritical);
        Context.LastError = message;
    }

    protected void ClearAlarm(string code)
    {
        AlarmManager.ClearAlarm(code);
    }

    protected void ClearAllAlarms()
    {
        AlarmManager.ClearAll();
    }
}
```

#### Task 1.2: Thêm Cancel Auto Function

```csharp
// ✅ NEW: Cancel trigger
public enum MachineTrigger
{
    Initialize, Start, Stop, Pause, Resume, Reset,
    EmergencyStop, ClearError, Complete,
    CancelAuto  // ← NEW
}

// In ConfigureStateMachine():
_stateMachine.Configure(MachineState.Running)
    .OnEntryAsync(OnRunningAsync)
    .OnExitAsync(OnExitRunningAsync)
    .Permit(MachineTrigger.Stop, MachineState.Stopping)
    .Permit(MachineTrigger.Pause, MachineState.Paused)
    .Permit(MachineTrigger.EmergencyStop, MachineState.EmergencyStop)
    .Permit(MachineTrigger.Complete, MachineState.Idle)
    .Permit(MachineTrigger.CancelAuto, MachineState.Idle);  // ← NEW

// ✅ NEW: CancelAutoAsync method
public virtual async Task<Result> CancelAutoAsync(CancellationToken cancellationToken = default)
{
    try
    {
        if (!_stateMachine.CanFire(MachineTrigger.CancelAuto))
            return Result.Failure($"Cannot cancel from state {State}");

        await _stateMachine.FireAsync(MachineTrigger.CancelAuto);
        _logger.Information("Auto sequence cancelled for machine {Name}", Name);
        return Result.Success("Auto sequence cancelled");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Failed to cancel auto for machine {Name}", Name);
        return Result.Failure("Cancel auto failed", ex);
    }
}
```

#### Task 1.3: Thêm Interlock System

**File: `src/Core/NAutoSuite.Core/Interlock/InterlockCondition.cs`** (NEW)

```csharp
namespace NAutoSuite.Core.Interlock;

public class InterlockCondition
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Func<bool> Condition { get; set; } = () => true;
    public bool IsRequired { get; set; } = true;
}

public class InterlockManager
{
    private readonly List<InterlockCondition> _conditions = new();
    private readonly ILogger _logger;

    public InterlockManager(ILogger? logger = null)
    {
        _logger = logger ?? Log.Logger;
    }

    public void AddCondition(InterlockCondition condition)
    {
        _conditions.Add(condition);
    }

    public void RemoveCondition(string id)
    {
        _conditions.RemoveAll(c => c.Id == id);
    }

    public (bool CanStart, List<string> FailedConditions) CheckStartConditions()
    {
        var failed = new List<string>();

        foreach (var condition in _conditions.Where(c => c.IsRequired))
        {
            if (!condition.Condition())
            {
                failed.Add(condition.Description);
                _logger.Warning("Interlock failed: {Description}", condition.Description);
            }
        }

        return (failed.Count == 0, failed);
    }
}
```

**Tích hợp vào MachineBase:**

```csharp
public abstract class MachineBase : IMachine
{
    // ✅ NEW: Interlock Manager
    protected readonly InterlockManager InterlockManager;

    protected MachineBase(string id, string name, ILogger? logger = null)
    {
        // ... existing code ...
        InterlockManager = new InterlockManager(logger);

        // Setup default interlocks
        SetupDefaultInterlocks();
    }

    // ✅ NEW: Setup default interlocks
    protected virtual void SetupDefaultInterlocks()
    {
        // Không cho Start nếu có alarm critical
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "NO_CRITICAL_ALARMS",
            Description = "No critical alarms present",
            Condition = () => !HasActiveAlarms || !ActiveAlarms.Any(a => a.IsCritical)
        });
    }

    // ✅ Modified: Check interlock before start
    public override async Task<Result> StartAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_stateMachine.CanFire(MachineTrigger.Start))
                return Result.Failure($"Cannot start from state {State}");

            // ✅ NEW: Check interlocks
            var (canStart, failedConditions) = InterlockManager.CheckStartConditions();
            if (!canStart)
            {
                var message = $"Cannot start: {string.Join(", ", failedConditions)}";
                _logger.Warning(message);
                return Result.Failure(message);
            }

            await _stateMachine.FireAsync(MachineTrigger.Start);
            return Result.Success("Machine started");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to start machine {Name}", Name);
            return Result.Failure("Start failed", ex);
        }
    }
}
```

---

### PHASE 2: Tạo Leadshine EtherCAT Implementation 🔧

#### Task 2.1: Tạo project Hardware.Leadshine

**Cấu trúc:**
```
src/Hardware/NAutoSuite.Hardware.Leadshine/
├── LTDMC.cs                    # P/Invoke wrapper (code bạn cung cấp)
├── LeadshineMaster.cs          # EtherCAT Master controller
├── LeadshineAxis.cs            # Axis implementation (thay thế template)
├── LeadshineIOModule.cs        # Remote IO module
└── NAutoSuite.Hardware.Leadshine.csproj
```

#### Task 2.2: Implement LeadshineMaster

**File: `src/Hardware/NAutoSuite.Hardware.Leadshine/LeadshineMaster.cs`**

```csharp
public class LeadshineMaster : IEtherCATMaster
{
    private readonly ushort _cardNo;
    private readonly string? _ipAddress;
    private readonly ILogger _logger;
    private bool _isConnected;

    public string Id { get; }
    public string Name { get; }
    public bool IsConnected => _isConnected;

    public LeadshineMaster(ushort cardNo, string? ipAddress = null, ILogger? logger = null)
    {
        _cardNo = cardNo;
        _ipAddress = ipAddress;
        _logger = logger ?? Log.Logger;
        Id = $"Leadshine_{cardNo}";
        Name = $"Leadshine EtherCAT Master #{cardNo}";
    }

    public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            short result;

            if (!string.IsNullOrEmpty(_ipAddress))
            {
                // Connect via Ethernet
                result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
            }
            else
            {
                // Connect via local
                result = LTDMC.dmc_board_init();
            }

            if (result != 0)
            {
                var errorMsg = $"Failed to initialize Leadshine card {_cardNo}, error code: {result}";
                _logger.Error(errorMsg);
                return Result.Failure(errorMsg);
            }

            _isConnected = true;
            _logger.Information("Leadshine Master {CardNo} connected successfully", _cardNo);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to connect Leadshine Master {CardNo}", _cardNo);
            return Result.Failure("Connection failed", ex);
        }
    }

    public async Task<Result> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = LTDMC.dmc_board_close();
            _isConnected = false;
            _logger.Information("Leadshine Master {CardNo} disconnected", _cardNo);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to disconnect Leadshine Master {CardNo}", _cardNo);
            return Result.Failure("Disconnection failed", ex);
        }
    }

    public int GetSlaveCount()
    {
        // TODO: Implement slave count detection
        return 0;
    }
}
```

---

### PHASE 3: Refactor Machine.PickAndPlace 📁

#### Task 3.1: Tạo cấu trúc mới

```
projects/Machine.PickAndPlace/
├── Machine.PickAndPlace.Core/              ← NEW - Logic máy
│   ├── PickAndPlaceMachine.cs              (Di chuyển từ project cũ)
│   ├── Stations/                           ← NEW
│   │   ├── PickStation.cs
│   │   └── PlaceStation.cs
│   ├── Sequences/                          ← NEW
│   │   └── AutoSequence.cs
│   └── Machine.PickAndPlace.Core.csproj
│
└── Machine.PickAndPlace.UI/                ← Rename từ Machine.PickAndPlace
    ├── ViewModels/
    ├── Views/
    ├── App.xaml
    ├── MainWindow.xaml
    └── Machine.PickAndPlace.UI.csproj
```

#### Task 3.2: Tạo Station Pattern

**File: `Machine.PickAndPlace.Core/Stations/PickStation.cs`**

```csharp
public class PickStation
{
    private readonly IAxis _axisX;
    private readonly IAxis _axisY;
    private readonly IAxis _axisZ;
    private readonly IOutput _vacuum;
    private readonly ILogger _logger;

    public PickStation(IAxis axisX, IAxis axisY, IAxis axisZ, IOutput vacuum, ILogger logger)
    {
        _axisX = axisX;
        _axisY = axisY;
        _axisZ = axisZ;
        _vacuum = vacuum;
        _logger = logger;
    }

    public async Task<Result> ExecuteAsync()
    {
        _logger.Information("Pick station started");

        // Move to pick position
        await _axisX.MoveAbsoluteAsync(100);
        await _axisY.MoveAbsoluteAsync(50);
        await Task.Delay(500);

        // Lower Z
        await _axisZ.MoveAbsoluteAsync(10);
        await Task.Delay(300);

        // Activate vacuum
        await _vacuum.WriteAsync(true);
        await Task.Delay(200);

        // Raise Z
        await _axisZ.MoveAbsoluteAsync(50);
        await Task.Delay(300);

        _logger.Information("Pick station completed");
        return Result.Success();
    }
}
```

#### Task 3.3: Refactor PickAndPlaceMachine

**File: `Machine.PickAndPlace.Core/PickAndPlaceMachine.cs`**

```csharp
public class PickAndPlaceMachine : MachineBase
{
    private readonly PickStation _pickStation;
    private readonly PlaceStation _placeStation;
    private readonly ILogger _machineLogger;

    public IAxis AxisX { get; }
    public IAxis AxisY { get; }
    public IAxis AxisZ { get; }

    public PickAndPlaceMachine(
        string id,
        string name,
        IAxis axisX,
        IAxis axisY,
        IAxis axisZ,
        IInput partSensor,
        IOutput vacuum,
        ILogger? logger = null)
        : base(id, name, logger)
    {
        _machineLogger = logger ?? Log.Logger;

        AxisX = axisX;
        AxisY = axisY;
        AxisZ = axisZ;

        // Create stations
        _pickStation = new PickStation(axisX, axisY, axisZ, vacuum, _machineLogger);
        _placeStation = new PlaceStation(axisX, axisY, axisZ, vacuum, _machineLogger);

        // Setup interlocks
        SetupMachineInterlocks();
    }

    private void SetupMachineInterlocks()
    {
        // Không cho Start nếu trục chưa home
        InterlockManager.AddCondition(new InterlockCondition
        {
            Id = "AXES_HOMED",
            Description = "All axes must be homed",
            Condition = () => AxisX.IsHomed && AxisY.IsHomed && AxisZ.IsHomed
        });
    }

    protected override async Task OnInitializingAsync()
    {
        await base.OnInitializingAsync();

        _machineLogger.Information("Initializing Pick and Place Machine...");

        // Connect hardware
        await AxisX.ConnectAsync();
        await AxisY.ConnectAsync();
        await AxisZ.ConnectAsync();

        _machineLogger.Information("Hardware connected");
    }

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        // ❌ KHÔNG xử lý Start/Stop/EMG ở đây - đã có trong MachineBase
        // ✅ CHỈ viết logic Auto Sequence

        _machineLogger.Information("Starting Pick and Place cycle {CycleCount}", Context.CycleCount + 1);

        try
        {
            // Execute auto sequence
            await _pickStation.ExecuteAsync();
            await _placeStation.ExecuteAsync();

            _machineLogger.Information("Cycle {CycleCount} completed successfully", Context.CycleCount + 1);
        }
        catch (Exception ex)
        {
            _machineLogger.Error(ex, "Cycle {CycleCount} failed", Context.CycleCount + 1);
            RaiseAlarm("AUTO_CYCLE_ERROR", ex.Message, isCritical: true);
        }
    }

    public async Task<Result> HomeAllAxesAsync()
    {
        _machineLogger.Information("Homing all axes...");

        var result = await AxisX.HomeAsync();
        if (!result.IsSuccess) return result;

        result = await AxisY.HomeAsync();
        if (!result.IsSuccess) return result;

        result = await AxisZ.HomeAsync();
        if (!result.IsSuccess) return result;

        return Result.Success("All axes homed");
    }
}
```

---

## 📊 KẾT QUẢ MONG ĐỢI

### Kiến trúc cuối cùng:

```
┌─────────────────────────────┐
│   Machine.PickAndPlace.UI    │  ← WPF UI
│   - MainWindow, ViewModels   │
└──────────────┬──────────────┘
               │ References
┌──────────────▼──────────────┐
│  Machine.PickAndPlace.Core   │  ← Logic máy RIÊNG
│  - PickAndPlaceMachine       │
│  - PickStation, PlaceStation │
│  - AutoSequence              │
│                              │
│  ✅ CHỈ viết logic Auto      │
│  ❌ KHÔNG viết Start/Stop    │
└──────────────┬──────────────┘
               │ Inherits
┌──────────────▼──────────────┐
│  NAutoSuite.Core.Base        │  ← Platform CHUNG
│  - MachineBase               │
│  - ✅ State Machine          │
│  - ✅ Start/Stop/Reset/EMG   │
│  - ✅ AlarmManager           │
│  - ✅ InterlockManager       │
│  - ✅ CancelAuto             │
└──────────────┬──────────────┘
               │ Uses
┌──────────────▼──────────────┐
│  Hardware.Abstractions       │
│  - IAxis, IPlc, IIO          │
└──────────────┬──────────────┘
               │ Implements
┌──────────────▼──────────────┐
│  Hardware.Implementation     │
│  - LeadshineMaster (LTDMC)   │
│  - LeadshineAxis             │
│  - Simulator                 │
└─────────────────────────────┘
```

### Lợi ích:

1. **Tái sử dụng**: Machine logic tách biệt, có thể dùng với nhiều UI
2. **Không lặp code**: Tất cả máy đều có Start/Stop/EMG từ Platform
3. **Chuẩn hóa**: Mọi project đều tuân theo cùng 1 pattern
4. **Dễ test**: Machine Core có thể test độc lập
5. **Dễ mở rộng**: Thêm máy mới chỉ cần viết logic Auto
