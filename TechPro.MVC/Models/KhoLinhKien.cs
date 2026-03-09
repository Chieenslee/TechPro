using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.Models
{
    public class KhoLinhKien
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(200)]
        public string TenLinhKien { get; set; } = string.Empty;

        [Required]
        public int SoLuongTon { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal GiaBan { get; set; } = 0;

        [StringLength(500)]
        public string? DanhSachModelTuongThich { get; set; } // JSON array or comma-separated

        [StringLength(100)]
        public string? DanhMuc { get; set; }

        [StringLength(50)]
        public string? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual CuaHang? CuaHang { get; set; }

        // Navigation properties
        public virtual ICollection<YeuCauLinhKien> YeuCauLinhKiens { get; set; } = new List<YeuCauLinhKien>();
    }
}

