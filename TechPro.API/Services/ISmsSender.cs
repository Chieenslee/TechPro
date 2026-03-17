namespace TechPro.API.Services
{
    public interface ISmsSender
    {
        Task<bool> SendAsync(string toPhone, string message, CancellationToken ct);
    }
}

