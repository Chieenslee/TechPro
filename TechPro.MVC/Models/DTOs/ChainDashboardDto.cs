using TechPro.Models;

namespace TechPro.Models.DTOs
{
    public class ChainDashboardDto
    {
        public List<CuaHang> Stores { get; set; } = new();
        public List<NguoiDung> Users { get; set; } = new();
        public decimal TotalRevenue { get; set; }
        public int TotalStaff { get; set; }
        public List<StoreRevenueDto> ThisMonthStoreRevenues { get; set; } = new();
        public List<UserWithRoleDto> UsersWithRoles { get; set; } = new();
    }

    public class StoreRevenueDto
    {
        public required string TenantId { get; set; }
        public decimal Revenue { get; set; }
    }

    public class UserWithRoleDto
    {
        public required NguoiDung User { get; set; }
        public required string Role { get; set; }
    }
}
