using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Hubs;
using TechPro.API.Models;
using TechPro.API.Models.DTOs;
using TechPro.API.Services;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // Yêu cầu đăng nhập cho TẤT CẢ endpoint trong controller này
    public class TechnicianController : ControllerBase
    {
        private readonly TechProDbContext _context;
        private readonly IHubContext<TicketHub> _hubContext;
        private readonly AuditLogService _audit;

        public TechnicianController(TechProDbContext context, IHubContext<TicketHub> hubContext, AuditLogService audit)
        {
            _context = context;
            _hubContext = hubContext;
            _audit = audit;
        }

        private string CallerEmail() => Request.Headers["X-Caller-Email"].FirstOrDefault()
                                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                                       ?? "unknown";
        private string CallerRole()  => Request.Headers["X-Caller-Role"].FirstOrDefault()
                                       ?? User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
                                       ?? "unknown";
        private string CallerIp()    => HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // GET api/Technician/tickets — Technician chỉ thấy phiếu của mình (filter assigneeId)
        // StoreAdmin/SysAdmin thấy tất cả để giám sát
        [HttpGet("tickets")]
        [Authorize(Roles = "Technician,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GetTickets(string? status = null, string? assigneeId = null, string? tenantId = null, string? searchTerm = null)
        {
            var query = _context.PhieuSuaChuas
                .Include(p => p.KyThuatVien)
                .Include(p => p.CuaHang)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "all")
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

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(p => p.Id.ToLower().Contains(term) || p.SoDienThoai.Contains(term) || p.TenKhachHang.ToLower().Contains(term));
            }

            var tickets = await query.OrderByDescending(p => p.NgayNhan).ToListAsync();
            return Ok(tickets);
        }

        [HttpGet("tickets/{id}")]
        [Authorize(Roles = "Technician,StoreAdmin,SystemAdmin")]
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

        // Chỉ Technician mới được cập nhật trạng thái phiếu của mình
        [HttpPut("tickets/{id}/status")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> UpdateStatus(string id, [FromBody] string status)
        {
            var ticket = await _context.PhieuSuaChuas.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.TrangThai = status;
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("TicketStatusChanged", id, status);

            await _audit.LogAsync(CallerEmail(), CallerRole(), "CapNhatTrangThaiPhieu", id, "PhieuSuaChua",
                ghiChu: $"Trạng thái mới: {status}", ipAddress: CallerIp());

            return Ok(new { message = "Updated status" });
        }

        // Chỉ StoreAdmin/SysAdmin mới được gán kỹ thuật viên
        [HttpPut("tickets/{id}/assign")]
        [Authorize(Roles = "StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> AssignTicket(string id, [FromBody] string? technicianId)
        {
            var ticket = await _context.PhieuSuaChuas.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.KyThuatVienId = technicianId;
            ticket.TrangThai = "fixing";
            await _context.SaveChangesAsync();

            await _audit.LogAsync(CallerEmail(), CallerRole(), "GanKyThuatVien", id, "PhieuSuaChua",
                ghiChu: $"Gán KTV: {technicianId}", ipAddress: CallerIp());

            return Ok(new { message = "Assigned technician" });
        }

        // Chỉ Technician mới được tạo yêu cầu linh kiện
        [HttpPost("tickets/{id}/parts")]
        [Authorize(Roles = "Technician")]
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
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> UpdateTestResult(string id, [FromBody] string result)
        {
            var ticket = await _context.PhieuSuaChuas.FindAsync(id);
            if (ticket == null) return NotFound();

            ticket.KetQuaKiemTra = result;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("tickets/{id}/notes")]
        [Authorize(Roles = "Technician,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GetNotes(string id)
        {
            var notes = await _context.TicketNotes
                .Where(n => n.PhieuSuaChuaId == id)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return Ok(notes);
        }

        [HttpPost("tickets/{id}/notes")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> AddNote(string id, [FromBody] TicketNote note)
        {
            note.PhieuSuaChuaId = id;
            if (note.CreatedAt == default) note.CreatedAt = DateTime.UtcNow;
            
            _context.TicketNotes.Add(note);
            await _context.SaveChangesAsync();
            return Ok(note);
        }

        [HttpGet("tickets/{id}/scratch-marks")]
        [Authorize(Roles = "Technician,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GetScratchMarks(string id)
        {
            var marks = await _context.ScratchMarks.Where(s => s.PhieuSuaChuaId == id).ToListAsync();
            return Ok(marks);
        }

        [HttpPost("tickets/{id}/scratch-marks")]
        [Authorize(Roles = "Technician")]
        public async Task<IActionResult> SaveScratchMarks(string id, [FromBody] List<ScratchMark> marks)
        {
            var existing = await _context.ScratchMarks.Where(s => s.PhieuSuaChuaId == id).ToListAsync();
            _context.ScratchMarks.RemoveRange(existing);
            
            foreach (var m in marks) m.PhieuSuaChuaId = id;
            _context.ScratchMarks.AddRange(marks);
            await _context.SaveChangesAsync();
            
            return Ok(marks);
        }
        // Lịch sử sửa chữa của khách — Support và Technician dùng
        [HttpGet("history")]
        [Authorize(Roles = "Technician,Support,StoreAdmin,SystemAdmin")]
        public async Task<IActionResult> GetHistory(string? phone, string? excludeId)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Ok(Array.Empty<object>());

            var query = _context.PhieuSuaChuas
                .Where(p => p.SoDienThoai == phone);

            if (!string.IsNullOrEmpty(excludeId))
                query = query.Where(p => p.Id != excludeId);

            var tickets = await query
                .OrderByDescending(p => p.NgayNhan)
                .Take(20)
                .Select(p => new {
                    p.Id,
                    p.TenThietBi,
                    p.MoTaLoi,
                    p.TrangThai,
                    p.NgayNhan,
                    p.SoDienThoai,
                    p.KyThuatVienId
                })
                .ToListAsync();

            return Ok(tickets);
        }
    }
}
