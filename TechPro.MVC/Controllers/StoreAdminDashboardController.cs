using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    [Authorize(Roles = "StoreAdmin,SystemAdmin")]
    [Route("StoreAdmin")]
    public class StoreAdminDashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
