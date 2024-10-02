using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SoderiaLaNueva_Api.Security
{
    public class RolesHandler(IHttpContextAccessor httpContextAccessor) : AuthorizationHandler<AuthorizeRolesAttribute>
    {
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AuthorizeRolesAttribute requirement)
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var roles = (_httpContextAccessor.HttpContext.User.Claims.First(x => x.Type == ClaimTypes.Role).Value).Split(',').ToList();

            if (!roles.Any(role => requirement.Roles.Contains(role)))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            context.Succeed(requirement);
            return Task.CompletedTask;
        }
    }
}
