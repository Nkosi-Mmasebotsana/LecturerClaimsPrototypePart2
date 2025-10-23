using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public class ClaimService : IClaimService
    {
        private static List<Claim> _claims = new List<Claim>();
        private static int _nextClaimId = 3;
        private static int _nextDocumentId = 2;

        static ClaimService()
        {
            // Initialize with sample data from your prototype
            _claims = new List<Claim>
            {
                new Claim
                {
                    ClaimId = 1,
                    LecturerId = 101,
                    Month = "September 2025",
                    TotalHours = 16,
                    TotalAmount = 8000,
                    Status = "Submitted",
                    SubmittedAt = DateTime.Now.AddDays(-3),
                    ClaimLines = new List<ClaimLine>
                    {
                        new ClaimLine { ClaimLineId = 1, ClaimId = 1, Description = "Lecture: Module C108", HoursWorked = 8, RatePerHour = 500, Subtotal = 4000 },
                        new ClaimLine { ClaimLineId = 2, ClaimId = 1, Description = "Preparation", HoursWorked = 4, RatePerHour = 500, Subtotal = 2000 }
                    },
                    Documents = new List<SupportingDocument>
                    {
                        new SupportingDocument { DocumentId = 1, ClaimId = 1, FileName = "attendance_Sept2025.pdf", FilePath = "/uploads/attendance_Sept2025.pdf", UploadedAt = DateTime.Now.AddDays(-3) }
                    }
                }
            };
        }

        public List<Claim> GetAllClaims() => _claims;

        public Claim? GetClaimById(int id) => _claims.FirstOrDefault(c => c.ClaimId == id);

        public List<Claim> GetClaimsByLecturer(int lecturerId) =>
            _claims.Where(c => c.LecturerId == lecturerId).ToList();

        public List<Claim> GetPendingClaims() =>
            _claims.Where(c => c.Status == "Submitted" || c.Status == "Pending").ToList();

        public void SubmitClaim(Claim claim)
        {
            claim.ClaimId = _nextClaimId++;
            claim.Status = "Submitted";
            claim.SubmittedAt = DateTime.Now;
            CalculateClaimTotal(claim);
            _claims.Add(claim);
        }

        public void ApproveClaim(int claimId, int approverId)
        {
            var claim = GetClaimById(claimId);
            if (claim != null)
            {
                claim.Status = "Approved";
                claim.ApprovedBy = approverId;
                claim.ApprovedAt = DateTime.Now;
            }
        }

        public void RejectClaim(int claimId, int approverId, string comment)
        {
            var claim = GetClaimById(claimId);
            if (claim != null)
            {
                claim.Status = "Rejected";
                claim.ApprovedBy = approverId;
                claim.ApprovedAt = DateTime.Now;
                claim.RejectionComment = comment;
            }
        }

        public void AddDocumentToClaim(int claimId, SupportingDocument document)
        {
            var claim = GetClaimById(claimId);
            if (claim != null)
            {
                document.DocumentId = _nextDocumentId++;
                document.UploadedAt = DateTime.Now;
                claim.Documents.Add(document);
            }
        }

        public void CalculateClaimTotal(Claim claim)
        {
            claim.TotalAmount = claim.ClaimLines.Sum(line => line.Subtotal);
            claim.TotalHours = claim.ClaimLines.Sum(line => line.HoursWorked);
        }
    }
}