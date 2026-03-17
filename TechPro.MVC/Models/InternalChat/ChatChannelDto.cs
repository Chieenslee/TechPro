namespace TechPro.Models.InternalChat
{
    public record ChatChannelDto(
        string Id,
        string Name,
        string Kind, // "channel" | "dm"
        string Scope, // "global" | "tenant" | "role"
        string? TenantId,
        string[] AllowedRoles,
        string Icon,
        string Color
    );
}

