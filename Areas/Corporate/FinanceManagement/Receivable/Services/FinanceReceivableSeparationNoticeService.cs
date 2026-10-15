using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Layanan penerimaan notifikasi pemisahan pegawai di Finance (INT-HR-FIN-003 §P.5, FIN-DEC-169, FIN-DEC-194).
/// Menerima pemberitahuan bahwa pegawai akan berhenti kerja (resign/PHK) agar Finance dapat memetakan
/// seluruh kewajiban piutang aktif pegawai bersangkutan sebelum hak akhir dicairkan.
/// Idempoten per SeparationId (AC 2) dan aman bagi pegawai tanpa piutang (AC 3).
/// </summary>
public sealed class FinanceReceivableSeparationNoticeService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.SeparationNotice";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public FinanceReceivableSeparationNoticeService(
        ApplicationDbContext dbContext,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Memproses notifikasi pemisahan pegawai dari HR.
    /// Idempoten per SeparationId (kiriman ulang dengan SeparationId yang sama tidak menggandakan mutasi).
    /// </summary>
    public async Task<SeparationNoticeResponse> ProcessNoticeAsync(
        SeparationNoticeRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.EmployeeId == Guid.Empty)
            throw new ArgumentException("EmployeeId wajib diisi.", nameof(request));

        if (request.SeparationId == Guid.Empty)
            throw new ArgumentException("SeparationId wajib diisi.", nameof(request));

        // 1. Periksa idempotensi: apakah SeparationId ini sudah pernah dicatat mutasinya di Finance (AC 2)
        var existingMovements = await _dbContext.FinReceivableMovements
            .Where(x => !x.IsDelete
                && x.CausationId == request.SeparationId
                && x.MovementType == FinReceivableMovementTypes.SeparationNotice)
            .ToListAsync(cancellationToken);

        if (existingMovements.Count > 0)
        {
            var currentReceivables = await _dbContext.FinReceivables
                .Where(x => !x.IsDelete
                    && x.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit
                    && x.BenefitOwnerId == request.EmployeeId
                    && (x.Status == FinReceivableStatuses.Outstanding || x.Status == FinReceivableStatuses.Partial))
                .ToListAsync(cancellationToken);

            var currentTotal = currentReceivables.Sum(x => x.OutstandingAmount);

            await _loggerService.AuditAsync(LogCategory, "SeparationNotice.IdempotentReplay",
                $"Notifikasi pemisahan pegawai {request.EmployeeId} (SeparationId: {request.SeparationId}) merupakan kiriman ulang (idempoten). Nol mutasi baru dibuat.",
                new
                {
                    request.EmployeeId,
                    request.SeparationId,
                    ExistingMovementCount = existingMovements.Count,
                    CurrentTotal = currentTotal
                });

            return new SeparationNoticeResponse
            {
                EmployeeId = request.EmployeeId,
                SeparationId = request.SeparationId,
                ActiveReceivableCount = currentReceivables.Count,
                FlaggedReceivableCount = currentReceivables.Count,
                TotalOutstandingAmount = currentTotal,
                IsIdempotentReplay = true,
                Status = "IDEMPOTENT_REPLAY",
                Message = $"Pemberitahuan pemisahan (SeparationId: {request.SeparationId}) sudah pernah diproses sebelumnya. Tidak ada mutasi baru yang dibuat."
            };
        }

        // 2. Ambil seluruh piutang aktif milik pegawai bersangkutan
        var activeReceivables = await _dbContext.FinReceivables
            .Where(x => !x.IsDelete
                && x.DebtorType == FinReceivableDebtorTypes.EmployeeBenefit
                && x.BenefitOwnerId == request.EmployeeId
                && (x.Status == FinReceivableStatuses.Outstanding || x.Status == FinReceivableStatuses.Partial))
            .ToListAsync(cancellationToken);

        // 3. Bila pegawai tidak memiliki piutang aktif (AC 3: Pegawai tanpa piutang: sukses tanpa perubahan)
        if (activeReceivables.Count == 0)
        {
            await _loggerService.AuditAsync(LogCategory, "SeparationNotice.NoActiveReceivables",
                $"Pemberitahuan pemisahan pegawai diterima untuk Pegawai {request.EmployeeId} (SeparationId: {request.SeparationId}, Tanggal: {request.SeparationDate:yyyy-MM-dd}). Pegawai tidak memiliki piutang aktif di Finance.",
                new
                {
                    request.EmployeeId,
                    request.SeparationId,
                    request.SeparationDate,
                    request.Reason
                });

            return new SeparationNoticeResponse
            {
                EmployeeId = request.EmployeeId,
                SeparationId = request.SeparationId,
                ActiveReceivableCount = 0,
                FlaggedReceivableCount = 0,
                TotalOutstandingAmount = 0m,
                IsIdempotentReplay = false,
                Status = "NO_RECEIVABLES",
                Message = "Notifikasi diterima. Pegawai tidak memiliki piutang aktif di Finance."
            };
        }

        // 4. Pegawai berpiutang aktif: tandai kartu piutangnya (AC 1)
        var totalOutstanding = activeReceivables.Sum(x => x.OutstandingAmount);
        var now = DateTimeOffset.UtcNow;
        var businessDate = FinanceBusinessDate.ToDateOnly(now);

        foreach (var receivable in activeReceivables)
        {
            // Catat baris mutasi subledger penandaan (FIN-DES-079, FIN-VAL-165: delta = 0, saldo tetap seimbang)
            var movement = new FinReceivableMovement
            {
                Id = Guid.NewGuid(),
                ReceivableId = receivable.Id,
                MovementType = FinReceivableMovementTypes.SeparationNotice,
                Amount = 0.00m,
                BalanceBefore = receivable.OutstandingAmount,
                BalanceAfter = receivable.OutstandingAmount,
                BusinessDate = businessDate,
                OccurredAt = now,
                ReferenceNumber = $"SEP-{request.SeparationId}",
                Notes = $"[SEPARATION_PENDING] Notifikasi pemisahan pegawai {request.EmployeeId} diterima (SeparationId: {request.SeparationId}, Tanggal: {request.SeparationDate:yyyy-MM-dd}). Alasan: {request.Reason}. Sisa kewajiban Rp {receivable.OutstandingAmount:N2} dipetakan untuk hak akhir.",
                CorrelationId = receivable.CorrelationId != Guid.Empty ? receivable.CorrelationId : request.SeparationId,
                CausationId = request.SeparationId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = Guid.Empty
            };

            _dbContext.FinReceivableMovements.Add(movement);
            receivable.UpdateDateTime = DateTime.UtcNow;

            // Tandai perjanjian angsuran jika ada
            var activePlan = await _dbContext.FinReceivableInstallmentPlans
                .Where(p => p.ReceivableId == receivable.Id && !p.IsDelete
                    && (p.Status == FinReceivableInstallmentPlanStatuses.Menunggu || p.Status == FinReceivableInstallmentPlanStatuses.Disetujui))
                .FirstOrDefaultAsync(cancellationToken);

            if (activePlan is not null)
            {
                var separationNote = $"[SEPARATION_PENDING] Pegawai dalam proses pemisahan sejak {request.SeparationDate:yyyy-MM-dd} (SeparationId: {request.SeparationId}).";
                activePlan.Notes = string.IsNullOrWhiteSpace(activePlan.Notes)
                    ? separationNote
                    : $"{activePlan.Notes} | {separationNote}";
                activePlan.UpdateDateTime = DateTime.UtcNow;
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _loggerService.AuditAsync(LogCategory, "SeparationNotice.Flagged",
            $"Pemberitahuan pemisahan pegawai diproses untuk Pegawai {request.EmployeeId} (SeparationId: {request.SeparationId}, Tanggal: {request.SeparationDate:yyyy-MM-dd}). " +
            $"Berhasil menandai {activeReceivables.Count} kartu piutang aktif dengan total kewajiban Rp {totalOutstanding:N2}.",
            new
            {
                request.EmployeeId,
                request.SeparationId,
                request.SeparationDate,
                request.Reason,
                ActiveCount = activeReceivables.Count,
                TotalOutstanding = totalOutstanding
            });

        return new SeparationNoticeResponse
        {
            EmployeeId = request.EmployeeId,
            SeparationId = request.SeparationId,
            ActiveReceivableCount = activeReceivables.Count,
            FlaggedReceivableCount = activeReceivables.Count,
            TotalOutstandingAmount = totalOutstanding,
            IsIdempotentReplay = false,
            Status = "FLAGGED",
            Message = $"Notifikasi pemisahan diterima. Berhasil menandai {activeReceivables.Count} kartu piutang aktif total Rp {totalOutstanding:N2} untuk pemetaan hak akhir."
        };
    }
}
