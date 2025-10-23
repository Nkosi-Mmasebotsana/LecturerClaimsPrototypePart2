using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IClaimService
    {
        List<Claim> GetAllClaims();
        Claim? GetClaimById(int id);
        List<Claim> GetClaimsByLecturer(int lecturerId);
        List<Claim> GetPendingClaims();
        void SubmitClaim(Claim claim);
        void ApproveClaim(int claimId, int approverId);
        void RejectClaim(int claimId, int approverId, string comment);
        void AddDocumentToClaim(int claimId, SupportingDocument document);
        void CalculateClaimTotal(Claim claim);
    }
}