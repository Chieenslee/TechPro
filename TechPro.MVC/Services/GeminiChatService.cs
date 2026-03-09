using System.Text;
using System.Text.Json;

namespace TechPro.Services
{
    public class GeminiChatService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GeminiChatService> _logger;

        public GeminiChatService(IConfiguration configuration, ILogger<GeminiChatService> logger)
        {
            _httpClient = new HttpClient();
            _apiKey = configuration["Gemini:ApiKey"] ?? "";
            _logger = logger;
        }

        public async Task<string> GetChatResponseAsync(string userMessage, string? conversationHistory = null)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                return "Xin lỗi, dịch vụ AI tư vấn chưa được cấu hình. Vui lòng liên hệ hotline 1900 6868 để được hỗ trợ.";
            }

            try
            {
                var prompt = BuildPrompt(userMessage, conversationHistory);
                
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new[]
                            {
                                new { text = prompt }
                            }
                        }
                    },
                    generationConfig = new
                    {
                        temperature = 0.7,
                        topK = 40,
                        topP = 0.95,
                        maxOutputTokens = 1024
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={_apiKey}",
                    content
                );

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? 
                           "Xin lỗi, tôi không thể trả lời câu hỏi này. Vui lòng liên hệ hotline 1900 6868.";
                }
                else
                {
                    _logger.LogWarning("Gemini API error: {StatusCode} - {Content}", 
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                    return "Xin lỗi, có lỗi xảy ra khi kết nối với dịch vụ AI. Vui lòng thử lại sau hoặc liên hệ hotline 1900 6868.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return "Xin lỗi, có lỗi xảy ra. Vui lòng liên hệ hotline 1900 6868 để được hỗ trợ trực tiếp.";
            }
        }

        private string BuildPrompt(string userMessage, string? history)
        {
            var systemPrompt = @"Bạn là trợ lý AI tư vấn của TechPro Care - một chuỗi cửa hàng sửa chữa điện thoại, máy tính bảng và thiết bị điện tử chuyên nghiệp tại Việt Nam.

Nhiệm vụ của bạn:
1. Tư vấn về các dịch vụ sửa chữa: thay màn hình, thay pin, sửa phần cứng, cấp cứu dữ liệu, mở khóa iCloud, vệ sinh bảo dưỡng
2. Hướng dẫn khách hàng về quy trình sửa chữa
3. Tư vấn về bảo hành và chính sách
4. Hỗ trợ tra cứu thông tin phiếu sửa chữa
5. Giải đáp các câu hỏi thường gặp

Quy tắc:
- Luôn lịch sự, thân thiện và chuyên nghiệp
- Trả lời bằng tiếng Việt
- Nếu không chắc chắn, hướng dẫn khách liên hệ hotline 1900 6868
- Không đưa ra thông tin về giá cụ thể nếu không chắc chắn
- Khuyến khích khách đặt lịch hẹn để được phục vụ tốt nhất

Thông tin liên hệ:
- Hotline: 1900 6868 (8:00 - 21:00)
- Email: support@techprocare.vn
- Địa chỉ: 123 Đường Công Nghệ, Quận Hoàn Kiếm, TP. Hà Nội

Hãy trả lời câu hỏi của khách hàng một cách ngắn gọn, rõ ràng và hữu ích.";

            if (!string.IsNullOrEmpty(history))
            {
                return $"{systemPrompt}\n\nLịch sử trò chuyện:\n{history}\n\nKhách hàng: {userMessage}\n\nTrợ lý AI:";
            }

            return $"{systemPrompt}\n\nKhách hàng: {userMessage}\n\nTrợ lý AI:";
        }
    }

    // Response models
    public class GeminiResponse
    {
        public Candidate[]? Candidates { get; set; }
    }

    public class Candidate
    {
        public Content? Content { get; set; }
    }

    public class Content
    {
        public Part[]? Parts { get; set; }
    }

    public class Part
    {
        public string? Text { get; set; }
    }
}

