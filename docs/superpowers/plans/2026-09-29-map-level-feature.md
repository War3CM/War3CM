# Kế hoạch Triển khai Tính năng Tăng Map Level & Rank 1

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bổ sung tính năng đặt Map Level (1-100) và nút chuyển đổi Rank 1 độc lập trên thẻ MAP & SAVE PROFILES, đồng bộ chuẩn DzAPI & KKAPI mà vẫn bảo toàn 100% dữ liệu file save cũ.

**Architecture:** Mở rộng `MapSaveManager` với các phương thức an toàn `ReadMapLevel`, `WriteMapLevel`, `ReadMapLevelRank`, `WriteMapLevelRank`. Cập nhật `MainForm` để chia lại lưới `slotActions` thành 3 hàng đều nhau (thu nhỏ 2 hàng cũ, hàng 3 chứa ô nhập Level 60px + nút Đặt Lvl ở cột trái, nút 👑 Rank 1 ở cột phải). Bổ sung từ điển đa ngôn ngữ trong `lang.ini`.

**Tech Stack:** C# .NET 8 WinForms, Git, Windows INI / regex stream parser.

## Global Constraints
- Cửa sổ ứng dụng cố định kích thước 820x648 (không làm vỡ layout, không phát sinh thanh cuộn dọc không mong muốn).
- Giới hạn Map Level: Số nguyên từ 1 đến 100 (mặc định 100 khi chưa có).
- Khóa INI cần đồng bộ:
  - Chuẩn DzAPI: `DzAPI_Map_GetMapLevel`, `DzAPI_Map_GetMapLevelRank`.
  - Chuẩn KK Platform (KKAPI): `MLS-MsGetPlayerMapLevel-0`, `MLS-MsGetPlayerMapLevel-1`, `MLS-MsGetPlayerMapLevel-2`, `MLS-MsGetPlayerMapLevel-3`.
- Section INI: `[DzAPI]`.
- Phải bảo toàn nguyên vẹn tất cả dữ liệu cũ trong file INI (các key `SSV-0-...`, token, daytime, trang bị...).

---

### Task 1: Bổ sung Từ điển Đa ngôn ngữ (VN, EN, CN) trong `lang.ini`

**Files:**
- Modify: `Source/lang.ini:70-80, 220-230, 370-380` (và root `lang.ini`)

**Interfaces:**
- Consumes: ConfigManager `T(key)`
- Produces: Các khóa ngôn ngữ `setMapLevel`, `tipSetLevel`, `btnRank1`, `tipRank1`, `invalidLevel`, `setLevelSuccess`, `rank1Enabled`, `rank1Disabled`.

- [ ] **Step 1: Cập nhật file `Source/lang.ini` và `lang.ini` ở thư mục gốc**
Thêm các bản dịch:
```ini
# VN
setMapLevel = Đặt Lvl
tipSetLevel = Đặt cấp độ Map Level (1-100) cho hồ sơ save đang chọn
btnRank1 = 👑 Rank 1
tipRank1 = Bật/Tắt thứ hạng Rank 1 (Top 1 BXH) để mở danh hiệu và quyền lợi ẩn
invalidLevel = Vui lòng nhập số Level hợp lệ từ 1 đến 100!
setLevelSuccess = ✔ Đã đặt Map Level: {0} cho hồ sơ save!
rank1Enabled = ✔ Đã kích hoạt Rank 1 (Top 1) cho hồ sơ save!
rank1Disabled = Đã tắt Rank 1.

# EN
setMapLevel = Set Lvl
tipSetLevel = Set Map Level (1-100) for the selected save profile
btnRank1 = 👑 Rank 1
tipRank1 = Toggle Rank 1 (Top 1 leaderboard) to unlock titles and perks
invalidLevel = Please enter a valid Map Level between 1 and 100!
setLevelSuccess = ✔ Set Map Level to {0} for save profile!
rank1Enabled = ✔ Activated Rank 1 (Top 1) for save profile!
rank1Disabled = Rank 1 deactivated.

# CN
setMapLevel = 设置等级
tipSetLevel = 为所选存档设置地图等级 (1-100)
btnRank1 = 👑 Rank 1
tipRank1 = 开启/关闭第一名排行 (Rank 1)，解锁专属称号与特权
invalidLevel = 请输入 1 到 100 之间的有效地图等级！
setLevelSuccess = ✔ 已将存档的地图等级设置为 {0}！
rank1Enabled = ✔ 已为存档激活第一名排行！
rank1Disabled = 已取消第一名排行。
```

