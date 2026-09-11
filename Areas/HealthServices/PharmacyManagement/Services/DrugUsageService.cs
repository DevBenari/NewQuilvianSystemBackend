using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Pencatatan pemakaian obat dan alat kesehatan untuk pasien.
/// </summary>
/// <remarks>
/// <para>
/// Pemakaian disusun dahulu sebagai draft, baru kemudian dicatat. Stok berkurang pada saat
/// pencatatan, bukan saat draft dibuat — sehingga baris yang salah ketik masih dapat
/// diperbaiki selagi belum menyentuh persediaan.
/// </para>
/// <para>
/// Sesudah dicatat, pemakaian berhenti pada keadaan "belum ditagihkan". Ia adalah transaksi
/// yang <em>dapat</em> ditagihkan; keputusan menagih beserta aturannya milik Billing, bukan
/// Farmasi.
/// </para>
/// <para>
/// Batch yang terpakai disimpan per baris. Ketika sebuah batch ditarik dari peredaran,
/// pertanyaannya bukan berapa sisanya di gudang melainkan siapa saja yang sudah menerimanya,
/// dan hanya catatan itu yang dapat menjawabnya.
/// </para>
/// </remarks>
public sealed class DrugUsageService
{
    private const string LogCategory = "PharmacyManagement";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly DrugStockService _drugStockService;

