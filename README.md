# TechPro Care (Full Solution)
Hệ thống quản lý sửa chữa & bảo hành thiết bị, gồm **2 project**:

- **`TechPro.API`**: ASP.NET Core Web API + EF Core (PostgreSQL/Supabase)
- **`TechPro.MVC`**: ASP.NET Core MVC (UI), gọi API qua `HttpClient`

> Repo này **không nhận đóng góp (no contributions)**. Vui lòng chỉ dùng nội bộ/triển khai theo nhu cầu của bạn.

---

## Tổng quan tính năng

- **Tiếp nhận (Support)**: tạo phiếu, xem danh sách, xử lý quy trình
- **Kỹ thuật (Technician)**: Kanban, nhận phiếu, cập nhật trạng thái, ghi chú, checklist, yêu cầu linh kiện
- **Kho (Storekeeper/StoreAdmin)**: quản lý tồn kho, duyệt/từ chối yêu cầu xuất kho, trả xác
- **Quản lý (StoreAdmin/SystemAdmin)**: dashboard, báo cáo, export
- **Khách hàng (Public)**: tra cứu trạng thái sửa chữa, tra cứu bảo hành, đặt lịch
- **Đa ngôn ngữ (VI/EN)**: chuyển sang English bằng **dịch tự động (Gemini)** + cache HTML

---

## Yêu cầu hệ thống

- .NET SDK **10.0**
- PostgreSQL (khuyến nghị Supabase)
- (Tuỳ chọn) Node/JS không bắt buộc – UI là Razor + JS

---

## Cấu hình quan trọng

### 1) API: kết nối database
File: `TechPro.API/appsettings.json`

- `ConnectionStrings:DefaultConnection`: chuỗi kết nối PostgreSQL/Supabase

> Với Supabase, dùng connection string Postgres (không dùng REST).

### 2) MVC: trỏ về API
File: `TechPro.MVC/appsettings.json`

- `ApiSettings:BaseUrl`: ví dụ `http://localhost:5078`

### 3) Gemini (dịch trang sang English)
File: `TechPro.MVC/appsettings.json`

- `Gemini:ApiKey`: API key Gemini dùng cho **dịch HTML tự động** khi chọn EN

Khuyến nghị production: **không hardcode key** trong repo, mà dùng biến môi trường:

- `Gemini__ApiKey`

---

## Chạy dự án (Local)

### Cách nhanh nhất (PowerShell – Windows)
Tại thư mục gốc repo:

```powershell
cd d:\TechPro\full
.\run-all.ps1 -Watch
```

Script sẽ mở 2 cửa sổ:

- `TechPro.API` chạy `dotnet watch run`
- `TechPro.MVC` chạy `dotnet watch run`

### Chạy thủ công

```powershell
# API
cd d:\TechPro\full\TechPro.API
dotnet run

# MVC
cd d:\TechPro\full\TechPro.MVC
dotnet run
```

Ports (xem chính xác trong `Properties/launchSettings.json`):

| Project | HTTP | HTTPS |
|---|---|---|
| TechPro.API | `http://localhost:5078` | `https://localhost:7103` |
| TechPro.MVC | `http://localhost:5077` | `https://localhost:7127` |

---

## API Documentation (Swagger)

Swagger UI chạy tự động ở môi trường **Development**:

| URL | Mô tả |
|---|---|
| **`http://localhost:5078/swagger`** | 🎨 Swagger UI |
| `http://localhost:5078/swagger/v1/swagger.json` | 📄 Raw OpenAPI JSON spec |

> **Xác thực**: nhập email người dùng vào ô `X-Caller-Email` trên Scalar UI để test các endpoint yêu cầu đăng nhập.

---

## Database & Migrations

`TechPro.API/Program.cs` có `context.Database.Migrate()` nên khi chạy API sẽ:

- Apply migrations (nếu có)
- Seed dữ liệu mẫu (nếu có cấu hình seed)

Nếu DB Supabase mới mà thiếu bảng, hãy đảm bảo migrations đã được apply hoặc tạo bảng tương ứng.

---

## Luồng kho – kỹ thuật (đúng quy trình)

