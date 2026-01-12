# 📖 HƯỚNG DẪN VIẾT LOGIC CHO MACHINE.PICKANDPLACE

## 🎯 TÓM TẮT VỊ TRÍ CÁC FILE

```
projects/Machine.PickAndPlace/
├── Machine.PickAndPlace.Core/           ← LOGIC CORE (Viết logic ở đây)
│   ├── PickAndPlaceMachine.cs          ← AUTO sequence
│   ├── Manual/
│   │   └── ManualController.cs         ← MANUAL control
│   └── Stations/
│       ├── PickStation.cs              ← Pick logic
│       └── PlaceStation.cs             ← Place logic
│
└── Machine.PickAndPlace/                ← UI LAYER (Bind & display)
    ├── Views/
    │   ├── AutoView.xaml               ← AUTO UI (✅ Done)
    │   ├── ManualView.xaml             ← MANUAL UI (✅ Done)
    │   └── SettingView.xaml            ← SETTINGS UI
    └── ViewModels/
        └── MainViewModel.cs            ← ViewModel (cần thêm Manual commands)
```

---

## 1️⃣ AUTO MODE - VIẾT Ở ĐÂU?

### 📍 File: `Machine.PickAndPlace.Core/PickAndPlaceMachine.cs`

**Method chính:** `OnRunningAsync()` - line 132

```csharp
protected override async Task OnRunningAsync()
{
    await base.OnRunningAsync();

    // ===============================
    // VIẾT TRÌNH TỰ TỰ ĐỘNG Ở ĐÂY
    // ===============================

    try
    {
        // Bước 1: Check điều kiện khởi động
        if (!CheckStartConditions())
        {
            RaiseAlarm(2000, "Start conditions not met", AlarmSeverity.Warning);
            return;
        }

        // Bước 2: Pick part
        await PickSequenceAsync();

        // Bước 3: Vision inspection (optional)
        // await VisionInspectionAsync();

        // Bước 4: Place part
        await PlaceSequenceAsync();

        // Bước 5: Return home
        await ReturnHomeAsync();

        Log.Logger.Information("Cycle {CycleCount} completed", Context.CycleCount + 1);
    }
    catch (Exception ex)
    {
        // Raise critical alarm - sẽ auto stop machine
        RaiseAlarm(2001, $"Cycle failed: {ex.Message}", AlarmSeverity.Critical);
        throw;
    }
}
```

### ✏️ Cách thêm bước mới:

**Ví dụ: Thêm Vision Inspection**

```csharp
// 1. Thêm method mới vào PickAndPlaceMachine.cs
private async Task VisionInspectionAsync()
{
    Log.Logger.Information("Vision inspection started");

    try
    {
        // TODO: Trigger camera
        // TODO: Wait for result
        await Task.Delay(500); // Simulate vision time

        // TODO: Check result
        bool passed = true; // Replace with actual result

        if (!passed)
        {
            RaiseAlarm(2300, "Vision inspection failed", AlarmSeverity.Critical);
            throw new Exception("Part rejected");
        }

        Log.Logger.Information("Vision inspection passed");
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Vision inspection failed");
        throw;
    }
}

// 2. Gọi trong OnRunningAsync()
protected override async Task OnRunningAsync()
{
    // ...
    await PickSequenceAsync();
    await VisionInspectionAsync();  // ← NEW
    await PlaceSequenceAsync();
    // ...
}
```

### 📋 Alarm Codes đã định nghĩa:

| Code | Mô tả | Severity |
|------|-------|----------|
| **1001-1005** | Hardware initialization errors | Critical |
| **1101-1103** | Homing errors | Critical |
| **2001** | Cycle failed (general) | Critical |
| **2101-2105** | Pick sequence errors | Critical |
| **2201-2207** | Place sequence errors | Critical/Warning |
| **2300-2399** | Reserved for Vision/Inspection | Critical/Warning |

### 🔧 Sửa Pick/Place sequence:

**File:** `PickAndPlaceMachine.cs` - lines 154-227 (Pick), 229-319 (Place)

