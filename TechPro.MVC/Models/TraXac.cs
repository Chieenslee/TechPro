using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.Models
{
    public class TraXac
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
        [StringLength(200)]
        public string TenLinhKien { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "pending"; // pending | returned

        [Required]
        public DateTime NgayTra { get; set; } = DateTime.Now;
    }
}

