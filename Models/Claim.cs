using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        [Key]
        public int ClaimId { get; set; }

        [Required]
        public int LecturerId { get; set; }

        [Required]
        [StringLength(50)]
        public string Month { get; set; } = string.Empty;

        [Required]
        [Range(0.1, 1000)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalHours { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Draft";

        public DateTime SubmittedAt { get; set; } = DateTime.Now;

        // Approval fields
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionComment { get; set; }

        // Navigation properties
        [ForeignKey("LecturerId")]
        public virtual Lecturer Lecturer { get; set; } = null!;

        public virtual ICollection<ClaimLine> ClaimLines { get; set; } = new List<ClaimLine>();
        public virtual ICollection<SupportingDocument> Documents { get; set; } = new List<SupportingDocument>();
    }
}