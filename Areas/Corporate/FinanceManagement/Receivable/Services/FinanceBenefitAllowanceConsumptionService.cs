using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Repositories;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Penjamin pemakaian plafon Benefit RS untuk piutang PAYER internal (IsInternalBenefit = true) yang plafonnya BELUM tercatat
/// terpakai. Menggantikan tanggung jawab plafon dari Penutupan Benefit (FinanceBenefitSettlementService.PostAsync, sudah dihapus).
///
/// Titik pemakaian plafon yang UTAMA adalah finalisasi Billing (BillingFinalizationService), atomik dengan pembentukan handoff.
/// Service ini hanya jaring pengaman untuk piutang historis yang difinalkan SEBELUM perubahan itu dan belum pernah masuk
/// Penutupan Benefit yang diterbitkan: dipanggil saat Invoice AR dibuat, sebelum dokumen terbit.
///
/// Idempoten dan tidak pernah memotong dua kali: kunci pemakaian = HandoffKey baris PAYER (FinReceivable.SourceHandoffKey, sama
/// dengan kunci yang dipakai finalisasi Billing dan Penutupan Benefit lama). Bila mutasi USE untuk kunci itu sudah ada (apa pun
/// penyebabnya, termasuk yang sudah dikembalikan), piutang dilewati. Pemakaian dicatat lewat EmployeeBenefitBalanceService
/// (validasi kelayakan, sisa plafon, kunci idempotensi, lock per pemilik, mutasi audit). Tidak mengubah saldo piutang, tidak
/// menulis buku mutasi Finance, dan tidak membuat penerimaan kas atau jurnal.
/// </summary>
public sealed class FinanceBenefitAllowanceConsumptionService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EmployeeBenefitBalanceService? _balanceService;

    public FinanceBenefitAllowanceConsumptionService(
        ApplicationDbContext dbContext,
        EmployeeBenefitBalanceService? balanceService = null)
    {
        _dbContext = dbContext;
        _balanceService = balanceService;
    }

    /// <summary>
    /// Memastikan plafon untuk piutang internal yang diberikan sudah tercatat terpakai (tepat satu kali). Bila belum ada transaksi
    /// pemanggil, service membuka transaksi serializable sendiri dan menyimpannya; bila ada, pemanggil yang menyimpan dan commit.
    /// </summary>
    public async Task EnsureConsumedAsync(
        IReadOnlyCollection<FinReceivable> receivables, Guid actorUserId, CancellationToken cancellationToken)
    {
        var targets = receivables
            .Where(x => x.DebtorType == FinReceivableDebtorTypes.Payer
                && x.IsInternalBenefit
                && x.Status != FinReceivableStatuses.Cancelled
                && x.OriginalAmount > 0m)
            .ToList();
        if (targets.Count == 0) return;

        if (!_dbContext.Database.IsRelational() || _dbContext.Database.CurrentTransaction is not null)
        {
            await ConsumeAsync(targets, actorUserId, cancellationToken);
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        try
        {
            await ConsumeAsync(targets, actorUserId, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private async Task ConsumeAsync(IReadOnlyList<FinReceivable> receivables, Guid actorUserId, CancellationToken cancellationToken)
    {
        foreach (var receivable in receivables)
        {
            var handoffKey = receivable.SourceHandoffKey ?? receivable.Id;

            // Sudah ada pemakaian untuk kunci ini (finalisasi Billing, atau Penutupan Benefit lama yang sudah diterbitkan):
            // tidak diulang. Pemakaian yang sudah dikembalikan juga tidak dipakai ulang di sini.
            var alreadyUsed = await _dbContext.Set<HrdBenefitBalanceMovement>().AsNoTracking()
                .AnyAsync(x => x.SourceHandoffKey == handoffKey && x.MovementType == BenefitMovementTypes.Use, cancellationToken);
            if (alreadyUsed) continue;

            if (_balanceService is null)
                throw new InvalidOperationException("Layanan plafon benefit tidak terdaftar; pemakaian plafon tidak dapat dipastikan.");

            var (personKind, personId) = await ResolveBenefitPersonAsync(receivable, cancellationToken);
            if (string.IsNullOrWhiteSpace(personKind) || !personId.HasValue)
            {
                throw new ReceivableInvoiceBatchValidationException(
                    $"Identitas kepesertaan benefit pegawai tidak lengkap untuk memastikan pemakaian plafon pada piutang {receivable.ReceivableNumber}.");
            }

            var result = await _balanceService.UseAsync(new BenefitBalanceUseRequest
            {
                SourceHandoffKey = handoffKey,
                InvoiceId = receivable.InvoiceId ?? Guid.Empty,
                BenefitPersonKind = personKind,
                BenefitPersonId = personId.Value,
                Amount = receivable.OriginalAmount,
                ActorUserId = actorUserId
            }, cancellationToken);

            if (string.Equals(result.Result, BenefitUseResults.Insufficient, StringComparison.OrdinalIgnoreCase))
            {
                throw new ReceivableInvoiceBatchValidationException(
                    result.Message ?? $"Sisa plafon tidak cukup untuk piutang {receivable.ReceivableNumber}. Sisa saat ini Rp {result.RemainingAmount:N0}.");
            }
            if (string.Equals(result.Result, BenefitUseResults.KeyConflict, StringComparison.OrdinalIgnoreCase))
            {
                throw new ReceivableInvoiceBatchConflictException(
                    result.Message ?? $"Konflik kunci serah terima saat memastikan pemakaian plafon untuk piutang {receivable.ReceivableNumber}.");
            }
            if (!string.Equals(result.Result, BenefitUseResults.Applied, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(result.Result, BenefitUseResults.AlreadyApplied, StringComparison.OrdinalIgnoreCase))
            {
                throw new ReceivableInvoiceBatchValidationException(
                    result.Message ?? $"Pemakaian plafon benefit untuk piutang {receivable.ReceivableNumber} tidak dapat dicatat: {result.Result}.");
            }
        }
    }

    /// <summary>
    /// Identitas kepesertaan benefit dari piutang: dari penjamin benefit pada kunjungan asal, lalu dari pemilik benefit pada piutang.
    /// (Dipindahkan dari FinanceBenefitSettlementService; langkah "mutasi USE yang sudah ada" ditangani pemeriksaan idempotensi di atas.)
    /// </summary>
    private async Task<(string? Kind, Guid? Id)> ResolveBenefitPersonAsync(FinReceivable receivable, CancellationToken cancellationToken)
    {
        if (_balanceService is null) return (null, null);

        if (receivable.InvoiceId.HasValue)
        {
            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .Where(x => x.Id == receivable.InvoiceId.Value)
                .Select(x => new { x.Id, x.EncounterId, x.InvoiceDate })
                .FirstOrDefaultAsync(cancellationToken);

            if (invoice is not null)
            {
                var guarantors = await _dbContext.RegPatientEncounterGuarantors.AsNoTracking()
                    .Include(x => x.CompanyGuarantor)
                    .Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete && !x.IsCancel)
                    .OrderByDescending(x => x.IsPrimary)
                    .ThenBy(x => x.Priority)
                    .ToListAsync(cancellationToken);

                var benefitGuarantor = guarantors.FirstOrDefault(x =>
                    (x.CompanyGuarantor != null && x.CompanyGuarantor.IsEmployeeBenefitGuarantor)
                    || x.EmployeeIdSnapshot.HasValue
                    || x.WorkforceProfileIdSnapshot.HasValue);

                if (benefitGuarantor is not null)
                {
                    var ownerProfileId = benefitGuarantor.WorkforceProfileIdSnapshot
                        ?? (benefitGuarantor.EmployeeIdSnapshot.HasValue
                            ? await _dbContext.MstEmployees.AsNoTracking()
                                .Where(x => x.Id == benefitGuarantor.EmployeeIdSnapshot.Value)
                                .Select(x => (Guid?)x.WorkforceProfileId)
                                .FirstOrDefaultAsync(cancellationToken)
                            : null);

                    var asOfDate = (invoice.InvoiceDate ?? receivable.RecognizedAt).UtcDateTime;
                    var balance = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
                    {
                        WorkforceProfileId = ownerProfileId,
                        EmployeeId = benefitGuarantor.EmployeeIdSnapshot ?? Guid.Empty,
                        FamilyMemberId = benefitGuarantor.FamilyMemberIdSnapshot,
                        AsOfDate = asOfDate
                    }, cancellationToken);

                    if (balance.IsUsable && !string.IsNullOrWhiteSpace(balance.BenefitPersonKind) && balance.BenefitPersonId.HasValue)
                        return (balance.BenefitPersonKind, balance.BenefitPersonId);
                }
            }
        }

        if (receivable.BenefitOwnerWorkforceProfileId.HasValue || receivable.BenefitOwnerId.HasValue)
        {
            var balance = await _balanceService.ReadAsync(new BenefitBalanceReadRequest
            {
                WorkforceProfileId = receivable.BenefitOwnerWorkforceProfileId,
                EmployeeId = receivable.BenefitOwnerId ?? Guid.Empty,
                AsOfDate = receivable.RecognizedAt.UtcDateTime
            }, cancellationToken);

            if (balance.IsUsable && !string.IsNullOrWhiteSpace(balance.BenefitPersonKind) && balance.BenefitPersonId.HasValue)
                return (balance.BenefitPersonKind, balance.BenefitPersonId);
        }

        return (null, null);
    }
}
