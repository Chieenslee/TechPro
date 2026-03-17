namespace TechPro.API.Services
{
    public interface IEmailSender
    {
        Task<bool> SendAsync(string toEmail, string subject, string body, CancellationToken ct);
    }
}

