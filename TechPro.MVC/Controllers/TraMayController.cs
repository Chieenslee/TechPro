using Microsoft.AspNetCore.Mvc;
using TechPro.Models;
using System.Text.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace TechPro.Controllers
{
    [Authorize(Roles = "Support,StoreAdmin,SystemAdmin")]
    [Route("Support/[controller]/{action=Index}/{id?}")]
    public class TraMayController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public TraMayController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient CreateClient()
        {
            return _clientFactory.CreateClient("TechProAPI");
        }

        public async Task<IActionResult> Index(string searchTerm, string status = "done")
        {
            ViewBag.SearchTerm = searchTerm;
            ViewBag.StatusFilter = status;

            var client = CreateClient();
            var url = "api/TiepNhan";
            
            var queryParams = new List<string>();
            if (!string.IsNullOrEmpty(searchTerm)) queryParams.Add($"searchTerm={Uri.EscapeDataString(searchTerm)}");
            if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={Uri.EscapeDataString(status)}");

            if (queryParams.Any())
            {
                url += "?" + string.Join("&", queryParams);
            }

            var response = await client.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var tickets = JsonSerializer.Deserialize<List<PhieuSuaChua>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(tickets ?? new List<PhieuSuaChua>());
            }

            return View(new List<PhieuSuaChua>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(string id)
        {
            var client = CreateClient();
            var response = await client.PostAsync($"api/TiepNhan/{id}/ThanhToan", null);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<dynamic>(content);
                return Json(new { success = true, message = result?.GetProperty("message").GetString() ?? "Thanh toán thành công" });
            }

            var errBody = await response.Content.ReadAsStringAsync();
            try {
                var errObj = JsonSerializer.Deserialize<dynamic>(errBody);
                return Json(new { success = false, message = errObj?.GetProperty("message").GetString() ?? "Lỗi thanh toán" });
            } catch {
                return Json(new { success = false, message = "Lỗi khi kết nối với máy chủ thanh toán" });
            }
        }
    }
}
