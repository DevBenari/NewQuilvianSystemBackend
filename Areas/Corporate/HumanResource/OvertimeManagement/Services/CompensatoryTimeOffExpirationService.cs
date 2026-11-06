using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services
{
    /// <summary>
    /// Eksekutor harian penghangusan saldo cuti pengganti CTO yang telah melampaui masa berlaku 90 hari kalender tanpa kompensasi uang tunai (BE-HRD-142, VR-OT-13, VR-OT-14, HRD-DEC-118).
    /// </summary>
    public class CompensatoryTimeOffExpirationService : ICompensatoryTimeOffExpirationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<CompensatoryTimeOffExpirationService> _logger;

        public CompensatoryTimeOffExpirationService(
            ApplicationDbContext context,
            ILogger<CompensatoryTimeOffExpirationService> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<CompensatoryExpirationRunResultDto> ExecuteExpirationRunAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var today = DateOnly.FromDateTime(now);

            // Cari seluruh tiket CTO yang telah melewati batas 90 hari dan belum ditandai hangus (VR-OT-13)
            var expiredTickets = await _context.Set<TrxCompensatoryTimeOff>()
                .Where(x => !x.IsDelete &&
                            !x.IsExpired &&
                            x.ExpiryDate.HasValue &&
                            x.ExpiryDate.Value < today &&
                            x.RemainingHours > 0)
                .ToListAsync(cancellationToken);

            int expiredCount = 0;
            decimal totalForfeitedDays = 0;
            decimal totalForfeitedHours = 0;

            foreach (var ticket in expiredTickets)
            {
                var forfeitedHours = ticket.RemainingHours;
                var forfeitedDays = ticket.GrantedDays;

                ticket.IsExpired = true;
                ticket.CompensatoryStatus = "Expired";
                ticket.ExpiredAt = now;
                ticket.ExpiredMinutes = (int)(forfeitedHours * 60m);
                ticket.RemainingHours = 0;
                ticket.RemainingMinutes = 0;
                ticket.Notes = $"{ticket.Notes} | Hangus otomatis setelah 90 hari kalender per regulasi RS PP 35/2021 (VR-OT-13, VR-OT-14).";
                ticket.UpdateDateTime = now;

                // Integrasi INT-HR-49: Posting mutasi debit penghangusan saldo di buku besar cuti Sub-08
                if (ticket.LeaveTypeId.HasValue)
                {
                    var lastLedgerEntry = await _context.Set<TrxLeaveBalanceLedger>()
                        .Where(x => x.WorkforceProfileId == ticket.WorkforceProfileId &&
                                    x.LeaveTypeId == ticket.LeaveTypeId.Value &&
                                    !x.IsDelete)
                        .OrderByDescending(x => x.TransactionDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    var currentBalance = lastLedgerEntry?.BalanceAfter ?? 0m;
                    var newBalance = Math.Max(0m, currentBalance - forfeitedDays);

                    var forfeitureLedger = new TrxLeaveBalanceLedger
                    {
                        Id = Guid.NewGuid(),
                        WorkforceProfileId = ticket.WorkforceProfileId,
                        LeaveTypeId = ticket.LeaveTypeId.Value,
                        PeriodYear = today.Year,
                        TransactionType = "CompensatoryForfeiture",
                        AmountDays = -forfeitedDays,
                        BalanceBefore = currentBalance,
                        BalanceAfter = newBalance,
                        TransactionDate = now,
                        IsForfeited = true,
                        Notes = $"Penghangusan otomatis kuota hak cuti CTO {ticket.CreditNumber} akibat kedaluwarsa 90 hari ({forfeitedDays} hari).",
                        CreateDateTime = now,
                        IsDelete = false
                    };

                    await _context.Set<TrxLeaveBalanceLedger>().AddAsync(forfeitureLedger, cancellationToken);
                }

                expiredCount++;
                totalForfeitedDays += forfeitedDays;
                totalForfeitedHours += forfeitedHours;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Eksekusi penghangusan CTO tuntas. Total tiket hangus: {Count}, Total hari: {Days}, Total jam: {Hours}",
                expiredCount, totalForfeitedDays, totalForfeitedHours);

            return new CompensatoryExpirationRunResultDto
            {
                TotalProcessedTickets = expiredTickets.Count,
                ExpiredTicketsCount = expiredCount,
                TotalForfeitedDays = totalForfeitedDays,
                TotalForfeitedHours = totalForfeitedHours,
                ExecutedAt = now,
                Message = expiredCount > 0
                    ? $"Eksekusi penghangusan berhasil: {expiredCount} tiket CTO ({totalForfeitedDays} hari / {totalForfeitedHours} jam) hangus otomatis setelah 90 hari kalender."
                    : "Tidak ada tiket CTO yang kedaluwarsa pada tanggal hari ini."
            };
        }
    }
}
