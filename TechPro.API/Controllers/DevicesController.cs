using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly TechProDbContext _context;

        public DevicesController(TechProDbContext context)
        {
            _context = context;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query is required.");
            }

            var thietBi = await _context.ThietBiBans
                .FirstOrDefaultAsync(t => t.SerialNumber == query.Trim());

            if (thietBi == null)
            {
                return NotFound();
            }

            return Ok(thietBi);
        }
    }
}
