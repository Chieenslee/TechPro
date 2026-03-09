using Microsoft.AspNetCore.Mvc;
using TechPro.API.Data;
using TechPro.API.Models;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly TechProDbContext _context;

        public BookingsController(TechProDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] LichHen model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.NgayHen.Date == DateTime.MinValue.Date)
            {
                return BadRequest("Vui lòng chọn ngày hẹn.");
            }

            _context.LichHens.Add(model);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = model.Id }, model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBooking(int id)
        {
            var booking = await _context.LichHens.FindAsync(id);
            if (booking == null) return NotFound();
            return Ok(booking);
        }
    }
}
