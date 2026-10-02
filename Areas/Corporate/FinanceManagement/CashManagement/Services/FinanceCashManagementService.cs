using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Services;

/// <summary>
/// Layanan transaksi kas harian dan setoran bank ke rekening rumah sakit (FIN-DES-018, FIN-DES-021, FIN-DES-022).
/// Mengatur siklus hidup FinBankDeposit (DRAFT -> POSTED -> VERIFIED, atau CANCELLED),
/// menghitung saldo kas tersedia dari kasir (BilCashierShift, FIN-CAP-006) secara serializable saat posting,
/// dan mengelola penutupan kas harian (FinDailyCashSnapshot, CLOSED dibekukan).
///
/// Kas kecil (Petty Cash) TIDAK memengaruhi kas kasir (FIN-DEC-020, FR-FIN-063, UAT-16).
/// </summary>
public sealed class FinanceCashManagementService
{
    private const string LogCategory = "Corporate.FinanceManagement.CashManagement";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public FinanceCashManagementService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------
    // Setoran Bank (FinBankDeposit) — Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<BankDepositResponse>> GetBankDepositsPagedAsync(
        BankDepositQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinBankDeposits.AsNoTracking()
            .Include(x => x.BankAccount).ThenInclude(b => b!.Bank)
            .Where(x => !x.IsDelete);

        if (request.BankAccountId.HasValue)
            query = query.Where(x => x.BankAccountId == request.BankAccountId.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status);

        if (request.DepositDateFrom.HasValue)
            query = query.Where(x => x.DepositDate >= request.DepositDateFrom.Value);

