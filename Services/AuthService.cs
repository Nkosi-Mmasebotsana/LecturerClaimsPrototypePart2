using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Services
{
    public class AuthService : IAuthService  // Make sure it implements IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public User? Authenticate(string username, string password)
        {
            // Simple authentication - in production, passwords should be hashed
            return _context.Users
                .FirstOrDefault(u => u.Username == username && u.Password == password);
        }

        public void StoreUserInSession(User user)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session != null)
            {
                session.SetInt32("UserId", user.UserId);
                session.SetString("Username", user.Username);
                session.SetString("FullName", user.FullName);
                session.SetString("Role", user.Role);
                session.SetString("Email", user.Email);
            }
        }

        public User? GetCurrentUser()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null) return null;

            var userId = session.GetInt32("UserId");
            var username = session.GetString("Username");

            if (userId == null || string.IsNullOrEmpty(username)) 
                return null;

            // Return user from database; if missing, treat as logged out
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId && u.Username == username);
            return user;
        }

        public bool IsLoggedIn()
        {
            return GetCurrentUser() != null;
        }

        public void Logout()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            session?.Clear();
        }

        public bool HasRole(string role)
        {
            var currentUser = GetCurrentUser();
            return currentUser?.Role == role;
        }
    }
}