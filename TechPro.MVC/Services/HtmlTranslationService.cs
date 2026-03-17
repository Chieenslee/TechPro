using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TechPro.Services
{
    public class HtmlTranslationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HtmlTranslationService> _logger;

        public HtmlTranslationService(IHttpClientFactory httpClientFactory, IConfiguration config, IWebHostEnvironment env, ILogger<HtmlTranslationService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _env = env;
            _logger = logger;
        }

        public async Task<(string Html, string Status)> TranslateHtmlToEnglishAsync(string html, string cacheKey, CancellationToken ct)
        {
            var cacheDir = Path.Combine(_env.ContentRootPath, "App_Data", "html-translate-cache");
            Directory.CreateDirectory(cacheDir);

            var fileName = Sha256Hex(cacheKey) + ".en.html";
            var cachePath = Path.Combine(cacheDir, fileName);
            if (File.Exists(cachePath))
            {
                return (await File.ReadAllTextAsync(cachePath, ct), "cache_hit");
            }

            var apiKey = _config["Gemini:ApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // No key → fallback to original
                return (html, "no_api_key");
            }

            // Gemini REST: translate while preserving HTML tags/attributes.
            var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={Uri.EscapeDataString(apiKey)}";

            var prompt = """
You are a professional website localization engine.

Task: Translate the following HTML page from Vietnamese to English.
Rules:
- Preserve ALL HTML tags, attributes, IDs, classes, URLs, inline styles, and scripts exactly.
- Only translate visible human-readable Vietnamese text nodes.
- Do NOT translate code, JSON keys, variable names, or any content inside <script> or <style>.
- Keep whitespace and formatting as close as possible.
- Output ONLY the translated HTML, nothing else.
""";

            var payload = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new object[]
                        {
                            new { text = prompt },
                            new { text = html }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 8192
                }
            };

            var client = _httpClientFactory.CreateClient();
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var res = await client.SendAsync(req, ct);
            if (!res.IsSuccessStatusCode)
            {
                var err = await res.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Gemini translate failed: {Status} {Body}", (int)res.StatusCode, Truncate(err, 500));
                return (html, $"gemini_http_{(int)res.StatusCode}");
            }

            var json = await res.Content.ReadAsStringAsync(ct);
            var translated = ExtractGeminiText(json);
            if (string.IsNullOrWhiteSpace(translated))
            {
                _logger.LogWarning("Gemini translate returned empty text. Raw: {Body}", Truncate(json, 500));
                return (html, "gemini_empty");
            }

            await File.WriteAllTextAsync(cachePath, translated, ct);
            return (translated, "gemini_ok");
        }

        private static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        private static string ExtractGeminiText(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;
                // candidates[0].content.parts[0].text
                var text = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();
                return text ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string Truncate(string s, int max)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            return s.Length <= max ? s : s[..max] + "...";
        }
    }
}

