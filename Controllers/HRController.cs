using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Attributes;

namespace ContractMonthlyClaimSystem.Controllers
{
    [AuthorizeRole("HR")]
    public class HRController : Controller
    {
        private readonly AppDbContext _context;
        public HRController(AppDbContext context) => _context = context;

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

        public async Task<IActionResult> ManageLecturers()
        {
            var lecturers = await _context.Lecturers.ToListAsync();
            return View(lecturers);
        }
    }
}
