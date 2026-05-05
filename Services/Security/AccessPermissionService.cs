using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Services.Security
{
    public class AccessPermissionService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccessPermissionService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<bool> HasAccessAsync(
            ClaimsPrincipal userPrincipal,
            string controllerName,
            string actionName)
        {
            if (userPrincipal.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            var userIdText =
                userPrincipal.FindFirstValue("user_id") ??
                userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdText, out var userId))
            {
                return false;
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return false;
            }

            if (!user.IsActive)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (roles.Any(x => x.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            if (user.DepartmentId == null || user.PositionId == null)
            {
                return false;
            }

            var hasPolicy = await _dbContext.SysAccessPolicies
                .AsNoTracking()
                .AnyAsync(x =>
                    x.DepartmentId == user.DepartmentId.Value &&
                    x.PositionId == user.PositionId.Value &&
                    x.IsAllowed &&
                    x.IsActive &&
                    !x.IsDelete &&
                    x.ControllerAccess != null &&
                    x.ControllerAccess.ControllerName == controllerName &&
                    x.ControllerAccess.IsActive &&
                    !x.ControllerAccess.IsDelete &&
                    x.ActionAccess != null &&
                    x.ActionAccess.ActionName == actionName &&
                    x.ActionAccess.IsActive &&
                    !x.ActionAccess.IsDelete
                );

            return hasPolicy;
        }
    }
}