using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PettyCash.Services;

/// <summary>
/// Service pengelola anggaran dan saldo berjalan kas kecil pada Bounded Context Corporate/FinanceManagement.
/// Satu-satunya penulis FinPettyCashBudget.CurrentBalance.
/// TopUpAsync dan AdjustAsync membuka transaction sendiri; ApplyDisbursementAsync sengaja TIDAK,
/// karena dipanggil dari dalam transaction milik PettyCashVoucherService di BillingManagement.
/// </summary>
public sealed class PettyCashBudgetService
{
    private const string LogCategory = "Corporate.FinanceManagement.PettyCash";
    private const string BudgetLockKey = "FIN_PETTY_CASH_BUDGET_HOSPITAL_MAIN";
    private const decimal MaxMoneyAmount = 9999999999999999.99m;
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public PettyCashBudgetService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    public async Task<PettyCashBudgetResponse> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var budget = await LoadActiveBudgetAsync(asNoTracking: true, cancellationToken);
        var reserved = await CalculateReservedAmountAsync(cancellationToken);
        return Map(budget, reserved);
    }

    public async Task<PettyCashOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken)
    {
        var activeBudget = await GetCurrentAsync(cancellationToken);

        var pendingEvidenceCount = await _dbContext.BilPettyCashVouchers.AsNoTracking()
            .CountAsync(x => !x.IsDelete && x.Status == PettyCashVoucherStatuses.CashReceived, cancellationToken);
        var pendingDisbursementCount = await _dbContext.BilPettyCashVouchers.AsNoTracking()
            .CountAsync(x => !x.IsDelete && x.Status == PettyCashVoucherStatuses.Requested, cancellationToken);

        return new PettyCashOverviewResponse
        {
            ActiveBudget = activeBudget,
            PendingEvidenceCount = pendingEvidenceCount,
            PendingDisbursementCount = pendingDisbursementCount,
            CurrentBalance = activeBudget.CurrentBalance,
            PemakaianSaldo = activeBudget.DisbursedBalance,
            DisbursedBalance = activeBudget.DisbursedBalance,
            TotalDisbursedThisPeriod = activeBudget.DisbursedBalance,
            SisaSaldo = activeBudget.RemainingBalance,
            RemainingBalance = activeBudget.RemainingBalance,
            RemainingBudgetAmount = activeBudget.RemainingBalance
        };
    }

    public async Task<PagedResult<PettyCashBudgetMovementResponse>> GetMovementsAsync(
        PettyCashBudgetMovementQuery request, CancellationToken cancellationToken)
    {
        Guid budgetId;
        if (request.BudgetId.HasValue)
        {
            budgetId = request.BudgetId.Value;
            var exists = await _dbContext.FinPettyCashBudgets.AsNoTracking()
                .AnyAsync(x => x.Id == budgetId && !x.IsDelete, cancellationToken);
            if (!exists) throw new KeyNotFoundException("Periode anggaran kas kecil tidak ditemukan.");
        }
        else
        {
            var budget = await LoadActiveBudgetAsync(asNoTracking: true, cancellationToken);
            budgetId = budget.Id;
        }

        var query = _dbContext.FinPettyCashBudgetMovements.AsNoTracking()
            .Where(x => x.BudgetId == budgetId && !x.IsDelete);

        if (!string.IsNullOrWhiteSpace(request.MovementType))
        {
            var movementType = request.MovementType.Trim().ToUpperInvariant();
            query = query.Where(x => x.MovementType == movementType);
        }
        if (request.StartDate.HasValue) query = query.Where(x => x.OccurredAt >= request.StartDate.Value);
        if (request.EndDate.HasValue) query = query.Where(x => x.OccurredAt <= request.EndDate.Value);

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.OccurredAt).ThenByDescending(x => x.CreateDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .Select(x => new
            {
                x.Id, x.MovementType, x.Amount, x.BalanceBefore, x.BalanceAfter,
                x.VoucherId, x.Reason, x.ActorUserId, x.OccurredAt,
                x.FundingSourceType, x.TransferReference
            })
            .ToListAsync(cancellationToken);

        var voucherIds = rows.Where(x => x.VoucherId.HasValue).Select(x => x.VoucherId!.Value).Distinct().ToList();
        var voucherNumbers = voucherIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await _dbContext.BilPettyCashVouchers.AsNoTracking()
                .Where(x => voucherIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.VoucherNumber, cancellationToken);

        var actorIds = rows.Select(x => x.ActorUserId).Distinct().ToList();
        var actorNames = actorIds.Count == 0
            ? new Dictionary<Guid, string?>()
            : await _dbContext.Users.AsNoTracking()
                .Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);

        var items = rows.Select(x => new PettyCashBudgetMovementResponse
        {
            Id = x.Id,
            MovementType = x.MovementType,
            Amount = x.Amount,
            BalanceBefore = x.BalanceBefore,
            BalanceAfter = x.BalanceAfter,
            VoucherId = x.VoucherId,
            VoucherNumber = x.VoucherId.HasValue && voucherNumbers.TryGetValue(x.VoucherId.Value, out var number) ? number : null,
            Reason = x.Reason,
            FundingSourceType = x.FundingSourceType,
            TransferReference = x.TransferReference,
            ActorUserId = x.ActorUserId,
            ActorName = actorNames.TryGetValue(x.ActorUserId, out var name) ? name : null,
            OccurredAt = x.OccurredAt
        }).ToList();

        return new PagedResult<PettyCashBudgetMovementResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<PettyCashBudgetResponse> TopUpAsync(
        PettyCashBudgetTopUpRequest request, Guid idempotencyKey, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (idempotencyKey == Guid.Empty) throw new PettyCashBudgetValidationException("Idempotency-Key wajib diisi.");
        ValidateAmountAndReason(request.Amount, request.Reason);
        var fundingSourceType = NormalizeAndValidateFundingSource(request.FundingSourceType, request.TransferReference);
        var transferReference = fundingSourceType == PettyCashFundingSourceTypes.Transfer
            ? request.TransferReference?.Trim()
            : null;
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync(cancellationToken);
            }

            var priorMovement = await _dbContext.FinPettyCashBudgetMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorMovement is not null)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                var replayBudget = await LoadActiveBudgetAsync(asNoTracking: true, cancellationToken);
                return Map(replayBudget, await CalculateReservedAmountAsync(cancellationToken));
            }

            var budget = await LoadActiveBudgetAsync(asNoTracking: false, cancellationToken);
            EnsureCurrentRowVersion(budget, request.ExpectedRowVersion);

            var before = budget.CurrentBalance;
            var after = checked(before + request.Amount);
            if (after > MaxMoneyAmount)
                throw new PettyCashBudgetValidationException("Saldo kas kecil melebihi batas nominal yang didukung.");
            var now = DateTimeOffset.UtcNow;

            var movement = new FinPettyCashBudgetMovement
            {
                BudgetId = budget.Id,
                Budget = budget,
                MovementType = PettyCashBudgetMovementTypes.TopUp,
                Amount = request.Amount,
                BalanceBefore = before,
                BalanceAfter = after,
                Reason = request.Reason.Trim(),
                FundingSourceType = fundingSourceType,
                TransferReference = transferReference,
                ActorUserId = actorUserId,
                IdempotencyKey = idempotencyKey,
                CorrelationId = Guid.NewGuid(),
                OccurredAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            budget.Movements.Add(movement);
            _dbContext.FinPettyCashBudgetMovements.Add(movement);
            budget.CurrentBalance = after;
            budget.TotalTopUpAmount = checked(budget.TotalTopUpAmount + request.Amount);
            budget.LastMovementAt = now;
            budget.RowVersion = Guid.NewGuid();
            budget.UpdateDateTime = DateTime.UtcNow;
            budget.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditMovementAsync("PettyCashBudget.TopUp", budget, movement, actorUserId);
            return Map(budget, await CalculateReservedAmountAsync(cancellationToken));
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException(
                "Top-up tidak dapat disimpan karena kolam anggaran atau idempotency key sudah diproses.", exception);
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

    public async Task<PettyCashBudgetResponse> AdjustAsync(
        PettyCashBudgetAdjustmentRequest request, Guid idempotencyKey, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (idempotencyKey == Guid.Empty) throw new PettyCashBudgetValidationException("Idempotency-Key wajib diisi.");
        ValidateAmountAndReason(request.Amount, request.Reason);
        var direction = NormalizeDirection(request.Direction);
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync(cancellationToken);
            }

            var priorMovement = await _dbContext.FinPettyCashBudgetMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorMovement is not null)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                var replayBudget = await LoadActiveBudgetAsync(asNoTracking: true, cancellationToken);
                return Map(replayBudget, await CalculateReservedAmountAsync(cancellationToken));
            }

            var budget = await LoadActiveBudgetAsync(asNoTracking: false, cancellationToken);
            EnsureCurrentRowVersion(budget, request.ExpectedRowVersion);

            var before = budget.CurrentBalance;
            var after = direction == PettyCashBudgetAdjustmentDirections.Decrease
                ? before - request.Amount
                : checked(before + request.Amount);

            if (direction == PettyCashBudgetAdjustmentDirections.Decrease && after < 0)
                throw new PettyCashBudgetInsufficientBalanceException(
                    "Koreksi ini akan membuat saldo kas kecil menjadi negatif.");
            if (after > MaxMoneyAmount)
                throw new PettyCashBudgetValidationException("Saldo kas kecil melebihi batas nominal yang didukung.");
            var now = DateTimeOffset.UtcNow;

            var movement = new FinPettyCashBudgetMovement
            {
                BudgetId = budget.Id,
                Budget = budget,
                MovementType = PettyCashBudgetMovementTypes.Adjustment,
                Amount = request.Amount,
                BalanceBefore = before,
                BalanceAfter = after,
                Reason = request.Reason.Trim(),
                ActorUserId = actorUserId,
                IdempotencyKey = idempotencyKey,
                CorrelationId = Guid.NewGuid(),
                OccurredAt = now,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            budget.Movements.Add(movement);
            _dbContext.FinPettyCashBudgetMovements.Add(movement);
            budget.CurrentBalance = after;
            budget.LastMovementAt = now;
            budget.RowVersion = Guid.NewGuid();
            budget.UpdateDateTime = DateTime.UtcNow;
            budget.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditMovementAsync("PettyCashBudget.Adjust", budget, movement, actorUserId);
            return Map(budget, await CalculateReservedAmountAsync(cancellationToken));
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException(
                "Koreksi tidak dapat disimpan karena kolam anggaran atau idempotency key sudah diproses.", exception);
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

    public async Task<PagedResult<PettyCashBudgetResponse>> GetPeriodsAsync(
        PettyCashBudgetPeriodQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinPettyCashBudgets.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim().ToUpperInvariant();
            query = query.Where(x => x.Status == status);
        }

        var total = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(x => x.PeriodStart).ThenByDescending(x => x.CreateDateTime)
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var activeReserved = rows.Any(x => x.Status == PettyCashBudgetStatuses.Active)
            ? await CalculateReservedAmountAsync(cancellationToken)
            : 0m;

        var items = rows
            .Select(x => Map(x, x.Status == PettyCashBudgetStatuses.Active ? activeReserved : 0m))
            .ToList();

        return new PagedResult<PettyCashBudgetResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items
        };
    }

    public async Task<PettyCashBudgetResponse> CreatePeriodAsync(
        CreatePettyCashBudgetPeriodRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var poolCode = string.IsNullOrWhiteSpace(request.PoolCode) ? "HOSPITAL_MAIN" : request.PoolCode.Trim();
        if (request.BudgetAmount <= 0 || decimal.Round(request.BudgetAmount, 2) != request.BudgetAmount)
            throw new PettyCashBudgetValidationException("Plafon anggaran wajib diisi dan harus lebih besar dari nol.");
        if (request.PeriodEnd.HasValue && request.PeriodEnd.Value <= request.PeriodStart)
            throw new PettyCashBudgetValidationException("Tanggal selesai periode harus setelah tanggal mulai.");

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync(cancellationToken);
            }

            var overlaps = await _dbContext.FinPettyCashBudgets.AsNoTracking()
                .Where(x => x.PoolCode == poolCode && !x.IsDelete && x.Status != PettyCashBudgetStatuses.Closed)
                .AnyAsync(x =>
                    request.PeriodStart <= (x.PeriodEnd ?? DateOnly.MaxValue) &&
                    (request.PeriodEnd ?? DateOnly.MaxValue) >= x.PeriodStart,
                    cancellationToken);
            if (overlaps)
                throw new PettyCashBudgetValidationException(
                    "Periode anggaran ini bertabrakan dengan periode yang sudah ada pada kolam yang sama.");

            var period = new FinPettyCashBudget
            {
                PoolCode = poolCode,
                PoolName = string.IsNullOrWhiteSpace(request.PoolName) ? "Kas Kecil Rumah Sakit" : request.PoolName.Trim(),
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                BudgetAmount = request.BudgetAmount,
                CurrentBalance = 0m,
                TotalTopUpAmount = 0m,
                TotalDisbursedAmount = 0m,
                Status = PettyCashBudgetStatuses.Draft,
                RowVersion = Guid.NewGuid(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinPettyCashBudgets.Add(period);
            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditPeriodAsync("PettyCashBudget.CreatePeriod", period, actorUserId);
            return Map(period, 0m);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException("Periode anggaran tidak dapat disimpan.", exception);
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

    public async Task<PettyCashBudgetResponse> ActivatePeriodAsync(
        Guid id, ActivatePettyCashBudgetPeriodRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync(cancellationToken);
            }

            var period = await _dbContext.FinPettyCashBudgets
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Periode anggaran tidak ditemukan.");
            if (period.Status != PettyCashBudgetStatuses.Draft)
                throw new PettyCashBudgetValidationException("Hanya periode berstatus Draf yang dapat diaktifkan.");
            EnsureCurrentRowVersion(period, request.ExpectedRowVersion);

            var alreadyActive = await _dbContext.FinPettyCashBudgets.AsNoTracking()
                .AnyAsync(x => x.PoolCode == period.PoolCode && x.Status == PettyCashBudgetStatuses.Active && !x.IsDelete, cancellationToken);
            if (alreadyActive)
                throw new PettyCashBudgetValidationException(
                    "Masih ada periode anggaran yang aktif pada kolam ini. Tutup periode itu lebih dulu sebelum mengaktifkan yang baru.");

            period.Status = PettyCashBudgetStatuses.Active;
            period.RowVersion = Guid.NewGuid();
            period.UpdateDateTime = DateTime.UtcNow;
            period.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditPeriodAsync("PettyCashBudget.ActivatePeriod", period, actorUserId);
            return Map(period, await CalculateReservedAmountAsync(cancellationToken));
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException(
                "Periode tidak dapat diaktifkan karena sudah ada periode aktif lain pada kolam ini.", exception);
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

    public async Task<PettyCashBudgetResponse> ClosePeriodAsync(
        Guid id, ClosePettyCashBudgetPeriodRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new PettyCashBudgetValidationException("Alasan penutupan wajib diisi.");

        IDbContextTransaction? transaction = null;
        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync(cancellationToken);
            }

            var period = await _dbContext.FinPettyCashBudgets.Include(x => x.Movements)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Periode anggaran tidak ditemukan.");
            if (period.Status != PettyCashBudgetStatuses.Active)
                throw new PettyCashBudgetValidationException("Hanya periode berstatus Aktif yang dapat ditutup.");
            EnsureCurrentRowVersion(period, request.ExpectedRowVersion);

            var pendingVoucherCount = await _dbContext.BilPettyCashVouchers.AsNoTracking()
                .CountAsync(v =>
                    (v.Status == PettyCashVoucherStatuses.WaitingApproval
                        || v.Status == PettyCashVoucherStatuses.Approved
                        || v.Status == PettyCashVoucherStatuses.Requested)
                    && !v.IsCancel && !v.IsDelete, cancellationToken);
            if (pendingVoucherCount > 0)
                throw new PettyCashBudgetValidationException(
                    $"Periode ini belum bisa ditutup: masih ada {pendingVoucherCount} permintaan yang belum dicairkan.");

            var now = DateTimeOffset.UtcNow;
            var remaining = period.CurrentBalance;
            var reason = request.Reason.Trim();

            if (remaining > 0)
            {
                if (request.SuccessorBudgetId is null)
                    throw new PettyCashBudgetValidationException(
                        "Masih ada sisa saldo pada periode ini. Pilih periode penerus untuk menerima sisa saldo tersebut.");

                var successor = await _dbContext.FinPettyCashBudgets
                    .SingleOrDefaultAsync(x => x.Id == request.SuccessorBudgetId.Value && !x.IsDelete, cancellationToken)
                    ?? throw new PettyCashBudgetValidationException("Periode anggaran penerus tidak ditemukan.");
                if (successor.Id == period.Id)
                    throw new PettyCashBudgetValidationException("Periode penerus tidak boleh periode itu sendiri.");
                if (successor.PoolCode != period.PoolCode)
                    throw new PettyCashBudgetValidationException("Periode penerus harus berada pada kolam anggaran yang sama.");
                if (successor.Status != PettyCashBudgetStatuses.Draft && successor.Status != PettyCashBudgetStatuses.Active)
                    throw new PettyCashBudgetValidationException("Periode penerus harus berstatus Draf atau Aktif.");

                var correlationId = Guid.NewGuid();

                var outMovement = new FinPettyCashBudgetMovement
                {
                    BudgetId = period.Id,
                    Budget = period,
                    MovementType = PettyCashBudgetMovementTypes.CarryForwardOut,
                    Amount = remaining,
                    BalanceBefore = remaining,
                    BalanceAfter = 0m,
                    Reason = reason,
                    ActorUserId = actorUserId,
                    CorrelationId = correlationId,
                    OccurredAt = now,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                period.Movements.Add(outMovement);
                _dbContext.FinPettyCashBudgetMovements.Add(outMovement);
                period.CurrentBalance = 0m;

                var successorBefore = successor.CurrentBalance;
                var successorAfter = checked(successorBefore + remaining);
                if (successorAfter > MaxMoneyAmount)
                    throw new PettyCashBudgetValidationException("Saldo periode penerus melebihi batas nominal yang didukung.");

                var inMovement = new FinPettyCashBudgetMovement
                {
                    BudgetId = successor.Id,
                    Budget = successor,
                    MovementType = PettyCashBudgetMovementTypes.CarryForwardIn,
                    Amount = remaining,
                    BalanceBefore = successorBefore,
                    BalanceAfter = successorAfter,
                    Reason = reason,
                    ActorUserId = actorUserId,
                    CorrelationId = correlationId,
                    OccurredAt = now,
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.FinPettyCashBudgetMovements.Add(inMovement);
                successor.CurrentBalance = successorAfter;
                successor.LastMovementAt = now;
                successor.UpdateDateTime = DateTime.UtcNow;
                successor.UpdateBy = actorUserId;

                period.SupersededByBudgetId = successor.Id;
            }

            period.Status = PettyCashBudgetStatuses.Closed;
            period.LastMovementAt = now;
            period.RowVersion = Guid.NewGuid();
            period.UpdateDateTime = DateTime.UtcNow;
            period.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditPeriodAsync("PettyCashBudget.ClosePeriod", period, actorUserId);
            return Map(period, await CalculateReservedAmountAsync(cancellationToken));
        }
        catch (DbUpdateConcurrencyException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.", exception);
        }
        catch (DbUpdateException exception)
        {
            if (transaction is not null) await transaction.RollbackAsync(CancellationToken.None);
            throw new PettyCashBudgetConflictException("Periode tidak dapat ditutup karena data terkait sudah berubah.", exception);
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

    public async Task<decimal> CalculateReservedAmountAsync(CancellationToken cancellationToken)
    {
        var sum = await _dbContext.BilPettyCashVouchers
            .Where(x => x.Status == PettyCashVoucherStatuses.Approved && !x.IsCancel && !x.IsDelete)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken);
        return sum ?? 0m;
    }

    public async Task ApplyDisbursementAsync(
        BilPettyCashVoucher voucher, Guid actorUserId, Guid correlationId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(voucher);

        if (_dbContext.Database.IsRelational()) await AcquireLockAsync(cancellationToken);

        var alreadyDisbursed = await _dbContext.FinPettyCashBudgetMovements.AnyAsync(
            x => x.VoucherId == voucher.Id && x.MovementType == PettyCashBudgetMovementTypes.Disbursement && !x.IsDelete,
            cancellationToken);
        if (alreadyDisbursed)
            throw new PettyCashBudgetConflictException("Uang untuk voucher ini sudah pernah diserahkan.");

        var budget = await LoadActiveBudgetAsync(asNoTracking: false, cancellationToken);
        if (voucher.Amount > budget.CurrentBalance)
            throw new PettyCashBudgetInsufficientBalanceException(
                "Saldo kas kecil tidak mencukupi untuk menyerahkan uang voucher ini. Tambah anggaran terlebih dahulu.");

        var before = budget.CurrentBalance;
        var after = before - voucher.Amount;
        var now = DateTimeOffset.UtcNow;

        var movement = new FinPettyCashBudgetMovement
        {
            BudgetId = budget.Id,
            Budget = budget,
            MovementType = PettyCashBudgetMovementTypes.Disbursement,
            Amount = voucher.Amount,
            BalanceBefore = before,
            BalanceAfter = after,
            VoucherId = voucher.Id,
            Reason = null,
            ActorUserId = actorUserId,
            CorrelationId = correlationId,
            OccurredAt = now,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        budget.Movements.Add(movement);
        _dbContext.FinPettyCashBudgetMovements.Add(movement);
        budget.CurrentBalance = after;
        budget.TotalDisbursedAmount = checked(budget.TotalDisbursedAmount + voucher.Amount);
        budget.LastMovementAt = now;
        budget.RowVersion = Guid.NewGuid();
        budget.UpdateDateTime = DateTime.UtcNow;
        budget.UpdateBy = actorUserId;
    }

    public async Task ApplyReturnAsync(
        BilPettyCashVoucher voucher, decimal amount, Guid actorUserId, Guid correlationId, string reason, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(voucher);

        if (_dbContext.Database.IsRelational()) await AcquireLockAsync(cancellationToken);

        var budget = await LoadActiveBudgetForMovementAsync(cancellationToken);

        var before = budget.CurrentBalance;
        var after = checked(before + amount);
        if (after > MaxMoneyAmount)
            throw new PettyCashBudgetValidationException("Saldo kas kecil melebihi batas nominal yang didukung.");
        var now = DateTimeOffset.UtcNow;

        var movement = new FinPettyCashBudgetMovement
        {
            BudgetId = budget.Id,
            Budget = budget,
            MovementType = PettyCashBudgetMovementTypes.Return,
            Amount = amount,
            BalanceBefore = before,
            BalanceAfter = after,
            VoucherId = voucher.Id,
            Reason = reason,
            ActorUserId = actorUserId,
            CorrelationId = correlationId,
            OccurredAt = now,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        budget.Movements.Add(movement);
        _dbContext.FinPettyCashBudgetMovements.Add(movement);
        budget.CurrentBalance = after;
        budget.LastMovementAt = now;
        budget.RowVersion = Guid.NewGuid();
        budget.UpdateDateTime = DateTime.UtcNow;
        budget.UpdateBy = actorUserId;
    }

    public async Task ApplyReversalAsync(
        BilPettyCashVoucher voucher, decimal outstandingAmount, Guid actorUserId, Guid correlationId, string reason, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(voucher);
        if (outstandingAmount <= 0) return;

        if (_dbContext.Database.IsRelational()) await AcquireLockAsync(cancellationToken);

        var alreadyReversed = await _dbContext.FinPettyCashBudgetMovements.AnyAsync(
            x => x.VoucherId == voucher.Id && x.MovementType == PettyCashBudgetMovementTypes.Reversal && !x.IsDelete,
            cancellationToken);
        if (alreadyReversed)
            throw new PettyCashBudgetValidationException("Pencairan ini sudah pernah dibatalkan.");

        var budget = await LoadActiveBudgetForMovementAsync(cancellationToken);

        var before = budget.CurrentBalance;
        var after = checked(before + outstandingAmount);
        if (after > MaxMoneyAmount)
            throw new PettyCashBudgetValidationException("Saldo kas kecil melebihi batas nominal yang didukung.");
        var now = DateTimeOffset.UtcNow;

        var movement = new FinPettyCashBudgetMovement
        {
            BudgetId = budget.Id,
            Budget = budget,
            MovementType = PettyCashBudgetMovementTypes.Reversal,
            Amount = outstandingAmount,
            BalanceBefore = before,
            BalanceAfter = after,
            VoucherId = voucher.Id,
            Reason = reason,
            ActorUserId = actorUserId,
            CorrelationId = correlationId,
            OccurredAt = now,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        budget.Movements.Add(movement);
        _dbContext.FinPettyCashBudgetMovements.Add(movement);
        budget.CurrentBalance = after;
        budget.TotalDisbursedAmount = Math.Max(0m, budget.TotalDisbursedAmount - outstandingAmount);
        budget.LastMovementAt = now;
        budget.RowVersion = Guid.NewGuid();
        budget.UpdateDateTime = DateTime.UtcNow;
        budget.UpdateBy = actorUserId;
    }

    /// <summary>
    /// Requirement 8: Pembalikan saldo saat voucher pencairan dibatalkan.
    /// Saldo kembali bertambah, pemakaian saldo berkurang, movement tercatat, audit trail terjaga.
    /// </summary>
    public async Task ApplyCancellationReversalAsync(
        BilPettyCashVoucher voucher, Guid actorUserId, Guid correlationId, string? reason, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(voucher);

        if (_dbContext.Database.IsRelational()) await AcquireLockAsync(cancellationToken);

        var budget = await LoadActiveBudgetForMovementAsync(cancellationToken);

        var before = budget.CurrentBalance;
        var after = checked(before + voucher.Amount);
        if (after > MaxMoneyAmount)
            throw new PettyCashBudgetValidationException("Saldo kas kecil melebihi batas nominal yang didukung.");
        var now = DateTimeOffset.UtcNow;

        var movement = new FinPettyCashBudgetMovement
        {
            BudgetId = budget.Id,
            Budget = budget,
            MovementType = PettyCashBudgetMovementTypes.Reversal,
            Amount = voucher.Amount,
            BalanceBefore = before,
            BalanceAfter = after,
            VoucherId = voucher.Id,
            Reason = string.IsNullOrWhiteSpace(reason) ? "Pembatalan voucher kas kecil" : reason.Trim(),
            ActorUserId = actorUserId,
            CorrelationId = correlationId,
            OccurredAt = now,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        budget.Movements.Add(movement);
        _dbContext.FinPettyCashBudgetMovements.Add(movement);

        budget.CurrentBalance = after;
        budget.TotalDisbursedAmount = Math.Max(0m, budget.TotalDisbursedAmount - voucher.Amount);
        budget.LastMovementAt = now;
        budget.RowVersion = Guid.NewGuid();
        budget.UpdateDateTime = DateTime.UtcNow;
        budget.UpdateBy = actorUserId;
    }

    public Task AcquireLockAsync(CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [BudgetLockKey], cancellationToken);

    private async Task<FinPettyCashBudget> LoadActiveBudgetAsync(bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking
            ? _dbContext.FinPettyCashBudgets.AsNoTracking()
            : _dbContext.FinPettyCashBudgets.Include(x => x.Movements);

        // Prefer active budget, then default HOSPITAL_MAIN pool, then any non-deleted budget
        var budget = await query.FirstOrDefaultAsync(x => x.Status == PettyCashBudgetStatuses.Active && !x.IsDelete, cancellationToken)
            ?? await query.FirstOrDefaultAsync(x => x.PoolCode == "HOSPITAL_MAIN" && !x.IsDelete, cancellationToken)
            ?? await query.FirstOrDefaultAsync(x => !x.IsDelete, cancellationToken);

        return budget ?? throw new KeyNotFoundException("Kolam kas kecil tidak ditemukan.");
    }

    private async Task<FinPettyCashBudget> LoadActiveBudgetForMovementAsync(CancellationToken cancellationToken)
    {
        try { return await LoadActiveBudgetAsync(asNoTracking: false, cancellationToken); }
        catch (KeyNotFoundException)
        {
            throw new PettyCashBudgetValidationException(
                "Kolam kas kecil belum tersedia. Finance perlu membuat saldo kas kecil terlebih dulu.");
        }
    }

    private static void EnsureCurrentRowVersion(FinPettyCashBudget budget, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty)
            throw new PettyCashBudgetValidationException("ExpectedRowVersion wajib diisi.");
        if (budget.RowVersion != expectedRowVersion)
            throw new PettyCashBudgetConflictException("Data telah berubah. Muat ulang sebelum melanjutkan.");
    }

    private static void ValidateAmountAndReason(decimal amount, string? reason)
    {
        const string message = "Nominal dan alasan wajib diisi, dan nominal harus lebih besar dari nol.";
        if (amount <= 0 || decimal.Round(amount, 2) != amount) throw new PettyCashBudgetValidationException(message);
        if (string.IsNullOrWhiteSpace(reason)) throw new PettyCashBudgetValidationException(message);
        if (reason.Trim().Length > 500) throw new PettyCashBudgetValidationException(message);
    }

    private static string NormalizeDirection(string? direction)
    {
        var normalized = (direction ?? string.Empty).Trim().ToUpperInvariant();
        if (!PettyCashBudgetAdjustmentDirections.All.Contains(normalized))
            throw new PettyCashBudgetValidationException("Direction harus INCREASE atau DECREASE.");
        return normalized;
    }

    private static string NormalizeAndValidateFundingSource(string? fundingSourceType, string? transferReference)
    {
        if (string.IsNullOrWhiteSpace(fundingSourceType))
            throw new PettyCashBudgetValidationException("Sumber dana penambahan anggaran wajib diisi (TRANSFER atau CASH).");

        var normalized = fundingSourceType.Trim().ToUpperInvariant();
        if (!PettyCashFundingSourceTypes.All.Contains(normalized))
            throw new PettyCashBudgetValidationException("Sumber dana harus bernilai TRANSFER atau CASH.");

        if (normalized == PettyCashFundingSourceTypes.Transfer)
        {
            if (string.IsNullOrWhiteSpace(transferReference))
                throw new PettyCashBudgetValidationException("Nomor referensi transfer wajib diisi untuk sumber dana TRANSFER.");
            if (transferReference.Trim().Length > 100)
                throw new PettyCashBudgetValidationException("Nomor referensi transfer tidak boleh melebihi 100 karakter.");
        }

        return normalized;
    }

    private Task AuditMovementAsync(string action, FinPettyCashBudget budget, FinPettyCashBudgetMovement movement, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Pergerakan anggaran kas kecil dicatat.", new
        {
            BudgetId = budget.Id,
            budget.PoolCode,
            MovementId = movement.Id,
            movement.MovementType,
            movement.Amount,
            movement.BalanceBefore,
            movement.BalanceAfter,
            movement.FundingSourceType,
            movement.TransferReference,
            movement.IdempotencyKey,
            movement.CorrelationId,
            ActorUserId = actorUserId
        });

    private Task AuditPeriodAsync(string action, FinPettyCashBudget period, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Periode anggaran kas kecil diperbarui.", new
        {
            BudgetId = period.Id,
            period.PoolCode,
            period.PeriodStart,
            period.PeriodEnd,
            period.BudgetAmount,
            period.Status,
            period.SupersededByBudgetId,
            ActorUserId = actorUserId
        });

    private static PettyCashBudgetResponse Map(FinPettyCashBudget budget, decimal reserved)
    {
        var pemakaianSaldo = budget.TotalDisbursedAmount;
        var sisaSaldo = budget.CurrentBalance;

        return new()
        {
            Id = budget.Id,
            PoolCode = budget.PoolCode,
            PoolName = budget.PoolName,
            PeriodStart = budget.PeriodStart,
            PeriodEnd = budget.PeriodEnd,
            BudgetAmount = budget.BudgetAmount,
            CurrentBalance = budget.CurrentBalance,
            PemakaianSaldo = pemakaianSaldo,
            DisbursedBalance = pemakaianSaldo,
            TotalDisbursedAmount = pemakaianSaldo,
            SisaSaldo = sisaSaldo,
            RemainingBalance = sisaSaldo,
            RemainingBudgetAmount = sisaSaldo,
            Status = budget.Status,
            SupersededByBudgetId = budget.SupersededByBudgetId,
            ReservedAmount = reserved,
            AvailableAmount = budget.CurrentBalance - reserved,
            TotalTopUpAmount = budget.TotalTopUpAmount,
            LastMovementAt = budget.LastMovementAt,
            RowVersion = budget.RowVersion
        };
    }
}

public sealed class PettyCashBudgetValidationException(string message) : Exception(message);

public sealed class PettyCashBudgetInsufficientBalanceException(string message) : Exception(message);

public sealed class PettyCashBudgetConflictException : Exception
{
    public PettyCashBudgetConflictException(string message) : base(message) { }
    public PettyCashBudgetConflictException(string message, Exception innerException) : base(message, innerException) { }
}

