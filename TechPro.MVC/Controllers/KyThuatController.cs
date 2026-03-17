using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechPro.Models;
using TechPro.Models.ViewModels;
using System.Security.Claims;

namespace TechPro.Controllers
{
    // Technician = tất cả nghiệp vụ kỹ thuật (xem, cập nhật trạng thái, ghi chú)
    // Hỗ trợ cả route cũ (/KyThuat/...) và route mới có prefix (/Technician/KyThuat/...)
    [Authorize]
    [Route("[controller]/{action=Index}/{id?}")]
    [Route("Technician/[controller]/{action=Index}/{id?}")]
    public class KyThuatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public KyThuatController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string? status = null, string? searchTerm = null)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = status ?? "all";
            
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tenantId = User.FindFirstValue("TenantId");

            // Technician thấy: phiếu chưa gán (pending) + phiếu gán cho mình trong cùng Tenant
            // Không filter assigneeId ở đây — API sẽ trả về tất cả và UI sẽ highlight phiếu của họ
            string queryParams = $"?tenantId={tenantId}";
            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                queryParams += $"&status={status}";
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await client.GetAsync($"api/Technician/tickets{queryParams}");
            
            if (response.IsSuccessStatusCode)
            {
                var tickets = await response.Content.ReadFromJsonAsync<List<PhieuSuaChua>>();
                // View (`Views/KyThuat/Index.cshtml`) currently expects these keys
                ViewBag.UserId = userId;
                ViewBag.UserRole = "Technician";
                return View(tickets ?? new List<PhieuSuaChua>());
            }

            return View(new List<PhieuSuaChua>());

        }

        public async Task<IActionResult> ChiTiet(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.GetAsync($"api/Technician/tickets/{id}");

            if (!response.IsSuccessStatusCode)
            {
                return NotFound();
            }

            var ticket = await response.Content.ReadFromJsonAsync<PhieuSuaChua>() ?? new PhieuSuaChua();

            // Nạp thêm danh sách kỹ thuật viên & linh kiện cho tab Phân công + Linh kiện
            try
            {
                // Danh sách kỹ thuật viên (dùng chung với StoreAdmin/Support)
                var staffResponse = await client.GetAsync("api/AdminUsers/technicians");
                if (staffResponse.IsSuccessStatusCode)
                {
                    var techs = await staffResponse.Content.ReadFromJsonAsync<List<NguoiDung>>();
                    ViewBag.Technicians = techs ?? new List<NguoiDung>();
                }

                // Danh sách linh kiện trong kho (lấy từ dashboard inventory)
                var invResponse = await client.GetAsync("api/Inventory/dashboard");
                if (invResponse.IsSuccessStatusCode)
                {
                    var json = await invResponse.Content.ReadFromJsonAsync<InventoryDashboardView>();
                    ViewBag.LinhKiens = json?.Inventory ?? new List<KhoLinhKien>();
                }
            }
            catch
            {
                ViewBag.Technicians = ViewBag.Technicians ?? new List<NguoiDung>();
                ViewBag.LinhKiens = ViewBag.LinhKiens ?? new List<KhoLinhKien>();
            }

            return View(ticket);
        }

        // Chỉ Technician mới được cập nhật trạng thái phiếu (phần việc của họ)
        [Authorize(Roles = "Technician")]
        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(string id, string trangThai)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PutAsJsonAsync($"api/Technician/tickets/{id}/status", trangThai);
            string? message = null;
            try
            {
                var body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("message", out var m) && m.ValueKind == System.Text.Json.JsonValueKind.String)
                        message = m.GetString();
                }
            }
            catch
            {
                // ignore parse errors
            }

            return Json(new { success = response.IsSuccessStatusCode, message });
        }

        // Technician tự nhận phiếu về tay mình
        [HttpPost]
        public async Task<IActionResult> GanChoToi(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsync($"api/Technician/tickets/{id}/self-assign", null);
            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }

            var body = await response.Content.ReadAsStringAsync();
            return Json(new { success = false, message = string.IsNullOrWhiteSpace(body) ? "Không nhận được phiếu." : body });
        }

        // GanChoToi đã bị xóa — Technician không tự nhận phiếu bất kỳ.
        // Chỉ StoreAdmin/SysAdmin mới được gán phiếu qua GanKyThuatVien.
        // Technician chỉ nhìn thấy và xử lý phiếu đã được gán cho mình.

        // Only StoreAdmin/SysAdmin can assign tickets to specific technicians
        [HttpPost]
        [Authorize(Roles = "StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GanKyThuatVien(string id, string kyThuatVienId)
        {
             var client = _httpClientFactory.CreateClient("TechProAPI");
             var response = await client.PutAsJsonAsync($"api/Technician/tickets/{id}/assign", kyThuatVienId);
             return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> TaoYeuCauLinhKien(string ticketId, string partId, int quantity = 1)
        {
            if (string.IsNullOrWhiteSpace(ticketId) || string.IsNullOrWhiteSpace(partId))
            {
                return Json(new { success = false, message = "Thiếu mã phiếu hoặc linh kiện." });
            }

            var model = new YeuCauLinhKien
            {
                PhieuSuaChuaId = ticketId,
                LinhKienId = partId,
                SoLuong = quantity <= 0 ? 1 : quantity,
                TenKyThuatVien = User.Identity?.Name ?? "Technician",
                TrangThai = "pending",
                NgayYeuCau = DateTime.Now
            };

            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsJsonAsync($"api/Technician/tickets/{ticketId}/parts", model);
            
            if (response.IsSuccessStatusCode)
                return Json(new { success = true });

            var body = await response.Content.ReadAsStringAsync();
            return Json(new { success = false, message = string.IsNullOrWhiteSpace(body) ? "Không tạo được yêu cầu." : body });
        }

        [HttpPost]
        public async Task<IActionResult> LuuKetQuaKiemTra(string id, string ketQua)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PutAsJsonAsync($"api/Technician/tickets/{id}/test-result", ketQua);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotes(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.GetAsync($"api/Technician/tickets/{id}/notes");
            if (response.IsSuccessStatusCode)
            {
                 var data = await response.Content.ReadFromJsonAsync<dynamic>();
                 return Json(data);
            }
            return Json(new List<object>());
        }

        [HttpPost]
        public async Task<IActionResult> AddNote(TicketNote note)
        {
            note.UserName = User.Identity?.Name ?? "Unknown";
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsJsonAsync($"api/Technician/tickets/{note.PhieuSuaChuaId}/notes", note);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpGet]
        public async Task<IActionResult> GetScratchMarks(string id)
        {
             var client = _httpClientFactory.CreateClient("TechProAPI");
             var response = await client.GetAsync($"api/Technician/tickets/{id}/scratch-marks");
             if (response.IsSuccessStatusCode)
             {
                 var data = await response.Content.ReadFromJsonAsync<dynamic>();
                 return Json(data);
             }
             return Json(new List<object>());
        }

        [HttpPost]
        public async Task<IActionResult> SaveScratchMarks(string id, [FromBody] List<ScratchMark> marks)
        {
            // Populate CreatedByName for new marks?
            foreach(var m in marks) 
            {
                if(string.IsNullOrEmpty(m.CreatedByName)) m.CreatedByName = User.Identity?.Name;
            }

            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsJsonAsync($"api/Technician/tickets/{id}/scratch-marks", marks);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(string phone, string excludeId)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.GetAsync($"api/Technician/history?phone={Uri.EscapeDataString(phone ?? "")}&excludeId={Uri.EscapeDataString(excludeId ?? "")}");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();
                return Content(data, "application/json");
            }
            return Json(new List<object>());
        }

        // Technician tracks their OWN part requests status
        [HttpGet]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> LinhKien()
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");

            // Forward email của Technician đang đăng nhập — API dùng để filter đúng dữ liệu
            var email = User.FindFirstValue(System.Security.Claims.ClaimTypes.Email) ?? "";
            client.DefaultRequestHeaders.Remove("X-Caller-Email");
            client.DefaultRequestHeaders.Remove("X-Caller-Role");
            client.DefaultRequestHeaders.Add("X-Caller-Email", email);
            client.DefaultRequestHeaders.Add("X-Caller-Role",  "Technician");

            var response = await client.GetAsync("api/Inventory/my-requests");

            if (response.IsSuccessStatusCode)
            {
                var myRequests = await response.Content.ReadFromJsonAsync<List<TechPro.Models.YeuCauLinhKien>>();
                ViewBag.MyPartRequests = myRequests ?? new List<TechPro.Models.YeuCauLinhKien>();
            }
            else
            {
                ViewBag.MyPartRequests = new List<TechPro.Models.YeuCauLinhKien>();
            }

            return View("LinhKien");
        }
        [HttpPost]
        public async Task<IActionResult> AiGopY([FromBody] AiGopYRequest request)
        {
            // Proxy to TiepNhan AnalyzeQuote or a dedicated AI endpoint
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var payload = new { moTaLoi = request.MoTaLoi, tenThietBi = request.TenThietBi };
            var response = await client.PostAsJsonAsync("api/TiepNhan/AnalyzeQuote", payload);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Content(result, "application/json");
            }
            return Json(new { success = false, message = "Không thể kết nối AI." });
        }

        [HttpGet]
        public IActionResult GetChecklist(string model)
        {
            // Built-in checklist database (expandable with DB later)
            var db = new Dictionary<string, List<ChecklistItem>>(StringComparer.OrdinalIgnoreCase)
            {
                ["iphone"] = new()
                {
                    new("Khởi động máy — check boot loop"),
                    new("Thử SIM + gọi đi"),
                    new("Kiểm tra màn hình: chết điểm, đốm sáng"),
                    new("Kiểm tra cảm ứng toàn bộ màn hình"),
                    new("Face ID / Touch ID hoạt động"),
                    new("Camera trước + sau + Flash"),
                    new("Loa ngoài, loa tai nghe, micro"),
                    new("Cổng sạc / sạc không dây"),
                    new("WiFi + Bluetooth + GPS"),
                    new("Tình trạng pin + % hao mòn"),
                    new("Vỏ máy: khe hở, nứt vẹo"),
                    new("Nước — kiểm tra đèn chỉ thị"),
                },
                ["samsung"] = new()
                {
                    new("Khởi động + vào boot menu kiểm tra"),
                    new("Màn hình: burn-in, điểm chết"),
                    new("Bút S-Pen (nếu có)"),
                    new("Cảm ứng vân tay (trong màn / cạnh)"),
                    new("Camera: AI mode, zoom"),
                    new("Loa, micro, kết nối Bluetooth"),
                    new("Sạc nhanh, sạc không dây, OTG"),
                    new("DeX mode (nếu có)"),
                    new("WiFi + 5G"),
                    new("Pin: dung lượng còn, phồng không"),
                    new("Bản lề (fold/flip): mở đóng trơn tru"),
                    new("Nước — kiểm tra rating IP"),
                },
                ["oppo"] = new()
                {
                    new("Khởi động + ColorOS boot"),
                    new("Màn hình AMOLED: đốm, viền burn"),
                    new("VOOC/SuperVOOC sạc nhanh"),
                    new("Camera: OIS, zoom, video 4K"),
                    new("Vân tay màn hình + khuôn mặt"),
                    new("Loa stereo + micro"),
                    new("WiFi + BT + NFC"),
                    new("Pin + nhiệt độ máy"),
                    new("Vỏ máy: trầy xước, cong"),
                },
                ["xiaomi"] = new()
                {
                    new("Khởi động + MIUI/HyperOS"),
                    new("Tình trạng màn hình"),
                    new("Vân tay + khuôn mặt"),
                    new("Sạc nhanh (67W / 120W)"),
                    new("Camera: Leica mode"),
                    new("Loa, micro"),
                    new("WiFi + BT"),
                    new("Pin + %hao mòn"),
                },
                ["laptop"] = new()
                {
                    new("POST + boot từ SSD/HDD"),
                    new("Màn hình: điểm chết, backlight, flicker"),
                    new("Bàn phím + touchpad"),
                    new("CPU/GPU nhiệt độ (HWiNFO)"),
                    new("RAM: memtest pass"),
                    new("SSD: SMART health check"),
                    new("Pin: capacity vs design capacity"),
                    new("Cổng USB / HDMI / USB-C"),
                    new("WiFi + BT"),
                    new("Loa, mic, webcam"),
                    new("Bản lề: mở đóng, vỏ cong"),
                },
            };

            // Fuzzy match against model keyword
            var modelLower = (model ?? "").ToLower();
            var matchedKey = db.Keys.FirstOrDefault(k => modelLower.Contains(k));
            var checklist = matchedKey != null ? db[matchedKey] : new List<ChecklistItem>
            {
                new("Kiểm tra nguồn / boot"),
                new("Màn hình / cảm ứng"),
                new("Âm thanh / micro"),
                new("Camera (nếu có)"),
                new("Kết nối WiFi / BT"),
                new("Pin / Sạc"),
                new("Ngoại quan vỏ máy"),
            };

            return Json(new { success = true, model = matchedKey ?? "generic", items = checklist });
        }
    }
}

public record AiGopYRequest(string TenThietBi, string MoTaLoi);
public record ChecklistItem(string Title, bool Done = false);
