using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Permintaan stok barang dan obat: pembuatan, perubahan, pengiriman, dan riwayatnya.
/// </summary>
/// <remarks>
/// Seluruh perintah yang mengubah data bersifat idempoten lewat <c>IdempotencyKey</c>, dan
/// memakai <c>ExpectedVersion</c> agar dua petugas tidak saling menimpa tanpa sadar.
/// </remarks>
public sealed class StockRequestService
{
    private const string LogCategory = "PharmacyManagement";

    private const string CreateAction = "CreateStockRequest";
    private const string UpdateAction = "UpdateStockRequest";
    private const string SubmitAction = "SubmitStockRequest";
    private const string CancelAction = "CancelStockRequest";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public StockRequestService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    // ============================================ 1. riwayat permintaan obat

    public async Task<PagedResult<StockRequestSummaryResponse>> GetPagedAsync(
        StockRequestPagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrxStockRequests.AsNoTracking().Where(x => !x.IsDelete);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.Priority.HasValue) query = query.Where(x => x.Priority == request.Priority);
        if (request.RequestingServiceUnitId.HasValue)
            query = query.Where(x => x.RequestingServiceUnitId == request.RequestingServiceUnitId);
        if (request.StorageLocationId.HasValue)
            query = query.Where(x => x.StorageLocationId == request.StorageLocationId);

