
using ContractMonthlyClaimSystem.Services;

namespace ContractMonthlyClaimSystem.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuthService authService)
        {
            // Skip auth for login page and static files
            var path = context.Request.Path;
            if (path.StartsWithSegments("/Auth/Login") ||
                path.StartsWithSegments("/lib") ||
                path.StartsWithSegments("/css") ||
                path.StartsWithSegments("/js") ||
                path.StartsWithSegments("/uploads"))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (!authService.IsLoggedIn())
            {
                context.Response.Redirect("/Auth/Login");
                return;
            }

            // Role-based access control
            var user = authService.GetCurrentUser();
            if (user != null)
            {
                // Define accessible routes for each role
                var accessible = CanAccess(path, user.Role);
                if (!accessible)
                {
                    context.Response.Redirect("/Auth/AccessDenied");
                    return;
                }
            }

            await _next(context);
        }

        private bool CanAccess(PathString path, string role)
        {
            var accessibleRoutes = new Dictionary<string, List<string>>
            {
                ["HR"] = new List<string> { "/HR/", "/Claim/", "/Home/", "/Auth/" },
                ["Programme Coordinator"] = new List<string> { "/Claim/ApproverDashboard", "/Claim/Details", "/Home/", "/Auth/" },
                ["Academic Manager"] = new List<string> { "/Claim/ApproverDashboard", "/Claim/Details", "/Home/", "/Auth/" },
                ["Lecturer"] = new List<string> { "/Claim/LecturerDashboard", "/Claim/Create", "/Claim/Submit", "/Claim/MyClaims", "/Home/", "/Auth/" }
            };

            return accessibleRoutes[role].Any(route => path.StartsWithSegments(route));
        }
    }
}