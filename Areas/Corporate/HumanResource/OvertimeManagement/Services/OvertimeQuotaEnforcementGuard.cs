using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Engine pelindung kepatuhan regulasi kuota lembur PP 35/2021 dan deteksi eskalasi darurat medis (BE-HRD-136, HRD-DEC-117, OT-B4, OT-W1).
    /// </summary>
    public class OvertimeQuotaEnforcementGuard : IOvertimeQuotaEnforcementGuard
    {
        private readonly ApplicationDbContext _context;
        private readonly IOvertimeSettingService _settingService;
        private readonly ILogger<OvertimeQuotaEnforcementGuard> _logger;

        public OvertimeQuotaEnforcementGuard(
            ApplicationDbContext context,
            IOvertimeSettingService settingService,
            ILogger<OvertimeQuotaEnforcementGuard> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _settingService = settingService ?? throw new ArgumentNullException(nameof(settingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OvertimeQuotaEvaluationResult> EvaluateQuotaAsync(
            Guid workforceProfileId,
            DateOnly overtimeDate,
            decimal requestedHours,
            bool isEmergencyCito = false,
            CancellationToken cancellationToken = default)
        {
            var settings = await _settingService.GetSettingsAsync(cancellationToken);
            var dailyMax = settings.DailyMaxHoursRegular;
            var weeklyMax = settings.WeeklyMaxHoursRegular;

            // Hitung rentang Senin s.d. Minggu periode berjalan (Kalender Kerja ID)
            var dayOfWeek = (int)overtimeDate.DayOfWeek;
            var daysSinceMonday = (dayOfWeek == 0) ? 6 : dayOfWeek - 1;
            var weekStart = overtimeDate.AddDays(-daysSinceMonday);
            var weekEnd = weekStart.AddDays(6);

            // Hitung akumulasi jam lembur harian staf yang sudah disetujui / aktif
            var dailyMinutes = await _context.Set<TrxOvertimePlanDetail>()
                .Where(x => x.WorkforceProfileId == workforceProfileId
                    && x.OvertimeDate == overtimeDate
                    && !x.IsDelete
                    && x.DetailStatus != "Cancelled"
                    && x.DetailStatus != "Rejected")
                .SumAsync(x => x.PlannedMinutes, cancellationToken);

            var accumDaily = (decimal)dailyMinutes / 60m;

            // Hitung akumulasi jam lembur mingguan staf (Senin - Minggu)
            var weeklyMinutes = await _context.Set<TrxOvertimePlanDetail>()
                .Where(x => x.WorkforceProfileId == workforceProfileId
                    && x.OvertimeDate >= weekStart
                    && x.OvertimeDate <= weekEnd
                    && !x.IsDelete
                    && x.DetailStatus != "Cancelled"
                    && x.DetailStatus != "Rejected")
                .SumAsync(x => x.PlannedMinutes, cancellationToken);

            var accumWeekly = (decimal)weeklyMinutes / 60m;

            var newTotalDaily = accumDaily + requestedHours;
            var newTotalWeekly = accumWeekly + requestedHours;

            var exceedsQuota = newTotalDaily > dailyMax || newTotalWeekly > weeklyMax;

            var result = new OvertimeQuotaEvaluationResult
            {
                AccumulatedDailyHours = newTotalDaily,
                AccumulatedWeeklyHours = newTotalWeekly
            };

            if (exceedsQuota)
            {
                if (isEmergencyCito)
                {
                    // OT-W1: Darurat medis penyelamatan nyawa diizinkan lewat tetapi dialihkan ke otorisasi Direktur Medis
                    result.IsAllowed = true;
                    result.IsMedicalDirectorEscalation = true;
                    _logger.LogWarning(
                        "Staff {StaffId} exceeds overtime quota on {Date} (Daily: {Daily}h, Weekly: {Weekly}h). Escalated to Medical Director for EmergencyCito.",
                        workforceProfileId, overtimeDate, newTotalDaily, newTotalWeekly);
                }
                else
                {
                    // OT-B4, VR-OT-04: Lembur biasa hard-block
                    result.IsAllowed = false;
                    result.IsMedicalDirectorEscalation = false;
                    result.DenialReason = "Pengajuan melebihi kuota maksimal lembur (4 jam/hari atau 18 jam/minggu) sesuai regulasi PP 35/2021.";
                    _logger.LogWarning(
                        "Staff {StaffId} blocked: Daily {Daily}h (max {MaxD}h) or Weekly {Weekly}h (max {MaxW}h) exceeds PP 35/2021.",
                        workforceProfileId, newTotalDaily, dailyMax, newTotalWeekly, weeklyMax);
                }
            }
            else
            {
                result.IsAllowed = true;
                result.IsMedicalDirectorEscalation = false;
            }

            return result;
        }
    }
}
