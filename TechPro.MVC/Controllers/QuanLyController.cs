using Microsoft.AspNetCore.Mvc;
using TechPro.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace TechPro.Controllers
{
    [Authorize(Roles = "StoreAdmin,SystemAdmin")]
    [Route("StoreAdmin/[controller]/{action=Index}/{id?}")]
    public class QuanLyController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public QuanLyController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient CreateClient()
        {
            return _clientFactory.CreateClient("TechProAPI");
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateClient();
            var response = await client.GetAsync("api/QuanLy/DashboardStats");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var viewModel = JsonSerializer.Deserialize<DashboardViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(viewModel);
            }

            return View(new DashboardViewModel());
        }

        public IActionResult BaoCao()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CauHinh()
        {
            var client = CreateClient();
            var tenantId = User.FindFirst("TenantId")?.Value ?? "STORE-001";
            var response = await client.GetAsync($"api/Chain/{tenantId}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var cuaHang = JsonSerializer.Deserialize<CuaHang>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(cuaHang);
            }

            return View(new CuaHang());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CauHinh(CuaHang model)
        {
            var client = CreateClient();
            var tenantId = User.FindFirst("TenantId")?.Value ?? "STORE-001";
            model.Id = tenantId; // ensure id matches
            
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/Chain/{tenantId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Đã lưu cấu hình cửa hàng thành công.";
            }
            else
            {
                ModelState.AddModelError("", "Lỗi khi lưu cấu hình.");
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetReportSummary(DateTime? fromDate, DateTime? toDate)
        {
            var client = CreateClient();
            var url = $"api/QuanLy/ReportSummary?fromDate={fromDate:yyyy-MM-dd}&toDate={toDate:yyyy-MM-dd}";
            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var summary = JsonSerializer.Deserialize<ReportSummaryViewModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return Json(new { success = true, data = summary });
            }

            return Json(new { success = false });
        }
    }
}
