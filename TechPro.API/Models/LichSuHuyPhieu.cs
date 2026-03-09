using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class LichSuHuyPhieu
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
        public string NguoiYeuCauId { get; set; } = string.Empty;

        [ForeignKey("NguoiYeuCauId")]
        public virtual NguoiDung? NguoiYeuCau { get; set; }

        [StringLength(450)]
        public string? NguoiDuyetId { get; set; }

        [ForeignKey("NguoiDuyetId")]
        public virtual NguoiDung? NguoiDuyet { get; set; }

        [Required]
        [StringLength(500)]
        public string LyDoHuy { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "YeuCauHuy"; // YeuCauHuy, DaHuy, TuChoiHuy

        [Required]
        public DateTime NgayYeuCau { get; set; } = DateTime.UtcNow;

        public DateTime? NgayDuyet { get; set; }
    }
}
