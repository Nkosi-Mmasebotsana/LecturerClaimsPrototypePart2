using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Services
{
    public interface IAuthService
    {
        User? Authenticate(string username, string password);
        void StoreUserInSession(User user);
        User? GetCurrentUser();
        bool IsLoggedIn();
        void Logout();
        bool HasRole(string role);
    }
}