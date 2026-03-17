using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechPro.Models.DTOs;

namespace TechPro.Controllers
{
    /// <summary>
    /// Module Kho Linh Kiện — domain riêng biệt, không thuộc Support hay StoreAdmin.
    ///   GET /Kho/View    → Support (read-only, xem tồn kho để báo giá)
    ///   GET /Kho/Request → Technician (theo dõi yêu cầu linh kiện của mình)
    ///   GET /Kho/Manage  → StoreAdmin (duyệt, từ chối, cảnh báo tồn kho)
    /// </summary>
    [Route("Kho")]
    public class KhoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public KhoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient Client()
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            // Forward caller identity — API dùng để ghi Audit Log
            var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "unknown";
            var role  = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value  ?? "unknown";
            client.DefaultRequestHeaders.Remove("X-Caller-Email");
            client.DefaultRequestHeaders.Remove("X-Caller-Role");
            client.DefaultRequestHeaders.Add("X-Caller-Email", email);
            client.DefaultRequestHeaders.Add("X-Caller-Role",  role);
            return client;
        }

        // ════════════════════════════════════════════════════════════
        // VIEW — Support: xem tồn kho để báo giá (read-only)
        // ════════════════════════════════════════════════════════════
        [HttpGet("View")]
        [Authorize(Roles = "Support")]   // Lễ tân xem tồn kho để báo giá — chỉ đọc
        [ActionName("View")]
        public async Task<IActionResult> KhoView(string? searchTerm = null)
        {
            ViewBag.SearchTerm = searchTerm;
            var response = await Client().GetAsync($"api/Inventory/dashboard?searchTerm={Uri.EscapeDataString(searchTerm ?? "")}");
            if (response.IsSuccessStatusCode)
            {
                var dto = await response.Content.ReadFromJsonAsync<InventoryDashboardDto>();
                if (dto != null) ViewBag.Inventory = dto.Inventory;
            }
            ViewBag.Inventory ??= new List<object>();
            return View("View");
        }

        // ⚠️  /Kho/Request KHÔNG TỒN TẠI — Technician không được vào domain /Kho/*.
        // Technician xem yêu cầu của mình tại: /Technician/KyThuat/LinhKien (domain riêng)

        // ════════════════════════════════════════════════════════════
        // MANAGE — Storekeeper / StoreAdmin: duyệt/từ chối/nhập-xuất/cảnh báo
        // ════════════════════════════════════════════════════════════
        [HttpGet("Manage")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        [ActionName("Manage")]
        public async Task<IActionResult> KhoManage(string? tab = "requests", string? searchTerm = null)
        {
            ViewBag.ActiveTab = tab;
            ViewBag.SearchTerm = searchTerm;
            var response = await Client().GetAsync($"api/Inventory/dashboard?searchTerm={Uri.EscapeDataString(searchTerm ?? "")}");
            if (response.IsSuccessStatusCode)
            {
                var dto = await response.Content.ReadFromJsonAsync<InventoryDashboardDto>();
                if (dto != null)
                {
                    ViewBag.Inventory = dto.Inventory;
                    ViewBag.PartRequests = dto.PartRequests;
                    ViewBag.WasteReturns = dto.WasteReturns;
                    ViewBag.PendingRequestsCount = dto.PendingRequestsCount;
                    ViewBag.PendingWasteCount = dto.PendingWasteCount;
                }
            }
            ViewBag.Inventory ??= new List<object>();
            ViewBag.PartRequests ??= new List<object>();
            ViewBag.WasteReturns ??= new List<object>();
            return View("Manage");
        }

        // ════════════════════════════════════════════════════════════
        // API ACTIONS — StoreAdmin only
        // ════════════════════════════════════════════════════════════

        [HttpPost("DuyetYeuCau")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetYeuCau(string id)
        {
            var response = await Client().PostAsync($"api/Inventory/approve/{id}", null);
            var body = await response.Content.ReadAsStringAsync();
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode
                    ? "Đã duyệt và xuất kho thành công!"
                    : (string.IsNullOrWhiteSpace(body) ? "Lỗi khi duyệt yêu cầu." : body)
            });
        }

        [HttpPost("TuChoiYeuCau")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiYeuCau(string id)
        {
            var response = await Client().PostAsync($"api/Inventory/reject/{id}", null);
            var body = await response.Content.ReadAsStringAsync();
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode
                    ? "Đã từ chối yêu cầu."
                    : (string.IsNullOrWhiteSpace(body) ? "Lỗi khi từ chối." : body)
            });
        }

        [HttpPost("XacNhanTraXac")]
        [Authorize(Roles = "Storekeeper,StoreAdmin,SystemAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanTraXac(string id)
        {
            var response = await Client().PostAsync($"api/Inventory/confirm-waste/{id}", null);
            var body = await response.Content.ReadAsStringAsync();
            return Json(new
            {
                success = response.IsSuccessStatusCode,
                message = response.IsSuccessStatusCode
                    ? "Đã xác nhận nhận xác linh kiện."
                    : (string.IsNullOrWhiteSpace(body) ? "Lỗi khi xác nhận." : body)
            });
        }

        [HttpGet("GetLowStockItems")]
        [Authorize(Roles = "StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GetLowStockItems(int threshold = 5)
        {
            var response = await Client().GetAsync($"api/Inventory/low-stock?threshold={threshold}");
            if (response.IsSuccessStatusCode)
                return Content(await response.Content.ReadAsStringAsync(), "application/json");
            return Json(new { success = false });
        }
    }
}
