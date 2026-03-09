namespace TechPro.Models.ViewModels
{
    // ViewModel cho Landing Page
    public class LandingPageViewModel
    {
        public List<DichVuViewModel> DichVus { get; set; } = new();
        public List<ChinhSachViewModel> ChinhSaches { get; set; } = new();
        public ThongTinLienHeViewModel ThongTinLienHe { get; set; } = new();
    }

    public class DichVuViewModel
    {
        public string Ten { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }

    public class ChinhSachViewModel
    {
        public string TieuDe { get; set; } = string.Empty;
        public string MoTa { get; set; } = string.Empty;
    }

    public class ThongTinLienHeViewModel
    {
        public string DiaChi { get; set; } = string.Empty;
        public string Hotline { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string EmailBusiness { get; set; } = string.Empty;
    }

    // ViewModel cho Search
    public class SearchViewModel
    {
        public string Query { get; set; } = string.Empty;
        public string Mode { get; set; } = "repair"; // repair | warranty
        public string? ErrorMessage { get; set; }
    }

    // ViewModel cho Warranty Check Result
    public class WarrantyCheckViewModel
    {
        public bool IsValid { get; set; }
        public string? EndDate { get; set; }
        public string? Model { get; set; }
        public string? PurchaseDate { get; set; }
        public string SerialNumber { get; set; } = string.Empty;
    }
}

