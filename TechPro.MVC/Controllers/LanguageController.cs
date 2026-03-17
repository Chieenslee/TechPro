using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    [Route("Language")]
    public class LanguageController : Controller
    {
        [HttpGet("Set")]
        public IActionResult Set(string culture = "vi", string? returnUrl = null)
        {
            var lang = culture.Equals("en", StringComparison.OrdinalIgnoreCase) ? "en" : "vi";

            Response.Cookies.Append(
                "tp_lang",
                lang,
                new CookieOptions
                {
                    HttpOnly = false,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddYears(1)
                });

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}

