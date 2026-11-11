using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Manajemen siklus penutupan periode cut-off bulanan (tgl 20), penguncian data lembur, dan rekonsiliasi retrospektif lembur susulan (BE-HRD-143, HRD-DEC-121).
    /// </summary>
    public class OvertimePeriodManagementService : IOvertimePeriodManagementService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OvertimePeriodManagementService> _logger;

        public OvertimePeriodManagementService(
            ApplicationDbContext context,
            ILogger<OvertimePeriodManagementService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OvertimePeriodDto> GetOrCreateCurrentPeriodAsync(CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            // Siklus Cut-Off Bulanan Rumah Sakit: Tgl 21 bulan lalu s.d. Tgl 20 bulan berjalan (HRD-DEC-121)
            DateOnly startDate;
            DateOnly endDate;
            DateOnly cutoffDate;
            string periodCode;
            string periodName;

            if (today.Day > 20)
            {
                // Misalnya 25 November: periode 21 Nov s.d. 20 Des
                startDate = new DateOnly(today.Year, today.Month, 21);
                var nextMonth = today.AddMonths(1);
                endDate = new DateOnly(nextMonth.Year, nextMonth.Month, 20);
                cutoffDate = endDate;
                periodCode = $"OTP-{nextMonth.Year:D4}-{nextMonth.Month:D2}";
                periodName = $"Periode Lembur {nextMonth:MMMM yyyy}";
            }
            else
            {
                // Misalnya 5 November: periode 21 Okt s.d. 20 Nov
                var prevMonth = today.AddMonths(-1);
                startDate = new DateOnly(prevMonth.Year, prevMonth.Month, 21);
                endDate = new DateOnly(today.Year, today.Month, 20);
                cutoffDate = endDate;
                periodCode = $"OTP-{today.Year:D4}-{today.Month:D2}";
                periodName = $"Periode Lembur {today:MMMM yyyy}";
            }

            var period = await _context.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.PeriodCode == periodCode && !x.IsDelete, cancellationToken);

            if (period == null)
            {
                period = new TrxOvertimePeriod
                {
                    Id = Guid.NewGuid(),
                    PeriodCode = periodCode,
                    PeriodName = periodName,
                    StartDate = startDate,
                    EndDate = endDate,
                    CutoffDate = cutoffDate,
                    PeriodStatus = "Open",
                    TotalCtoGrantedHours = 0,
                    IsHandoffExecuted = false,
                    RequireAttendanceFinal = true,
                    RequireVerificationComplete = true,
                    RequireSettlementComplete = true,
                    IsActive = true,
                    CreateDateTime = DateTime.UtcNow,
                    IsDelete = false
                };

                await _context.Set<TrxOvertimePeriod>().AddAsync(period, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Periode lembur baru {PeriodCode} berhasil diinisialisasi otomatis.", periodCode);
            }

            var verifiedCount = await _context.Set<TrxOvertimeRealization>()
                .CountAsync(x => !x.IsDelete &&
                                 x.ActualStartDate >= period.StartDate &&
                                 x.ActualStartDate <= period.CutoffDate &&
                                 x.RealizationStatus == "Verified", cancellationToken);

            var retrospectiveCount = await _context.Set<TrxOvertimeRealization>()
                .CountAsync(x => !x.IsDelete &&
                                 x.IsRetrospectiveAdjustment &&
                                 x.RealizationStatus == "Verified" &&
                                 !x.IsPayrollPosted, cancellationToken);

            return new OvertimePeriodDto
            {
                Id = period.Id,
                PeriodCode = period.PeriodCode,
                PeriodName = period.PeriodName,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                CutoffDate = period.CutoffDate ?? period.EndDate,
                PeriodStatus = period.PeriodStatus,
                TotalCtoGrantedHours = period.TotalCtoGrantedHours,
                IsHandoffExecuted = period.IsHandoffExecuted,
                TotalVerifiedRealizations = verifiedCount,
                TotalRetrospectiveAdjustments = retrospectiveCount,
                ClosedAt = period.ClosedAt,
                ReopenedAt = period.ReopenedAt,
                ReopenCount = period.ReopenCount
            };
        }

        public async Task<List<OvertimePeriodDto>> GetAllPeriodsAsync(CancellationToken cancellationToken = default)
        {
            var periods = await _context.Set<TrxOvertimePeriod>()
                .Where(x => !x.IsDelete)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);

            var result = new List<OvertimePeriodDto>();
            foreach (var p in periods)
            {
                var cutoff = p.CutoffDate ?? p.EndDate;
                var verifiedCount = await _context.Set<TrxOvertimeRealization>()
                    .CountAsync(x => !x.IsDelete &&
                                     x.ActualStartDate >= p.StartDate &&
                                     x.ActualStartDate <= cutoff &&
                                     x.RealizationStatus == "Verified", cancellationToken);

                result.Add(new OvertimePeriodDto
                {
                    Id = p.Id,
                    PeriodCode = p.PeriodCode,
                    PeriodName = p.PeriodName,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    CutoffDate = cutoff,
                    PeriodStatus = p.PeriodStatus,
                    TotalCtoGrantedHours = p.TotalCtoGrantedHours,
                    IsHandoffExecuted = p.IsHandoffExecuted,
                    TotalVerifiedRealizations = verifiedCount,
                    ClosedAt = p.ClosedAt,
                    ReopenedAt = p.ReopenedAt,
                    ReopenCount = p.ReopenCount
                });
            }

            return result;
        }

        public async Task<OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>> ClosePeriodAsync(
            Guid periodId,
            CloseOvertimePeriodCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == periodId && !x.IsDelete, cancellationToken);

            if (period == null)
            {
                return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Fail(
                    StatusCodes.Status404NotFound, "Periode lembur tidak ditemukan.");
            }

            if (period.PeriodStatus == "Locked" || period.PeriodStatus == "Closed")
            {
                return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Fail(
                    StatusCodes.Status400BadRequest, $"Periode lembur sudah terkunci/ditutup ({period.PeriodStatus}).");
            }

            var cutoff = period.CutoffDate ?? period.EndDate;

            // 1. Cek Anomali Discrepancy yang masih tertahan (VR-OT-10, HRD-DEC-121)
            var pendingAnomaliesCount = await _context.Set<TrxOvertimeRealization>()
                .CountAsync(x => !x.IsDelete &&
                                 x.ActualStartDate >= period.StartDate &&
                                 x.ActualStartDate <= cutoff &&
                                 (x.RealizationStatus == "FlaggedDiscrepancy" || x.RealizationStatus == "WaitingVerification"),
                            cancellationToken);

            if (pendingAnomaliesCount > 0 && !command.ForceCloseWithPendingAnomalies)
            {
                return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Penutupan periode ditahan: Masih terdapat {pendingAnomaliesCount} klaim lembur yang menunggu verifikasi atau berstatus FlaggedDiscrepancy. Harap selesaikan anomali logbook fisik atau gunakan opsi paksa.");
            }

            // 2. Rekonsiliasi Retrospektif (HRD-DEC-121): Deteksi klaim dari tanggal sebelum period.StartDate yang baru disetujui
            var retrospectiveClaims = await _context.Set<TrxOvertimeRealization>()
                .Where(x => !x.IsDelete &&
                            x.ActualStartDate < period.StartDate &&
                            x.RealizationStatus == "Verified" &&
                            !x.IsPayrollPosted &&
                            !x.IsRetrospectiveAdjustment)
                .ToListAsync(cancellationToken);

            foreach (var retro in retrospectiveClaims)
            {
                retro.IsRetrospectiveAdjustment = true;
                retro.RealizationNotes = $"{retro.RealizationNotes} | Diikutkan dalam rekonsiliasi retrospektif periode {period.PeriodCode}";
                retro.UpdateDateTime = DateTime.UtcNow;
            }

            // 3. Hitung Total Jam CTO yang diterbitkan dalam periode ini
            var ctoTotalHours = await _context.Set<TrxCompensatoryTimeOff>()
                .Where(x => !x.IsDelete &&
                            x.EarnedDate >= period.StartDate &&
                            x.EarnedDate <= cutoff)
                .SumAsync(x => x.RemainingHours, cancellationToken);

            var prevStatus = period.PeriodStatus;
            period.PeriodStatus = "Locked";
            period.ClosedAt = DateTime.UtcNow;
            period.ClosedByUserId = actorUserId;
            period.CloseReason = command?.CloseReason ?? "Penutupan resmi cut-off lembur bulanan tgl 20";
            period.TotalCtoGrantedHours = ctoTotalHours;
            period.CloseVersion += 1;
            period.UpdateBy = actorUserId;
            period.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Periode lembur {PeriodCode} berhasil ditutup dan dikunci oleh user {UserId}. Klaim retrospektif: {RetroCount}",
                period.PeriodCode, actorUserId, retrospectiveClaims.Count);

            var totalIncluded = await _context.Set<TrxOvertimeRealization>()
                .CountAsync(x => !x.IsDelete &&
                                 x.ActualStartDate >= period.StartDate &&
                                 x.ActualStartDate <= cutoff &&
                                 x.RealizationStatus == "Verified", cancellationToken);

            var result = new OvertimePeriodOperationResultDto
            {
                PeriodId = period.Id,
                PeriodCode = period.PeriodCode,
                PreviousStatus = prevStatus,
                NewStatus = period.PeriodStatus,
                TotalIncludedRealizations = totalIncluded,
                RetrospectiveCount = retrospectiveClaims.Count,
                Message = $"Periode lembur {period.PeriodCode} berhasil ditutup dan dikunci. Sebanyak {totalIncluded} klaim reguler dan {retrospectiveClaims.Count} klaim rapel retrospektif siap diserahterimakan ke Payroll."
            };

            return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Ok(
                result,
                "Periode lembur berhasil ditutup.",
                StatusCodes.Status200OK);
        }

        public async Task<OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>> ReopenPeriodAsync(
            Guid periodId,
            ReopenOvertimePeriodCommand command,
            Guid actorUserId,
            CancellationToken cancellationToken = default)
        {
            var period = await _context.Set<TrxOvertimePeriod>()
                .FirstOrDefaultAsync(x => x.Id == periodId && !x.IsDelete, cancellationToken);

            if (period == null)
            {
                return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Fail(
                    StatusCodes.Status404NotFound, "Periode lembur tidak ditemukan.");
            }

            if (period.PeriodStatus != "Locked" && period.PeriodStatus != "Closed")
            {
                return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Fail(
                    StatusCodes.Status400BadRequest, $"Hanya periode berstatus Locked atau Closed yang dapat dibuka kembali. Status saat ini: {period.PeriodStatus}.");
            }

            if (period.IsHandoffExecuted)
            {
                return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Fail(
                    StatusCodes.Status409Conflict,
                    "Periode tidak dapat dibuka kembali karena paket data telah diserahterimakan secara permanen ke Modul Penggajian (Payroll Handoff executed).");
            }

            var prevStatus = period.PeriodStatus;
            period.PeriodStatus = "Open";
            period.ReopenedAt = DateTime.UtcNow;
            period.ReopenedByUserId = actorUserId;
            period.ReopenReason = command.ReopenReason;
            period.ReopenCount += 1;
            period.UpdateBy = actorUserId;
            period.UpdateDateTime = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogWarning(
                "Periode lembur {PeriodCode} dibuka kembali oleh {UserId} dengan alasan: {Reason}",
                period.PeriodCode, actorUserId, command.ReopenReason);

            var result = new OvertimePeriodOperationResultDto
            {
                PeriodId = period.Id,
                PeriodCode = period.PeriodCode,
                PreviousStatus = prevStatus,
                NewStatus = period.PeriodStatus,
                Message = $"Periode lembur {period.PeriodCode} berhasil dibuka kembali untuk penyesuaian data koreksi administratif."
            };

            return OvertimePlanningServiceResult<OvertimePeriodOperationResultDto>.Ok(
                result,
                "Periode lembur berhasil dibuka kembali.",
                StatusCodes.Status200OK);
        }
    }
}
