# Deferred Hardware Initialization

## Problem
User yêu cầu: "tôi muốn phải khởi động xong giao diện chương trình thì mới bắt đầu khởi tạo card rồi mới đến trục và các remote IO nếu có."

## Previous Behavior (WRONG)

```
Timeline trước đây:
┌─────────────────────────────────────────────────────┐
│ [0ms]   App.OnStartup()                             │
│         └─ Host.Build()                             │
│            └─ DI Container creates LeadshineMaster  │
│               └─ Constructor calls dmc_board_init() │ ❌ TOO EARLY!
│                  └─ Hardware initialized            │
│                     (Card, axes connected)          │
│                                                      │
│ [500ms] MainWindow constructor                      │
│         └─ Create views                             │
│                                                      │
│ [800ms] MainWindow.Loaded event                     │
│         └─ Auto-call InitializeCommand              │ ❌ AUTO-INIT!
│            └─ PickAndPlaceMachine.OnInitializingAsync()│
│               └─ _axis.ConnectAsync()               │
│                  (Already connected!)               │
│                                                      │
│ [1000ms] UI appears                                 │
│          (Hardware already initialized before user sees UI)│
└─────────────────────────────────────────────────────┘
```

**Problems:**
1. ❌ Hardware khởi tạo TRƯỚC KHI UI xuất hiện
2. ❌ Card được init trong constructor (không thể handle errors gracefully)
3. ❌ User không có control - hardware tự động kết nối
4. ❌ Không thấy logs của quá trình khởi tạo vì LogPanel chưa sẵn sàng

## New Behavior (CORRECT)

```
Timeline mới:
┌─────────────────────────────────────────────────────┐
│ [0ms]   App.OnStartup()                             │
│         └─ Host.Build()                             │
│            └─ DI Container creates LeadshineMaster  │
│               └─ Constructor does NOT init card     │ ✅ Just creates instance
│                  └─ Log: "Instance created"         │
│                                                      │
│ [500ms] MainWindow constructor                      │
│         └─ Create views (including LogView)         │
│            └─ LogPanel subscribes to logs           │ ✅ Ready to receive logs
│                                                      │
│ [800ms] MainWindow.Loaded event                     │
│         └─ No auto-initialization                   │ ✅ Wait for user
│                                                      │
│ [1000ms] UI appears                                 │
│          └─ User sees interface                     │ ✅ UI ready
│             └─ LogPanel ready                       │
│             └─ Hardware NOT initialized yet         │
│                                                      │
│ [USER CLICKS "Initialize" BUTTON]                   │ 👆 User control
│         └─ MainViewModel.InitializeCommand          │
│            └─ Machine.InitializeAsync()             │
│               └─ Machine.OnInitializingAsync()      │
│                  └─ _axis.ConnectAsync()            │
│                     └─ LeadshineMaster.ConnectAsync()│
│                        └─ dmc_board_init()          │ ✅ NOW initialize!
│                           └─ Logs appear in UI      │
│                        └─ Get card info             │
│                        └─ _axis.ConnectAsync()      │
│                           └─ Enable axis            │
│                           └─ Configure gear ratio   │
│                           └─ Set profile            │
│                           └─ All logs visible in UI │ ✅
└─────────────────────────────────────────────────────┘
```

**Benefits:**
1. ✅ UI khởi động hoàn toàn TRƯỚC KHI touch hardware
2. ✅ User có full control - click Initialize khi sẵn sàng
3. ✅ Tất cả logs hiển thị trong LogPanel
4. ✅ Errors có thể handle gracefully với UI feedback
5. ✅ Phù hợp với flow: UI → Card → Axes → Remote IO

## Changes Made

### 1. Removed Auto-Initialize from MainWindow

