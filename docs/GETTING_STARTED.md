# Hướng dẫn bắt đầu - NAutoSuite

## Yêu cầu hệ thống

- **OS**: Windows 10/11 (64-bit)
- **.NET SDK**: 8.0 hoặc mới hơn
- **IDE**: Visual Studio 2022 hoặc VS Code
- **RAM**: Tối thiểu 4GB
- **Disk**: 500MB cho framework

## Cài đặt .NET 8

```bash
# Kiểm tra phiên bản hiện tại
dotnet --version

# Download từ: https://dotnet.microsoft.com/download/dotnet/8.0
```

## Clone Repository

```bash
git clone https://github.com/yourname/NAutoSuite.git
cd NAutoSuite
```

## Build Project

```bash
# Build toàn bộ solution
dotnet build NAutoSuite.sln

# Hoặc build specific project
dotnet build src/Apps/NAutoSuite.App.Template/NAutoSuite.App.Template.csproj
```

## Chạy Template App

```bash
cd src/Apps/NAutoSuite.App.Template
dotnet run
```

App sẽ mở với giao diện:
- **Machine State**: Hiển thị trạng thái hiện tại
- **Initialize**: Kết nối hardware (simulator)
- **Start**: Bắt đầu chu kỳ máy
- **Stop**: Dừng máy
- **Reset**: Reset về trạng thái ban đầu

## Tạo dự án mới

### Bước 1: Copy Template

```bash
# Copy vào thư mục projects
cp -r src/Apps/NAutoSuite.App.Template projects/MyFirstMachine

# Đổi tên solution
cd projects/MyFirstMachine
```

### Bước 2: Sửa Namespace

Tìm và thay thế `NAutoSuite.App.Template` → `MyFirstMachine` trong:
- `*.csproj`
- `*.cs`
- `*.xaml`

### Bước 3: Customize Machine

Sửa `Machine/TemplateMachine.cs`:

```csharp
public class MyFirstMachine : MachineBase
{
    public MyFirstMachine(string id, string name, IAxis? axisX = null)
        : base(id, name)
    {
        _axisX = axisX;
    }

    protected override async Task OnRunningAsync()
    {
        await base.OnRunningAsync();

        // YOUR LOGIC HERE
        _logger.Information("Cycle {Count}", Context.CycleCount);

        // Example: Move axis
        if (_axisX != null && _axisX.IsHomed)
        {
            await _axisX.MoveAbsoluteAsync(100);
            await Task.Delay(1000);
            await _axisX.MoveAbsoluteAsync(0);
        }
    }
}
```

### Bước 4: Configure DI

Sửa `App.xaml.cs`:

```csharp
.ConfigureServices((context, services) =>
{
    // Hardware - thay Simulator bằng driver thật khi ready
    services.AddSingleton<IAxis>(sp =>
        new SimulatorAxis("AxisX", "X Axis", Log.Logger));

    // Machine
    services.AddSingleton<IMachine>(sp =>
    {
        var axisX = sp.GetRequiredService<IAxis>();
        return new MyFirstMachine("M001", "My Machine", axisX, Log.Logger);
    });

    // ViewModels
    services.AddSingleton<MainViewModel>();
    services.AddSingleton<MainWindow>();
})
```

### Bước 5: Run

```bash
dotnet run
```

## Thêm Hardware thật

### Leadshine Motion Controller

1. **Install driver DLL**:
   - Copy `LTDMC.dll` vào `bin/Debug/net8.0-windows/`

2. **Sửa DI registration**:

```csharp
// Thay Simulator
services.AddSingleton<IAxis>(sp =>
    new LeadshineAxis("AxisX", "X Axis", cardId: 0, axisIndex: 0, Log.Logger));
```

3. **Configure parameters**:

```csharp
var axis = new LeadshineAxis("X", "X Axis", 0, 0);
await axis.ConnectAsync();
await axis.SetServoAsync(true);
await axis.HomeAsync();
```

