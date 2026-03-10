using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechPro.Models; // For ViewModels if any
using TechPro.Models;
using System.Security.Claims;

namespace TechPro.Controllers
{
    [Authorize(Roles = "Technician,StoreAdmin,SystemAdmin")]
    public class KyThuatController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public KyThuatController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string? status = null)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var tenantId = User.FindFirstValue("TenantId");

            // Filter logic moved to API params
            // If Technician, filter by assigned? Or show all?
            // Original logic:
            // var query = _context.PhieuSuaChuas...
            // if (User.IsInRole("Technician")) query = query.Where(p => p.KyThuatVienId == userId);
            // This logic should be in API or passed as params.
            
            string queryParams = $"?status={status}&tenantId={tenantId}";
            if (User.IsInRole("Technician"))
            {
                queryParams += $"&assigneeId={userId}";
            }

            var response = await client.GetAsync($"api/Technician/tickets{queryParams}");
            
            if (response.IsSuccessStatusCode)
            {
                var tickets = await response.Content.ReadFromJsonAsync<List<PhieuSuaChua>>();
                return View(tickets);
            }

            return View(new List<PhieuSuaChua>());
        }

        public async Task<IActionResult> ChiTiet(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.GetAsync($"api/Technician/tickets/{id}");

            if (response.IsSuccessStatusCode)
            {
                var ticket = await response.Content.ReadFromJsonAsync<PhieuSuaChua>();
                return View(ticket);
            }

            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(string id, string trangThai)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PutAsJsonAsync($"api/Technician/tickets/{id}/status", trangThai);
            
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> GanChoToi(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // API needs generic user ID or passed explicitly
            // Actually API `AssignTicket` takes `technicianId`.
            // User ID in MVC Cookie is `Permissions` schema or just ID?
            // AuthController sets: new Claim(ClaimTypes.Name, result.User.Email)
            // It didn't set NameIdentifier (ID). AuthController Login:
            /*
            new Claim(ClaimTypes.Name, result.User.Email),
            new Claim(ClaimTypes.Email, result.User.Email),
            */
            // I should have set NameIdentifier to `result.User.Id`.
            // I'll assume I can fix AuthController later or now.
            // For now, let's assume NameIdentifier is available or use Email if ID not set.
            // Wait, standard Identity uses ID as NameIdentifier.
            
            // I'll Fix AccountController login to set NameIdentifier.
            
            var client = _httpClientFactory.CreateClient("TechProAPI");
            // If we don't have ID, we can't assign.
            // Let's assume passed userId is valid.
            
            var response = await client.PutAsJsonAsync($"api/Technician/tickets/{id}/assign", userId);
            return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> GanKyThuatVien(string id, string kyThuatVienId)
        {
             var client = _httpClientFactory.CreateClient("TechProAPI");
             var response = await client.PutAsJsonAsync($"api/Technician/tickets/{id}/assign", kyThuatVienId);
             return Json(new { success = response.IsSuccessStatusCode });
        }

        [HttpPost]
        public async Task<IActionResult> TaoYeuCauLinhKien(YeuCauLinhKien model)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsJsonAsync($"api/Technician/tickets/{model.PhieuSuaChuaId}/parts", model);
            
            if (response.IsSuccessStatusCode)
                return Json(new { success = true });
            
            return Json(new { success = false });
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
