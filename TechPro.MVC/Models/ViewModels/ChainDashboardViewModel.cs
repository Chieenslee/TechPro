using TechPro.Models;

namespace TechPro.Models.ViewModels
{
    public class ChainDashboardViewModel
    {
        public string ActiveTab { get; set; } = "overview";
        public List<CuaHang> Stores { get; set; } = new();
        public List<NguoiDung> Users { get; set; } = new();
        public List<KhoLinhKien> InventoryItems { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int ActiveStoresCount { get; set; }
        public int TotalStaff { get; set; }
    }
}

