using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ClaimController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private const long MaxFileSize = 5 * 1024 * 1024; // 5MB
        private static readonly string[] AllowedExtensions = { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };

        // Static in-memory data
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

        // LECTURER DASHBOARD
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

        // CREATE CLAIM VIEW
        public IActionResult Create(int lecturerId = 101)
        {
            var lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == lecturerId);
            if (lecturer == null) return NotFound();

            ViewBag.Lecturer = lecturer;
            return View(new Claim { LecturerId = lecturerId, ClaimLines = new List<ClaimLine>() });
        }

        // SUBMIT CLAIM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(Claim claim, List<IFormFile> documents)
        {
            try
            {
                var lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == claim.LecturerId);
                if (lecturer == null) return NotFound();

                // Compute totals
                claim.ClaimId = Claims.Count > 0 ? Claims.Max(c => c.ClaimId) + 1 : 1;
                claim.SubmittedAt = DateTime.Now;
                claim.Status = "Submitted";
                claim.TotalHours = claim.ClaimLines.Sum(l => l.HoursWorked);
                claim.TotalAmount = claim.ClaimLines.Sum(l => l.HoursWorked * l.RatePerHour);

                // Handle documents
                claim.Documents = new List<SupportingDocument>();
                if (documents != null && documents.Any())
                {
                    foreach (var file in documents)
                    {
                        if (file.Length > 0)
                        {
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
                }

                Claims.Add(claim);
                TempData["Message"] = "Claim submitted successfully!";
                return RedirectToAction("LecturerDashboard", new { lecturerId = claim.LecturerId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error submitting claim: {ex.Message}";
                return RedirectToAction("Create", new { lecturerId = claim.LecturerId });
            }
        }

        // APPROVER DASHBOARD
        public IActionResult ApproverDashboard(string role = "Programme Coordinator")
        {
            var pendingClaims = Claims.Where(c => c.Status == "Submitted")
                                      .OrderBy(c => c.SubmittedAt)
                                      .ToList();

            // Assign lecturer info
            foreach (var claim in pendingClaims)
            {
                claim.Lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == claim.LecturerId);
            }

            ViewBag.Role = role;
            return View(pendingClaims);
        }

        // DETAILS
        public IActionResult Details(int id)
        {
            var claim = Claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            claim.Lecturer = Lecturers.FirstOrDefault(l => l.LecturerId == claim.LecturerId);
            ViewBag.Users = Users;
            return View(claim);
        }

        // APPROVE CLAIM
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
            return RedirectToAction("ApproverDashboard");
        }

        // REJECT CLAIM
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Reject(int id, int approverId, string comment)
        {
            var claim = Claims.FirstOrDefault(c => c.ClaimId == id);
            if (claim == null) return NotFound();

            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["Error"] = "Rejection comment is required.";
                return RedirectToAction("ApproverDashboard");
            }

            claim.Status = "Rejected";
            claim.RejectedBy = approverId;
            claim.RejectedAt = DateTime.Now;
            claim.RejectionComment = comment;
            TempData["Message"] = $"Claim #{id} rejected!";
            return RedirectToAction("ApproverDashboard");
        }

        // FILE UPLOAD HANDLER
        private async Task<(bool Success, string FileName, string FilePath)> HandleFileUpload(IFormFile file)
        {
            try
            {
                if (file.Length > MaxFileSize) throw new Exception("File too large.");
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(ext)) throw new Exception("Invalid file type.");

                var uploads = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var uniqueFile = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(uploads, uniqueFile);

                using (var stream = new FileStream(path, FileMode.Create))
                    await file.CopyToAsync(stream);

                return (true, file.FileName, $"/uploads/{uniqueFile}");
            }
            catch
            {
                return (false, "", "");
            }
        }
    }
}
