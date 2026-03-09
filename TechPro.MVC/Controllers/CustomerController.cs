using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    public class CustomerController : Controller
    {
        public IActionResult Index()
        {
             return Content("Customer module is being migrated.");
        }
    }
}
