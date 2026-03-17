using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    // Lightweight API stub so frontend notification JS không bị 404
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        // GET /api/Notification?unreadOnly=true|false
        [HttpGet]
        public IActionResult GetAll([FromQuery] bool unreadOnly = false)
        {
            return Ok(new
            {
                success = true,
                unreadCount = 0,
                data = Array.Empty<object>()
            });
        }

        // POST /api/Notification/{id}/read
        [HttpPost("{id}/read")]
        public IActionResult MarkAsRead(string id)
        {
            return Ok(new { success = true });
        }

        // POST /api/Notification/read-all
        [HttpPost("read-all")]
        public IActionResult MarkAllAsRead()
        {
            return Ok(new { success = true });
        }
    }
}
