using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Lecturer
    {
        [Key]
        public int LecturerId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(1, 10000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; }

        // Navigation property
        public virtual ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}