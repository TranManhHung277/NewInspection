# Machine Pick and Place - Example Project

Đây là project ví dụ hoàn chỉnh cho NAutoSuite với UI Shell chuẩn công nghiệp.

## UI Layout

### Header
- **Machine Number**: #1 (có thể thay đổi trong Settings)
- **Machine State**: Hiển thị trạng thái realtime
- **Alarm**: Thông báo lỗi
- **Project Name**: Pick & Place Machine (center)
- **Logo + Time**: Ở góc phải
- **Close Button**: Đóng ứng dụng

### Content Area
Hiển thị view tương ứng với tab được chọn (Auto, Manual, Setting, Data, Log)

### Footer Navigation
5 tabs: **Auto** | **Manual** | **Setting** | **Data** | **Log**

## Cấu trúc Project

```
Machine.PickAndPlace/
├── Machine/
│   └── PickAndPlaceMachine.cs     # Machine logic
├── ViewModels/
│   └── MainViewModel.cs            # Main ViewModel
├── Views/
│   ├── AutoView.xaml              # Auto mode view
│   ├── ManualView.xaml            # Manual control
│   ├── SettingView.xaml           # Settings
│   ├── DataView.xaml              # Production data
│   └── LogView.xaml               # System logs
├── App.xaml.cs                     # DI setup
└── MainWindow.xaml                 # Shell layout
```

## Cách chạy

### Đã fix tất cả lỗi

Project đã được fix và chạy thành công. Các lỗi đã được sửa:

1. **Namespace conflict**: Đổi `Machine.PickAndPlace.Machine` thành `Machine.PickAndPlace.Machines`
2. **Logger field**: Thêm `_machineLogger` field trong PickAndPlaceMachine.cs
3. **Context access**: Đổi `_machine.Context` thành `_pickAndPlaceMachine.Context`
4. **Switch expression**: Thêm explicit cast `(object)` cho switch expression
5. **StartupUri**: Remove StartupUri từ App.xaml vì dùng DI
6. **Theme resources**: Merge Theme.Dark.xaml vào App.xaml resources

### Build và chạy

```bash
cd projects/Machine.PickAndPlace
dotnet build
dotnet run
```

Ứng dụng sẽ khởi động và tự động initialize machine. Kiểm tra logs trong thư mục `logs/`.

## Tính năng

### Auto Mode
- Hiển thị số chu kỳ đã chạy
- Thời gian chạy
- Visualization (placeholder)
- Control panel: Start, Stop, Reset, Home All

### Manual Mode
- Manual control (placeholder)

### Setting Mode
- Cài đặt Machine Number
- Cài đặt Project Name

### Data Mode
- Production statistics (placeholder)

### Log Mode
- System logs (placeholder)

## Customization

### Thay đổi Machine Number

Trong SettingView, sửa Machine Number và nó sẽ update header ngay lập tức.

### Thay đổi Theme

Tạo file theme riêng trong project và merge vào `App.xaml`:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceDictionary Source="pack://application:,,,/NAutoSuite.UI.Themes;component/Theme.Dark.xaml"/>
            <ResourceDictionary Source="Themes/CustomTheme.xaml"/>
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

### Thêm View mới

1. Tạo XAML trong `Views/`
2. Thêm vào `MainWindow.xaml.cs` constructor
3. Thêm case trong `FooterControl_TabChanged`
4. (Optional) Thêm tab trong `ShellFooterControl`

## Mở rộng

### Thêm hardware thật

Trong `App.xaml.cs`, thay Simulator:

```csharp
services.AddSingleton<IAxis>(sp =>
    new LeadshineAxis("AxisX", "X Axis", cardId: 0, axisIndex: 0));
```

### Thêm logic machine

Sửa `PickAndPlaceMachine.cs`:
- `OnInitializingAsync()`: Khởi tạo
- `OnRunningAsync()`: Chu kỳ auto
- `PickSequenceAsync()`, `PlaceSequenceAsync()`: Custom logic

## Screenshot

```
┌─────────────────────────────────────────────────────────────┐
│ #1 │ [Running] │ [No Alarm]  Pick & Place  [LOGO]  [X] │
│────────────────────────────────────────────────────────────│
│                                                             │
│                   [AUTO VIEW CONTENT]                       │
│                                                             │
│────────────────────────────────────────────────────────────│
│  [▶ Auto]  [🎮 Manual]  [⚙ Setting]  [📊 Data]  [📝 Log]  │
└─────────────────────────────────────────────────────────────┘
```

## Next Steps

1. Fix các lỗi compile ở trên
2. Build & run
3. Test navigation giữa các tabs
4. Customize theo nhu cầu project
5. Thêm hardware thật khi ready
