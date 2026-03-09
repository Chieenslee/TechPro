using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Hubs;
using TechPro.API.Models;
using TechPro.API.Models.DTOs;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechnicianController : ControllerBase
    {
        private readonly TechProDbContext _context;
        private readonly IHubContext<TicketHub> _hubContext;

        public TechnicianController(TechProDbContext context, IHubContext<TicketHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("tickets")]
        public async Task<IActionResult> GetTickets(string? status = null, string? assigneeId = null, string? tenantId = null)
        {
            var query = _context.PhieuSuaChuas
                .Include(p => p.KyThuatVien)
                .Include(p => p.CuaHang)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(p => p.TrangThai == status);
            }

            if (!string.IsNullOrEmpty(assigneeId))
            {
                query = query.Where(p => p.KyThuatVienId == assigneeId);
            }

            if (!string.IsNullOrEmpty(tenantId))
            {
                query = query.Where(p => p.TenantId == tenantId);
            }

            var tickets = await query.OrderByDescending(p => p.NgayNhan).ToListAsync();
            return Ok(tickets);
        }

        [HttpGet("tickets/{id}")]
        public async Task<IActionResult> GetTicket(string id)
        {
            var ticket = await _context.PhieuSuaChuas
                .Include(p => p.KyThuatVien)
                .Include(p => p.CuaHang)
                .Include(p => p.YeuCauLinhKiens).ThenInclude(y => y.LinhKien)
                .Include(p => p.TraXacs)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (ticket == null) return NotFound();
            return Ok(ticket);
        }

        [HttpPut("tickets/{id}/status")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
        {
            var ticket = await _context.PhieuSuaChuas.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.TrangThai = status;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("TicketStatusChanged", id, status);

            return Ok(new { message = "Updated status" });
        }

        [HttpPut("tickets/{id}/assign")]
        public async Task<IActionResult> AssignTicket(string id, [FromBody] string? technicianId)
        {
            var ticket = await _context.PhieuSuaChuas.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.KyThuatVienId = technicianId;
            ticket.TrangThai = "fixing"; // Auto change status? or keep existing. "repairing"
            await _context.SaveChangesAsync();
            
            return Ok(new { message = "Assigned technician" });
        }

        [HttpPost("tickets/{id}/parts")]
        public async Task<IActionResult> RequestPart(string id, [FromBody] YeuCauLinhKien request)
        {
            request.PhieuSuaChuaId = id;
            request.NgayYeuCau = DateTime.Now;
            request.TrangThai = "pending";
            
            _context.YeuCauLinhKiens.Add(request);
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.All.SendAsync("NewPartRequest", request.Id);
            return Ok(request);
        }

        [HttpPut("tickets/{id}/test-result")]
        public async Task<IActionResult> UpdateTestResult(string id, [FromBody] string result)
        {
            var ticket = await _context.PhieuSuaChuas.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.KetQuaKiemTra = result;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("tickets/{id}/notes")]
        public async Task<IActionResult> GetNotes(string id)
        {
            var notes = await _context.TicketNotes
                .Where(n => n.PhieuSuaChuaId == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return Ok(notes);
        }

        [HttpPost("tickets/{id}/notes")]
        public async Task<IActionResult> AddNote(string id, [FromBody] TicketNote note)
        {
            note.PhieuSuaChuaId = id;
            if (note.CreatedAt == default) note.CreatedAt = DateTime.UtcNow;
            
            _context.TicketNotes.Add(note);
            await _context.SaveChangesAsync();
            return Ok(note);
        }

        [HttpGet("tickets/{id}/scratch-marks")]
        public async Task<IActionResult> GetScratchMarks(string id)
        {
            var marks = await _context.ScratchMarks.Where(s => s.PhieuSuaChuaId == id).ToListAsync();
            return Ok(marks);
        }

        [HttpPost("tickets/{id}/scratch-marks")]
        public async Task<IActionResult> SaveScratchMarks(string id, [FromBody] List<ScratchMark> marks)
        {
            var existing = await _context.ScratchMarks.Where(s => s.PhieuSuaChuaId == id).ToListAsync();
            _context.ScratchMarks.RemoveRange(existing);
            
            foreach (var m in marks) m.PhieuSuaChuaId = id;
            _context.ScratchMarks.AddRange(marks);
            await _context.SaveChangesAsync();
            
            return Ok(marks);
        }
    }
}
