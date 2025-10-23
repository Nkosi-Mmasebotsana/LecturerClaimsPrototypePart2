using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class SupportingDocument
    {
        public int DocumentId { get; set; }
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "File name is required")]
        public string FileName { get; set; } = "";

        public string FilePath { get; set; } = "";
        public long FileSize { get; set; }
        public string ContentType { get; set; } = "";
        public DateTime UploadedAt { get; set; }
    }
}