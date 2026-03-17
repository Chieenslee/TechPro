using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SystemAdmin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<NguoiDung> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly TechProDbContext _context;

        public AdminUsersController(
            UserManager<NguoiDung> userManager,
            RoleManager<IdentityRole> roleManager,
            TechProDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        public record CreateStaffRequest(string email, string tenDayDu, string role, string tenantId);
        public record TransferStaffRequest(string userId, string targetTenantId);

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStaffRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.email) || string.IsNullOrWhiteSpace(req.tenDayDu) || string.IsNullOrWhiteSpace(req.role))
                return BadRequest("Thiếu thông tin.");

            var email = req.email.Trim().ToLowerInvariant();
            var role = req.role.Trim();
            var tenantId = string.IsNullOrWhiteSpace(req.tenantId) ? null : req.tenantId.Trim();

            if (!await _roleManager.RoleExistsAsync(role))
                return BadRequest("Role không tồn tại.");

            if (!string.IsNullOrEmpty(tenantId) && tenantId != "HEAD_OFFICE")
            {
                var tenantExists = await _context.CuaHangs.AnyAsync(c => c.Id == tenantId);
                if (!tenantExists) return BadRequest("Mã cửa hàng không tồn tại.");
            }

            var existing = await _userManager.FindByEmailAsync(email);
            if (existing != null) return Conflict("Email đã tồn tại.");

            var user = new NguoiDung
            {
                UserName = email,
                Email = email,
                TenDayDu = req.tenDayDu.Trim(),
                TenantId = tenantId,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, "TechPro@123");
            if (!createResult.Succeeded)
            {
                var err = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return BadRequest(err);
            }

            await _userManager.AddToRoleAsync(user, role);
            return Ok("Đã tạo nhân viên. Mật khẩu mặc định: TechPro@123");
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> Transfer([FromBody] TransferStaffRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.userId) || string.IsNullOrWhiteSpace(req.targetTenantId))
                return BadRequest("Thiếu thông tin.");

            var user = await _userManager.FindByIdAsync(req.userId);
            if (user == null) return NotFound("Không tìm thấy user.");

            var tenantId = req.targetTenantId.Trim();
            if (tenantId != "HEAD_OFFICE")
            {
                var tenantExists = await _context.CuaHangs.AnyAsync(c => c.Id == tenantId);
                if (!tenantExists) return BadRequest("Mã cửa hàng đích không tồn tại.");
            }

            user.TenantId = tenantId;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var err = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(err);
            }

            return Ok("Đã điều chuyển nhân viên.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            // Prevent deleting the last SystemAdmin (basic guard)
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("SystemAdmin"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("SystemAdmin");
                if (admins.Count <= 1)
                    return BadRequest("Không thể xóa SystemAdmin cuối cùng.");
            }

            var res = await _userManager.DeleteAsync(user);
            if (!res.Succeeded)
            {
                var err = string.Join(", ", res.Errors.Select(e => e.Description));
                return BadRequest(err);
            }

            return Ok();
        }
    }
}

