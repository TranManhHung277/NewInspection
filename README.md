# NAutoSuite

**Kiến trúc chuẩn công nghiệp cho hệ thống tự động hóa C# .NET 8 + WPF**

NAutoSuite là framework tổng hợp giúp phát triển nhanh các ứng dụng máy tự động hóa, HMI công nghiệp với kiến trúc module hóa, dễ bảo trì và mở rộng.

## Tính năng chính

- **Kiến trúc phân tầng rõ ràng**: Core - Hardware - UI hoàn toàn tách biệt
- **Hardware Abstraction**: Thay đổi phần cứng không cần sửa logic
- **MVVM Pattern**: UI độc lập với business logic
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **Realtime**: Scheduler hỗ trợ chu kỳ thời gian thực
- **State Machine**: Quản lý trạng thái máy chuẩn công nghiệp
- **Logging**: Serilog với file rotation tự động
- **Theme System**: Dark theme có thể tùy chỉnh
- **Industrial Controls**: LED, State indicator, Alarm list...

## Cấu trúc Solution

```
NAutoSuite/
├── src/
│   ├── Core/
│   │   └── NAutoSuite.Core                    # Logic core dùng chung
│   │
│   ├── Hardware/
│   │   ├── NAutoSuite.Hardware.Abstractions   # Interface phần cứng
│   │   ├── NAutoSuite.Hardware.Leadshine      # Driver Leadshine
│   │   └── NAutoSuite.Hardware.Simulator      # Simulator cho test
│   │
│   ├── UI/
│   │   ├── NAutoSuite.UI.Infrastructure       # MVVM framework
│   │   ├── NAutoSuite.UI.Themes               # Theme tái sử dụng
│   │   └── NAutoSuite.UI.Controls             # Custom controls
│   │
│   └── Apps/
│       └── NAutoSuite.App.Template            # Template WPF app
│
└── projects/                                   # Các dự án thực tế
```

## Bắt đầu nhanh

### 1. Build solution

```bash
dotnet build NAutoSuite.sln
```

### 2. Chạy Template App

```bash
cd src/Apps/NAutoSuite.App.Template
dotnet run
```

### 3. Tạo dự án mới

Copy folder `NAutoSuite.App.Template` và tùy chỉnh:

```bash
# Copy template
cp -r src/Apps/NAutoSuite.App.Template projects/MyMachine

# Đổi tên namespace
# Sửa file Machine/TemplateMachine.cs
# Cấu hình hardware trong App.xaml.cs
```

## Cấu trúc Core Module

### State Machine

```csharp
public class MyMachine : MachineBase
{
    public MyMachine(string id, string name)
        : base(id, name)
    {
    }

    protected override async Task OnRunningAsync()
    {
        // Logic chu kỳ máy
        await Task.Delay(1000);
    }
}
```

### Hardware Abstraction

```csharp
// Định nghĩa interface
public interface IAxis
{
    Task<Result> MoveAbsoluteAsync(double position);
    Task<Result> HomeAsync();
}

// Leadshine implementation
public class LeadshineAxis : AxisBase { }

// Simulator implementation
public class SimulatorAxis : AxisBase { }

// Thay đổi trong DI - không cần sửa logic
services.AddSingleton<IAxis, LeadshineAxis>();  // Production
services.AddSingleton<IAxis, SimulatorAxis>();  // Development
```

### Dependency Injection

```csharp
// App.xaml.cs
protected override void OnStartup(StartupEventArgs e)
{
    _host = Host.CreateDefaultBuilder()
        .ConfigureServices((context, services) =>
        {
            // Hardware
            services.AddSingleton<IAxis>(sp =>
                new LeadshineAxis("X", "X Axis", 0, 0));

            // Machine
            services.AddSingleton<IMachine, MyMachine>();

            // ViewModels
            services.AddSingleton<MainViewModel>();
        })
        .Build();
}
```

## Các Module Chính

### 1. NAutoSuite.Core

**Chức năng**: Logic nghiệp vụ cốt lõi, độc lập với hardware và UI

**Thành phần**:
- `MachineBase`: Base class cho tất cả máy
- `AxisBase`: Base class cho motion axis
- `AlarmManager`: Quản lý alarm
- `RecipeBase`: Quản lý recipe/công thức
- `Scheduler`: Realtime task scheduler
- `Result<T>`: Result pattern cho error handling

**Sử dụng**:
```csharp
public class MyMachine : MachineBase
{
    public override async Task InitializeAsync()
    {
        // Khởi tạo
        return Result.Success("Initialized");
    }
}
```

### 2. Hardware.Abstractions

**Chức năng**: Interface chuẩn cho tất cả hardware

**Interfaces**:
- `IAxis`: Motion control
- `ICamera`: Machine vision
- `IPlc`: PLC communication
- `IIOModule`: Digital I/O
- `IEtherCATMaster`: EtherCAT fieldbus

