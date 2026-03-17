using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    [Authorize(Roles = "Storekeeper,SystemAdmin")]
    [Route("Storekeeper")]
    public class StorekeeperDashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
