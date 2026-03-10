using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    [Authorize(Roles = "Technician,StoreAdmin,SystemAdmin")]
    [Route("Technician")]
    public class TechnicianDashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}
