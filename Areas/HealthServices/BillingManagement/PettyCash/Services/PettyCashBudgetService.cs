using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

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

    // BE-BKC-058, PC-DES-025: satu panggilan untuk seluruh kartu ringkasan halaman gabungan.
    // Reuse GetCurrentAsync untuk anggaran/saldo/sisa, ditambah dua hitungan status voucher
    // yang dijumlah langsung dari ApplicationDbContext (pola sama dengan
    // PettyCashVoucherService.GetSummaryAsync) — MUST NOT menyuntikkan PettyCashVoucherService
    // ke sini karena arahnya sudah terbalik (VoucherService yang bergantung ke BudgetService).
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
            TotalDisbursedThisPeriod = activeBudget.TotalDisbursedAmount
        };
    }

    public async Task<PagedResult<PettyCashBudgetMovementResponse>> GetMovementsAsync(
        PettyCashBudgetMovementQuery request, CancellationToken cancellationToken)
    {
        // BudgetId eksplisit membuka riwayat periode manapun, termasuk yang sudah Ditutup;
        // kosong tetap jatuh ke periode Aktif seperti perilaku sebelum revisi ini (PC-DES-017).
        Guid budgetId;
        if (request.BudgetId.HasValue)
        {
            budgetId = request.BudgetId.Value;
            var exists = await _dbContext.BilPettyCashBudgets.AsNoTracking()
                .AnyAsync(x => x.Id == budgetId && !x.IsDelete, cancellationToken);
            if (!exists) throw new KeyNotFoundException("Periode anggaran kas kecil tidak ditemukan.");
        }
        else
        {
            var budget = await LoadActiveBudgetAsync(asNoTracking: true, cancellationToken);
            budgetId = budget.Id;
        }

        var query = _dbContext.BilPettyCashBudgetMovements.AsNoTracking()
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

            var before = budget.CurrentBalance;
            var after = direction == PettyCashBudgetAdjustmentDirections.Decrease
                ? before - request.Amount
                : checked(before + request.Amount);

            // PC-DES-016: penjaga "tidak boleh di bawah komitmen berjalan" dicabut bersama
            // mekanisme ReservedAmount — satu-satunya syarat yang tersisa adalah saldo tidak
            // boleh negatif (BIL-VAL-054, dipersempit).
            if (direction == PettyCashBudgetAdjustmentDirections.Decrease && after < 0)
                throw new PettyCashBudgetInsufficientBalanceException(
                    "Koreksi ini akan membuat saldo kas kecil menjadi negatif.");
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
        var query = _dbContext.BilPettyCashBudgets.AsNoTracking().Where(x => !x.IsDelete);
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

        // ReservedAmount (warisan, PC-DES-016) hanya bermakna untuk periode Aktif — lihat
        // catatan pada PettyCashBudgetResponse.ReservedAmount. Periode lain diberi 0 karena
        // tidak ada voucher yang dapat menjanjikan diri atas periode yang bukan sedang berjalan.
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

    // PC-DES-017. Membuat periode BARU berstatus Draf — belum menerima pergerakan uang apa
    // pun sampai diaktifkan (PC-DES-017, FR-BKC-092).
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

            // BIL-VAL-102: periode baru MUST NOT tumpang tindih periode lain pada kolam yang
            // sama, kecuali periode yang sudah Ditutup (riwayatnya boleh tumpang tindih tanggal
            // dengan periode aktif berikutnya bila keduanya sengaja dibuat begitu — yang
            // dicegah hanya dua periode yang SAMA-SAMA masih berlaku pada tanggal yang sama).
            var overlaps = await _dbContext.BilPettyCashBudgets.AsNoTracking()
                .Where(x => x.PoolCode == poolCode && !x.IsDelete && x.Status != PettyCashBudgetStatuses.Closed)
                .AnyAsync(x =>
                    request.PeriodStart <= (x.PeriodEnd ?? DateOnly.MaxValue) &&
                    (request.PeriodEnd ?? DateOnly.MaxValue) >= x.PeriodStart,
                    cancellationToken);
            if (overlaps)
                throw new PettyCashBudgetValidationException(
                    "Periode anggaran ini bertabrakan dengan periode yang sudah ada pada kolam yang sama.");

            var period = new BilPettyCashBudget
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
            _dbContext.BilPettyCashBudgets.Add(period);
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

    // PC-DES-017, BIL-VAL-103. Paling banyak satu periode Aktif per kolam pada satu waktu —
    // ditegakkan di sini DAN oleh unique index parsial IX_BilPettyCashBudget_ActivePerPool,
    // supaya invariant tetap tegak walau dua permintaan aktivasi lolos kunci penasihat
    // bersamaan (jaring pengaman kedua, pola sama dengan PC-DES-006).
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

            var period = await _dbContext.BilPettyCashBudgets
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Periode anggaran tidak ditemukan.");
            if (period.Status != PettyCashBudgetStatuses.Draft)
                throw new PettyCashBudgetValidationException("Hanya periode berstatus Draf yang dapat diaktifkan.");
            EnsureCurrentRowVersion(period, request.ExpectedRowVersion);

            var alreadyActive = await _dbContext.BilPettyCashBudgets.AsNoTracking()
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

    // PC-DES-018. Menutup periode Aktif dan memindahkan sisa saldonya ke periode penerus
    // sebagai DUA baris ledger dalam satu transaction — CarryForwardOut pada periode ini,
    // CarryForwardIn pada periode penerus. Ditolak bila masih ada permintaan yang belum
    // dicairkan (BIL-VAL-104): "belum dicairkan" diperiksa terhadap kosakata status LAMA
    // (WaitingApproval/Approved) DAN BARU (Requested) sekaligus, karena PettyCashVoucherService
    // — di luar scope task ini — masih menulis kosakata lama sampai BE-BKC-055.
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

            var period = await _dbContext.BilPettyCashBudgets.Include(x => x.Movements)
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

                var successor = await _dbContext.BilPettyCashBudgets
                    .SingleOrDefaultAsync(x => x.Id == request.SuccessorBudgetId.Value && !x.IsDelete, cancellationToken)
                    ?? throw new PettyCashBudgetValidationException("Periode anggaran penerus tidak ditemukan.");
                if (successor.Id == period.Id)
                    throw new PettyCashBudgetValidationException("Periode penerus tidak boleh periode itu sendiri.");
                if (successor.PoolCode != period.PoolCode)
                    throw new PettyCashBudgetValidationException("Periode penerus harus berada pada kolam anggaran yang sama.");
                if (successor.Status != PettyCashBudgetStatuses.Draft && successor.Status != PettyCashBudgetStatuses.Active)
                    throw new PettyCashBudgetValidationException("Periode penerus harus berstatus Draf atau Aktif.");

                var correlationId = Guid.NewGuid();

                var outMovement = new BilPettyCashBudgetMovement
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
                _dbContext.BilPettyCashBudgetMovements.Add(outMovement);
                period.CurrentBalance = 0m;

                var successorBefore = successor.CurrentBalance;
                var successorAfter = checked(successorBefore + remaining);
                if (successorAfter > MaxMoneyAmount)
                    throw new PettyCashBudgetValidationException("Saldo periode penerus melebihi batas nominal yang didukung.");

                var inMovement = new BilPettyCashBudgetMovement
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
                _dbContext.BilPettyCashBudgetMovements.Add(inMovement);
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

    // PC-DES-005: dijumlah dari voucher APPROVED (uang belum diserahkan tetapi sudah dijanjikan).
    // MUST dihitung di sini, MUST NOT dihitung ulang di layar.
    // Warisan pra-revisi 15 September 2026 (PC-DES-016 mencabut mekanisme ini dari operasi
    // milik service ini sendiri) — method ini TETAP ADA karena PettyCashVoucherService (di
    // luar scope BE-BKC-054) masih memanggilnya lewat GetCurrentAsync().AvailableAmount pada
    // ApproveAsync. Dihapus penuh saat BE-BKC-055 menyentuh service tersebut.
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

    // BE-BKC-057, PC-DES-019, BIL-VAL-098, BIL-VAL-106: dipanggil dari dalam transaction milik
    // PettyCashVoucherService.ReturnAsync (pola sama dengan ApplyDisbursementAsync). Pemanggil
    // MUST sudah memvalidasi status voucher dan invariant "total pengembalian tidak melampaui
    // Amount voucher" (BIL-VAL-098) sebelum memanggil ini — method ini hanya menulis sisi saldo.
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

        var movement = new BilPettyCashBudgetMovement
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
        _dbContext.BilPettyCashBudgetMovements.Add(movement);
        budget.CurrentBalance = after;
        budget.LastMovementAt = now;
        budget.RowVersion = Guid.NewGuid();
        budget.UpdateDateTime = DateTime.UtcNow;
        budget.UpdateBy = actorUserId;
    }

    // BE-BKC-057, PC-DES-020, BIL-VAL-099, BIL-VAL-106: dipanggil dari dalam transaction milik
    // PettyCashVoucherService.ReverseAsync. "outstandingAmount" (Amount − ReturnedAmount) MUST
    // sudah dihitung pemanggil sesaat sebelum panggilan ini, dari voucher yang sedang dikunci
    // penasihat per-voucher yang sama (PC-DES-011) — bukan nominal dari pengguna.
    // "alreadyReversed" adalah jaring pengaman kedua; gerbang utama "belum pernah dibalik"
    // sudah ditegakkan struktural oleh ChangeVoucherAsync (voucher REVERSED ditolak untuk
    // seluruh aksi, BIL-VAL-100), dipasangkan dengan unique index parsial
    // IX_BilPettyCashBudgetMovement_Voucher_Reversal — sama seperti pola alreadyDisbursed pada
    // ApplyDisbursementAsync.
    public async Task ApplyReversalAsync(
        BilPettyCashVoucher voucher, decimal outstandingAmount, Guid actorUserId, Guid correlationId, string reason, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(voucher);
        // Sisa sudah nol berarti seluruh nominal sudah kembali lebih dulu lewat RETURN — tidak
        // ada apa pun yang perlu dipindahkan ke saldo, dan CK_BilPettyCashBudgetMovement_Amount
        // ("Amount" > 0) melarang baris ledger bernilai nol. Voucher tetap berpindah ke REVERSED
        // oleh pemanggil walau tanpa baris REVERSAL di sini.
        if (outstandingAmount <= 0) return;

        if (_dbContext.Database.IsRelational()) await AcquireLockAsync(cancellationToken);

        var alreadyReversed = await _dbContext.BilPettyCashBudgetMovements.AnyAsync(
            x => x.VoucherId == voucher.Id && x.MovementType == PettyCashBudgetMovementTypes.Reversal && !x.IsDelete,
            cancellationToken);
        if (alreadyReversed)
            // BIL-VAL-099
            throw new PettyCashBudgetValidationException("Pencairan ini sudah pernah dibatalkan.");

        var budget = await LoadActiveBudgetForMovementAsync(cancellationToken);

        var before = budget.CurrentBalance;
        var after = checked(before + outstandingAmount);
        if (after > MaxMoneyAmount)
            throw new PettyCashBudgetValidationException("Saldo kas kecil melebihi batas nominal yang didukung.");
        var now = DateTimeOffset.UtcNow;

        var movement = new BilPettyCashBudgetMovement
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
        _dbContext.BilPettyCashBudgetMovements.Add(movement);
        budget.CurrentBalance = after;
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

    // BE-BKC-057, BIL-VAL-106: pembungkus LoadActiveBudgetAsync khusus Return/Reversal supaya
    // ketiadaan periode aktif menghasilkan 422 sesuai kontrak amendment 15 September 2026,
    // bukan 404 seperti ApplyDisbursementAsync (gap pra-existing pada BE-BKC-054/055, dicatat
    // pada laporan task — bukan diperbaiki di sini karena mengubah perilaku endpoint lain).
    private async Task<BilPettyCashBudget> LoadActiveBudgetForMovementAsync(CancellationToken cancellationToken)
    {
        try { return await LoadActiveBudgetAsync(asNoTracking: false, cancellationToken); }
        catch (KeyNotFoundException)
        {
            // BIL-VAL-106
            throw new PettyCashBudgetValidationException(
                "Belum ada periode anggaran yang aktif. Finance perlu membuat dan mengaktifkan periode anggaran lebih dulu.");
        }
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

    // BIL-AT-076/§ Security — Reason SENSITIF (data-dictionary.md), MUST NOT masuk custom logger.
    private Task AuditPeriodAsync(string action, BilPettyCashBudget period, Guid actorUserId) =>
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

    private static PettyCashBudgetResponse Map(BilPettyCashBudget budget, decimal reserved) => new()
    {
        Id = budget.Id,
        PoolCode = budget.PoolCode,
        PoolName = budget.PoolName,
        PeriodStart = budget.PeriodStart,
        PeriodEnd = budget.PeriodEnd,
        BudgetAmount = budget.BudgetAmount,
        RemainingBudgetAmount = budget.BudgetAmount - budget.TotalDisbursedAmount,
        Status = budget.Status,
        SupersededByBudgetId = budget.SupersededByBudgetId,
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
