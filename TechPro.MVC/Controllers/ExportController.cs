using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using System.Text;
using System.Text.Json;
using TechPro.Models;

namespace TechPro.Controllers
{
    [Authorize(Roles = "StoreAdmin,SystemAdmin")]
    public class ExportController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ExportController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Content("Export module ready.");
        }

        private HttpClient Client() => _httpClientFactory.CreateClient("TechProAPI");

        [HttpGet]
        [Route("StoreAdmin/Export/ExportTicketsCsv")]
        public async Task<IActionResult> ExportTicketsCsv(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            var resp = await Client().GetAsync($"api/QuanLy/ExportTickets?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");
            if (!resp.IsSuccessStatusCode) return BadRequest("Không tải được dữ liệu phiếu.");

            var json = await resp.Content.ReadAsStringAsync();
            var tickets = JsonSerializer.Deserialize<List<PhieuSuaChua>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            var sb = new StringBuilder();
            sb.AppendLine("Id,NgayNhan,TenKhachHang,SoDienThoai,TenThietBi,TrangThai,TongTien");
            foreach (var t in tickets)
            {
                sb.AppendLine(string.Join(",",
                    Csv(t.Id),
                    Csv(t.NgayNhan.ToString("yyyy-MM-dd HH:mm")),
                    Csv(t.TenKhachHang),
                    Csv(t.SoDienThoai),
                    Csv(t.TenThietBi),
                    Csv(t.TrangThai),
                    Csv(t.TongTien.ToString("0"))));
            }

            var bytes = Encoding.UTF8.GetBytes("\uFEFF" + sb.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"tickets_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
        }

        [HttpGet]
        [Route("StoreAdmin/Export/ExportRevenueCsv")]
        public async Task<IActionResult> ExportRevenueCsv(DateTime? fromDate, DateTime? toDate)
        {
            var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
            var to = toDate ?? DateTime.UtcNow;

            var resp = await Client().GetAsync($"api/QuanLy/ExportRevenueDaily?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}");
            if (!resp.IsSuccessStatusCode) return BadRequest("Không tải được dữ liệu doanh thu.");

            var json = await resp.Content.ReadAsStringAsync();
            var points = JsonSerializer.Deserialize<List<RevenueDataPoint>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

            var sb = new StringBuilder();
            sb.AppendLine("Ngay,DoanhThu");
            foreach (var p in points)
                sb.AppendLine($"{Csv(p.Name)},{Csv(p.Value.ToString("0"))}");

            var bytes = Encoding.UTF8.GetBytes("\uFEFF" + sb.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"revenue_{from:yyyyMMdd}_{to:yyyyMMdd}.csv");
        }

        [HttpGet]
        [Route("StoreAdmin/Export/ExportInventoryCsv")]
        public async Task<IActionResult> ExportInventoryCsv()
        {
            var resp = await Client().GetAsync("api/Inventory/dashboard");
            if (!resp.IsSuccessStatusCode) return BadRequest("Không tải được dữ liệu kho.");

            var json = await resp.Content.ReadAsStringAsync();
            var dto = JsonSerializer.Deserialize<TechPro.Models.DTOs.InventoryDashboardDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            var items = dto?.Inventory ?? new List<KhoLinhKien>();

            var sb = new StringBuilder();
            sb.AppendLine("Id,TenLinhKien,DanhMuc,GiaBan,SoLuongTon,TenantId");
            foreach (var i in items)
            {
                sb.AppendLine(string.Join(",",
                    Csv(i.Id),
                    Csv(i.TenLinhKien),
                    Csv(i.DanhMuc),
                    Csv(i.GiaBan.ToString("0")),
                    Csv(i.SoLuongTon.ToString()),
                    Csv(i.TenantId)));
            }

            var bytes = Encoding.UTF8.GetBytes("\uFEFF" + sb.ToString());
            return File(bytes, "text/csv; charset=utf-8", $"inventory_{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        private static string Csv(string? s)
        {
            s ??= "";
            s = s.Replace("\"", "\"\"");
            return $"\"{s}\"";
        }
    }
}
