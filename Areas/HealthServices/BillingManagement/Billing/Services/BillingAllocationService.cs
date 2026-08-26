using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingAllocationService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public BillingAllocationService(
        ApplicationDbContext dbContext,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<AllocationResponse> AllocateDepositAsync(
        Guid encounterId,
        DepositAllocationRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateRequest(encounterId, request, idempotencyKey);
        var payloadHash = ComputePayloadHash(encounterId, request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_CALCULATION_{request.InvoiceId:N}", cancellationToken);
                await AcquireLockAsync($"BIL_DEPOSIT_{encounterId:N}", cancellationToken);
            }

            var prior = await _dbContext.BilSettlements
                .Include(x => x.Allocations)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash
                    || prior.Purpose != BillingSettlementPurposes.InvoicePayment
                    || prior.InvoiceId != request.InvoiceId)
                    throw new BillingAllocationConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");

                var priorAllocation = prior.Allocations.SingleOrDefault(
                    x => !x.IsDelete && x.ReversesAllocationId is null)
                    ?? throw new BillingAllocationConflictException(
                        "Hasil idempotent tidak memiliki allocation yang valid.");
                var replayAccount = await _dbContext.BilDepositAccounts.AsNoTracking()
                    .SingleOrDefaultAsync(
                        x => x.EncounterId == encounterId && !x.IsDelete,
                        cancellationToken)
                    ?? throw new BillingAllocationConflictException(
                        "Hasil idempotent tidak memiliki account deposit yang valid.");
                var replayInvoice = await LoadInvoiceAsync(request.InvoiceId, cancellationToken);
                var replayCalculation = await LoadCurrentCalculationAsync(replayInvoice, cancellationToken);
                var replayPosition = await CalculateInvoicePositionAsync(
                    replayInvoice.Id, replayCalculation.PatientAmount, cancellationToken);

                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditAllocationAsync(
                    priorAllocation, replayAccount, replayInvoice, replayPosition,
                    actorUserId, request.Reason, prior.CorrelationId, true);
                return MapResponse(
                    priorAllocation, prior, replayAccount, replayInvoice,
                    replayPosition, true);
            }

            var keyUsedByMovement = await _dbContext.BilDepositMovements.AsNoTracking()
                .AnyAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (keyUsedByMovement)
                throw new BillingAllocationConflictException(
                    "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
            var correlationUsed = await _dbContext.BilSettlements.AsNoTracking()
                    .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken)
                || await _dbContext.BilDepositMovements.AsNoTracking()
                    .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken);
            if (correlationUsed)
                throw new BillingAllocationConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var account = await _dbContext.BilDepositAccounts
                .Include(x => x.Movements)
                .SingleOrDefaultAsync(
                    x => x.EncounterId == encounterId && !x.IsDelete,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Deposit rawat inap belum tersedia.");
            if (account.Status != BillingDepositAccountStatuses.Active)
                throw new BillingAllocationValidationException("Account deposit sudah ditutup.");
            if (account.RowVersion != request.ExpectedDepositRowVersion)
                throw new BillingAllocationConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");

            var invoice = await LoadInvoiceAsync(request.InvoiceId, cancellationToken);
            if (invoice.EncounterId != encounterId)
                throw new BillingAllocationValidationException(
                    "Deposit hanya dapat dialokasikan ke invoice pada encounter yang sama.");
            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingAllocationValidationException(
                    "Hanya invoice OPEN yang dapat menerima allocation.");
            if (invoice.RowVersion != request.ExpectedInvoiceRowVersion
                || invoice.CurrentCalculationVersion != request.ExpectedCalculationVersion)
                throw new BillingAllocationConflictException(
                    "Data telah berubah. Muat ulang sebelum melanjutkan.");

            var calculation = await LoadCurrentCalculationAsync(invoice, cancellationToken);
            var positionBefore = await CalculateInvoicePositionAsync(
                invoice.Id, calculation.PatientAmount, cancellationToken);
            if (request.Amount > account.AvailableBalance
                || request.Amount > positionBefore.OutstandingAmount)
                throw new BillingAllocationValidationException(
                    "Dana deposit atau saldo tagihan tidak mencukupi.");

            var now = DateTimeOffset.UtcNow;
            var beforeBalance = account.AvailableBalance;
            var settlement = new BilSettlement
            {
                InvoiceId = invoice.Id,
                Purpose = BillingSettlementPurposes.InvoicePayment,
                RequestedAmount = request.Amount,
                SuccessfulAmount = request.Amount,
                AllocatedAmount = request.Amount,
                Status = BillingSettlementStatuses.Settled,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                StartedAt = now,
                CompletedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            var allocation = new BilPaymentAllocation
            {
                SettlementId = settlement.Id,
                Settlement = settlement,
                TargetType = BillingAllocationTargetTypes.Invoice,
                TargetId = invoice.Id,
                Amount = request.Amount,
                CalculationVersion = calculation.VersionNo,
                AllocatedAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            settlement.Allocations.Add(allocation);
            var movement = new BilDepositMovement
            {
                DepositAccountId = account.Id,
                DepositAccount = account,
                MovementType = BillingDepositMovementTypes.Allocation,
                Amount = request.Amount,
                SettlementId = settlement.Id,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                OccurredAt = now,
                Reason = request.Reason.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };

            _dbContext.BilSettlements.Add(settlement);
            _dbContext.BilPaymentAllocations.Add(allocation);
            account.Movements.Add(movement);
            _dbContext.BilDepositMovements.Add(movement);
            account.AvailableBalance = beforeBalance - request.Amount;
            account.RowVersion = Guid.NewGuid();
            account.UpdateDateTime = DateTime.UtcNow;
            account.UpdateBy = actorUserId;
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            var positionAfter = positionBefore with
            {
                NetAllocatedAmount = positionBefore.NetAllocatedAmount + request.Amount,
                OutstandingAmount = Math.Max(positionBefore.OutstandingAmount - request.Amount, 0)
            };
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditAllocationAsync(
                allocation, account, invoice, positionAfter,
                actorUserId, request.Reason, request.CorrelationId, false, beforeBalance);
            return MapResponse(
                allocation, settlement, account, invoice, positionAfter, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingAllocationConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingAllocationConflictException(
                "Allocation tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
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

    internal async Task<SettlementAllocationResult> ReconcileSettlementAsync(
        BilSettlement settlement,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        if (settlement.Purpose != BillingSettlementPurposes.InvoicePayment
            || !settlement.InvoiceId.HasValue)
            return SettlementAllocationResult.None;

        var invoice = await LoadInvoiceAsync(settlement.InvoiceId.Value, cancellationToken);
        var calculation = await LoadCurrentCalculationAsync(invoice, cancellationToken);
        var allocations = await _dbContext.BilPaymentAllocations
            .Where(x => x.SettlementId == settlement.Id && !x.IsDelete)
            .OrderByDescending(x => x.AllocatedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
        var settlementCredit = await _dbContext.BilRefundableCredits
            .SingleOrDefaultAsync(
                x => x.SourceType == BillingRefundableCreditSourceTypes.Settlement
                    && x.SourceId == settlement.Id
                    && !x.IsDelete,
                cancellationToken);
        var activeAllocated = NetAmount(allocations);
        var availableCredit = settlementCredit?.AvailableAmount ?? 0;
        var accounted = activeAllocated + availableCredit;
        var allocatedDelta = 0m;
        var creditDelta = 0m;

        if (settlement.SuccessfulAmount > accounted)
        {
            var newFunds = settlement.SuccessfulAmount - accounted;
            var position = await CalculateInvoicePositionAsync(
                invoice.Id, calculation.PatientAmount, cancellationToken);
            allocatedDelta = Math.Min(newFunds, position.OutstandingAmount);
            if (allocatedDelta > 0)
            {
                var allocation = new BilPaymentAllocation
                {
                    SettlementId = settlement.Id,
                    Settlement = settlement,
                    TargetType = BillingAllocationTargetTypes.Invoice,
                    TargetId = invoice.Id,
                    Amount = allocatedDelta,
                    CalculationVersion = calculation.VersionNo,
                    AllocatedAt = occurredAt,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                settlement.Allocations.Add(allocation);
                _dbContext.BilPaymentAllocations.Add(allocation);
                activeAllocated += allocatedDelta;
            }

            var refundable = newFunds - allocatedDelta;
            if (refundable > 0)
            {
                creditDelta = refundable;
                if (settlementCredit is null)
                {
                    settlementCredit = NewCredit(
                        invoice.Id,
                        BillingRefundableCreditSourceTypes.Settlement,
                        settlement.Id,
                        refundable,
                        occurredAt,
                        actorUserId);
                    _dbContext.BilRefundableCredits.Add(settlementCredit);
                }
                else
                {
                    settlementCredit.OriginalAmount += refundable;
                    settlementCredit.AvailableAmount += refundable;
                    settlementCredit.Status = BillingRefundableCreditStatuses.Available;
                    Touch(settlementCredit, actorUserId);
                }
            }
        }
        else if (settlement.SuccessfulAmount < accounted)
        {
            var reduction = accounted - settlement.SuccessfulAmount;
            if (settlementCredit is not null && settlementCredit.AvailableAmount > 0)
            {
                var creditReduction = Math.Min(reduction, settlementCredit.AvailableAmount);
                settlementCredit.AvailableAmount -= creditReduction;
                settlementCredit.Status = settlementCredit.AvailableAmount == 0
                    ? BillingRefundableCreditStatuses.Exhausted
                    : BillingRefundableCreditStatuses.Available;
                Touch(settlementCredit, actorUserId);
                creditDelta = -creditReduction;
                reduction -= creditReduction;
            }

            if (reduction > 0)
            {
                var reversedIds = allocations
                    .Where(x => x.ReversesAllocationId.HasValue)
                    .Select(x => x.ReversesAllocationId!.Value)
                    .ToHashSet();
                foreach (var original in allocations.Where(
                    x => !x.ReversesAllocationId.HasValue && !reversedIds.Contains(x.Id)))
                {
                    if (reduction <= 0) break;
                    var amount = Math.Min(original.Amount, reduction);
                    var reversal = new BilPaymentAllocation
                    {
                        SettlementId = settlement.Id,
                        Settlement = settlement,
                        TargetType = original.TargetType,
                        TargetId = original.TargetId,
                        Amount = amount,
                        CalculationVersion = calculation.VersionNo,
                        AllocatedAt = occurredAt,
                        ReversesAllocationId = original.Id,
                        CreateDateTime = DateTime.UtcNow,
                        CreateBy = actorUserId
                    };
                    settlement.Allocations.Add(reversal);
                    _dbContext.BilPaymentAllocations.Add(reversal);
                    activeAllocated -= amount;
                    allocatedDelta -= amount;
                    reduction -= amount;
                }
            }
        }

        settlement.AllocatedAmount = Math.Max(activeAllocated, 0);
        if (allocatedDelta != 0 || creditDelta != 0)
        {
            invoice.RowVersion = Guid.NewGuid();
            invoice.UpdateDateTime = DateTime.UtcNow;
            invoice.UpdateBy = actorUserId;
        }
        return new SettlementAllocationResult(
            invoice.Id, allocatedDelta, creditDelta, settlement.AllocatedAmount,
            settlement.CorrelationId);
    }

    internal async Task<decimal> ReconcileCalculationExcessAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        Guid actorUserId,
        DateTimeOffset recognizedAt,
        CancellationToken cancellationToken)
    {
        var allocations = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice
                && x.TargetId == invoice.Id && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var netAllocated = NetAmount(allocations);
        var credits = await _dbContext.BilRefundableCredits
            .Where(x => x.InvoiceId == invoice.Id
                && x.SourceType == BillingRefundableCreditSourceTypes.AllocationExcess
                && !x.IsDelete)
            .OrderByDescending(x => x.RecognizedAt)
            .ThenByDescending(x => x.Id)
            .ToListAsync(cancellationToken);
        var desiredAvailable = Math.Max(netAllocated - calculation.PatientAmount, 0);
        var currentAvailable = credits.Sum(x => x.AvailableAmount);

        if (desiredAvailable > currentAvailable)
        {
            var amount = desiredAvailable - currentAvailable;
            var credit = NewCredit(
                invoice.Id,
                BillingRefundableCreditSourceTypes.AllocationExcess,
                calculation.Id,
                amount,
                recognizedAt,
                actorUserId);
            _dbContext.BilRefundableCredits.Add(credit);
        }
        else if (desiredAvailable < currentAvailable)
        {
            var reduction = currentAvailable - desiredAvailable;
            foreach (var credit in credits.Where(x => x.AvailableAmount > 0))
            {
                if (reduction <= 0) break;
                var amount = Math.Min(credit.AvailableAmount, reduction);
                credit.AvailableAmount -= amount;
                credit.Status = credit.AvailableAmount == 0
                    ? BillingRefundableCreditStatuses.Exhausted
                    : BillingRefundableCreditStatuses.Available;
                Touch(credit, actorUserId);
                reduction -= amount;
            }
        }

        return desiredAvailable;
    }

    internal Task AuditSettlementAllocationAsync(
        SettlementAllocationResult result,
        Guid actorUserId) =>
        result.InvoiceId == Guid.Empty
            ? Task.CompletedTask
            : _loggerService.AuditAsync(
                LogCategory,
                "BillingAllocation.SettlementReconciled",
                "Dana settlement sukses direkonsiliasi ke invoice dan refundable credit.",
                new
                {
                    result.InvoiceId,
                    result.AllocatedDelta,
                    result.CreditDelta,
                    result.TotalAllocated,
                    result.CorrelationId,
                    ActorUserId = actorUserId
                });

    private async Task<InvoiceFundsPosition> CalculateInvoicePositionAsync(
        Guid invoiceId,
        decimal patientAmount,
        CancellationToken cancellationToken)
    {
        var allocations = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice
                && x.TargetId == invoiceId && !x.IsDelete)
            .ToListAsync(cancellationToken);
        var netAllocated = NetAmount(allocations);
        var allocationExcess = await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId
                && x.SourceType == BillingRefundableCreditSourceTypes.AllocationExcess
                && !x.IsDelete)
            .SumAsync(x => (decimal?)x.AvailableAmount, cancellationToken) ?? 0;
        var refundable = await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId && !x.IsDelete)
            .SumAsync(x => (decimal?)x.AvailableAmount, cancellationToken) ?? 0;
        var writeOffTotal = await _dbContext.BilWriteOffCases.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId
                && x.Status == BillingWriteOffCaseStatuses.Posted && !x.IsDelete)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        var adjustmentNet = await _dbContext.BilAdjustments.AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId
                && x.Status == BillingAdjustmentStatuses.Posted && !x.IsDelete)
            .SumAsync(
                x => (decimal?)(x.Direction == BillingAdjustmentDirections.Credit ? x.Amount : -x.Amount),
                cancellationToken) ?? 0;
        return new InvoiceFundsPosition(
            netAllocated,
            Math.Max(patientAmount - netAllocated + allocationExcess - writeOffTotal - adjustmentNet, 0),
            refundable);
    }

    private async Task<BilInvoice> LoadInvoiceAsync(
        Guid invoiceId,
        CancellationToken cancellationToken) =>
        await _dbContext.BilInvoices.SingleOrDefaultAsync(
            x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
        ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

    private async Task<BilCalculationVersion> LoadCurrentCalculationAsync(
        BilInvoice invoice,
        CancellationToken cancellationToken) =>
        await _dbContext.BilCalculationVersions.AsNoTracking().SingleOrDefaultAsync(
            x => x.InvoiceId == invoice.Id
                && x.VersionNo == invoice.CurrentCalculationVersion
                && !x.IsDelete,
            cancellationToken)
        ?? throw new BillingAllocationValidationException(
            "Invoice belum memiliki hasil perhitungan terkini.");

    private static decimal NetAmount(IEnumerable<BilPaymentAllocation> allocations) =>
        allocations.Sum(x => x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount);

    private static BilRefundableCredit NewCredit(
        Guid invoiceId,
        string sourceType,
        Guid sourceId,
        decimal amount,
        DateTimeOffset recognizedAt,
        Guid actorUserId) => new()
        {
            InvoiceId = invoiceId,
            SourceType = sourceType,
            SourceId = sourceId,
            OriginalAmount = amount,
            AvailableAmount = amount,
            Status = BillingRefundableCreditStatuses.Available,
            RecognizedAt = recognizedAt,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

    private static void Touch(BilRefundableCredit credit, Guid actorUserId)
    {
        credit.UpdateDateTime = DateTime.UtcNow;
        credit.UpdateBy = actorUserId;
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditAllocationAsync(
        BilPaymentAllocation allocation,
        BilDepositAccount account,
        BilInvoice invoice,
        InvoiceFundsPosition position,
        Guid actorUserId,
        string reason,
        Guid correlationId,
        bool isReplay,
        decimal? balanceBefore = null) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingAllocation.DepositAllocated",
            "Dana deposit dialokasikan ke running invoice tanpa menutup invoice.",
            new
            {
                AllocationId = allocation.Id,
                allocation.SettlementId,
                InvoiceId = invoice.Id,
                allocation.Amount,
                allocation.CalculationVersion,
                DepositBalanceBefore = balanceBefore ?? account.AvailableBalance + allocation.Amount,
                DepositBalanceAfter = account.AvailableBalance,
                position.OutstandingAmount,
                InvoiceStatus = invoice.Status,
                CorrelationId = correlationId,
                ActorUserId = actorUserId,
                Reason = reason.Trim(),
                IsReplay = isReplay
            });

    private static AllocationResponse MapResponse(
        BilPaymentAllocation allocation,
        BilSettlement settlement,
        BilDepositAccount account,
        BilInvoice invoice,
        InvoiceFundsPosition position,
        bool isReplay) => new()
        {
            Id = allocation.Id,
            SettlementId = allocation.SettlementId,
            InvoiceId = invoice.Id,
            TargetType = allocation.TargetType,
            Amount = allocation.Amount,
            CalculationVersion = allocation.CalculationVersion ?? invoice.CurrentCalculationVersion,
            AllocatedAt = allocation.AllocatedAt,
            DepositBalance = account.AvailableBalance,
            InvoiceOutstanding = position.OutstandingAmount,
            RefundableCredit = position.RefundableCredit,
            DepositRowVersion = account.RowVersion,
            InvoiceRowVersion = invoice.RowVersion,
            CorrelationId = settlement.CorrelationId,
            IsReplay = isReplay
        };

    private static void ValidateRequest(
        Guid encounterId,
        DepositAllocationRequest request,
        Guid idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (encounterId == Guid.Empty || request.InvoiceId == Guid.Empty)
            throw new BillingAllocationValidationException(
                "EncounterId dan InvoiceId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingAllocationValidationException("Idempotency-Key wajib diisi.");
        if (request.ExpectedDepositRowVersion == Guid.Empty
            || request.ExpectedInvoiceRowVersion == Guid.Empty
            || request.ExpectedCalculationVersion <= 0)
            throw new BillingAllocationValidationException(
                "Version token deposit, invoice, dan kalkulasi wajib diisi.");
        if (request.Amount <= 0
            || request.Amount > MaxMoneyAmount
            || decimal.Round(request.Amount, 2) != request.Amount)
            throw new BillingAllocationValidationException(
                "Nominal allocation harus positif dan maksimal memiliki dua angka desimal.");
        if (string.IsNullOrWhiteSpace(request.Reason) || request.Reason.Trim().Length > 500)
            throw new BillingAllocationValidationException(
                "Alasan allocation wajib diisi dan maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingAllocationValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static string ComputePayloadHash(
        Guid encounterId,
        DepositAllocationRequest request)
    {
        var canonical = string.Join('|',
            encounterId.ToString("N"),
            request.InvoiceId.ToString("N"),
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.ExpectedDepositRowVersion.ToString("N"),
            request.ExpectedInvoiceRowVersion.ToString("N"),
            request.ExpectedCalculationVersion.ToString(CultureInfo.InvariantCulture),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private readonly record struct InvoiceFundsPosition(
        decimal NetAllocatedAmount,
        decimal OutstandingAmount,
        decimal RefundableCredit);
}

public readonly record struct SettlementAllocationResult(
    Guid InvoiceId,
    decimal AllocatedDelta,
    decimal CreditDelta,
    decimal TotalAllocated,
    Guid CorrelationId)
{
    public static SettlementAllocationResult None => new(Guid.Empty, 0, 0, 0, Guid.Empty);
}

public abstract class BillingAllocationException : Exception
{
    protected BillingAllocationException(string message) : base(message) { }
    protected BillingAllocationException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingAllocationValidationException(string message)
    : BillingAllocationException(message);

public sealed class BillingAllocationConflictException : BillingAllocationException
{
    public BillingAllocationConflictException(string message) : base(message) { }
    public BillingAllocationConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
