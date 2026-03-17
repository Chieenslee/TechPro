using TechPro.Models;

namespace TechPro.Models.ViewModels
{
    // ViewModel nhẹ để đọc dữ liệu từ API InventoryDashboardDto
    public class InventoryDashboardView
    {
        public List<KhoLinhKien> Inventory { get; set; } = new();
    }
}

