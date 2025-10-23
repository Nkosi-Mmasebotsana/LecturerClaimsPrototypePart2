using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Lecturer is required")]
        public int LecturerId { get; set; }

        [Required(ErrorMessage = "Month is required")]
        [StringLength(50, ErrorMessage = "Month cannot exceed 50 characters")]
        public string Month { get; set; } = "";

        [Required(ErrorMessage = "Total hours is required")]
        [Range(0.1, 1000, ErrorMessage = "Hours must be between 0.1 and 1000")]
        public decimal TotalHours { get; set; }

        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; } = "Draft";

        public DateTime SubmittedAt { get; set; }

        // APPROVAL INFO
        public int? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }

        // REJECTION INFO (added)
        public int? RejectedBy { get; set; }
        public DateTime? RejectedAt { get; set; }
        public string? RejectionComment { get; set; }

        public List<ClaimLine> ClaimLines { get; set; } = new List<ClaimLine>();
        public List<SupportingDocument> Documents { get; set; } = new List<SupportingDocument>();
        public Lecturer? Lecturer { get; set; }
    }
}