    public DrugUsageService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        DrugStockService drugStockService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _drugStockService = drugStockService;
    }

    // ==================================================================== daftar

    public async Task<PagedResult<DrugUsageSummaryResponse>> GetPagedAsync(
        DrugUsagePagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PhmDrugUsages.AsNoTracking().Where(x => !x.IsDelete);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.EncounterId.HasValue)
            query = query.Where(x => x.EncounterId == request.EncounterId);
        if (request.PatientId.HasValue)
            query = query.Where(x => x.Encounter!.PatientId == request.PatientId);
        if (request.StorageLocationId.HasValue)
            query = query.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (request.DrugId.HasValue)
            query = query.Where(x => x.Items.Any(i => i.DrugId == request.DrugId && !i.IsDelete));

        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date.ToUniversalTime();
            query = query.Where(x => x.UsedAt >= start);
        }

        if (request.EndDate.HasValue)
        {
            var endExclusive = request.EndDate.Value.Date.ToUniversalTime().AddDays(1);
            query = query.Where(x => x.UsedAt < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.UsageNumber.ToLower().Contains(keyword) ||
                (x.Encounter != null && x.Encounter.Patient != null &&
                 (x.Encounter.Patient.FullName.ToLower().Contains(keyword) ||
                  x.Encounter.Patient.MedicalRecordNumber.ToLower().Contains(keyword))) ||
                x.Items.Any(i => !i.IsDelete && i.DrugNameSnapshot.ToLower().Contains(keyword)));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.UsedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DrugUsageSummaryResponse
            {
                Id = x.Id,
                UsageNumber = x.UsageNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                PatientId = x.Encounter != null ? x.Encounter.PatientId : Guid.Empty,
                PatientName = x.Encounter != null && x.Encounter.Patient != null
                    ? x.Encounter.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Encounter != null && x.Encounter.Patient != null
                    ? x.Encounter.Patient.MedicalRecordNumber : string.Empty,
                ServiceUnitName = x.Encounter != null && x.Encounter.ServiceUnit != null
                    ? x.Encounter.ServiceUnit.ServiceUnitName : string.Empty,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation != null
                    ? x.StorageLocation.StorageLocationName : string.Empty,
                RecordedByName = x.RecordedByWorkforce != null
                    ? x.RecordedByWorkforce.DisplayName : string.Empty,
                Status = x.Status,
                UsedAt = x.UsedAt,
                RecordedAt = x.RecordedAt,
                BilledAt = x.BilledAt,
                ItemCount = x.ItemCount,
                Version = x.Version,
                IsEditable = x.Status == DrugUsageStatus.Draft,
                CanRecord = x.Status == DrugUsageStatus.Draft,
                CanCancel = x.Status == DrugUsageStatus.Draft
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DrugUsageSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<DrugUsageDetailResponse?> GetDetailAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(id, tracking: false, cancellationToken);
        return entity == null ? null : MapDetail(entity);
    }

    // ====================================================================== buat

    public async Task<DrugUsageDetailResponse> CreateAsync(CreateDrugUsageRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var id = DeterministicId(request.IdempotencyKey);

        var existing = await _dbContext.PhmDrugUsages.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        if (existing != null) return (await GetDetailAsync(id, cancellationToken))!;

        await EnsureEncounterAsync(request.EncounterId, cancellationToken);
        await EnsureWorkforceAsync(request.RecordedByWorkforceId, cancellationToken);

        // Stok hanya boleh keluar dari lokasi yang memang diizinkan menyerahkan. Unit tempat
        // pasien dirawat tidak menjadi sumber stok; ia hanya konteks kunjungan.
        await EnsureDispensingLocationAsync(request.StorageLocationId, cancellationToken);

        var drugs = await ResolveDrugsAsync(request.Items, cancellationToken);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var entity = new PhmDrugUsage
        {
            Id = id,
            UsageNumber = $"USG-{now:yyyyMMdd}-{id.ToString("N")[..6].ToUpperInvariant()}",
            EncounterId = request.EncounterId,
            StorageLocationId = request.StorageLocationId,
            RecordedByWorkforceId = request.RecordedByWorkforceId,
            Status = DrugUsageStatus.Draft,
            UsedAt = request.UsedAt?.ToUniversalTime() ?? now,
            Notes = Normalize(request.Notes),
            ItemCount = request.Items.Count,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.PhmDrugUsages.Add(entity);
        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    // ====================================================================== ubah

    public async Task<DrugUsageDetailResponse> UpdateAsync(Guid id,
        UpdateDrugUsageRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Pemakaian obat tidak ditemukan.");

        if (entity.Status != DrugUsageStatus.Draft)
            throw new DrugUsageConflictException("PHM060",
                "Pemakaian yang sudah dicatat tidak dapat diubah. Stoknya sudah berkurang.");

        EnsureVersion(entity.Version, request.ExpectedVersion);
        await EnsureDispensingLocationAsync(request.StorageLocationId, cancellationToken);

        var drugs = await ResolveDrugsAsync(request.Items, cancellationToken);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in entity.Items.Where(x => !x.IsDelete))
        {
            item.IsDelete = true;
            item.DeleteDateTime = now;
            item.DeleteBy = actorUserId;
        }

        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        entity.StorageLocationId = request.StorageLocationId;
        entity.UsedAt = request.UsedAt?.ToUniversalTime() ?? entity.UsedAt;
        entity.Notes = Normalize(request.Notes);
        entity.ItemCount = request.Items.Count;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ==================================================================== catat

    /// <summary>
    /// Mencatat pemakaian: stok berkurang, dan transaksinya siap ditagihkan.
    /// </summary>
    /// <remarks>
    /// Batch diambil menurut kedaluwarsa terdekat dan disimpan per baris. Bila stok satu
    /// obat saja tidak mencukupi, seluruh pencatatan dibatalkan — sebagian obat yang
    /// terlanjur berkurang untuk pemakaian yang tidak jadi tercatat adalah selisih yang
    /// tidak akan pernah bisa dijelaskan.
    /// </remarks>
    public async Task<DrugUsageDetailResponse> RecordAsync(Guid id,
        DrugUsageCommandRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Pemakaian obat tidak ditemukan.");

        if (entity.Status == DrugUsageStatus.NotBilled || entity.Status == DrugUsageStatus.Billed)
            return (await GetDetailAsync(id, cancellationToken))!;

        if (entity.Status != DrugUsageStatus.Draft)
            throw new DrugUsageConflictException("PHM060",
                "Hanya pemakaian berstatus draft yang dapat dicatat.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var activeItems = entity.Items.Where(x => !x.IsDelete).OrderBy(x => x.LineNumber).ToList();
        if (activeItems.Count == 0)
            throw new DrugUsageUnprocessableException("PHM061",
                "Pemakaian tanpa satu pun item tidak dapat dicatat.");

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in activeItems)
        {
            List<(Guid DrugBatchId, decimal Quantity)> plan;
            try
            {
                plan = await _drugStockService.PlanFefoAsync(item.DrugId,
                    entity.StorageLocationId, item.Quantity, cancellationToken);
            }
            catch (DrugStockUnprocessableException ex)
            {
                throw new DrugUsageUnprocessableException(ex.Code,
                    $"{item.DrugNameSnapshot}: {ex.Message}");
            }

            var sequence = 1;
            foreach (var (batchId, quantity) in plan)
            {
                try
                {
                    await _drugStockService.IssueBatchAsync(batchId, entity.StorageLocationId,
                        quantity, consumeReservation: false,
                        DrugStockSourceDocumentTypes.DrugUsage, entity.Id,
                        $"Pemakaian {entity.UsageNumber} baris {item.LineNumber}.",
                        $"usg-{item.Id:N}-{sequence}", cancellationToken);
                }
                catch (DrugStockUnprocessableException ex)
                {
                    throw new DrugUsageUnprocessableException(ex.Code,
                        $"{item.DrugNameSnapshot}: {ex.Message}");
                }

                _dbContext.PhmDrugUsageAllocations.Add(new PhmDrugUsageAllocation
                {
                    DrugUsageItemId = item.Id,
                    DrugBatchId = batchId,
                    SequenceNumber = sequence++,
                    Quantity = quantity,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }
        }

        entity.Status = DrugUsageStatus.NotBilled;
        entity.RecordedAt = now;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "DrugUsage.Record",
            "Mencatat pemakaian obat pasien dan mengurangi stok.",
            new { entity.Id, entity.UsageNumber, entity.EncounterId, entity.ItemCount });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ==================================================================== batal

    /// <summary>
    /// Membatalkan pemakaian yang masih draft.
    /// </summary>
    /// <remarks>
    /// Pemakaian yang sudah dicatat tidak dapat dibatalkan dari sini. Stoknya sudah keluar
    /// dan mungkin sudah masuk hitungan tagihan; mengembalikannya adalah koreksi stok
    /// tersendiri yang wajib meninggalkan jejaknya sendiri.
    /// </remarks>
    public async Task<DrugUsageDetailResponse> CancelAsync(Guid id,
        CancelDrugUsageRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new DrugUsageUnprocessableException("PHM062",
                "Alasan pembatalan wajib diisi.");

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Pemakaian obat tidak ditemukan.");

        if (entity.Status == DrugUsageStatus.Cancelled)
            return (await GetDetailAsync(id, cancellationToken))!;

        if (entity.Status != DrugUsageStatus.Draft)
            throw new DrugUsageConflictException("PHM060",
                "Pemakaian yang sudah dicatat tidak dapat dibatalkan. Stoknya sudah berkurang; " +
                "perbaiki lewat koreksi stok.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        entity.Status = DrugUsageStatus.Cancelled;
        entity.CancelReason = request.Reason.Trim();
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ================================================================= penolong

    private Task<PhmDrugUsage?> LoadAsync(Guid id, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PhmDrugUsages
            .Include(x => x.Encounter).ThenInclude(x => x!.Patient)
            .Include(x => x.Encounter).ThenInclude(x => x!.ServiceUnit)
            .Include(x => x.StorageLocation)
            .Include(x => x.RecordedByWorkforce)
            .Include(x => x.Items).ThenInclude(x => x.Allocations).ThenInclude(x => x.DrugBatch)
            .Where(x => x.Id == id && !x.IsDelete);

        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    private void AddItems(Guid usageId, List<DrugUsageItemInput> inputs,
        Dictionary<Guid, DrugInfo> drugs, Guid actorUserId, DateTime now)
    {
        var line = 1;
        foreach (var input in inputs)
        {
            var drug = drugs[input.DrugId];
            _dbContext.PhmDrugUsageItems.Add(new PhmDrugUsageItem
            {
                DrugUsageId = usageId,
                DrugId = input.DrugId,
                MeasurementId = input.MeasurementId,
                DrugCodeSnapshot = drug.Code,
                DrugNameSnapshot = drug.Name,
                MeasurementNameSnapshot = drug.MeasurementNames.GetValueOrDefault(input.MeasurementId),
                Quantity = input.Quantity,
                Note = Normalize(input.Note),
                LineNumber = line++,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
    }

    private sealed record DrugInfo(string Code, string Name, Dictionary<Guid, string> MeasurementNames);

    private async Task<Dictionary<Guid, DrugInfo>> ResolveDrugsAsync(
        List<DrugUsageItemInput> items, CancellationToken cancellationToken)
    {
        var drugIds = items.Select(x => x.DrugId).Distinct().ToList();
        var measurementIds = items.Select(x => x.MeasurementId).Distinct().ToList();

        var drugs = await _dbContext.MstDrugs.AsNoTracking()
            .Where(x => drugIds.Contains(x.Id) && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.DrugCode, x.DrugName })
            .ToListAsync(cancellationToken);

        if (drugs.Count != drugIds.Count)
            throw new DrugUsageUnprocessableException("PHM063",
                "Ada obat yang tidak ditemukan atau tidak aktif.");

        var measurements = await _dbContext.MstMeasurements.AsNoTracking()
            .Where(x => measurementIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.MeasurementName })
            .ToListAsync(cancellationToken);

        if (measurements.Count != measurementIds.Count)
            throw new DrugUsageUnprocessableException("PHM063",
                "Ada satuan yang tidak ditemukan pada master.");

        var names = measurements.ToDictionary(x => x.Id, x => x.MeasurementName);
        return drugs.ToDictionary(x => x.Id, x => new DrugInfo(x.DrugCode, x.DrugName, names));
    }

    private async Task EnsureEncounterAsync(Guid encounterId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.RegPatientEncounters.AsNoTracking()
            .AnyAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new DrugUsageUnprocessableException("PHM063",
                "Kunjungan pasien tidak ditemukan.");
    }

    private async Task EnsureWorkforceAsync(Guid workforceId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new DrugUsageUnprocessableException("PHM063",
                "Petugas pencatat tidak ditemukan atau tidak aktif.");
    }

    private async Task EnsureDispensingLocationAsync(Guid storageLocationId,
        CancellationToken cancellationToken)
    {
        var location = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == storageLocationId && !x.IsDelete, cancellationToken)
            ?? throw new DrugUsageUnprocessableException("PHM063",
                "Lokasi penyimpanan tidak ditemukan.");

        if (!location.IsActive)
            throw new DrugUsageUnprocessableException("PHM064",
                $"Lokasi {location.StorageLocationName} sudah tidak aktif.");

        if (!location.IsAllowDispensing)
            throw new DrugUsageUnprocessableException("PHM064",
                $"Lokasi {location.StorageLocationName} tidak diizinkan menyerahkan obat.");
    }

    private static void EnsureItemsValid(List<DrugUsageItemInput> items)
    {
        if (items.Count == 0)
            throw new DrugUsageUnprocessableException("PHM061",
                "Pemakaian harus memuat sekurang-kurangnya satu item.");

        if (items.Any(x => x.Quantity <= 0))
            throw new DrugUsageUnprocessableException("PHM065",
                "Jumlah setiap item harus lebih dari nol.");
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new DrugUsageConflictException("PHM066",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
    }

    private static void EnsureIdempotencyKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Idempotency key wajib diisi.");
    }

    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new DrugUsageConflictException("PHM066",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new DrugUsageForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"DrugUsage:{key.Trim()}"))[..16]);

    private static DrugUsageDetailResponse MapDetail(PhmDrugUsage x) => new()
    {
        Id = x.Id,
        UsageNumber = x.UsageNumber,
        EncounterId = x.EncounterId,
        EncounterNumber = x.Encounter?.EncounterNumber ?? string.Empty,
        PatientId = x.Encounter?.PatientId ?? Guid.Empty,
        PatientName = x.Encounter?.Patient?.FullName ?? string.Empty,
        MedicalRecordNumber = x.Encounter?.Patient?.MedicalRecordNumber ?? string.Empty,
        ServiceUnitName = x.Encounter?.ServiceUnit?.ServiceUnitName ?? string.Empty,
        StorageLocationId = x.StorageLocationId,
        StorageLocationName = x.StorageLocation?.StorageLocationName ?? string.Empty,
        RecordedByWorkforceId = x.RecordedByWorkforceId,
        RecordedByName = x.RecordedByWorkforce?.DisplayName ?? string.Empty,
        Status = x.Status,
        UsedAt = x.UsedAt,
        RecordedAt = x.RecordedAt,
        BilledAt = x.BilledAt,
        Notes = x.Notes,
        CancelReason = x.CancelReason,
        ItemCount = x.ItemCount,
        Version = x.Version,
        IsEditable = x.Status == DrugUsageStatus.Draft,
        CanRecord = x.Status == DrugUsageStatus.Draft,
        CanCancel = x.Status == DrugUsageStatus.Draft,
        Items = [.. x.Items.Where(i => !i.IsDelete).OrderBy(i => i.LineNumber)
            .Select(i => new DrugUsageItemResponse
            {
                Id = i.Id,
                DrugId = i.DrugId,
                DrugCode = i.DrugCodeSnapshot,
                DrugName = i.DrugNameSnapshot,
                MeasurementId = i.MeasurementId,
                MeasurementName = i.MeasurementNameSnapshot,
                Quantity = i.Quantity,
                Note = i.Note,
                LineNumber = i.LineNumber,
                Allocations = [.. i.Allocations.Where(a => !a.IsDelete)
                    .OrderBy(a => a.SequenceNumber)
                    .Select(a => new DrugUsageAllocationResponse
                    {
                        DrugBatchId = a.DrugBatchId,
                        BatchNumber = a.DrugBatch != null ? a.DrugBatch.BatchNumber : string.Empty,
                        ExpiryDate = a.DrugBatch != null ? a.DrugBatch.ExpiryDate : null,
                        Quantity = a.Quantity
                    })]
            })]
    };
}

/// <summary>Benturan atau transisi tidak sah; dipetakan ke `409`.</summary>
public sealed class DrugUsageConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Pengguna tidak berwenang; dipetakan ke `403`.</summary>
public sealed class DrugUsageForbiddenException(string message) : Exception(message);

/// <summary>Prasyarat aturan belum terpenuhi; dipetakan ke `422`.</summary>
public sealed class DrugUsageUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
