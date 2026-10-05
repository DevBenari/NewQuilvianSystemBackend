using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services
{
    /// <summary>
    /// Cakupan pengguna yang login pada Daftar Pasien Rawat Jalan (RJ-DOC-DEC-013, RJ-DOC-DEC-014).
    /// </summary>
    /// <param name="CanReadAll">Memegang <c>OutpatientEncounter : ReadAll</c>.</param>
    /// <param name="DoctorId">Data dokter milik pengguna, bila ada.</param>
    /// <param name="ClinicIds">Poli tempat pengguna bertugas sebagai perawat nurse station.</param>
    public sealed record ClinicalActorScope(bool CanReadAll, Guid? DoctorId, IReadOnlyList<Guid> ClinicIds)
    {
        public bool HasAnyScope => CanReadAll || DoctorId.HasValue || ClinicIds.Count > 0;

        public string Label
        {
            get
            {
                if (CanReadAll)
                    return "Semua klinik";

                if (DoctorId.HasValue && ClinicIds.Count > 0)
                    return "Pasien Anda dan klinik cluster Anda";

                return DoctorId.HasValue ? "Pasien Anda" : "Klinik cluster Anda";
            }
        }
    }

    /// <summary>
    /// Mengenali pengguna yang login sebagai dokter dan/atau perawat nurse station.
    /// </summary>
    /// <remarks>
    /// Urutan pengenalannya disalin dari <c>DoctorQueueController.ResolveAllowedDoctorIdAsync</c>
    /// dan <c>NurseStationQueueController.GetAllowedClusterIdsAsync</c>/<c>GetClinicIdsByClusterIdsAsync</c>
    /// supaya dokter dan perawat melihat pasien yang sama dengan layar antreannya. Cabang
    /// SuperAdmin berbasis nama role pada kedua controller itu sengaja <b>tidak</b> dibawa:
    /// "lihat semua" ditentukan butir hak akses <c>OutpatientEncounter : ReadAll</c> yang diatur
    /// admin lewat layar Akses Role.
    /// </remarks>
    public class ClinicalActorScopeService
    {
        public const string ResourceName = "OutpatientEncounter";
        public const string ReadAllAction = "ReadAll";

        private readonly ApplicationDbContext _dbContext;
        private readonly AccessPermissionService _accessPermissionService;

        public ClinicalActorScopeService(
            ApplicationDbContext dbContext,
            AccessPermissionService accessPermissionService)
        {
            _dbContext = dbContext;
            _accessPermissionService = accessPermissionService;
        }

        public async Task<ClinicalActorScope> ResolveAsync(
            ClaimsPrincipal user,
            CancellationToken cancellationToken = default)
        {
            var canReadAll = await _accessPermissionService.HasAccessAsync(user, ResourceName, ReadAllAction);
            var doctorId = await ResolveDoctorIdAsync(user, cancellationToken);
            var clinicIds = await ResolveNurseClinicIdsAsync(user, cancellationToken);

            return new ClinicalActorScope(canReadAll, doctorId, clinicIds);
        }

        private async Task<Guid?> ResolveDoctorIdAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var doctorIdClaim = user.FindFirstValue("doctor_id") ?? user.FindFirstValue("DoctorId");
            if (Guid.TryParse(doctorIdClaim, out var doctorId) && doctorId != Guid.Empty)
            {
                return doctorId;
            }

            var workforceClaim = user.FindFirstValue("workforce_profile_id") ?? user.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId))
            {
                var doctorByWorkforce = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.WorkforceProfileId == workforceProfileId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (doctorByWorkforce != Guid.Empty)
                {
                    return doctorByWorkforce;
                }
            }

            var email = await GetCurrentUserEmailAsync(user, cancellationToken);
            if (email == null)
            {
                return null;
            }

            var doctorByEmail = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.Email != null && x.Email.ToLower() == email)
                .Select(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            return doctorByEmail != Guid.Empty ? doctorByEmail : null;
        }

        private async Task<List<Guid>> ResolveNurseClinicIdsAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var employeeId = await ResolveEmployeeIdAsync(user, cancellationToken);
            if (!employeeId.HasValue)
            {
                return new List<Guid>();
            }

            var staffAssignments = await _dbContext.Set<MstNurseStationClusterStaff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.EmployeeId == employeeId.Value)
                .Select(x => new { StaffId = x.Id, ClusterId = x.NurseStationClusterId })
                .ToListAsync(cancellationToken);

            if (staffAssignments.Count == 0)
            {
                return new List<Guid>();
            }

            var staffIds = staffAssignments.Select(x => x.StaffId).Distinct().ToList();

            // Mapping per perawat lebih dulu; cluster lama tanpa mapping per perawat memakai
            // seluruh poli cluster — sama dengan NurseStationQueueController.
            var specificMappings = await _dbContext.Set<MstNurseStationClusterStaffClinic>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && staffIds.Contains(x.NurseStationClusterStaffId))
                .Select(x => new { x.NurseStationClusterStaffId, x.ClinicId })
                .ToListAsync(cancellationToken);

            var clusterByStaffId = staffAssignments.ToDictionary(x => x.StaffId, x => x.ClusterId);

            var clustersWithSpecificMappings = specificMappings
                .Where(x => clusterByStaffId.ContainsKey(x.NurseStationClusterStaffId))
                .Select(x => clusterByStaffId[x.NurseStationClusterStaffId])
                .ToHashSet();

            var fallbackClusterIds = staffAssignments
                .Select(x => x.ClusterId)
                .Distinct()
                .Where(x => !clustersWithSpecificMappings.Contains(x))
                .ToList();

            var clinicIds = specificMappings.Select(x => x.ClinicId).ToHashSet();

            if (fallbackClusterIds.Count > 0)
            {
                var legacyClinicIds = await _dbContext.Set<MstNurseStationClusterClinic>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && fallbackClusterIds.Contains(x.NurseStationClusterId))
                    .Select(x => x.ClinicId)
                    .Distinct()
                    .ToListAsync(cancellationToken);

                clinicIds.UnionWith(legacyClinicIds);
            }

            return clinicIds.ToList();
        }

        private async Task<Guid?> ResolveEmployeeIdAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var employeeIdClaim = user.FindFirstValue("employee_id") ?? user.FindFirstValue("EmployeeId");
            if (Guid.TryParse(employeeIdClaim, out var employeeId))
            {
                var exists = await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == employeeId && !x.IsDelete && x.IsActive, cancellationToken);

                return exists ? employeeId : null;
            }

            var workforceClaim = user.FindFirstValue("workforce_profile_id") ?? user.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId))
            {
                var byWorkforce = await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                return byWorkforce;
            }

            var email = await GetCurrentUserEmailAsync(user, cancellationToken);
            if (email == null)
            {
                return null;
            }

            return await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.Email.ToLower() == email)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<string?> GetCurrentUserEmailAsync(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            var userIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("user_id");
            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return null;
            }

            var email = await _dbContext.Users
                .AsNoTracking()
                .Where(x => x.Id == userId)
                .Select(x => x.Email)
                .FirstOrDefaultAsync(cancellationToken);

            return string.IsNullOrWhiteSpace(email) ? null : email.ToLower();
        }
    }
}
