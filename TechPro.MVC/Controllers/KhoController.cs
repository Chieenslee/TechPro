using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechPro.Models.DTOs;

namespace TechPro.Controllers
{
    // Support can only VIEW inventory. StoreAdmin+ can manage (approve/reject).
    [Authorize(Roles = "Support,StoreAdmin,SystemAdmin")]
    [Route("Support/[controller]/{action=Index}/{id?}")]
    public class KhoController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public KhoController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [Authorize(Roles = "Support,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> Index(string? tab = "inventory", string? searchTerm = null)
        {
            ViewBag.ActiveTab = tab ?? "inventory";
            ViewBag.SearchTerm = searchTerm;

            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.GetAsync($"api/Inventory/dashboard?searchTerm={Uri.EscapeDataString(searchTerm ?? "")}");

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
            else
            {
                // Handle error or empty
                ViewBag.Inventory = new List<object>();
                ViewBag.PartRequests = new List<object>();
                ViewBag.WasteReturns = new List<object>();
            }

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "StoreAdmin,SystemAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyetYeuCau(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsync($"api/Inventory/approve/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Đã duyệt và xuất kho thành công!" });
            }
            
            var error = await response.Content.ReadFromJsonAsync<dynamic>(); // Or generic error object
            // Just return fail
            return Json(new { success = false, message = "Lỗi khi duyệt yêu cầu." });
        }

        [HttpPost]
        [Authorize(Roles = "StoreAdmin,SystemAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoiYeuCau(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsync($"api/Inventory/reject/{id}", null);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Đã từ chối yêu cầu." });
            }
             return Json(new { success = false, message = "Lỗi khi từ chối." });
        }

        [HttpPost]
        [Authorize(Roles = "StoreAdmin,SystemAdmin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanTraXac(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PostAsync($"api/Inventory/confirm-waste/{id}", null);

           if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true, message = "Đã xác nhận nhận xác linh kiện." });
            }
            return Json(new { success = false, message = "Lỗi khi xác nhận." });
        }

        [HttpGet]
        [Authorize(Roles = "StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GetLowStockItems(int threshold = 5)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            // Pass tenantId via header or let API infer from (Not implemented in Auth yet, need to forward token)
            // But we use Cookie Auth in MVC, API doesn't know User unless we pass a token.
            // Since we implemented "Login" in MVC by verifying against API, the cookie is LOCAL.
            // The API calls from MVC are SERVER-TO-SERVER (HttpClient).
            // Currently API endpoints are OPEN (Secure? No).
            // This is a common refactor gap.
            // For now, allow API to be open or trust localhost.
            // Long term: Use Client Credentials or Forward User Context.
            
            var response = await client.GetAsync($"api/Inventory/low-stock?threshold={threshold}");
             if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<dynamic>(); // { success: true, data: [...] }
                return Json(data);
            }
            return Json(new { success = false });
        }
    }
}
