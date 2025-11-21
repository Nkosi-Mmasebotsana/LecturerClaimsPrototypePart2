using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimLine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Add this line
        public int ClaimLineId { get; set; }

        [Required]
        public int ClaimId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 1000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HoursWorked { get; set; }

        [Required]
        [Range(1, 10000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal RatePerHour { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [ForeignKey("ClaimId")]
        public virtual Claim Claim { get; set; } = null!;
    }
}