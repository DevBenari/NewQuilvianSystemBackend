using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using System.Security.Claims;

namespace QuilvianSystemBackend.Shared.HumanResource.Services
{
    public class HumanResourceContextService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;

        public HumanResourceContextService(
            ApplicationDbContext dbContext,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<HumanResourceUserContextDto> GetCurrentAsync(
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();

            return await GetByUserIdAsync(userId, cancellationToken);
        }

        public async Task<HumanResourceUserContextDto> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            if (userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "Identitas user login tidak valid.");
            }

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken)
                ?? throw new UnauthorizedAccessException(
                    "User login tidak ditemukan.");

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException(
                    "Akun user sudah tidak aktif.");
            }

            if (user.AccessValidUntil.HasValue &&
                user.AccessValidUntil.Value < DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException(
                    "Masa akses akun user sudah berakhir.");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var result = new HumanResourceUserContextDto
            {
                UserId = user.Id,
                UserCode = user.UserCode,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                DisplayName = user.DisplayName,
                UserType = user.UserType.ToString(),
                IsUserActive = user.IsActive,
                AccessValidUntil = user.AccessValidUntil,
                WorkforceProfileId = user.WorkforceProfileId,
                EmployeeId = user.EmployeeId,
                DoctorId = user.DoctorId,
                ExternalUserId = user.ExternalUserId,
                ProfileType = ResolveProfileType(user),
                HasWorkforceProfile = user.WorkforceProfileId.HasValue,
                Roles = roles
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x)
                    .ToList()
            };

            if (!user.WorkforceProfileId.HasValue)
            {
                result.Warnings.Add(
                    "Akun login belum terhubung dengan profil tenaga kerja.");
                result.IsContextComplete = false;

                return result;
            }

            var workforceProfileId = user.WorkforceProfileId.Value;

            var workforceProfile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x =>
                        x.Id == workforceProfileId &&
                        x.IsActive &&
                        !x.IsDelete,
                    cancellationToken);

            if (workforceProfile == null)
            {
                result.Warnings.Add(
                    "Profil tenaga kerja tidak ditemukan atau sudah tidak aktif.");
                result.IsContextComplete = false;

                return result;
            }

            result.WorkforceProfileCode = workforceProfile.ProfileCode;
            result.WorkforceDisplayName = workforceProfile.DisplayName;

            await ValidateWorkforceSubtypeAsync(
                user,
                workforceProfileId,
                result,
                cancellationToken);

            await ResolveOrganizationAssignmentAsync(
                workforceProfileId,
                result,
                cancellationToken);

            await ResolveManagerAsync(
                workforceProfileId,
                result,
                cancellationToken);

            await ResolveDirectReportsAsync(
                workforceProfileId,
                result,
                cancellationToken);

            result.IsContextComplete =
                result.HasWorkforceProfile &&
                result.HasValidWorkforceSubtype &&
                result.HasOrganizationAssignment &&
                result.Warnings.All(x =>
                    !x.Contains("tidak sesuai", StringComparison.OrdinalIgnoreCase) &&
                    !x.Contains("lebih dari satu", StringComparison.OrdinalIgnoreCase));

            return result;
        }

        private Guid GetCurrentUserId()
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            var userIdText =
                principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                principal?.FindFirstValue("user_id");

            if (!Guid.TryParse(userIdText, out var userId) ||
                userId == Guid.Empty)
            {
                throw new UnauthorizedAccessException(
                    "Identitas user pada token tidak valid.");
            }

            return userId;
        }

        private async Task ValidateWorkforceSubtypeAsync(
            ApplicationUser user,
            Guid workforceProfileId,
            HumanResourceUserContextDto result,
            CancellationToken cancellationToken)
        {
            var subtypeCount =
                (user.EmployeeId.HasValue ? 1 : 0) +
                (user.DoctorId.HasValue ? 1 : 0) +
                (user.ExternalUserId.HasValue ? 1 : 0);

            var isSubtypeValid = subtypeCount == 1;

            if (user.EmployeeId.HasValue)
            {
                var employee = await _dbContext.MstEmployees
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == user.EmployeeId.Value &&
                        x.IsActive &&
                        !x.IsDelete)
                    .Select(x => new
                    {
                        x.Id,
                        x.WorkforceProfileId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (employee == null)
                {
                    isSubtypeValid = false;
                    result.Warnings.Add(
                        "Employee yang terhubung dengan akun tidak ditemukan atau tidak aktif.");
                }
                else if (employee.WorkforceProfileId != workforceProfileId)
                {
                    isSubtypeValid = false;
                    result.Warnings.Add(
                        "WorkforceProfileId pada akun tidak sesuai dengan Employee.");
                }
            }

            if (user.DoctorId.HasValue)
            {
                var doctor = await _dbContext.MstDoctors
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == user.DoctorId.Value &&
                        x.IsActive &&
                        !x.IsDelete)
                    .Select(x => new
                    {
                        x.Id,
                        x.WorkforceProfileId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (doctor == null)
                {
                    isSubtypeValid = false;
                    result.Warnings.Add(
                        "Doctor yang terhubung dengan akun tidak ditemukan atau tidak aktif.");
                }
                else if (doctor.WorkforceProfileId != workforceProfileId)
                {
                    isSubtypeValid = false;
                    result.Warnings.Add(
                        "WorkforceProfileId pada akun tidak sesuai dengan Doctor.");
                }
            }

            if (user.ExternalUserId.HasValue)
            {
                var externalUser = await _dbContext.MstExternalUsers
                    .AsNoTracking()
                    .Where(x =>
                        x.Id == user.ExternalUserId.Value &&
                        x.IsActive &&
                        !x.IsDelete)
                    .Select(x => new
                    {
                        x.Id,
                        x.WorkforceProfileId
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (externalUser == null)
                {
                    isSubtypeValid = false;
                    result.Warnings.Add(
                        "External user yang terhubung dengan akun tidak ditemukan atau tidak aktif.");
                }
                else if (externalUser.WorkforceProfileId != workforceProfileId)
                {
                    isSubtypeValid = false;
                    result.Warnings.Add(
                        "WorkforceProfileId pada akun tidak sesuai dengan External User.");
                }
            }

            if (subtypeCount == 0)
            {
                result.Warnings.Add(
                    "Profil tenaga kerja belum terhubung dengan Employee, Doctor, atau External User.");
            }
            else if (subtypeCount > 1)
            {
                result.Warnings.Add(
                    "Akun terhubung ke lebih dari satu jenis profil tenaga kerja.");
            }

            result.HasValidWorkforceSubtype = isSubtypeValid;
        }

        private async Task ResolveOrganizationAssignmentAsync(
            Guid workforceProfileId,
            HumanResourceUserContextDto result,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var assignments = await _dbContext.WfpOrganizationAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue ||
                     x.EffectiveEndDate.Value >= now))
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Take(3)
                .ToListAsync(cancellationToken);

            if (assignments.Count == 0)
            {
                result.Warnings.Add(
                    "Penempatan organisasi aktif untuk profil tenaga kerja belum tersedia.");
                return;
            }

            var primaryAssignments = assignments
                .Where(x => x.IsPrimary)
                .ToList();

            if (primaryAssignments.Count > 1)
            {
                result.Warnings.Add(
                    "Ditemukan lebih dari satu penempatan organisasi primary yang aktif.");
            }

            var assignment = primaryAssignments.FirstOrDefault() ?? assignments[0];

            if (!assignment.IsPrimary)
            {
                result.Warnings.Add(
                    "Penempatan organisasi aktif ditemukan, tetapi belum ditandai sebagai primary.");
            }

            result.OrganizationAssignmentId = assignment.Id;
            result.LegalEntityId = assignment.LegalEntityId;
            result.HospitalSiteId = assignment.HospitalSiteId;
            result.OrganizationUnitId = assignment.OrganizationUnitId;
            result.DepartmentId = assignment.DepartmentId;
            result.PositionId = assignment.PositionId;
            result.CostCenterId = assignment.CostCenterId;
            result.WorkLocationId = assignment.WorkLocationId;
            result.EmployeeGradeId = assignment.EmployeeGradeId;
            result.AssignmentType = assignment.AssignmentType;
            result.HasOrganizationAssignment = true;
        }

        private async Task ResolveManagerAsync(
            Guid workforceProfileId,
            HumanResourceUserContextDto result,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var managerAssignments = await _dbContext.WfpManagerAssignments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete &&
                    !x.IsCancel &&
                    x.EffectiveStartDate <= now &&
                    (!x.EffectiveEndDate.HasValue ||
                     x.EffectiveEndDate.Value >= now))
                .OrderByDescending(x => x.IsPrimaryManager)
                .ThenByDescending(x => x.EffectiveStartDate)
                .Take(3)
                .ToListAsync(cancellationToken);

            if (managerAssignments.Count == 0)
            {
                result.Warnings.Add(
                    "Manager aktif untuk profil tenaga kerja belum ditentukan.");
                return;
            }

            var primaryManagerAssignments = managerAssignments
                .Where(x => x.IsPrimaryManager)
                .ToList();

            if (primaryManagerAssignments.Count > 1)
            {
                result.Warnings.Add(
                    "Ditemukan lebih dari satu primary manager yang aktif.");
            }

            var managerAssignment =
                primaryManagerAssignments.FirstOrDefault() ??
                managerAssignments[0];

            var managerProfile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x =>
                    x.Id == managerAssignment.ManagerWorkforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.ProfileCode,
                    x.DisplayName
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (managerProfile == null)
            {
                result.Warnings.Add(
                    "Profil manager tidak ditemukan atau sudah tidak aktif.");
                return;
            }

            result.ManagerAssignmentId = managerAssignment.Id;
            result.ManagerWorkforceProfileId = managerProfile.Id;
            result.ManagerProfileCode = managerProfile.ProfileCode;
            result.ManagerDisplayName = managerProfile.DisplayName;
            result.ManagerType = managerAssignment.ManagerType;
            result.ManagerCanApproveRequests = managerAssignment.CanApproveRequests;
            result.HasManager = true;
        }

        private async Task ResolveDirectReportsAsync(
            Guid managerWorkforceProfileId,
            HumanResourceUserContextDto result,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var directReportRows = await (
                from managerAssignment in _dbContext.WfpManagerAssignments.AsNoTracking()
                join workforceProfile in _dbContext.MstWorkforceProfiles.AsNoTracking()
                    on managerAssignment.WorkforceProfileId equals workforceProfile.Id
                where
                    managerAssignment.ManagerWorkforceProfileId == managerWorkforceProfileId &&
                    managerAssignment.IsActive &&
                    !managerAssignment.IsDelete &&
                    !managerAssignment.IsCancel &&
                    managerAssignment.EffectiveStartDate <= now &&
                    (!managerAssignment.EffectiveEndDate.HasValue ||
                     managerAssignment.EffectiveEndDate.Value >= now) &&
                    workforceProfile.IsActive &&
                    !workforceProfile.IsDelete
                orderby
                    managerAssignment.IsPrimaryManager descending,
                    managerAssignment.EffectiveStartDate descending
                select new
                {
                    ManagerAssignmentId = managerAssignment.Id,
                    managerAssignment.WorkforceProfileId,
                    WorkforceProfileCode = workforceProfile.ProfileCode,
                    workforceProfile.DisplayName,
                    managerAssignment.ManagerType,
                    managerAssignment.IsPrimaryManager,
                    managerAssignment.CanApproveRequests,
                    managerAssignment.EffectiveStartDate
                })
                .ToListAsync(cancellationToken);

            var directReports = directReportRows
                .GroupBy(x => x.WorkforceProfileId)
                .Select(group => group
                    .OrderByDescending(x => x.IsPrimaryManager)
                    .ThenByDescending(x => x.EffectiveStartDate)
                    .First())
                .OrderBy(x => x.DisplayName)
                .Select(x => new HumanResourceDirectReportDto
                {
                    ManagerAssignmentId = x.ManagerAssignmentId,
                    WorkforceProfileId = x.WorkforceProfileId,
                    WorkforceProfileCode = x.WorkforceProfileCode,
                    DisplayName = x.DisplayName,
                    ManagerType = x.ManagerType,
                    IsPrimaryManager = x.IsPrimaryManager,
                    CanApproveRequests = x.CanApproveRequests
                })
                .ToList();

            result.DirectReports = directReports;
            result.DirectReportCount = directReports.Count;
            result.IsManager = directReports.Count > 0;
            result.CanApproveRequests = directReports.Any(x =>
                x.CanApproveRequests);
        }

        private static string ResolveProfileType(ApplicationUser user)
        {
            if (user.EmployeeId.HasValue)
            {
                return "Employee";
            }

            if (user.DoctorId.HasValue)
            {
                return "Doctor";
            }

            if (user.ExternalUserId.HasValue)
            {
                return "ExternalUser";
            }

            return user.WorkforceProfileId.HasValue
                ? "WorkforceProfileOnly"
                : "AccountOnly";
        }
    }
}