**Lợi ích**: Đổi phần cứng chỉ cần implement lại interface, không đổi logic

### 3. Hardware Implementations

**Leadshine**: Driver cho motion controller Leadshine (DMC series)
**Simulator**: Giả lập hardware cho test không cần thiết bị

**Thêm driver mới**:
1. Tạo project `NAutoSuite.Hardware.YourHardware`
2. Implement interface từ `Hardware.Abstractions`
3. Register trong DI container

### 4. UI.Infrastructure

**MVVM Framework**:
- `ViewModelBase`: Base cho ViewModels
- `RelayCommand`: Command implementation
- `NavigationService`: Navigation giữa views
- Converters: BoolToVisibility, InverseBool...

### 5. UI.Themes

**Dark Theme** với color scheme công nghiệp:
- Consistent colors
- Button styles (Primary, Success, Warning, Error)
- Control styles (TextBox, Card, CheckBox...)

**Sử dụng**:
```xml
<Window.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/NAutoSuite.UI.Themes;component/Theme.Dark.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Window.Resources>
```

### 6. UI.Controls

**Custom Controls**:
- `IOIndicator`: LED indicator cho I/O
- `MachineStateControl`: Hiển thị trạng thái máy
- `AxisStatusControl`: (TODO) Trạng thái axis
- `AlarmListControl`: (TODO) Danh sách alarm

## Best Practices

### 1. Tổ chức Code

- **Core**: Chỉ chứa business logic, không phụ thuộc UI/Hardware cụ thể
- **Hardware**: Implement interface, wrapper cho SDK phần cứng
- **UI**: Chỉ binding và hiển thị, không chứa logic
- **Machine**: Kế thừa `MachineBase`, override methods cần thiết

### 2. Error Handling

Sử dụng `Result<T>` pattern:

```csharp
public async Task<Result> DoSomethingAsync()
{
    try
    {
        // Logic here
        return Result.Success("Operation completed");
    }
    catch (Exception ex)
    {
        return Result.Failure("Operation failed", ex);
    }
}

// Sử dụng
var result = await DoSomethingAsync();
if (result.IsSuccess)
{
    // Success
}
else
{
    // Handle error: result.Message, result.Exception
}
```

### 3. Logging

```csharp
// Khởi tạo trong App.xaml.cs
LogService.Initialize("logs/machine-.log");

// Sử dụng
_logger.Information("Machine started");
_logger.Warning("Temperature high: {Temp}", temperature);
_logger.Error(ex, "Failed to connect");
```

### 4. State Management

```csharp
// Machine tự động quản lý state transitions
await _machine.InitializeAsync();  // Uninitialized -> Idle
await _machine.StartAsync();       // Idle -> Running
await _machine.PauseAsync();       // Running -> Paused
await _machine.StopAsync();        // * -> Stopped
await _machine.ResetAsync();       // * -> Idle
```

## Mở rộng

### Thêm Hardware mới

1. Tạo project trong `src/Hardware/`
2. Implement interface từ `Hardware.Abstractions`
3. Thêm reference vào App project
4. Đăng ký trong DI

### Thêm Custom Control

1. Tạo UserControl trong `NAutoSuite.UI.Controls`
2. Định nghĩa DependencyProperty
3. Sử dụng trong XAML

### Thêm Feature mới

1. Core logic → `NAutoSuite.Core`
2. Hardware interface → `Hardware.Abstractions`
3. UI control → `UI.Controls`
4. ViewModel → App project

## Công nghệ sử dụng

- **.NET 8**: Target framework
- **WPF**: Desktop UI framework
- **CommunityToolkit.Mvvm**: MVVM helpers
- **Serilog**: Structured logging
- **Stateless**: State machine
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **Microsoft.Extensions.Hosting**: Application lifetime management

## Roadmap

- [ ] EtherCAT driver implementation
- [ ] Vision camera drivers (Basler, Hikvision)
- [ ] PLC communication (Mitsubishi, Siemens)
- [ ] Recipe management UI
- [ ] Alarm history viewer
- [ ] Real-time chart controls
- [ ] Multi-language support
- [ ] User management & permissions
- [ ] MES integration template
- [ ] SCADA connector

## Đóng góp

Mọi đóng góp đều được chào đón! Vui lòng:
1. Fork repo
2. Tạo feature branch
3. Commit changes
4. Push và tạo Pull Request

## License

MIT License - Tự do sử dụng cho commercial và open source projects

## Liên hệ

- Issues: [GitHub Issues](https://github.com/yourname/NAutoSuite/issues)
- Discussions: [GitHub Discussions](https://github.com/yourname/NAutoSuite/discussions)

---

**Phát triển bởi**: NAutoSuite Team
**Phiên bản**: 1.0.0
**Cập nhật**: 2026-01-11
