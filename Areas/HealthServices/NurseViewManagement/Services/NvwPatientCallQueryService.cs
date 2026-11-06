using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.NurseViewManagement.Services
{
    public class NvwPatientCallQueryService
    {
        private readonly ApplicationDbContext _dbContext;

        public NvwPatientCallQueryService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public static string PriorityLabel(NvwPatientCallPriority priority) => priority switch
        {
            NvwPatientCallPriority.Normal => "Panggilan",
            NvwPatientCallPriority.Emergency => "Darurat",
            _ => priority.ToString()
        };

        public static string StatusLabel(NvwPatientCallStatus status) => status switch
        {
            NvwPatientCallStatus.Waiting => "Menunggu",
            NvwPatientCallStatus.Claimed => "Diklaim",
            NvwPatientCallStatus.Handled => "Ditangani",
            NvwPatientCallStatus.Cancelled => "Dibatalkan",
            NvwPatientCallStatus.ClosedBySystem => "Ditutup sistem",
            _ => status.ToString()
        };

        public static bool IsActiveStatus(NvwPatientCallStatus status) =>
            status is NvwPatientCallStatus.Waiting or NvwPatientCallStatus.Claimed;

        public static int WaitingSeconds(DateTime firstPressedAtUtc, DateTime nowUtc) =>
            (int)Math.Max(0, Math.Floor((nowUtc - firstPressedAtUtc).TotalSeconds));

        public static NvwTabletCallResponse MapTabletCall(NvwPatientCall call, string? claimedByName, DateTime nowUtc)
        {
            var isActive = IsActiveStatus(call.CallStatus);

            return new NvwTabletCallResponse
            {
                CallId = call.Id,
                Priority = call.Priority,
                PriorityLabel = PriorityLabel(call.Priority),
                CallStatus = call.CallStatus,
                StatusLabel = StatusLabel(call.CallStatus),
                FirstPressedAt = call.FirstPressedAt,
                WaitingSeconds = WaitingSeconds(call.FirstPressedAt, nowUtc),
                PressCount = call.PressCount,
                ClaimedByName = claimedByName,
                CanCancel = isActive && call.CallStatus == NvwPatientCallStatus.Waiting && call.Priority == NvwPatientCallPriority.Normal,
                CanEscalate = isActive && call.Priority == NvwPatientCallPriority.Normal,
                CanConfirmNurseArrived = isActive,
                IsActive = isActive,
                Version = call.Version
            };
        }

        public async Task<NvwTabletCallResponse?> GetForTabletAsync(NvwBedTablet tablet, CancellationToken cancellationToken)
        {
            var call = await _dbContext.NvwPatientCalls.AsNoTracking()
                .Where(x =>
                    x.BedId == tablet.BedId &&
                    !x.IsDelete &&
                    (x.CallStatus == NvwPatientCallStatus.Waiting || x.CallStatus == NvwPatientCallStatus.Claimed))
                .OrderByDescending(x => x.FirstPressedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (call == null)
                return null;

            string? claimedByName = null;
            if (call.ClaimedByUserId.HasValue)
            {
                var claimer = await _dbContext.Users.AsNoTracking()
                    .Where(x => x.Id == call.ClaimedByUserId.Value)
                    .Select(x => new { x.UserName, x.DisplayName })
                    .FirstOrDefaultAsync(cancellationToken);
                claimedByName = string.IsNullOrWhiteSpace(claimer?.DisplayName) ? claimer?.UserName : claimer.DisplayName;
            }

            return MapTabletCall(call, claimedByName, DateTime.UtcNow);
        }
    }
}