**File**: [MainWindow.xaml.cs](d:\Projects\C#\NAutoSuite\projects\Machine.PickAndPlace.Frontend\MainWindow.xaml.cs)

**Before** (lines 34-38):
```csharp
// Auto-initialize machine
Loaded += async (s, e) =>
{
    await _viewModel.InitializeCommand.ExecuteAsync(null);
};
```

**After** (lines 34-35):
```csharp
// Hardware initialization is done manually via Initialize button
// Do NOT auto-initialize on window load
```

### 2. Deferred Board Initialization in LeadshineMaster

**File**: [LeadshineMaster.cs](d:\Projects\C#\NAutoSuite\src\Hardware\NAutoSuite.Hardware.Leadshine\LeadshineMaster.cs)

**Before** (Constructor - lines 49-82):
```csharp
public LeadshineMaster(ushort cardNo, string? ipAddress = null, ILogger? logger = null)
{
    // ... setup ...

    // ❌ Initialize board in constructor
    short result;
    if (!string.IsNullOrEmpty(_ipAddress))
        result = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
    else
        result = LTDMC.dmc_board_init();

    if (result <= 0)
        throw new InvalidOperationException($"Failed to initialize card");
}
```

**After** (Constructor - lines 51-62):
```csharp
private bool _boardInitialized = false;

public LeadshineMaster(ushort cardNo, string? ipAddress = null, ILogger? logger = null)
{
    // ... setup ...

    // ✅ Do NOT initialize board in constructor
    // Board initialization will be done in ConnectAsync() when user is ready
    _logger.Information("LeadshineMaster instance created. Board will be initialized when ConnectAsync is called.");
}
```

**ConnectAsync()** now initializes the board (lines 64-138):
```csharp
public async Task<Result> ConnectAsync(CancellationToken cancellationToken = default)
{
    // Step 1: Initialize board (only once)
    if (!_boardInitialized)
    {
        _logger.Information("Initializing Leadshine board...");

        short initResult;
        if (!string.IsNullOrEmpty(_ipAddress))
            initResult = LTDMC.dmc_board_init_eth(_cardNo, _ipAddress);
        else
            initResult = LTDMC.dmc_board_init();

        if (initResult <= 0)
        {
            _logger.Error("Failed to initialize card");
            return Result.Failure("Board initialization failed");
        }

        _boardInitialized = true;
        _logger.Information("Board initialized successfully");
    }

    // Step 2: Get card information
    // ... get axes, IO count, version ...

    _isConnected = true;
    return Result.Success("Connected");
}
```

## Initialization Flow

### User Perspective

1. **App starts** → UI loads → User sees main window
2. **User clicks "Initialize"** → Hardware begins initialization
3. **Logs appear in Log tab** → User can monitor progress
4. **Initialization complete** → Machine ready to run

### Technical Flow

```
User clicks "Initialize" button
  ↓
MainViewModel.InitializeCommand.ExecuteAsync()
  ↓
PickAndPlaceMachineFrontend.InitializeAsync()
  ↓
[MachineBase state transition: Uninitialized → Initializing]
  ↓
PickAndPlaceMachineFrontend.OnInitializingAsync()
  ↓
_axis.ConnectAsync() (from DI-injected IAxis)
  ↓
LeadshineAxis.ConnectAsync()
  ↓
_master.ConnectAsync() (LeadshineMaster)
  ↓
┌──────────────────────────────────────────┐
│ LeadshineMaster.ConnectAsync()           │
│                                          │
│ 1. if (!_boardInitialized)               │
│    └─ LTDMC.dmc_board_init()             │ ← Hardware init happens HERE
│       └─ Log: "Initializing board..."    │
│       └─ Log: "Board initialized"        │
│    └─ _boardInitialized = true           │
│                                          │
│ 2. Get card info                         │
│    └─ dmc_get_total_axes()               │
│    └─ dmc_get_total_ionum()              │
│    └─ dmc_get_card_version()             │
│    └─ Log: "Connected. Axes: X, IO: Y/Z" │
│                                          │
│ 3. _isConnected = true                   │
│    └─ return Result.Success()            │
└──────────────────────────────────────────┘
  ↓
LeadshineAxis continues configuration:
  ↓
  ├─ nmc_set_axis_enable()
  │  └─ Log: "Axis enabled"
  ├─ dmc_set_equiv() (gear ratio)
  │  └─ Log: "Gear ratio set"
  └─ dmc_set_profile_unit()
     └─ Log: "Profile set"
  ↓
[MachineBase state transition: Initializing → Idle]
  ↓
User sees "Initialized successfully" + all logs in UI
```

## Expected Logs in UI

When user clicks Initialize, they should see these logs in LogPanel:

```
[15:30:12.123] [Information] LeadshineMaster instance created (Card 0). Board will be initialized when ConnectAsync is called.
[15:30:12.234] [Information] Initializing PickAndPlace Machine Frontend...
[15:30:12.345] [Information] Initializing Leadshine board (Card 0)...
[15:30:12.456] [Information] Initializing locally (dmc_board_init)
[15:30:12.567] [Information] Board initialized successfully (result=1)
[15:30:12.678] [Information] Leadshine Card Version: 0x12345678
[15:30:12.789] [Information] Leadshine Master 0 connected successfully. Axes: 4, Inputs: 32, Outputs: 32
[15:30:12.890] [Information] Axis connected successfully
[15:30:13.001] [Information] Axis PAP001_X enabled successfully
[15:30:13.112] [Information] Axis PAP001_X gear ratio set: equiv=233.016
[15:30:13.223] [Information] Axis PAP001_X default profile set: MinVel=100, MaxVel=36000, Acc/Dec=0.2
[15:30:13.334] [Information] Initialized successfully
```

## Testing Checklist

- [ ] Run app → UI appears WITHOUT hardware initialization
- [ ] Check Log tab → Should see "LeadshineMaster instance created" only
- [ ] Click "Initialize" button
- [ ] Watch Log tab → Should see full initialization sequence
- [ ] Verify card is initialized AFTER button click, not before
- [ ] Check machine state transitions: Uninitialized → Initializing → Idle
- [ ] Verify no errors during initialization
- [ ] Test with hardware disconnected → Should show error in UI gracefully

## Benefits Summary

### Before (Wrong)
❌ Hardware init in constructor (during DI container build)
❌ Auto-initialize on window load
❌ User has no control
❌ Logs missed by LogPanel
❌ Hard to debug initialization errors

### After (Correct)
✅ Hardware init deferred until ConnectAsync()
✅ User controls initialization via button
✅ All logs visible in LogPanel
✅ Graceful error handling with UI feedback
✅ Clear initialization flow: UI → Card → Axes → IO

## Future Enhancements

For remote IO initialization (when added):

```csharp
protected override async Task OnInitializingAsync()
{
    // 1. Initialize Card
    var result = await _master.ConnectAsync();
    if (!result.IsSuccess) throw ...

    // 2. Initialize Axes
    foreach (var axis in _axes)
    {
        result = await axis.ConnectAsync();
        if (!result.IsSuccess) throw ...
    }

    // 3. Initialize Remote IO (if any)
    foreach (var remoteIO in _remoteIOs)
    {
        result = await remoteIO.ConnectAsync();
        if (!result.IsSuccess) throw ...
    }
}
```

This maintains the order: **UI → Card → Axes → Remote IO**
