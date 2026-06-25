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
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.IsActive);

            if (user == null)
            {
                return false;
            }

            var roles = await _userManager.GetRolesAsync(user);

            if (IsSuperAdminUser(user, roles))
            {
                return true;
            }

            var actionAccess = await _dbContext.SysActionAccesses
                .AsNoTracking()
                .Where(x =>
                    x.ActionName == actionName &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsSystemOnly &&
                    x.ControllerAccess != null &&
                    x.ControllerAccess.ControllerName == controllerName &&
                    x.ControllerAccess.IsActive &&
                    !x.ControllerAccess.IsDelete &&
                    !x.ControllerAccess.IsSystemOnly)
                .Select(x => new
                {
                    ActionAccessId = x.Id,
                    x.ControllerAccessId
                })
                .FirstOrDefaultAsync();

            if (actionAccess == null)
            {
                return false;
            }

            var now = DateTime.UtcNow;

            var hasAccess = await (
                from organization in _dbContext.ApplicationUserOrganizations.AsNoTracking()
                join policy in _dbContext.SysAccessPolicies.AsNoTracking()
                    on new
                    {
                        organization.DepartmentId,
                        organization.PositionId
                    }
                    equals new
                    {
                        policy.DepartmentId,
                        policy.PositionId
                    }
                where organization.UserId == user.Id
                      && organization.IsActive
                      && !organization.IsDelete
                      && (!organization.EffectiveStartDate.HasValue ||
                          organization.EffectiveStartDate.Value <= now)
                      && (!organization.EffectiveEndDate.HasValue ||
                          organization.EffectiveEndDate.Value >= now)

                      && policy.ControllerAccessId == actionAccess.ControllerAccessId
                      && policy.ActionAccessId == actionAccess.ActionAccessId
                      && policy.IsAllowed
                      && policy.IsActive
                      && !policy.IsDelete
                select policy.Id
            ).AnyAsync();

            return hasAccess;
        }

        private static bool IsSuperAdminUser(ApplicationUser user, IEnumerable<string> roles)
        {
            if (roles.Any(x => x.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            var userTypeProperty = user.GetType().GetProperty("UserType");
            var userTypeValue = userTypeProperty?.GetValue(user);

            if (userTypeValue == null)
            {
                return false;
            }

            if (userTypeValue is int userTypeInt)
            {
                return userTypeInt == 1;
            }

            if (userTypeValue is long userTypeLong)
            {
                return userTypeLong == 1;
            }

            var valueType = userTypeValue.GetType();
            if (valueType.IsEnum && Enum.TryParse(valueType, "SuperAdmin", true, out var superAdminValue))
            {
                return Equals(userTypeValue, superAdminValue);
            }

            var text = userTypeValue.ToString();
            return text == "1" ||
                   text?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true;
        }
    }
}