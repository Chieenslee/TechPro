using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using TechPro.Models.DTOs;

namespace TechPro.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string email, string password, string? loginMode = "store", string? tenantId = null, string? returnUrl = null)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Vui lòng nhập email và mật khẩu.";
                TempData["LoginMode"] = loginMode;
                TempData["TenantId"] = tenantId ?? "";
                return View();
            }

            var loginRequest = new LoginRequest
            {
                Email = email,
                Password = password,
                LoginMode = loginMode,
                TenantId = tenantId
            };

            var client = _httpClientFactory.CreateClient("TechProAPI");
            var json = JsonSerializer.Serialize(loginRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("api/Auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (result != null && result.Success && result.User != null)
                {
                    // Create Claims Principal
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, result.User.Email),
                        new Claim(ClaimTypes.Email, result.User.Email),
                        new Claim("FullName", result.User.Name), // Custom claim
                        new Claim(ClaimTypes.Role, result.User.Role)
                    };

                    if (!string.IsNullOrEmpty(result.User.TenantId))
                    {
                        claims.Add(new Claim("TenantId", result.User.TenantId));
                    }

                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);

                    // Sign In
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                    return RedirectToLocal(returnUrl ?? result.RedirectUrl);
                }
            }

            // Failure
            var errorResponse = await response.Content.ReadFromJsonAsync<LoginResponse>(); // Try to get error message
            TempData["Error"] = errorResponse?.Message ?? (loginMode == "store" 
                ? "Mã cửa hàng, email hoặc mật khẩu không chính xác." 
                : "Email hoặc mật khẩu không chính xác.");
            TempData["LoginMode"] = loginMode;
            TempData["TenantId"] = tenantId ?? "";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
