using System;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Services;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Simple database setup - NO Identity for now
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IClaimService, ClaimService>();
builder.Services.AddScoped<IAuthService, AuthService>(); // Add this line

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor(); // Add this for session access

var app = builder.Build();

// Create database on startup - NO migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated(); // This creates DB and tables without migrations
    Console.WriteLine("Database and tables created successfully!");
}

// Configure pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession(); // Add this before authorization
app.UseMiddleware<AuthMiddleware>(); // Add custom auth middleware

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();