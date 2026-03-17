using System.Security.Claims;

namespace TechPro.Services
{
    /// <summary>
    /// DelegatingHandler that automatically attaches the logged-in user's email
    /// to every outgoing API request as the X-Caller-Email header.
    /// The API uses this to identify who is performing the action without needing
    /// cookie forwarding between two separate apps.
    /// </summary>
    public class CallerEmailHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CallerEmailHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                var email = user.FindFirstValue(ClaimTypes.Email)
                    ?? user.FindFirstValue(ClaimTypes.Name)
                    ?? user.Identity.Name;

                if (!string.IsNullOrEmpty(email))
                {
                    request.Headers.Remove("X-Caller-Email");
                    request.Headers.Add("X-Caller-Email", email);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
