using System.ComponentModel.DataAnnotations;

namespace TechPro.API.Models.DTOs
{
    public class ChangePasswordRequest
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
    
    public class ChangePasswordResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
