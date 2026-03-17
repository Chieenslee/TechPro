namespace TechPro.API.Services
{
    public class SmsOptions
    {
        // Nếu không set Url => không gửi, chỉ log
        public string? Url { get; set; }

        // Tuỳ nhà cung cấp: token/apiKey
        public string? ApiKey { get; set; }

        // Template message. Supported placeholders:
        // {TicketId}, {DeviceName}, {CustomerName}, {Status}
        public string MessageTemplate { get; set; } =
            "TechPro: Phiếu {TicketId} ({DeviceName}) đã sửa xong. Vui lòng đến cửa hàng nhận máy.";
    }
}

