using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class ChiTietDieuChuyen
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PhieuDieuChuyenId { get; set; } = string.Empty;

        [ForeignKey("PhieuDieuChuyenId")]
        public virtual PhieuDieuChuyen? PhieuDieuChuyen { get; set; }

        [Required]
        [StringLength(50)]
        public string LinhKienId { get; set; } = string.Empty;

        [ForeignKey("LinhKienId")]
        public virtual KhoLinhKien? LinhKien { get; set; }

        [Required]
        public int SoLuongYeuCau { get; set; } = 1;

        public int? SoLuongThucThuat { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }
    }
}
