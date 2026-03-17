using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin")]
    public class ChainController : ControllerBase
    {
        private readonly TechProDbContext _context;

        public ChainController(TechProDbContext context)
        {
            _context = context;
        }

        // GET: api/Chain/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var stores = await _context.CuaHangs.Include(x => x.PhieuSuaChuas).ToListAsync();
            
            var totalRevenue = await _context.PhieuSuaChuas
                .Where(p => p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered)
                .SumAsync(p => p.TongTien);
                
            var users = await _context.Users.Include(u => u.CuaHang).ToListAsync();
            var totalStaff = users.Count;

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            
            var rawRevenues = await _context.PhieuSuaChuas
                .Where(p => p.TenantId != null && (p.NgayHoanThanh ?? p.NgayNhan) >= startOfMonth && (p.TrangThai == PhieuSuaChua.Statuses.Done || p.TrangThai == PhieuSuaChua.Statuses.Delivered))
                .GroupBy(p => p.TenantId)
                .Select(g => new { TenantId = g.Key, Revenue = g.Sum(x => x.TongTien) })
                .ToListAsync();

            var userRolesList = await _context.UserRoles.ToListAsync();
            var rolesDict = await _context.Roles.ToDictionaryAsync(r => r.Id, r => r.Name);

            var usersWithRoles = users.Select(u => new 
            {
                User = u,
                Role = userRolesList.Where(ur => ur.UserId == u.Id)
                                    .Select(ur => rolesDict.TryGetValue(ur.RoleId, out var r) ? r : "N/A")
                                    .FirstOrDefault() ?? "N/A"
            }).ToList();

            return Ok(new
            {
                Stores = stores,
                Users = users,
                TotalRevenue = totalRevenue,
                TotalStaff = totalStaff,
                ThisMonthStoreRevenues = rawRevenues,
                UsersWithRoles = usersWithRoles
            });
        }

        // GET: api/Chain
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuaHang>>> GetCuaHangs()
        {
            return await _context.CuaHangs.Include(x => x.PhieuSuaChuas).ToListAsync();
        }

        // GET: api/Chain/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CuaHang>> GetCuaHang(string id)
        {
            var cuaHang = await _context.CuaHangs.FindAsync(id);

            if (cuaHang == null)
            {
                return NotFound();
            }

            return cuaHang;
        }

        // PUT: api/Chain/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCuaHang(string id, CuaHang cuaHang)
        {
            if (id != cuaHang.Id)
            {
                return BadRequest();
            }

            _context.Entry(cuaHang).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CuaHangExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Chain
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CuaHang>> PostCuaHang(CuaHang cuaHang)
        {
            _context.CuaHangs.Add(cuaHang);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CuaHangExists(cuaHang.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetCuaHang", new { id = cuaHang.Id }, cuaHang);
        }

        // DELETE: api/Chain/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCuaHang(string id)
        {
            var cuaHang = await _context.CuaHangs.FindAsync(id);
            if (cuaHang == null)
            {
                return NotFound();
            }

            _context.CuaHangs.Remove(cuaHang);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CuaHangExists(string id)
        {
            return _context.CuaHangs.Any(e => e.Id == id);
        }
    }
}
