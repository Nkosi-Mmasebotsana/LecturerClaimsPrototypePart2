using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class HRController : Controller
    {
        private readonly AppDbContext _context;

        public HRController(AppDbContext context)
        {
            _context = context;
        }

        // HR Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var stats = new
            {
                TotalLecturers = await _context.Lecturers.CountAsync(),
                TotalClaims = await _context.Claims.CountAsync(),
                PendingClaims = await _context.Claims.CountAsync(c => c.Status == "Submitted"),
                ApprovedClaims = await _context.Claims.CountAsync(c => c.Status == "Approved")
            };

            ViewBag.Stats = stats;
            return View();
        }

        // Manage Lecturers
        public async Task<IActionResult> ManageLecturers()
        {
            var lecturers = await _context.Lecturers.ToListAsync();
            return View(lecturers);
        }

        [HttpGet]
        public IActionResult AddLecturer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLecturer(Lecturer lecturer)
        {
            if (ModelState.IsValid)
            {
                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Lecturer added successfully!";
                return RedirectToAction(nameof(ManageLecturers));
            }
            return View(lecturer);
        }

        [HttpGet]
        public async Task<IActionResult> EditLecturer(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer == null) return NotFound();
            return View(lecturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLecturer(Lecturer lecturer)
        {
            if (ModelState.IsValid)
            {
                _context.Lecturers.Update(lecturer);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Lecturer updated successfully!";
                return RedirectToAction(nameof(ManageLecturers));
            }
            return View(lecturer);
        }

        // Generate Reports
        public async Task<IActionResult> GenerateReports()
        {
            var approvedClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.ClaimLines)
                .Where(c => c.Status == "Approved")
                .ToListAsync();

            return View(approvedClaims);
        }

        // View All Claims
        public async Task<IActionResult> ViewAllClaims()
        {
            var claims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.ClaimLines)
                .Include(c => c.Documents)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            return View(claims);
        }
    }
}