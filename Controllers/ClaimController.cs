using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    [Route("[controller]/[action]")]
    public class ClaimController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };

        // In-memory data
        private static readonly List<Lecturer> Lecturers = new()
        {
            new Lecturer {LecturerId = 101, FullName = "Dr. Tumi N.", Email = "tumi@gmail.com", HourlyRate = 500 },
            new Lecturer {LecturerId = 102, FullName = "Mrs. Kgosi S.", Email = "kgosi@yahoo.com", HourlyRate = 400 }
        };

        private static readonly List<User> Users = new()
        {
            new User {UserId = 1, FullName = "Kholo Nkosi", Role = "Programme Coordinator" },
            new User {UserId = 2, FullName = "Siya Sepuru", Role = "Academic Manager" }
        };

        private static readonly List<Claim> Claims = new();

        public ClaimController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // ---------- LECTURER DASHBOARD ----------

        [HttpGet]
        public IActionResult LecturerDashboard(int lecturerId = 101)
        {
            var lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == lecturerId);
            if (lecturer == null) return NotFound();

            var claims = Claims.Where(c => c.LecturerId == lecturerId)
                               .OrderByDescending(c => c.SubmittedAt)
                               .ToList();

            ViewBag.Lecturer = lecturer;
            return View(claims);
        }

        [HttpGet]
        public IActionResult Create(int lecturerId = 101)
        {
            var lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == lecturerId);
            if (lecturer == null) return NotFound();

            ViewBag.Lecturer = lecturer;
            return View(new Claim { LecturerId = lecturerId, ClaimLines = new List<ClaimLine>() });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim claim, List<IFormFile> documents)
        {
            try
            {
                var lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == claim.LecturerId);
                if (lecturer == null) return NotFound();

                claim.ClaimLines ??= new List<ClaimLine>();

                claim.ClaimId = Claims.Any() ? Claims.Max(c => c.ClaimId) + 1 : 1;
                claim.SubmittedAt = DateTime.Now;
                claim.Status = "Submitted";

                claim.TotalHours = 0;
                claim.TotalAmount = 0;
                for (int i = 0; i < claim.ClaimLines.Count; i++)
                {
                    var line = claim.ClaimLines[i];
                    line.ClaimLineId = i + 1;
                    line.ClaimId = claim.ClaimId;
                    line.Subtotal = line.HoursWorked * line.RatePerHour;
                    claim.TotalHours += line.HoursWorked;
                    claim.TotalAmount += line.Subtotal;
                }

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
                                DocumentId = claim.Documents.Count + 1,
                                ClaimId = claim.ClaimId,
                                FileName = result.FileName,
                                FilePath = result.FilePath,
                                FileSize = file.Length,
                                ContentType = file.ContentType,
                                UploadedAt = DateTime.Now
                            });
                        }
                    }
                }

                Claims.Add(claim);
                TempData["Message"] = "Claim submitted successfully!";
                return RedirectToAction(nameof(LecturerDashboard), new { lecturerId = claim.LecturerId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error submitting claim: {ex.Message}";
                return RedirectToAction(nameof(Create), new { lecturerId = claim.LecturerId });
            }
        }

        // ---------- APPROVER DASHBOARD ----------

        [HttpGet]
        public IActionResult ApproverDashboard(string role = "Programme Coordinator")
        {
            var pendingClaims = Claims.Where(c => c.Status == "Submitted")
                                      .OrderBy(c => c.SubmittedAt)
                                      .ToList();

            foreach (var claim in pendingClaims)
                claim.Lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == claim.LecturerId);

            ViewBag.Role = role;
            return View(pendingClaims);
        }

        [HttpGet("{id}")]
        public IActionResult Details(int id)
        {
            var claim = Claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            claim.Lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == claim.LecturerId);
            ViewBag.Users = Users;
            return View(claim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Approve(int id, int approverId)
        {
            var claim = Claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            claim.Status = "Approved";
            claim.ApprovedBy = approverId;
            claim.ApprovedAt = DateTime.Now;
            TempData["Message"] = $"Claim #{id} approved!";
            return RedirectToAction(nameof(ApproverDashboard));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, int approverId, string comment)
        {
            var claim = Claims.FirstOrDefault(c => c.ClaimId == id);
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
            TempData["Message"] = $"Claim #{id} rejected!";
            return RedirectToAction(nameof(ApproverDashboard));
        }

        // ---------- LECTURERS TAB ----------

        [HttpGet]
        public IActionResult LecturersList()  // renamed method
        {
            return View(Lecturers.OrderBy(l => l.LecturerId).ToList());
        }

        [HttpGet]
        public IActionResult AddLecturer()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddLecturer(Lecturer lecturer)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all required fields.";
                return View(lecturer);
            }

            lecturer.LecturerId = Lecturers.Any() ? Lecturers.Max(l => l.LecturerId) + 1 : 1;
            Lecturers.Add(lecturer);

            TempData["Message"] = $"Lecturer {lecturer.FullName} added successfully!";
            return RedirectToAction(nameof(LecturersList)); // updated redirect
        }

        // ---------- HELPER ----------
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
