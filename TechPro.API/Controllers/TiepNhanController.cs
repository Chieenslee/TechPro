using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TiepNhanController : ControllerBase
    {
        private readonly TechProDbContext _context;

        public TiepNhanController(TechProDbContext context)
        {
            _context = context;
        }

        // GET: api/TiepNhan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhieuSuaChua>>> GetPhieuSuaChuas(string? searchTerm, string? status)
        {
            var query = _context.PhieuSuaChuas.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "all")
            {
                query = query.Where(p => p.TrangThai == status);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                var lowerTerm = searchTerm.Trim().ToLower();
                query = query.Where(p => p.TenKhachHang.ToLower().Contains(lowerTerm) ||
                                         p.SoDienThoai.Contains(searchTerm.Trim()) ||
                                         p.Id.Contains(searchTerm.Trim()));
            }

            return await query.OrderByDescending(p => p.NgayNhan).ToListAsync();
        }

        // GET: api/TiepNhan/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PhieuSuaChua>> GetPhieuSuaChua(string id)
        {
            var phieuSuaChua = await _context.PhieuSuaChuas.FindAsync(id);

            if (phieuSuaChua == null)
            {
                return NotFound();
            }

            return phieuSuaChua;
        }

        // POST: api/TiepNhan/TaoPhieu
        [HttpPost("TaoPhieu")]
        public async Task<ActionResult<PhieuSuaChua>> CreatePhieuSuaChua(PhieuSuaChua phieuSuaChua)
        {
            if (string.IsNullOrEmpty(phieuSuaChua.Id))
            {
                var now = DateTime.UtcNow.AddHours(7);
                phieuSuaChua.Id = "PSC" + now.ToString("yyMMddHHmmss");
            }
            
            phieuSuaChua.NgayNhan = DateTime.UtcNow;
            phieuSuaChua.TrangThai = PhieuSuaChua.Statuses.Pending;

            // If Accessories is array in MVC but string here, client handles joining
            // Already handled by model binding if implemented correctly on client

            _context.PhieuSuaChuas.Add(phieuSuaChua);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PhieuSuaChuaExists(phieuSuaChua.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetPhieuSuaChua", new { id = phieuSuaChua.Id }, phieuSuaChua);
        }

        private bool PhieuSuaChuaExists(string id)
        {
            return _context.PhieuSuaChuas.Any(e => e.Id == id);
        }

        public class AnalyzeRequest
        {
            public string MoTaLoi { get; set; } = string.Empty;
            public string TenThietBi { get; set; } = string.Empty;
        }

        [HttpPost("AnalyzeQuote")]
        public async Task<IActionResult> AnalyzeQuote([FromBody] AnalyzeRequest request, [FromServices] TechPro.API.Services.SmartDiagnosisService aiService)
        {
            var result = await aiService.AnalyzeQuoteAsync(request.MoTaLoi, request.TenThietBi);
            return Ok(result);
        }
    }
}
