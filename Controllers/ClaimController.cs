using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Route("[controller]/[action]")]
    public class ClaimController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AppDbContext _context;

        private const long MaxFileSize = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };

        public ClaimController(IWebHostEnvironment environment, AppDbContext context)
        {
            _environment = environment;
            _context = context;
        }

        // --- LECTURER DASHBOARD ---
        [HttpGet]
        public async Task<IActionResult> LecturerDashboard(int lecturerId = 1)
        {
            var lecturer = await _context.Lecturers.FindAsync(lecturerId);
            if (lecturer == null) return NotFound();

            var claims = await _context.Claims
                .Include(c => c.ClaimLines)
                .Include(c => c.Documents)
                .Where(c => c.LecturerId == lecturerId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            ViewBag.Lecturer = lecturer;
            return View(claims);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int lecturerId = 1)
        {
            var lecturer = await _context.Lecturers.FindAsync(lecturerId);
            if (lecturer == null) return NotFound();

            ViewBag.Lecturer = lecturer;
            return View(new Claim
            {
                LecturerId = lecturerId,
                ClaimLines = new List<ClaimLine>()
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim claim, List<IFormFile> documents)
        {
            try
            {
                var lecturer = await _context.Lecturers.FindAsync(claim.LecturerId);
                if (lecturer == null) return NotFound();

                claim.ClaimLines ??= new List<ClaimLine>();

                // Set claim properties
                claim.SubmittedAt = DateTime.Now;
                claim.Status = "Submitted";
                claim.TotalHours = 0;
                claim.TotalAmount = 0;

                // Calculate line items
                foreach (var line in claim.ClaimLines)
                {
                    line.Subtotal = line.HoursWorked * line.RatePerHour;
                    claim.TotalHours += line.HoursWorked;
                    claim.TotalAmount += line.Subtotal;
                }

                // Handle file uploads
                claim.Documents = new List<SupportingDocument>();
                if (documents != null && documents.Any())
                {
                    foreach (var file in documents)
                    {
                        if (file.Length <= 0) continue;
                        var result = await HandleFileUpload(file);

                        if (result.Success)
                        {
                            claim.Documents.Add(new SupportingDocument
                            {
                                FileName = result.FileName,
                                FilePath = result.FilePath,
                                FileSize = file.Length,
                                ContentType = file.ContentType,
                                UploadedAt = DateTime.Now
                            });
                        }
                    }
                }

                // Save to database
                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(LecturerDashboard), new { lecturerId = claim.LecturerId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error submitting claim: {ex.Message}";
                return RedirectToAction(nameof(Create), new { lecturerId = claim.LecturerId });
            }
        }

        // --- APPROVER DASHBOARD ---
        [HttpGet]
        public async Task<IActionResult> ApproverDashboard(string role = "Programme Coordinator")
        {
            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.ClaimLines)
                .Where(c => c.Status == "Submitted")
                .OrderBy(c => c.SubmittedAt)
                .ToListAsync();

            ViewBag.Role = role;
            ViewBag.Users = await _context.Users.ToListAsync();
            return View(pendingClaims);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var claim = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.ClaimLines)
                .Include(c => c.Documents)
                .FirstOrDefaultAsync(c => c.ClaimId == id);

            if (claim == null) return NotFound();

            ViewBag.Users = await _context.Users.ToListAsync();
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int approverId)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            claim.Status = "Approved";
            claim.ApprovedBy = approverId;
            claim.ApprovedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Claim #{id} approved!";
            return RedirectToAction(nameof(ApproverDashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, int approverId, string comment)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Rejection comment is required.";
                return RedirectToAction(nameof(Details), new { id });
            }

            claim.Status = "Rejected";
            claim.RejectedBy = approverId;
            claim.RejectedAt = DateTime.Now;
            claim.RejectionComment = comment;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Claim #{id} rejected!";
            return RedirectToAction(nameof(ApproverDashboard));
        }

        // --- LECTURERS TAB ---
        [HttpGet]
        public async Task<IActionResult> LecturersList()
        {
            var lecturers = await _context.Lecturers.OrderBy(l => l.LecturerId).ToListAsync();
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
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return View(lecturer);
            }

            // Auto-generate LecturerId if needed, or let database handle it
            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Lecturer {lecturer.FullName} added successfully!";
            return RedirectToAction(nameof(LecturersList));
        }

        // --- HELPER ---
        private async Task<(bool Success, string FileName, string FilePath)> HandleFileUpload(IFormFile file)
        {
            try
            {
                if (file.Length > MaxFileSize) return (false, "", "");
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext)) return (false, "", "");

                var uploads = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var unique = $"{Guid.NewGuid()}{ext}";
                var full = Path.Combine(uploads, unique);

                using (var stream = new FileStream(full, FileMode.Create))
                    await file.CopyToAsync(stream);

                return (true, file.FileName, $"/uploads/{unique}");
            }
            catch
            {
                return (false, "", "");
            }
        }
    }
}