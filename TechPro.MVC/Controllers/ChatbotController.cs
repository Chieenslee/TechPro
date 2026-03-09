using Microsoft.AspNetCore.Mvc;

namespace TechPro.Controllers
{
    public class ChatbotRequest 
    {
        public string message { get; set; } = string.Empty;
        public string history { get; set; } = string.Empty;
    }

    public class ChatbotController : Controller
    {
        public IActionResult Index()
        {
             return Content("Hệ thống Chatbot đang hoạt động.");
        }
        
        [HttpPost]
        public IActionResult SendMessage([FromBody] ChatbotRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.message))
            {
                return Json(new { success = false, message = "Vui lòng nhập câu hỏi." });
            }

            var msg = request.message.ToLower();
            string response = "Xin lỗi, TechPro AI hiện tại chưa thể hiểu câu hỏi của bạn. Vui lòng liên hệ tổng đài 1900 6868 hoặc để lại số điện thoại, kỹ thuật viên sẽ gọi lại cho bạn ngay.";

            // Basic Rule-based logic
            if (msg.Contains("giá") && msg.Contains("màn hình"))
            {
                response = "TechPro Care có dịch vụ thay màn hình chính hãng cho các dòng iPhone, Samsung, Xiaomi,... Giá dịch vụ dao động từ 500.000đ đến 4.500.000đ tùy vào dòng máy. Bạn đang sử dụng model máy nào ạ?";
            }
            else if (msg.Contains("giá") || msg.Contains("bao nhiêu tiền"))
            {
                response = "Giá sửa chữa tại TechPro Care luôn công khai và cạnh tranh nhất thị trường. Để báo giá chính xác, bạn vui lòng cung cấp thêm thông tin model thiết bị và tình trạng lỗi chi tiết nhé!";
            }
            else if (msg.Contains("thời gian") || msg.Contains("bao lâu"))
            {
                response = "Hầu hết các dịch vụ siêu tốc như thay pin, ép kính, thay màn hình tại TechPro Care đều có thể lấy ngay trong vòng 30 - 60 phút. Bạn có muốn đặt lịch hẹn trước để kỹ thuật viên chuẩn bị linh kiện không?";
            }
            else if (msg.Contains("bảo hành") || msg.Contains("chính sách"))
            {
                response = "Dịch vụ của TechPro Care áp dụng Chính sách bảo hành Vàng: 1 đổi 1 trong 30 ngày nếu phát sinh lỗi linh kiện, và bảo hành lên đến 12 tháng tùy loại dịch vụ. Cực kỳ an tâm nhé bạn!";
            }
            else if (msg.Contains("ở đâu") || msg.Contains("địa chỉ"))
            {
                response = "TechPro Care có trụ sở chính tại: 123 Đường Công Nghệ, Quận Hoàn Kiếm, TP. Hà Nội. Cửa hàng mở cửa liên tục từ 8:00 đến 21:00 các ngày trong tuần.";
            }
            else if (msg.Contains("pin") && msg.Contains("chai"))
            {
                 response = "Nếu pin của bạn nhanh hao (tình trạng dưới 80%), hay tắt điện thoại đột ngột thì đây là lúc bạn nên thay pin mới. Tại TechPro Care, kiểm tra mức độ chai pin là hoàn toàn miễn phí!";
            }
            else if (msg.Contains("chào") || msg.Contains("hi ") || msg == "hi" || msg == "hello")
            {
                response = "Dạ chào bạn, TechPro AI có thể hỗ trợ tư vấn dịch vụ gì cho bạn hôm nay? Bạn có thể hỏi về giá cả, thời gian, hoặc chính sách bảo hành nhé.";
            }
            else if (msg.Contains("cam ơn") || msg.Contains("cám ơn") || msg.Contains("cảm ơn") || msg.Contains("ok") || msg.Contains("thank"))
            {
                response = "Rất vui được hỗ trợ bạn. Chúc bạn một ngày tốt lành! Nếu cần hỗ trợ thêm, đừng ngần ngại nhắn lại cho TechPro AI nhé.";
            }

            return Json(new { success = true, response = response });
        }
    }
}
