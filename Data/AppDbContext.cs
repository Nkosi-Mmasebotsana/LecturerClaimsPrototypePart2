using System.Collections.Generic;
using System.Reflection.Emit;
using ContractMonthlyClaimSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ContractMonthlyClaimSystem.Data
{
    public class AppDbContext : DbContext  // Changed from IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Claim> Claims { get; set; }
        public DbSet<ClaimLine> ClaimLines { get; set; }
        public DbSet<SupportingDocument> SupportingDocuments { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Claim>()
                .HasOne(c => c.Lecturer)
                .WithMany(l => l.Claims)
                .HasForeignKey(c => c.LecturerId);

            modelBuilder.Entity<ClaimLine>()
                .HasOne(cl => cl.Claim)
                .WithMany(c => c.ClaimLines)
                .HasForeignKey(cl => cl.ClaimId);

            modelBuilder.Entity<SupportingDocument>()
                .HasOne(sd => sd.Claim)
                .WithMany(c => c.Documents)
                .HasForeignKey(sd => sd.ClaimId);

            // Seed initial data
            modelBuilder.Entity<Lecturer>().HasData(
                new Lecturer { LecturerId = 1, FullName = "Dr. Tumi N.", Email = "tumi@gmail.com", HourlyRate = 500 },
                new Lecturer { LecturerId = 2, FullName = "Mrs. Kgosi S.", Email = "kgosi@yahoo.com", HourlyRate = 400 }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    FullName = "Kholo Nkosi",
                    Email = "kholo@hr.com",
                    Username = "kholo.hr",
                    Password = "hr123", // In production, hash passwords!
                    Role = "HR"
                },
                new User
                {
                    UserId = 2,
                    FullName = "Siya Sepuru",
                    Email = "siya@coordinator.com",
                    Username = "siya.coord",
                    Password = "coord123",
                    Role = "Programme Coordinator"
                },
                new User
                {
                    UserId = 3,
                    FullName = "Lerato Mbeki",
                    Email = "lerato@manager.com",
                    Username = "lerato.manager",
                    Password = "manager123",
                    Role = "Academic Manager"
                },
                new User
                {
                    UserId = 4,
                    FullName = "Dr. Tumi N.",
                    Email = "tumi@gmail.com",
                    Username = "tumi.lecturer",
                    Password = "lecturer123",
                    Role = "Lecturer"
                }
                );
        }
    }
}