### Camera (Future)

```csharp
services.AddSingleton<ICamera>(sp =>
    new BaslerCamera("Cam1", "Main Camera"));
```

### PLC (Future)

```csharp
services.AddSingleton<IPlc>(sp =>
    new MitsubishiPlc("PLC1", "192.168.1.100"));
```

## Customize UI

### Thay đổi Theme

Sửa `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <!-- Sử dụng theme có sẵn -->
            <ResourceDictionary Source="pack://application:,,,/NAutoSuite.UI.Themes;component/Theme.Dark.xaml"/>

            <!-- Hoặc override colors -->
            <ResourceDictionary>
                <SolidColorBrush x:Key="PrimaryBrush" Color="#FF6200EE"/>
            </ResourceDictionary>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Thêm View mới

1. Tạo `Views/SettingsView.xaml`
2. Tạo `ViewModels/SettingsViewModel.cs`
3. Register trong DI
4. Navigate: `_navigationService.NavigateTo<SettingsViewModel>()`

### Sử dụng Custom Controls

```xml
<controls:IOIndicator
    LabelText="Sensor 1"
    IsActive="{Binding Sensor1Active}"/>

<controls:MachineStateControl
    State="{Binding MachineState}"/>
```

## Debugging

### Logging

Logs được lưu tại: `logs/app-YYYYMMDD.log`

```csharp
_logger.Debug("Debug info");
_logger.Information("Info message");
_logger.Warning("Warning");
_logger.Error(ex, "Error occurred");
```

### Breakpoints

Set breakpoints trong:
- `OnRunningAsync()`: Cycle logic
- `OnInitializingAsync()`: Initialization
- ViewModels: UI logic

### Simulator Mode

Sử dụng Simulator để test không cần hardware:

```csharp
services.AddSingleton<IAxis>(sp =>
    new SimulatorAxis("X", "X Axis", Log.Logger));
```

## Deployment

### Build Release

```bash
dotnet publish -c Release -r win-x64 --self-contained
```

Output: `bin/Release/net8.0-windows/win-x64/publish/`

### Create Installer

Sử dụng:
- **WiX Toolset**: MSI installer
- **Inno Setup**: Setup wizard
- **MSIX**: Windows Store format

### Configuration

Tạo `appsettings.json`:

```json
{
  "Machine": {
    "Id": "MACHINE_001",
    "Name": "Production Line 1"
  },
  "Hardware": {
    "AxisX": {
      "CardId": 0,
      "AxisIndex": 0
    }
  },
  "Logging": {
    "MinimumLevel": "Information"
  }
}
```

## Troubleshooting

### Build Errors

**Error**: Cannot find NAutoSuite.Core
- **Fix**: `dotnet restore`

**Error**: NU1605 Dependency conflict
- **Fix**: Update packages to compatible versions

### Runtime Errors

**Error**: DLL not found (LTDMC.dll)
- **Fix**: Copy DLL to output directory
- Set "Copy to Output Directory" = "Copy if newer"

**Error**: Hardware connection failed
- **Fix**:
  - Check hardware power
  - Check USB/Ethernet connection
  - Verify driver installation

### Performance Issues

**UI freezing**:
- Đảm bảo sử dụng `async/await`
- Không block UI thread

**High CPU**:
- Kiểm tra cycle time trong Scheduler
- Optimize realtime loop

## Next Steps

1. Đọc [ARCHITECTURE.md](ARCHITECTURE.md) để hiểu kiến trúc
2. Xem [API_REFERENCE.md](API_REFERENCE.md) cho chi tiết API
3. Tham gia [Discussions](https://github.com/yourname/NAutoSuite/discussions)
4. Check [Examples](../examples/) folder

## Support

- **Documentation**: [docs/](../docs/)
- **Issues**: [GitHub Issues](https://github.com/yourname/NAutoSuite/issues)
- **Email**: support@nautosuit.com
