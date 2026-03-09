using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    public class NotificationController : Controller
    {
        public IActionResult Index()
        {
             return Content("Notification module is being migrated.");
        }
        
        public IActionResult GetNotifications()
        {
            return Json(new List<object>()); // Return empty list to prevent JS errors
        }
        
        public IActionResult MarkAsRead(string id)
        {
            return Ok();
        }
    }
}
