using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalFeeManagement.FeeCalculation.Services
{
    /// <summary>
    /// Guard dan resolver konteks pembatasan data tenaga medis (MDF-PERM-1.0 §5, MF-DEC-010, BE-MDF-017).
    /// Memisahkan pengguna berhak penuh (Staf/Supervisor/Manajer/Admin) dari tenaga medis penerima jasa (Dokter/Workforce).
    /// </summary>
    public class MedicalFeeAccessGuard
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _permissionService;

        public MedicalFeeAccessGuard(
            ApplicationDbContext dbContext,
            AccessPermissionService permissionService)
        {
            _dbContext = dbContext;
            _permissionService = permissionService;
        }

        /// <summary>
        /// Memetakan ClaimsPrincipal pengguna ke MedicalFeeActorContext.
        /// Menentukan apakah pengguna memiliki hak akses tanpa batas (Staf/Admin) atau dibatasi ke dirinya sendiri (Tenaga Medis).
        /// </summary>
        public async Task<MedicalFeeActorContext> ResolveActorContextAsync(ClaimsPrincipal userPrincipal, CancellationToken ct = default)
        {
            if (userPrincipal?.Identity?.IsAuthenticated != true)
            {
                return new MedicalFeeActorContext { IsUnrestricted = false };
            }

            var userIdText = userPrincipal.FindFirstValue("user_id") ?? userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdText, out var userId);
            var userName = userPrincipal.FindFirstValue(ClaimTypes.Name) ?? userPrincipal.FindFirstValue("username");

            // 1. Cek hak akses administratif / manajemen:
            // SuperAdmin atau peran manajemen yang memegang izin kalkulasi, verifikasi, approval, atau kelola peran.
            bool isSuperAdmin = userPrincipal.IsInRole("SuperAdmin") ||
                                string.Equals(userPrincipal.FindFirstValue("role"), "SuperAdmin", StringComparison.OrdinalIgnoreCase);

            if (isSuperAdmin)
            {
                return MedicalFeeActorContext.CreateUnrestricted(userId, userName);
            }

            // Periksa izin manajemen periode / verifikasi jasa / koreksi / kelola master
            bool canManagePeriods = await _permissionService.HasAccessAsync(userPrincipal, "MedicalFeePeriods", "Read");
            bool canVerifyFees = await _permissionService.HasAccessAsync(userPrincipal, "MedicalFeeServiceFees", "Verify");
            bool canApproveAdjustment = await _permissionService.HasAccessAsync(userPrincipal, "MedicalFeeServiceFees", "ApproveAdjustment");
            bool canManageRoles = await _permissionService.HasAccessAsync(userPrincipal, "MedicalFeeRoles", "Manage");

            // Jika pengguna memiliki izin operasional/manajemen Medical Fee, ia berhak melihat data seluruh penerima (T-84)
            if (canManagePeriods || canVerifyFees || canApproveAdjustment || canManageRoles)
            {
                return MedicalFeeActorContext.CreateUnrestricted(userId, userName);
            }

            // 2. Jika bukan staf/manajemen, periksa apakah pengguna terhubung ke profil dokter atau staf (tenaga medis)
            // Cek claim doctor_id
            var doctorIdClaim = userPrincipal.FindFirstValue("doctor_id") ?? userPrincipal.FindFirstValue("DoctorId");
            if (Guid.TryParse(doctorIdClaim, out var doctorId) && doctorId != Guid.Empty)
            {
                return MedicalFeeActorContext.CreateRestricted(userId, doctorId, MdfPayeeTypes.Doctor, userName);
            }

            // Cek claim workforce_profile_id
            var workforceClaim = userPrincipal.FindFirstValue("workforce_profile_id") ?? userPrincipal.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId) && workforceProfileId != Guid.Empty)
            {
                // Cek apakah workforce ini terhubung ke MstDoctor
                var linkedDoctor = await _dbContext.MstDoctors
                    .AsNoTracking()
                    .Where(d => !d.IsDelete && d.IsActive && d.WorkforceProfileId == workforceProfileId)
                    .Select(d => d.Id)
                    .FirstOrDefaultAsync(ct);

                if (linkedDoctor != Guid.Empty)
                {
                    return MedicalFeeActorContext.CreateRestricted(userId, linkedDoctor, MdfPayeeTypes.Doctor, userName);
                }

                return MedicalFeeActorContext.CreateRestricted(userId, workforceProfileId, MdfPayeeTypes.Workforce, userName);
            }

            // Cek dari tabel Users jika claims tidak mencantumkan ID dokter secara langsung
            if (userId != Guid.Empty)
            {
                var appUser = await _dbContext.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.DoctorId, u.WorkforceProfileId })
                    .FirstOrDefaultAsync(ct);

                if (appUser?.DoctorId.HasValue == true && appUser.DoctorId.Value != Guid.Empty)
                {
                    return MedicalFeeActorContext.CreateRestricted(userId, appUser.DoctorId.Value, MdfPayeeTypes.Doctor, userName);
                }

                if (appUser?.WorkforceProfileId.HasValue == true && appUser.WorkforceProfileId.Value != Guid.Empty)
                {
                    var linkedDoctor = await _dbContext.MstDoctors
                        .AsNoTracking()
                        .Where(d => !d.IsDelete && d.IsActive && d.WorkforceProfileId == appUser.WorkforceProfileId.Value)
                        .Select(d => d.Id)
                        .FirstOrDefaultAsync(ct);

                    if (linkedDoctor != Guid.Empty)
                    {
                        return MedicalFeeActorContext.CreateRestricted(userId, linkedDoctor, MdfPayeeTypes.Doctor, userName);
                    }

                    return MedicalFeeActorContext.CreateRestricted(userId, appUser.WorkforceProfileId.Value, MdfPayeeTypes.Workforce, userName);
                }
            }

            // Default fallback: Jika bukan dokter dan tidak punya izin manajemen, anggap restricted dengan target kosong (menghasilkan 0 data demi keamanan privasi)
            return new MedicalFeeActorContext
            {
                UserId = userId,
                UserName = userName,
                IsUnrestricted = false,
                RestrictedPayeeReferenceId = Guid.Empty,
                RestrictedPayeeType = null
            };
        }
    }
}

