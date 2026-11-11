using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Service penyedia dan pengelola parameter kebijakan operasional lembur rumah sakit (BE-HRD-132, HRD-DEC-117, HRD-DEC-118, HRD-DEC-121).
    /// </summary>
    public class OvertimeSettingService : IOvertimeSettingService
    {
        private static readonly object LockObj = new();
        private static OvertimeSettingDto _cachedSettings = new()
        {
            DailyMaxHoursRegular = 4m,
            WeeklyMaxHoursRegular = 18m,
            RoundingIntervalMinutes = 30,
            MinimumThresholdMinutes = 30,
            CtoValidityDays = 90,
            MonthlyCutoffDay = 20,
            BiometricDiscrepancyTolerance = 30,
            PostClaimCitoGraceDays = 1,
            IsActive = true,
            LastUpdatedAt = DateTime.UtcNow
        };

        private readonly ILogger<OvertimeSettingService> _logger;

        public OvertimeSettingService(ILogger<OvertimeSettingService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<OvertimeSettingDto> GetSettingsAsync(CancellationToken cancellationToken = default)
        {
            lock (LockObj)
            {
                return Task.FromResult(new OvertimeSettingDto
                {
                    DailyMaxHoursRegular = _cachedSettings.DailyMaxHoursRegular,
                    WeeklyMaxHoursRegular = _cachedSettings.WeeklyMaxHoursRegular,
                    RoundingIntervalMinutes = _cachedSettings.RoundingIntervalMinutes,
                    MinimumThresholdMinutes = _cachedSettings.MinimumThresholdMinutes,
                    CtoValidityDays = _cachedSettings.CtoValidityDays,
                    MonthlyCutoffDay = _cachedSettings.MonthlyCutoffDay,
                    BiometricDiscrepancyTolerance = _cachedSettings.BiometricDiscrepancyTolerance,
                    PostClaimCitoGraceDays = _cachedSettings.PostClaimCitoGraceDays,
                    IsActive = _cachedSettings.IsActive,
                    LastUpdatedAt = _cachedSettings.LastUpdatedAt,
                    LastUpdatedByUserId = _cachedSettings.LastUpdatedByUserId
                });
            }
        }

        public Task<OvertimeSettingDto> UpdateSettingsAsync(
            UpdateOvertimeSettingCommand command,
            Guid? actorUserId = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(command);

            lock (LockObj)
            {
                _cachedSettings.DailyMaxHoursRegular = command.DailyMaxHoursRegular > 0 ? command.DailyMaxHoursRegular : 4m;
                _cachedSettings.WeeklyMaxHoursRegular = command.WeeklyMaxHoursRegular > 0 ? command.WeeklyMaxHoursRegular : 18m;
                _cachedSettings.RoundingIntervalMinutes = command.RoundingIntervalMinutes > 0 ? command.RoundingIntervalMinutes : 30;
                _cachedSettings.MinimumThresholdMinutes = command.MinimumThresholdMinutes > 0 ? command.MinimumThresholdMinutes : 30;
                _cachedSettings.CtoValidityDays = command.CtoValidityDays > 0 ? command.CtoValidityDays : 90;
                _cachedSettings.MonthlyCutoffDay = (command.MonthlyCutoffDay >= 1 && command.MonthlyCutoffDay <= 28) ? command.MonthlyCutoffDay : 20;
                _cachedSettings.BiometricDiscrepancyTolerance = command.BiometricDiscrepancyTolerance > 0 ? command.BiometricDiscrepancyTolerance : 30;
                _cachedSettings.PostClaimCitoGraceDays = command.PostClaimCitoGraceDays > 0 ? command.PostClaimCitoGraceDays : 1;
                _cachedSettings.LastUpdatedAt = DateTime.UtcNow;
                _cachedSettings.LastUpdatedByUserId = actorUserId;

                _logger.LogInformation(
                    "Overtime operational settings updated by {UserId}: DailyMax={DailyMax}, WeeklyMax={WeeklyMax}, Cutoff={CutoffDay}",
                    actorUserId, _cachedSettings.DailyMaxHoursRegular, _cachedSettings.WeeklyMaxHoursRegular, _cachedSettings.MonthlyCutoffDay);

                return Task.FromResult(new OvertimeSettingDto
                {
                    DailyMaxHoursRegular = _cachedSettings.DailyMaxHoursRegular,
                    WeeklyMaxHoursRegular = _cachedSettings.WeeklyMaxHoursRegular,
                    RoundingIntervalMinutes = _cachedSettings.RoundingIntervalMinutes,
                    MinimumThresholdMinutes = _cachedSettings.MinimumThresholdMinutes,
                    CtoValidityDays = _cachedSettings.CtoValidityDays,
                    MonthlyCutoffDay = _cachedSettings.MonthlyCutoffDay,
                    BiometricDiscrepancyTolerance = _cachedSettings.BiometricDiscrepancyTolerance,
                    PostClaimCitoGraceDays = _cachedSettings.PostClaimCitoGraceDays,
                    IsActive = _cachedSettings.IsActive,
                    LastUpdatedAt = _cachedSettings.LastUpdatedAt,
                    LastUpdatedByUserId = _cachedSettings.LastUpdatedByUserId
                });
            }
        }
    }
}
