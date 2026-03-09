using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace TechPro.Controllers
{
    [Authorize(Roles = "StoreAdmin,SystemAdmin")]
    public class ExportController : Controller
    {
        public IActionResult Index()
        {
             return Content("Export module is being migrated.");
        }
    }
}
