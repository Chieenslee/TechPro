using Microsoft.AspNetCore.Authorization;
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
                    RedirectUrl = PerformRedirect(role)
                });
            }

            return Unauthorized(new LoginResponse { Success = false, Message = "Email hoặc mật khẩu không chính xác." });
        }

        // Chỉ người đã đăng nhập mới được đổi mật khẩu
        // Và chỉ đổi được mật khẩu của chính mình
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ChangePasswordResponse { Success = false, Message = "Dữ liệu không hợp lệ." });
            }

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound(new ChangePasswordResponse { Success = false, Message = "Không tìm thấy người dùng." });
            }

            // Kiểm tra người gọi chính là chủ tài khoản
            // Ưu tiên: X-Caller-Email header (MVC server-to-server)
            // Fallback: User.Claims (gọi trực tiếp với JWT)
            var callerEmail = Request.Headers["X-Caller-Email"].FirstOrDefault()
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
            var callerRole  = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;

            // Fail-secure: không xác định được danh tính người gọi → từ chối ngay
            // Tránh bypass khi callerEmail = null làm mất điều kiện kiểm tra
            if (string.IsNullOrEmpty(callerEmail) || callerEmail == "unknown")
            {
                return Forbid();
            }

            if (callerEmail != user.Email && callerRole != "SystemAdmin")
            {
                return Forbid();  // 403 — không phải chính chủ
            }

            var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new ChangePasswordResponse { Success = true, Message = "Đổi mật khẩu thành công!" });
            }

            var err = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new ChangePasswordResponse { Success = false, Message = err });
        }

        // Redirect dựa trên role từ Identity DB — không tin tưởng email prefix
        private string PerformRedirect(string role) => role switch
        {
            "SystemAdmin" => "/SysAdmin",
            "StoreAdmin"  => "/StoreAdmin",
            "Technician"  => "/Technician",
            "Support"     => "/Support",
            "Storekeeper" => "/Storekeeper",
            _             => "/"
        };
    }
}
