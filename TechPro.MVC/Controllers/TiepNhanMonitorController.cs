using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TechPro.Models;

namespace TechPro.Controllers
{
    /// <summary>
    /// TiepNhan monitor cho StoreAdmin/SystemAdmin — chỉ xem & giám sát, không tạo/hủy.
    /// URL: /StoreAdmin/TiepNhan/...
    /// </summary>
    [Authorize(Roles = "StoreAdmin,SystemAdmin")]
    [Route("StoreAdmin/TiepNhan/{action=Index}/{id?}")]
    public class TiepNhanMonitorController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public TiepNhanMonitorController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient CreateClient()
        {
            return _clientFactory.CreateClient("TechProAPI");
        }

        // GET: /StoreAdmin/TiepNhan
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
                return View("Index", tickets ?? new List<PhieuSuaChua>());
            }

            return View("Index", new List<PhieuSuaChua>());
        }

        // GET: /StoreAdmin/TiepNhan/ChiTiet/5
        public async Task<IActionResult> ChiTiet(string id)
        {
            var client = CreateClient();
            var response = await client.GetAsync($"api/Technician/tickets/{id}");

            if (response.IsSuccessStatusCode)
            {
                var ticket = await response.Content.ReadFromJsonAsync<PhieuSuaChua>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View("ChiTiet", ticket ?? new PhieuSuaChua());
            }

            return NotFound();
        }
    }
}

