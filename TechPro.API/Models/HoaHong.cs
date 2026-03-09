using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class HoaHong
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string PhieuSuaChuaId { get; set; } = string.Empty;

        [ForeignKey("PhieuSuaChuaId")]
        public virtual PhieuSuaChua? PhieuSuaChua { get; set; }

        [Required]
        [StringLength(450)]
        public string NhanVienId { get; set; } = string.Empty;

        [ForeignKey("NhanVienId")]
        public virtual NguoiDung? NhanVien { get; set; }

        [Required]
        [StringLength(50)]
        public string LoaiHoaHong { get; set; } = "SuaChua"; // SuaChua, BanHang, CSKH

        [Column(TypeName = "decimal(18,2)")]
        public decimal SoTien { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; } = "TamTinh"; // TamTinh, DaChot, DaThanhToan

        [Required]
        public DateTime NgayGhiNhan { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? GhiChu { get; set; }
    }
}
