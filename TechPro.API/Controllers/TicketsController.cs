using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketsController : ControllerBase
    {
        private readonly TechProDbContext _context;

        public TicketsController(TechProDbContext context)
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

            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.KyThuatVien)
                .FirstOrDefaultAsync(p => 
                    p.Id == query.Trim() || 
                    p.SoDienThoai == query.Trim());

            if (phieu == null)
            {
                return NotFound();
            }

            return Ok(phieu);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTicket(string id)
        {
            var phieu = await _context.PhieuSuaChuas
                .Include(p => p.KyThuatVien)
                .Include(p => p.CuaHang)
                .Include(p => p.YeuCauLinhKiens)
                    .ThenInclude(y => y.LinhKien)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phieu == null)
            {
                return NotFound();
            }

            return Ok(phieu);
        }

        [HttpPost("{id}/confirm")]
        public async Task<IActionResult> ConfirmQuote(string id)
        {
            var ticket = await _context.PhieuSuaChuas.FindAsync(id);
            if (ticket == null)
            {
                return NotFound(new { message = "Không tìm thấy phiếu." });
            }

            if (ticket.TrangThai != PhieuSuaChua.Statuses.WaitingParts)
            {
                return BadRequest(new { message = "Phiếu không ở trạng thái chờ xác nhận." });
            }

            ticket.TrangThai = PhieuSuaChua.Statuses.Repairing;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Đã xác nhận báo giá." });
        }
    }
}
