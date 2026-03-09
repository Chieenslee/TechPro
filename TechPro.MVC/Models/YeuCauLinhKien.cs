using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.Models
{
    public class YeuCauLinhKien
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50)]
        public string PhieuSuaChuaId { get; set; } = string.Empty;

        [ForeignKey("PhieuSuaChuaId")]
        public virtual PhieuSuaChua PhieuSuaChua { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string TenKyThuatVien { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LinhKienId { get; set; } = string.Empty;

        [ForeignKey("LinhKienId")]
        public virtual KhoLinhKien LinhKien { get; set; } = null!;

        [Required]
        public int SoLuong { get; set; } = 1;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "pending"; // pending | approved | rejected

        [Required]
        public DateTime NgayYeuCau { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? GiaTaiThoiDiemYeuCau { get; set; }
    }
}