        // Menyaring menurut obat berarti menyaring permintaan yang MEMUAT obat itu,
        // bukan permintaan yang seluruhnya obat itu.
        if (request.DrugId.HasValue)
            query = query.Where(x => x.Items.Any(i => i.DrugId == request.DrugId && !i.IsDelete));

        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date.ToUniversalTime();
            query = query.Where(x => x.RequestedAt >= start);
        }

        if (request.EndDate.HasValue)
        {
            // Batas akhir dibuat eksklusif pada hari berikutnya, supaya permintaan yang
            // dibuat sore hari pada tanggal itu tetap ikut terbawa.
            var endExclusive = request.EndDate.Value.Date.ToUniversalTime().AddDays(1);
            query = query.Where(x => x.RequestedAt < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.RequestNumber.ToLower().Contains(keyword) ||
                (x.RequestingServiceUnit != null &&
                 x.RequestingServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                x.Items.Any(i => !i.IsDelete &&
                    (i.DrugNameSnapshot.ToLower().Contains(keyword) ||
                     i.DrugCodeSnapshot.ToLower().Contains(keyword))));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.RequestedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new StockRequestSummaryResponse
            {
                Id = x.Id,
                RequestNumber = x.RequestNumber,
                RequestingServiceUnitId = x.RequestingServiceUnitId,
                RequestingServiceUnitName = x.RequestingServiceUnit != null
                    ? x.RequestingServiceUnit.ServiceUnitName : string.Empty,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation != null
                    ? x.StorageLocation.StorageLocationName : string.Empty,
                RequestedByName = x.RequestedByWorkforce != null
                    ? x.RequestedByWorkforce.DisplayName : string.Empty,
                Status = x.Status,
                Priority = x.Priority,
                RequestedAt = x.RequestedAt,
                NeededAt = x.NeededAt,
                SubmittedAt = x.SubmittedAt,
                ItemCount = x.ItemCount,
                Version = x.Version,
                IsEditable = x.Status == StockRequestStatus.Draft
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<StockRequestSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ============================================ 2. detail permintaan

    public async Task<StockRequestDetailResponse?> GetDetailAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(id, tracking: false, cancellationToken);
        return entity == null ? null : MapDetail(entity);
    }

    // ============================================ 3. buat permintaan

    public async Task<StockRequestDetailResponse> CreateAsync(CreateStockRequestRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var actorUserId = GetCurrentUserId();
        var fingerprint = BuildFingerprint(request.StorageLocationId, request.Priority,
            request.NeededAt, request.Notes, request.Items);

        var prior = await FindIdempotentAsync(CreateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(prior.StockRequestId, cancellationToken))!;
        }

        await ValidateReferencesAsync(request.RequestingServiceUnitId, request.StorageLocationId,
            request.RequestedByWorkforceId, cancellationToken);

        var drugs = await ResolveDrugsAsync(request.Items, cancellationToken);
        var now = DateTime.UtcNow;
        var id = DeterministicId(request.IdempotencyKey);

        var entity = new TrxStockRequest
        {
            Id = id,
            RequestNumber = $"REQ-{now:yyyyMMdd}-{id.ToString("N")[..6].ToUpperInvariant()}",
            RequestingServiceUnitId = request.RequestingServiceUnitId,
            StorageLocationId = request.StorageLocationId,
            RequestedByWorkforceId = request.RequestedByWorkforceId,
            Status = StockRequestStatus.Draft,
            Priority = request.Priority,
            NeededAt = request.NeededAt?.ToUniversalTime(),
            Notes = Normalize(request.Notes),
            RequestedAt = now,
            ItemCount = request.Items.Count,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.TrxStockRequests.Add(entity);
        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        _dbContext.TrxStockRequestHistories.Add(NewHistory(entity.Id, StockRequestStatus.Draft,
            null, CreateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockRequest.Create",
            "Membuat permintaan stok barang atau obat.",
            new { entity.Id, entity.RequestNumber, entity.ItemCount, ActorUserId = actorUserId });

        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    // ============================================ 4. edit permintaan

    /// <summary>
    /// Mengubah permintaan yang masih berstatus <c>Draft</c>.
    /// </summary>
    /// <remarks>
    /// Setelah dikirim, permintaan tidak boleh diubah lagi. Gudang sudah melihatnya dan
    /// mungkin sudah mulai menyiapkan; mengubah isinya diam-diam membuat yang disiapkan
    /// tidak lagi cocok dengan yang diminta. Bila memang perlu berubah, permintaan
    /// dibatalkan lalu dibuat baru — sehingga jejaknya tetap terbaca.
    /// </remarks>
    public async Task<StockRequestDetailResponse> UpdateAsync(Guid id,
        UpdateStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var actorUserId = GetCurrentUserId();
        var fingerprint = BuildFingerprint(request.StorageLocationId, request.Priority,
            request.NeededAt, request.Notes, request.Items);

        var prior = await FindIdempotentAsync(UpdateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            if (prior.StockRequestId != id)
                throw new StockRequestConflictException("PHM013",
                    "Idempotency key sudah dipakai untuk permintaan lain.");
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        if (entity.Status != StockRequestStatus.Draft)
            throw new StockRequestConflictException("PHM004",
                "Permintaan yang sudah dikirim tidak dapat diubah. Batalkan lalu buat permintaan baru.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var storageValid = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .AnyAsync(x => x.Id == request.StorageLocationId && !x.IsDelete, cancellationToken);
        if (!storageValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Lokasi penyimpanan tidak ditemukan.");

        var drugs = await ResolveDrugsAsync(request.Items, cancellationToken);
        var now = DateTime.UtcNow;

        // Baris lama ditandai terhapus, bukan dibuang dari basis data, supaya jejak
        // penyuntingan tetap dapat ditelusuri bila kelak dipersoalkan.
        foreach (var existing in entity.Items.Where(x => !x.IsDelete))
        {
            existing.IsDelete = true;
            existing.DeleteDateTime = now;
            existing.DeleteBy = actorUserId;
        }

        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        entity.StorageLocationId = request.StorageLocationId;
        entity.Priority = request.Priority;
        entity.NeededAt = request.NeededAt?.ToUniversalTime();
        entity.Notes = Normalize(request.Notes);
        entity.ItemCount = request.Items.Count;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.TrxStockRequestHistories.Add(NewHistory(entity.Id, entity.Status, entity.Status,
            UpdateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ============================================ perintah status

    /// <summary>Mengirim permintaan ke gudang; sesudah ini isinya terkunci.</summary>
    public async Task<StockRequestDetailResponse> SubmitAsync(Guid id,
        SubmitStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var actorUserId = GetCurrentUserId();
        var fingerprint = Hash($"submit:{id}");

        var prior = await FindIdempotentAsync(SubmitAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        if (entity.Status != StockRequestStatus.Draft)
            throw new StockRequestConflictException("PHM004",
                "Hanya permintaan berstatus Draft yang dapat dikirim.");

        if (!entity.Items.Any(x => !x.IsDelete))
            throw new StockRequestUnprocessableException("PHM002",
                "Permintaan tanpa satu pun item tidak dapat dikirim.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var now = DateTime.UtcNow;
        entity.Status = StockRequestStatus.Submitted;
        entity.SubmittedAt = now;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.TrxStockRequestHistories.Add(NewHistory(entity.Id, StockRequestStatus.Submitted,
            StockRequestStatus.Draft, SubmitAction, null, request.IdempotencyKey, fingerprint,
            actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockRequest.Submit",
            "Mengirim permintaan stok ke gudang.",
            new { entity.Id, entity.RequestNumber, ActorUserId = actorUserId });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    public async Task<StockRequestDetailResponse> CancelAsync(Guid id,
        CancelStockRequestRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new StockRequestUnprocessableException("PHM009",
                "Alasan pembatalan wajib diisi.");

        var actorUserId = GetCurrentUserId();
        var reason = request.Reason.Trim();
        var fingerprint = Hash(reason);

        var prior = await FindIdempotentAsync(CancelAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Permintaan stok tidak ditemukan.");

        if (entity.Status is not (StockRequestStatus.Draft or StockRequestStatus.Submitted))
            throw new StockRequestConflictException("PHM004",
                "Permintaan hanya dapat dibatalkan sebelum diputuskan gudang.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var now = DateTime.UtcNow;
        var from = entity.Status;
        entity.Status = StockRequestStatus.Cancelled;
        entity.DecidedAt = now;
        entity.DecisionReason = reason;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.TrxStockRequestHistories.Add(NewHistory(entity.Id, StockRequestStatus.Cancelled,
            from, CancelAction, reason, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ============================================================== penolong

    private Task<TrxStockRequest?> LoadAsync(Guid id, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.TrxStockRequests
            .Include(x => x.RequestingServiceUnit)
            .Include(x => x.StorageLocation)
            .Include(x => x.RequestedByWorkforce)
            .Include(x => x.Items)
            .Include(x => x.Histories.Where(h => !h.IsDelete))
            .Where(x => x.Id == id && !x.IsDelete);

        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    private void AddItems(Guid requestId, List<StockRequestItemInput> inputs,
        Dictionary<Guid, DrugSnapshot> drugs, Guid actorUserId, DateTime now)
    {
        var line = 1;
        foreach (var input in inputs)
        {
            var drug = drugs[input.DrugId];

            // Ditambahkan lewat DbSet, bukan lewat navigasi induk yang sudah dilacak, agar
            // entity baru pasti berstatus Added walaupun kuncinya diisi dari aplikasi.
            _dbContext.TrxStockRequestItems.Add(new TrxStockRequestItem
            {
                StockRequestId = requestId,
                DrugId = input.DrugId,
                MeasurementId = input.MeasurementId,
                DrugCodeSnapshot = drug.Code,
                DrugNameSnapshot = drug.Name,
                MeasurementNameSnapshot = drug.MeasurementNames.GetValueOrDefault(input.MeasurementId),
                RequestedQuantity = input.RequestedQuantity,
                Note = Normalize(input.Note),
                LineNumber = line++,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
    }

    private sealed record DrugSnapshot(string Code, string Name,
        Dictionary<Guid, string> MeasurementNames);

    /// <summary>
    /// Memastikan seluruh obat dan satuan yang diminta benar-benar ada dan aktif, lalu
    /// mengambil nama serta kodenya sekali saja untuk disalin ke setiap baris.
    /// </summary>
    private async Task<Dictionary<Guid, DrugSnapshot>> ResolveDrugsAsync(
        List<StockRequestItemInput> items, CancellationToken cancellationToken)
    {
        var drugIds = items.Select(x => x.DrugId).Distinct().ToList();
        var measurementIds = items.Select(x => x.MeasurementId).Distinct().ToList();

        var drugs = await _dbContext.MstDrugs.AsNoTracking()
            .Where(x => drugIds.Contains(x.Id) && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.DrugCode, x.DrugName })
            .ToListAsync(cancellationToken);

        if (drugs.Count != drugIds.Count)
            throw new StockRequestUnprocessableException("PHM001",
                "Ada obat yang tidak ditemukan atau tidak aktif pada master farmasi.");

        var measurements = await _dbContext.MstMeasurements.AsNoTracking()
            .Where(x => measurementIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.MeasurementName })
            .ToListAsync(cancellationToken);

        if (measurements.Count != measurementIds.Count)
            throw new StockRequestUnprocessableException("PHM001",
                "Ada satuan yang tidak ditemukan pada master.");

        var measurementNames = measurements.ToDictionary(x => x.Id, x => x.MeasurementName);

        return drugs.ToDictionary(x => x.Id,
            x => new DrugSnapshot(x.DrugCode, x.DrugName, measurementNames));
    }

    private async Task ValidateReferencesAsync(Guid serviceUnitId, Guid storageLocationId,
        Guid workforceId, CancellationToken cancellationToken)
    {
        var unitValid = await _dbContext.MstServiceUnits.AsNoTracking()
            .AnyAsync(x => x.Id == serviceUnitId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!unitValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Unit peminta tidak ditemukan atau tidak aktif.");

        var storageValid = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .AnyAsync(x => x.Id == storageLocationId && !x.IsDelete, cancellationToken);
        if (!storageValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Lokasi penyimpanan tidak ditemukan.");

        var workforceValid = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!workforceValid)
            throw new StockRequestUnprocessableException("PHM001",
                "Petugas peminta tidak ditemukan atau tidak aktif.");
    }

    /// <summary>
    /// Satu obat hanya boleh muncul sekali. Dua baris obat yang sama membuat gudang
    /// menyiapkan dua kali untuk kebutuhan yang satu.
    /// </summary>
    private static void EnsureItemsValid(List<StockRequestItemInput> items)
    {
        if (items.Count == 0)
            throw new StockRequestUnprocessableException("PHM002",
                "Permintaan harus memuat sekurang-kurangnya satu item.");

        if (items.Any(x => x.RequestedQuantity <= 0))
            throw new StockRequestUnprocessableException("PHM003",
                "Jumlah permintaan setiap item harus lebih dari nol.");

        var duplicate = items.GroupBy(x => x.DrugId).Any(g => g.Count() > 1);
        if (duplicate)
            throw new StockRequestUnprocessableException("PHM005",
                "Satu obat hanya boleh muncul satu kali. Gabungkan jumlahnya menjadi satu baris.");
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new StockRequestConflictException("PHM012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key wajib diisi.");
    }

    private static void EnsureSameFingerprint(string source, string fingerprint)
    {
        if (!string.Equals(source, BuildSource(fingerprint), StringComparison.Ordinal))
            throw new StockRequestConflictException("PHM013",
                "Idempotency key dipakai dengan isi permintaan yang berbeda.");
    }

    private Task<TrxStockRequestHistory?> FindIdempotentAsync(string action, string key,
        CancellationToken cancellationToken) =>
        _dbContext.TrxStockRequestHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Action == action && x.CorrelationId == key.Trim() &&
                                      !x.IsDelete, cancellationToken);

    private static TrxStockRequestHistory NewHistory(Guid requestId, StockRequestStatus to,
        StockRequestStatus? from, string action, string? reason, string idempotencyKey,
        string fingerprint, Guid actorUserId, DateTime now) => new()
        {
            StockRequestId = requestId,
            FromStatus = from,
            ToStatus = to,
            Action = action,
            Reason = reason,
            ActorUserId = actorUserId,
            OccurredAt = now,
            Source = BuildSource(fingerprint),
            CorrelationId = idempotencyKey.Trim(),
            CreateDateTime = now,
            CreateBy = actorUserId
        };

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new StockRequestConflictException("PHM012",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new StockRequestForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static string BuildFingerprint(Guid storageLocationId, StockRequestPriority priority,
        DateTime? neededAt, string? notes, List<StockRequestItemInput> items) =>
        Hash(string.Join('|', storageLocationId, (int)priority,
            neededAt?.ToUniversalTime().Ticks, Normalize(notes),
            string.Join(',', items.OrderBy(x => x.DrugId)
                .Select(x => $"{x.DrugId:N}:{x.MeasurementId:N}:{x.RequestedQuantity}"))));

    private static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"StockRequest:{key.Trim()}"))[..16]);

    private static StockRequestDetailResponse MapDetail(TrxStockRequest x) => new()
    {
        Id = x.Id,
        RequestNumber = x.RequestNumber,
        RequestingServiceUnitId = x.RequestingServiceUnitId,
        RequestingServiceUnitName = x.RequestingServiceUnit?.ServiceUnitName ?? string.Empty,
        StorageLocationId = x.StorageLocationId,
        StorageLocationName = x.StorageLocation?.StorageLocationName ?? string.Empty,
        RequestedByWorkforceId = x.RequestedByWorkforceId,
        RequestedByName = x.RequestedByWorkforce?.DisplayName ?? string.Empty,
        Status = x.Status,
        Priority = x.Priority,
        RequestedAt = x.RequestedAt,
        NeededAt = x.NeededAt,
        SubmittedAt = x.SubmittedAt,
        DecidedAt = x.DecidedAt,
        DecisionReason = x.DecisionReason,
        Notes = x.Notes,
        ItemCount = x.ItemCount,
        Version = x.Version,
        IsEditable = x.Status == StockRequestStatus.Draft,
        Items = [.. x.Items.Where(i => !i.IsDelete).OrderBy(i => i.LineNumber)
            .Select(i => new StockRequestItemResponse
            {
                Id = i.Id,
                DrugId = i.DrugId,
                DrugCode = i.DrugCodeSnapshot,
                DrugName = i.DrugNameSnapshot,
                MeasurementId = i.MeasurementId,
                MeasurementName = i.MeasurementNameSnapshot,
                RequestedQuantity = i.RequestedQuantity,
                FulfilledQuantity = i.FulfilledQuantity,
                Note = i.Note,
                LineNumber = i.LineNumber
            })],
        Histories = [.. x.Histories.Where(h => !h.IsDelete).OrderByDescending(h => h.OccurredAt)
            .Select(h => new StockRequestHistoryResponse
            {
                Id = h.Id,
                FromStatus = h.FromStatus,
                ToStatus = h.ToStatus,
                Action = h.Action,
                Reason = h.Reason,
                OccurredAt = h.OccurredAt
            })]
    };
}

/// <summary>Benturan data atau transisi status yang tidak sah; dipetakan ke `409`.</summary>
public sealed class StockRequestConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Pengguna tidak berwenang atas tindakan ini; dipetakan ke `403`.</summary>
public sealed class StockRequestForbiddenException(string message) : Exception(message);

/// <summary>Prasyarat aturan permintaan stok belum terpenuhi; dipetakan ke `422`.</summary>
public sealed class StockRequestUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
