using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Services
{
    public class PayrollVarianceResolutionService : IPayrollVarianceResolutionService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PayrollVarianceResolutionService> _logger;

        public PayrollVarianceResolutionService(
            ApplicationDbContext context,
            ILogger<PayrollVarianceResolutionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PayrollServiceResult<PayrollVarianceAlertDto>> ResolveAlertAsync(
            Guid alertId,
            ResolveVarianceAlertCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation(
                "Memproses resolusi justifikasi audit alert ID: {AlertId}. Actor: {ActorId}",
                alertId, actorUserId);

            if (command == null || string.IsNullOrWhiteSpace(command.JustificationNotes))
            {
                return PayrollServiceResult<PayrollVarianceAlertDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Catatan alasan justifikasi audit wajib diisi.");
            }

            var trimmedNotes = command.JustificationNotes.Trim();
            if (trimmedNotes.Length < 20)
            {
                return PayrollServiceResult<PayrollVarianceAlertDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan justifikasi variansi wajib minimal 20 karakter untuk audit compliance rumah sakit (VR-PAY-15).");
            }

            var alert = await _context.TrxPayrollVarianceAlerts
                .Include(a => a.WorkforceProfile)
                    .ThenInclude(p => p!.PrimaryDepartment)
                .Include(a => a.PayrollRun)
                .FirstOrDefaultAsync(a => a.Id == alertId && a.IsActive, cancellationToken);

            if (alert == null)
            {
                return PayrollServiceResult<PayrollVarianceAlertDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Alert variansi dengan ID {alertId} tidak ditemukan.");
            }

            if (alert.PayrollRun != null && alert.PayrollRun.IsLocked)
            {
                return PayrollServiceResult<PayrollVarianceAlertDto>.Fail(
                    StatusCodes.Status422UnprocessableEntity,
                    $"Batch penggajian {alert.PayrollRun.RunNumber} telah berstatus final terkunci dan tidak dapat diubah.");
            }

            alert.JustificationNotes = trimmedNotes;
            alert.ResolutionStatus = "Justified";
            alert.ResolvedByUserId = actorUserId;
            alert.ResolvedAt = DateTime.UtcNow;
            alert.UpdateDateTime = DateTime.UtcNow;
            alert.UpdateBy = actorUserId;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Alert variansi ID {AlertId} berhasil di-justifikasi oleh user ID {ActorId}.",
                alert.Id, actorUserId);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == actorUserId, cancellationToken);

            var dto = new PayrollVarianceAlertDto
            {
                Id = alert.Id,
                PayrollRunId = alert.PayrollRunId,
                WorkforceProfileId = alert.WorkforceProfileId,
                EmployeeNumber = alert.WorkforceProfile?.ProfileCode ?? "-",
                EmployeeName = alert.WorkforceProfile?.DisplayName ?? "Staf",
                DepartmentName = alert.WorkforceProfile?.PrimaryDepartment?.DepartmentName ?? "-",
                PeriodYear = alert.PeriodYear,
                PeriodMonth = alert.PeriodMonth,
                PreviousNetPay = alert.PreviousNetPay,
                CurrentNetPay = alert.CurrentNetPay,
                VarianceAmount = alert.VarianceAmount,
                VariancePercentage = alert.VariancePercentage,
                AlertSeverity = alert.AlertSeverity,
                AlertReason = alert.AlertReason,
                ResolutionStatus = alert.ResolutionStatus,
                JustificationNotes = alert.JustificationNotes,
                ResolvedByUserId = alert.ResolvedByUserId,
                ResolvedByUserName = user?.UserName,
                ResolvedAt = alert.ResolvedAt
            };

            return PayrollServiceResult<PayrollVarianceAlertDto>.Ok(
                dto,
                "Justifikasi audit variansi berhasil disimpan.");
        }

        public async Task<PayrollServiceResult<BatchVarianceSubmissionCheckDto>> CheckBatchCanBeSubmittedAsync(
            Guid runId,
            CancellationToken cancellationToken = default)
        {
            var run = await _context.TrxPayrollRuns
                .FirstOrDefaultAsync(r => r.Id == runId && r.IsActive, cancellationToken);

            if (run == null)
            {
                return PayrollServiceResult<BatchVarianceSubmissionCheckDto>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Batch penggajian dengan ID {runId} tidak ditemukan.");
            }

            var pendingAlerts = await _context.TrxPayrollVarianceAlerts
                .Include(a => a.WorkforceProfile)
                .Where(a => a.PayrollRunId == runId && a.ResolutionStatus == "PendingReview" && a.IsActive)
                .ToListAsync(cancellationToken);

            var canSubmit = pendingAlerts.Count == 0;
            var summaries = pendingAlerts.Select(a =>
                $"{a.WorkforceProfile?.DisplayName ?? "Staf"} ({a.AlertSeverity}): {a.AlertReason}").ToList();

            var message = canSubmit
                ? "Seluruh alert anomali variansi telah terjustifikasi. Batch siap diajukan untuk review manajer."
                : $"Pengajuan batch diblokir keras (PAY-B3). Masih terdapat {pendingAlerts.Count} anomali variansi gaji yang belum memiliki catatan justifikasi resmi (VR-PAY-14).";

            return PayrollServiceResult<BatchVarianceSubmissionCheckDto>.Ok(
                new BatchVarianceSubmissionCheckDto
                {
                    RunId = run.Id,
                    RunNumber = run.RunNumber,
                    CanBeSubmitted = canSubmit,
                    PendingAlertsCount = pendingAlerts.Count,
                    PendingAlertSummaries = summaries,
                    Message = message
                },
                canSubmit ? "Pengecekan lolos." : "Terdapat pending variansi.");
        }
    }
}
