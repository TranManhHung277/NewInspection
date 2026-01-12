# Pick and Place Machine - Cấu Trúc Dự Án

## 📁 Cấu Trúc (Projects KHÔNG lồng nhau)

```
NAutoSuite/projects/
├── Machine.PickAndPlace.Core/      ← Thư viện dùng chung
│   └── Machine.PickAndPlace.Core.csproj
│
├── Machine.PickAndPlace.Backend/   ← Chương trình 1: Logic
│   └── PickAndPlace.Backend.csproj
│
└── Machine.PickAndPlace.Frontend/  ← Chương trình 2: Giao diện
    └── PickAndPlace.Frontend.csproj
```

**QUAN TRỌNG**: Các project *.csproj **KHÔNG** lồng vào nhau, mà nằm **ngang hàng** trong folder `projects/`.

---

## 1. Machine.PickAndPlace.Core

### Mô tả
**Class Library** chứa logic core của máy.

### Nội dung
- `PickAndPlaceMachine.cs` - Machine logic, AUTO sequence
- `PickAndPlaceData.cs` - Machine data/settings
- `PickAndPlaceIOMap.cs` - IO mapping
- `Manual/ManualController.cs` - Manual control

### Build
```bash
cd Machine.PickAndPlace.Core
dotnet build
```

---

## 2. Machine.PickAndPlace.Backend

### Mô tả
**Console Application** - Chương trình xử lý LOGIC máy.

### Chức năng
- Khởi tạo hardware (Simulator)
- Tạo PickAndPlaceMachine instance
- Initialize (Home all axes)
- Chạy AUTO sequence

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

## 3. Machine.PickAndPlace.Frontend

### Mô tả
**WPF Application** - Chương trình GIAO DIỆN điều khiển máy.

### Chức năng
- Auto View - Start/Stop/Reset/Init
- Manual View - JOG/Move/Home axes
- Monitor View - Giám sát
- Settings View - Cấu hình

### Build và Run
```bash
cd Machine.PickAndPlace.Frontend
dotnet build
dotnet run
```

---

## 🔧 Project References

### Machine.PickAndPlace.Core depends on:
```xml
<ProjectReference Include="..\..\src\Core\NAutoSuite.Core\NAutoSuite.Core.csproj" />
```

### Machine.PickAndPlace.Backend depends on:
```xml
<ProjectReference Include="..\Machine.PickAndPlace.Core\Machine.PickAndPlace.Core.csproj" />
<ProjectReference Include="..\..\src\Core\NAutoSuite.Core\NAutoSuite.Core.csproj" />
<ProjectReference Include="..\..\src\Hardware\NAutoSuite.Hardware.Simulator\NAutoSuite.Hardware.Simulator.csproj" />
```

### Machine.PickAndPlace.Frontend depends on:
```xml
<ProjectReference Include="..\Machine.PickAndPlace.Core\Machine.PickAndPlace.Core.csproj" />
<ProjectReference Include="..\..\src\Core\NAutoSuite.Core\NAutoSuite.Core.csproj" />
<ProjectReference Include="..\..\src\Hardware\NAutoSuite.Hardware.Simulator\NAutoSuite.Hardware.Simulator.csproj" />
<ProjectReference Include="..\..\src\UI\NAutoSuite.UI.Infrastructure\NAutoSuite.UI.Infrastructure.csproj" />
<ProjectReference Include="..\..\src\UI\NAutoSuite.UI.Themes\NAutoSuite.UI.Themes.csproj" />
<ProjectReference Include="..\..\src\UI\NAutoSuite.UI.Controls\NAutoSuite.UI.Controls.csproj" />
```

---

## ✅ Build Status

```
✅ Machine.PickAndPlace.Core     - Build succeeded
✅ Machine.PickAndPlace.Backend  - Build succeeded
✅ Machine.PickAndPlace.Frontend - Build succeeded
```

---

## 🚀 Cách Sử Dụng

### Build tất cả
```bash
cd D:\3. Program\C#\NAutoSuite\projects

dotnet build Machine.PickAndPlace.Core\Machine.PickAndPlace.Core.csproj
dotnet build Machine.PickAndPlace.Backend\PickAndPlace.Backend.csproj
dotnet build Machine.PickAndPlace.Frontend\PickAndPlace.Frontend.csproj
```

### Run Backend (Logic)
```bash
cd Machine.PickAndPlace.Backend
dotnet run
```

### Run Frontend (Giao diện)
```bash
cd Machine.PickAndPlace.Frontend
dotnet run
```

---

## 📝 Lưu Ý

1. **Projects ngang hàng**
   - Các project *.csproj **KHÔNG** lồng vào nhau
   - Tất cả nằm cùng cấp trong folder `projects/`

2. **References đúng**
   - Core: `..\..\src\...` (lên 2 cấp)
   - Backend/Frontend: `..\Machine.PickAndPlace.Core\` (cùng cấp)

3. **Độc lập**
   - Backend và Frontend build/run độc lập
   - Core là thư viện dùng chung

4. **Output**
   - Core: `bin/Debug/net8.0/Machine.PickAndPlace.Core.dll`
   - Backend: `bin/Debug/net8.0/PickAndPlace.Backend.exe`
   - Frontend: `bin/Debug/net8.0-windows/PickAndPlace.Frontend.exe`

---

## 🎯 Kết Luận

**Đúng theo yêu cầu:**
- ✅ 3 projects KHÔNG lồng nhau
- ✅ Nằm ngang hàng trong folder `projects/`
- ✅ Backend = Logic (Console App)
- ✅ Frontend = Giao diện (WPF App)
- ✅ Core = Thư viện dùng chung
- ✅ Tất cả build thành công!
