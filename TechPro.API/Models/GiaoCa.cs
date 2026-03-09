using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class GiaoCa
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string TenantId { get; set; } = string.Empty;

        [ForeignKey("TenantId")]
        public virtual CuaHang? CuaHang { get; set; }

        [Required]
        [StringLength(450)]
        public string NguoiGiaoId { get; set; } = string.Empty;

        [ForeignKey("NguoiGiaoId")]
        public virtual NguoiDung? NguoiGiao { get; set; }

        [StringLength(450)]
        public string? NguoiNhanId { get; set; }

        [ForeignKey("NguoiNhanId")]
        public virtual NguoiDung? NguoiNhan { get; set; }

        [Required]
        public DateTime ThoiGianGiao { get; set; } = DateTime.UtcNow;

        public DateTime? ThoiGianNhan { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TienMatBanGiao { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal ThucNhanTienMat { get; set; } = 0;

        [StringLength(20)]
        public string TrangThai { get; set; } = "ChoNhan"; // ChoNhan, DaNhan, ChoDuyetQuy, DaChotQuy

        [StringLength(500)]
        public string? GhiChu { get; set; }
    }
}
