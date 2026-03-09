using System.ComponentModel.DataAnnotations;

namespace TechPro.API.Models.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string? LoginMode { get; set; } = "store";
        public string? TenantId { get; set; }
    }
}
