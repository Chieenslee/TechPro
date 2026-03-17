using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class PhieuSuaChua
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string TenKhachHang { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Phone]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string TenThietBi { get; set; } = string.Empty;

        [StringLength(100)]
        public string? SerialNumber { get; set; }

        public static class Statuses
        {
            public const string Pending = "pending";
            public const string Repairing = "repairing";
            public const string WaitingParts = "waiting_parts";
            public const string Done = "done";
            public const string Delivered = "delivered";
        }

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = Statuses.Pending;

        [Required]
        public DateTime NgayNhan { get; set; } = DateTime.UtcNow;

        // Ngày hoàn thành thực tế — dùng để tính doanh thu đúng kỳ kế toán
        // Được set khi TrangThai chuyển sang "done" hoặc "delivered"
        public DateTime? NgayHoanThanh { get; set; }

        [StringLength(1000)]
        public string? MoTaLoi { get; set; }

        [StringLength(4000)]
        public string? KetQuaKiemTra { get; set; }

        [StringLength(450)]
        public string? KyThuatVienId { get; set; }

        [ForeignKey("KyThuatVienId")]
        public virtual NguoiDung? KyThuatVien { get; set; }

        [StringLength(50)]
        public string? MatKhauManHinh { get; set; }

        public bool? CoBaoHanh { get; set; }

        [StringLength(50)]
        public string? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual CuaHang? CuaHang { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TongTien { get; set; } = 0;

        public bool IsICloudRemoved { get; set; }
        public bool IsFindMyOff { get; set; }

        [StringLength(500)]
        public string? PhuKien { get; set; }

        // Navigation properties
        public virtual ICollection<YeuCauLinhKien> YeuCauLinhKiens { get; set; } = new List<YeuCauLinhKien>();
        public virtual ICollection<TraXac> TraXacs { get; set; } = new List<TraXac>();
    }
}

