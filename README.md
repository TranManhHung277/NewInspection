# NAutoSuite UI Controls Guide

This file documents how to use core UI controls in new WPF projects, including the ViewModel properties/commands you must add.

## Prerequisites

- Add project reference to `src/UI/NAutoSuite.UI.Controls/NAutoSuite.UI.Controls.csproj`.
- Merge the UI theme resources if your app does not already do so.

A common pattern is: one `MainViewModel` owns header/footer state and commands, and `MainWindow` switches views based on footer tab events.

## Minimal MainViewModel template (recommended)

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

    // Content navigation
    [ObservableProperty] private object? _currentView;

    // Footer commands (ShellFooterControl expects these names)
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

## 1) ShellHeaderControl

### Purpose

Top header bar that shows machine info, project/model name, run mode, and status.

### XAML usage

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

### Required ViewModel properties

Add these to your ViewModel:

- `int MachineNumber`
- `MachineState MachineState` (from `NAutoSuite.Core.Machine`)
- `string ProjectName`
- `string ModelName`
- `MachineRunMode RunMode` (from `NAutoSuite.Core.Machine`)
- `HeaderStatusLevel HeaderStatusLevel` (from `NAutoSuite.UI.Controls`)

### Events

- `HeaderClicked`: optional, typically used to jump to a default tab.
- `MinimizeClicked`: minimize window.
- `CloseClicked`: confirm exit.

## 2) ShellFooterControl

### Purpose

Bottom navigation bar with tabs (Auto, Manual, Data, Camera, Setting, Log) and action buttons (Start/Stop/Reset/Home).

### XAML usage

```xml
<controls:ShellFooterControl
    x:Name="FooterControl"
    TabChanged="FooterControl_TabChanged"/>
```

### Required ViewModel commands

`ShellFooterControl` uses the control `DataContext` and expects these command properties to exist with these exact names:

- `ICommand StartCommand`
- `ICommand StopCommand`
- `ICommand ResetCommand`
- `ICommand HomeHoldStartCommand`
- `ICommand HomeHoldEndCommand`

Using CommunityToolkit, define them as `IRelayCommand` like in the template above.

### Handling tab change

```csharp
private void FooterControl_TabChanged(object? sender, string tabName)
{
    // tabName is one of: Auto, Manual, Data, Camera, Setting, Log
}
```

### Wiring views to tabs (recommended pattern)

1. Create the view classes (e.g., `AutoView`, `ManualView`, `DataView`, `CameraView`, `SettingView`, `LogView`).
2. Instantiate them once in `MainWindow` and switch by tab name.

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

## 3) InputField

### Purpose

Validated input field with optional on-screen keyboard and numeric formatting.

### ViewModel properties to add

- For text input: `string OperatorName`
- For numeric input: `double TargetSpeed` (or `int`, etc.)

### XAML usage

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

### Key properties

- `Text` (string) or `Value` (object). Prefer `Value` for numeric types.
- `ValueType`: `String`, `Int32`, `Float`, `Double`, `Decimal`.
- `KeyboardType`: `Text` or `Numeric`.
- `ValidationMode`: `None`, `Uppercase`, `Phone`, `IpAddress`, `MacAddress`, `Email`.
- `MinValue` / `MaxValue` for numeric range validation.
- `CommitOnEnter`: if true, commits only when Enter is pressed.
- `ShowKeyboardButton`: shows the keyboard button.

## 4) DisplayField

### Purpose

Read-only value display with formatting and optional color zoning.

### ViewModel properties to add

- Example: `double CurrentSpeed`, `double Temperature`, or `DateTime LastUpdateTime`

### XAML usage

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

### Key properties

- `Value` and `ValueType` (same enum as above but includes `DateTime`).
- `DisplayDecimals` or `FormatString` for formatting.
- `ColorZoneTarget`: `None`, `Border`, `Background`, `Text`.
- `Threshold1/2/3` and `ZoneColor1/2/3/4` for zoning.

## 5) ActionButton

### Purpose

Reusable button with normal/hover/pressed colors, optional blinking, icon placement, toggle mode, and click vs hold behavior.

### ViewModel properties/commands to add

- Click: `ICommand StartCommand`
- Hold: `ICommand HomeHoldStartCommand`, `ICommand HomeHoldEndCommand`
- Toggle: `bool IsAutoMode` (bind to `IsChecked`)

### XAML usage

```xml
<controls:ActionButton
    Text="START"
    Width="200"
    Height="64"
    NormalBackground="#4CAF50"
    HoverBackground="#66BB6A"
    PressedBackground="#388E3C"
    BlinkBackground="#FFD54F"
    IsBlinking="{Binding IsStartRequired}"
    ClickCommand="{Binding StartCommand}"
    HoldCommand="{Binding HomeHoldStartCommand}"
    HoldCompletedCommand="{Binding HomeHoldEndCommand}"/>
```

### Toggle usage

```xml
<controls:ActionButton
    Text="AUTO OFF"
    CheckedText="AUTO ON"
    IsToggle="True"
    IsChecked="{Binding IsAutoMode}"
    NormalBackground="#37474F"
    CheckedBackground="#1E88E5"
    ClickCommand="{Binding ToggleAutoModeCommand}"/>
```

### Hold usage (press and hold)

```xml
<controls:ActionButton
    Text="HOME (HOLD)"
    HoldDelayMs="1500"
    HoldCommand="{Binding HomeHoldStartCommand}"
    HoldCompletedCommand="{Binding HomeHoldEndCommand}"/>
```

### Icon usage

```xml
<controls:ActionButton
    Text="APPLY"
    Icon="{StaticResource ApplyIcon}"
    IconPosition="Left"
    IconSize="22"
    ClickCommand="{Binding ResetCommand}"/>
```

### Key properties

- Colors: `NormalBackground`, `HoverBackground`, `PressedBackground`, `BlinkBackground`.
- Toggle: `IsToggle`, `IsChecked`, `CheckedBackground`, `CheckedText`.
- Blink: `IsBlinking`, `BlinkIntervalMs`.
- Hold: `HoldDelayMs` (milliseconds), `HoldCommand`, `HoldCompletedCommand`.
- Icon: `Icon`, `IconPosition`, `IconSize`, `ShowIcon`, `ShowText`.
- Layout: `ContentHorizontalAlignment`, `ContentVerticalAlignment`, `ContentPadding`, `CornerRadius`.

## Notes

- Header and footer controls are designed for 1920x1080 layouts; they scale using `Viewbox`.
- For new projects, keep bindings in a single main ViewModel to simplify onboarding.
