using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace TechPro.Controllers
{
    [Authorize]
    public class SearchController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public SearchController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Index(string? searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction("Index", "Home");
            }

            var searchTermClean = searchTerm.Trim().ToUpper();
            var role = User.FindFirstValue(ClaimTypes.Role) ?? "Support";

            // Determine base path based on role
            string targetPath = "/Support/TiepNhan";
            
            if (role == "Technician")
            {
                targetPath = "/Technician/KyThuat";
            }
            else if (role == "StoreAdmin" || role == "SystemAdmin")
            {
                // Quản lý tìm kiếm theo danh sách tiếp nhận để giám sát
                targetPath = "/StoreAdmin/TiepNhan";
            }

            // Redirect to the assigned ticket list with the search term
            return Redirect($"{targetPath}?searchTerm={Uri.EscapeDataString(searchTerm)}");
        }
    }
}
