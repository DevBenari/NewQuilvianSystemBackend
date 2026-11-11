using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Repositories;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollVarianceGuardService : IPayrollVarianceGuardService
    {
        private readonly ApplicationDbContext _context;
        private readonly IPayrollSettingService _settingService;
        private readonly ILogger<PayrollVarianceGuardService> _logger;

        public PayrollVarianceGuardService(
            ApplicationDbContext context,
            IPayrollSettingService settingService,
            ILogger<PayrollVarianceGuardService> logger)
        {
            _context = context;
            _settingService = settingService;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<EvaluateVarianceResultDto>> EvaluateRunVariancesAsync(
            Guid runId,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memulai audit Payroll Variance Guard untuk batch penggajian ID: {RunId}. Actor: {ActorId}",
                runId, actorUserId);

            var run = await _context.TrxPayrollRuns
                .Include(r => r.PayrollPeriod)
                .Include(r => r.Employees.Where(e => e.IsActive))
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<EvaluateVarianceResultDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            var setting = await ResolveEffectiveSettingAsync(run.ConfigurationSnapshotJson, cancellationToken);
            var thresholdPercent = setting.VarianceThresholdPercentage > 0 ? setting.VarianceThresholdPercentage : 20.00m;
            var thresholdNominal = setting.VarianceThresholdAmount > 0 ? setting.VarianceThresholdAmount : 2000000m;

            var employees = run.Employees.ToList();
            var profileIds = employees.Select(e => e.WorkforceProfileId).Distinct().ToList();

            var currentYear = run.PayrollPeriod?.FiscalYear ?? run.CreateDateTime.Year;
            var currentMonth = run.PayrollPeriod?.PeriodNumber ?? run.CreateDateTime.Month;

            // Kueri riwayat penggajian periode sebelumnya yang sudah disetujui/dibayarkan
            var previousEmployeesHistory = await _context.TrxPayrollRunEmployees
                .Include(e => e.PayrollRun)
                .Where(e => profileIds.Contains(e.WorkforceProfileId)
                    && e.PayrollRunId != run.Id
                    && e.IsActive
                    && (e.PayrollRun!.RunStatus == "Paid" || e.PayrollRun.RunStatus == "Approved" || e.PayrollRun.RunStatus == "Closed"))
                .OrderByDescending(e => e.PayrollRun!.CreateDateTime)
                .ToListAsync(cancellationToken);

            var previousNetPayMap = previousEmployeesHistory
                .GroupBy(e => e.WorkforceProfileId)
                .ToDictionary(g => g.Key, g => g.First().NetPay);

            // Bersihkan alert lama yang masih PendingReview (yang sudah di-Justified tetap dipertahankan)
            var existingPendingAlerts = await _context.TrxPayrollVarianceAlerts
                .Where(a => a.PayrollRunId == run.Id && a.ResolutionStatus == "PendingReview" && a.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var alert in existingPendingAlerts)
            {
                _context.TrxPayrollVarianceAlerts.Remove(alert);
            }

            var newAlerts = new List<TrxPayrollVarianceAlert>();

            foreach (var emp in employees)
            {
                var hasHistory = previousNetPayMap.TryGetValue(emp.WorkforceProfileId, out var prevNet);

                if (!hasHistory)
                {
                    // Anomali PAY-W4: Pegawai baru pertama kali gajian
                    newAlerts.Add(new TrxPayrollVarianceAlert
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunId = run.Id,
                        WorkforceProfileId = emp.WorkforceProfileId,
                        PeriodYear = currentYear,
                        PeriodMonth = currentMonth,
                        PreviousNetPay = 0m,
                        CurrentNetPay = emp.NetPay,
                        VarianceAmount = emp.NetPay,
                        VariancePercentage = 100m,
                        AlertSeverity = "Warning",
                        AlertReason = "Pegawai baru belum memiliki histori gaji periode sebelumnya (PAY-W4 NewEmployeeBaseline).",
                        ResolutionStatus = "PendingReview",
                        IsActive = true,
                        CreateBy = actorUserId,
                        CreateDateTime = DateTime.UtcNow
                    });
                    continue;
                }

                var diffNominal = Math.Abs(emp.NetPay - prevNet);
                var diffPercent = prevNet > 0
                    ? Math.Round((diffNominal / prevNet) * 100m, 2, MidpointRounding.AwayFromZero)
                    : 100m;

                // Proteksi PAY-W3: Sisa Take-Home Pay <= 30% dari Gaji Pokok
                if (emp.BaseSalary > 0 && emp.NetPay <= (emp.BaseSalary * 0.30m))
                {
                    newAlerts.Add(new TrxPayrollVarianceAlert
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunId = run.Id,
                        WorkforceProfileId = emp.WorkforceProfileId,
                        PeriodYear = currentYear,
                        PeriodMonth = currentMonth,
                        PreviousNetPay = prevNet,
                        CurrentNetPay = emp.NetPay,
                        VarianceAmount = diffNominal,
                        VariancePercentage = diffPercent,
                        AlertSeverity = "High",
                        AlertReason = $"Gaji bersih staf (Rp {emp.NetPay:N2}) tersisa <= 30% dari gaji pokok akibat tingginya beban potongan (PAY-W3 TakeHomePayProtection).",
                        ResolutionStatus = "PendingReview",
                        IsActive = true,
                        CreateBy = actorUserId,
                        CreateDateTime = DateTime.UtcNow
                    });
                    continue;
                }

                // Proteksi PAY-W1 / PAY-W2: Deviasi melampaui batas ambang persentase / nominal
                if (diffPercent >= thresholdPercent || diffNominal >= thresholdNominal)
                {
                    var isSurgeCritical = diffPercent >= 50m && emp.NetPay > prevNet;
                    var severity = isSurgeCritical ? "Critical" : "Warning";
                    var direction = emp.NetPay >= prevNet ? "Kenaikan" : "Penurunan";

                    var reason = isSurgeCritical
                        ? $"Lonjakan drastis gaji bersih staf >= 50% (+{diffPercent:N1}%, +Rp {diffNominal:N2}) dibanding bulan lalu (PAY-W2 CriticalSurge)."
                        : $"{direction} gaji bersih {diffPercent:N1}% (selisih Rp {diffNominal:N2}) melampaui ambang batas toleransi RS ({thresholdPercent:N0}% / Rp {thresholdNominal:N0}) (PAY-W1 VarianceAlert).";

                    newAlerts.Add(new TrxPayrollVarianceAlert
                    {
                        Id = Guid.NewGuid(),
                        PayrollRunId = run.Id,
                        WorkforceProfileId = emp.WorkforceProfileId,
                        PeriodYear = currentYear,
                        PeriodMonth = currentMonth,
                        PreviousNetPay = prevNet,
                        CurrentNetPay = emp.NetPay,
                        VarianceAmount = diffNominal,
                        VariancePercentage = diffPercent,
                        AlertSeverity = severity,
                        AlertReason = reason,
                        ResolutionStatus = "PendingReview",
                        IsActive = true,
                        CreateBy = actorUserId,
                        CreateDateTime = DateTime.UtcNow
                    });
                }
            }

            if (newAlerts.Count > 0)
            {
                _context.TrxPayrollVarianceAlerts.AddRange(newAlerts);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var allAlerts = await GetRunVarianceAlertsAsync(run.Id, cancellationToken);
            var alertList = allAlerts.Data ?? new List<PayrollVarianceAlertDto>();

            _logger.LogInformation(
                "Audit Variance Guard selesai untuk batch {RunNumber}. Terdeteksi {Count} anomali variansi ({Critical} Critical, {High} High, {Warning} Warning).",
                run.RunNumber, alertList.Count,
                alertList.Count(a => a.AlertSeverity == "Critical"),
                alertList.Count(a => a.AlertSeverity == "High"),
                alertList.Count(a => a.AlertSeverity == "Warning"));

            return PayrollServiceResult<EvaluateVarianceResultDto>.Ok(
                new EvaluateVarianceResultDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    TotalEmployeesChecked = employees.Count,
                    TotalAlertsGenerated = alertList.Count,
                    TotalCriticalAlerts = alertList.Count(a => a.AlertSeverity == "Critical"),
                    TotalHighAlerts = alertList.Count(a => a.AlertSeverity == "High"),
                    TotalWarningAlerts = alertList.Count(a => a.AlertSeverity == "Warning"),
                    TotalPendingReviewAlerts = alertList.Count(a => a.ResolutionStatus == "PendingReview"),
                    Message = $"Pemeriksaan variansi selesai: terdeteksi {alertList.Count} alert anomali ({alertList.Count(a => a.ResolutionStatus == "PendingReview")} memerlukan justifikasi resmi).",
                    Alerts = alertList
                },
                "Pemeriksaan variansi gaji berhasil.");
        }

        public async Task<PayrollServiceResult<List<PayrollVarianceAlertDto>>> GetRunVarianceAlertsAsync(
            Guid runId,
            CancellationToken cancellationToken = default)
        {
            var alerts = await _context.TrxPayrollVarianceAlerts
                .Include(a => a.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(a => a.ResolvedByUser)
                .Where(a => a.PayrollRunId == runId && a.IsActive)
                .OrderByDescending(a => a.AlertSeverity == "Critical")
                .ThenByDescending(a => a.AlertSeverity == "High")
                .ThenByDescending(a => a.VarianceAmount)
                .ToListAsync(cancellationToken);

            var dtos = alerts.Select(a => new PayrollVarianceAlertDto
            {
                Id = a.Id,
                PayrollRunId = a.PayrollRunId,
                WorkforceProfileId = a.WorkforceProfileId,
                EmployeeNumber = a.WorkforceProfile?.ProfileCode ?? "-",
                EmployeeName = a.WorkforceProfile?.DisplayName ?? "Staf Rumah Sakit",
                DepartmentName = a.WorkforceProfile?.PrimaryDepartment?.DepartmentName ?? "-",
                PeriodYear = a.PeriodYear,
                PeriodMonth = a.PeriodMonth,
                PreviousNetPay = a.PreviousNetPay,
                CurrentNetPay = a.CurrentNetPay,
                VarianceAmount = a.VarianceAmount,
                VariancePercentage = a.VariancePercentage,
                AlertSeverity = a.AlertSeverity,
                AlertReason = a.AlertReason,
                ResolutionStatus = a.ResolutionStatus,
                JustificationNotes = a.JustificationNotes,
                ResolvedByUserId = a.ResolvedByUserId,
                ResolvedByUserName = a.ResolvedByUser?.UserName,
                ResolvedAt = a.ResolvedAt
            }).ToList();

            return PayrollServiceResult<List<PayrollVarianceAlertDto>>.Ok(dtos, "Daftar alert variansi berhasil diambil.");
        }

        private async Task<HrdPayrollSettingDto> ResolveEffectiveSettingAsync(
            string? snapshotJson,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(snapshotJson))
            {
                try
                {
                    var snapshot = JsonSerializer.Deserialize<HrdPayrollSettingDto>(snapshotJson, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (snapshot != null) return snapshot;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Gagal mem-parsing snapshot setting JSON. Menggunakan setting aktif saat ini.");
                }
            }

            return await _settingService.GetSettingAsync(cancellationToken);
        }
    }
}