- [ ] **Step 2: Kiểm tra test ConfigManager hiện tại**
Run: `dotnet test Source/tests/WpmUnitTests/WpmUnitTests.csproj` hoặc chạy test suite để đảm bảo `lang.ini` tải hợp lệ.
Expected: PASS

- [ ] **Step 3: Commit**
```bash
git add Source/lang.ini lang.ini
git commit -m "feat(lang): add localization keys for map level and rank 1"
```

---

### Task 2: Triển khai Logic Đọc/Ghi Map Level & Rank 1 trong `MapSaveManager.cs`

**Files:**
- Modify: `Source/Core/MapSaveManager.cs`
- Test: `Source/tests/WpmUnitTests/Program.cs`

**Interfaces:**
- Produces:
  - `public static int ReadMapLevel(string iniPath, int defaultLevel = 100)`
  - `public static bool WriteMapLevel(string iniPath, int level)`
  - `public static bool ReadMapLevelRank(string iniPath)`
  - `public static bool WriteMapLevelRank(string iniPath, bool isRank1)`

- [ ] **Step 1: Viết Unit Test kiểm thử đọc/ghi Level và Rank 1**
Trong `Source/tests/WpmUnitTests/Program.cs`, thêm test case:
- Test 1: Đọc/ghi trên file trống -> tự động tạo section `[DzAPI]` và ghi đủ cả DzAPI + KKAPI.
- Test 2: Ghi đè trên file có sẵn dữ liệu `SSV-0-DAYTIME=12345` -> bảo toàn nguyên vẹn `SSV-0-DAYTIME`, chỉ thêm/sửa key level.
- Test 3: Bật/tắt Rank 1 -> `DzAPI_Map_GetMapLevelRank` chuyển đổi chính xác giữa `1` và `0`.
- Test 4: Ràng buộc số level (1-100).

- [ ] **Step 2: Chạy test để xác nhận test thất bại (Red phase)**
Run: `dotnet run --project Source/tests/WpmUnitTests/WpmUnitTests.csproj`
Expected: Compile error do chưa có các hàm trong `MapSaveManager`.

- [ ] **Step 3: Triển khai code các hàm trong `MapSaveManager.cs`**
Triển khai thuật toán đọc/ghi an toàn bằng dòng:
- Tìm vị trí `[DzAPI]`.
- Cập nhật hoặc bổ sung các khóa:
  - `DzAPI_Map_GetMapLevel=<level>`
  - `MLS-MsGetPlayerMapLevel-0=<level>`
  - `MLS-MsGetPlayerMapLevel-1=<level>`
  - `MLS-MsGetPlayerMapLevel-2=<level>`
  - `MLS-MsGetPlayerMapLevel-3=<level>`
- Quản lý `DzAPI_Map_GetMapLevelRank=1` hoặc `=0`.
- Sử dụng ghi tạm file `.tmp` rồi replace để chống corrupt file.

- [ ] **Step 4: Chạy test để xác nhận test pass (Green phase)**
Run: `dotnet run --project Source/tests/WpmUnitTests/WpmUnitTests.csproj`
Expected: PASS toàn bộ các test case.

- [ ] **Step 5: Commit**
```bash
git add Source/Core/MapSaveManager.cs Source/tests/WpmUnitTests/Program.cs
git commit -m "feat(core): implement Map Level and Rank 1 read/write logic with safety preservation"
```

---

### Task 3: Tích hợp Giao diện Hàng 3 & Sự kiện trong `MainForm.cs`

**Files:**
- Modify: `Source/Forms/MainForm.cs`

**Interfaces:**
- Consumes: `MapSaveManager.ReadMapLevel`, `MapSaveManager.WriteMapLevel`, `MapSaveManager.ReadMapLevelRank`, `MapSaveManager.WriteMapLevelRank`, `ConfigManager.GetText`

