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
        private readonly bool _enforceClinicalPolicyForSuperAdmin;
        private readonly bool _authorizationDisabled;

        public AccessPermissionService(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _enforceClinicalPolicyForSuperAdmin = configuration.GetValue<bool>(
                "Security:Authorization:EnforceClinicalPolicyForSuperAdmin");

            // Saklar untuk mematikan SELURUH pemeriksaan hak akses selama pengembangan,
            // ketika izin per departemen dan jabatan belum ditetapkan dan pemeriksaannya
            // hanya menghalangi pengujian alur.
            //
            // Saklar ini SENGAJA tidak berlaku di produksi. Mematikan otorisasi di sana
            // berarti siapa pun yang berhasil login dapat membuka rekam medis pasien mana
            // pun dan menghapus data apa pun. Karena itu nilai konfigurasinya diabaikan
            // begitu lingkungannya produksi, bukan sekadar diberi peringatan.
            _authorizationDisabled =
                !configuration.GetValue("Security:Authorization:Enabled", true) &&
                !environment.IsProduction();
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

            // Pengguna tetap wajib login. Yang dilepas hanya pemeriksaan hak akses
            // per controller dan aksi, bukan autentikasinya.
            if (_authorizationDisabled)
            {
                return true;
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

            var isSuperAdmin = IsSuperAdminUser(user, roles);

            if (isSuperAdmin && !_enforceClinicalPolicyForSuperAdmin)
            {
                return true;
            }

            if (isSuperAdmin)
            {
                var systemOnlyAction = await _dbContext.SysActionAccesses
                    .AsNoTracking()
                    .Where(x =>
                        x.ActionName == actionName &&
                        x.IsActive &&
                        !x.IsDelete &&
                        x.ControllerAccess != null &&
                        x.ControllerAccess.ControllerName == controllerName &&
                        x.ControllerAccess.IsActive &&
                        !x.ControllerAccess.IsDelete)
                    .Select(x => new
                    {
                        ActionIsSystemOnly = x.IsSystemOnly,
                        ControllerIsSystemOnly = x.ControllerAccess!.IsSystemOnly
                    })
                    .FirstOrDefaultAsync();

                if (systemOnlyAction == null)
                {
                    return false;
                }

                if (systemOnlyAction.ActionIsSystemOnly || systemOnlyAction.ControllerIsSystemOnly)
                {
                    return true;
                }
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
