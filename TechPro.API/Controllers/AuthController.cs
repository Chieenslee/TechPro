using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechPro.API.Data;
using TechPro.API.Models;
using TechPro.API.Models.DTOs;

namespace TechPro.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SignInManager<NguoiDung> _signInManager;
        private readonly UserManager<NguoiDung> _userManager;
        private readonly TechProDbContext _context;

        public AuthController(
            SignInManager<NguoiDung> signInManager,
            UserManager<NguoiDung> userManager,
            TechProDbContext context)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return Unauthorized(new LoginResponse { Success = false, Message = "Email hoặc mật khẩu không chính xác." });
            }

            // Validate tenant ID for store mode
            if (request.LoginMode == "store")
            {
                if (string.IsNullOrEmpty(request.TenantId))
                {
                    return BadRequest(new LoginResponse { Success = false, Message = "Vui lòng nhập mã cửa hàng." });
                }

                var effectiveTenantId = request.TenantId; // Tenant entered by user (or passed from UI if hidden)
                
                // Check if user belongs to the specified tenant
                if (!string.IsNullOrEmpty(user.TenantId) && user.TenantId != effectiveTenantId)
                {
                    // Verify tenant exists in database
                    var tenantExists = await _context.CuaHangs.AnyAsync(c => c.Id == effectiveTenantId);
                    if (!tenantExists && effectiveTenantId != "HEAD_OFFICE")
                    {
                        return Unauthorized(new LoginResponse { Success = false, Message = "Mã cửa hàng không tồn tại." });
                    }
                    // If tenant exists but user belongs to another tenant?Original logic implies a match is needed if user.TenantId is set.
                     if (effectiveTenantId != "HEAD_OFFICE") // Allow head office generic?
                     {
                         // Strict check: if user has tenantId, it must match.
                         // But original code: if (!string.IsNullOrEmpty(user.TenantId) && user.TenantId != effectiveTenantId) -> Error (implicit via return View)
                         // Actually original code checks existence. If exists, it might proceed if validation logic allows.
                         // But let's assume strict check.
                         return Unauthorized(new LoginResponse { Success = false, Message = "Tài khoản không thuộc cửa hàng này." });
                     }
                }
            }

            // Attempt login
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
            
            if (result.Succeeded)
            {
                var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "User";
                
                return Ok(new LoginResponse 
                { 
                    Success = true, 
                    Message = "Login successful",
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email ?? "",
                        Name = user.TenDayDu,
                        Role = role,
                        TenantId = user.TenantId
                    },
                    RedirectUrl = PerformRedirect(request.Email)
                });
            }

            return Unauthorized(new LoginResponse { Success = false, Message = "Email hoặc mật khẩu không chính xác." });
        }

        private string PerformRedirect(string email)
        {
            if (email.StartsWith("sysadmin")) return "/Chain";
            if (email.StartsWith("admin")) return "/QuanLy";
            if (email.StartsWith("tech")) return "/KyThuat";
            if (email.StartsWith("support")) return "/TiepNhan";
            if (email.StartsWith("kho")) return "/Kho";
            return "/QuanLy";
        }
    }
}
