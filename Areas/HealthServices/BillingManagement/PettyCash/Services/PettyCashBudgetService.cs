using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;
using System.Globalization;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;

// BE-BKC-036 / PC-DES-004,005,006,011,014: satu-satunya penulis BilPettyCashBudget.CurrentBalance.
// TopUpAsync dan AdjustAsync membuka transaction sendiri; ApplyDisbursementAsync sengaja TIDAK,
// karena ia MUST dipanggil dari dalam transaction milik PettyCashVoucherService (BE-BKC-037).
public sealed class PettyCashBudgetService
{
    private const string LogCategory = "HealthServices.BillingManagement.PettyCash";
    private const string BudgetLockKey = "BIL_PETTY_CASH_BUDGET_HOSPITAL_MAIN";
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

    public async Task<PagedResult<PettyCashBudgetMovementResponse>> GetMovementsAsync(
        PettyCashBudgetMovementQuery request, CancellationToken cancellationToken)
    {
        var budget = await LoadActiveBudgetAsync(asNoTracking: true, cancellationToken);
        var query = _dbContext.BilPettyCashBudgetMovements.AsNoTracking()
            .Where(x => x.BudgetId == budget.Id && !x.IsDelete);

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
                x.VoucherId, x.Reason, x.ActorUserId, x.OccurredAt
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
        IDbContextTransaction? transaction = null;

        try
        {
            if (_dbContext.Database.IsRelational())
            {
                transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
                await AcquireLockAsync(cancellationToken);
            }

            var priorMovement = await _dbContext.BilPettyCashBudgetMovements.AsNoTracking()
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

            var movement = new BilPettyCashBudgetMovement
            {
                BudgetId = budget.Id,
                Budget = budget,
                MovementType = PettyCashBudgetMovementTypes.TopUp,
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
            _dbContext.BilPettyCashBudgetMovements.Add(movement);
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

            var priorMovement = await _dbContext.BilPettyCashBudgetMovements.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey, cancellationToken);
            if (priorMovement is not null)
            {
                if (transaction is not null) await transaction.CommitAsync(cancellationToken);
                var replayBudget = await LoadActiveBudgetAsync(asNoTracking: true, cancellationToken);
                return Map(replayBudget, await CalculateReservedAmountAsync(cancellationToken));
            }

            var budget = await LoadActiveBudgetAsync(asNoTracking: false, cancellationToken);
            EnsureCurrentRowVersion(budget, request.ExpectedRowVersion);
            var reserved = await CalculateReservedAmountAsync(cancellationToken);

            var before = budget.CurrentBalance;
            var after = direction == PettyCashBudgetAdjustmentDirections.Decrease
                ? before - request.Amount
                : checked(before + request.Amount);

            if (direction == PettyCashBudgetAdjustmentDirections.Decrease && (after < 0 || after < reserved))
                throw new PettyCashBudgetInsufficientBalanceException(
                    $"Koreksi ini akan membuat saldo kas kecil tidak mencukupi untuk voucher yang sudah disetujui. Sisa yang sudah dijanjikan Rp {reserved.ToString("N0", CultureInfo.InvariantCulture)}.");
            if (after > MaxMoneyAmount)
                throw new PettyCashBudgetValidationException("Saldo kas kecil melebihi batas nominal yang didukung.");
            var now = DateTimeOffset.UtcNow;

            var movement = new BilPettyCashBudgetMovement
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
            _dbContext.BilPettyCashBudgetMovements.Add(movement);
            budget.CurrentBalance = after;
            budget.LastMovementAt = now;
            budget.RowVersion = Guid.NewGuid();
            budget.UpdateDateTime = DateTime.UtcNow;
            budget.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            if (transaction is not null) await transaction.CommitAsync(cancellationToken);
            await AuditMovementAsync("PettyCashBudget.Adjust", budget, movement, actorUserId);
            return Map(budget, reserved);
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

    // PC-DES-005: dijumlah dari voucher APPROVED (uang belum diserahkan tetapi sudah dijanjikan).
    // MUST dihitung di sini, MUST NOT dihitung ulang di layar.
    public async Task<decimal> CalculateReservedAmountAsync(CancellationToken cancellationToken)
    {
        var sum = await _dbContext.BilPettyCashVouchers
            .Where(x => x.Status == PettyCashVoucherStatuses.Approved && !x.IsCancel && !x.IsDelete)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken);
        return sum ?? 0m;
    }

    // PC-DES-004: MUST NOT membuka transaction sendiri — pemanggil (PettyCashVoucherService,
    // BE-BKC-037) MUST sudah berada di dalam transaction dan MUST memanggil SaveChangesAsync-nya
    // sendiri setelah method ini kembali. BIL-VAL-048 ditegakkan di sini karena saldo bisa saja
    // sudah berubah sejak voucher disetujui.
    public async Task ApplyDisbursementAsync(
        BilPettyCashVoucher voucher, Guid actorUserId, Guid correlationId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(voucher);

        if (_dbContext.Database.IsRelational()) await AcquireLockAsync(cancellationToken);

        var alreadyDisbursed = await _dbContext.BilPettyCashBudgetMovements.AnyAsync(
            x => x.VoucherId == voucher.Id && x.MovementType == PettyCashBudgetMovementTypes.Disbursement && !x.IsDelete,
            cancellationToken);
        if (alreadyDisbursed)
            // BIL-VAL-057
            throw new PettyCashBudgetConflictException("Uang untuk voucher ini sudah pernah diserahkan.");

        var budget = await LoadActiveBudgetAsync(asNoTracking: false, cancellationToken);
        if (voucher.Amount > budget.CurrentBalance)
            throw new PettyCashBudgetInsufficientBalanceException(
                "Saldo kas kecil tidak mencukupi untuk menyerahkan uang voucher ini. Tambah anggaran terlebih dahulu.");

        var before = budget.CurrentBalance;
        var after = before - voucher.Amount;
        var now = DateTimeOffset.UtcNow;

        var movement = new BilPettyCashBudgetMovement
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
        _dbContext.BilPettyCashBudgetMovements.Add(movement);
        budget.CurrentBalance = after;
        budget.TotalDisbursedAmount = checked(budget.TotalDisbursedAmount + voucher.Amount);
        budget.LastMovementAt = now;
        budget.RowVersion = Guid.NewGuid();
        budget.UpdateDateTime = DateTime.UtcNow;
        budget.UpdateBy = actorUserId;
    }

    // Public: BE-BKC-037 (PettyCashVoucherService.ApproveAsync) MUST mengambil kunci yang sama
    // sebelum membaca CalculateReservedAmountAsync/GetCurrentAsync, supaya dua persetujuan
    // bersamaan tidak sama-sama lolos memeriksa komitmen yang belum saling terlihat (PC-DES-005).
    public Task AcquireLockAsync(CancellationToken cancellationToken) =>
        _dbContext.Database.ExecuteSqlRawAsync(
            "SELECT pg_advisory_xact_lock(hashtext({0}));", [BudgetLockKey], cancellationToken);

    private async Task<BilPettyCashBudget> LoadActiveBudgetAsync(bool asNoTracking, CancellationToken cancellationToken)
    {
        var query = asNoTracking
            ? _dbContext.BilPettyCashBudgets.AsNoTracking()
            : _dbContext.BilPettyCashBudgets.Include(x => x.Movements);
        return await query.SingleOrDefaultAsync(x => x.Status == PettyCashBudgetStatuses.Active && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Kolam anggaran kas kecil aktif tidak ditemukan.");
    }

    private static void EnsureCurrentRowVersion(BilPettyCashBudget budget, Guid expectedRowVersion)
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

    // BIL-AT-076/§ Security — Reason SENSITIF (data-dictionary.md), MUST NOT masuk custom logger.
    private Task AuditMovementAsync(string action, BilPettyCashBudget budget, BilPettyCashBudgetMovement movement, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, action, "Pergerakan anggaran kas kecil dicatat.", new
        {
            BudgetId = budget.Id,
            budget.PoolCode,
            MovementId = movement.Id,
            movement.MovementType,
            movement.Amount,
            movement.BalanceBefore,
            movement.BalanceAfter,
            movement.IdempotencyKey,
            movement.CorrelationId,
            ActorUserId = actorUserId
        });

    private static PettyCashBudgetResponse Map(BilPettyCashBudget budget, decimal reserved) => new()
    {
        Id = budget.Id,
        PoolCode = budget.PoolCode,
        PoolName = budget.PoolName,
        CurrentBalance = budget.CurrentBalance,
        ReservedAmount = reserved,
        AvailableAmount = budget.CurrentBalance - reserved,
        TotalTopUpAmount = budget.TotalTopUpAmount,
        TotalDisbursedAmount = budget.TotalDisbursedAmount,
        LastMovementAt = budget.LastMovementAt,
        RowVersion = budget.RowVersion
    };
}

public sealed class PettyCashBudgetValidationException(string message) : Exception(message);

public sealed class PettyCashBudgetInsufficientBalanceException(string message) : Exception(message);

public sealed class PettyCashBudgetConflictException : Exception
{
    public PettyCashBudgetConflictException(string message) : base(message) { }
    public PettyCashBudgetConflictException(string message, Exception innerException) : base(message, innerException) { }
}
