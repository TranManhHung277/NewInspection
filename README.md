# Huong Dan Su Dung NAutoSuite UI Controls

Tai lieu nay huong dan cach dung cac control UI trong du an WPF moi, tap trung vao: can them bien/command gi trong ViewModel, va cach wiring cho ro rang, de de huong dan nguoi khac.

## 1) Chuan bi truoc khi dung

Ban can:
- Them project reference toi `src/UI/NAutoSuite.UI.Controls/NAutoSuite.UI.Controls.csproj`.
- Cau hinh theme/resources neu du an cua ban chua merge cac resource chung.

Mot pattern de quan ly ro rang:
- Dung 1 `MainViewModel` de giu state cua header/footer + command.
- `MainWindow` chi lam nhiem vu switch view theo tab.

## 2) Mau MainViewModel toi thieu (khuyen nghi)

Copy mau nay roi them logic thuc te sau:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NAutoSuite.Core.Machine;
using NAutoSuite.UI.Controls;

public partial class MainViewModel : ObservableObject
{
    // Header bindings
    [ObservableProperty] private int _machineNumber = 1;
    [ObservableProperty] private string _projectName = "My Project";
    [ObservableProperty] private string _modelName = "Default";
    [ObservableProperty] private MachineState _machineState = MachineState.Uninitialized;
    [ObservableProperty] private MachineRunMode _runMode = MachineRunMode.Auto;
    [ObservableProperty] private HeaderStatusLevel _headerStatusLevel = HeaderStatusLevel.Ok;

    // Noi dung dang hien thi
    [ObservableProperty] private object? _currentView;

    // Command cho footer (giu dung ten nay de binding de dang)
    public IRelayCommand StartCommand { get; }
    public IRelayCommand StopCommand { get; }
    public IRelayCommand ResetCommand { get; }
    public IRelayCommand HomeHoldStartCommand { get; }
    public IRelayCommand HomeHoldEndCommand { get; }

    public MainViewModel()
    {
        StartCommand = new RelayCommand(OnStart);
        StopCommand = new RelayCommand(OnStop);
        ResetCommand = new RelayCommand(OnReset);
        HomeHoldStartCommand = new RelayCommand(OnHomeHoldStart);
        HomeHoldEndCommand = new RelayCommand(OnHomeHoldEnd);
    }

    public void NavigateToView(object view) => CurrentView = view;

    private void OnStart() { }
    private void OnStop() { }
    private void OnReset() { }
    private void OnHomeHoldStart() { }
    private void OnHomeHoldEnd() { }
}
```

## 3) ShellHeaderControl

### Muc dich
Thanh header tren cung: so may, trang thai, ten du an/model, run mode, muc canh bao.

### Cac bien can co trong ViewModel
Ban can (toi thieu):
- `int MachineNumber`
- `MachineState MachineState`
- `string ProjectName`
- `string ModelName`
- `MachineRunMode RunMode`
- `HeaderStatusLevel HeaderStatusLevel`

### Cach dung trong XAML

```xml
<Window
    xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <controls:ShellHeaderControl
        MachineNumberValue="{Binding MachineNumber}"
        MachineState="{Binding MachineState}"
        ProjectNameValue="{Binding ProjectName}"
        ModelNameValue="{Binding ModelName}"
        RunModeValue="{Binding RunMode}"
        StatusLevel="{Binding HeaderStatusLevel}"
        HeaderClicked="HeaderControl_HeaderClicked"
        MinimizeClicked="HeaderControl_MinimizeClicked"
        CloseClicked="HeaderControl_CloseClicked"/>
