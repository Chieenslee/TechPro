using System.ComponentModel.DataAnnotations;

namespace TechPro.Models
{
    public class RevenueDaily
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public DateTime Ngay { get; set; }

        [DataType(DataType.Currency)]
        public decimal DoanhThu { get; set; }

        [StringLength(50)]
        public string? TenantId { get; set; }
    }
}

