using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ContractMonthlyClaimSystem.Services;

namespace ContractMonthlyClaimSystem.Attributes
{
    public class AuthorizeRoleAttribute : TypeFilterAttribute
    {
        public AuthorizeRoleAttribute(params string[] roles) : base(typeof(AuthorizeRoleFilter))
        {
            Arguments = new object[] { roles };
        }
    }

    public class AuthorizeRoleFilter : IAuthorizationFilter
    {
        private readonly string[] _roles;
        private readonly IAuthService _authService;

        public AuthorizeRoleFilter(string[] roles, IAuthService authService)
        {
            _roles = roles;
            _authService = authService;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = _authService.GetCurrentUser();
            if (user == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            if (!_roles.Any(r => string.Equals(r, user.Role, StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }
}