```csharp
// Ví dụ: Thay đổi vị trí Pick
private async Task PickSequenceAsync()
{
    Log.Logger.Information("Pick sequence started");

    try
    {
        // THAY ĐỔI VỊ TRÍ Ở ĐÂY
        if (_axisX != null)
        {
            // Thay 100 bằng vị trí mới
            var result = await _axisX.MoveAbsoluteAsync(150);  // ← NEW POSITION
            if (!result.IsSuccess)
            {
                RaiseAlarm(2101, $"Axis X move failed: {result.Message}", AlarmSeverity.Critical);
                throw new Exception(result.Message);
            }
        }

        // ... tương tự cho Y, Z
    }
    catch (Exception ex)
    {
        Log.Logger.Error(ex, "Pick sequence failed");
        throw;
    }
}
```

---

## 2️⃣ MANUAL MODE - VIẾT Ở ĐÂU?

### 📍 File: `Machine.PickAndPlace.Core/Manual/ManualController.cs` (✅ Created)

**Chức năng đã có:**

```csharp
// 1. Move axis to position
await machine.Manual.MoveToPositionAsync("X", 100.0);

// 2. JOG axis (continuous)
machine.Manual.JogAxis("X", positiveDirection: true, JogSpeed.Medium);
await machine.Manual.StopAxisAsync("X"); // Stop JOG

// 3. Home axis
await machine.Manual.HomeAxisAsync("X");

// 4. Preset positions
await machine.Manual.MoveToPickPositionAsync();
await machine.Manual.MoveToPlacePositionAsync();
await machine.Manual.MoveToHomePositionAsync();

// 5. IO control
await machine.Manual.SetVacuumAsync(true);  // ON
await machine.Manual.SetVacuumAsync(false); // OFF

// 6. Get status
double pos = machine.Manual.GetPosition("X");
bool homed = machine.Manual.IsAxisHomed("X");
bool moving = machine.Manual.IsAxisMoving("X");
```

### ✏️ Cách thêm chức năng Manual mới:

**Ví dụ: Thêm Camera Trigger**

```csharp
// File: ManualController.cs

public async Task<Result> TriggerCameraAsync(CancellationToken ct = default)
{
    _logger.Information("Manual: Triggering camera");

    try
    {
        // TODO: Trigger camera IO
        // await _cameraOutput.WriteAsync(true, ct);
        // await Task.Delay(100, ct);
        // await _cameraOutput.WriteAsync(false, ct);

        return Result.Success("Camera triggered");
    }
    catch (Exception ex)
    {
        _logger.Error(ex, "Manual: Camera trigger failed");
        return Result.Failure("Camera trigger failed", ex);
    }
}
```

---

## 3️⃣ UI - BIND Ở ĐÂU?

### 📍 AUTO UI: `Views/AutoView.xaml` (✅ Done)

Buttons đã bind sẵn trong MainViewModel:
- ▶ START → `StartCommand`
- ⏹ STOP → `StopCommand`
- 🔄 RESET → `ResetCommand`
- 🏠 HOME ALL → `HomeAllCommand`

### 📍 MANUAL UI: `Views/ManualView.xaml` (✅ Created)

**CẦN TẠO ViewModel cho Manual**

File: `Machine.PickAndPlace/ViewModels/ManualViewModel.cs` (chưa có)

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Machine.PickAndPlace.Core;
using System.Windows.Threading;

namespace Machine.PickAndPlace.ViewModels;

public partial class ManualViewModel : ObservableObject
{
    private readonly PickAndPlaceMachine _machine;
    private readonly DispatcherTimer _updateTimer;

    // Axis positions
    [ObservableProperty]
    private double _axisXPosition;

    [ObservableProperty]
    private double _axisYPosition;

    [ObservableProperty]
    private double _axisZPosition;

    // Target positions for absolute move
    [ObservableProperty]
    private double _axisXTargetPosition = 0;

    // Status
    [ObservableProperty]
    private string _axisXStatus = "Not Homed";

