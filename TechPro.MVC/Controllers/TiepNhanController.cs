using Microsoft.AspNetCore.Mvc;
using TechPro.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace TechPro.Controllers
{
    [Authorize(Roles = "Support,StoreAdmin,SystemAdmin")]
    public class TiepNhanController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public TiepNhanController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient CreateClient()
        {
            return _clientFactory.CreateClient("TechProAPI");
        }

        // GET: TiepNhan
        public async Task<IActionResult> Index(string searchTerm, string status = "all")
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
                return View(tickets);
            }

            return View(new List<PhieuSuaChua>());
        }

        // POST: TiepNhan/TaoPhieu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoPhieu(PhieuSuaChua phieuSuaChua)
        {
            /*
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, message = "Dữ liệu không hợp lệ: " + string.Join(", ", errors) });
            }
            */

             // Handle checkboxes for accessories (if specific logic needed, e.g. from Request.Form)
            var accessories = Request.Form["Accessories"];
            if (accessories.Count > 0)
            {
                phieuSuaChua.PhuKien = string.Join(", ", accessories);
            }

            var client = CreateClient();
            var json = JsonSerializer.Serialize(phieuSuaChua);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/TiepNhan/TaoPhieu", content);

            if (response.IsSuccessStatusCode)
            {
                 var responseContent = await response.Content.ReadAsStringAsync();
                 var createdTicket = JsonSerializer.Deserialize<PhieuSuaChua>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                 return Json(new { success = true, message = "Tạo phiếu thành công! Mã: " + createdTicket.Id });
            }

            return Json(new { success = false, message = "Lỗi khi tạo phiếu: " + response.ReasonPhrase });
        }

        [HttpPost]
        public async Task<IActionResult> AnalyzeQuote([FromBody] object request)
        {
            var client = CreateClient();
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/TiepNhan/AnalyzeQuote", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Content(result, "application/json"); // Proxy JSON response
            }

            return Json(new { success = false, message = "Không thể kết nối dịch vụ chuẩn đoán AI." });
        }
    }
}
