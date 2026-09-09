using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.DTOs.Auth;
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

        /// <summary>
        /// Seluruh pasangan <c>resource : action</c> yang benar-benar boleh dipakai pengguna ini.
        ///
        /// <para>
        /// Dibuat untuk frontend. Tanpa ini layar hanya dapat menyembunyikan aksi sedekat
        /// <b>peran</b>, padahal yang menentukan adalah kewenangan — dan menebak dari nama peran
        /// menghasilkan tombol yang terlihat tetapi selalu ditolak <c>403</c>, atau lebih buruk,
        /// tombol yang tersembunyi padahal petugasnya berhak.
        /// </para>
        ///
        /// <para>
        /// <b>Jawabannya wajib sama persis dengan <see cref="HasAccessAsync"/>.</b> Keduanya
        /// membaca tabel yang sama dengan penyaring yang sama; kalau salah satu berubah, yang
        /// lain wajib ikut. Daftar yang lebih longgar daripada penjaganya menghasilkan tombol
        /// yang menipu; yang lebih ketat menyembunyikan pekerjaan yang sah.
        /// </para>
        /// </summary>
        public async Task<EffectivePermissionSet> GetEffectivePermissionsAsync(
            ClaimsPrincipal userPrincipal)
        {
            var kosong = new EffectivePermissionSet(false, new List<EffectivePermission>());

            if (userPrincipal.Identity?.IsAuthenticated != true)
            {
                return kosong;
            }

            var userIdText =
                userPrincipal.FindFirstValue("user_id") ??
                userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdText, out var userId))
            {
                return kosong;
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == userId &&
                    x.IsActive);

            if (user == null)
            {
                return kosong;
            }

            var roles = await _userManager.GetRolesAsync(user);
            var isSuperAdmin = IsSuperAdminUser(user, roles);

            // Pasangan yang terdaftar dan hidup. Penyaring aktif dan terhapusnya sama dengan
            // yang dipakai HasAccessAsync.
            var terdaftar = _dbContext.SysActionAccesses
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    !x.IsDelete &&
                    x.ControllerAccess != null &&
                    x.ControllerAccess.IsActive &&
                    !x.ControllerAccess.IsDelete);

            // SuperAdmin tanpa penegakan kebijakan klinis: HasAccessAsync menjawab benar untuk
            // pasangan apa pun, jadi daftarnya adalah seluruh pasangan yang terdaftar.
            if (isSuperAdmin && !_enforceClinicalPolicyForSuperAdmin)
            {
                var semua = await terdaftar
                    .Select(x => new
                    {
                        Resource = x.ControllerAccess!.ControllerName,
                        Action = x.ActionName
                    })
                    .Distinct()
                    .ToListAsync();

                return new EffectivePermissionSet(
                    true,
                    semua
                        .Select(x => new EffectivePermission(x.Resource, x.Action))
                        .OrderBy(x => x.Resource)
                        .ThenBy(x => x.Action)
                        .ToList());
            }

            var now = DateTime.UtcNow;

            // Jalur kebijakan. Bentuk join-nya disalin dari HasAccessAsync supaya keduanya
            // tidak dapat menyimpang tanpa terlihat.
            var lewatKebijakan = await (
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
                join action in _dbContext.SysActionAccesses.AsNoTracking()
                    on policy.ActionAccessId equals action.Id
                where organization.UserId == user.Id
                      && organization.IsActive
                      && !organization.IsDelete
                      && (!organization.EffectiveStartDate.HasValue ||
                          organization.EffectiveStartDate.Value <= now)
                      && (!organization.EffectiveEndDate.HasValue ||
                          organization.EffectiveEndDate.Value >= now)

                      && policy.IsAllowed
                      && policy.IsActive
                      && !policy.IsDelete
                      && policy.ControllerAccessId == action.ControllerAccessId

                      && action.IsActive
                      && !action.IsDelete
                      && !action.IsSystemOnly
                      && action.ControllerAccess != null
                      && action.ControllerAccess.IsActive
                      && !action.ControllerAccess.IsDelete
                      && !action.ControllerAccess.IsSystemOnly
                select new
                {
                    Resource = action.ControllerAccess!.ControllerName,
                    Action = action.ActionName
                })
                .Distinct()
                .ToListAsync();

            var hasil = lewatKebijakan
                .Select(x => new EffectivePermission(x.Resource, x.Action))
                .ToList();

            // SuperAdmin dengan penegakan kebijakan klinis tetap memegang pasangan bertanda
            // system-only, persis seperti cabang yang sama pada HasAccessAsync.
            if (isSuperAdmin)
            {
                var systemOnly = await terdaftar
                    .Where(x => x.IsSystemOnly || x.ControllerAccess!.IsSystemOnly)
                    .Select(x => new
                    {
                        Resource = x.ControllerAccess!.ControllerName,
                        Action = x.ActionName
                    })
                    .Distinct()
                    .ToListAsync();

                hasil.AddRange(systemOnly.Select(x => new EffectivePermission(x.Resource, x.Action)));
            }

            return new EffectivePermissionSet(
                isSuperAdmin,
                hasil
                    .Distinct()
                    .OrderBy(x => x.Resource)
                    .ThenBy(x => x.Action)
                    .ToList());
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