    [ObservableProperty]
    private bool _vacuumOn;

    [ObservableProperty]
    private int _jogSpeedIndex = 1; // Medium speed

    public ManualViewModel(PickAndPlaceMachine machine)
    {
        _machine = machine;

        // Update positions periodically
        _updateTimer = new DispatcherTimer();
        _updateTimer.Interval = TimeSpan.FromMilliseconds(100);
        _updateTimer.Tick += UpdatePositions;
        _updateTimer.Start();
    }

    private void UpdatePositions(object? sender, EventArgs e)
    {
        AxisXPosition = _machine.Manual.GetPosition("X");
        AxisYPosition = _machine.Manual.GetPosition("Y");
        AxisZPosition = _machine.Manual.GetPosition("Z");

        AxisXStatus = _machine.Manual.IsAxisHomed("X") ? "Homed" : "Not Homed";
    }

    // ========================
    // AXIS X COMMANDS
    // ========================

    [RelayCommand]
    private void JogXPositive()
    {
        var speed = (Manual.ManualController.JogSpeed)_jogSpeedIndex;
        _machine.Manual.JogAxis("X", positiveDirection: true, speed);
    }

    [RelayCommand]
    private void JogXNegative()
    {
        var speed = (Manual.ManualController.JogSpeed)_jogSpeedIndex;
        _machine.Manual.JogAxis("X", positiveDirection: false, speed);
    }

    [RelayCommand]
    private async Task MoveAxisX()
    {
        await _machine.Manual.MoveToPositionAsync("X", AxisXTargetPosition);
    }

    [RelayCommand]
    private async Task HomeAxisX()
    {
        await _machine.Manual.HomeAxisAsync("X");
    }

    // ========================
    // AXIS Y COMMANDS
    // ========================

    [RelayCommand]
    private void JogYPositive()
    {
        var speed = (Manual.ManualController.JogSpeed)_jogSpeedIndex;
        _machine.Manual.JogAxis("Y", positiveDirection: true, speed);
    }

    [RelayCommand]
    private void JogYNegative()
    {
        var speed = (Manual.ManualController.JogSpeed)_jogSpeedIndex;
        _machine.Manual.JogAxis("Y", positiveDirection: false, speed);
    }

    [RelayCommand]
    private async Task HomeAxisY()
    {
        await _machine.Manual.HomeAxisAsync("Y");
    }

    // ========================
    // AXIS Z COMMANDS
    // ========================

    [RelayCommand]
    private void JogZUp()
    {
        var speed = (Manual.ManualController.JogSpeed)_jogSpeedIndex;
        _machine.Manual.JogAxis("Z", positiveDirection: true, speed);
    }

    [RelayCommand]
    private void JogZDown()
    {
        var speed = (Manual.ManualController.JogSpeed)_jogSpeedIndex;
        _machine.Manual.JogAxis("Z", positiveDirection: false, speed);
    }

    [RelayCommand]
    private async Task HomeAxisZ()
    {
        await _machine.Manual.HomeAxisAsync("Z");
    }

    // ========================
    // PRESET POSITIONS
    // ========================

    [RelayCommand]
    private async Task MoveToPick()
    {
        await _machine.Manual.MoveToPickPositionAsync();
    }

    [RelayCommand]
    private async Task MoveToPlace()
    {
        await _machine.Manual.MoveToPlacePositionAsync();
    }

    [RelayCommand]
    private async Task MoveToHome()
    {
        await _machine.Manual.MoveToHomePositionAsync();
    }

    // ========================
    // IO CONTROL
    // ========================

    [RelayCommand]
    private async Task ToggleVacuum()
    {
        await _machine.Manual.SetVacuumAsync(VacuumOn);
    }

    // ========================
    // STOP
    // ========================

