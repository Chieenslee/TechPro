using Microsoft.AspNetCore.Mvc;
using TechPro.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace TechPro.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    [Route("SysAdmin/[controller]/{action=Index}/{id?}")]
    [Route("Chain/{action=Index}/{id?}")] // Back-compat for JS hardcoded '/Chain/*'
    public class ChainController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public ChainController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        private HttpClient CreateClient()
        {
            return _clientFactory.CreateClient("TechProAPI");
        }

        private static string NextStoreId(IEnumerable<CuaHang> stores)
        {
            // Prefer existing STORE-xxx pattern; fallback to timestamp-based
            var max = stores
                .Select(s => s.Id)
                .Where(id => !string.IsNullOrWhiteSpace(id) && id.StartsWith("STORE-", StringComparison.OrdinalIgnoreCase))
                .Select(id =>
                {
                    var part = id.Substring("STORE-".Length);
                    return int.TryParse(part, out var n) ? n : (int?)null;
                })
                .Where(n => n.HasValue)
                .Select(n => n!.Value)
                .DefaultIfEmpty(0)
                .Max();

            var next = max + 1;
            if (next <= 999) return $"STORE-{next:D3}";
            return "STORE-" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        }

        // GET: Chain
        public async Task<IActionResult> Index(string tab = "overview")
        {
            var client = CreateClient();
            var response = await client.GetAsync("api/Chain/dashboard");
            
            var vm = new TechPro.Models.ViewModels.ChainDashboardViewModel
            {
                ActiveTab = tab,
                Stores = new List<CuaHang>(),
                Users = new List<NguoiDung>(),
                ActiveStoresCount = 0,
                TotalRevenue = 0,
                TotalStaff = 0,
                InventoryItems = new List<KhoLinhKien>()
            };

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var dashData = JsonSerializer.Deserialize<TechPro.Models.DTOs.ChainDashboardDto>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (dashData != null)
                {
                    vm.Stores = dashData.Stores;
                    vm.Users = dashData.Users;
                    vm.ActiveStoresCount = vm.Stores.Count(s => s.TrangThai == "active");
                    vm.TotalRevenue = dashData.TotalRevenue;
                    vm.TotalStaff = dashData.TotalStaff;
                    
                    ViewBag.ThisMonthStoreRevenues = dashData.ThisMonthStoreRevenues;
                    ViewBag.UsersWithRoles = dashData.UsersWithRoles;
                }
            }
            else
            {
                ViewBag.UsersWithRoles = new List<TechPro.Models.DTOs.UserWithRoleDto>();
                ViewBag.ThisMonthStoreRevenues = new List<TechPro.Models.DTOs.StoreRevenueDto>();
            }

            if (tab == "inventory")
            {
                var invResp = await client.GetAsync("api/Inventory/dashboard");
                if (invResp.IsSuccessStatusCode)
                {
                    var invContent = await invResp.Content.ReadAsStringAsync();
                    var invData = JsonSerializer.Deserialize<TechPro.Models.DTOs.InventoryDashboardDto>(invContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (invData?.Inventory != null)
                    {
                        vm.InventoryItems = invData.Inventory;
                    }
                }
            }
            else if (tab == "overview")
            {
                // System alerts: low stock across system
                var lowStockResp = await client.GetAsync("api/Inventory/low-stock?threshold=5");
                if (lowStockResp.IsSuccessStatusCode)
                {
                    var body = await lowStockResp.Content.ReadAsStringAsync();
                    // Keep as raw JSON for view to render quickly
                    ViewBag.LowStockJson = body;
                }
                else
                {
                    ViewBag.LowStockJson = "{\"success\":false,\"data\":[]}";
                }
            }

            return View(vm);
        }

        // ===========================
        // AJAX endpoints used by UI
        // ===========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoCuaHang(string? TenCuaHang, string? DiaChi, string? Hotline, string? AdminEmail)
        {
            if (string.IsNullOrWhiteSpace(TenCuaHang))
                return Json(new { success = false, message = "Thiếu tên cửa hàng." });

            var client = CreateClient();

            // Determine next store id
            var storesResp = await client.GetAsync("api/Chain");
            var stores = new List<CuaHang>();
            if (storesResp.IsSuccessStatusCode)
            {
                var content = await storesResp.Content.ReadAsStringAsync();
                stores = JsonSerializer.Deserialize<List<CuaHang>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
            }

            var store = new CuaHang
            {
                Id = NextStoreId(stores),
                TenCuaHang = TenCuaHang.Trim(),
                DiaChi = string.IsNullOrWhiteSpace(DiaChi) ? null : DiaChi.Trim(),
                Hotline = string.IsNullOrWhiteSpace(Hotline) ? null : Hotline.Trim(),
                AdminEmail = string.IsNullOrWhiteSpace(AdminEmail) ? null : AdminEmail.Trim(),
                TrangThai = "active"
            };

            var json = JsonSerializer.Serialize(store);
            var body = new StringContent(json, Encoding.UTF8, "application/json");
            var resp = await client.PostAsync("api/Chain", body);

            if (resp.IsSuccessStatusCode)
                return Json(new { success = true, message = $"Đã tạo cửa hàng {store.Id}." });

            var err = await resp.Content.ReadAsStringAsync();
            return Json(new { success = false, message = string.IsNullOrWhiteSpace(err) ? "Không tạo được cửa hàng." : err });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaCuaHang(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return Json(new { success = false, message = "Thiếu mã cửa hàng." });

            var client = CreateClient();
            var resp = await client.DeleteAsync($"api/Chain/{Uri.EscapeDataString(id)}");
            return Json(new
            {
                success = resp.IsSuccessStatusCode,
                message = resp.IsSuccessStatusCode ? "Đã xóa cửa hàng." : "Không xóa được cửa hàng."
            });
        }

        public record CreateStaffRequest(string email, string tenDayDu, string role, string tenantId);
        public record TransferStaffRequest(string userId, string targetTenantId);
        public record UpdatePriceRequest(string partId, decimal giaBan);

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TaoNhanVien([FromForm] CreateStaffRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.email) || string.IsNullOrWhiteSpace(request.tenDayDu) || string.IsNullOrWhiteSpace(request.role))
                return Json(new { success = false, message = "Thiếu thông tin nhân viên." });

            var client = CreateClient();
            var resp = await client.PostAsJsonAsync("api/AdminUsers", request);
            if (resp.IsSuccessStatusCode)
            {
                var msg = await resp.Content.ReadAsStringAsync();
                return Json(new { success = true, message = string.IsNullOrWhiteSpace(msg) ? "Đã tạo nhân viên." : msg });
            }
            return Json(new { success = false, message = await resp.Content.ReadAsStringAsync() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DieuChuyenNhanVien([FromForm] TransferStaffRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.userId) || string.IsNullOrWhiteSpace(request.targetTenantId))
                return Json(new { success = false, message = "Thiếu thông tin điều chuyển." });

            var client = CreateClient();
            var resp = await client.PostAsJsonAsync("api/AdminUsers/transfer", request);
            return Json(new
            {
                success = resp.IsSuccessStatusCode,
                message = resp.IsSuccessStatusCode ? "Đã điều chuyển." : (await resp.Content.ReadAsStringAsync())
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaNhanVien(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return Json(new { success = false, message = "Thiếu userId." });

            var client = CreateClient();
            var resp = await client.DeleteAsync($"api/AdminUsers/{Uri.EscapeDataString(userId)}");
            return Json(new
            {
                success = resp.IsSuccessStatusCode,
                message = resp.IsSuccessStatusCode ? "Đã xóa nhân viên." : "Không xóa được nhân viên."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatGiaChuan([FromForm] UpdatePriceRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.partId) || request.giaBan < 0)
                return Json(new { success = false, message = "Dữ liệu giá không hợp lệ." });

            var client = CreateClient();
            var resp = await client.PutAsJsonAsync($"api/Inventory/price/{Uri.EscapeDataString(request.partId)}", new { giaBan = request.giaBan });
            return Json(new
            {
                success = resp.IsSuccessStatusCode,
                message = resp.IsSuccessStatusCode ? "Đã đồng bộ giá." : (await resp.Content.ReadAsStringAsync())
            });
        }

        // GET: Chain/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();

            var client = CreateClient();
            var response = await client.GetAsync($"api/Chain/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CuaHang>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(data);
            }
            return NotFound();
        }

        // GET: Chain/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Chain/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenCuaHang,DiaChi,SoDienThoai")] CuaHang cuaHang)
        {
            if (ModelState.IsValid)
            {
                var client = CreateClient();
                var json = JsonSerializer.Serialize(cuaHang);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/Chain", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(cuaHang);
        }

        // GET: Chain/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();

            var client = CreateClient();
            var response = await client.GetAsync($"api/Chain/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CuaHang>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(data);
            }
            return NotFound();
        }

        // POST: Chain/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,TenCuaHang,DiaChi,SoDienThoai")] CuaHang cuaHang)
        {
            if (id != cuaHang.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var client = CreateClient();
                var json = JsonSerializer.Serialize(cuaHang);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"api/Chain/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return View(cuaHang);
        }

        // GET: Chain/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();

            var client = CreateClient();
            var response = await client.GetAsync($"api/Chain/{id}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<CuaHang>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(data);
            }
            return NotFound();
        }

        // POST: Chain/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var client = CreateClient();
            var response = await client.DeleteAsync($"api/Chain/{id}");
            return RedirectToAction(nameof(Index));
        }
    }
}
