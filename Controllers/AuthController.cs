using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        private readonly Dictionary<string, (string Controller, string Action)> _roleRedirects =
            new()
            {
                { "HR", ("HR", "Dashboard") },
                { "Programme Coordinator", ("Claim", "ApproverDashboard") },
                { "Academic Manager", ("Claim", "ApproverDashboard") },
                { "Lecturer", ("Claim", "LecturerDashboard") }
            };

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
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

            if (_roleRedirects.TryGetValue(user.Role, out var target))
                return RedirectToAction(target.Action, target.Controller);

            _authService.Logout();
            TempData["Error"] = "Your role is not recognized.";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            _authService.Logout();
            TempData["Message"] = "You have been logged out successfully.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied() => View();
    }
}