</Window>
```

### Su kien
- `HeaderClicked`: thuong dung de nhay ve tab mac dinh.
- `MinimizeClicked`: thu nho cua so.
- `CloseClicked`: dong chuong trinh (thuong co confirm).

## 4) ShellFooterControl (da chuan MVVM voi ICommand)

### Muc dich
Thanh dieu huong ben duoi + nut START/STOP/RESET/HOME.

### Quan trong: footer da dung DependencyProperty cho command
Ban phai bind command mot cach ro rang trong XAML (khong nen trong cho DataContext tu tim).

### Cac command can co trong ViewModel
- `ICommand StartCommand`
- `ICommand StopCommand`
- `ICommand ResetCommand`
- `ICommand HomeHoldStartCommand`
- `ICommand HomeHoldEndCommand`

### Cach dung trong XAML (khuyen nghi copy dung)

```xml
<controls:ShellFooterControl
    x:Name="FooterControl"
    StartCommand="{Binding StartCommand}"
    StopCommand="{Binding StopCommand}"
    ResetCommand="{Binding ResetCommand}"
    HomeHoldStartCommand="{Binding HomeHoldStartCommand}"
    HomeHoldEndCommand="{Binding HomeHoldEndCommand}"
    TabChanged="FooterControl_TabChanged"/>
```

### Xu ly chuyen tab

```csharp
private void FooterControl_TabChanged(object? sender, string tabName)
{
    // tabName: Auto, Manual, Data, Camera, Setting, Log
}
```

### Wiring view theo tab (pattern ro rang, de day nguoi khac)

Buoc 1: Tao cac view:
- `AutoView`, `ManualView`, `DataView`, `CameraView`, `SettingView`, `LogView`

Buoc 2: Tao 1 lan trong `MainWindow`, sau do switch theo tab:

```csharp
private readonly AutoView _autoView;
private readonly ManualView _manualView;
private readonly DataView _dataView;
private readonly CameraView _cameraView;
private readonly SettingView _settingView;
private readonly LogView _logView;

public MainWindow(MainViewModel viewModel)
{
    InitializeComponent();
    DataContext = viewModel;

    _autoView = new AutoView { DataContext = viewModel };
    _manualView = new ManualView { DataContext = viewModel };
    _dataView = new DataView { DataContext = viewModel };
    _cameraView = new CameraView { DataContext = viewModel };
    _settingView = new SettingView { DataContext = viewModel };
    _logView = new LogView();

    viewModel.NavigateToView(_autoView);
}

private void FooterControl_TabChanged(object? sender, string tabName)
{
    object view = tabName switch
    {
        "Auto" => _autoView,
        "Manual" => _manualView,
        "Data" => _dataView,
        "Camera" => _cameraView,
        "Setting" => _settingView,
        "Log" => _logView,
        _ => _autoView
    };

    ((MainViewModel)DataContext).NavigateToView(view);
}
```

## 5) InputField

### Muc dich
O nhap co validate + ban phim ao + format so.

### Bien can co trong ViewModel
Vi du:
- `string OperatorName`
- `double TargetSpeed`
- hoac `int`, `decimal`, ...

### Cach dung

```xml
<controls:InputField
    Text="{Binding OperatorName, UpdateSourceTrigger=PropertyChanged}"
    ValidationMode="Uppercase"/>

<controls:InputField
    Value="{Binding TargetSpeed}"
    ValueType="Double"
    DisplayDecimals="2"
    KeyboardType="Numeric"
    AllowDecimal="True"
    AllowNegative="False"
    CommitOnEnter="True"/>
```

### Thuoc tinh hay dung
- `Text` (chuoi) hoac `Value` (so). Neu la so, uu tien `Value`.
- `ValueType`: `String`, `Int32`, `Float`, `Double`, `Decimal`.
- `ValidationMode`: `Uppercase`, `Phone`, `IpAddress`, ...
- `MinValue` / `MaxValue`: rang buoc so.
- `ShowKeyboardButton`: bat nut ban phim ao.

## 6) DisplayField

### Muc dich
Hien thi du lieu read-only, co format va color-zone.

### Bien can co trong ViewModel
Vi du:
- `double CurrentSpeed`
- `double Temperature`
- `DateTime LastUpdateTime`

### Cach dung

```xml
<controls:DisplayField
    Value="{Binding CurrentSpeed}"
    ValueType="Double"
    DisplayDecimals="2"/>

