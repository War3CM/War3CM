# Thiết kế Tính năng Tăng Map Level & Rank 1 (Map & Save Profiles)

- **Ngày tạo:** 2026-09-29
- **Dự án:** Warcraft Platform Manager & Launcher (`d:\work\phanmemwar`)
- **Trạng thái:** Đã được người dùng duyệt thiết kế

---

## 1. Mục tiêu & Bối cảnh
Người chơi Warcraft 3 RPG thường cần đạt cấp độ Map Level cao (tối đa 100) để mở khóa các tính năng, kỹ năng, tướng hoặc nhận bổng lộc trong map RPG mà không phải cày lại từ đầu.
Ngoài ra, các map RPG hiện đại (như map `D35C`, `6BBCAA`, v.v.) còn kiểm tra thêm chỉ số thứ hạng trên bảng xếp hạng `MapLevelRank` (Rank 1 = Top 1) để mở khóa danh hiệu VIP, khung avatar độc quyền hoặc buff chỉ số ẩn.

Tính năng này cung cấp giao diện trực quan ngay trên thẻ **MAP & SAVE PROFILES** của phần mềm để:
1. Cho phép nhập số Map Level từ 1 đến 100 và bấm nút áp dụng vào Save Profile đang chọn.
2. Cung cấp nút chuyển đổi (Toggle) **👑 Rank 1** độc lập bên cạnh ô Map Level.
3. Đồng bộ hoàn hảo cho cả chuẩn **DzAPI** (`DzAPI_Map_GetMapLevel`, `DzAPI_Map_GetMapLevelRank`) và chuẩn **KK Platform / KKAPI** (`MLS-MsGetPlayerMapLevel-0..3`).

---

## 2. Nghiên cứu Kỹ thuật & Bằng chứng Dữ liệu (Dịch ngược DLL & Map)

Qua dịch ngược file DLL hệ thống của game (`dz_w3_plugin.dll`, `kkapi_local_plugin.dll`) và script Lua/JASS trong map:
* `DzAPI_Map_GetMapLevel(player)`:
  Gọi `GetPrivateProfileIntA("DzAPI", "DzAPI_Map_GetMapLevel", 0, "dz_w3_plugin.ini")`.
* `DzAPI_Map_GetMapLevelRank(player)`:
  Gọi `GetPrivateProfileIntA("DzAPI", "DzAPI_Map_GetMapLevelRank", 0, "dz_w3_plugin.ini")`.
* `MsGetPlayerMapLevel(player)` (KK Platform):
  Gọi `GetPrivateProfileIntA("DzAPI", "MLS-MsGetPlayerMapLevel-0", 0, "kkapi_local_plugin.ini" / "dz_w3_plugin.ini")`.
* Quá trình nạp map của phần mềm:
  `MapSaveManager.Prepare()` sao chép file save slot (`Saves/<MapHash>/<SlotName>.ini`) thành `dz_w3_plugin.ini` trong thư mục game Warcraft 3 khi chơi.
  Do đó, việc ghi các khóa này trực tiếp vào file Save Profile đảm bảo map nhận đúng 100% khi vào game.

---

## 3. Thiết kế Giao diện (UI Layout)

### 3.1. Bố cục `slotActions` (Thẻ MAP & SAVE PROFILES)
Khu vực nút điều khiển `slotActions` được điều chỉnh từ lưới 2 hàng thành **3 hàng** đều nhau (mỗi hàng chiếm ~33.3% chiều cao, giảm chiều cao của 2 hàng cũ để vừa vặn với kích thước cửa sổ cố định 820x648):

* **Hàng 1**:
  * Cột 0: `btnNewSlot` (`+ New` / `+ Mới`)
  * Cột 1: `btnBackupSlot` (`Backup` / `Sao lưu`)
* **Hàng 2**:
  * Cột 0: `btnDeleteSlot` (`Delete` / `Xóa`)
  * Cột 1: `btnRestoreSlot` (`Restore` / `Khôi phục`)
* **Hàng 3 (Mới)**:
  * Cột 0 (Dưới nút `Delete`): Nhóm thiết lập Level gồm:
    * `txtMapLevel` (`ModernTextBox`): Rộng cố định `60px`, `TextAlign = Center`, chỉ cho phép nhập ký tự số từ `1` đến `100`.
    * `btnSetLevel` (`ModernButton`): Chiếm phần còn lại của cột (~115px), màu xanh dương (`Color.FromArgb(28, 52, 82)`), nhãn `Đặt Lvl` (VN) / `Set Lvl` (EN) / `设置等级` (CN).
  * Cột 1 (Dưới nút `Restore`):
    * `btnRank1` (`ModernButton`): Rộng 100% cột (~180px), nhãn `👑 Rank 1`. Có hiệu ứng Toggle (sáng viền vàng kim `Color.FromArgb(240, 180, 40)` khi Rank 1 đang bật, viền mặc định khi tắt).

---

## 4. Thiết kế Logic & Xử lý Dữ liệu

