namespace TechPro.Models.InternalChat
{
    public record ChatMessageDto(
        string Id,
        string ChannelId,
        string SenderId,
        string SenderName,
        string SenderRole,
        string SenderTenantId,
        string Body,
        DateTimeOffset SentAt
    );
}