<controls:DisplayField
    Value="{Binding Temperature}"
    ValueType="Double"
    DisplayDecimals="1"
    ColorZoneTarget="Background"
    Threshold1="40"
    Threshold2="60"
    Threshold3="80"
    ZoneColor1="#2E7D32"
    ZoneColor2="#F9A825"
    ZoneColor3="#EF6C00"
    ZoneColor4="#C62828"/>
```

## 7) ActionButton

### Muc dich
Nut nhan nang cao: mau theo state, blink, toggle, nhan giu, icon.

### Bien/command can co trong ViewModel
Tuy muc dich:
- Click: `ICommand StartCommand`
- Hold: `ICommand HomeHoldStartCommand`, `ICommand HomeHoldEndCommand`
- Toggle: `bool IsAutoMode`

### Dung co ban

```xml
<controls:ActionButton
    Text="START"
    NormalBackground="#4CAF50"
    HoverBackground="#66BB6A"
    PressedBackground="#388E3C"
    ClickCommand="{Binding StartCommand}"/>
```

### Toggle + doi text khi toggle

```xml
<controls:ActionButton
    Text="AUTO OFF"
    CheckedText="AUTO ON"
    IsToggle="True"
    IsChecked="{Binding IsAutoMode}"
    NormalBackground="#37474F"
    CheckedBackground="#1E88E5"/>
```

### Nhan giu (co the set thoi gian giu)

```xml
<controls:ActionButton
    Text="HOME (HOLD)"
    HoldDelayMs="1500"
    HoldCommand="{Binding HomeHoldStartCommand}"
    HoldCompletedCommand="{Binding HomeHoldEndCommand}"/>
```

### Them icon

```xml
<controls:ActionButton
    Text="APPLY"
    Icon="{StaticResource ApplyIcon}"
    IconPosition="Left"
    IconSize="22"
    ClickCommand="{Binding ResetCommand}"/>
```

### Cac thuoc tinh quan trong
- Mau: `NormalBackground`, `HoverBackground`, `PressedBackground`, `BlinkBackground`.
- Blink: `IsBlinking`, `BlinkIntervalMs`.
- Toggle: `IsToggle`, `IsChecked`, `CheckedBackground`, `CheckedText`.
- Hold: `HoldDelayMs`, `HoldCommand`, `HoldCompletedCommand`.
- Icon: `Icon`, `IconPosition`, `IconSize`, `ShowIcon`, `ShowText`.
- Layout: `ContentHorizontalAlignment`, `ContentVerticalAlignment`, `ContentPadding`, `CornerRadius`.

## 8) LogPanel

### Muc dich
Bang log co filter, search, export, clear.

### Cach dung nhanh trong project
Buoc 1 (quan trong): cau hinh Serilog day log len UI sink.

```csharp
using NAutoSuite.UI.Controls.Services;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.With<CallerTypeEnricher>()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.UISink()
    .CreateLogger();
```

Buoc 2: dat control vao view:

```xml
<controls:LogPanel Title="System Logs"/>
```

### ViewModel can gi?
- Mac dinh: khong can them gi. `LogPanel` tu co ViewModel rieng.

### Log thu cong (tuy chon)
Neu ban dat ten x:Name cho panel:

```csharp
MyLogPanel.LogInfo("Connected to device");
MyLogPanel.LogWarning("Pressure is low");
MyLogPanel.LogError("Failed to start", ex.ToString());
```

## Ghi chu cuoi

- Header/Footer toi uu cho 1920x1080 va co scale bang `Viewbox`.
- De de huong dan nguoi khac: giu tat ca binding va command trong 1 `MainViewModel`, va chi de `MainWindow` lam switch view.