### 4.1. Đọc dữ liệu tự động (Khi chọn Slot)
* Sự kiện `cboSlots.SelectedIndexChanged`:
  * Đọc file `.ini` của slot đang chọn.
  * Trích xuất giá trị `DzAPI_Map_GetMapLevel` (hoặc `MLS-MsGetPlayerMapLevel-0`). Nếu có, hiển thị vào `txtMapLevel`. Nếu slot chưa có level, hiển thị mặc định `100`.
  * Trích xuất `DzAPI_Map_GetMapLevelRank`. Nếu giá trị bằng `1`, cập nhật giao diện `btnRank1` thành trạng thái bật (Active); nếu không thì trạng thái tắt (Inactive).
  * Nếu không có slot nào được chọn: Vô hiệu hóa (disable) `txtMapLevel`, `btnSetLevel`, `btnRank1`.

### 4.2. Ghi dữ liệu Map Level (`btnSetLevel_Click`)
* Kiểm tra giá trị nhập: Chuyển đổi chuỗi sang số nguyên. Ràng buộc: `1 <= level <= 100`.
* Nếu người dùng nhập sai (chữ, số < 1 hoặc > 100), tự động điều chỉnh về cận hợp lệ và thông báo trên thanh trạng thái.
* Cập nhật file `.ini` của slot:
  * Quét các dòng trong file, định vị section `[DzAPI]` (tự tạo nếu chưa có).
  * Ghi hoặc cập nhật các dòng:
    ```ini
    [DzAPI]
    DzAPI_Map_GetMapLevel=<level>
    MLS-MsGetPlayerMapLevel-0=<level>
    MLS-MsGetPlayerMapLevel-1=<level>
    MLS-MsGetPlayerMapLevel-2=<level>
    MLS-MsGetPlayerMapLevel-3=<level>
    ```
  * Bảo toàn 100% tất cả các dòng dữ liệu khác (như `SSV-0-...`, token, daytime, tài nguyên).
* Nếu game Warcraft 3 đang mở hoặc file `<war3Dir>\dz_w3_plugin.ini` tồn tại, đồng bộ ngay lập tức sang file đó.
* Cập nhật thông báo trạng thái: `✔ Đã đặt Map Level: {0} cho hồ sơ!`.

### 4.3. Bật/Tắt Rank 1 (`btnRank1_Click`)
* Kiểm tra trạng thái hiện tại:
  * Nếu đang Tắt: Ghi `DzAPI_Map_GetMapLevelRank = 1` vào file slot (và `dz_w3_plugin.ini` nếu có), chuyển viền nút sang vàng sáng, báo `✔ Đã kích hoạt Rank 1 (Top 1) cho hồ sơ!`.
  * Nếu đang Bật: Ghi `DzAPI_Map_GetMapLevelRank = 0` (hoặc xóa khóa), chuyển viền nút về bình thường, báo `Đã tắt Rank 1.`.

---

## 5. Đa ngôn ngữ (Localization trong `lang.ini`)
Bổ sung các khóa sau cho 3 ngôn ngữ `VN`, `EN`, `CN`:
* `setMapLevel`: `Đặt Lvl` / `Set Lvl` / `设置等级`
* `tipSetLevel`: `Cài đặt cấp độ Map Level (1-100) cho hồ sơ đang chọn` / `Set Map Level (1-100) for selected profile` / `设置当前存档的地图等级 (1-100)`
* `btnRank1`: `👑 Rank 1`
* `tipRank1`: `Bật/Tắt thứ hạng Rank 1 (Top 1 BXH) để mở danh hiệu và quà cấp bậc` / `Toggle Rank 1 (Top 1) to unlock exclusive titles and perks` / `切换第一名排行 (Rank 1)`
* `invalidLevel`: `Vui lòng nhập Level hợp lệ từ 1 đến 100!` / `Please enter a valid level between 1 and 100!` / `请输入 1 到 100 之间的有效等级！`
* `setLevelSuccess`: `✔ Đã đặt Map Level: {0} cho hồ sơ save!` / `✔ Set Map Level to {0} for save profile!` / `✔ 已将存档的地图等级设置为 {0}！`
* `rank1Enabled`: `✔ Đã kích hoạt Rank 1 cho hồ sơ save!` / `✔ Activated Rank 1 for save profile!` / `✔ 已为存档激活第一名排行！`
* `rank1Disabled`: `Đã hủy kích hoạt Rank 1.` / `Deactivated Rank 1.` / `已取消第一名排行。`

---

## 6. Kế hoạch Kiểm thử & Xác minh (Verification)
1. **Unit Test (WpmUnitTests)**:
   * Test hàm đọc/ghi Map Level vào file INI rỗng và file INI đã có dữ liệu game phức tạp (`SSV-0-...`).
   * Test tính năng bảo toàn dữ liệu (không làm mất các khóa khác trong `[DzAPI]`).
   * Test kiểm tra tính hợp lệ của số Level (chặn < 1, chặn > 100, ép kiểu chính xác).
   * Test hàm bật/tắt Rank 1 (`DzAPI_Map_GetMapLevelRank`).
2. **UI & Layout Test**:
   * Kiểm tra giao diện ở 3 chế độ ngôn ngữ (VN, EN, CN) xem nút có bị tràn chữ hay lệch hàng không.
   * Kiểm tra kích thước cửa sổ cố định 820x648 đảm bảo không phát sinh thanh cuộn ngoài ý muốn.
