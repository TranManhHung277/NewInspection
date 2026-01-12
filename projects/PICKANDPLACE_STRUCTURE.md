# Pick and Place Machine - Cấu Trúc Cuối Cùng

## 📁 Cấu Trúc (2 Projects Độc Lập)

```
NAutoSuite/projects/
├── Machine.PickAndPlace.Backend/   ← Chương trình 1: LOGIC + DATA + IO MAP
│   ├── Program.cs                  ← Entry point
│   ├── Machine/
│   │   ├── PickAndPlaceMachine.cs  ← Machine logic
│   │   ├── PickAndPlaceData.cs     ← Data/Settings
│   │   └── PickAndPlaceIOMap.cs    ← IO Mapping
│   ├── Manual/
│   │   └── ManualController.cs     ← Manual control
│   ├── Stations/
│   │   ├── PickStation.cs
│   │   └── PlaceStation.cs
│   └── PickAndPlace.Backend.csproj
│
└── Machine.PickAndPlace.Frontend/  ← Chương trình 2: GIAO DIỆN WPF
    ├── App.xaml / App.xaml.cs
    ├── MainWindow.xaml
    ├── Views/                      ← AutoView, ManualView, etc.
    ├── ViewModels/
    └── PickAndPlace.Frontend.csproj
```

**QUAN TRỌNG**:
- ✅ **KHÔNG có** Machine.PickAndPlace.Core
- ✅ Logic + IOMap + Data **TẤT CẢ** nằm trong Backend
- ✅ Frontend chỉ là UI thuần

---

## 1. Machine.PickAndPlace.Backend

### Mô tả
**Console Application** - Chứa TẤT CẢ logic điều khiển máy.

### Nội dung
```
Backend/
├── Program.cs                      ← Entry point, tạo machine, chạy AUTO
├── Machine/
│   ├── PickAndPlaceMachine.cs      ← Machine logic, AUTO sequence
│   ├── PickAndPlaceData.cs         ← Data/Settings (positions, speeds)
│   └── PickAndPlaceIOMap.cs        ← IO Mapping (inputs, outputs)
├── Manual/
│   └── ManualController.cs         ← Manual control logic
└── Stations/
    ├── PickStation.cs              ← Pick station logic
    └── PlaceStation.cs             ← Place station logic
```

### Namespace
```csharp
namespace PickAndPlace.Backend.Machine;
namespace PickAndPlace.Backend.Manual;
namespace PickAndPlace.Backend.Stations;
```

### Dependencies
```xml
<ProjectReference Include="..\..\src\Core\NAutoSuite.Core\NAutoSuite.Core.csproj" />
<ProjectReference Include="..\..\src\Hardware\NAutoSuite.Hardware.Simulator\NAutoSuite.Hardware.Simulator.csproj" />
<PackageReference Include="Serilog.Sinks.Console" Version="6.1.1" />
<PackageReference Include="Serilog.Sinks.File" Version="7.0.0" />
```

### Build và Run
```bash
cd Machine.PickAndPlace.Backend
dotnet build
dotnet run
```

### Output
```
=== PickAndPlace Backend Started ===
Machine created: PickAndPlaceMachine
Initializing machine (Homing all axes)...
Machine initialized successfully
Starting AUTO mode...
Press any key to stop...
```

---

## 2. Machine.PickAndPlace.Frontend

### Mô tả
**WPF Application** - Giao diện điều khiển (UI only).

### Nội dung
```
Frontend/
├── App.xaml / App.xaml.cs          ← WPF startup
├── MainWindow.xaml                 ← Main window
├── Views/
│   ├── AutoView.xaml               ← Auto mode UI
│   ├── ManualView.xaml             ← Manual mode UI
│   ├── MonitorView.xaml            ← Monitor UI
│   └── SettingsView.xaml           ← Settings UI
└── ViewModels/
    └── MainViewModel.cs            ← UI logic only
```

### Dependencies
```xml
<ProjectReference Include="..\..\src\Core\NAutoSuite.Core\NAutoSuite.Core.csproj" />
<ProjectReference Include="..\..\src\Hardware\NAutoSuite.Hardware.Simulator\NAutoSuite.Hardware.Simulator.csproj" />
<ProjectReference Include="..\..\src\UI\NAutoSuite.UI.Infrastructure\NAutoSuite.UI.Infrastructure.csproj" />
<ProjectReference Include="..\..\src\UI\NAutoSuite.UI.Themes\NAutoSuite.UI.Themes.csproj" />
<ProjectReference Include="..\..\src\UI\NAutoSuite.UI.Controls\NAutoSuite.UI.Controls.csproj" />
```

### Build và Run
```bash
cd Machine.PickAndPlace.Frontend
dotnet build
dotnet run
```

---

## 🎯 Phân Chia Trách Nhiệm

### Backend (Logic)
- ✅ PickAndPlaceMachine - AUTO sequence
- ✅ PickAndPlaceData - Positions, speeds, settings
- ✅ PickAndPlaceIOMap - IO addresses mapping
- ✅ ManualController - Manual operations
- ✅ PickStation, PlaceStation - Station logic
- ✅ Hardware initialization (Simulator)
- ✅ Chạy logic máy

### Frontend (UI)
- ✅ AutoView - UI Start/Stop/Reset
- ✅ ManualView - UI JOG/Move/Home
- ✅ MonitorView - UI hiển thị status
- ✅ SettingsView - UI cấu hình
- ✅ KHÔNG có machine logic
- ✅ Chỉ hiển thị giao diện

---

## 📝 Khi Tạo Dự Án Mới

### Bước 1: Tạo Backend
```bash
cd projects
dotnet new console -n YourMachine.Backend
```

### Bước 2: Viết Logic trong Backend
```
YourMachine.Backend/
├── Program.cs                  ← Entry point
├── Machine/
│   ├── YourMachine.cs          ← Machine logic
│   ├── YourMachineData.cs      ← Data
│   └── YourMachineIOMap.cs     ← IO Map
└── Manual/
    └── ManualController.cs
```

### Bước 3: Tạo Frontend (Optional)
```bash
cd projects
dotnet new wpf -n YourMachine.Frontend
```

---

## ✅ Build Status

```
✅ Machine.PickAndPlace.Backend  - Build succeeded (chứa TẤT CẢ logic)
✅ Machine.PickAndPlace.Frontend - Build succeeded (UI only)
```

---

## 🔧 Ưu Điểm Cấu Trúc Này

1. **Đơn giản**
   - Chỉ 2 projects
   - Backend có tất cả logic
   - Frontend chỉ UI

2. **Rõ ràng**
   - Logic ở Backend
   - UI ở Frontend
   - Không lồng nhau

3. **Dễ maintain**
   - Thay đổi logic → sửa Backend
   - Thay đổi UI → sửa Frontend
   - Độc lập hoàn toàn

4. **Dễ deploy**
   - Backend chạy trên máy điều khiển
   - Frontend có thể ở máy khác
   - Hoặc chạy chung 1 máy

---

## 🎯 Kết Luận

- ✅ **Backend**: TẤT CẢ logic + IOMap + Data trong 1 project
- ✅ **Frontend**: Chỉ WPF UI
- ✅ **KHÔNG có Core**: Đã gộp hết vào Backend
- ✅ 2 projects độc lập, không lồng nhau
