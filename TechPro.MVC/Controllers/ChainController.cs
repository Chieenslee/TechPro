using Microsoft.AspNetCore.Mvc;
using TechPro.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace TechPro.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    [Route("SysAdmin/[controller]/{action=Index}/{id?}")]
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

            return View(vm);
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
