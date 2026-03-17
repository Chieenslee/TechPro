using System.Text;
using TechPro.Services;

namespace TechPro.Middleware
{
    public class HtmlAutoTranslateMiddleware
    {
        private readonly RequestDelegate _next;

        public HtmlAutoTranslateMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, HtmlTranslationService translator)
        {
            // Only translate when explicitly set to English by cookie.
            var lang = context.Request.Cookies["tp_lang"];
            var isEnglish = string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase);
            if (!isEnglish)
            {
                await _next(context);
                return;
            }

            // Skip non-GET pages and API calls
            if (!HttpMethods.IsGet(context.Request.Method) ||
                context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var originalBody = context.Response.Body;
            await using var mem = new MemoryStream();
            context.Response.Body = mem;

            try
            {
                await _next(context);

                // Only translate HTML 200 responses.
                if (context.Response.StatusCode != 200 ||
                    context.Response.ContentType == null ||
                    !context.Response.ContentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    mem.Position = 0;
                    await mem.CopyToAsync(originalBody);
                    return;
                }

                mem.Position = 0;
                var html = await new StreamReader(mem, Encoding.UTF8).ReadToEndAsync();

                // Cache key includes path + query + a hash of content length (cheap).
                var cacheKey = $"{context.Request.Path}{context.Request.QueryString}|len:{html.Length}";
                var (translated, status) = await translator.TranslateHtmlToEnglishAsync(html, cacheKey, context.RequestAborted);
                context.Response.Headers["X-TP-Lang"] = "en";
                context.Response.Headers["X-TP-Translate"] = status;

                var outBytes = Encoding.UTF8.GetBytes(translated);
                context.Response.Headers.ContentLength = outBytes.Length;
                await originalBody.WriteAsync(outBytes, 0, outBytes.Length, context.RequestAborted);
            }
            finally
            {
                context.Response.Body = originalBody;
            }
        }
    }
}

