using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Dtos;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Billing hanya merekam dan mengekspos fakta handoff AR/AP secara idempotent; belum ada
/// konsumen AR/AP nyata di repository ini, sehingga tidak ada pengiriman aktif yang dibangun.
/// </summary>
public sealed class BillingArApHandoffService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public BillingArApHandoffService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Menambahkan entity handoff ke context yang sama tanpa membuka transaksi sendiri;
    /// pemanggil (BillingFinalizationService) yang memegang batas transaksi dan SaveChanges.
    /// </summary>
    public async Task StageHandoffsForFinalizationAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        BilFinalizationRecord finalizationRecord,
        decimal outstandingAtFinalization,
        bool isDepartureException,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        if (isDepartureException && outstandingAtFinalization > 0)
        {
            _dbContext.BilArHandoffs.Add(new BilArHandoff
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                FinalizationRecordId = finalizationRecord.Id,
                FinalizationRecord = finalizationRecord,
                DebtorType = BillingArDebtorTypes.PatientGuarantor,
                Amount = outstandingAtFinalization,
                DueDate = invoice.InvoiceDate,
                Status = BillingHandoffStatuses.Created,
                HandoffKey = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                CausationId = finalizationRecord.CorrelationId,
                CreatedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }

        var payerAmount = calculation.PrimaryAmount + calculation.ExcessAmount;
        if (payerAmount > 0)
        {
            var guarantor = await _dbContext.TrxPatientEncounterGuarantors.AsNoTracking()
                .Where(x => x.EncounterId == invoice.EncounterId && x.IsActive && !x.IsDelete)
                .FirstOrDefaultAsync(cancellationToken);
            _dbContext.BilArHandoffs.Add(new BilArHandoff
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                FinalizationRecordId = finalizationRecord.Id,
                FinalizationRecord = finalizationRecord,
                DebtorType = BillingArDebtorTypes.Payer,
                DebtorReferenceId = guarantor?.InsuranceProviderId,
                Amount = payerAmount,
                Status = BillingHandoffStatuses.Created,
                HandoffKey = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                CausationId = finalizationRecord.CorrelationId,
                CreatedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            });
        }

        var doctorId = await _dbContext.TrxPatientEncounters.AsNoTracking()
            .Where(x => x.Id == invoice.EncounterId)
            .Select(x => x.DoctorId)
            .FirstOrDefaultAsync(cancellationToken);
        if (doctorId.HasValue)
        {
            var grossDoctorShare = await _dbContext.BilInvoiceItems.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id
                    && x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
                .SumAsync(x => (decimal?)x.DoctorShare, cancellationToken) ?? 0;
            var doctorDiscountTotal = await _dbContext.BilDiscountApplications.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id && x.DiscountType == DiscountPolicyValues.Doctor
                    && x.ApprovalStatus == BillingDiscountApprovalStatuses.Approved && !x.IsDelete)
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
            var apAmount = Math.Max(grossDoctorShare - doctorDiscountTotal, 0);
            if (apAmount > 0)
            {
                var isReady = !isDepartureException && payerAmount == 0;
                _dbContext.BilApHandoffs.Add(new BilApHandoff
                {
                    InvoiceId = invoice.Id,
                    Invoice = invoice,
                    FinalizationRecordId = finalizationRecord.Id,
                    FinalizationRecord = finalizationRecord,
                    DoctorId = doctorId.Value,
                    Amount = apAmount,
                    ReadinessStatus = isReady
                        ? BillingApReadinessStatuses.Ready
                        : BillingApReadinessStatuses.NotReady,
                    Status = BillingHandoffStatuses.Created,
                    HandoffKey = Guid.NewGuid(),
                    CorrelationId = Guid.NewGuid(),
                    CausationId = finalizationRecord.CorrelationId,
                    CreatedAt = now,
                    ReadyAt = isReady ? now : null,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                });
            }
        }
    }

    /// <summary>
    /// Dipanggil setelah adjustment/write-off diposting; hanya membuat correction bila invoice
    /// sudah FINAL dan memiliki AR handoff yang ada. Idempotent per source (at-least-once safe).
    /// </summary>
    public async Task RecordCorrectionIfLinkedAsync(
        Guid invoiceId,
        Guid? sourceAdjustmentId,
        Guid? sourceWriteOffCaseId,
        string direction,
        decimal amount,
        string reason,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken);
        if (invoice is null || invoice.Status != BillingInvoiceStatuses.Final) return;

        var arHandoff = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .OrderBy(x => x.DebtorType == BillingArDebtorTypes.PatientGuarantor ? 0 : 1)
            .FirstOrDefaultAsync(cancellationToken);
        if (arHandoff is null) return;

        var sourceKey = sourceAdjustmentId ?? sourceWriteOffCaseId;
        if (!sourceKey.HasValue) return;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_HANDOFF_CORRECTION_{sourceKey.Value:N}", cancellationToken);
            }

            var exists = await _dbContext.BilHandoffAdjustments.AsNoTracking().AnyAsync(
                x => (sourceAdjustmentId.HasValue && x.SourceAdjustmentId == sourceAdjustmentId)
                    || (sourceWriteOffCaseId.HasValue && x.SourceWriteOffCaseId == sourceWriteOffCaseId),
                cancellationToken);
            if (exists)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return;
            }

            var now = DateTimeOffset.UtcNow;
            var correction = new BilHandoffAdjustment
            {
                ArHandoffId = arHandoff.Id,
                SourceAdjustmentId = sourceAdjustmentId,
                SourceWriteOffCaseId = sourceWriteOffCaseId,
                Direction = direction,
                Amount = amount,
                Reason = reason,
                CorrelationId = Guid.NewGuid(),
                CausationId = sourceKey.Value,
                CreatedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilHandoffAdjustments.Add(correction);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditCorrectionAsync(correction, actorUserId);
        }
        catch (DbUpdateException)
        {
            // Correction sudah tercatat oleh percobaan lain (at-least-once); aman diabaikan.
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
        }
        catch
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<HandoffStatusResponse> GetHandoffStatusAsync(
        Guid finalizationRecordId,
        CancellationToken cancellationToken)
    {
        var record = await _dbContext.BilFinalizationRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == finalizationRecordId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Finalization record tidak ditemukan.");

        var arHandoffs = await _dbContext.BilArHandoffs.AsNoTracking()
            .Where(x => x.FinalizationRecordId == finalizationRecordId && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var apHandoffs = await _dbContext.BilApHandoffs.AsNoTracking()
            .Where(x => x.FinalizationRecordId == finalizationRecordId && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var arIds = arHandoffs.Select(x => x.Id).ToHashSet();
        var apIds = apHandoffs.Select(x => x.Id).ToHashSet();
        var adjustments = await _dbContext.BilHandoffAdjustments.AsNoTracking()
            .Where(x => !x.IsDelete)
            .ToListAsync(cancellationToken);
        var linkedAdjustments = adjustments.Where(x =>
            (x.ArHandoffId.HasValue && arIds.Contains(x.ArHandoffId.Value))
            || (x.ApHandoffId.HasValue && apIds.Contains(x.ApHandoffId.Value)))
            .ToList();

        return new HandoffStatusResponse
        {
            InvoiceId = record.InvoiceId,
            FinalizationRecordId = record.Id,
            ArHandoffs = arHandoffs.Select(MapAr).ToList(),
            ApHandoffs = apHandoffs.Select(MapAp).ToList(),
            Adjustments = linkedAdjustments.Select(MapAdjustment).ToList()
        };
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditCorrectionAsync(BilHandoffAdjustment correction, Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingHandoff.Correction",
            "Koreksi post-final ditautkan ke handoff AR/AP tanpa memutasi posting asal.",
            new
            {
                HandoffAdjustmentId = correction.Id,
                correction.ArHandoffId,
                correction.ApHandoffId,
                correction.SourceAdjustmentId,
                correction.SourceWriteOffCaseId,
                correction.Direction,
                correction.Amount,
                correction.CorrelationId,
                ActorUserId = actorUserId
            });

    private static ArHandoffResponse MapAr(BilArHandoff handoff) => new()
    {
        Id = handoff.Id,
        DebtorType = handoff.DebtorType,
        DebtorReferenceId = handoff.DebtorReferenceId,
        Amount = handoff.Amount,
        DueDate = handoff.DueDate,
        Status = handoff.Status,
        CreatedAt = handoff.CreatedAt
    };

    private static ApHandoffResponse MapAp(BilApHandoff handoff) => new()
    {
        Id = handoff.Id,
        DoctorId = handoff.DoctorId,
        Amount = handoff.Amount,
        ReadinessStatus = handoff.ReadinessStatus,
        Status = handoff.Status,
        CreatedAt = handoff.CreatedAt,
        ReadyAt = handoff.ReadyAt
    };

    private static HandoffAdjustmentResponse MapAdjustment(BilHandoffAdjustment adjustment) => new()
    {
        Id = adjustment.Id,
        Direction = adjustment.Direction,
        Amount = adjustment.Amount,
        Reason = adjustment.Reason,
        CreatedAt = adjustment.CreatedAt
    };
}
