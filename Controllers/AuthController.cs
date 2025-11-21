
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (_authService.IsLoggedIn())
            {
                return RedirectToDashboard();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["Error"] = "Please enter both username and password.";
                return View();
            }

            var user = _authService.Authenticate(username, password);
            if (user == null)
            {
                TempData["Error"] = "Invalid username or password.";
                return View();
            }

            _authService.StoreUserInSession(user);
            TempData["Message"] = $"Welcome back, {user.FullName}!";

            return RedirectToDashboard();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["Message"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        private IActionResult RedirectToDashboard()
        {
            var user = _authService.GetCurrentUser();
            return user?.Role switch
            {
                "HR" => RedirectToAction("Dashboard", "HR"),
                "Programme Coordinator" or "Academic Manager" => RedirectToAction("ApproverDashboard", "Claim"),
                "Lecturer" => RedirectToAction("LecturerDashboard", "Claim"),
                _ => RedirectToAction("Login", "Auth")
            };
        }
    }
}