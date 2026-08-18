# 3 prompt bóc tách toàn dự án (ASPX API + Angular → Blazor Server)

Dùng với AI có quyền đọc toàn bộ repo (Claude Code, Cursor, agent tương tự).
Chạy tuần tự 1 → 2 → 3. Mỗi prompt ghi kết quả ra file trong `/docs/migration/`, prompt sau đọc lại file của prompt trước.

---

## PROMPT 1 — Bóc toàn bộ tầng server

> Bạn đang khảo sát một dự án legacy để chuẩn bị migrate sang Blazor Server. Backend là ASP.NET (.NET Framework), các file `.aspx` + code-behind `.cs` đang đóng vai trò API endpoint cho một SPA Angular.
>
> **Nhiệm vụ:** quét toàn bộ source backend và lập bản đồ API đầy đủ.
>
> **Cách làm:**
> 1. Tìm mọi file đóng vai trò endpoint: `.aspx.cs`, `.ashx`, `WebMethod`, ApiController, HttpHandler. Liệt kê danh sách file tìm được **trước khi** bắt đầu phân tích, và báo cho tôi số lượng.
> 2. Xử lý theo lô, mỗi lô một file. Sau mỗi file, ghi kết quả nối tiếp vào `/docs/migration/01-api-map.md`. Không giữ hết trong đầu rồi mới xuất một lần.
> 3. Với mỗi endpoint/action, xuất một dòng trong bảng markdown:
>
> | # | File:dòng | Action | Method | Tham số vào (tên - kiểu - bắt buộc) | Trả về (cấu trúc) | Bảng DB đụng tới | Loại (R/C/U/D/Mixed) | Nghiệp vụ (1 câu) | Phức tạp 1-5 |
>
> **Quy tắc bắt buộc:**
> - Chỉ ghi những gì **có thật trong code**. Không suy đoán, không thêm tham số bạn nghĩ "nên có". Chỗ nào không rõ thì ghi `??? cần xác nhận`.
> - Giữ nguyên tên action đúng như code, kể cả viết tắt hay sai chính tả.
> - Luôn kèm `File:dòng` để người đọc truy ngược được.
>
> **Xuất thêm 3 mục ở cuối file:**
> - **Business rule ẩn** — liệt kê mọi giá trị hardcode (mã khách hàng, mã chi nhánh, ngày cố định), magic number, điều kiện `if` bất thường, code bị comment out, comment mô tả ngoại lệ. Với mỗi mục đánh giá: `Nghiệp vụ rõ ràng` / `Workaround cho case đặc biệt` / `Nghi là bug nhưng có thể đang được phụ thuộc`. **Không được "dọn sạch" hay bỏ qua những chỗ trông vô lý** — đó thường là nghiệp vụ thật.
> - **Rào cản kỹ thuật khi lên .NET 8** — đặc biệt: đang dùng Entity Framework 6 hay ADO.NET/Dapper; chỗ phụ thuộc `System.Web` (HttpContext, Session, Server.MapPath); thư viện NuGet chỉ có bản .NET Framework; code nối chuỗi SQL.
> - **Ứng viên bóc thành service dùng chung** — logic lặp lại ở nhiều endpoint.

---

## PROMPT 2 — Bóc toàn bộ màn hình và lập kế hoạch chia việc

