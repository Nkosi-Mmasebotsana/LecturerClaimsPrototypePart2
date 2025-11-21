using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Models;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Services;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Route("[controller]/[action]")]
    public class ClaimController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        private const long MaxFileSize = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };

        public ClaimController(IWebHostEnvironment environment, AppDbContext context, IAuthService authService)
        {
            _environment = environment;
            _context = context;
            _authService = authService;
        }

        // ------------------------- HELPER -----------------------------
        private async Task<Lecturer?> GetCurrentLecturerAsync()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return null;

            // Map user email → lecturer email (your DB is built this way)
            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.Email == user.Email);

            return lecturer;
        }

        // ==================================================================
        //                          LECTURER DASHBOARD
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> LecturerDashboard()
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToAction("AccessDenied", "Auth");

            var claims = await _context.Claims
                .Include(c => c.ClaimLines)
                .Include(c => c.Documents)
                .Where(c => c.LecturerId == lecturer.LecturerId)
                .OrderByDescending(c => c.SubmittedAt)
                .ToListAsync();

            ViewBag.Lecturer = lecturer;
            return View(claims);
        }

        // -------------------- CREATE CLAIM ------------------------
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var lecturer = await GetCurrentLecturerAsync();
            if (lecturer == null) return RedirectToAction("AccessDenied", "Auth");

            ViewBag.Lecturer = lecturer;

            return View(new Claim
            {
                LecturerId = lecturer.LecturerId,
                ClaimLines = new List<ClaimLine>()
            });
        }

        // -------------------- SUBMIT CLAIM ------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim claim, List<IFormFile> documents)
        {
            try
            {
                var lecturer = await GetCurrentLecturerAsync();
                if (lecturer == null) return RedirectToAction("AccessDenied", "Auth");

                // Force claim to be tied to the logged-in lecturer
                claim.LecturerId = lecturer.LecturerId;

                claim.ClaimLines ??= new List<ClaimLine>();

                claim.SubmittedAt = DateTime.Now;
                claim.Status = "Submitted";
                claim.TotalHours = 0;
                claim.TotalAmount = 0;

                foreach (var line in claim.ClaimLines)
                {
                    line.Subtotal = line.HoursWorked * line.RatePerHour;
                    claim.TotalHours += line.HoursWorked;
                    claim.TotalAmount += line.Subtotal;
                }

                // ----------- 180 HOUR LIMIT RULE -------------
                if (claim.TotalHours > 180)
                {
                    TempData["Error"] = "Total hours cannot exceed 180 hours for a monthly claim.";
                    return RedirectToAction(nameof(Create));
                }

                // ---------------- FILE UPLOAD -----------------
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

                // Save to DB
                _context.Claims.Add(claim);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(LecturerDashboard));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error submitting claim: {ex.Message}";
                return RedirectToAction(nameof(Create));
            }
        }

        // ==================================================================
        //                         APPROVER DASHBOARD
        // ==================================================================
        [HttpGet]
        public async Task<IActionResult> ApproverDashboard()
        {
            var user = _authService.GetCurrentUser();
            if (user == null) return RedirectToAction("Login", "Auth");

            var pendingClaims = await _context.Claims
                .Include(c => c.Lecturer)
                .Include(c => c.ClaimLines)
                .Where(c =>
                    c.Status == "Submitted" ||
                    c.Status == "Verified" ||
                    c.Status == "Approved")
                .OrderBy(c => c.SubmittedAt)
                .ToListAsync();

            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.Role = user.Role;

            return View(pendingClaims);
        }

        // -------------------- CLAIM DETAILS ------------------------
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

        // ==================================================================
        //                         APPROVAL WORKFLOW
        // ==================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, int approverId)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            var approver = await _context.Users.FindAsync(approverId);
            if (approver == null) return NotFound();

            switch (approver.Role)
            {
                case "Programme Coordinator":
                    if (claim.Status != "Submitted")
                    {
                        TempData["Error"] = "Only 'Submitted' claims can be Verified.";
                        return RedirectToAction(nameof(ApproverDashboard));
                    }
                    claim.Status = "Verified";
                    claim.ApprovedBy = approverId;
                    claim.ApprovedAt = DateTime.Now;
                    break;

                case "Academic Manager":
                    if (claim.Status != "Verified")
                    {
                        TempData["Error"] = "Only 'Verified' claims can be Approved.";
                        return RedirectToAction(nameof(ApproverDashboard));
                    }
                    claim.Status = "Approved";
                    claim.ApprovedBy = approverId;
                    claim.ApprovedAt = DateTime.Now;
                    break;

                case "HR":
                    if (claim.Status != "Approved")
                    {
                        TempData["Error"] = "Only 'Approved' claims can be Processed by HR.";
                        return RedirectToAction(nameof(ApproverDashboard));
                    }
                    claim.Status = "Processed";
                    claim.ApprovedBy = approverId;
                    claim.ApprovedAt = DateTime.Now;
                    break;

                default:
                    TempData["Error"] = "You are not authorised to approve claims.";
                    return RedirectToAction(nameof(ApproverDashboard));
            }

            await _context.SaveChangesAsync();
            TempData["Message"] = $"Claim #{id} moved to '{claim.Status}'.";
            return RedirectToAction(nameof(ApproverDashboard));
        }

        // -------------------- REJECT ------------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, int approverId, string comment)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null) return NotFound();

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "A rejection reason is required.";
                return RedirectToAction(nameof(Details), new { id });
            }

            claim.Status = "Rejected";
            claim.RejectedBy = approverId;
            claim.RejectedAt = DateTime.Now;
            claim.RejectionComment = comment;

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Claim #{id} rejected successfully.";
            return RedirectToAction(nameof(ApproverDashboard));
        }

        // ==================================================================
        //                         LECTURERS LIST (HR)
        // ==================================================================
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
                TempData["Error"] = "Please complete all required fields.";
                return View(lecturer);
            }

            _context.Lecturers.Add(lecturer);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Lecturer {lecturer.FullName} added successfully!";
            return RedirectToAction(nameof(LecturersList));
        }

        // ==================================================================
        //                         FILE UPLOAD
        // ==================================================================
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
