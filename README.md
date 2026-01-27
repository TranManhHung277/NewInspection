# NAutoSuite UI Controls Guide

This file documents how to use core UI controls in new WPF projects.

## Prerequisites

- Add project reference to `src/UI/NAutoSuite.UI.Controls/NAutoSuite.UI.Controls.csproj`.
- Merge the UI theme resources if your app does not already do so.

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
Footer uses `DataContext` reflection to find these commands:
- `ICommand StartCommand`
- `ICommand StopCommand`
- `ICommand ResetCommand`
- `ICommand HomeHoldStartCommand`
- `ICommand HomeHoldEndCommand`

### Handling tab change
```csharp
private void FooterControl_TabChanged(object? sender, string tabName)
{
    // tabName is one of: Auto, Manual, Data, Camera, Setting, Log
}
```

### Wiring views to tabs (recommended pattern)
1) Create the view classes (e.g., `AutoView`, `ManualView`, `DataView`, `CameraView`, `SettingView`, `LogView`).
2) Instantiate them once in `MainWindow` and switch by tab name.

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

## Notes
- Header and footer controls are designed for 1920x1080 layouts; they scale using `Viewbox`.
- For new projects, keep bindings in a single main ViewModel to simplify onboarding.