- [ ] **Step 1: Khai báo Controls và cập nhật Lưới `slotActions`**
- Thêm trường:
  ```csharp
  private ModernTextBox txtMapLevel;
  private ModernButton btnSetLevel;
  private ModernButton btnRank1;
  private bool _isRank1Active;
  ```
- Cập nhật `slotActions`:
  - `RowCount = 3`
  - 3 dòng đều nhau: `RowStyles.Add(new RowStyle(SizeType.Percent, 33.33f))` x 3.
  - Hàng 0: `btnNewSlot`, `btnBackupSlot`
  - Hàng 1: `btnDeleteSlot`, `btnRestoreSlot`
  - Hàng 2:
    - Cột 0: Panel `pnlLevelGroup` chia làm 2 cột: Cột 0 rộng `60px` chứa `txtMapLevel`, Cột 1 co dãn chứa `btnSetLevel`.
    - Cột 1: `btnRank1` rộng 100% cột.

- [ ] **Step 2: Xử lý sự kiện `cboSlots.SelectedIndexChanged` và `RefreshSlots()`**
- Khi chọn 1 slot:
  - Đọc level từ slot file gán vào `txtMapLevel.TextContent`.
  - Đọc rank1 từ slot file gán vào `_isRank1Active`, cập nhật màu viền/nền của `btnRank1`.
  - Enable các controls `txtMapLevel`, `btnSetLevel`, `btnRank1`.
- Khi danh sách trống:
  - Disable các controls trên.

- [ ] **Step 3: Xử lý sự kiện bấm `btnSetLevel`**
- Đọc text từ `txtMapLevel`. Kiểm tra `int.TryParse` và kẹp giá trị `Math.Clamp(val, 1, 100)`.
- Ghi vào file slot của profile đang chọn.
- Nếu thư mục game có file `dz_w3_plugin.ini` (hoặc game đang chạy), đồng bộ luôn sang `dz_w3_plugin.ini`.
- Cập nhật thông báo trạng thái `statusLabel.Text = string.Format(T("setLevelSuccess"), val)`.

- [ ] **Step 4: Xử lý sự kiện bấm `btnRank1`**
- Đảo trạng thái: `_isRank1Active = !_isRank1Active`.
- Ghi vào file slot và file game.
- Cập nhật màu viền nút `btnRank1.BorderColor = _isRank1Active ? Color.FromArgb(240, 180, 40) : Color.FromArgb(50, 110, 180)`.
- Cập nhật trạng thái `statusLabel`.

- [ ] **Step 5: Xử lý Đa ngôn ngữ và Tooltips trong `ApplyLanguage()` & `UpdateActionTips()`**
- Gán tooltip giải thích tác dụng cho `txtMapLevel`, `btnSetLevel`, `btnRank1`.

- [ ] **Step 6: Kiểm tra biên dịch & Chạy toàn bộ Unit Tests**
Run: `dotnet test Source/tests/WpmUnitTests/WpmUnitTests.csproj`
Expected: PASS

- [ ] **Step 7: Commit**
```bash
git add Source/Forms/MainForm.cs
git commit -m "feat(ui): integrate Map Level input and Rank 1 toggle button into Map & Save Profiles card"
```

---

### Task 4: Kiểm thử Toàn diện & Xác thực Trực quan (Verification)

**Files:**
- Test: `Source/tests/WpmUnitTests/Program.cs`
- Build: `Source/Phanmemwar3.csproj`

- [ ] **Step 1: Chạy toàn bộ Unit Test suite**
Run: `dotnet run --project Source/tests/WpmUnitTests/WpmUnitTests.csproj`
Expected: Toàn bộ assertions đều PASS, không có lỗi ngoại lệ.

- [ ] **Step 2: Build Release và kiểm tra file thực thi**
Run: `dotnet build Source/Phanmemwar3.csproj -c Release`
Expected: Build thành công 0 warning/error.

- [ ] **Step 3: Commit hoàn thiện**
```bash
git add .
git commit -m "chore(release): verify and build final executable with map level & rank 1 feature"
```
