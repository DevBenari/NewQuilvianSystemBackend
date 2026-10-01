using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Data;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// Satu-satunya penulis FinNonPatientReceivable/FinNonPatientReceivableSettlement (BE-FIN-057,
/// FIN-DEC-099..104, FIN-DES-074..077). Aggregate BERDIRI SENDIRI (FIN-DEC-101) — MUST NOT
/// memanggil FinanceReceivableService, FinanceReceiptService, atau FinanceAccountingOutboxService.
/// Ketiadaan panggilan outbox akuntansi itu DISENGAJA dan menjadi isi FIN-OQ-044, bukan kelalaian.
///
/// FIN-DEC-103: write-off dan cancel adalah SATU AKSI LANGSUNG tanpa jenjang persetujuan —
/// berbeda sengaja dari FinReceivableWriteOff. Kelonggaran ini HANYA berlaku untuk entity ini.
///
/// Reason pada write-off/cancel disimpan ke kolom Note (ditambahkan sebagai baris baru, mengikuti
/// pola "tidak pernah menghapus, selalu menambah" yang berlaku di blueprint ini) — kontrak
/// (api-contract.md §E.1) tidak menyediakan kolom Reason khusus pada NonPatientReceivableResponse,
/// dan Note adalah satu-satunya kolom teks bebas yang ada pada model. Dicatat sebagai keputusan
/// teknis pada laporan BE-FIN-057, bukan kebijakan bisnis baru.
/// </summary>
public sealed class FinanceNonPatientReceivableService
{
    private const string LogCategory = "Corporate.FinanceManagement.Receivable.NonPatientReceivable";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public FinanceNonPatientReceivableService(ApplicationDbContext dbContext, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    // ------------------------------------------------------------------------------------
    // Baca
    // ------------------------------------------------------------------------------------

    public async Task<PagedResult<NonPatientReceivableResponse>> GetPagedAsync(NonPatientReceivableQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinNonPatientReceivables.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(request.Category)) query = query.Where(x => x.Category == request.Category);
        if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
        if (request.PeriodStart.HasValue) query = query.Where(x => x.PeriodStart >= request.PeriodStart.Value);
        if (request.PeriodEnd.HasValue) query = query.Where(x => x.PeriodEnd <= request.PeriodEnd.Value);
        if (request.DueDateFrom.HasValue) query = query.Where(x => x.DueDate >= request.DueDateFrom.Value);
        if (request.DueDateTo.HasValue) query = query.Where(x => x.DueDate <= request.DueDateTo.Value);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToUpper();
            query = query.Where(x => x.CounterpartyName.ToUpper().Contains(search) || x.RentedObject.ToUpper().Contains(search) || x.ReceivableNumber.ToUpper().Contains(search));
        }

