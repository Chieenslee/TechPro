# 📚 TECHPRO CARE - TÀI LIỆU HƯỚNG DẪN ĐẦY ĐỦ

## 📋 MỤC LỤC

1. [Giới thiệu](#giới-thiệu)
2. [Hướng dẫn cài đặt và chạy ứng dụng](#hướng-dẫn-cài-đặt-và-chạy-ứng-dụng)
3. [Tài khoản đăng nhập để test](#tài-khoản-đăng-nhập-để-test)
4. [Tính năng mới](#tính-năng-mới)
5. [Hướng dẫn cấu hình Gemini AI](#hướng-dẫn-cấu-hình-gemini-ai)
6. [Hướng dẫn chuyển đổi từ React](#hướng-dẫn-chuyển-đổi-từ-react)
7. [Cấu trúc dự án](#cấu-trúc-dự-án)

---

## 🎯 GIỚI THIỆU

**TechPro Care** là hệ thống quản lý sửa chữa thiết bị điện tử với đầy đủ các chức năng:
- Quản lý tiếp nhận phiếu sửa chữa
- Điều phối kỹ thuật viên (Kanban board)
- Quản lý kho linh kiện
- Báo cáo và thống kê
- Tra cứu trạng thái sửa chữa
- Tra cứu bảo hành
- Đặt lịch sửa chữa
- AI Chatbot tư vấn
- Quản lý chuỗi cửa hàng (System Admin)

**Công nghệ sử dụng:**
- ASP.NET Core 10.0 (MVC)
- Entity Framework Core 10.0
- SQL Server
- SignalR (Real-time)
- Bootstrap 5.3.3
- Chart.js 4.4.3
- Gemini AI (Chatbot)

---

## 🚀 HƯỚNG DẪN CÀI ĐẶT VÀ CHẠY ỨNG DỤNG

### Yêu cầu hệ thống
- .NET SDK 10.0
- SQL Server Express hoặc SQL Server
- Visual Studio 2022 hoặc VS Code

### Bước 1: Cấu hình Database

**Connection String** trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LC\\SQLEXPRESS;Database=TechProDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=True"
  }
}
```

**Lưu ý:** Thay `LC\\SQLEXPRESS` bằng tên SQL Server của bạn.

### Bước 2: Khôi phục Packages

```powershell
cd D:\TechPro\TechPro
dotnet restore
```

### Bước 3: Build dự án

```powershell
dotnet build
```

### Bước 4: Tạo Database

```powershell
# Tạo migration
dotnet ef migrations add InitialCreate

# Tạo database và tables
dotnet ef database update
```

**Lưu ý:** Nếu đã có migrations, chỉ cần chạy `dotnet ef database update`.

### Bước 5: Chạy ứng dụng

```powershell
dotnet run
```

Ứng dụng sẽ chạy tại:
- **HTTP:** http://localhost:5077
- **HTTPS:** https://localhost:7127

### Bước 6: Database Seeding

Khi chạy ứng dụng lần đầu, hệ thống sẽ tự động:
- Tạo các roles (SystemAdmin, StoreAdmin, Technician, Support)
- Tạo các tài khoản test
- Tạo cửa hàng mẫu
- Seed dữ liệu mẫu (30 linh kiện, 30 phiếu sửa chữa, v.v.)

---

## 🔐 TÀI KHOẢN ĐĂNG NHẬP ĐỂ TEST

Tất cả tài khoản có **mật khẩu: `demo123`**

### 1. System Admin (Quản lý chuỗi)
- **Email:** `admin@techpro.com`
- **Mật khẩu:** `demo123`
- **Role:** SystemAdmin
- **Trang chính:** `/Chain` - Tổng quan chuỗi
- **Quyền:** Quản lý toàn hệ thống, tất cả cửa hàng

### 2. Store Admin (Quản lý cửa hàng)
- **Email:** `admin@store1.com`
- **Mật khẩu:** `demo123`
- **Role:** StoreAdmin
- **Trang chính:** `/QuanLy` - Báo cáo & Hiệu suất
- **Quyền:** Quản lý cửa hàng, xem báo cáo, cấu hình

### 3. Support (Tiếp nhận)
- **Email:** `support@store1.com`
- **Mật khẩu:** `demo123`
- **Role:** Support
- **Trang chính:** `/TiepNhan` - Quản lý tiếp nhận
- **Quyền:** Tiếp nhận máy, tạo phiếu mới, quản lý kho

### 4. Technician (Kỹ thuật viên)
- **Email:** `tech@store1.com`
- **Mật khẩu:** `demo123`
- **Role:** Technician
- **Trang chính:** `/KyThuat` - Điều phối kỹ thuật
- **Quyền:** Xem và sửa chữa phiếu được phân công

---

## 🎯 CÁC TRANG THEO ROLE

### System Admin (`admin@techpro.com`)
- `/Chain` - Tổng quan chuỗi (4 tabs: Overview, Stores, Staff, Inventory)
- Có thể xem tất cả cửa hàng, nhân sự, kho tổng
- Quản lý cửa hàng: Tạo, cập nhật, xóa
- Quản lý nhân sự: Tạo, điều chuyển, xóa
- Quản lý kho tổng: Cập nhật giá chuẩn

### Store Admin (`admin@store1.com`)
- `/QuanLy` - Báo cáo & Hiệu suất
- `/QuanLy/CauHinh` - Cấu hình cửa hàng
- `/TiepNhan` - Quản lý tiếp nhận
- `/Kho` - Quản lý kho vận
- `/KyThuat` - Điều phối kỹ thuật

### Support (`support@store1.com`)
- `/TiepNhan` - Quản lý tiếp nhận (tạo phiếu mới)
- `/Kho` - Quản lý kho vận (duyệt yêu cầu linh kiện)
- `/KyThuat` - Xem danh sách phiếu

### Technician (`tech@store1.com`)
- `/KyThuat` - Việc của tôi (My Tasks)
- `/KyThuat/ChiTiet/{id}` - Chi tiết phiếu sửa chữa
- Chỉ thấy phiếu được phân công hoặc chưa phân công

---

## 🚀 TÍNH NĂNG MỚI

### 1. 🤖 AI Chatbot với Gemini API

**Tính năng:**
- Chatbot tư vấn tự động 24/7
- Tích hợp Google Gemini AI
- Hỗ trợ tư vấn về dịch vụ sửa chữa
- Hướng dẫn quy trình và chính sách
- Tra cứu thông tin nhanh chóng

**Cách sử dụng:**
1. Click vào icon robot ở góc dưới bên phải (trang Home)
2. Nhập câu hỏi hoặc chọn câu hỏi nhanh
3. Chatbot sẽ trả lời tự động

**Cấu hình:** Xem phần [Hướng dẫn cấu hình Gemini AI](#hướng-dẫn-cấu-hình-gemini-ai)

### 2. 🌙 Dark Mode

**Tính năng:**
- Chuyển đổi giao diện sáng/tối
- Lưu tùy chọn trong localStorage
- Tự động áp dụng khi reload trang

**Cách sử dụng:**
- Click vào icon mặt trăng/trời ở header

### 3. ⌨️ Keyboard Shortcuts

**Phím tắt:**
- `Ctrl/Cmd + K`: Mở tìm kiếm nhanh
- `Ctrl/Cmd + /`: Mở chatbot
- `Escape`: Đóng modal/chatbot

### 4. 📊 Real-time Stats Widget

**Tính năng:**
- Cập nhật thống kê tự động mỗi 30 giây
- Hiển thị số liệu thời gian thực
- Animation mượt mà khi thay đổi

### 5. 📊 Export Excel/PDF Báo cáo

**Tính năng:**
- Export danh sách phiếu sửa chữa ra Excel (CSV)
- Export báo cáo doanh thu
- Export danh sách kho linh kiện
- In báo cáo trực tiếp từ trình duyệt
- Xuất phiếu sửa chữa ra PDF

**Cách sử dụng:**
1. Vào trang **Quản lý** (`/QuanLy`)
2. Click nút **"Xuất Excel"** hoặc **"In báo cáo"**

### 6. 🔔 Hệ thống thông báo Real-time

**Tính năng:**
- Thông báo real-time qua SignalR
- Hiển thị thông báo trong header với badge số lượng chưa đọc
- Auto-refresh thông báo mới
- Đánh dấu đã đọc

**Cách sử dụng:**
1. Click icon **bell** ở header
2. Xem danh sách thông báo
3. Click "Đánh dấu tất cả đã đọc" để xóa badge

### 7. 🔍 Advanced Search & Filters

**Tính năng:**
- Tìm kiếm nâng cao với debounce (tự động tìm sau 500ms)
- Filter theo trạng thái
- Filter theo khoảng thời gian
- Clear filters nhanh

### 8. 👤 Customer History Tracking

**Tính năng:**
- Xem lịch sử khách hàng
- Tổng số phiếu đã sửa
- Tổng chi tiêu
- Danh sách thiết bị bảo hành
- Lịch sử sửa chữa chi tiết

**Cách sử dụng:**
1. Từ trang tiếp nhận, click vào số điện thoại
2. Hoặc truy cập: `/Customer/History?phoneNumber=0912345678`

### 9. ⚠️ Inventory Alerts

**Tính năng:**
- Cảnh báo tự động khi:
  - Linh kiện hết hàng (số lượng = 0)
  - Linh kiện sắp hết (số lượng < 5)
- Auto-check: Tự động kiểm tra mỗi 5 phút
- Toast notifications với link đến trang kho

### 10. 🎨 UI/UX Improvements

**Tính năng:**
- Custom scrollbar đẹp mắt
- Smooth animations
- Loading skeletons
- Focus styles cho accessibility
- Print-friendly styles
- Responsive design

### 11. 📱 Social Contact Widget

**Tính năng:**
- Widget liên hệ các nền tảng xã hội
- Mở ngang sang trái với animation
- Các nền tảng: Messenger, Zalo, YouTube, Telegram, Facebook, TikTok, Phone, Email

---

## 🔧 HƯỚNG DẪN CẤU HÌNH GEMINI AI

### Bước 1: Lấy API Key từ Google Gemini

1. Truy cập: https://makersuite.google.com/app/apikey
2. Đăng nhập bằng tài khoản Google
3. Click "Create API Key"
4. Copy API Key được tạo

### Bước 2: Cấu hình trong appsettings.json

Mở file `appsettings.json` và thêm API Key:

```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY_HERE"
  }
}
```

Thay `YOUR_GEMINI_API_KEY_HERE` bằng API Key bạn đã copy.

### Bước 3: Khởi động lại ứng dụng

Sau khi cấu hình, khởi động lại ứng dụng để chatbot hoạt động.

### Lưu ý

- API Key miễn phí có giới hạn số lượng request
- Không chia sẻ API Key công khai
- Có thể cấu hình trong biến môi trường để bảo mật hơn

---

## 📖 HƯỚNG DẪN CHUYỂN ĐỔI TỪ REACT

Dự án này đã được chuyển đổi từ ứng dụng React sang ASP.NET Core MVC với đầy đủ các chức năng.

### Các giai đoạn đã hoàn thành

#### ✅ Giai đoạn 1: Thiết kế cơ sở dữ liệu & Entities

**Các Entities đã tạo:**
- `NguoiDung.cs` - Quản lý người dùng (kế thừa IdentityUser)
- `CuaHang.cs` - Thông tin cửa hàng
- `PhieuSuaChua.cs` - Phiếu sửa chữa
- `KhoLinhKien.cs` - Quản lý linh kiện trong kho
- `YeuCauLinhKien.cs` - Yêu cầu xuất linh kiện
- `TraXac.cs` - Trả xác linh kiện
- `ThietBiBan.cs` - Thiết bị đã bán (để kiểm tra bảo hành)
- `LichHen.cs` - Đặt lịch sửa chữa
- `TicketNote.cs` - Ghi chú nội bộ trên phiếu
- `ScratchMark.cs` - Đánh dấu vết xước trên thiết bị
- `RevenueDaily.cs` - Doanh thu theo ngày
- `Notification.cs` - Thông báo

**DbContext:**
- `TechProDbContext.cs` - Kết nối với SQL Server qua EF Core

#### ✅ Giai đoạn 2: Xây dựng giao diện bố cục

**Layouts:**
- `Views/Shared/_Layout.cshtml` - Layout chính với Bootstrap 5, jQuery, Toastr, SignalR
- `Views/Shared/_ThanhBen.cshtml` - Sidebar với menu động theo vai trò

**Tính năng:**
- Responsive design với Bootstrap 5
- Menu tự động lọc theo vai trò người dùng
- Header với tìm kiếm và thông báo

#### ✅ Giai đoạn 3: Chức năng tiếp nhận

**Controller:** `TiepNhanController.cs`

**Actions:**
- `Index` - Danh sách phiếu tiếp nhận
- `TaoPhieu` - Tạo phiếu mới (POST với AJAX)
- `KiemTraBaoHanh` - Kiểm tra bảo hành qua Serial (AJAX)

**View:** `Views/TiepNhan/Index.cshtml`
- Table Bootstrap hiển thị danh sách phiếu
- Modal tạo phiếu mới
- AJAX kiểm tra bảo hành tự động khi nhập Serial
- Toastr notifications

#### ✅ Giai đoạn 4: Giao diện kỹ thuật & Kanban

**Controller:** `KyThuatController.cs`

**Actions:**
- `Index` - Kanban board với 4 cột trạng thái
- `ChiTiet` - Chi tiết phiếu sửa chữa
- `CapNhatTrangThai` - Cập nhật trạng thái (AJAX)
- `GanChoToi` - Kỹ thuật viên nhận phiếu (AJAX)
- `LuuKetQuaKiemTra` - Lưu kết quả kiểm tra
- `TaoYeuCauLinhKien` - Tạo yêu cầu linh kiện

**View:** `Views/KyThuat/Index.cshtml`
- Kanban board với Bootstrap columns
- Drag & drop visual (có thể mở rộng)
- SignalR real-time updates

**View:** `Views/KyThuat/ChiTiet.cshtml`
- Chi tiết phiếu với thông tin đầy đủ
- Chat nội bộ với SignalR
- Visual device inspector (đánh dấu vết xước)
- Buttons cập nhật trạng thái

#### ✅ Giai đoạn 5: Quản lý kho & yêu cầu linh kiện

**Controller:** `KhoController.cs`

**Actions:**
- `Index` - Trang quản lý kho (3 tabs: Inventory, Requests, Waste)
- `DuyetYeuCau` - Duyệt yêu cầu và trừ tồn kho
- `TuChoiYeuCau` - Từ chối yêu cầu
- `XacNhanTraXac` - Xác nhận trả xác linh kiện
- `GetLowStockAlerts` - Lấy cảnh báo hết hàng

**View:** `Views/Kho/Index.cshtml`
- Tab navigation
- Table hiển thị tồn kho
- Danh sách yêu cầu với buttons duyệt/từ chối
- AJAX operations với Toastr notifications

#### ✅ Giai đoạn 6: Báo cáo & cấu hình (Admin)

**Controller:** `QuanLyController.cs`

**Actions:**
- `Index` - Dashboard với thống kê và biểu đồ
- `BaoCao` - Trang báo cáo
- `CauHinh` - Cấu hình cửa hàng (GET/POST)

**View:** `Views/QuanLy/Index.cshtml`
- Stat cards với thống kê
- Chart.js biểu đồ doanh thu (7/30/90 ngày)
- Biểu đồ trạng thái phiếu (Doughnut)
- Top linh kiện bán chạy (Horizontal bar)
- Hiệu suất kỹ thuật viên (Grouped bar)
- Export Excel và In báo cáo

**View:** `Views/QuanLy/CauHinh.cshtml`
- Form cấu hình cửa hàng
- Validation với Data Annotations
- Toastr success notification

#### ✅ Giai đoạn 7: Trang công khai (Public)

**Controller:** `HomeController.cs`

**Actions:**
- `Index` - Trang chủ (Landing page)
- `Search` - Tra cứu phiếu sửa chữa hoặc bảo hành
- `TraCuu` - Chi tiết tra cứu phiếu
- `Warranty` - Chi tiết tra cứu bảo hành
- `DatLich` - Đặt lịch sửa chữa (GET/POST)
- `XacNhanBaoGia` - Xác nhận báo giá

**Views:**
- `Views/Home/Index.cshtml` - Landing page với chatbot và social widget
- `Views/Home/TraCuu.cshtml` - Tra cứu trạng thái sửa chữa
- `Views/Home/Warranty.cshtml` - Tra cứu bảo hành
- `Views/Home/DatLich.cshtml` - Đặt lịch sửa chữa

#### ✅ Giai đoạn 8: Quản lý chuỗi (System Admin)

**Controller:** `ChainController.cs`

**Actions:**
- `Index` - Dashboard tổng quan chuỗi (4 tabs)
- `ChiTietCuaHang` - Chi tiết cửa hàng
- `TaoCuaHang` - Tạo cửa hàng mới
- `CapNhatCuaHang` - Cập nhật cửa hàng
- `XoaCuaHang` - Xóa cửa hàng
- `TaoNhanVien` - Tạo nhân viên mới
- `DieuChuyenNhanVien` - Điều chuyển nhân viên
- `XoaNhanVien` - Xóa nhân viên
- `CapNhatGiaChuan` - Cập nhật giá chuẩn kho tổng
- `GetStoreRevenue` - API lấy doanh thu cửa hàng

**Views:**
- `Views/Chain/Index.cshtml` - Dashboard với 4 tabs
- `Views/Chain/ChiTietCuaHang.cshtml` - Chi tiết cửa hàng

---

## 📁 CẤU TRÚC DỰ ÁN

```
TechPro/
├── Controllers/              # Các controller
│   ├── AccountController.cs      # Authentication
│   ├── HomeController.cs         # Public pages
│   ├── TiepNhanController.cs     # Reception
│   ├── KyThuatController.cs      # Technician
│   ├── KhoController.cs          # Warehouse
│   ├── QuanLyController.cs      # Manager
│   ├── ChainController.cs       # System Admin
│   ├── CustomerController.cs    # Customer history
│   ├── ExportController.cs      # Export Excel/PDF
│   ├── NotificationController.cs # Notifications API
│   ├── ChatbotController.cs     # AI Chatbot API
│   └── FeaturesController.cs    # Features API
├── Models/                  # Các model/entity
│   ├── NguoiDung.cs
│   ├── PhieuSuaChua.cs
│   ├── KhoLinhKien.cs
│   ├── CuaHang.cs
│   ├── LichHen.cs
│   ├── TicketNote.cs
│   ├── ScratchMark.cs
│   ├── RevenueDaily.cs
│   ├── Notification.cs
│   └── ViewModels/
├── Views/                   # Các view Razor
│   ├── Shared/
│   │   ├── _Layout.cshtml
│   │   ├── _ThanhBen.cshtml
│   │   ├── _Chatbot.cshtml
│   │   └── _SocialContactWidget.cshtml
│   ├── Home/
│   ├── TiepNhan/
│   ├── KyThuat/
│   ├── Kho/
│   ├── QuanLy/
│   ├── Chain/
│   └── Customer/
├── Data/                    # DbContext & Seeding
│   ├── TechProDbContext.cs
│   └── DbInitializer.cs
├── Hubs/                    # SignalR hubs
│   └── TicketHub.cs
├── Services/                # Services
│   └── GeminiChatService.cs
├── wwwroot/                 # Static files
│   ├── css/
│   ├── js/
│   ├── images/
│   └── lib/
├── Migrations/              # EF Core migrations
├── appsettings.json        # Configuration
└── Program.cs              # Entry point
```

---

## 🔧 CÁC THƯ VIỆN ĐÃ SỬ DỤNG

- **Bootstrap 5.3.3** - UI Framework
- **Bootstrap Icons 1.11.3** - Icons
- **jQuery 3.7.1** - AJAX & DOM manipulation
- **Toastr.js** - Notifications
- **Chart.js 4.4.3** - Charts/Graphs
- **SignalR 8.0.11** - Real-time communication
- **Entity Framework Core 10.0** - ORM
- **ASP.NET Core Identity 10.0** - Authentication
- **jsPDF 2.5.1** - PDF export
- **html2canvas 1.4.1** - Screenshot for PDF
- **SheetJS (xlsx) 0.18.5** - Excel export
- **Lucide Icons** - Additional icons

---

## 📝 GHI CHÚ QUAN TRỌNG

1. **Roles:** Được tạo tự động khi seed database
2. **Database:** Migration sẽ tự động tạo tables
3. **SignalR:** Cần HTTPS hoặc config CORS cho production
4. **Validation:** Đã có cả client-side và server-side
5. **Security:** Anti-forgery tokens đã được thêm vào tất cả forms
6. **Seeding:** Tự động seed 30 records cho mỗi loại dữ liệu lớn
7. **Mật khẩu test:** Tất cả tài khoản dùng `demo123` (chỉ dùng cho test)

---

## 🔄 RESET DỮ LIỆU

Nếu muốn reset và seed lại dữ liệu:

```powershell
# Xóa database
dotnet ef database drop

# Tạo lại database
dotnet ef database update

# Chạy lại ứng dụng (sẽ tự động seed)
dotnet run
```

---

## ⚠️ BẢO MẬT

**LƯU Ý:** Đây là mật khẩu test, không dùng cho môi trường production!

Trong production, cần:
- Đổi mật khẩu mạnh
- Bật yêu cầu mật khẩu phức tạp
- Sử dụng 2FA nếu cần
- Cấu hình HTTPS
- Bảo mật API keys

---

## 🎯 CÁC TÍNH NĂNG CHÍNH

- ✅ Quản lý tiếp nhận phiếu sửa chữa
- ✅ Kanban board cho kỹ thuật viên
- ✅ Quản lý kho linh kiện
- ✅ Duyệt yêu cầu linh kiện
- ✅ Dashboard với biểu đồ đa dạng
- ✅ Cấu hình cửa hàng
- ✅ Real-time updates với SignalR
- ✅ Role-based authorization
- ✅ Toastr notifications
- ✅ AJAX operations
- ✅ Export Excel/PDF
- ✅ Customer history tracking
- ✅ Inventory alerts
- ✅ AI Chatbot
- ✅ Dark mode
- ✅ Keyboard shortcuts
- ✅ Social contact widget
- ✅ Tra cứu trạng thái sửa chữa
- ✅ Tra cứu bảo hành
- ✅ Đặt lịch sửa chữa
- ✅ Quản lý chuỗi cửa hàng

---

## 📞 HỖ TRỢ

Nếu có vấn đề, vui lòng kiểm tra:
1. Connection string trong `appsettings.json`
2. Database đã được tạo chưa
3. Roles và users đã được seed chưa
4. Packages đã được restore chưa
5. SQL Server đang chạy chưa

---

**Phiên bản:** 1.0.0  
**Cập nhật lần cuối:** 2026-01-08

