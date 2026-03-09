using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.Models
{
    public class LichHen
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ThietBi { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string ChiNhanh { get; set; } = string.Empty;

        [Required]
        public DateTime NgayHen { get; set; }

        [Required]
        [StringLength(10)]
        public string GioHen { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? MoTaLoi { get; set; }

        [StringLength(50)]
        public string? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual CuaHang? CuaHang { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

