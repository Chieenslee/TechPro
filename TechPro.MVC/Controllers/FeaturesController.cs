using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    public class FeaturesController : Controller
    {
        public IActionResult Index()
        {
             return Content("Features module is being migrated.");
        }
    }
}
