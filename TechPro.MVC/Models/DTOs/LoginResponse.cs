namespace TechPro.Models.DTOs
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; } // Optional if using JWT
        public UserDto? User { get; set; }
        public string? RedirectUrl { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? TenantId { get; set; }
    }
}
