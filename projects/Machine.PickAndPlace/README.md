# Pick and Place Machine

Dự án Pick and Place được chia thành **2 chương trình độc lập**:

```
Machine.PickAndPlace/
├── PickAndPlace.Backend/       ← Chương trình xử lý LOGIC máy
│   └── Console Application
│
├── PickAndPlace.Frontend/      ← Chương trình GIAO DIỆN WPF
│   └── WPF Application
│
└── Machine.PickAndPlace.Core/  ← Thư viện dùng chung
    └── Class Library
```

---

## 1. PickAndPlace.Backend (Logic)

### Mô tả
Chương trình **Console Application** chạy logic điều khiển máy Pick and Place.

### Chức năng
- ✅ Khởi tạo hardware (Simulator)
- ✅ Tạo PickAndPlaceMachine instance
- ✅ Initialize (Home all axes)
- ✅ Chạy AUTO sequence
- ✅ Xử lý logic máy

### Build và Run

```bash
# Build
cd PickAndPlace.Backend
dotnet build

# Run
dotnet run
```

### Output
```
=== PickAndPlace Backend Started ===
Machine created: PickAndPlaceMachine
Initializing machine (Homing all axes)...
Machine initialized successfully
Starting AUTO mode...
Machine started - Running AUTO sequence
Press any key to stop...
```

---

## 2. PickAndPlace.Frontend (Giao diện)

### Mô tả
Chương trình **WPF Application** cung cấp giao diện điều khiển máy.

### Chức năng
- ✅ Auto View - Điều khiển AUTO mode (Start/Stop/Reset/Init)
- ✅ Manual View - Điều khiển Manual mode (JOG, Move, Home axes)
- ✅ Monitor View - Giám sát trạng thái máy
- ✅ Settings View - Cấu hình máy

### Build và Run

```bash
# Build
cd PickAndPlace.Frontend
dotnet build

# Run (hoặc double click .exe)
dotnet run
```

### Giao diện
- **Shell Header**: Tiêu đề, logo, thông tin máy
- **Auto View**: Start, Stop, Reset, Initialize, Cycle count
- **Manual View**: JOG axes, Move to position, Home, Vacuum control
- **Monitor View**: Real-time status, alarms
- **Settings View**: Machine parameters

---

## 3. Machine.PickAndPlace.Core (Dùng chung)

### Mô tả
Thư viện **Class Library** chứa logic core của máy Pick and Place.

### Nội dung
- ✅ `PickAndPlaceMachine.cs` - Machine logic, AUTO sequence
- ✅ `PickAndPlaceData.cs` - Machine data/settings
- ✅ `PickAndPlaceIOMap.cs` - IO mapping
- ✅ `Manual/ManualController.cs` - Manual control logic

### Build

```bash
cd Machine.PickAndPlace.Core
dotnet build
```

---

## 📦 Cấu Trúc Chi Tiết

### Backend (Logic)
```
PickAndPlace.Backend/
├── PickAndPlace.Backend.csproj
├── Program.cs                      ← Entry point
└── bin/Debug/net8.0/
    └── PickAndPlace.Backend.exe    ← Chương trình chạy
```

### Frontend (Giao diện)
```
PickAndPlace.Frontend/
├── PickAndPlace.Frontend.csproj
├── App.xaml / App.xaml.cs          ← WPF Application
├── MainWindow.xaml                 ← Main window
├── Views/
│   ├── AutoView.xaml               ← Auto mode UI
│   ├── ManualView.xaml             ← Manual mode UI
│   ├── MonitorView.xaml            ← Monitor UI
│   └── SettingsView.xaml           ← Settings UI
├── ViewModels/
│   └── MainViewModel.cs            ← Main ViewModel
└── bin/Debug/net8.0-windows/
    └── PickAndPlace.Frontend.exe   ← Chương trình WPF
```

### Core (Dùng chung)
```
Machine.PickAndPlace.Core/
├── Machine.PickAndPlace.Core.csproj
├── PickAndPlaceMachine.cs          ← Machine logic
├── PickAndPlaceData.cs             ← Data/Settings
├── PickAndPlaceIOMap.cs            ← IO Map
└── Manual/
    └── ManualController.cs         ← Manual control
```

---

## 🔧 Dependencies

### Backend depends on:
- Machine.PickAndPlace.Core ✅
- NAutoSuite.Core ✅
- NAutoSuite.Hardware.Simulator ✅
- Serilog ✅

### Frontend depends on:
- Machine.PickAndPlace.Core ✅
- NAutoSuite.Core ✅
- NAutoSuite.Hardware.Simulator ✅
- NAutoSuite.UI.Infrastructure ✅
- NAutoSuite.UI.Themes ✅
- NAutoSuite.UI.Controls ✅
- CommunityToolkit.Mvvm ✅

### Core depends on:
- NAutoSuite.Core ✅
- NAutoSuite.Hardware.Abstractions ✅

---

## 🚀 Cách Sử Dụng

### Scenario 1: Chạy Backend độc lập (Test logic)
```bash
cd PickAndPlace.Backend
dotnet run
```
→ Máy sẽ chạy AUTO sequence và log ra console

### Scenario 2: Chạy Frontend độc lập (Test UI)
```bash
cd PickAndPlace.Frontend
dotnet run
```
→ Giao diện WPF hiển thị, điều khiển máy qua UI

### Scenario 3: Chạy cả 2 (Production)
1. Chạy Backend trên máy điều khiển (có phần cứng)
2. Chạy Frontend trên máy khác (HMI, laptop)
3. Frontend kết nối Backend qua network (nếu cần)

---

## ✅ Build Status

```
✅ Machine.PickAndPlace.Core     - Build succeeded
✅ PickAndPlace.Backend           - Build succeeded
✅ PickAndPlace.Frontend          - Build succeeded
```

---

## 📝 Lưu Ý

1. **Backend và Frontend ĐỘC LẬP**
   - Mỗi chương trình có thể build/run riêng
   - Không phụ thuộc lẫn nhau

2. **Core là thư viện dùng chung**
   - Cả Backend và Frontend đều reference Core
   - Logic máy nằm trong Core

3. **Simulator**
   - Backend dùng SimulatorAxis, SimulatorOutput
   - Frontend cũng dùng Simulator (nếu chạy độc lập)
   - Thay Simulator bằng hardware thật khi deploy

4. **Logs**
   - Backend: `PickAndPlace.Backend/logs/`
   - Frontend: `PickAndPlace.Frontend/logs/`

---

## 🎯 Kết Luận

- **Backend**: Xử lý logic, chạy AUTO, manual control
- **Frontend**: Giao diện WPF, điều khiển và giám sát
- **Core**: Logic dùng chung

**Đúng theo yêu cầu: 2 chương trình độc lập trong 1 folder dự án!**
