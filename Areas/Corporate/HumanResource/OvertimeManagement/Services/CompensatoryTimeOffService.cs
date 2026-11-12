using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.LeaveAndOvertime.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Layanan penerbitan hak cuti pengganti lembur (Compensatory Time Off / CTO), rasio 8 jam = 1 hari, masa kedaluwarsa 90 hari, dan posting mutasi kredit cuti Sub-08 (BE-HRD-141, INT-HR-49, VR-OT-13, VR-OT-15, HRD-DEC-118).
    /// </summary>
    public class CompensatoryTimeOffService : ICompensatoryTimeOffService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompensatoryTimeOffService> _logger;

        public CompensatoryTimeOffService(
            ApplicationDbContext context,
            ILogger<CompensatoryTimeOffService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>> GenerateCompensatoryTimeOffAsync(
            Guid realizationId,
            Guid hrAdminUserId,
            CancellationToken cancellationToken = default)
        {
            var realization = await _context.Set<TrxOvertimeRealization>()
                .Include(x => x.WorkforceProfile)
                .FirstOrDefaultAsync(x => x.Id == realizationId && !x.IsDelete, cancellationToken);

            if (realization == null)
            {
                return OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>.Fail(
                    StatusCodes.Status404NotFound, "Klaim realisasi lembur tidak ditemukan.");
            }

            if (realization.CompensationType != "CompensatoryTimeOff")
            {
                return OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Klaim ini memilih kompensasi finansial ({realization.CompensationType}), bukan hak cuti pengganti (CompensatoryTimeOff).");
            }

            if (realization.RealizationStatus != "Verified")
            {
                return OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>.Fail(
                    StatusCodes.Status400BadRequest,
                    $"Tiket CTO hanya dapat diterbitkan untuk lembur yang telah berstatus Verified. Status saat ini: {realization.RealizationStatus}.");
            }

            if (realization.VerifiedMinutes <= 0)
            {
                return OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>.Fail(
                    StatusCodes.Status400BadRequest, "Menit lembur terverifikasi adalah 0 menit sehingga tidak ada kuota CTO yang dapat diterbitkan.");
            }

            // Periksa apakah tiket CTO sudah pernah diterbitkan untuk klaim ini
            var existingTicket = await _context.Set<TrxCompensatoryTimeOff>()
                .FirstOrDefaultAsync(x => x.OvertimeRealizationId == realization.Id && !x.IsDelete, cancellationToken);

            if (existingTicket != null)
            {
                return OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>.Fail(
                    StatusCodes.Status409Conflict,
                    $"Tiket cuti pengganti CTO sudah pernah diterbitkan sebelumnya ({existingTicket.CreditNumber}).");
            }

            var now = DateTime.UtcNow;
            var year = now.Year;
            var month = now.Month;
            var prefix = $"CTO/{year:D4}/{month:D2}/";

            var lastCount = await _context.Set<TrxCompensatoryTimeOff>()
                .Where(x => x.CreditNumber.StartsWith(prefix))
                .CountAsync(cancellationToken);

            var nextSeq = lastCount + 1;
            var creditNumber = $"{prefix}{nextSeq:D4}";

            // 1. Rasio Konversi: 8 Jam Lembur = 1 Hari Cuti CTO (VR-OT-15, HRD-DEC-118)
            decimal grantedDays = Math.Round((decimal)realization.VerifiedMinutes / 480m, 1);
            decimal remainingHours = Math.Round((decimal)realization.VerifiedMinutes / 60m, 2);

            // 2. Masa Berlaku: 90 Hari Kalender Sejak Tanggal Pelaksanaan Lembur (VR-OT-13)
            var expiryDate = realization.ActualStartDate.AddDays(90);

            // 3. Integrasi INT-HR-49: Posting Mutasi Kredit ke Buku Besar Cuti Sub-08 (TrxLeaveBalanceLedger)
            var leaveType = await _context.Set<MstLeaveType>()
                .FirstOrDefaultAsync(x => (x.LeaveTypeCode.Contains("CTO") || x.LeaveTypeName.Contains("Pengganti") || x.IsActive) && !x.IsDelete, cancellationToken);

            if (leaveType == null)
            {
                leaveType = await _context.Set<MstLeaveType>().FirstOrDefaultAsync(cancellationToken);
            }

            var leaveTypeId = leaveType?.Id ?? Guid.NewGuid();

            var lastLedgerEntry = await _context.Set<TrxLeaveBalanceLedger>()
                .Where(x => x.WorkforceProfileId == realization.WorkforceProfileId &&
                            x.LeaveTypeId == leaveTypeId &&
                            !x.IsDelete)
                .OrderByDescending(x => x.TransactionDate)
                .FirstOrDefaultAsync(cancellationToken);

            var balanceBefore = lastLedgerEntry?.BalanceAfter ?? 0m;
            var balanceAfter = balanceBefore + grantedDays;

            var ledgerEntry = new TrxLeaveBalanceLedger
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = realization.WorkforceProfileId,
                LeaveTypeId = leaveTypeId,
                PeriodYear = realization.ActualStartDate.Year,
                TransactionType = "CompensatoryCredit",
                AmountDays = grantedDays,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                TransactionDate = now,
                ExpiryDate = expiryDate.ToDateTime(TimeOnly.MinValue),
                Notes = $"Kredit hak libur CTO {creditNumber} (Lembur terverifikasi: {realization.VerifiedMinutes}m / 8 jam = {grantedDays} hari)",
                CreateBy = hrAdminUserId,
                CreateDateTime = now,
                IsDelete = false
            };

            await _context.Set<TrxLeaveBalanceLedger>().AddAsync(ledgerEntry, cancellationToken);

            // 4. Buat Tiket TrxCompensatoryTimeOff
            var ctoTicket = new TrxCompensatoryTimeOff
            {
                Id = Guid.NewGuid(),
                CreditNumber = creditNumber,
                WorkforceProfileId = realization.WorkforceProfileId,
                OvertimeRequestId = realization.OvertimeRequestId,
                OvertimeRealizationId = realization.Id,
                LeaveTypeId = leaveTypeId,
                LeaveBalanceTransactionId = ledgerEntry.Id,
                EarnedDate = realization.ActualStartDate,
                EffectiveStartDate = realization.ActualStartDate,
                ExpiryDate = expiryDate,
                GrantedDays = grantedDays,
                RemainingHours = remainingHours,
                SourceOvertimeMinutes = realization.VerifiedMinutes,
                ConversionRate = 1.0m,
                EarnedMinutes = realization.VerifiedMinutes,
                RemainingMinutes = realization.VerifiedMinutes,
                CompensatoryStatus = "Available",
                IsExpired = false,
                GeneratedAt = now,
                ApprovedAt = now,
                ApprovedByUserId = hrAdminUserId,
                Notes = $"Hak libur pengganti lembur tanggal {realization.ActualStartDate:yyyy-MM-dd}. Berlaku s.d. {expiryDate:yyyy-MM-dd} (90 hari).",
                IsActive = true,
                CreateBy = hrAdminUserId,
                CreateDateTime = now,
                IsDelete = false
            };

            await _context.Set<TrxCompensatoryTimeOff>().AddAsync(ctoTicket, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Tiket CTO {CreditNumber} berhasil diterbitkan untuk pegawai {ProfileId}. Kuota: {Days} hari ({Hours} jam). Berlaku s.d: {ExpiryDate}",
                ctoTicket.CreditNumber, ctoTicket.WorkforceProfileId, grantedDays, remainingHours, expiryDate);

            var result = new GenerateCompensatoryTimeOffResultDto
            {
                CompensatoryTimeOffId = ctoTicket.Id,
                CreditNumber = ctoTicket.CreditNumber,
                WorkforceProfileId = ctoTicket.WorkforceProfileId,
                EmployeeName = realization.WorkforceProfile != null ? realization.WorkforceProfile.DisplayName : string.Empty,
                EarnedDate = ctoTicket.EarnedDate,
                ExpiryDate = expiryDate,
                SourceOvertimeMinutes = ctoTicket.SourceOvertimeMinutes,
                GrantedDays = grantedDays,
                RemainingHours = remainingHours,
                CompensatoryStatus = ctoTicket.CompensatoryStatus,
                LeaveBalanceLedgerId = ledgerEntry.Id,
                Message = $"Tiket cuti pengganti CTO {creditNumber} berhasil diterbitkan: {grantedDays} hari cuti (masa berlaku 90 hari s.d. {expiryDate:yyyy-MM-dd})."
            };

            return OvertimePlanningServiceResult<GenerateCompensatoryTimeOffResultDto>.Ok(
                result,
                "Tiket hak cuti pengganti lembur (CTO) berhasil diterbitkan.",
                StatusCodes.Status201Created);
        }

        public async Task<List<CompensatoryTimeOffTicketDto>> GetStaffTicketsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken = default)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var tickets = await _context.Set<TrxCompensatoryTimeOff>()
                .Include(x => x.WorkforceProfile)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.EarnedDate)
                .ToListAsync(cancellationToken);

            return tickets.Select(t =>
            {
                var daysUntilExpiry = t.ExpiryDate.HasValue
                    ? t.ExpiryDate.Value.DayNumber - today.DayNumber
                    : 0;

                return new CompensatoryTimeOffTicketDto
                {
                    Id = t.Id,
                    CreditNumber = t.CreditNumber,
                    WorkforceProfileId = t.WorkforceProfileId,
                    EmployeeName = t.WorkforceProfile != null ? t.WorkforceProfile.DisplayName : string.Empty,
                    EarnedDate = t.EarnedDate,
                    EffectiveStartDate = t.EffectiveStartDate,
                    ExpiryDate = t.ExpiryDate,
                    DaysUntilExpiry = Math.Max(0, daysUntilExpiry),
                    IsExpiringSoon = daysUntilExpiry is > 0 and <= 14,
                    GrantedDays = t.GrantedDays,
                    RemainingHours = t.RemainingHours,
                    SourceOvertimeMinutes = t.SourceOvertimeMinutes,
                    CompensatoryStatus = t.CompensatoryStatus,
                    IsExpired = t.IsExpired,
                    Notes = t.Notes
                };
            }).ToList();
        }
    }
}
