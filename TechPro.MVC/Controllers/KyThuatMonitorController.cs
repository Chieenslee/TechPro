using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechPro.Models;

namespace TechPro.Controllers
{
    /// <summary>
    /// KyThuat monitor cho StoreAdmin/SystemAdmin — xem & phân công, không thao tác như Technician.
    /// URL: /StoreAdmin/KyThuat/...
    /// </summary>
    [Authorize(Roles = "StoreAdmin,SystemAdmin")]
    [Route("StoreAdmin/KyThuat/{action=Index}/{id?}")]
    public class KyThuatMonitorController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public KyThuatMonitorController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string? status = null, string? searchTerm = null)
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = status ?? "all";

            var client = _httpClientFactory.CreateClient("TechProAPI");
            var tenantId = User.FindFirstValue("TenantId");

            string queryParams = $"?status={status}&tenantId={tenantId}";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                queryParams += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
            }

            var response = await client.GetAsync($"api/Technician/tickets{queryParams}");

            if (response.IsSuccessStatusCode)
            {
                var tickets = await response.Content.ReadFromJsonAsync<List<PhieuSuaChua>>();
                return View("Index", tickets);
            }

            return View("Index", new List<PhieuSuaChua>());
        }

        public async Task<IActionResult> ChiTiet(string id)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.GetAsync($"api/Technician/tickets/{id}");

            if (response.IsSuccessStatusCode)
            {
                var ticket = await response.Content.ReadFromJsonAsync<PhieuSuaChua>();
                return View("ChiTiet", ticket);
            }

            return NotFound();
        }

        // Only StoreAdmin/SystemAdmin can assign tickets to specific technicians
        [HttpPost]
        public async Task<IActionResult> GanKyThuatVien(string id, string kyThuatVienId)
        {
            var client = _httpClientFactory.CreateClient("TechProAPI");
            var response = await client.PutAsJsonAsync($"api/Technician/tickets/{id}/assign", kyThuatVienId);
            return Json(new { success = response.IsSuccessStatusCode });
        }
    }
}

