# LogPanel Title Customization

## Overview

LogPanel giờ hỗ trợ **customizable title** thông qua `Title` DependencyProperty. Bạn có thể đặt tên khác nhau cho từng LogPanel instance.

## Usage

### Default Title (No customization)

```xml
<UserControl xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <!-- Default title: "System Logs" -->
    <controls:LogPanel/>

</UserControl>
```

**Result**: Title hiển thị "System Logs"

### Custom Title

```xml
<UserControl xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <!-- Custom title -->
    <controls:LogPanel Title="Hardware Logs"/>

</UserControl>
```

**Result**: Title hiển thị "Hardware Logs"

## Examples

### Example 1: Different titles for different views

#### Main Log View
```xml
<!-- File: LogView.xaml -->
<UserControl x:Class="Machine.PickAndPlace.Views.LogView"
             xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <controls:LogPanel Title="System Logs"/>

</UserControl>
```

#### Diagnostic View
```xml
<!-- File: DiagnosticView.xaml -->
<UserControl x:Class="Machine.PickAndPlace.Views.DiagnosticView"
             xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <controls:LogPanel Title="Diagnostic Logs"/>

</UserControl>
```

#### Hardware Monitor View
```xml
<!-- File: HardwareMonitorView.xaml -->
<UserControl x:Class="Machine.PickAndPlace.Views.HardwareMonitorView"
             xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <controls:LogPanel Title="Hardware Events"/>

</UserControl>
```

### Example 2: Multiple LogPanels in same view

```xml
<UserControl xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="*"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- Application Logs -->
        <controls:LogPanel Grid.Row="0"
                          Title="Application Logs"
                          Margin="10"/>

        <!-- Error Logs Only -->
        <controls:LogPanel Grid.Row="1"
                          Title="Error Logs"
                          Margin="10"/>
    </Grid>

</UserControl>
```

**Note**: Cả 2 LogPanel sẽ nhận cùng logs từ UILogService. Bạn có thể filter riêng cho từng panel bằng cách unchecks các log levels không cần thiết.

### Example 3: Binding Title to ViewModel

```xml
<UserControl xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <!-- Bind title to ViewModel property -->
    <controls:LogPanel Title="{Binding LogPanelTitle}"/>

</UserControl>
```

```csharp
// ViewModel
public class MyViewModel : ObservableObject
{
    [ObservableProperty]
    private string _logPanelTitle = "Custom Log Title";

    public void ChangeTitle()
    {
        LogPanelTitle = "Updated Title";
    }
}
```

### Example 4: Dynamic title with machine info

```xml
<UserControl xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <!-- Title includes machine number -->
    <controls:LogPanel Title="{Binding MachineNumber, StringFormat='Machine {0} - Logs'}"/>

</UserControl>
```

**Result**: Title hiển thị "Machine 1 - Logs", "Machine 2 - Logs", etc.

## Implementation Details

### DependencyProperty Definition

```csharp
// LogPanel.xaml.cs
public partial class LogPanel : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(LogPanel),
            new PropertyMetadata("System Logs")); // Default value

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
```

### XAML Binding

```xml
<!-- LogPanel.xaml -->
<TextBlock Text="{Binding Title, RelativeSource={RelativeSource AncestorType=UserControl}}"
           FontSize="28" FontWeight="Bold"
           Foreground="White" VerticalAlignment="Center"/>
```

**Key points:**
- Uses `RelativeSource` to bind to UserControl's Title property
- `AncestorType=UserControl` finds the LogPanel instance
- Works with WPF data binding and two-way updates

## Advanced Scenarios

### Scenario 1: Language/Localization

```csharp
// Resources.resx
public static string LogPanel_Title_System => "System Logs";
public static string LogPanel_Title_Hardware => "Hardware Logs";
public static string LogPanel_Title_Errors => "Error Logs";
```

```xml
<controls:LogPanel Title="{x:Static res:Resources.LogPanel_Title_System}"/>
```

### Scenario 2: Title with Icon

While the current implementation only supports text, you can extend it:

```xml
<StackPanel Orientation="Horizontal">
    <!-- Custom title with icon -->
    <Path Data="..." Fill="White" Width="24" Height="24" Margin="0,0,10,0"/>
    <TextBlock Text="Logs" FontSize="28"/>
</StackPanel>
```

If you need this, consider adding a `TitleTemplate` property (future enhancement).

### Scenario 3: Title based on Log Count

```xml
<controls:LogPanel>
    <controls:LogPanel.Title>
        <MultiBinding StringFormat="Logs ({0} entries)">
            <Binding Path="FilteredLogs.Count"/>
        </MultiBinding>
    </controls:LogPanel.Title>
</controls:LogPanel>
```

**Note**: This requires LogPanelViewModel exposure. Current implementation uses internal ViewModel.

## Current LogView Configuration

Trong PickAndPlace.Frontend project, LogView hiện tại:

```xml
<!-- File: LogView.xaml -->
<UserControl x:Class="Machine.PickAndPlace.Views.LogView"
             xmlns:controls="clr-namespace:NAutoSuite.UI.Controls;assembly=NAutoSuite.UI.Controls">

    <!-- Uses default title "System Logs" -->
    <controls:LogPanel/>

</UserControl>
```

**To customize**, thay đổi thành:

```xml
<controls:LogPanel Title="Pick & Place Logs"/>
```

hoặc:

```xml
<controls:LogPanel Title="Machine Logs"/>
```

hoặc binding động:

```xml
<controls:LogPanel Title="{Binding ProjectName, StringFormat='{0} - Logs'}"/>
```

## Benefits

✅ **Reusable**: Same LogPanel control, different titles
✅ **Flexible**: Supports static text, bindings, and localization
✅ **Simple**: Just one property to change
✅ **Standard WPF**: Uses DependencyProperty pattern

## Future Enhancements

Potential improvements:
- [ ] `TitleTemplate` for complex title layouts (icons, buttons)
- [ ] `ShowTitle` bool to hide/show title section
- [ ] `TitleFontSize`, `TitleForeground` for styling
- [ ] `SubTitle` for secondary text under main title
