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
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Skip auth for login/logout/accessdenied pages and static files
            if (path.StartsWith("/auth/login") ||
                path.StartsWith("/auth/logout") ||
                path.StartsWith("/auth/accessdenied") ||
                path.StartsWith("/lib") ||
                path.StartsWith("/css") ||
                path.StartsWith("/js") ||
                path.StartsWith("/uploads"))
            {
                await _next(context);
                return;
            }

            // Check if user is authenticated
            if (!authService.IsLoggedIn())
            {
                context.Response.Redirect("/auth/login");
                return;
            }

            // Role-based access control
            var user = authService.GetCurrentUser();
            if (user != null && !CanAccess(path, user.Role))
            {
                context.Response.Redirect("/auth/accessdenied");
                return;
            }

            await _next(context);
        }

        private bool CanAccess(string path, string role)
        {
            // Define routes each role can access
            var accessibleRoutes = new Dictionary<string, List<string>>
            {
                ["hr"] = new List<string> { "/hr", "/claim", "/auth" },
                ["programme coordinator"] = new List<string> { "/claim", "/auth" },
                ["academic manager"] = new List<string> { "/claim", "/auth" },
                ["lecturer"] = new List<string> { "/claim", "/auth" }
            };

            return accessibleRoutes.TryGetValue(role.ToLower(), out var routes) &&
                   routes.Any(route => path.StartsWith(route));
        }
    }
}
