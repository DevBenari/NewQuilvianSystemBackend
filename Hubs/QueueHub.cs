using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace QuilvianSystemBackend.Hubs
{
    [Authorize]
    public class QueueHub : Hub
    {
        private static readonly HashSet<string> NurseStationClusterClaimTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "nurse_station_cluster_id",
            "nurseStationClusterId",
            "NurseStationClusterId",
            "cluster_id",
            "clusterId",
            "ClusterId",
            "queue_display_cluster_id",
            "queueDisplayClusterId",
            "QueueDisplayClusterId",
            "queue_display_nurse_station_cluster_id",
            "queueDisplayNurseStationClusterId",
            "QueueDisplayNurseStationClusterId"
        };

        private readonly ApplicationDbContext _dbContext;

        public QueueHub(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task JoinNurseStationCluster(Guid nurseStationClusterId)
        {
            if (nurseStationClusterId == Guid.Empty)
            {
                throw new HubException("Nurse station cluster tidak valid.");
            }

            if (!await CanAccessNurseStationClusterAsync(nurseStationClusterId))
            {
                throw new HubException("Anda tidak memiliki akses ke nurse station cluster ini.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                QueueRealtimeService.BuildNurseStationClusterGroupName(nurseStationClusterId)
            );
        }

        public async Task LeaveNurseStationCluster(Guid nurseStationClusterId)
        {
            if (nurseStationClusterId == Guid.Empty)
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                QueueRealtimeService.BuildNurseStationClusterGroupName(nurseStationClusterId)
            );
        }

        public async Task<List<Guid>> JoinAccessibleNurseStationClusters()
        {
            var clusterIds = await ResolveAccessibleNurseStationClusterIdsAsync();

            if (IsCurrentUserSuperAdmin())
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    QueueRealtimeService.BuildNurseStationAllGroupName()
                );
            }

            foreach (var clusterId in clusterIds)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    QueueRealtimeService.BuildNurseStationClusterGroupName(clusterId)
                );
            }

            return clusterIds;
        }

        public async Task JoinDoctorQueue(Guid doctorId)
        {
            if (doctorId == Guid.Empty)
            {
                throw new HubException("Doctor tidak valid.");
            }

            if (!await CanAccessDoctorQueueAsync(doctorId))
            {
                throw new HubException("Anda tidak memiliki akses ke antrean dokter ini.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                QueueRealtimeService.BuildDoctorQueueDoctorGroupName(doctorId)
            );
        }

        public async Task LeaveDoctorQueue(Guid doctorId)
        {
            if (doctorId == Guid.Empty)
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                QueueRealtimeService.BuildDoctorQueueDoctorGroupName(doctorId)
            );
        }

        public async Task<List<Guid>> JoinAccessibleDoctorQueues()
        {
            var doctorIds = await ResolveAccessibleDoctorIdsAsync();

            if (IsCurrentUserSuperAdmin())
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    QueueRealtimeService.BuildDoctorQueueAllGroupName()
                );
            }

            foreach (var doctorId in doctorIds)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    QueueRealtimeService.BuildDoctorQueueDoctorGroupName(doctorId)
                );
            }

            return doctorIds;
        }

        public async Task JoinDoctorClinicQueue(Guid clinicId)
        {
            if (clinicId == Guid.Empty)
            {
                throw new HubException("Clinic tidak valid.");
            }

            if (!await CanAccessDoctorClinicQueueAsync(clinicId))
            {
                throw new HubException("Anda tidak memiliki akses ke antrean poli ini.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                QueueRealtimeService.BuildDoctorQueueClinicGroupName(clinicId)
            );
        }

        public async Task LeaveDoctorClinicQueue(Guid clinicId)
        {
            if (clinicId == Guid.Empty)
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                QueueRealtimeService.BuildDoctorQueueClinicGroupName(clinicId)
            );
        }

        public async Task<List<Guid>> JoinAccessibleDoctorClinicQueues()
        {
            var clinicIds = await ResolveAccessibleDoctorClinicIdsAsync();

            if (IsCurrentUserSuperAdmin())
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    QueueRealtimeService.BuildDoctorQueueAllGroupName()
                );
            }

            foreach (var clinicId in clinicIds)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    QueueRealtimeService.BuildDoctorQueueClinicGroupName(clinicId)
                );
            }

            return clinicIds;
        }

        private async Task<List<Guid>> ResolveAccessibleNurseStationClusterIdsAsync()
        {
            if (IsCurrentUserSuperAdmin())
            {
                return await _dbContext.Set<MstNurseStationCluster>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .Distinct()
                    .ToListAsync();
            }

            var employee = await ResolveCurrentEmployeeAsync();
            if (employee == null)
            {
                return new List<Guid>();
            }

            return await (
                from staff in _dbContext.Set<MstNurseStationClusterStaff>().AsNoTracking()
                join cluster in _dbContext.Set<MstNurseStationCluster>().AsNoTracking()
                    on staff.NurseStationClusterId equals cluster.Id
                where !staff.IsDelete &&
                      staff.IsActive &&
                      staff.EmployeeId == employee.Id &&
                      !cluster.IsDelete &&
                      cluster.IsActive
                orderby cluster.Id
                select staff.NurseStationClusterId
            )
            .Distinct()
            .ToListAsync();
        }

        private async Task<List<Guid>> ResolveAccessibleDoctorIdsAsync()
        {
            if (IsCurrentUserSuperAdmin())
            {
                return await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .Distinct()
                    .ToListAsync();
            }

            var allowedDoctorId = await ResolveCurrentDoctorIdAsync();
            if (!allowedDoctorId.HasValue || allowedDoctorId.Value == Guid.Empty)
            {
                return new List<Guid>();
            }

            var exists = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == allowedDoctorId.Value && !x.IsDelete && x.IsActive);

            return exists ? new List<Guid> { allowedDoctorId.Value } : new List<Guid>();
        }

        private async Task<List<Guid>> ResolveAccessibleDoctorClinicIdsAsync()
        {
            if (IsCurrentUserSuperAdmin())
            {
                return await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive)
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .Distinct()
                    .ToListAsync();
            }

            var allowedDoctorId = await ResolveCurrentDoctorIdAsync();
            if (!allowedDoctorId.HasValue || allowedDoctorId.Value == Guid.Empty)
            {
                return new List<Guid>();
            }

            return await (
                from schedule in _dbContext.Set<MstDoctorSchedule>().AsNoTracking()
                join clinic in _dbContext.Set<MstClinic>().AsNoTracking()
                    on schedule.ClinicId equals clinic.Id
                where !schedule.IsDelete &&
                      schedule.IsActive &&
                      schedule.DoctorId == allowedDoctorId.Value &&
                      !clinic.IsDelete &&
                      clinic.IsActive
                orderby clinic.Id
                select schedule.ClinicId
            )
            .Distinct()
            .ToListAsync();
        }

        private async Task<bool> CanAccessNurseStationClusterAsync(Guid nurseStationClusterId)
        {
            if (nurseStationClusterId == Guid.Empty)
            {
                return false;
            }

            if (HasNurseStationClusterClaim(nurseStationClusterId))
            {
                return true;
            }

            var accessibleIds = await ResolveAccessibleNurseStationClusterIdsAsync();
            return accessibleIds.Contains(nurseStationClusterId);
        }

        private async Task<bool> CanAccessDoctorQueueAsync(Guid doctorId)
        {
            if (doctorId == Guid.Empty)
            {
                return false;
            }

            var accessibleIds = await ResolveAccessibleDoctorIdsAsync();
            return accessibleIds.Contains(doctorId);
        }

        private async Task<bool> CanAccessDoctorClinicQueueAsync(Guid clinicId)
        {
            if (clinicId == Guid.Empty)
            {
                return false;
            }

            var accessibleIds = await ResolveAccessibleDoctorClinicIdsAsync();
            return accessibleIds.Contains(clinicId);
        }

        private bool HasNurseStationClusterClaim(Guid nurseStationClusterId)
        {
            if (nurseStationClusterId == Guid.Empty || Context.User == null)
            {
                return false;
            }

            foreach (var claim in Context.User.Claims)
            {
                if (!NurseStationClusterClaimTypes.Contains(claim.Type))
                {
                    continue;
                }

                foreach (var token in SplitClaimGuidTokens(claim.Value))
                {
                    if (Guid.TryParse(token, out var claimClusterId) && claimClusterId == nurseStationClusterId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static IEnumerable<string> SplitClaimGuidTokens(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Enumerable.Empty<string>();
            }

            return value
                .Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x));
        }

        private bool IsCurrentUserSuperAdmin()
        {
            if (Context.User?.IsInRole("SuperAdmin") == true)
            {
                return true;
            }

            var roleClaims = (Context.User?.FindAll(ClaimTypes.Role) ?? Enumerable.Empty<Claim>())
                .Concat(Context.User?.FindAll("role") ?? Enumerable.Empty<Claim>())
                .Concat(Context.User?.FindAll("roles") ?? Enumerable.Empty<Claim>())
                .Select(x => x.Value);

            if (roleClaims.Any(x => x.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase)))
            {
                return true;
            }

            var userTypeClaim =
                Context.User?.FindFirstValue("user_type") ??
                Context.User?.FindFirstValue("UserType") ??
                Context.User?.FindFirstValue("userType");

            return IsSuperAdminValue(userTypeClaim);
        }

        private static bool IsSuperAdminValue(object? value)
        {
            if (value == null)
            {
                return false;
            }

            if (value is int intValue)
            {
                return intValue == 1;
            }

            if (value is long longValue)
            {
                return longValue == 1;
            }

            var valueType = value.GetType();
            if (valueType.IsEnum && Enum.TryParse(valueType, "SuperAdmin", true, out var superAdminValue))
            {
                return Equals(value, superAdminValue);
            }

            var text = value.ToString();
            return text == "1" || text?.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) == true;
        }

        private async Task<MstEmployee?> ResolveCurrentEmployeeAsync()
        {
            var employeeIdClaim = Context.User?.FindFirstValue("employee_id") ?? Context.User?.FindFirstValue("EmployeeId");
            if (Guid.TryParse(employeeIdClaim, out var employeeId))
            {
                return await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == employeeId && !x.IsDelete && x.IsActive);
            }

            var workforceClaim = Context.User?.FindFirstValue("workforce_profile_id") ?? Context.User?.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId))
            {
                return await _dbContext.Set<MstEmployee>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete && x.IsActive);
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return null;
            }

            var currentUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId);

            if (currentUser?.Email == null)
            {
                return null;
            }

            var email = currentUser.Email.ToLower();
            return await _dbContext.Set<MstEmployee>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => !x.IsDelete && x.IsActive && x.Email.ToLower() == email);
        }

        private async Task<Guid?> ResolveCurrentDoctorIdAsync()
        {
            var doctorIdClaim = Context.User?.FindFirstValue("doctor_id") ?? Context.User?.FindFirstValue("DoctorId");
            if (Guid.TryParse(doctorIdClaim, out var doctorId))
            {
                return doctorId;
            }

            var workforceClaim = Context.User?.FindFirstValue("workforce_profile_id") ?? Context.User?.FindFirstValue("WorkforceProfileId");
            if (Guid.TryParse(workforceClaim, out var workforceProfileId))
            {
                var doctorByWorkforce = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x => !x.IsDelete && x.IsActive && x.WorkforceProfileId == workforceProfileId)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (doctorByWorkforce != Guid.Empty)
                {
                    return doctorByWorkforce;
                }
            }

            var currentUserId = GetCurrentUserId();
            if (currentUserId == Guid.Empty)
            {
                return null;
            }

            var currentUser = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == currentUserId);

            if (currentUser?.Email == null)
            {
                return null;
            }

            var email = currentUser.Email.ToLower();
            var doctorByEmail = await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.IsActive && x.Email.ToLower() == email)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            return doctorByEmail == Guid.Empty ? null : doctorByEmail;
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                Context.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                Context.User?.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId) ? userId : Guid.Empty;
        }
    }
}
