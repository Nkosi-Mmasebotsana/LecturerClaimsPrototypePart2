using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class ClaimLine
    {
        public int ClaimLineId { get; set; }
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string Description { get; set; } = "";

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(0.1, 1000, ErrorMessage = "Hours must be between 0.1 and 1000")]
        public decimal HoursWorked { get; set; }

        [Required(ErrorMessage = "Rate per hour is required")]
        [Range(1, 10000, ErrorMessage = "Rate must be between 1 and 10000")]
        public decimal RatePerHour { get; set; }

        public decimal Subtotal { get; set; }
    }
}