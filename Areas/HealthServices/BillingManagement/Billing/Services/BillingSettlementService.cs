using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingSettlementService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private readonly ApplicationDbContext _dbContext;
    private readonly IBillingPaymentProviderAdapter _providerAdapter;
    private readonly BillingAllocationService _allocationService;
    private readonly CashierShiftService _cashierShiftService;
    private readonly LoggerService _loggerService;

    public BillingSettlementService(
        ApplicationDbContext dbContext,
        IBillingPaymentProviderAdapter providerAdapter,
        BillingAllocationService allocationService,
        CashierShiftService cashierShiftService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _providerAdapter = providerAdapter;
        _allocationService = allocationService;
        _cashierShiftService = cashierShiftService;
        _loggerService = loggerService;
    }

    public async Task<SettlementResponse> GetAsync(
        Guid settlementId,
        CancellationToken cancellationToken)
    {
        if (settlementId == Guid.Empty)
            throw new BillingSettlementValidationException("SettlementId wajib diisi.");

        var settlement = await _dbContext.BilSettlements.AsNoTracking()
            .Include(x => x.Tenders)
            .SingleOrDefaultAsync(x => x.Id == settlementId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Settlement tidak ditemukan.");
        return MapSettlement(settlement, false);
    }

    public async Task<SettlementResponse> CreateAsync(
        CreateSettlementRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateCreateRequest(request, idempotencyKey);
        var payloadHash = ComputeSettlementPayloadHash(request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync(TargetLockKey(request), cancellationToken);
            }

            var prior = await _dbContext.BilSettlements
                .Include(x => x.Tenders)
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingSettlementConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");

                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditSettlementAsync(prior, actorUserId, true);
                return MapSettlement(prior, true);
            }

            if (await _dbContext.BilSettlements.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingSettlementConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            await ValidateTargetAndAmountAsync(request, cancellationToken);
            var activeExists = await _dbContext.BilSettlements.AsNoTracking()
                .AnyAsync(x => !x.IsDelete
                    && x.Purpose == request.Purpose
                    && x.InvoiceId == request.InvoiceId
                    && x.DepositAccountId == request.DepositAccountId
                    && x.Status != BillingSettlementStatuses.Settled,
                    cancellationToken);
            if (activeExists)
                throw new BillingSettlementConflictException(
                    "Target masih memiliki settlement aktif; lanjutkan settlement yang sudah ada.");

            var now = DateTimeOffset.UtcNow;
            var settlement = new BilSettlement
            {
                InvoiceId = request.InvoiceId,
                DepositAccountId = request.DepositAccountId,
                Purpose = request.Purpose,
                RequestedAmount = request.RequestedAmount,
                SuccessfulAmount = 0,
                AllocatedAmount = 0,
                Status = BillingSettlementStatuses.Draft,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                StartedAt = now,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.BilSettlements.Add(settlement);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditSettlementAsync(settlement, actorUserId, false);
            return MapSettlement(settlement, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingSettlementConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingSettlementConflictException(
                "Settlement tidak dapat disimpan karena target, correlation, atau idempotency key sudah diproses.",
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

    public async Task<TenderResponse> AddTenderAsync(
        Guid settlementId,
        CreateTenderRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateTenderRequest(settlementId, request, idempotencyKey);
        var payloadHash = ComputeTenderPayloadHash(settlementId, request);
        BilTender tender;
        MstPaymentMethod paymentMethod;
        var isReplay = false;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_SETTLEMENT_{settlementId:N}", cancellationToken);
            }

            var settlement = await _dbContext.BilSettlements
                .Include(x => x.Tenders)
                .SingleOrDefaultAsync(x => x.Id == settlementId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Settlement tidak ditemukan.");
            var prior = settlement.Tenders.SingleOrDefault(
                x => x.IdempotencyKey == idempotencyKey && !x.IsDelete);
            if (prior is not null)
            {
                if (prior.PayloadHash != payloadHash)
                    throw new BillingSettlementConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");
                if (prior.Status != BillingTenderStatuses.Created)
                {
                    if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                    return MapTender(prior, true);
                }

                tender = prior;
                paymentMethod = await LoadPaymentMethodAsync(prior.PaymentMethodId, cancellationToken);
                if (paymentMethod.IsCash)
                {
                    var activeShift = await RequireCashierShiftAsync(actorUserId, cancellationToken);
                    if (tender.CashierShiftId.HasValue && tender.CashierShiftId != activeShift.Id)
                        throw new BillingSettlementConflictException(
                            "Tender tunai sudah terikat ke shift kasir lain.");
                    tender.CashierShiftId = activeShift.Id;
                }
                isReplay = true;
            }
            else
            {
                if (settlement.RowVersion != request.ExpectedRowVersion)
                    throw new BillingSettlementConflictException(
                        "Data telah berubah. Muat ulang sebelum melanjutkan.");
                if (settlement.Status == BillingSettlementStatuses.Settled)
                    throw new BillingSettlementValidationException(
                        "Settlement sudah lunas dan tidak dapat menerima tender baru.");

                paymentMethod = await LoadPaymentMethodAsync(request.PaymentMethodId, cancellationToken);
                ValidatePaymentMethod(paymentMethod);
                var activeShift = paymentMethod.IsCash
                    ? await RequireCashierShiftAsync(actorUserId, cancellationToken)
                    : null;
                var collectibleAmount = CalculateCollectibleAmount(settlement);
                if (request.Amount > collectibleAmount)
                    throw new BillingSettlementValidationException(
                        "Total metode pembayaran melebihi saldo yang harus dibayar.");

                var now = DateTimeOffset.UtcNow;
                tender = new BilTender
                {
                    SettlementId = settlement.Id,
                    Settlement = settlement,
                    PaymentMethodId = request.PaymentMethodId,
                    Amount = request.Amount,
                    Status = BillingTenderStatuses.Created,
                    IdempotencyKey = idempotencyKey,
                    PayloadHash = payloadHash,
                    CorrelationId = request.CorrelationId,
                    CausationId = request.CausationId,
                    AttemptedAt = now,
                    CashierShiftId = activeShift?.Id,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                settlement.Tenders.Add(tender);
                _dbContext.BilTenders.Add(tender);
                RecalculateSettlement(settlement, actorUserId, now);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            if (isReplay && paymentMethod.IsCash)
                await _dbContext.SaveChangesAsync(cancellationToken);

            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingSettlementConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingSettlementConflictException(
                "Tender tidak dapat disimpan karena correlation atau idempotency key sudah diproses.",
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

        await AuditTenderCreatedAsync(tender, actorUserId, isReplay);
        if (paymentMethod.IsCash)
        {
            var response = await ReconcileTenderAsync(
                tender.Id,
                new BillingPaymentProviderResult(
                    $"cash:{tender.Id:N}",
                    BillingPaymentProviderOutcome.Succeeded,
                    null,
                    "CASH_RECEIVED",
                    DateTimeOffset.UtcNow,
                    tender.CorrelationId,
                    tender.CausationId),
                actorUserId,
                cancellationToken);
            response.IsReplay = isReplay;
            return response;
        }
        var providerRequest = new BillingPaymentProviderRequest(
            tender.Id,
            paymentMethod.PaymentMethodCode,
            paymentMethod.IntegrationCode,
            tender.Amount,
            tender.IdempotencyKey,
            tender.CorrelationId,
            tender.CausationId);

        try
        {
            var providerResult = await _providerAdapter.SubmitAsync(providerRequest, cancellationToken);
            var response = await ReconcileTenderAsync(
                tender.Id, providerResult, actorUserId, cancellationToken);
            response.IsReplay = isReplay;
            return response;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (BillingPaymentProviderTimeoutException exception)
        {
            var pending = await PersistUnknownProviderResultAsync(
                tender, "TIMEOUT", actorUserId, cancellationToken);
            throw new BillingSettlementProviderPendingException(
                exception.Message, pending, StatusCodes.Status504GatewayTimeout, exception);
        }
        catch (BillingSettlementException)
        {
            throw;
        }
        catch (Exception exception)
        {
            var pending = await PersistUnknownProviderResultAsync(
                tender, "UNKNOWN", actorUserId, cancellationToken);
            throw new BillingSettlementProviderPendingException(
                "Status pembayaran belum dapat dipastikan dari provider.",
                pending,
                StatusCodes.Status502BadGateway,
                exception);
        }
    }

    public async Task<TenderResponse> ReconcileTenderAsync(
        Guid tenderId,
        BillingPaymentProviderResult result,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateProviderResult(tenderId, result);
        var resultHash = ComputeProviderResultHash(result);
        var cashReceiptApplied = false;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_TENDER_{tenderId:N}", cancellationToken);
            }

            var tender = await _dbContext.BilTenders
                .Include(x => x.Settlement)
                .ThenInclude(x => x.Tenders)
                .SingleOrDefaultAsync(x => x.Id == tenderId && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Tender tidak ditemukan.");
            var paymentMethod = await LoadPaymentMethodAsync(
                tender.PaymentMethodId, cancellationToken);
            if (paymentMethod.IsCash
                && (result.EventId != $"cash:{tender.Id:N}"
                    || result.Outcome != BillingPaymentProviderOutcome.Succeeded
                    || !string.IsNullOrWhiteSpace(result.ProviderReference)))
                throw new BillingSettlementValidationException(
                    "Tender tunai hanya dapat direkonsiliasi oleh penerimaan kas internal.");

            if (tender.CorrelationId != result.CorrelationId
                || tender.CausationId != result.CausationId)
                throw new BillingSettlementConflictException(
                    "Correlation atau causation provider tidak sesuai dengan tender.");
            if (tender.LastProviderEventId == result.EventId)
            {
                if (tender.LastProviderPayloadHash != resultHash)
                    throw new BillingSettlementConflictException(
                        "Event provider yang sama memiliki isi berbeda.");
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return MapTender(tender, true);
            }
            if (tender.ProviderOccurredAt.HasValue
                && result.OccurredAt < tender.ProviderOccurredAt.Value)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                return MapTender(tender, true);
            }

            var targetStatus = MapProviderStatus(result.Outcome);
            if (IsTerminal(tender.Status))
            {
                var reversalAllowed = tender.Status == BillingTenderStatuses.Succeeded
                    && targetStatus == BillingTenderStatuses.Reversed;
                if (!reversalAllowed)
                {
                    if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                    return MapTender(tender, true);
                }
            }
            if (targetStatus == BillingTenderStatuses.Reversed
                && tender.Status != BillingTenderStatuses.Succeeded)
                throw new BillingSettlementValidationException(
                    "Hanya tender berhasil yang dapat direversal.");
            if (targetStatus == BillingTenderStatuses.Succeeded
                && !paymentMethod.IsCash
                && string.IsNullOrWhiteSpace(result.ProviderReference))
                throw new BillingSettlementValidationException(
                    "ProviderReference wajib tersedia untuk tender yang berhasil.");
            if (!string.IsNullOrWhiteSpace(tender.ProviderReference)
                && !string.IsNullOrWhiteSpace(result.ProviderReference)
                && tender.ProviderReference != result.ProviderReference)
                throw new BillingSettlementConflictException(
                    "ProviderReference tidak konsisten dengan hasil sebelumnya.");
            if (!string.IsNullOrWhiteSpace(result.ProviderReference)
                && await _dbContext.BilTenders.AsNoTracking().AnyAsync(
                    x => x.Id != tender.Id
                        && x.ProviderReference == result.ProviderReference
                        && !x.IsDelete,
                    cancellationToken))
                throw new BillingSettlementConflictException(
                    "ProviderReference sudah digunakan oleh tender lain.");

            var beforeTenderStatus = tender.Status;
            var beforeSettlementStatus = tender.Settlement.Status;
            tender.Status = targetStatus;
            tender.ProviderReference ??= result.ProviderReference;
            tender.ProviderStatusCode = result.ProviderStatusCode;
            tender.ProviderOccurredAt = result.OccurredAt;
            tender.LastProviderEventId = result.EventId;
            tender.LastProviderPayloadHash = resultHash;
            tender.SettledAt = targetStatus is BillingTenderStatuses.Succeeded
                or BillingTenderStatuses.Reversed
                ? result.OccurredAt
                : null;
            tender.RowVersion = Guid.NewGuid();
            tender.UpdateDateTime = DateTime.UtcNow;
            tender.UpdateBy = actorUserId;

            if (targetStatus == BillingTenderStatuses.Succeeded
                && beforeTenderStatus != BillingTenderStatuses.Succeeded
                && paymentMethod.IsCash)
            {
                var activeShift = await RequireCashierShiftAsync(actorUserId, cancellationToken);
                if (!tender.CashierShiftId.HasValue || tender.CashierShiftId != activeShift.Id)
                    throw new BillingSettlementConflictException(
                        "Tender tunai tidak terikat ke shift kasir aktif yang sama.");
                cashReceiptApplied = await _cashierShiftService.ApplyCashReceiptAsync(
                    activeShift,
                    "TENDER",
                    tender.Id,
                    tender.Amount,
                    actorUserId,
                    tender.CorrelationId,
                    tender.CausationId,
                    result.OccurredAt,
                    cancellationToken);
            }

            if (targetStatus == BillingTenderStatuses.Succeeded
                && beforeTenderStatus != BillingTenderStatuses.Succeeded
                && tender.Settlement.Purpose == BillingSettlementPurposes.DepositTopUp)
                await ApplyDepositMovementAsync(tender, actorUserId, cancellationToken);

            RecalculateSettlement(tender.Settlement, actorUserId, result.OccurredAt);
            SettlementAllocationResult allocationResult;
            try
            {
                allocationResult = await _allocationService.ReconcileSettlementAsync(
                    tender.Settlement, actorUserId, result.OccurredAt, cancellationToken);
            }
            catch (BillingAllocationConflictException exception)
            {
                throw new BillingSettlementConflictException(exception.Message, exception);
            }
            catch (BillingAllocationValidationException exception)
            {
                throw new BillingSettlementValidationException(exception.Message);
            }
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditTenderResultAsync(
                tender, beforeTenderStatus, beforeSettlementStatus, actorUserId);
            await _allocationService.AuditSettlementAllocationAsync(
                allocationResult, actorUserId);
            if (cashReceiptApplied)
                await _cashierShiftService.AuditCashReceiptAsync(
                    tender.Id, cancellationToken);
            return MapTender(tender, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingSettlementConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingSettlementConflictException(
                "Hasil provider tidak dapat disimpan karena event atau reference sudah diproses.",
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

    private Task<TenderResponse> PersistUnknownProviderResultAsync(
        BilTender tender,
        string providerStatusCode,
        Guid actorUserId,
        CancellationToken cancellationToken) =>
        ReconcileTenderAsync(
            tender.Id,
            new BillingPaymentProviderResult(
                $"unknown:{tender.Id:N}:{tender.IdempotencyKey:N}",
                BillingPaymentProviderOutcome.Pending,
                null,
                providerStatusCode,
                DateTimeOffset.UtcNow,
                tender.CorrelationId,
                tender.CausationId),
            actorUserId,
            cancellationToken);

    private async Task ApplyDepositMovementAsync(
        BilTender tender,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        var accountId = tender.Settlement.DepositAccountId
            ?? throw new BillingSettlementConflictException(
                "Settlement top-up tidak memiliki account deposit.");
        if (_dbContext.Database.IsRelational())
            await AcquireLockAsync($"BIL_DEPOSIT_{accountId:N}", cancellationToken);

        var priorMovement = await _dbContext.BilDepositMovements
            .SingleOrDefaultAsync(x => x.IdempotencyKey == tender.IdempotencyKey, cancellationToken);
        if (priorMovement is not null)
        {
            if (priorMovement.SettlementId != tender.SettlementId
                || priorMovement.PaymentMethodId != tender.PaymentMethodId
                || priorMovement.Amount != tender.Amount)
                throw new BillingSettlementConflictException(
                    "Idempotency tender sudah dipakai oleh movement deposit lain.");
            return;
        }

        var account = await _dbContext.BilDepositAccounts
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.Id == accountId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Account deposit tidak ditemukan.");
        if (account.Status != BillingDepositAccountStatuses.Active)
            throw new BillingSettlementValidationException("Account deposit sudah ditutup.");
        var afterBalance = checked(account.AvailableBalance + tender.Amount);
        if (afterBalance > MaxMoneyAmount)
            throw new BillingSettlementValidationException(
                "Saldo deposit melebihi batas nominal yang didukung.");

        var movement = new BilDepositMovement
        {
            DepositAccountId = account.Id,
            DepositAccount = account,
            MovementType = BillingDepositMovementTypes.TopUp,
            Amount = tender.Amount,
            SettlementId = tender.SettlementId,
            PaymentMethodId = tender.PaymentMethodId,
            CashierShiftId = tender.CashierShiftId,
            IdempotencyKey = tender.IdempotencyKey,
            PayloadHash = tender.PayloadHash,
            CorrelationId = tender.CorrelationId,
            CausationId = tender.CausationId,
            OccurredAt = tender.ProviderOccurredAt ?? DateTimeOffset.UtcNow,
            Reason = tender.CashierShiftId.HasValue
                ? "Settlement tender tunai berhasil."
                : "Settlement tender non-tunai berhasil.",
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        account.Movements.Add(movement);
        _dbContext.BilDepositMovements.Add(movement);
        account.AvailableBalance = afterBalance;
        account.RowVersion = Guid.NewGuid();
        account.UpdateDateTime = DateTime.UtcNow;
        account.UpdateBy = actorUserId;
    }

    private async Task ValidateTargetAndAmountAsync(
        CreateSettlementRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Purpose == BillingSettlementPurposes.InvoicePayment)
        {
            var invoice = await _dbContext.BilInvoices.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == request.InvoiceId && !x.IsDelete,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");
            if (invoice.Status != BillingInvoiceStatuses.Open)
                throw new BillingSettlementValidationException(
                    "Hanya invoice OPEN yang dapat menerima settlement.");
            var calculation = await _dbContext.BilCalculationVersions.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.InvoiceId == invoice.Id
                        && x.VersionNo == invoice.CurrentCalculationVersion
                        && !x.IsDelete,
                    cancellationToken)
                ?? throw new BillingSettlementValidationException(
                    "Invoice belum memiliki hasil perhitungan terkini.");
            var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
                .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice
                    && x.TargetId == invoice.Id && !x.IsDelete)
                .SumAsync(
                    x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount),
                    cancellationToken) ?? 0;
            var allocationExcess = await _dbContext.BilRefundableCredits.AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id
                    && x.SourceType == BillingRefundableCreditSourceTypes.AllocationExcess
                    && !x.IsDelete)
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
            var available = Math.Max(
                calculation.PatientAmount - paidAmount + allocationExcess - writeOffTotal - adjustmentNet, 0);
            if (request.RequestedAmount > available)
                throw new BillingSettlementValidationException(
                    "Nominal settlement melebihi saldo pasien pada invoice.");
            return;
        }

        var account = await _dbContext.BilDepositAccounts.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.DepositAccountId && !x.IsDelete,
                cancellationToken)
            ?? throw new KeyNotFoundException("Account deposit tidak ditemukan.");
        if (account.Status != BillingDepositAccountStatuses.Active)
            throw new BillingSettlementValidationException("Account deposit sudah ditutup.");
    }

    private async Task<MstPaymentMethod> LoadPaymentMethodAsync(
        Guid paymentMethodId,
        CancellationToken cancellationToken) =>
        await _dbContext.MstPaymentMethods.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == paymentMethodId && !x.IsDelete && !x.IsCancel,
                cancellationToken)
        ?? throw new KeyNotFoundException("Metode pembayaran tidak ditemukan.");

    private async Task<BilCashierShift> RequireCashierShiftAsync(
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _cashierShiftService.RequireActiveShiftAsync(
                actorUserId, cancellationToken);
        }
        catch (CashierShiftValidationException exception)
        {
            throw new BillingSettlementValidationException(exception.Message);
        }
        catch (CashierShiftForbiddenException exception)
        {
            throw new BillingSettlementValidationException(exception.Message);
        }
    }

    private static void ValidatePaymentMethod(MstPaymentMethod paymentMethod)
    {
        if (!paymentMethod.IsActive || !paymentMethod.IsAvailableForBilling)
            throw new BillingSettlementValidationException(
                "Metode pembayaran tidak aktif atau tidak tersedia untuk Billing.");
        if (paymentMethod.IsInsurance
            || paymentMethod.IsCompanyGuarantor
            || paymentMethod.IsMembership)
            throw new BillingSettlementValidationException(
                "Metode penjamin tidak dapat digunakan sebagai tender pasien.");
    }

    private static void ValidateCreateRequest(
        CreateSettlementRequest request,
        Guid idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (idempotencyKey == Guid.Empty)
            throw new BillingSettlementValidationException("Idempotency-Key wajib diisi.");
        var hasInvoice = request.InvoiceId.HasValue && request.InvoiceId != Guid.Empty;
        var hasDeposit = request.DepositAccountId.HasValue && request.DepositAccountId != Guid.Empty;
        if (hasInvoice == hasDeposit)
            throw new BillingSettlementValidationException(
                "Settlement harus memiliki tepat satu target invoice atau account deposit.");
        if (request.Purpose == BillingSettlementPurposes.InvoicePayment && !hasInvoice
            || request.Purpose == BillingSettlementPurposes.DepositTopUp && !hasDeposit
            || request.Purpose is not (BillingSettlementPurposes.InvoicePayment
                or BillingSettlementPurposes.DepositTopUp))
            throw new BillingSettlementValidationException(
                "Purpose settlement tidak sesuai dengan target.");
        ValidateMoney(request.RequestedAmount, "Nominal settlement");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingSettlementValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateTenderRequest(
        Guid settlementId,
        CreateTenderRequest request,
        Guid idempotencyKey)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (settlementId == Guid.Empty || request.PaymentMethodId == Guid.Empty)
            throw new BillingSettlementValidationException(
                "SettlementId dan PaymentMethodId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingSettlementValidationException("Idempotency-Key wajib diisi.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingSettlementValidationException("ExpectedRowVersion wajib diisi.");
        ValidateMoney(request.Amount, "Nominal tender");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingSettlementValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateMoney(decimal amount, string fieldName)
    {
        if (amount <= 0 || amount > MaxMoneyAmount || decimal.Round(amount, 2) != amount)
            throw new BillingSettlementValidationException(
                $"{fieldName} harus positif dan maksimal memiliki dua angka desimal.");
    }

    private static void ValidateProviderResult(
        Guid tenderId,
        BillingPaymentProviderResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        if (tenderId == Guid.Empty || string.IsNullOrWhiteSpace(result.EventId))
            throw new BillingSettlementValidationException(
                "TenderId dan EventId provider wajib diisi.");
        if (result.EventId.Length > 100)
            throw new BillingSettlementValidationException(
                "EventId provider maksimal 100 karakter.");
        if (result.ContractVersion != BillingPaymentProviderContracts.CurrentVersion)
            throw new BillingSettlementValidationException(
                "Versi kontrak provider tidak didukung.");
        if (result.CorrelationId == Guid.Empty || result.CausationId == Guid.Empty)
            throw new BillingSettlementValidationException(
                "CorrelationId dan CausationId provider wajib diisi.");
        if (result.ProviderReference?.Length > 150
            || result.ProviderStatusCode?.Length > 50)
            throw new BillingSettlementValidationException(
                "Metadata hasil provider melebihi batas panjang yang didukung.");
    }

    private static string MapProviderStatus(BillingPaymentProviderOutcome outcome) => outcome switch
    {
        BillingPaymentProviderOutcome.Pending => BillingTenderStatuses.Pending,
        BillingPaymentProviderOutcome.Succeeded => BillingTenderStatuses.Succeeded,
        BillingPaymentProviderOutcome.Failed => BillingTenderStatuses.Failed,
        BillingPaymentProviderOutcome.Expired => BillingTenderStatuses.Expired,
        BillingPaymentProviderOutcome.Reversed => BillingTenderStatuses.Reversed,
        _ => throw new BillingSettlementValidationException("Status provider tidak didukung.")
    };

    private static bool IsTerminal(string status) => status is
        BillingTenderStatuses.Succeeded
        or BillingTenderStatuses.Failed
        or BillingTenderStatuses.Expired
        or BillingTenderStatuses.Reversed;

    private static void RecalculateSettlement(
        BilSettlement settlement,
        Guid actorUserId,
        DateTimeOffset occurredAt)
    {
        var activeTenders = settlement.Tenders.Where(x => !x.IsDelete).ToList();
        var successful = activeTenders
            .Where(x => x.Status == BillingTenderStatuses.Succeeded)
            .Sum(x => x.Amount);
        var reserved = activeTenders
            .Where(x => x.Status is BillingTenderStatuses.Created or BillingTenderStatuses.Pending)
            .Sum(x => x.Amount);
        settlement.SuccessfulAmount = successful;
        settlement.Status = successful >= settlement.RequestedAmount
            ? BillingSettlementStatuses.Settled
            : successful > 0
                ? BillingSettlementStatuses.PartiallySettled
                : reserved > 0
                    ? BillingSettlementStatuses.InProgress
                    : activeTenders.Count > 0
                        ? BillingSettlementStatuses.Failed
                        : BillingSettlementStatuses.Draft;
        settlement.CompletedAt = settlement.Status is BillingSettlementStatuses.Settled
            or BillingSettlementStatuses.Failed
            ? occurredAt
            : null;
        settlement.RowVersion = Guid.NewGuid();
        settlement.UpdateDateTime = DateTime.UtcNow;
        settlement.UpdateBy = actorUserId;
    }

    private static decimal CalculatePendingAmount(BilSettlement settlement) =>
        settlement.Tenders
            .Where(x => !x.IsDelete
                && x.Status is BillingTenderStatuses.Created or BillingTenderStatuses.Pending)
            .Sum(x => x.Amount);

    private static decimal CalculateCollectibleAmount(BilSettlement settlement) =>
        Math.Max(
            settlement.RequestedAmount
            - settlement.SuccessfulAmount
            - CalculatePendingAmount(settlement),
            0);

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditSettlementAsync(
        BilSettlement settlement,
        Guid actorUserId,
        bool isReplay) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingSettlement.Create",
            "Settlement pasien dibuat atau hasil idempotent dikembalikan.",
            new
            {
                SettlementId = settlement.Id,
                settlement.Purpose,
                settlement.RequestedAmount,
                settlement.Status,
                settlement.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });

    private Task AuditTenderCreatedAsync(
        BilTender tender,
        Guid actorUserId,
        bool isReplay) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingSettlement.TenderCreated",
            "Tender pasien disimpan sebelum direkonsiliasi.",
            new
            {
                TenderId = tender.Id,
                tender.SettlementId,
                tender.PaymentMethodId,
                tender.Amount,
                tender.Status,
                tender.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });

    private Task AuditTenderResultAsync(
        BilTender tender,
        string beforeTenderStatus,
        string beforeSettlementStatus,
        Guid actorUserId) =>
        _loggerService.AuditAsync(
            LogCategory,
            "BillingSettlement.TenderReconciled",
            "Status tender direkonsiliasi tanpa menyimpan payload atau credential provider.",
            new
            {
                TenderId = tender.Id,
                tender.SettlementId,
                TenderStatusBefore = beforeTenderStatus,
                TenderStatusAfter = tender.Status,
                SettlementStatusBefore = beforeSettlementStatus,
                SettlementStatusAfter = tender.Settlement.Status,
                tender.Amount,
                tender.ProviderStatusCode,
                tender.Settlement.SuccessfulAmount,
                tender.Settlement.AllocatedAmount,
                tender.CorrelationId,
                ActorUserId = actorUserId
            });

    private static string TargetLockKey(CreateSettlementRequest request) =>
        request.InvoiceId.HasValue
            ? $"BIL_SETTLEMENT_INVOICE_{request.InvoiceId.Value:N}"
            : $"BIL_SETTLEMENT_DEPOSIT_{request.DepositAccountId!.Value:N}";

    private static string ComputeSettlementPayloadHash(CreateSettlementRequest request)
    {
        var canonical = string.Join('|',
            request.InvoiceId?.ToString("N") ?? string.Empty,
            request.DepositAccountId?.ToString("N") ?? string.Empty,
            request.Purpose,
            request.RequestedAmount.ToString(CultureInfo.InvariantCulture),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Hash(canonical);
    }

    private static string ComputeTenderPayloadHash(
        Guid settlementId,
        CreateTenderRequest request)
    {
        var canonical = string.Join('|',
            settlementId.ToString("N"),
            request.PaymentMethodId.ToString("N"),
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.ExpectedRowVersion.ToString("N"),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Hash(canonical);
    }

    private static string ComputeProviderResultHash(BillingPaymentProviderResult result)
    {
        var canonical = string.Join('|',
            result.EventId,
            result.Outcome,
            result.ProviderReference ?? string.Empty,
            result.ProviderStatusCode ?? string.Empty,
            result.OccurredAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
            result.CorrelationId.ToString("N"),
            result.CausationId.ToString("N"),
            result.ContractVersion);
        return Hash(canonical);
    }

    private static string Hash(string canonical) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));

    private static SettlementResponse MapSettlement(
        BilSettlement settlement,
        bool isReplay)
    {
        var pending = CalculatePendingAmount(settlement);
        var outstanding = Math.Max(
            settlement.RequestedAmount - settlement.SuccessfulAmount, 0);
        return new SettlementResponse
        {
            Id = settlement.Id,
            InvoiceId = settlement.InvoiceId,
            DepositAccountId = settlement.DepositAccountId,
            Purpose = settlement.Purpose,
            Status = settlement.Status,
            RequestedAmount = settlement.RequestedAmount,
            SuccessfulAmount = settlement.SuccessfulAmount,
            AllocatedAmount = settlement.AllocatedAmount,
            PendingAmount = pending,
            OutstandingAmount = outstanding,
            CollectibleAmount = Math.Max(outstanding - pending, 0),
            IsReplay = isReplay,
            CorrelationId = settlement.CorrelationId,
            RowVersion = settlement.RowVersion,
            StartedAt = settlement.StartedAt,
            CompletedAt = settlement.CompletedAt,
            Tenders = settlement.Tenders
                .Where(x => !x.IsDelete)
                .OrderBy(x => x.AttemptedAt)
                .ThenBy(x => x.Id)
                .Select(x => MapTender(x, false))
                .ToList()
        };
    }

    private static TenderResponse MapTender(BilTender tender, bool isReplay) => new()
    {
        Id = tender.Id,
        SettlementId = tender.SettlementId,
        PaymentMethodId = tender.PaymentMethodId,
        Amount = tender.Amount,
        Status = tender.Status,
        ProviderReferenceMasked = MaskProviderReference(tender.ProviderReference),
        ProviderStatusCode = tender.ProviderStatusCode,
        AttemptedAt = tender.AttemptedAt,
        SettledAt = tender.SettledAt,
        CashierShiftId = tender.CashierShiftId,
        RowVersion = tender.RowVersion,
        IsReplay = isReplay
    };

    private static string? MaskProviderReference(string? providerReference)
    {
        if (string.IsNullOrWhiteSpace(providerReference)) return null;
        return providerReference.Length <= 4
            ? "****"
            : $"****{providerReference[^4..]}";
    }
}

public abstract class BillingSettlementException : Exception
{
    protected BillingSettlementException(string message) : base(message) { }
    protected BillingSettlementException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingSettlementValidationException(string message)
    : BillingSettlementException(message);

public sealed class BillingSettlementConflictException : BillingSettlementException
{
    public BillingSettlementConflictException(string message) : base(message) { }
    public BillingSettlementConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}

public sealed class BillingSettlementProviderPendingException : BillingSettlementException
{
    public BillingSettlementProviderPendingException(
        string message,
        TenderResponse tender,
        int statusCode,
        Exception innerException)
        : base(message, innerException)
    {
        Tender = tender;
        StatusCode = statusCode;
    }

    public TenderResponse Tender { get; }
    public int StatusCode { get; }
}
