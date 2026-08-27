using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingFinalizationService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly IBillingChargeSourceAdapter _chargeSourceAdapter;
    private readonly BillingArApHandoffService _arApHandoffService;
    private readonly LoggerService _loggerService;

    public BillingFinalizationService(
        ApplicationDbContext dbContext,
        IBillingChargeSourceAdapter chargeSourceAdapter,
        BillingArApHandoffService arApHandoffService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _chargeSourceAdapter = chargeSourceAdapter;
        _arApHandoffService = arApHandoffService;
        _loggerService = loggerService;
    }

    public async Task<FinalizationPreviewResponse> PreviewAsync(
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
        return await BuildReadinessAsync(invoice, cancellationToken);
    }

    public async Task<FinalizationResponse> FinalizeAsync(
        Guid invoiceId,
        FinalizeInvoiceRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateFinalizeRequest(request, actorUserId);
        var payloadHash = ComputeFinalizePayloadHash(invoiceId, request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_INVOICE_LEDGER_{invoiceId:N}", cancellationToken);
            }

            var prior = await _dbContext.BilFinalizationRecords
                .Include(x => x.Invoice)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingFinalizationConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditFinalizationAsync(prior, actorUserId, true);
                return Map(prior, true);
            }

            if (await _dbContext.BilFinalizationRecords.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingFinalizationConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var invoice = await _dbContext.BilInvoices
                .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingFinalizationConflictException(
                    "Invoice tidak lagi berstatus OPEN untuk difinalisasi.");
            if (invoice.RowVersion != request.ExpectedRowVersion)
                throw new BillingFinalizationConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");

            var readiness = await BuildReadinessAsync(invoice, cancellationToken);
            var isDepartureException = !string.IsNullOrWhiteSpace(request.DepartureReason);
            if (!readiness.AllOrdersComplete || !readiness.CalculationCurrent)
                throw new BillingFinalizationBlockedException(
                    "Invoice belum siap difinalisasi.", readiness);
            if (!isDepartureException && readiness.Outstanding > 0)
                throw new BillingFinalizationBlockedException(
                    "Tanggung jawab pasien belum lunas; ajukan write-off/adjustment atau catat departure exception untuk melanjutkan.",
                    readiness);

            var now = DateTimeOffset.UtcNow;
            var record = new BilFinalizationRecord
            {
                InvoiceId = invoice.Id,
                Invoice = invoice,
                CalculationVersion = readiness.CalculationVersion,
                OutstandingAtFinalization = readiness.Outstanding,
                IsDepartureException = isDepartureException,
                DepartureReason = isDepartureException
                    ? request.DepartureReason!.Trim().ToUpperInvariant()
                    : null,
                DebtorIdentity = isDepartureException ? request.DebtorIdentity!.Trim() : null,
                DebtorRelationship = isDepartureException ? request.DebtorRelationship!.Trim() : null,
                Reason = request.Reason.Trim(),
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                FinalizedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilFinalizationRecords.Add(record);
            invoice.Status = BillingInvoiceStatuses.Final;
            invoice.InvoiceDate ??= now;
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;

            var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                .SingleAsync(
                    x => x.InvoiceId == invoice.Id && x.VersionNo == readiness.CalculationVersion,
                    cancellationToken);
            await _arApHandoffService.StageHandoffsForFinalizationAsync(
                invoice, calculation, record, readiness.Outstanding, isDepartureException,
                actorUserId, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditFinalizationAsync(record, actorUserId, false);
            return Map(record, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingFinalizationConflictException(
                "Finalisasi tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
                exception);
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

    private async Task<FinalizationPreviewResponse> BuildReadinessAsync(
        BilInvoice invoice,
        CancellationToken cancellationToken)
    {
        if (invoice.CurrentCalculationVersion <= 0)
            return new FinalizationPreviewResponse
            {
                InvoiceId = invoice.Id,
                AllOrdersComplete = false,
                CalculationCurrent = false,
                Outstanding = 0,
                IsReadyForNormalFinalization = false,
                BlockingReasons = ["Invoice belum memiliki hasil perhitungan."],
                CalculationVersion = 0,
                InvoiceRowVersion = invoice.RowVersion
            };

        var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
            .SingleOrDefaultAsync(x => x.InvoiceId == invoice.Id
                && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete, cancellationToken)
            ?? throw new BillingFinalizationValidationException(
                "Invoice belum memiliki hasil perhitungan terkini.");

        var allOrdersComplete = await AreAllOrdersCompleteAsync(invoice.Id, cancellationToken);
        var calculationCurrent = await IsCalculationCurrentAsync(invoice.Id, calculation, cancellationToken);
        var outstanding = await CalculateOutstandingAsync(invoice, calculation, cancellationToken);

        var blockingReasons = new List<string>();
        if (!allOrdersComplete)
            blockingReasons.Add("Semua order harus selesai sebelum invoice difinalkan.");
        if (!calculationCurrent)
            blockingReasons.Add("Tagihan berubah; hitung ulang sebelum finalisasi.");
        if (outstanding > 0)
            blockingReasons.Add(
                "Tanggung jawab pasien belum lunas; ajukan write-off/adjustment atau catat departure exception untuk melanjutkan.");

        return new FinalizationPreviewResponse
        {
            InvoiceId = invoice.Id,
            AllOrdersComplete = allOrdersComplete,
            CalculationCurrent = calculationCurrent,
            Outstanding = outstanding,
            IsReadyForNormalFinalization = allOrdersComplete && calculationCurrent && outstanding == 0,
            BlockingReasons = blockingReasons,
            CalculationVersion = calculation.VersionNo,
            InvoiceRowVersion = invoice.RowVersion
        };
    }

    private async Task<bool> AreAllOrdersCompleteAsync(Guid invoiceId, CancellationToken cancellationToken)
    {
        var activeItems = await _dbContext.BilInvoiceItems.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId
                && x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete)
            .ToListAsync(cancellationToken);
        return activeItems.All(_chargeSourceAdapter.IsOrderComplete);
    }

    private async Task<bool> IsCalculationCurrentAsync(
        Guid invoiceId,
        BilCalculationVersion calculation,
        CancellationToken cancellationToken)
    {
        var latestItemChange = await _dbContext.BilInvoiceItems.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId)
            .Select(x => x.UpdateDateTime ?? x.CreateDateTime)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken);
        var latestDiscountChange = await _dbContext.BilDiscountApplications.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId)
            .Select(x => x.UpdateDateTime ?? x.CreateDateTime)
            .OrderByDescending(x => x)
            .FirstOrDefaultAsync(cancellationToken);
        var latestChange = latestItemChange > latestDiscountChange ? latestItemChange : latestDiscountChange;
        return latestChange <= calculation.CalculatedAt.UtcDateTime;
    }

    private async Task<decimal> CalculateOutstandingAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        CancellationToken cancellationToken)
    {
        var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice
                && x.TargetId == invoice.Id && !x.IsDelete)
            .SumAsync(
                x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount),
                cancellationToken) ?? 0;
        var allocationExcess = await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.SourceType == BillingRefundableCreditSourceTypes.AllocationExcess && !x.IsDelete)
            .SumAsync(x => (decimal?)x.AvailableAmount, cancellationToken) ?? 0;
        var writeOffTotal = await _dbContext.BilWriteOffCases.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.Status == BillingWriteOffCaseStatuses.Posted && !x.IsDelete)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        var adjustmentNet = await _dbContext.BilAdjustments.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.Status == BillingAdjustmentStatuses.Posted && !x.IsDelete)
            .SumAsync(
                x => (decimal?)(x.Direction == BillingAdjustmentDirections.Credit ? x.Amount : -x.Amount),
                cancellationToken) ?? 0;
        return Math.Max(
            calculation.PatientAmount - paidAmount + allocationExcess - writeOffTotal - adjustmentNet, 0);
    }

    private static void ValidateFinalizeRequest(FinalizeInvoiceRequest request, Guid actorUserId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (actorUserId == Guid.Empty)
            throw new BillingFinalizationValidationException("Identitas pengguna tidak valid.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingFinalizationValidationException("ExpectedRowVersion wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new BillingFinalizationValidationException(
                "Alasan finalisasi wajib diisi dan maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingFinalizationValidationException("CorrelationId dan CausationId wajib diisi.");

        if (string.IsNullOrWhiteSpace(request.DepartureReason)) return;
        var reason = request.DepartureReason.Trim().ToUpperInvariant();
        if (reason is not (BillingDepartureReasons.Death
            or BillingDepartureReasons.EmergencyTransfer
            or BillingDepartureReasons.Dama))
            throw new BillingFinalizationValidationException(
                "DepartureReason harus DEATH, EMERGENCY_TRANSFER, atau DAMA.");
        if (string.IsNullOrWhiteSpace(request.DebtorIdentity) || string.IsNullOrWhiteSpace(request.DebtorRelationship))
            throw new BillingFinalizationValidationException(
                "Pihak yang menanggung sisa tagihan harus dicatat.");
    }

    private static string ComputeFinalizePayloadHash(Guid invoiceId, FinalizeInvoiceRequest request)
    {
        var canonical = string.Join('|',
            invoiceId.ToString("N"),
            request.DepartureReason?.Trim().ToUpperInvariant() ?? string.Empty,
            request.DebtorIdentity?.Trim() ?? string.Empty,
            request.DebtorRelationship?.Trim() ?? string.Empty,
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditFinalizationAsync(BilFinalizationRecord record, Guid actorUserId, bool isReplay) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingFinalization.Create",
            "Invoice difinalisasi sebagai satu efek final per invoice.",
            new
            {
                FinalizationRecordId = record.Id,
                record.InvoiceId,
                record.CalculationVersion,
                record.OutstandingAtFinalization,
                record.IsDepartureException,
                record.DepartureReason,
                HasDebtorEvidence = !string.IsNullOrWhiteSpace(record.DebtorIdentity),
                record.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });

    private static FinalizationResponse Map(BilFinalizationRecord record, bool isReplay) => new()
    {
        Id = record.Id,
        InvoiceId = record.InvoiceId,
        CalculationVersion = record.CalculationVersion,
        OutstandingAtFinalization = record.OutstandingAtFinalization,
        IsDepartureException = record.IsDepartureException,
        DepartureReason = record.DepartureReason,
        InvoiceStatus = record.Invoice.Status,
        FinalizedAt = record.FinalizedAt,
        InvoiceRowVersion = record.Invoice.RowVersion,
        CorrelationId = record.CorrelationId,
        IsReplay = isReplay
    };
}

public abstract class BillingFinalizationException : Exception
{
    protected BillingFinalizationException(string message) : base(message) { }
    protected BillingFinalizationException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingFinalizationValidationException(string message)
    : BillingFinalizationException(message);

public sealed class BillingFinalizationConflictException : BillingFinalizationException
{
    public BillingFinalizationConflictException(string message) : base(message) { }
    public BillingFinalizationConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingFinalizationBlockedException : BillingFinalizationException
{
    public BillingFinalizationBlockedException(string message, FinalizationPreviewResponse checklist)
        : base(message)
    {
        Checklist = checklist;
    }

    public FinalizationPreviewResponse Checklist { get; }
}
