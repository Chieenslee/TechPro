using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    [Authorize(Roles = "SystemAdmin")]
    [Route("SysAdmin")]
    public class SysAdminDashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
