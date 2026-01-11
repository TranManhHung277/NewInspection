# Kiến trúc NAutoSuite

## Tổng quan

NAutoSuite áp dụng kiến trúc 3 tầng với các nguyên tắc:
- **Separation of Concerns**: Mỗi layer có trách nhiệm riêng
- **Dependency Inversion**: Phụ thuộc vào abstraction, không phụ thuộc vào implementation
- **Single Responsibility**: Mỗi class chỉ làm một việc
- **Open/Closed**: Mở để mở rộng, đóng để sửa đổi

## Các Tầng

### 1. Core Layer (Business Logic)

**Trách nhiệm**:
- Định nghĩa business logic
- State machine cho máy
- Alarm management
- Recipe management
- Realtime scheduling

**Không phụ thuộc**:
- UI framework (WPF)
- Hardware cụ thể
- External dependencies

**Interfaces chính**:
```csharp
IMachine        // Machine control
IDevice         // Base device
IAxis           // Motion axis
IInput/IOutput  // Digital I/O
```

### 2. Hardware Layer (Infrastructure)

**Trách nhiệm**:
- Implement hardware interfaces
- Wrapper cho hardware SDK
- Communication protocol

**Phụ thuộc**:
- Core (abstractions only)
- Hardware vendor SDKs

**Structure**:
```
Hardware/
├── Abstractions/    # Interfaces
├── Leadshine/       # Leadshine implementation
├── Simulator/       # Test simulator
├── Vision/          # Camera drivers
└── PLC/             # PLC communication
```

### 3. UI Layer (Presentation)

**Trách nhiệm**:
- User interface
- Data binding
- View navigation
- User interaction

**Phụ thuộc**:
- Core (abstractions)
- UI.Infrastructure
- UI.Themes
- UI.Controls

**Pattern**: MVVM
- View (XAML)
- ViewModel (C#)
- Model (Core)

## Data Flow

```
┌─────────────┐
│     UI      │ ← User Interaction
└──────┬──────┘
       │ Binding
       ▼
┌─────────────┐
│  ViewModel  │ ← Commands, Properties
└──────┬──────┘
       │ Calls
       ▼
┌─────────────┐
│  Machine    │ ← Business Logic
│   (Core)    │
└──────┬──────┘
       │ Uses
       ▼
┌─────────────┐
│  Hardware   │ ← Hardware Control
│ (Abstraction)│
└──────┬──────┘
       │ Implements
       ▼
┌─────────────┐
│  Driver     │ ← SDK/DLL
└─────────────┘
```

## Dependency Injection

Tất cả dependencies được inject qua constructor:

```csharp
// Registration
services.AddSingleton<IAxis, LeadshineAxis>();
services.AddSingleton<IMachine, MyMachine>();

// Usage
public class MyMachine : MachineBase
{
    private readonly IAxis _axis;

    public MyMachine(IAxis axis)
    {
        _axis = axis;
    }
}
```

**Lợi ích**:
- Testable (mock dependencies)
- Loosely coupled
- Easy to swap implementations

## State Machine

Machine states được quản lý bởi `Stateless` library:

```
┌──────────────┐
│Uninitialized │
└──────┬───────┘
       │ Initialize
       ▼
┌──────────────┐
│     Idle     │◄────┐
└──────┬───────┘     │
       │ Start       │ Reset
       ▼             │
┌──────────────┐     │
│   Running    │─────┘
└──────┬───────┘
       │ Stop
       ▼
┌──────────────┐
│   Stopped    │
└──────────────┘
```

## Threading Model

- **UI Thread**: WPF dispatcher
- **Background Thread**: Machine cycle, hardware polling
- **Sync Context**: Sử dụng `async/await`

```csharp
// Scheduler chạy realtime loop
var scheduler = new Scheduler(TimeSpan.FromMilliseconds(10));
scheduler.Start(async ct =>
{
    // 10ms cycle
    await UpdateInputsAsync();
    await RunLogicAsync();
    await UpdateOutputsAsync();
});
```

## Error Handling Strategy

### 1. Result Pattern

```csharp
public async Task<Result> OperationAsync()
{
    try
    {
        // Work
        return Result.Success();
    }
    catch (Exception ex)
    {
        return Result.Failure("Error", ex);
    }
}
```

### 2. Alarm System

```csharp
_alarmManager.Raise(1001, "Motor overload", AlarmSeverity.Error);
```

### 3. Logging

```csharp
_logger.Error(ex, "Failed to initialize axis {Name}", axisName);
```

## Testing Strategy

### Unit Tests
- Core logic tests
- Không cần hardware
- Sử dụng mocks

```csharp
var mockAxis = new Mock<IAxis>();
var machine = new MyMachine(mockAxis.Object);
```

### Integration Tests
- Test với Simulator
- Test full cycle

### Hardware Tests
- Test với hardware thật
- Manual testing

## Performance Considerations

### Realtime Requirements
- Cycle time: 1-100ms
- Jitter: < 1ms
- Sử dụng `PeriodicTimer` cho accurate timing

### Memory Management
- Reuse objects
- Pool allocations
- Avoid GC trong realtime loop

### Threading
- Dedicated thread cho realtime
- Thread pool cho background tasks

## Security

### User Management
- Role-based access control
- Password hashing
- Session management

### Data Protection
- Recipe encryption
- Audit logging
- Backup strategy

## Scalability

### Multi-Machine
- Multiple machine instances
- Central coordination
- Shared resources

### MES Integration
- REST API
- Message queue (MQTT, RabbitMQ)
- Database sync

## Extension Points

### Custom Hardware
1. Implement `IAxis`, `ICamera`, etc.
2. Register trong DI
3. No code change needed

### Custom Controls
1. Create UserControl
2. Add to UI.Controls
3. Use in XAML

### Custom Machine Logic
1. Inherit `MachineBase`
2. Override lifecycle methods
3. Implement cycle logic

## Design Patterns Sử dụng

- **Factory Pattern**: Device creation
- **Strategy Pattern**: Algorithm selection
- **Observer Pattern**: Event handling
- **Template Method**: MachineBase lifecycle
- **Dependency Injection**: IoC
- **MVVM**: UI separation
- **Repository Pattern**: Data access (future)
