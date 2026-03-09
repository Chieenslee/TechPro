using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TechPro.Models
{
    public class NguoiDung : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string TenDayDu { get; set; } = string.Empty;

        [NotMapped]
        public string Name
        {
            get => TenDayDu;
            set => TenDayDu = value;
        }

        [NotMapped]
        public string Role { get; set; } = "store_admin";

        [StringLength(50)]
        public string? AvatarUrl { get; set; }

        [StringLength(50)]
        public string? TenantId { get; set; }

        [ForeignKey("TenantId")]
        public virtual CuaHang? CuaHang { get; set; }

        // Navigation properties
        public virtual ICollection<PhieuSuaChua> PhieuSuaChuas { get; set; } = new List<PhieuSuaChua>();
    }
}

