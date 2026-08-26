using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed class BillingDepositService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private readonly ApplicationDbContext _dbContext;
    private readonly BillingNumberSeriesService _numberSeries;
    private readonly CashierShiftService _cashierShiftService;
    private readonly LoggerService _loggerService;

    public BillingDepositService(
        ApplicationDbContext dbContext,
        BillingNumberSeriesService numberSeries,
        CashierShiftService cashierShiftService,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _numberSeries = numberSeries;
        _cashierShiftService = cashierShiftService;
        _loggerService = loggerService;
    }

    public async Task<DepositResponse> GetByEncounterAsync(
        Guid encounterId,
        CancellationToken cancellationToken)
    {
        if (encounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");

        var account = await _dbContext.BilDepositAccounts.AsNoTracking()
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Deposit rawat inap belum tersedia.");

        return MapDeposit(account);
    }

    public async Task<SettlementResponse> TopUpAsync(
        Guid encounterId,
        DepositTopUpRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateTopUpRequest(encounterId, request, idempotencyKey);
        var payloadHash = ComputeTopUpPayloadHash(encounterId, request);
        var cashReceiptApplied = false;
        Guid? cashReceiptSourceId = null;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_DEPOSIT_{encounterId:N}", cancellationToken);
            }

            var priorMovement = await _dbContext.BilDepositMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorMovement is not null)
            {
                if (priorMovement.MovementType != BillingDepositMovementTypes.TopUp
                    || priorMovement.PayloadHash != payloadHash)
                    throw new BillingDepositConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");

                var replayAccount = await LoadAccountAsync(priorMovement.DepositAccountId, cancellationToken);
                if (replayAccount.EncounterId != encounterId)
                    throw new BillingDepositConflictException(
                        "Permintaan idempotent tidak sesuai dengan encounter deposit.");

                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditTopUpAsync(replayAccount, priorMovement, actorUserId, true);
                return MapSettlement(replayAccount, priorMovement, true);
            }
            if (await _dbContext.BilDepositMovements.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingDepositConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var encounter = await _dbContext.TrxPatientEncounters.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == encounterId && !x.IsDelete && !x.IsCancel,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Encounter tidak ditemukan.");
            if (encounter.EncounterType != EncounterType.Inpatient)
                throw new BillingDepositValidationException(
                    "Deposit hanya tersedia untuk encounter rawat inap.");
            if (encounter.EncounterStatus is EncounterStatus.Cancelled or EncounterStatus.NoShow)
                throw new BillingDepositValidationException(
                    "Encounter yang dibatalkan atau tidak hadir tidak dapat menerima deposit.");

            var paymentMethod = await _dbContext.MstPaymentMethods.AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == request.PaymentMethodId && !x.IsDelete && !x.IsCancel,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Metode pembayaran tidak ditemukan.");
            ValidatePaymentMethod(paymentMethod.IsActive, paymentMethod.IsAvailableForBilling,
                paymentMethod.IsInsurance, paymentMethod.IsCompanyGuarantor,
                paymentMethod.IsMembership, paymentMethod.IsNeedReferenceNumber,
                paymentMethod.IsNeedApproval, paymentMethod.IsNeedAttachment);
            BilCashierShift? cashierShift = null;
            if (paymentMethod.IsCash)
            {
                try
                {
                    cashierShift = await _cashierShiftService.RequireActiveShiftAsync(
                        actorUserId, cancellationToken);
                }
                catch (CashierShiftValidationException exception)
                {
                    throw new BillingDepositValidationException(exception.Message);
                }
                catch (CashierShiftForbiddenException exception)
                {
                    throw new BillingDepositValidationException(exception.Message);
                }
            }

            var account = await _dbContext.BilDepositAccounts
                .Include(x => x.Movements)
                .SingleOrDefaultAsync(x => x.EncounterId == encounterId && !x.IsDelete, cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (account is null)
            {
                if (request.ExpectedRowVersion.HasValue)
                    throw new BillingDepositConflictException(
                        "Data telah berubah. Muat ulang sebelum melanjutkan.");

                account = new BilDepositAccount
                {
                    EncounterId = encounterId,
                    AccountNumber = await _numberSeries.AllocateDepositAccountNumberAsync(
                        actorUserId, now, cancellationToken),
                    AvailableBalance = 0,
                    Status = BillingDepositAccountStatuses.Active,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.BilDepositAccounts.Add(account);
            }
            else
            {
                EnsureActiveAndCurrent(account, request.ExpectedRowVersion);
            }

            var beforeBalance = account.AvailableBalance;
            var afterBalance = checked(beforeBalance + request.Amount);
            if (afterBalance > MaxMoneyAmount)
                throw new BillingDepositValidationException(
                    "Saldo deposit melebihi batas nominal yang didukung.");
            var movement = new BilDepositMovement
            {
                DepositAccountId = account.Id,
                DepositAccount = account,
                MovementType = BillingDepositMovementTypes.TopUp,
                Amount = request.Amount,
                PaymentMethodId = request.PaymentMethodId,
                CashierShiftId = cashierShift?.Id,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                OccurredAt = now,
                Reason = request.Reason.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            account.Movements.Add(movement);
            _dbContext.BilDepositMovements.Add(movement);
            if (cashierShift is not null)
            {
                cashReceiptApplied = await _cashierShiftService.ApplyCashReceiptAsync(
                    cashierShift,
                    "DEPOSIT_MOVEMENT",
                    movement.Id,
                    movement.Amount,
                    actorUserId,
                    movement.CorrelationId,
                    movement.CausationId,
                    movement.OccurredAt,
                    cancellationToken);
                cashReceiptSourceId = movement.Id;
            }
            account.AvailableBalance = afterBalance;
            account.RowVersion = Guid.NewGuid();
            account.UpdateDateTime = DateTime.UtcNow;
            account.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditTopUpAsync(account, movement, actorUserId, false, beforeBalance);
            if (cashReceiptApplied && cashReceiptSourceId.HasValue)
                await _cashierShiftService.AuditCashReceiptAsync(
                    cashReceiptSourceId.Value, cancellationToken);
            return MapSettlement(account, movement, false);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Top-up tidak dapat disimpan karena account, correlation, atau idempotency key sudah diproses.",
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

    public async Task<DepositResponse> ReverseTopUpAsync(
        Guid encounterId,
        Guid movementId,
        ReverseDepositMovementRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        ValidateReversalRequest(encounterId, movementId, request, idempotencyKey);
        var payloadHash = ComputeReversalPayloadHash(encounterId, movementId, request);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync($"BIL_DEPOSIT_{encounterId:N}", cancellationToken);
            }

            var priorReversal = await _dbContext.BilDepositMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorReversal is not null)
            {
                if (priorReversal.MovementType != BillingDepositMovementTypes.Reversal
                    || priorReversal.ReversesMovementId != movementId
                    || priorReversal.PayloadHash != payloadHash)
                    throw new BillingDepositConflictException(
                        "Permintaan yang sama memiliki isi berbeda; gunakan permintaan baru.");

                var replayAccount = await LoadAccountAsync(priorReversal.DepositAccountId, cancellationToken);
                if (replayAccount.EncounterId != encounterId)
                    throw new BillingDepositConflictException(
                        "Permintaan idempotent tidak sesuai dengan encounter deposit.");

                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                await AuditReversalAsync(replayAccount, priorReversal, actorUserId, true);
                return MapDeposit(replayAccount);
            }
            if (await _dbContext.BilDepositMovements.AsNoTracking()
                .AnyAsync(x => x.CorrelationId == request.CorrelationId, cancellationToken))
                throw new BillingDepositConflictException(
                    "CorrelationId sudah diproses; gunakan correlation baru.");

            var account = await _dbContext.BilDepositAccounts
                .Include(x => x.Movements)
                .SingleOrDefaultAsync(
                    x => x.EncounterId == encounterId && !x.IsDelete,
                    cancellationToken)
                ?? throw new KeyNotFoundException("Deposit rawat inap belum tersedia.");
            EnsureActiveAndCurrent(account, request.ExpectedRowVersion);

            var original = account.Movements.SingleOrDefault(
                x => x.Id == movementId && !x.IsDelete)
                ?? throw new KeyNotFoundException("Movement deposit tidak ditemukan.");
            if (original.MovementType != BillingDepositMovementTypes.TopUp)
                throw new BillingDepositValidationException(
                    "Hanya movement top-up yang dapat dibalik melalui operasi ini.");
            if (account.Movements.Any(x => x.ReversesMovementId == movementId && !x.IsDelete))
                throw new BillingDepositConflictException("Movement top-up sudah pernah dibalik.");
            if (account.AvailableBalance < original.Amount)
                throw new BillingDepositValidationException(
                    "Dana deposit atau saldo tagihan tidak mencukupi.");

            var beforeBalance = account.AvailableBalance;
            var now = DateTimeOffset.UtcNow;
            var reversal = new BilDepositMovement
            {
                DepositAccountId = account.Id,
                DepositAccount = account,
                MovementType = BillingDepositMovementTypes.Reversal,
                Amount = original.Amount,
                SettlementId = original.SettlementId,
                PaymentMethodId = original.PaymentMethodId,
                IdempotencyKey = idempotencyKey,
                PayloadHash = payloadHash,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                OccurredAt = now,
                Reason = request.Reason.Trim(),
                ReversesMovementId = original.Id,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            account.Movements.Add(reversal);
            _dbContext.BilDepositMovements.Add(reversal);
            account.AvailableBalance = beforeBalance - original.Amount;
            account.RowVersion = Guid.NewGuid();
            account.UpdateDateTime = DateTime.UtcNow;
            account.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditReversalAsync(account, reversal, actorUserId, false, beforeBalance);
            return MapDeposit(account);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new BillingDepositConflictException(
                "Reversal tidak dapat disimpan karena account, movement, correlation, atau idempotency key sudah diproses.",
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

    private async Task<BilDepositAccount> LoadAccountAsync(
        Guid accountId,
        CancellationToken cancellationToken) =>
        await _dbContext.BilDepositAccounts.AsNoTracking()
            .Include(x => x.Movements)
            .SingleOrDefaultAsync(x => x.Id == accountId && !x.IsDelete, cancellationToken)
        ?? throw new BillingDepositConflictException(
            "Receipt idempotency tidak memiliki account deposit yang valid.");

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken);

    private Task AuditTopUpAsync(
        BilDepositAccount account,
        BilDepositMovement movement,
        Guid actorUserId,
        bool isReplay,
        decimal? beforeBalance = null)
    {
        var resolvedBefore = beforeBalance ?? account.AvailableBalance - movement.Amount;
        return _loggerService.AuditAsync(
            LogCategory,
            "BillingDeposit.TopUp",
            "Top-up dana rawat inap dicatat sebagai movement deposit yang belum dialokasikan.",
            new
            {
                DepositAccountId = account.Id,
                DepositMovementId = movement.Id,
                movement.PaymentMethodId,
                movement.Amount,
                BeforeBalance = resolvedBefore,
                AfterBalance = resolvedBefore + movement.Amount,
                movement.IdempotencyKey,
                movement.CorrelationId,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });
    }

    private Task AuditReversalAsync(
        BilDepositAccount account,
        BilDepositMovement reversal,
        Guid actorUserId,
        bool isReplay,
        decimal? beforeBalance = null)
    {
        var resolvedBefore = beforeBalance ?? account.AvailableBalance + reversal.Amount;
        return _loggerService.AuditAsync(
            LogCategory,
            "BillingDeposit.ReverseTopUp",
            "Top-up deposit dibalik melalui movement kompensasi baru.",
            new
            {
                DepositAccountId = account.Id,
                DepositMovementId = reversal.Id,
                reversal.ReversesMovementId,
                reversal.Amount,
                BeforeBalance = resolvedBefore,
                AfterBalance = resolvedBefore - reversal.Amount,
                reversal.IdempotencyKey,
                reversal.CorrelationId,
                Reason = reversal.Reason,
                ActorUserId = actorUserId,
                IsReplay = isReplay
            });
    }

    private static void EnsureActiveAndCurrent(
        BilDepositAccount account,
        Guid? expectedRowVersion)
    {
        if (account.Status != BillingDepositAccountStatuses.Active)
            throw new BillingDepositValidationException("Account deposit sudah ditutup.");
        if (!expectedRowVersion.HasValue || account.RowVersion != expectedRowVersion.Value)
            throw new BillingDepositConflictException(
                "Data telah berubah. Muat ulang sebelum melanjutkan.");
    }

    private static void ValidatePaymentMethod(
        bool isActive,
        bool isAvailableForBilling,
        bool isInsurance,
        bool isCompanyGuarantor,
        bool isMembership,
        bool needsReference,
        bool needsApproval,
        bool needsAttachment)
    {
        if (!isActive || !isAvailableForBilling)
            throw new BillingDepositValidationException(
                "Metode pembayaran tidak aktif atau tidak tersedia untuk Billing.");
        if (isInsurance || isCompanyGuarantor || isMembership)
            throw new BillingDepositValidationException(
                "Metode penjamin tidak dapat digunakan sebagai top-up deposit.");
        if (needsReference || needsApproval || needsAttachment)
            throw new BillingDepositValidationException(
                "Metode pembayaran ini memerlukan settlement/tender yang belum tersedia.");
    }

    private static void ValidateTopUpRequest(
        Guid encounterId,
        DepositTopUpRequest request,
        Guid idempotencyKey)
    {
        if (encounterId == Guid.Empty)
            throw new BillingDepositValidationException("EncounterId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingDepositValidationException("Idempotency-Key wajib diisi.");
        if (request.PaymentMethodId == Guid.Empty)
            throw new BillingDepositValidationException("PaymentMethodId wajib diisi.");
        if (request.Amount <= 0 || request.Amount > MaxMoneyAmount
            || decimal.Round(request.Amount, 2) != request.Amount)
            throw new BillingDepositValidationException(
                "Nominal top-up harus positif dan maksimal memiliki dua angka desimal.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingDepositValidationException(
                "ExpectedRowVersion tidak boleh Guid.Empty.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingDepositValidationException("Alasan top-up wajib diisi.");
        if (request.Reason.Trim().Length > 500)
            throw new BillingDepositValidationException("Alasan top-up maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingDepositValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static void ValidateReversalRequest(
        Guid encounterId,
        Guid movementId,
        ReverseDepositMovementRequest request,
        Guid idempotencyKey)
    {
        if (encounterId == Guid.Empty || movementId == Guid.Empty)
            throw new BillingDepositValidationException(
                "EncounterId dan MovementId wajib diisi.");
        if (idempotencyKey == Guid.Empty)
            throw new BillingDepositValidationException("Idempotency-Key wajib diisi.");
        if (request.ExpectedRowVersion == Guid.Empty)
            throw new BillingDepositValidationException("ExpectedRowVersion wajib diisi.");
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new BillingDepositValidationException("Alasan reversal wajib diisi.");
        if (request.Reason.Trim().Length > 500)
            throw new BillingDepositValidationException("Alasan reversal maksimal 500 karakter.");
        if (request.CorrelationId == Guid.Empty || request.CausationId == Guid.Empty)
            throw new BillingDepositValidationException(
                "CorrelationId dan CausationId wajib diisi.");
    }

    private static string ComputeTopUpPayloadHash(
        Guid encounterId,
        DepositTopUpRequest request)
    {
        var canonical = string.Join('|',
            encounterId.ToString("N"),
            request.PaymentMethodId.ToString("N"),
            request.Amount.ToString(CultureInfo.InvariantCulture),
            request.ExpectedRowVersion?.ToString("N") ?? string.Empty,
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static string ComputeReversalPayloadHash(
        Guid encounterId,
        Guid movementId,
        ReverseDepositMovementRequest request)
    {
        var canonical = string.Join('|',
            encounterId.ToString("N"),
            movementId.ToString("N"),
            request.ExpectedRowVersion.ToString("N"),
            request.Reason.Trim(),
            request.CorrelationId.ToString("N"),
            request.CausationId.ToString("N"));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(canonical)));
    }

    private static SettlementResponse MapSettlement(
        BilDepositAccount account,
        BilDepositMovement movement,
        bool isReplay) => new()
        {
            RequestedAmount = movement.Amount,
            SuccessfulAmount = movement.Amount,
            AllocatedAmount = 0,
            OutstandingAmount = 0,
            CollectibleAmount = 0,
            Status = BillingSettlementStatuses.Settled,
            IsReplay = isReplay,
            CorrelationId = movement.CorrelationId,
            DepositMovementId = movement.Id,
            Deposit = MapDeposit(account)
        };

    private static DepositResponse MapDeposit(BilDepositAccount account)
    {
        decimal runningBalance = 0;
        var movements = account.Movements
            .Where(x => !x.IsDelete)
            .OrderBy(x => x.OccurredAt)
            .ThenBy(x => x.CreateDateTime)
            .ThenBy(x => x.Id)
            .Select(x =>
            {
                var effect = x.MovementType == BillingDepositMovementTypes.TopUp
                    ? x.Amount
                    : -x.Amount;
                runningBalance += effect;
                return new DepositMovementResponse
                {
                    Id = x.Id,
                    MovementType = x.MovementType,
                    Amount = x.Amount,
                    BalanceEffect = effect,
                    BalanceAfter = runningBalance,
                    SettlementId = x.SettlementId,
                    PaymentMethodId = x.PaymentMethodId,
                    CashierShiftId = x.CashierShiftId,
                    CorrelationId = x.CorrelationId,
                    OccurredAt = x.OccurredAt,
                    Reason = x.Reason,
                    ReversesMovementId = x.ReversesMovementId
                };
            })
            .ToList();

        return new DepositResponse
        {
            Id = account.Id,
            EncounterId = account.EncounterId,
            AccountNumber = account.AccountNumber,
            AvailableBalance = account.AvailableBalance,
            Status = account.Status,
            RowVersion = account.RowVersion,
            Movements = movements
        };
    }
}

public sealed class BillingDepositValidationException(string message) : Exception(message);

public sealed class BillingDepositConflictException : Exception
{
    public BillingDepositConflictException(string message) : base(message) { }
    public BillingDepositConflictException(string message, Exception innerException)
        : base(message, innerException) { }
}
