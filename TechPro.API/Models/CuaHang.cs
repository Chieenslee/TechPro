using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class CuaHang
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(200)]
        public string TenCuaHang { get; set; } = string.Empty;

        [NotMapped]
        public string Name
        {
            get => TenCuaHang;
            set => TenCuaHang = value;
        }

        [StringLength(500)]
        public string? DiaChi { get; set; }

        [NotMapped]
        public string? Address
        {
            get => DiaChi;
            set => DiaChi = value;
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal DoanhThu { get; set; } = 0;

        [Required]
        [StringLength(20)]
        public string TrangThai { get; set; } = "active"; // active | inactive

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        [StringLength(100)]
        [EmailAddress]
        public string? AdminEmail { get; set; }

        [StringLength(20)]
        public string? Hotline { get; set; }

        [StringLength(500)]
        public string? MauInHoaDon { get; set; }

        // Navigation properties
        public virtual ICollection<NguoiDung> NhanViens { get; set; } = new List<NguoiDung>();
        public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
    }
}

