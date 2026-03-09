using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.Models
{
    public class ThietBiBan
    {
        [Key]
        [StringLength(100)]
        public string SerialNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public DateTime NgayMua { get; set; }

        [Required]
        public int ThoiHanBaoHanhThang { get; set; } = 12;

        [StringLength(100)]
        public string? TenKhachHang { get; set; }

        [StringLength(50)]
        public string? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual CuaHang? CuaHang { get; set; }
    }
}

