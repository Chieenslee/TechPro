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

        // GET: api/Chain
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CuaHang>>> GetCuaHangs()
        {
            return await _context.CuaHangs.ToListAsync();
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