> Đọc `/docs/migration/01-api-map.md` để có bản đồ API. Bây giờ quét toàn bộ source Angular.
>
> **Nhiệm vụ:** lập bản đồ màn hình và ghép với bản đồ API, rồi đề xuất thứ tự migrate.
>
> **Cách làm:**
> 1. Đọc file routing để lấy danh sách route/màn hình. Báo tổng số màn hình trước khi phân tích.
> 2. Với mỗi màn hình, truy đủ lời gọi API — kể cả gọi gián tiếp qua service, interceptor, resolver, guard. Nếu URL ghép chuỗi động, ghi rõ công thức và các giá trị có thể có.
> 3. Ghi ra `/docs/migration/02-screen-map.md`:
>
> | Route | Component chính | Action API gọi | Gọi lúc nào (load/submit/event) | Quyền truy cập | Phức tạp 1-5 |
>
> **Sau bảng, xuất tiếp:**
>
> **a. Đối chiếu hai chiều**
> - API mồ côi: có trong backend nhưng không màn hình nào gọi → ứng viên xoá, không migrate.
> - Lời gọi hụt: Angular gọi action không tìm thấy trong bản đồ API → dấu hiệu còn source chưa khảo sát. Báo rõ.
> - API dùng chung nhiều màn hình → phải bóc thành service trước tiên.
>
> **b. Thứ tự migrate, chia 3 đợt**, mỗi màn hình kèm lý do một câu:
> - Đợt 1: read-only, ít phụ thuộc, dùng để chạy thử quy trình
> - Đợt 2: CRUD tiêu chuẩn
> - Đợt 3: nghiệp vụ phức tạp, nhiều ràng buộc
>
> **c. Chia việc cho đúng 2 người**, theo nguyên tắc: mỗi người ôm trọn một số màn hình từ service → UI → test (chia dọc, không chia theo tầng); hai người không được sửa chung file. Chỉ rõ file/thư mục nào là **vùng dùng chung cần một owner duy nhất**.
>
> **d. Ước lượng** số ngày công cho mỗi đợt, nêu rõ giả định đã dùng để ước lượng.

---

## PROMPT 3 — Bóc component và lập kế hoạch thư viện UI

> Đọc `/docs/migration/02-screen-map.md`. Bây giờ quét toàn bộ component Angular ở mức chi tiết.
>
> **Nhiệm vụ:** lập danh mục component và ánh xạ sang Blazor.
>
> Ghi ra `/docs/migration/03-component-map.md` gồm 4 phần:
>
> **1. Component dùng lại nhiều nơi** — cái nào xuất hiện ở ≥2 màn hình. Đây là thư viện component cần xây dựng **trước tiên**, do một người sở hữu.
>
> | Component | Số màn hình dùng | Input/Output | Chức năng | Ưu tiên xây |
>
> **2. Thư viện bên thứ ba** — mọi package UI đang dùng (grid, chart, date picker, editor, upload, tree…):
>
> | Thư viện | Dùng ở đâu | Chức năng đang khai thác | Tương đương trong MudBlazor/Radzen | Có mất tính năng không | Rủi ro 1-5 |
>
> Với thư viện không có bản Blazor tương đương, đề xuất rõ: JS interop bọc lại, hay đổi sang giải pháp khác, hay viết tay. **Không được ghi chung chung "có thể tìm giải pháp thay thế"** — phải nêu tên cụ thể hoặc nói thẳng là chưa có.
>
> **3. Phần khó chuyển** — liệt kê những chỗ cần quyết định con người, kèm `File:dòng`:
> - Logic phụ thuộc DOM trực tiếp, animation phức tạp
> - State chia sẻ giữa nhiều component (service dùng chung, RxJS store)
> - Form động, form lồng nhau, validation tuỳ biến
> - Upload/download file, xử lý real-time
> - Màn hình dữ liệu lớn — cần cảnh báo riêng, vì Blazor Server render trên server nên bảng vài nghìn dòng sẽ gây nghẽn, phải ảo hoá hoặc phân trang server-side
>
> **4. Danh sách quyết định cần chốt** — dạng câu hỏi để hai người ngồi lại quyết, không tự quyết thay.
>
> **Quy tắc:** đây là bản kiểm kê, không phải bản chuyển đổi code. Chưa sinh Razor component ở bước này.

---

## Lưu ý khi chạy

- Với repo lớn, AI sẽ chạm giới hạn context giữa chừng. Vì vậy cả 3 prompt đều yêu cầu **ghi file theo lô** — nếu đứt, bảo nó đọc lại file đã ghi và tiếp tục từ dòng cuối, không cần chạy lại từ đầu.
- Kết quả của Prompt 1 và 2 phải có người mở code gốc đối chiếu ngẫu nhiên vài chỗ trước khi dùng để chia việc. Sai ở bước lập bản đồ sẽ kéo sai suốt dự án.
- Commit cả 3 file vào repo. Đây vừa là tài liệu bàn giao, vừa là thứ giữ hai người nhìn cùng một bản đồ.