        var descending = !string.Equals(request.SortDirection, "asc", StringComparison.OrdinalIgnoreCase);
        query = request.SortBy.Trim().ToLowerInvariant() switch
        {
            "receivablenumber" => descending ? query.OrderByDescending(x => x.ReceivableNumber) : query.OrderBy(x => x.ReceivableNumber),
            "outstandingamount" => descending ? query.OrderByDescending(x => x.OutstandingAmount) : query.OrderBy(x => x.OutstandingAmount),
            "status" => descending ? query.OrderByDescending(x => x.Status) : query.OrderBy(x => x.Status),
            _ => descending ? query.OrderByDescending(x => x.DueDate) : query.OrderBy(x => x.DueDate)
        };

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize).Take(request.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<NonPatientReceivableResponse>
        {
            PageNumber = request.PageNumber, PageSize = request.PageSize, TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)request.PageSize), Items = items.Select(Map).ToList()
        };
    }

    public async Task<NonPatientReceivableDetailResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var receivable = await _dbContext.FinNonPatientReceivables.AsNoTracking()
            .Include(x => x.Settlements)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan sewa tidak ditemukan.");

        return new NonPatientReceivableDetailResponse
        {
            Receivable = Map(receivable),
            Settlements = receivable.Settlements.OrderBy(x => x.SettlementDate).Select(MapSettlement).ToList()
        };
    }

    /// <summary>FIN-DES-074: memakai definisi kelompok umur yang sama persis dengan piutang pasien
    /// (ReceivableAgingBuckets, Services.FinanceReceivableService) — dua laporan terpisah,
    /// satu definisi kelompok.</summary>
    public async Task<List<ReceivableAgingBucketResult>> GetAgingAsync(DateOnly? asOfDate, string? category, CancellationToken cancellationToken)
    {
        var referenceDate = asOfDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var query = _dbContext.FinNonPatientReceivables.AsNoTracking()
            .Where(x => !x.IsDelete && x.OutstandingAmount > 0);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category);

        var receivables = await query.Select(x => new { x.DueDate, x.OutstandingAmount }).ToListAsync(cancellationToken);

        var buckets = ReceivableAgingBuckets.Labels.ToDictionary(label => label, label => new ReceivableAgingBucketResult { BucketLabel = label });
        foreach (var receivable in receivables)
        {
            var daysPastDue = Math.Max(0, referenceDate.DayNumber - receivable.DueDate.DayNumber);
            var bucket = buckets[ReceivableAgingBuckets.Resolve(daysPastDue)];
            bucket.ReceivableCount++;
            bucket.TotalOutstandingAmount += receivable.OutstandingAmount;
        }
        return ReceivableAgingBuckets.Labels.Select(label => buckets[label]).ToList();
    }

    public async Task<NonPatientReceivableSummaryResponse> GetSummaryAsync(string? category, CancellationToken cancellationToken)
    {
        var query = _dbContext.FinNonPatientReceivables.AsNoTracking().Where(x => !x.IsDelete);
        if (!string.IsNullOrWhiteSpace(category)) query = query.Where(x => x.Category == category);

        var counts = await query.GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);
        int Count(string status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

        return new NonPatientReceivableSummaryResponse
        {
            TotalReceivable = counts.Sum(c => c.Count),
            OutstandingCount = Count(FinNonPatientReceivableStatuses.Outstanding),
            PartiallySettledCount = Count(FinNonPatientReceivableStatuses.PartiallySettled),
            SettledCount = Count(FinNonPatientReceivableStatuses.Settled),
            WrittenOffCount = Count(FinNonPatientReceivableStatuses.WrittenOff),
            CancelledCount = Count(FinNonPatientReceivableStatuses.Cancelled),
            TotalOutstandingAmount = await query.SumAsync(x => x.OutstandingAmount, cancellationToken)
        };
    }

    public Task<NonPatientReceivableFilterMetadataResponse> GetFilterMetadataAsync(CancellationToken cancellationToken) =>
        Task.FromResult(new NonPatientReceivableFilterMetadataResponse
        {
            PageSizeOptions = [10, 25, 50, 100],
            SortableFields = ["dueDate", "receivableNumber", "outstandingAmount", "status"],
            CategoryOptions = [FinNonPatientReceivableCategories.Parking, FinNonPatientReceivableCategories.Tenant],
            StatusOptions =
            [
                FinNonPatientReceivableStatuses.Outstanding, FinNonPatientReceivableStatuses.PartiallySettled,
                FinNonPatientReceivableStatuses.Settled, FinNonPatientReceivableStatuses.WrittenOff, FinNonPatientReceivableStatuses.Cancelled
            ]
        });

    public static NonPatientReceivableResponse Map(FinNonPatientReceivable x)
    {
        var totalBilled = x.BilledAmount + x.LateFeeAmount;
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new NonPatientReceivableResponse
        {
            Id = x.Id,
            ReceivableNumber = x.ReceivableNumber,
            Category = x.Category,
            CounterpartyName = x.CounterpartyName,
            RentedObject = x.RentedObject,
            PeriodStart = x.PeriodStart,
            PeriodEnd = x.PeriodEnd,
            DueDate = x.DueDate,
            BilledAmount = x.BilledAmount,
            LateFeeAmount = x.LateFeeAmount,
            TotalBilledAmount = totalBilled,
            SettledAmount = totalBilled - x.OutstandingAmount,
            OutstandingAmount = x.OutstandingAmount,
            DaysPastDue = Math.Max(0, today.DayNumber - x.DueDate.DayNumber),
            Status = x.Status,
            Note = x.Note,
            RowVersion = x.RowVersion
        };
    }

    public static SettlementRowResponse MapSettlement(FinNonPatientReceivableSettlement x) => new()
    {
        Id = x.Id,
        SettlementDate = x.SettlementDate,
        Amount = x.Amount,
        PaymentMethod = x.PaymentMethod,
        ReferenceNumber = x.ReferenceNumber,
        Note = x.Note
    };

    // ------------------------------------------------------------------------------------
    // Tulis — Pencatatan (FIN-VAL-154..157)
    // ------------------------------------------------------------------------------------

    public async Task<FinNonPatientReceivable> CreateAsync(CreateNonPatientReceivableRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var category = ValidateCategory(request.Category);
        ValidateBilledAmount(request.BilledAmount);
        ValidateLateFeeAmount(request.LateFeeAmount);
        ValidatePeriod(request.PeriodStart, request.PeriodEnd, request.DueDate);
        var counterpartyName = ValidateRequiredText(request.CounterpartyName, "Nama penyewa", 200);
        var rentedObject = ValidateRequiredText(request.RentedObject, "Objek sewa", 200);

        var receivable = new FinNonPatientReceivable
        {
            ReceivableNumber = GenerateNumber("NPR"),
            Category = category,
            CounterpartyName = counterpartyName,
            RentedObject = rentedObject,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            DueDate = request.DueDate,
            BilledAmount = request.BilledAmount,
            LateFeeAmount = request.LateFeeAmount,
            OutstandingAmount = request.BilledAmount + request.LateFeeAmount,
            Status = FinNonPatientReceivableStatuses.Outstanding,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };
        _dbContext.FinNonPatientReceivables.Add(receivable);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await AuditAsync("Create", receivable.Id, actorUserId);
        return receivable;
    }

    // FIN-VAL-159: hanya sah bila belum pernah menerima pembayaran sama sekali.
    public async Task<FinNonPatientReceivable> UpdateAsync(Guid id, UpdateNonPatientReceivableRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var category = ValidateCategory(request.Category);
        ValidateBilledAmount(request.BilledAmount);
        ValidateLateFeeAmount(request.LateFeeAmount);
        ValidatePeriod(request.PeriodStart, request.PeriodEnd, request.DueDate);
        var counterpartyName = ValidateRequiredText(request.CounterpartyName, "Nama penyewa", 200);
        var rentedObject = ValidateRequiredText(request.RentedObject, "Objek sewa", 200);

        var receivable = await _dbContext.FinNonPatientReceivables
            .Include(x => x.Settlements)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan sewa tidak ditemukan.");
        EnsureCurrent(receivable.RowVersion, request.ExpectedRowVersion);

        if (receivable.Status is FinNonPatientReceivableStatuses.WrittenOff or FinNonPatientReceivableStatuses.Cancelled)
            throw new NonPatientReceivableValidationException("Langkah ini tidak bisa dilakukan dari status tagihan saat ini.");
        if (receivable.Settlements.Count > 0)
            throw new NonPatientReceivableValidationException("Tagihan yang sudah menerima pembayaran tidak bisa dikoreksi. Betulkan lewat pencatatan pelunasan.");

        receivable.Category = category;
        receivable.CounterpartyName = counterpartyName;
        receivable.RentedObject = rentedObject;
        receivable.PeriodStart = request.PeriodStart;
        receivable.PeriodEnd = request.PeriodEnd;
        receivable.DueDate = request.DueDate;
        receivable.BilledAmount = request.BilledAmount;
        receivable.LateFeeAmount = request.LateFeeAmount;
        receivable.OutstandingAmount = request.BilledAmount + request.LateFeeAmount;
        receivable.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        receivable.UpdateDateTime = DateTime.UtcNow;
        receivable.UpdateBy = actorUserId;
        receivable.RowVersion = Guid.NewGuid();

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException exception) { throw Stale(exception); }
        await AuditAsync("Update", receivable.Id, actorUserId);
        return receivable;
    }

    // ------------------------------------------------------------------------------------
    // Pelunasan — FIN-VAL-161/162, state-transition-matrix.md §E.1
    // ------------------------------------------------------------------------------------

    public async Task<FinNonPatientReceivable> AddSettlementAsync(
        Guid id, CreateNonPatientReceivableSettlementRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        if (request.Amount == 0) throw new NonPatientReceivableBadRequestException("Nominal pelunasan tidak boleh nol.");
        var paymentMethod = ValidateRequiredText(request.PaymentMethod, "Metode pembayaran", 50);

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await BeginTransactionAsync(cancellationToken);
            await AcquireLockAsync($"FIN_NON_PATIENT_RECEIVABLE_{id:N}", cancellationToken);

            var receivable = await _dbContext.FinNonPatientReceivables
                .Include(x => x.Settlements)
                .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
                ?? throw new KeyNotFoundException("Tagihan sewa tidak ditemukan.");
            EnsureCurrent(receivable.RowVersion, request.ExpectedRowVersion);

            if (receivable.Status is FinNonPatientReceivableStatuses.WrittenOff or FinNonPatientReceivableStatuses.Cancelled)
                throw new NonPatientReceivableValidationException("Langkah ini tidak bisa dilakukan dari status tagihan saat ini.");

            var totalBilled = receivable.BilledAmount + receivable.LateFeeAmount;
            var currentSettled = receivable.Settlements.Sum(x => x.Amount);
            var newSettled = currentSettled + request.Amount;

            if (newSettled < 0)
                throw new NonPatientReceivableValidationException("Pembatalan pembayaran ini melebihi pembayaran yang pernah tercatat.");
            if (newSettled > totalBilled)
                throw new NonPatientReceivableValidationException("Jumlah pembayaran melebihi nilai tagihan. Periksa kembali nominalnya.");

            var settlement = new FinNonPatientReceivableSettlement
            {
                NonPatientReceivableId = receivable.Id,
                SettlementDate = request.SettlementDate,
                Amount = request.Amount,
                PaymentMethod = paymentMethod,
                ReferenceNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? null : request.ReferenceNumber.Trim(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                CreateDateTime = DateTime.UtcNow,
                CreateBy = actorUserId
            };
            _dbContext.FinNonPatientReceivableSettlements.Add(settlement);

            receivable.OutstandingAmount = totalBilled - newSettled;
            receivable.Status = newSettled <= 0m
                ? FinNonPatientReceivableStatuses.Outstanding
                : newSettled >= totalBilled ? FinNonPatientReceivableStatuses.Settled : FinNonPatientReceivableStatuses.PartiallySettled;
            receivable.UpdateDateTime = DateTime.UtcNow;
            receivable.UpdateBy = actorUserId;
            receivable.RowVersion = Guid.NewGuid();

            await _dbContext.SaveChangesAsync(cancellationToken);
            await CommitAsync(transaction, cancellationToken);
            await AuditAsync("Settlement.Add", receivable.Id, actorUserId);
            return receivable;
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
    // Write-off dan Cancel — FIN-DEC-103: satu aksi langsung, TANPA jenjang persetujuan
    // ------------------------------------------------------------------------------------

    public async Task<FinNonPatientReceivable> WriteOffAsync(
        Guid id, WriteOffNonPatientReceivableRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var reason = ValidateReason(request.Reason);

        var receivable = await _dbContext.FinNonPatientReceivables
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan sewa tidak ditemukan.");
        EnsureCurrent(receivable.RowVersion, request.ExpectedRowVersion);

        if (receivable.Status is not (FinNonPatientReceivableStatuses.Outstanding or FinNonPatientReceivableStatuses.PartiallySettled))
            throw new NonPatientReceivableValidationException("Langkah ini tidak bisa dilakukan dari status tagihan saat ini.");

        receivable.Status = FinNonPatientReceivableStatuses.WrittenOff;
        receivable.OutstandingAmount = 0m;
        receivable.Note = AppendNote(receivable.Note, "Write-off", reason);
        receivable.UpdateDateTime = DateTime.UtcNow;
        receivable.UpdateBy = actorUserId;
        receivable.RowVersion = Guid.NewGuid();

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException exception) { throw Stale(exception); }
        await AuditAsync("WriteOff", receivable.Id, actorUserId);
        return receivable;
    }

    public async Task<FinNonPatientReceivable> CancelAsync(
        Guid id, CancelNonPatientReceivableRequest request, Guid actorUserId, CancellationToken cancellationToken)
    {
        var reason = ValidateReason(request.Reason);

        var receivable = await _dbContext.FinNonPatientReceivables
            .Include(x => x.Settlements)
            .SingleOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Tagihan sewa tidak ditemukan.");
        EnsureCurrent(receivable.RowVersion, request.ExpectedRowVersion);

        if (receivable.Status != FinNonPatientReceivableStatuses.Outstanding)
            throw new NonPatientReceivableValidationException("Langkah ini tidak bisa dilakukan dari status tagihan saat ini.");
        if (receivable.Settlements.Count > 0)
            throw new NonPatientReceivableValidationException("Tagihan yang sudah menerima pembayaran tidak bisa dibatalkan. Betulkan lewat pencatatan pelunasan bernilai minus.");

        receivable.Status = FinNonPatientReceivableStatuses.Cancelled;
        receivable.Note = AppendNote(receivable.Note, "Cancel", reason);
        receivable.UpdateDateTime = DateTime.UtcNow;
        receivable.UpdateBy = actorUserId;
        receivable.CancelDateTime = DateTime.UtcNow;
        receivable.CancelBy = actorUserId;
        receivable.IsCancel = true;
        receivable.RowVersion = Guid.NewGuid();

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException exception) { throw Stale(exception); }
        await AuditAsync("Cancel", receivable.Id, actorUserId);
        return receivable;
    }

    // ------------------------------------------------------------------------------------
    // Helper
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

    private static NonPatientReceivableConflictException Stale(Exception? inner = null) =>
        new("Data tagihan ini sudah diubah pengguna lain. Muat ulang halaman sebelum melanjutkan.", inner);

    // FIN-VAL-154
    private static string ValidateCategory(string? value)
    {
        var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
        if (normalized is not (FinNonPatientReceivableCategories.Parking or FinNonPatientReceivableCategories.Tenant))
            throw new NonPatientReceivableBadRequestException("Jenis sewa hanya boleh Parkir atau Tenant.");
        return normalized;
    }

    // FIN-VAL-155
    private static void ValidateBilledAmount(decimal amount)
    {
        if (amount <= 0) throw new NonPatientReceivableBadRequestException("Nominal tagihan harus lebih besar dari nol.");
    }

    // FIN-VAL-156
    private static void ValidateLateFeeAmount(decimal amount)
    {
        if (amount < 0) throw new NonPatientReceivableBadRequestException("Nominal denda tidak boleh kurang dari nol.");
    }

    // FIN-VAL-157
    private static void ValidatePeriod(DateOnly periodStart, DateOnly periodEnd, DateOnly dueDate)
    {
        if (periodEnd < periodStart || dueDate < periodStart)
            throw new NonPatientReceivableBadRequestException("Periode tagihan dan tanggal jatuh tempo tidak masuk akal. Periksa kembali tanggalnya.");
    }

    // FIN-VAL-158
    private static string ValidateReason(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new NonPatientReceivableBadRequestException(
                "Tuliskan alasannya — penghapusan dan pembatalan tagihan sewa tidak melewati persetujuan siapa pun, jadi alasannya wajib tercatat.");
        var trimmed = value.Trim();
        if (trimmed.Length > 500) throw new NonPatientReceivableBadRequestException("Alasan maksimal 500 karakter.");
        return trimmed;
    }

    private static string ValidateRequiredText(string? value, string label, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new NonPatientReceivableBadRequestException($"{label} wajib diisi.");
        var trimmed = value.Trim();
        if (trimmed.Length > maxLength) throw new NonPatientReceivableBadRequestException($"{label} maksimal {maxLength} karakter.");
        return trimmed;
    }

    private static string AppendNote(string? existingNote, string action, string reason)
    {
        var line = $"[{action}] {reason}";
        var combined = string.IsNullOrWhiteSpace(existingNote) ? line : $"{existingNote}\n{line}";
        return combined.Length <= 500 ? combined : combined[^500..];
    }

    // Nomor tagihan tidak punya format baku pada kontrak yang terkunci — dibuat unik lewat Guid,
    // bukan Count/Max/Last+1 (QBE-CODE-002/003), mengikuti pola GenerateNumber FinanceReceivableService.
    private static string GenerateNumber(string prefix)
    {
        var candidate = $"{prefix}-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid():N}";
        return candidate.Length <= 50 ? candidate : candidate[..50];
    }

    private Task AuditAsync(string action, Guid entityId, Guid actorUserId) =>
        _loggerService.AuditAsync(LogCategory, $"FinanceNonPatientReceivable.{action}",
            $"Perubahan piutang sewa non-pasien dicatat. EntityId={entityId}",
            new { EntityId = entityId, ActorUserId = actorUserId });
}

public sealed class NonPatientReceivableBadRequestException(string message) : Exception(message);
public sealed class NonPatientReceivableValidationException(string message) : Exception(message);
public sealed class NonPatientReceivableConflictException(string message, Exception? innerException = null) : Exception(message, innerException);
