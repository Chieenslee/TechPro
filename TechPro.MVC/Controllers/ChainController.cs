using Microsoft.AspNetCore.Mvc;
using TechPro.Models;
using System.Text.Json;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace TechPro.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
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
            var response = await client.GetAsync("api/Chain");
            var stores = new List<CuaHang>();
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                stores = JsonSerializer.Deserialize<List<CuaHang>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<CuaHang>();
            }

            var vm = new TechPro.Models.ViewModels.ChainDashboardViewModel
            {
                ActiveTab = tab,
                Stores = stores,
                ActiveStoresCount = stores.Count(s => s.TrangThai == "active"),
                TotalRevenue = 500000000, // Static mock for now
                TotalStaff = 15, // Static mock for now
                InventoryItems = new List<KhoLinhKien>() // Empty for now unless tab == inventory
            };

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

            ViewBag.UsersWithRoles = new List<dynamic>(); // Prevent null ref in view
            ViewBag.ThisMonthStoreRevenues = new List<dynamic>(); // Prevent null ref in view

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
