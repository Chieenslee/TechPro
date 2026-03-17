using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechPro.Models;
using TechPro.Models.ViewModels;
using System.Text.Json;
using System.Text;

namespace TechPro.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index()
        {
            // Nếu đã đăng nhập, redirect theo role
            if (User.Identity?.IsAuthenticated == true)
            {
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                return userRole switch
                {
                    "SystemAdmin" => RedirectToAction("Index", "Chain"),
                    "Technician" => RedirectToAction("Index", "KyThuat"),
                    "Support" => RedirectToAction("Index", "TiepNhan"),
                    "StoreAdmin" => RedirectToAction("Index", "QuanLy"),
                    _ => RedirectToAction("Index", "TiepNhan")
                };
            }

            // Tạo ViewModel với dữ liệu từ database hoặc config
            var viewModel = new LandingPageViewModel
            {
                DichVus = new List<DichVuViewModel>
                {
                    new() { Ten = "Thay màn hình", MoTa = "Màn hình zin chính hãng, bảo hành cảm ứng 12 tháng.", Icon = "phone" },
                    new() { Ten = "Thay Pin chính hãng", MoTa = "Pin dung lượng chuẩn, kiểm tra độ chai pin miễn phí.", Icon = "battery-charging" },
                    new() { Ten = "Cấp cứu dữ liệu", MoTa = "Khôi phục dữ liệu từ máy hỏng, treo logo, vỡ nát.", Icon = "box-seam" },
                    new() { Ten = "Mở khóa iCloud", MoTa = "Xử lý phần mềm, mở khóa tài khoản an toàn.", Icon = "shield-check" },
                    new() { Ten = "Sửa chữa phần cứng", MoTa = "Xử lý lỗi mainboard, IC nguồn, mất sóng chuyên sâu.", Icon = "cpu" },
                    new() { Ten = "Vệ sinh & Bảo dưỡng", MoTa = "Vệ sinh máy miễn phí cho khách hàng sửa chữa.", Icon = "check-circle" },
                    new() { Ten = "Ép kính công nghệ cao", MoTa = "Giữ nguyên màn hình gốc, tiết kiệm chi phí tối đa.", Icon = "wrench" },
                    new() { Ten = "Phụ kiện chính hãng", MoTa = "Cung cấp cáp sạc, tai nghe, ốp lưng chất lượng cao.", Icon = "headphones" }
                },
                ChinhSaches = new List<ChinhSachViewModel>
                {
                    new() { TieuDe = "1 Đổi 1", MoTa = "Đổi mới linh kiện trong 30 ngày nếu có lỗi sản xuất." },
                    new() { TieuDe = "Hoàn tiền 100%", MoTa = "Nếu khách hàng không hài lòng về chất lượng sửa chữa." },
                    new() { TieuDe = "Bảo hành trọn đời", MoTa = "Dành cho dịch vụ thay pin cao cấp và dán cường lực." },
                    new() { TieuDe = "Hỗ trợ từ xa", MoTa = "Tư vấn kỹ thuật online miễn phí trọn đời sản phẩm." }
                },
                ThongTinLienHe = new ThongTinLienHeViewModel
                {
                    DiaChi = "123 Đường Công Nghệ, Quận Hoàn Kiếm, TP. Hà Nội",
                    Hotline = "1900 6868 (8:00 - 21:00)",
                    Email = "support@techprocare.vn",
                    EmailBusiness = "business@techprocare.vn"
                }
            };

            return View(viewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Search(string query, string mode = "repair")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập thông tin tra cứu.";
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient("TechProAPI");

            if (mode == "repair")
            {
                // Tìm phiếu sửa chữa
                var response = await client.GetAsync($"api/TiepNhan/search?query={Uri.EscapeDataString(query)}");
                
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin phiếu sửa chữa này.";
                    TempData["SearchQuery"] = query;
                    TempData["SearchMode"] = mode;
                    return RedirectToAction("Index");
                }

                var phieu = await response.Content.ReadFromJsonAsync<PhieuSuaChua>();
                if (phieu == null)
                {
                     TempData["ErrorMessage"] = "Không tìm thấy thông tin phiếu sửa chữa này.";
                     return RedirectToAction("Index");
                }

                return RedirectToAction("TraCuu", new { id = phieu.Id });
            }
            else
            {
                // Kiểm tra bảo hành
                var response = await client.GetAsync($"api/TiepNhan/device-warranty?serial={Uri.EscapeDataString(query)}");

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin Serial Number này trong hệ thống bán hàng.";
                    TempData["SearchQuery"] = query;
                    TempData["SearchMode"] = mode;
                    return RedirectToAction("Index");
                }

                var thietBi = await response.Content.ReadFromJsonAsync<ThietBiBan>();

                var ngayHetHan = thietBi.NgayMua.AddMonths(thietBi.ThoiHanBaoHanhThang);
                var conBaoHanh = ngayHetHan > DateTime.UtcNow.AddHours(7);

                var warrantyViewModel = new WarrantyCheckViewModel
                {
                    IsValid = conBaoHanh,
                    EndDate = ngayHetHan.ToString("dd/MM/yyyy"),
                    Model = thietBi.Model,
                    PurchaseDate = thietBi.NgayMua.ToString("dd/MM/yyyy"),
                    SerialNumber = query.Trim()
                };

                return RedirectToAction("Warranty", warrantyViewModel);
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> TraCuu(string? id = null, string? q = null)
        {
            // Ưu tiên id, nếu không có thì dùng q
            var searchTerm = id ?? q;
            
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction("Index");
            }

            var client = _httpClientFactory.CreateClient("TechProAPI");

            // Tim truc tiep theo ID neu co
            PhieuSuaChua? phieu = null;
            if (!string.IsNullOrEmpty(id))
            {
                 var response = await client.GetAsync($"api/TiepNhan/{id}");
                 if (response.IsSuccessStatusCode)
                 {
                     phieu = await response.Content.ReadFromJsonAsync<PhieuSuaChua>();
                 }
            }
            else
            {
                // Dung search API
                var response = await client.GetAsync($"api/TiepNhan/search?query={Uri.EscapeDataString(q)}");
                if (response.IsSuccessStatusCode)
                {
                    phieu = await response.Content.ReadFromJsonAsync<PhieuSuaChua>();
                }
            }

            if (phieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin phiếu sửa chữa này.";
                return RedirectToAction("Index");
            }

            // Calculate costs
            // Note: The logic for calculation should be moved to API ideally (e.g. Calculated Properties on DTO), 
            // but for now keeping it here with data from API is fine.
            var partsCost = phieu.YeuCauLinhKiens?
                .Where(y => y.TrangThai == "approved")
                .Sum(y => (y.GiaTaiThoiDiemYeuCau ?? 0) * y.SoLuong) ?? 0;
            var serviceFee = phieu.CoBaoHanh == true ? 0 : 200000;
            var totalCost = partsCost + serviceFee;

            ViewBag.PartsCost = partsCost;
            ViewBag.ServiceFee = serviceFee;
            ViewBag.TotalCost = totalCost;
            
            if (phieu.YeuCauLinhKiens != null)
            {
                var partsList = phieu.YeuCauLinhKiens
                    .Where(y => y.TrangThai == "approved")
                    .Select(y => new { Name = y.LinhKien?.TenLinhKien ?? "N/A", Price = y.GiaTaiThoiDiemYeuCau ?? 0, Quantity = y.SoLuong })
                    .ToList();
                ViewBag.PartsList = partsList;
            }
            else
            {
                ViewBag.PartsList = new List<object>();
            }

            return View(phieu);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult DatLich()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLich(LichHen model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Ghép Date + Time vào NgayHen nếu người dùng gửi date và giờ tách
            if (model.NgayHen.Date == DateTime.MinValue.Date)
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn ngày hẹn.");
                return View(model);
            }

            var client = _httpClientFactory.CreateClient("TechProAPI");
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Bookings", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["BookingSuccess"] = "Đã đặt lịch thành công! Chúng tôi sẽ liên hệ xác nhận.";
                return RedirectToAction("DatLich");
            }
            else
            {
                ModelState.AddModelError("", "Có lỗi xảy ra khi đặt lịch.");
                return View(model);
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanBaoGia(string ticketId)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsync($"api/TiepNhan/{ticketId}/XacNhanBaoGia", null);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Đã xác nhận báo giá." });
            }
            else
            {
                 // Try read message
                 // var msg = await response.Content.ReadAsStringAsync();
                 return Json(new { success = false, message = "Lỗi khi xác nhận báo giá." });
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Warranty(WarrantyCheckViewModel? warrantyInfo)
        {
            if (warrantyInfo == null)
            {
                return RedirectToAction("Index");
            }
            return View(warrantyInfo);
        }
    }
}
