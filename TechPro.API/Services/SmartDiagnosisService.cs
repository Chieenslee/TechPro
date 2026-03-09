using TechPro.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace TechPro.API.Services
{
    public class SmartDiagnosisService
    {
        private readonly TechProDbContext _context;

        public SmartDiagnosisService(TechProDbContext context)
        {
            _context = context;
        }

        public async Task<DiagnosisResult> AnalyzeQuoteAsync(string moTaLoi, string tenThietBi)
        {
            var result = new DiagnosisResult { Parts = new List<SuggestedPart>() };
            if (string.IsNullOrWhiteSpace(moTaLoi)) return result;

            moTaLoi = moTaLoi.ToLower();
            tenThietBi = (tenThietBi ?? "").ToLower();

            // Nhận diện từ khóa bệnh lý thông thường (Fuzzy & Keyword Matching)
            var keywords = new List<string>();
            if (moTaLoi.Contains("vỡ kính") || moTaLoi.Contains("bể kính") || moTaLoi.Contains("nứt kính")) keywords.Add("mặt kính");
            if (moTaLoi.Contains("màn hình") || moTaLoi.Contains("xoắn") || moTaLoi.Contains("sọc") || moTaLoi.Contains("chớp") || moTaLoi.Contains("lem mực")) keywords.Add("màn hình");
            if (moTaLoi.Contains("pin") || moTaLoi.Contains("sập nguồn") || moTaLoi.Contains("phồng") || moTaLoi.Contains("hao pin") || moTaLoi.Contains("nhanh hết")) keywords.Add("pin");
            if (moTaLoi.Contains("nước") || moTaLoi.Contains("chập") || moTaLoi.Contains("chất lỏng") || moTaLoi.Contains("nóng máy")) keywords.Add("ic nguồn");
            if (moTaLoi.Contains("camera") || moTaLoi.Contains("mờ") || moTaLoi.Contains("không chụp") || moTaLoi.Contains("quay phim")) keywords.Add("camera");
            if (moTaLoi.Contains("nghe gọi") || moTaLoi.Contains("loa") || moTaLoi.Contains("mic") || moTaLoi.Contains("nhỏ")) keywords.Add("loa");

            // Rút trích một số từ khóa thiết bị để thu hẹp kết quả
            string deviceFilter = "";
            var words = tenThietBi.Split(' ');
            if (words.Length > 0)
            {
                // Simple heuristc: lấy chữ đầu tiên hoặc cụm "iPhone", "Samsung"
                deviceFilter = words.FirstOrDefault(w => w.Contains("iphone") || w.Contains("samsung") || w.Contains("xiaomi") || w.Contains("oppo")) ?? "";
            }

            if (!keywords.Any()) 
            {
                 result.GiaiPhap = "Chưa thể nhận diện rõ lỗi. Cần kỹ thuật viên đo đạc phần cứng thêm (Bo mạch / Nguồn).";
                 return result;
            }

            var query = _context.KhoLinhKiens.AsQueryable();
            var suggestedParts = new List<SuggestedPart>();

            foreach (var kw in keywords)
            {
                // Ưu tiên linh kiện khớp với cả Lỗi & Model máy nếu có, không có thì lấy khớp lỗi
                var matchedPart = await query
                    .Where(k => (k.TenLinhKien.ToLower().Contains(kw) || k.DanhMuc.ToLower().Contains(kw)) && 
                                (string.IsNullOrEmpty(deviceFilter) || k.DanhSachModelTuongThich.ToLower().Contains(deviceFilter)))
                    .OrderByDescending(k => k.SoLuongTon) 
                    .FirstOrDefaultAsync();

                // Fallback nếu không khớp được model
                if (matchedPart == null && !string.IsNullOrEmpty(deviceFilter)) 
                {
                     matchedPart = await query
                        .Where(k => (k.TenLinhKien.ToLower().Contains(kw) || k.DanhMuc.ToLower().Contains(kw)))
                        .OrderByDescending(k => k.SoLuongTon) 
                        .FirstOrDefaultAsync();
                }

                if (matchedPart != null && !suggestedParts.Any(p => p.LinhKienId == matchedPart.Id))
                {
                    suggestedParts.Add(new SuggestedPart
                    {
                        LinhKienId = matchedPart.Id,
                        TenLinhKien = matchedPart.TenLinhKien,
                        Gia = matchedPart.GiaBan,
                        TonKho = matchedPart.SoLuongTon,
                        BaoHanhThang = matchedPart.DanhMuc == "Pin" ? 12 : 6
                    });
                }
            }

            result.Parts = suggestedParts;
            decimal tienCongThuong = 150000;
            decimal tienPhuPhiPhucTap = suggestedParts.Any(p => p.TenLinhKien.ToLower().Contains("ic")) ? 200000 : 0;
            
            result.TienCong = tienCongThuong + tienPhuPhiPhucTap;
            result.TongTienTamTinh = suggestedParts.Sum(p => p.Gia) + result.TienCong; 
            
            if (suggestedParts.Any())
            {
                result.GiaiPhap = "Dựa vào dấu hiệu, hệ thống chuẩn đoán máy có thể bị lỗi thiết bị ngoại vi và cần thay thế linh kiện chuyên dụng.";
            }

            return result;
        }
    }

    public class DiagnosisResult
    {
        public List<SuggestedPart> Parts { get; set; } = new();
        public decimal TongTienTamTinh { get; set; }
        public decimal TienCong { get; set; }
        public string GiaiPhap { get; set; } = string.Empty;
    }

    public class SuggestedPart
    {
        public string LinhKienId { get; set; } = string.Empty;
        public string TenLinhKien { get; set; } = string.Empty;
        public decimal Gia { get; set; }
        public int TonKho { get; set; }
        public int BaoHanhThang { get; set; }
    }
}
