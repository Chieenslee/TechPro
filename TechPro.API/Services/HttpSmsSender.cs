using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace TechPro.API.Services
{
    public class HttpSmsSender : ISmsSender
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SmsOptions _opt;
        private readonly ILogger<HttpSmsSender> _logger;

        public HttpSmsSender(IHttpClientFactory httpClientFactory, IOptions<SmsOptions> opt, ILogger<HttpSmsSender> logger)
        {
            _httpClientFactory = httpClientFactory;
            _opt = opt.Value;
            _logger = logger;
        }

        public async Task<bool> SendAsync(string toPhone, string message, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(_opt.Url))
            {
                _logger.LogInformation("[SMS disabled] To={To} Msg={Msg}", toPhone, message);
                return false;
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                using var req = new HttpRequestMessage(HttpMethod.Post, _opt.Url);

                if (!string.IsNullOrWhiteSpace(_opt.ApiKey))
                {
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _opt.ApiKey);
                }

                // Payload generic: provider tự map
                var payload = new { to = toPhone, message };
                req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var res = await client.SendAsync(req, ct);
                var ok = res.IsSuccessStatusCode;
                if (!ok)
                {
                    var body = await res.Content.ReadAsStringAsync(ct);
                    _logger.LogWarning("SMS send failed: {Status} {Body}", (int)res.StatusCode, body);
                }
                return ok;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "SMS send exception");
                return false;
            }
        }
    }
}