1. **Technician**: vào chi tiết phiếu → tab **Linh kiện** → chọn linh kiện → bấm **Yêu cầu**
2. **Storekeeper/StoreAdmin**: vào `/Kho/Manage?tab=requests` → bấm **Duyệt**
3. Tồn kho bị trừ, yêu cầu chuyển trạng thái **approved**

---

## Dịch tự động sang English (tất cả trang)

- Nút đổi ngôn ngữ nằm trên header (icon translate).
- Khi chọn **English**, middleware trong MVC sẽ:
  - Render HTML bình thường
  - Gọi Gemini dịch **chỉ text hiển thị** (giữ nguyên tag/attribute)
  - Cache vào `TechPro.MVC/App_Data/html-translate-cache`

Debug nhanh:
- Response headers có:
  - `X-TP-Lang: en`
  - `X-TP-Translate: gemini_ok | cache_hit | gemini_http_XXX | ...`

---

## Tài khoản & vai trò (tuỳ seed)

Các role chính:

| Role | Quyền hạn |
|---|---|
| `SystemAdmin` | Toàn bộ hệ thống, quản lý chuỗi cửa hàng |
| `StoreAdmin` | Quản lý 1 cửa hàng, xem báo cáo |
| `Support` | Tiếp nhận, tạo/đóng phiếu sửa chữa |
| `Technician` | Sửa chữa, yêu cầu linh kiện |
| `Storekeeper` | Quản lý kho, duyệt yêu cầu linh kiện |

Thông tin account seed phụ thuộc `TechPro.API/Data/DbInitializer.cs`.

---

## Danh sách trang (35 trang)

| Nhóm | URL | Mô tả |
|---|---|---|
| Public | `/` | Trang chủ |
| Public | `/Home/TraCuu` | Tra cứu phiếu sửa chữa |
| Public | `/Home/DatLich` | Đặt lịch hẹn |
| Public | `/Home/Warranty` | Tra cứu bảo hành |
| Public | `/TraMay` | Tra máy theo Serial Number |
| Public | `/Account/Login` | Đăng nhập |
| Support | `/TiepNhan` | Danh sách phiếu tiếp nhận |
| Support | `/TiepNhan/ChiTiet/{id}` | Chi tiết phiếu sửa chữa |
| Support | `/Support` | Màn hình Support tổng quan |
| Technician | `/KyThuat` | Danh sách phiếu kỹ thuật |
| Technician | `/KyThuat/ChiTiet/{id}` | Chi tiết sửa chữa |
| Technician | `/KyThuat/LinhKien` | Quản lý linh kiện yêu cầu |
| Technician | `/TechnicianDashboard` | Dashboard kỹ thuật viên |
| Kho | `/Kho` | Tổng quan kho |
| Kho | `/Kho/Manage` | Quản lý tồn kho |
| Kho | `/Kho/Request` | Duyệt yêu cầu linh kiện |
| Kho | `/Kho/View` | Xem chi tiết kho |
| Kho | `/StorekeeperDashboard` | Dashboard thủ kho |
| Quản lý | `/QuanLy` | Tổng quan quản lý |
| Quản lý | `/QuanLy/BaoCao` | Báo cáo thống kê |
| Quản lý | `/QuanLy/CauHinh` | Cấu hình cửa hàng |
| Quản lý | `/StoreAdminDashboard` | Dashboard quản lý cửa hàng |
| Monitor | `/TiepNhanMonitor` | Monitor tiếp nhận realtime |
| Monitor | `/KyThuatMonitor` | Monitor kỹ thuật realtime |
| SystemAdmin | `/SysAdminDashboard` | Dashboard hệ thống |
| SystemAdmin | `/Chain` | Danh sách chuỗi cửa hàng |
| SystemAdmin | `/Chain/ChiTietCuaHang/{id}` | Chi tiết cửa hàng |

---

## Triển khai (Deploy) gợi ý

- Reverse proxy: Nginx (khuyến nghị)
- Chạy API + MVC như 2 service
- DB: Supabase Postgres
- Logs: nên bật rotate (disk nhỏ)

---

## No contributions

Repo này **không nhận PR/issue/đóng góp**. Nếu cần chỉnh sửa theo yêu cầu riêng, hãy fork hoặc triển khai nội bộ.

