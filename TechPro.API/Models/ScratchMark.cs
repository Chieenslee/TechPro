using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.API.Models
{
    public class ScratchMark
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50)]
        public string PhieuSuaChuaId { get; set; } = string.Empty;

        [ForeignKey("PhieuSuaChuaId")]
        public virtual PhieuSuaChua PhieuSuaChua { get; set; } = null!;

        [Range(0, 100)]
        public double X { get; set; }

        [Range(0, 100)]
        public double Y { get; set; }

        [StringLength(450)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? CreatedByName { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

