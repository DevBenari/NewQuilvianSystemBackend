using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Security.Claims;

namespace QuilvianSystemBackend.Hubs
{
    [Authorize]
    public class QueueHub : Hub
    {
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

        private async Task<bool> CanAccessNurseStationClusterAsync(Guid nurseStationClusterId)
        {
            if (IsCurrentUserSuperAdmin())
            {
                return await _dbContext.Set<MstNurseStationCluster>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == nurseStationClusterId && !x.IsDelete && x.IsActive);
            }

            var employee = await ResolveCurrentEmployeeAsync();
            if (employee == null)
            {
                return false;
            }

            return await _dbContext.Set<MstNurseStationClusterStaff>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.EmployeeId == employee.Id &&
                    x.NurseStationClusterId == nurseStationClusterId);
        }

        private async Task<bool> CanAccessDoctorQueueAsync(Guid doctorId)
        {
            if (IsCurrentUserSuperAdmin())
            {
                return await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == doctorId && !x.IsDelete && x.IsActive);
            }

            var allowedDoctorId = await ResolveCurrentDoctorIdAsync();
            return allowedDoctorId.HasValue && allowedDoctorId.Value == doctorId;
        }

        private async Task<bool> CanAccessDoctorClinicQueueAsync(Guid clinicId)
        {
            if (IsCurrentUserSuperAdmin())
            {
                return true;
            }

            var allowedDoctorId = await ResolveCurrentDoctorIdAsync();
            if (!allowedDoctorId.HasValue)
            {
                return false;
            }

            return await _dbContext.Set<MstDoctorSchedule>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.DoctorId == allowedDoctorId.Value &&
                    x.ClinicId == clinicId);
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