        if (request.DepositDateTo.HasValue)
            query = query.Where(x => x.DepositDate <= request.DepositDateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x =>
                x.DepositNumber.ToUpper().Contains(search) ||
                (x.DepositSlipNumber != null && x.DepositSlipNumber.ToUpper().Contains(search)));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "depositnumber" => descending ? query.OrderByDescending(x => x.DepositNumber) : query.OrderBy(x => x.DepositNumber),
            "amount" => descending ? query.OrderByDescending(x => x.Amount) : query.OrderBy(x => x.Amount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.DepositDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.DepositDate).ThenBy(x => x.CreateDateTime)
        };

        var total = await query.CountAsync(cancellationToken);
        var rawItems = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var shiftIds = rawItems.Where(x => x.CashierShiftId.HasValue).Select(x => x.CashierShiftId!.Value).Distinct().ToList();
        var shiftDict = shiftIds.Count != 0
            ? await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(s => shiftIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.ShiftNumber, cancellationToken)
            : [];

        var items = rawItems.Select(x => MapDeposit(x, shiftDict)).ToList();

        return new PagedResult<BankDepositResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<BankDepositResponse> GetBankDepositByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var deposit = await _dbContext.FinBankDeposits.AsNoTracking()
            .Include(x => x.BankAccount).ThenInclude(b => b!.Bank)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Setoran bank tidak ditemukan.");

        string? shiftNumber = null;
        if (deposit.CashierShiftId.HasValue)
        {
            shiftNumber = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(s => s.Id == deposit.CashierShiftId.Value)
                .Select(s => s.ShiftNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return MapDeposit(deposit, shiftNumber);
    }

    // ------------------------------------------------------------------------------------
    // Saldo Kas Tersedia (FIN-DEC-017, FR-FIN-060..062)
    // ------------------------------------------------------------------------------------

    public async Task<AvailableCashBalanceResponse> GetAvailableCashBalanceAsync(
        DateOnly? date, Guid? cashierShiftId, CancellationToken cancellationToken)
    {
        var effectiveDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        decimal totalCashierCash = 0m;

        if (cashierShiftId.HasValue)
        {
            var shift = await _dbContext.BilCashierShifts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == cashierShiftId.Value && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");
            totalCashierCash = GetShiftCash(shift);
        }
        else
        {
            var startOfDay = new DateTimeOffset(effectiveDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1);
            var shifts = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(x => !x.IsDelete && x.OpenedAt >= startOfDay && x.OpenedAt < endOfDay)
                .ToListAsync(cancellationToken);
            totalCashierCash = shifts.Sum(GetShiftCash);
        }

        var depositQuery = _dbContext.FinBankDeposits.AsNoTracking()
            .Where(x => !x.IsDelete && (x.Status == FinBankDepositStatuses.Posted || x.Status == FinBankDepositStatuses.Verified));

        if (cashierShiftId.HasValue)
            depositQuery = depositQuery.Where(x => x.CashierShiftId == cashierShiftId.Value);
        else
            depositQuery = depositQuery.Where(x => x.DepositDate == effectiveDate);

        var totalDeposited = await depositQuery.SumAsync(x => x.Amount, cancellationToken);
        var available = Math.Max(0m, totalCashierCash - totalDeposited);

        return new AvailableCashBalanceResponse
        {
            Date = effectiveDate,
            CashierShiftId = cashierShiftId,
            TotalCashierCash = totalCashierCash,
            TotalDepositedCash = totalDeposited,
            AvailableBalance = available
        };
    }

    // ------------------------------------------------------------------------------------
    // Setoran Bank (FinBankDeposit) — Perubahan Status & Mutasi
    // ------------------------------------------------------------------------------------

    public async Task<BankDepositResponse> CreateBankDepositAsync(
        CreateBankDepositRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (request.Amount <= 0)
            throw new CashBadRequestException("Nominal setoran harus lebih besar dari nol.");

        var bankAccount = await _dbContext.MstBankAccounts.AsNoTracking()
            .Include(x => x.Bank)
            .SingleOrDefaultAsync(x => x.Id == request.BankAccountId && !x.IsDelete && x.IsActive, cancellationToken)
            ?? throw new KeyNotFoundException("Rekening bank tidak ditemukan atau tidak aktif.");

        var snapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
            .SingleOrDefaultAsync(x => x.CashDate == request.DepositDate && !x.IsDelete, cancellationToken);
        if (snapshot is not null && snapshot.Status == FinDailyCashSnapshotStatuses.Closed)
            throw new CashValidationException("Kas tanggal ini sudah ditutup dan tidak dapat diubah.");

        string? shiftNumber = null;
        if (request.CashierShiftId.HasValue)
        {
            var shift = await _dbContext.BilCashierShifts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == request.CashierShiftId.Value && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Shift kasir tidak ditemukan.");
            shiftNumber = shift.ShiftNumber;
        }

        var deposit = new FinBankDeposit
        {
            DepositNumber = GenerateDepositNumber(request.DepositDate),
            DepositDate = request.DepositDate,
            BankAccountId = request.BankAccountId,
            Amount = request.Amount,
            CashierShiftId = request.CashierShiftId,
            DepositSlipNumber = request.DepositSlipNumber?.Trim(),
            Status = FinBankDepositStatuses.Draft,
            Notes = request.Notes?.Trim(),
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.FinBankDeposits.Add(deposit);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Deposit.Create", deposit.Id, actorUserId, new { deposit.DepositNumber, deposit.Amount });

        deposit.BankAccount = bankAccount;
        return MapDeposit(deposit, shiftNumber);
    }

    public async Task<BankDepositResponse> PostBankDepositAsync(
        Guid id, PostBankDepositRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            var deposit = await _dbContext.FinBankDeposits
                .Include(x => x.BankAccount).ThenInclude(b => b!.Bank)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Setoran bank tidak ditemukan.");

            EnsureCurrent(deposit.RowVersion, request.ExpectedRowVersion);

            if (deposit.Status != FinBankDepositStatuses.Draft)
                throw new CashValidationException("Hanya setoran berstatus DRAFT yang dapat diposting.");

            await AcquireLockAsync($"FIN_CASH_{deposit.DepositDate:yyyyMMdd}", cancellationToken);

            var snapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CashDate == deposit.DepositDate && !x.IsDelete, cancellationToken);
            if (snapshot is not null && snapshot.Status == FinDailyCashSnapshotStatuses.Closed)
                throw new CashValidationException("Kas tanggal ini sudah ditutup dan tidak dapat diubah.");

            // FIN-DES-021, FR-FIN-062: Saldo kas tersedia dihitung ulang saat transaksi berlangsung
            var availableBalance = await ComputeAvailableCashInternalAsync(
                deposit.DepositDate, deposit.CashierShiftId, deposit.Id, cancellationToken);

            // FIN-VAL-060, UAT-13: Setoran melebihi kas yang tersedia ditolak 422
            if (deposit.Amount > availableBalance)
            {
                throw new CashValidationException(
                    $"Nominal setoran melebihi kas yang tersedia. Saldo tersedia saat ini Rp {availableBalance:N0}.");
            }

            deposit.Status = FinBankDepositStatuses.Posted;
            deposit.PostedBy = actorUserId;
            deposit.PostedAt = DateTimeOffset.UtcNow;
            if (!string.IsNullOrWhiteSpace(request.DepositSlipNumber))
                deposit.DepositSlipNumber = request.DepositSlipNumber.Trim();

            deposit.RowVersion = Guid.NewGuid();
            deposit.UpdateDateTime = DateTime.UtcNow;
            deposit.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("Deposit.Post", deposit.Id, actorUserId, new { deposit.DepositNumber, deposit.Amount, availableBalance });

            string? shiftNumber = null;
            if (deposit.CashierShiftId.HasValue)
            {
                shiftNumber = await _dbContext.BilCashierShifts.AsNoTracking()
                    .Where(s => s.Id == deposit.CashierShiftId.Value)
                    .Select(s => s.ShiftNumber)
                    .FirstOrDefaultAsync(cancellationToken);
            }

            return MapDeposit(deposit, shiftNumber);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    public async Task<BankDepositResponse> VerifyBankDepositAsync(
        Guid id, VerifyBankDepositRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var deposit = await _dbContext.FinBankDeposits
            .Include(x => x.BankAccount).ThenInclude(b => b!.Bank)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Setoran bank tidak ditemukan.");

        EnsureCurrent(deposit.RowVersion, request.ExpectedRowVersion);

        if (deposit.Status != FinBankDepositStatuses.Posted)
            throw new CashValidationException("Hanya setoran berstatus POSTED yang dapat diverifikasi dengan rekening koran.");

        deposit.Status = FinBankDepositStatuses.Verified;
        if (!string.IsNullOrWhiteSpace(request.DepositSlipNumber))
            deposit.DepositSlipNumber = request.DepositSlipNumber.Trim();

        deposit.RowVersion = Guid.NewGuid();
        deposit.UpdateDateTime = DateTime.UtcNow;
        deposit.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Deposit.Verify", deposit.Id, actorUserId, new { deposit.DepositNumber });

        string? shiftNumber = null;
        if (deposit.CashierShiftId.HasValue)
        {
            shiftNumber = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(s => s.Id == deposit.CashierShiftId.Value)
                .Select(s => s.ShiftNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return MapDeposit(deposit, shiftNumber);
    }

    public async Task<BankDepositResponse> CancelBankDepositAsync(
        Guid id, CancelBankDepositRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var deposit = await _dbContext.FinBankDeposits
            .Include(x => x.BankAccount).ThenInclude(b => b!.Bank)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Setoran bank tidak ditemukan.");

        EnsureCurrent(deposit.RowVersion, request.ExpectedRowVersion);

        if (deposit.Status == FinBankDepositStatuses.Verified)
            throw new CashValidationException("Setoran yang sudah diverifikasi tidak dapat dibatalkan.");

        if (deposit.Status == FinBankDepositStatuses.Cancelled)
            throw new CashValidationException("Setoran sudah dibatalkan sebelumnya.");

        if (deposit.Status == FinBankDepositStatuses.Posted)
        {
            var snapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
                .SingleOrDefaultAsync(x => x.CashDate == deposit.DepositDate && !x.IsDelete, cancellationToken);
            if (snapshot is not null && snapshot.Status == FinDailyCashSnapshotStatuses.Closed)
                throw new CashValidationException("Kas harian tanggal ini sudah ditutup. Setoran yang sudah diposting tidak dapat dibatalkan.");
        }

        deposit.Status = FinBankDepositStatuses.Cancelled;
        if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            var reason = request.Reason.Trim();
            deposit.Notes = string.IsNullOrWhiteSpace(deposit.Notes)
                ? $"Dibatalkan: {reason}"
                : $"{deposit.Notes} | Dibatalkan: {reason}";
            if (deposit.Notes.Length > 500)
                deposit.Notes = deposit.Notes[..500];
        }

        deposit.RowVersion = Guid.NewGuid();
        deposit.UpdateDateTime = DateTime.UtcNow;
        deposit.UpdateBy = actorUserId;

        await _dbContext.SaveChangesAsync(cancellationToken);

        await AuditAsync("Deposit.Cancel", deposit.Id, actorUserId, new { deposit.DepositNumber });

        string? shiftNumber = null;
        if (deposit.CashierShiftId.HasValue)
        {
            shiftNumber = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(s => s.Id == deposit.CashierShiftId.Value)
                .Select(s => s.ShiftNumber)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return MapDeposit(deposit, shiftNumber);
    }

    // ------------------------------------------------------------------------------------
    // Kas Harian (FinDailyCashSnapshot) — Baca & Breakdown
    // ------------------------------------------------------------------------------------

    public async Task<DailyCashResponse> GetCurrentDailyCashAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var snapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
            .SingleOrDefaultAsync(x => x.CashDate == today && !x.IsDelete, cancellationToken);

        if (snapshot is not null)
            return MapDailyCash(snapshot);

        // Jika belum ada baris kas hari ini, hitung posisi berjalan live
        var previousClosedSnapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
            .Where(x => !x.IsDelete && x.CashDate < today && x.Status == FinDailyCashSnapshotStatuses.Closed)
            .OrderByDescending(x => x.CashDate)
            .FirstOrDefaultAsync(cancellationToken);

        var openingBalance = previousClosedSnapshot?.ClosingBalance ?? 0m;

        var startOfDay = new DateTimeOffset(today.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endOfDay = startOfDay.AddDays(1);
        var shifts = await _dbContext.BilCashierShifts.AsNoTracking()
            .Where(x => !x.IsDelete && x.OpenedAt >= startOfDay && x.OpenedAt < endOfDay)
            .ToListAsync(cancellationToken);

        var cashReceiptAmount = shifts.Sum(GetShiftCash);

        var bankDepositAmount = await _dbContext.FinBankDeposits.AsNoTracking()
            .Where(x => !x.IsDelete && x.DepositDate == today &&
                        (x.Status == FinBankDepositStatuses.Posted || x.Status == FinBankDepositStatuses.Verified))
            .SumAsync(x => x.Amount, cancellationToken);

        var closingBalance = openingBalance + cashReceiptAmount - bankDepositAmount;

        return new DailyCashResponse
        {
            Id = Guid.Empty,
            CashDate = today,
            OpeningBalance = openingBalance,
            CashReceiptAmount = cashReceiptAmount,
            OtherReceiptAmount = 0m,
            DisbursementAmount = 0m,
            BankDepositAmount = bankDepositAmount,
            ClosingBalance = closingBalance,
            Status = FinDailyCashSnapshotStatuses.Open,
            RowVersion = Guid.Empty
        };
    }

    public async Task<PagedResult<DailyCashResponse>> GetDailyCashPagedAsync(
        DailyCashQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinDailyCashSnapshots.AsNoTracking().Where(x => !x.IsDelete);

        if (request.DateFrom.HasValue)
            query = query.Where(x => x.CashDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(x => x.CashDate <= request.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(request.Status))
            query = query.Where(x => x.Status == request.Status);

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = descending ? query.OrderByDescending(x => x.CashDate) : query.OrderBy(x => x.CashDate);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => MapDailyCash(x))
            .ToListAsync(cancellationToken);

        return new PagedResult<DailyCashResponse>
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize),
            Items = items
        };
    }

    public async Task<DailyCashBreakdownResponse> GetDailyCashBreakdownAsync(
        DateOnly cashDate, CancellationToken cancellationToken)
    {
        var snapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
            .SingleOrDefaultAsync(x => x.CashDate == cashDate && !x.IsDelete, cancellationToken);

        DailyCashResponse snapshotResponse;
        if (snapshot is not null)
        {
            snapshotResponse = MapDailyCash(snapshot);
        }
        else
        {
            var previousClosedSnapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
                .Where(x => !x.IsDelete && x.CashDate < cashDate && x.Status == FinDailyCashSnapshotStatuses.Closed)
                .OrderByDescending(x => x.CashDate)
                .FirstOrDefaultAsync(cancellationToken);

            var openingBalance = previousClosedSnapshot?.ClosingBalance ?? 0m;

            var startOfDate = new DateTimeOffset(cashDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var endOfDate = startOfDate.AddDays(1);
            var shiftsToday = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(x => !x.IsDelete && x.OpenedAt >= startOfDate && x.OpenedAt < endOfDate)
                .ToListAsync(cancellationToken);

            var cashReceiptAmount = shiftsToday.Sum(GetShiftCash);

            var bankDepositAmount = await _dbContext.FinBankDeposits.AsNoTracking()
                .Where(x => !x.IsDelete && x.DepositDate == cashDate &&
                            (x.Status == FinBankDepositStatuses.Posted || x.Status == FinBankDepositStatuses.Verified))
                .SumAsync(x => x.Amount, cancellationToken);

            snapshotResponse = new DailyCashResponse
            {
                Id = Guid.Empty,
                CashDate = cashDate,
                OpeningBalance = openingBalance,
                CashReceiptAmount = cashReceiptAmount,
                OtherReceiptAmount = 0m,
                DisbursementAmount = 0m,
                BankDepositAmount = bankDepositAmount,
                ClosingBalance = openingBalance + cashReceiptAmount - bankDepositAmount,
                Status = FinDailyCashSnapshotStatuses.Open,
                RowVersion = Guid.Empty
            };
        }

        var startOfDay = new DateTimeOffset(cashDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
        var endOfDay = startOfDay.AddDays(1);
        var shifts = await _dbContext.BilCashierShifts.AsNoTracking()
            .Where(x => !x.IsDelete && x.OpenedAt >= startOfDay && x.OpenedAt < endOfDay)
            .OrderBy(x => x.OpenedAt)
            .ToListAsync(cancellationToken);

        var shiftBreakdown = shifts.Select(s => new DailyCashShiftBreakdownItem
        {
            ShiftId = s.Id,
            ShiftNumber = s.ShiftNumber,
            CashierId = s.CashierId,
            RegisterId = s.RegisterId,
            OpeningCash = s.OpeningCash,
            SystemCash = s.SystemCash,
            PhysicalCash = s.PhysicalCash,
            Variance = s.Variance,
            Status = s.Status,
            EffectiveCash = GetShiftCash(s)
        }).ToList();

        var deposits = await _dbContext.FinBankDeposits.AsNoTracking()
            .Include(x => x.BankAccount).ThenInclude(b => b!.Bank)
            .Where(x => !x.IsDelete && x.DepositDate == cashDate)
            .OrderBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);

        var shiftIds = deposits.Where(x => x.CashierShiftId.HasValue).Select(x => x.CashierShiftId!.Value).Distinct().ToList();
        var shiftDict = shiftIds.Count != 0
            ? await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(s => shiftIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.ShiftNumber, cancellationToken)
            : [];

        var depositResponses = deposits.Select(x => MapDeposit(x, shiftDict)).ToList();

        return new DailyCashBreakdownResponse
        {
            Snapshot = snapshotResponse,
            CashierShifts = shiftBreakdown,
            BankDeposits = depositResponses
        };
    }

    // ------------------------------------------------------------------------------------
    // Penutupan Kas Harian (FinDailyCashSnapshot) — Serializable & Freeze
    // ------------------------------------------------------------------------------------

    public async Task<DailyCashResponse> CloseDailyCashAsync(
        DateOnly cashDate, CloseDailyCashRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);

            await AcquireLockAsync($"FIN_CASH_{cashDate:yyyyMMdd}", cancellationToken);

            var snapshot = await _dbContext.FinDailyCashSnapshots
                .SingleOrDefaultAsync(x => x.CashDate == cashDate && !x.IsDelete, cancellationToken);

            if (snapshot is not null)
            {
                // FIN-VAL-063, FR-FIN-065: Kas yang sudah ditutup tidak boleh diubah
                if (snapshot.Status == FinDailyCashSnapshotStatuses.Closed)
                    throw new CashValidationException("Kas tanggal ini sudah ditutup dan tidak dapat diubah.");

                if (request.ExpectedRowVersion.HasValue)
                    EnsureCurrent(snapshot.RowVersion, request.ExpectedRowVersion.Value);
            }

            // FIN-VAL-064, UAT-15: Penutupan hari menolak setoran yang belum diposting (masih DRAFT)
            var draftDepositsCount = await _dbContext.FinBankDeposits
                .CountAsync(x => !x.IsDelete && x.DepositDate == cashDate && x.Status == FinBankDepositStatuses.Draft, cancellationToken);

            if (draftDepositsCount > 0)
            {
                throw new CashValidationException(
                    $"Masih ada {draftDepositsCount} setoran yang belum diposting. Selesaikan dahulu sebelum menutup kas.");
            }

            // FIN-VAL-066: Saldo awal mengikuti penutupan hari sebelumnya
            var previousClosedSnapshot = await _dbContext.FinDailyCashSnapshots.AsNoTracking()
                .Where(x => !x.IsDelete && x.CashDate < cashDate && x.Status == FinDailyCashSnapshotStatuses.Closed)
                .OrderByDescending(x => x.CashDate)
                .FirstOrDefaultAsync(cancellationToken);

            var expectedOpeningBalance = previousClosedSnapshot?.ClosingBalance ?? 0m;
            if (request.OpeningBalance.HasValue && request.OpeningBalance.Value != expectedOpeningBalance)
            {
                throw new CashValidationException("Saldo awal tidak sesuai dengan saldo akhir hari sebelumnya.");
            }

            var openingBalance = expectedOpeningBalance;

            // Penerimaan kasir hari itu dari BilCashierShift (FIN-CAP-006, FIN-DEC-020)
            var startOfDay = new DateTimeOffset(cashDate.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1);
            var shifts = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(x => !x.IsDelete && x.OpenedAt >= startOfDay && x.OpenedAt < endOfDay)
                .ToListAsync(cancellationToken);

            var cashReceiptAmount = shifts.Sum(GetShiftCash);
            var otherReceiptAmount = Math.Max(0m, request.OtherReceiptAmount ?? 0m);
            var disbursementAmount = Math.Max(0m, request.DisbursementAmount ?? 0m);

            // Setoran bank berstatus POSTED atau VERIFIED
            var bankDepositAmount = await _dbContext.FinBankDeposits
                .Where(x => !x.IsDelete && x.DepositDate == cashDate &&
                            (x.Status == FinBankDepositStatuses.Posted || x.Status == FinBankDepositStatuses.Verified))
                .SumAsync(x => x.Amount, cancellationToken);

            // Formula penutupan kas CK_FinDailyCashSnapshot_Formula
            var closingBalance = openingBalance + cashReceiptAmount + otherReceiptAmount - disbursementAmount - bankDepositAmount;

            if (snapshot is null)
            {
                snapshot = new FinDailyCashSnapshot
                {
                    CashDate = cashDate,
                    OpeningBalance = openingBalance,
                    CashReceiptAmount = cashReceiptAmount,
                    OtherReceiptAmount = otherReceiptAmount,
                    DisbursementAmount = disbursementAmount,
                    BankDepositAmount = bankDepositAmount,
                    ClosingBalance = closingBalance,
                    Status = FinDailyCashSnapshotStatuses.Closed,
                    ClosedBy = actorUserId,
                    ClosedAt = DateTimeOffset.UtcNow,
                    RowVersion = Guid.NewGuid(),
                    CreateDateTime = DateTime.UtcNow,
                    CreateBy = actorUserId
                };
                _dbContext.FinDailyCashSnapshots.Add(snapshot);
            }
            else
            {
                snapshot.OpeningBalance = openingBalance;
                snapshot.CashReceiptAmount = cashReceiptAmount;
                snapshot.OtherReceiptAmount = otherReceiptAmount;
                snapshot.DisbursementAmount = disbursementAmount;
                snapshot.BankDepositAmount = bankDepositAmount;
                snapshot.ClosingBalance = closingBalance;
                snapshot.Status = FinDailyCashSnapshotStatuses.Closed;
                snapshot.ClosedBy = actorUserId;
                snapshot.ClosedAt = DateTimeOffset.UtcNow;
                snapshot.RowVersion = Guid.NewGuid();
                snapshot.UpdateDateTime = DateTime.UtcNow;
                snapshot.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);

            await AuditAsync("DailyCash.Close", snapshot.Id, actorUserId, new { cashDate, closingBalance });

            return MapDailyCash(snapshot);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            await RollbackAsync(transaction);
            throw Stale(exception);
        }
        catch
        {
            await RollbackAsync(transaction);
            throw;
        }
        finally
        {
            if (transaction is not null) await transaction.DisposeAsync();
        }
    }

    // ------------------------------------------------------------------------------------
    // Logika Internal & Kalkulasi Kas
    // ------------------------------------------------------------------------------------

    private async Task<decimal> ComputeAvailableCashInternalAsync(
        DateOnly date, Guid? cashierShiftId, Guid? excludeDepositId, CancellationToken cancellationToken)
    {
        decimal totalCashierCash = 0m;
        if (cashierShiftId.HasValue)
        {
            var shift = await _dbContext.BilCashierShifts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == cashierShiftId.Value && !x.IsDelete, cancellationToken);
            if (shift is not null)
                totalCashierCash = GetShiftCash(shift);
        }
        else
        {
            var startOfDay = new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);
            var endOfDay = startOfDay.AddDays(1);
            var shifts = await _dbContext.BilCashierShifts.AsNoTracking()
                .Where(x => !x.IsDelete && x.OpenedAt >= startOfDay && x.OpenedAt < endOfDay)
                .ToListAsync(cancellationToken);
            totalCashierCash = shifts.Sum(GetShiftCash);
        }

        var depositQuery = _dbContext.FinBankDeposits.AsNoTracking()
            .Where(x => !x.IsDelete && (x.Status == FinBankDepositStatuses.Posted || x.Status == FinBankDepositStatuses.Verified));

        if (excludeDepositId.HasValue)
            depositQuery = depositQuery.Where(x => x.Id != excludeDepositId.Value);

        if (cashierShiftId.HasValue)
            depositQuery = depositQuery.Where(x => x.CashierShiftId == cashierShiftId.Value);
        else
            depositQuery = depositQuery.Where(x => x.DepositDate == date);

        var totalDeposited = await depositQuery.SumAsync(x => x.Amount, cancellationToken);
        return Math.Max(0m, totalCashierCash - totalDeposited);
    }

    /// <summary>
    /// Menentukan kas kasir yang dapat diakui (FIN-CAP-006, FIN-DEC-017).
    /// Jika kas fisik sudah dihitung (PhysicalCash > 0), gunakan nilai kas fisik (mencakup selisih yang disahkan jika status REVIEWED).
    /// Jika masih OPEN atau physical cash belum dihitung, gunakan SystemCash.
    /// </summary>
    public static decimal GetShiftCash(BilCashierShift shift)
    {
        if (shift.PhysicalCash > 0)
            return shift.PhysicalCash;

        return Math.Max(0m, shift.SystemCash);
    }

    // ------------------------------------------------------------------------------------
    // Infrastruktur Database & Concurrency
    // ------------------------------------------------------------------------------------

    private async Task<IDbContextTransaction?> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (!_dbContext.Database.IsRelational()) return null;
        return await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
    }

    private Task AcquireLockAsync(string key, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock(hashtext({0}));", [key], cancellationToken)
            : Task.CompletedTask;

    private static Task CommitAsync(IDbContextTransaction? transaction, CancellationToken cancellationToken) =>
        transaction is null ? Task.CompletedTask : transaction.CommitAsync(cancellationToken);

    private static Task RollbackAsync(IDbContextTransaction? transaction) =>
        transaction is null ? Task.CompletedTask : transaction.RollbackAsync(CancellationToken.None);

    private static void EnsureCurrent(Guid actualRowVersion, Guid expectedRowVersion)
    {
        if (expectedRowVersion == Guid.Empty || actualRowVersion != expectedRowVersion) throw Stale();
    }

    private static CashConflictException Stale(Exception? inner = null) =>
        new("Data telah berubah. Muat ulang sebelum melanjutkan.", inner);

    private static string GenerateDepositNumber(DateOnly date)
    {
        var candidate = $"DEP-{date:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private Task AuditAsync(string action, Guid entityId, Guid actorUserId, object? details = null) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceCashManagement.{action}",
            $"Aktivitas kas dicatat. EntityId={entityId} ActorUserId={actorUserId}",
            new { EntityId = entityId, ActorUserId = actorUserId, Details = details });

    // ------------------------------------------------------------------------------------
    // Mapping Helpers
    // ------------------------------------------------------------------------------------

    public static BankDepositResponse MapDeposit(FinBankDeposit x, IReadOnlyDictionary<Guid, string> shiftDict) => new()
    {
        Id = x.Id,
        DepositNumber = x.DepositNumber,
        DepositDate = x.DepositDate,
        BankAccountId = x.BankAccountId,
        AccountNumber = x.BankAccount?.AccountNumber,
        AccountName = x.BankAccount?.AccountName,
        BankName = x.BankAccount?.Bank?.BankName,
        Amount = x.Amount,
        CashierShiftId = x.CashierShiftId,
        ShiftNumber = x.CashierShiftId.HasValue && shiftDict.TryGetValue(x.CashierShiftId.Value, out var sn) ? sn : null,
        DepositSlipNumber = x.DepositSlipNumber,
        Status = x.Status,
        PostedBy = x.PostedBy,
        PostedAt = x.PostedAt,
        Notes = x.Notes,
        RowVersion = x.RowVersion
    };

    public static BankDepositResponse MapDeposit(FinBankDeposit x, string? shiftNumber = null) => new()
    {
        Id = x.Id,
        DepositNumber = x.DepositNumber,
        DepositDate = x.DepositDate,
        BankAccountId = x.BankAccountId,
        AccountNumber = x.BankAccount?.AccountNumber,
        AccountName = x.BankAccount?.AccountName,
        BankName = x.BankAccount?.Bank?.BankName,
        Amount = x.Amount,
        CashierShiftId = x.CashierShiftId,
        ShiftNumber = shiftNumber,
        DepositSlipNumber = x.DepositSlipNumber,
        Status = x.Status,
        PostedBy = x.PostedBy,
        PostedAt = x.PostedAt,
        Notes = x.Notes,
        RowVersion = x.RowVersion
    };

    public static DailyCashResponse MapDailyCash(FinDailyCashSnapshot x) => new()
    {
        Id = x.Id,
        CashDate = x.CashDate,
        OpeningBalance = x.OpeningBalance,
        CashReceiptAmount = x.CashReceiptAmount,
        OtherReceiptAmount = x.OtherReceiptAmount,
        DisbursementAmount = x.DisbursementAmount,
        BankDepositAmount = x.BankDepositAmount,
        ClosingBalance = x.ClosingBalance,
        Status = x.Status,
        ClosedBy = x.ClosedBy,
        ClosedAt = x.ClosedAt,
        RowVersion = x.RowVersion
    };
}

public sealed class CashBadRequestException(string message) : Exception(message);
public sealed class CashValidationException(string message) : Exception(message);
public sealed class CashConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