    [RelayCommand]
    private async Task StopAllAxes()
    {
        await _machine.Manual.StopAxisAsync("X", emergency: true);
        await _machine.Manual.StopAxisAsync("Y", emergency: true);
        await _machine.Manual.StopAxisAsync("Z", emergency: true);
    }
}
```

**Cách register ViewModel:**

File: `App.xaml.cs` - thêm vào `ConfigureServices()`

```csharp
// Register ViewModels
services.AddSingleton<MainViewModel>();
services.AddSingleton<ManualViewModel>();  // ← ADD THIS

// Register Views
services.AddSingleton<MainWindow>();
```

**Set DataContext cho ManualView:**

File: `ManualView.xaml.cs`

```csharp
public partial class ManualView : UserControl
{
    public ManualView()
    {
        InitializeComponent();
    }

    // Set DataContext from DI container
    public void SetViewModel(ManualViewModel viewModel)
    {
        DataContext = viewModel;
    }
}
```

---

## 4️⃣ SETTINGS - VIẾTở ĐÂU?

### 📍 File: `Views/SettingView.xaml`

Thêm các settings như:
- Vị trí Pick/Place (X, Y, Z)
- Speeds (JOG, motion)
- IO assignments
- Vision parameters

**Ví dụ Settings UI:**

```xaml
<!-- SettingView.xaml -->
<StackPanel>
    <TextBlock Text="Pick Position" FontSize="18" FontWeight="SemiBold"/>

    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>
            <ColumnDefinition Width="*"/>
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>

        <TextBlock Grid.Row="0" Grid.Column="0" Text="X:" Margin="0,0,10,0"/>
        <TextBox Grid.Row="0" Grid.Column="1" Text="{Binding PickPositionX}"/>

        <TextBlock Grid.Row="1" Grid.Column="0" Text="Y:" Margin="0,0,10,0"/>
        <TextBox Grid.Row="1" Grid.Column="1" Text="{Binding PickPositionY}"/>

        <TextBlock Grid.Row="2" Grid.Column="0" Text="Z:" Margin="0,0,10,0"/>
        <TextBox Grid.Row="2" Grid.Column="1" Text="{Binding PickPositionZ}"/>
    </Grid>

    <Button Content="Save Settings" Command="{Binding SaveSettingsCommand}"/>
</StackPanel>
```

---

## 🎯 TÓM TẮT - VIỀt LOGIC Ở ĐÂU?

### ✅ AUTO MODE:
```
File: Machine.PickAndPlace.Core/PickAndPlaceMachine.cs
Method: OnRunningAsync() - line 132
```

### ✅ MANUAL MODE:
```
File: Machine.PickAndPlace.Core/Manual/ManualController.cs (✅ Created)
Public methods: JogAxis(), MoveToPosition(), SetVacuum(), etc.
```

### ⚠️ MANUAL UI (CẦN HOÀN THÀNH):
```
File: Machine.PickAndPlace/ViewModels/ManualViewModel.cs (CẦN TẠO)
Bind commands to ManualView.xaml
```

### ⚠️ SETTINGS (TÙY CHỌN):
```
File: Machine.PickAndPlace/Views/SettingView.xaml
Create SettingsViewModel to store/load configurations
```

---

## 📌 CHECKLIST HOÀN THÀNH

- [x] ✅ AUTO sequence logic (PickAndPlaceMachine.cs)
- [x] ✅ MANUAL controller logic (ManualController.cs)
- [x] ✅ AUTO UI (AutoView.xaml)
- [x] ✅ MANUAL UI (ManualView.xaml)
- [ ] ⚠️ MANUAL ViewModel (ManualViewModel.cs) - CẦN TẠO
- [ ] ⚠️ SETTINGS UI - TÙY CHỌN
- [ ] ⚠️ Register ManualViewModel in DI

---

## 🚀 NEXT STEPS

1. **Tạo ManualViewModel.cs** (copy code từ trên)
2. **Register trong App.xaml.cs** (DI container)
3. **Set DataContext** cho ManualView
4. **Test Manual mode** - JOG, Move, Home, IO
5. **Thêm Settings** nếu cần

Sau đó toàn bộ logic AUTO + MANUAL đã hoàn chỉnh! 🎉
