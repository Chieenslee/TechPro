using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class PhieuDieuChuyen
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TuCuaHangId { get; set; } = string.Empty;

        [ForeignKey("TuCuaHangId")]
        public virtual CuaHang? TuCuaHang { get; set; }

        [Required]
        [StringLength(50)]
        public string DenCuaHangId { get; set; } = string.Empty;

        [ForeignKey("DenCuaHangId")]
        public virtual CuaHang? DenCuaHang { get; set; }

        [Required]
        [StringLength(450)]
        public string NguoiYeuCauId { get; set; } = string.Empty;

        [ForeignKey("NguoiYeuCauId")]
        public virtual NguoiDung? NguoiYeuCau { get; set; }

        [StringLength(450)]
        public string? NguoiDuyetXuatId { get; set; }

        [ForeignKey("NguoiDuyetXuatId")]
        public virtual NguoiDung? NguoiDuyetXuat { get; set; }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "ChoDuyet"; // ChoDuyet, DaXuat, DangVanChuyen, DaNhan, TuChoi

        [Required]
        public DateTime NgayYeuCau { get; set; } = DateTime.UtcNow;

        public DateTime? NgayXuat { get; set; }
        public DateTime? NgayNhan { get; set; }

        [StringLength(500)]
        public string? GhiChu { get; set; }

        public virtual ICollection<ChiTietDieuChuyen> ChiTietDieuChuyens { get; set; } = new List<ChiTietDieuChuyen>();
    }
}
