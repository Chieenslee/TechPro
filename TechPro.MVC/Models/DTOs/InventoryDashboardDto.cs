using TechPro.Models;

namespace TechPro.Models.DTOs
{
    public class InventoryDashboardDto
    {
        public List<KhoLinhKien> Inventory { get; set; } = new();
        public List<YeuCauLinhKien> PartRequests { get; set; } = new();
        public List<TraXac> WasteReturns { get; set; } = new();
        public int PendingRequestsCount { get; set; }
        public int PendingWasteCount { get; set; }
    }
}
