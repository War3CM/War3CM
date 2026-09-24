# Warcraft Platform Manager & Launcher

Mã nguồn WinForms .NET 8 Windows. Mở `Phanmemwar3.csproj` trên Windows với .NET 8 SDK và chạy `dotnet build -c Release`. Gói này gồm toàn bộ mã nguồn, tài nguyên, `lang.ini` và cấu hình mẫu; không bao gồm Warcraft III, map hoặc DLL plugin của bên thứ ba.

## Sử dụng

1. Chọn thư mục Warcraft III chứa `war3.exe` (bản game tương thích với plugin đang dùng).
2. Chọn plugin và map từ `Maps/` (kể cả thư mục con), hoặc duyệt/kéo thả `.w3x`/`.w3m`.
3. Bấm **+ Mới** để tạo một save trống, đặt tên, hoặc chọn slot đã có. Bấm **Backup** để tạo bản sao của slot hiện tại.
4. Bấm **Khởi động game**. Launcher chép slot ra `<War3>/dz_w3_plugin.ini`, tạo một bản map với đường dẫn ngắn dưới `Maps/WPM/`, rồi chạy YDWE loader bằng `-launchwar3 -loadfile` đối với plugin hoặc chạy `war3.exe -loadfile` đối với profile Clean. Giữ launcher mở đến khi thoát game; khi tiến trình kết thúc, nó đồng bộ INI về slot và giữ bản cũ ở `<slot>.ini.previous`.

Mỗi map dùng thư mục `Saves/<TênMap>_<mã đường dẫn>/` để tránh trùng tên map. File INI trước khi chạy được lưu trong `Saves/_PreviousRoot/` (tên UUID). Thư mục Saves cần quyền ghi và nên nằm ngoài `Program Files`. Không chạy đồng thời nhiều phiên War3 cùng thư mục game vì plugin dùng chung một INI; launcher từ chối chạy nếu phát hiện tiến trình `war3` khác.

Các nhãn trong giao diện nằm ở hai mục `[VN]` và `[EN]` của `lang.ini`. Cài đặt ngôn ngữ ở **Cài đặt nâng cao**; các nhãn chính được cập nhật sau khi lưu. Một số thông báo lỗi và màn hình cập nhật kế thừa source cũ chưa được chuyển hết thành khóa ngôn ngữ.

## Giới hạn cần kiểm tra trên máy có game

Cơ chế `-loadfile` và việc plugin ghi `dz_w3_plugin.ini` cần thử với đúng bản game, map, DLL và cấu hình YDWE đang dùng. Khi dùng plugin, launcher chạy YDWE loader rồi theo dõi tiến trình `war3.exe` thực tế. Nếu không thấy tiến trình game trong 20 giây, launcher báo lỗi và khôi phục INI trước đó. Mã YDWE xử lý đường dẫn map ngắn hơn 54 ký tự, nhưng vẫn phải thử thực tế trên đúng bản game và bản loader của bạn. Nếu game hoặc launcher bị tắt cưỡng bức, hãy đối chiếu INI tại thư mục game và bản sao trong `Saves/_PreviousRoot` trước khi chạy lại. Save trắng là file INI rỗng; một số plugin có thể cần cấu trúc INI mặc định riêng.


## Bản giao diện và quản lý Save mới

Banner được vẽ bằng chữ tiếng Anh, không dùng ảnh nhúng chữ tiếng Việt. EN/VN/CN chỉ thay đổi các điều khiển. Có nút quét lại plugin, xóa mềm slot vào `Saves/<Map>/_Trash`, khôi phục slot đã xóa, và giữ bản trước mỗi lần đồng bộ trong `_History`. Sau khi đồng bộ thành công, file INI gốc của game được trả về trạng thái trước lúc mở map. Bản map gốc không bị đổi tên; khi chạy chỉ tạo bản sao tạm tên ngắn dưới `Maps/WPM` và không ghi đè file trùng tên. Cần thử trực tiếp với Warcraft III và YDWE thực tế trên Windows; tham khảo `README_UI.md` và bài kiểm tra độc lập trong `tests/MapSaveManagerSmoke`.
