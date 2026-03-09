using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechPro.Models
{
    public class TicketNote
    {
        [Key]
        [StringLength(50)]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [StringLength(50)]
        public string PhieuSuaChuaId { get; set; } = string.Empty;

        [ForeignKey("PhieuSuaChuaId")]
        public virtual PhieuSuaChua PhieuSuaChua { get; set; } = null!;

        [StringLength(450)]
        public string? UserId { get; set; }

        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

