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
/// Pengembalian obat yang sudah diserahkan, kembali ke lokasi penyimpanan.
/// </summary>
/// <remarks>
/// <para>
/// Stok tidak bertambah saat retur diajukan, melainkan setelah diperiksa. Obat yang kembali
/// sudah pernah keluar dari pengawasan farmasi — suhunya, kemasannya, dan keasliannya tidak
/// lagi dijamin. Pemeriksalah yang menentukan berapa yang layak kembali dan dalam keadaan apa.
/// </para>
/// <para>
/// Setiap baris wajib menyebut batchnya. Obat yang kembali harus masuk sebagai batch yang sama
/// dengan asalnya; salah batch berarti tanggal kedaluwarsanya keliru dan penarikan obat tidak
/// lagi dapat menemukannya.
/// </para>
/// </remarks>
public sealed class DrugReturnService
{
    private const string LogCategory = "PharmacyManagement";

    private const string CreateAction = "CreateDrugReturn";
    private const string UpdateAction = "UpdateDrugReturn";
    private const string SubmitAction = "SubmitDrugReturn";
    private const string VerifyAction = "VerifyDrugReturn";
    private const string RejectAction = "RejectDrugReturn";
    private const string CancelAction = "CancelDrugReturn";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly DrugStockService _drugStockService;

    public DrugReturnService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        DrugStockService drugStockService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _drugStockService = drugStockService;
    }

    // ==================================================================== daftar

    public async Task<PagedResult<DrugReturnSummaryResponse>> GetPagedAsync(
        DrugReturnPagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrxDrugReturns.AsNoTracking().Where(x => !x.IsDelete);

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
            query = query.Where(x => x.ReturnedAt >= start);
        }

        if (request.EndDate.HasValue)
        {
            var endExclusive = request.EndDate.Value.Date.ToUniversalTime().AddDays(1);
            query = query.Where(x => x.ReturnedAt < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.ReturnNumber.ToLower().Contains(keyword) ||
                (x.Encounter != null && x.Encounter.Patient != null &&
                 (x.Encounter.Patient.FullName.ToLower().Contains(keyword) ||
                  x.Encounter.Patient.MedicalRecordNumber.ToLower().Contains(keyword))) ||
                x.Items.Any(i => !i.IsDelete && i.DrugNameSnapshot.ToLower().Contains(keyword)));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 100 ? 10 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.ReturnedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DrugReturnSummaryResponse
            {
                Id = x.Id,
                ReturnNumber = x.ReturnNumber,
                EncounterId = x.EncounterId,
                EncounterNumber = x.Encounter != null ? x.Encounter.EncounterNumber : string.Empty,
                PatientId = x.Encounter != null ? x.Encounter.PatientId : Guid.Empty,
                PatientName = x.Encounter != null && x.Encounter.Patient != null
                    ? x.Encounter.Patient.FullName : string.Empty,
                MedicalRecordNumber = x.Encounter != null && x.Encounter.Patient != null
                    ? x.Encounter.Patient.MedicalRecordNumber : string.Empty,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation != null
                    ? x.StorageLocation.StorageLocationName : string.Empty,
                ReturnedByName = x.ReturnedByWorkforce != null
                    ? x.ReturnedByWorkforce.DisplayName : string.Empty,
                VerifiedByName = x.VerifiedByWorkforce != null
                    ? x.VerifiedByWorkforce.DisplayName : null,
                Status = x.Status,
                ReturnedAt = x.ReturnedAt,
                SubmittedAt = x.SubmittedAt,
                VerifiedAt = x.VerifiedAt,
                ItemCount = x.ItemCount,
                Version = x.Version,
                IsEditable = x.Status == DrugReturnStatus.Draft,
                CanSubmit = x.Status == DrugReturnStatus.Draft,
                CanVerify = x.Status == DrugReturnStatus.Submitted,
                CanReject = x.Status == DrugReturnStatus.Submitted,
                CanCancel = x.Status == DrugReturnStatus.Draft ||
                            x.Status == DrugReturnStatus.Submitted
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<DrugReturnSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<DrugReturnDetailResponse?> GetDetailAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(id, tracking: false, cancellationToken);
        return entity == null ? null : MapDetail(entity);
    }

    // ====================================================================== buat

    public async Task<DrugReturnDetailResponse> CreateAsync(CreateDrugReturnRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var id = DeterministicId(request.IdempotencyKey);

        var existing = await _dbContext.TrxDrugReturns.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);
        if (existing != null) return (await GetDetailAsync(id, cancellationToken))!;

        await EnsureEncounterAsync(request.EncounterId, cancellationToken);
        await EnsureWorkforceAsync(request.ReturnedByWorkforceId, cancellationToken);
        await EnsureReceivingLocationAsync(request.StorageLocationId, cancellationToken);

        var drugs = await ResolveItemsAsync(request.Items, cancellationToken);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;
        var fingerprint = BuildFingerprint(request.StorageLocationId, request.Reason, request.Items);

        var entity = new TrxDrugReturn
        {
            Id = id,
            ReturnNumber = $"RTN-{now:yyyyMMdd}-{id.ToString("N")[..6].ToUpperInvariant()}",
            EncounterId = request.EncounterId,
            StorageLocationId = request.StorageLocationId,
            ReturnedByWorkforceId = request.ReturnedByWorkforceId,
            SourceDrugUsageId = request.SourceDrugUsageId,
            SourceOprMaterialUsageId = request.SourceOprMaterialUsageId,
            Status = DrugReturnStatus.Draft,
            ReturnedAt = request.ReturnedAt?.ToUniversalTime() ?? now,
            Reason = Normalize(request.Reason),
            ItemCount = request.Items.Count,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.TrxDrugReturns.Add(entity);
        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        _dbContext.TrxDrugReturnHistories.Add(NewHistory(entity.Id, DrugReturnStatus.Draft, null,
            CreateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    // ====================================================================== ubah

    public async Task<DrugReturnDetailResponse> UpdateAsync(Guid id,
        UpdateDrugReturnRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Retur obat tidak ditemukan.");

        if (entity.Status != DrugReturnStatus.Draft)
            throw new DrugReturnConflictException("PHM070",
                "Retur yang sudah diajukan tidak dapat diubah.");

        EnsureVersion(entity.Version, request.ExpectedVersion);
        await EnsureReceivingLocationAsync(request.StorageLocationId, cancellationToken);

        var drugs = await ResolveItemsAsync(request.Items, cancellationToken);
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
        entity.SourceDrugUsageId = request.SourceDrugUsageId;
        entity.ReturnedAt = request.ReturnedAt?.ToUniversalTime() ?? entity.ReturnedAt;
        entity.Reason = Normalize(request.Reason);
        entity.ItemCount = request.Items.Count;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ==================================================================== ajukan

    public Task<DrugReturnDetailResponse> SubmitAsync(Guid id,
        DrugReturnCommandRequest request, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, request.IdempotencyKey, request.ExpectedVersion, SubmitAction,
            DrugReturnStatus.Draft, DrugReturnStatus.Submitted, null,
            (entity, now) => entity.SubmittedAt = now, cancellationToken);

    public Task<DrugReturnDetailResponse> RejectAsync(Guid id,
        DrugReturnReasonRequest request, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, request.IdempotencyKey, request.ExpectedVersion, RejectAction,
            DrugReturnStatus.Submitted, DrugReturnStatus.Rejected, request.Reason,
            (entity, now) =>
            {
                entity.DecisionReason = request.Reason.Trim();
                entity.VerifiedAt = now;
            }, cancellationToken);

    public Task<DrugReturnDetailResponse> CancelAsync(Guid id,
        DrugReturnReasonRequest request, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, request.IdempotencyKey, request.ExpectedVersion, CancelAction,
            null, DrugReturnStatus.Cancelled, request.Reason,
            (entity, _) => entity.DecisionReason = request.Reason.Trim(), cancellationToken);

    // =================================================================== periksa

    /// <summary>
    /// Memeriksa retur dan mengembalikan barang yang layak ke stok.
    /// </summary>
    /// <remarks>
    /// Setiap baris wajib diputuskan, walaupun nol. Baris yang tidak disebut tidak dianggap
    /// ditolak: diam bisa berarti "tidak layak" atau "lupa diperiksa", dan keduanya berbeda
    /// akibatnya bagi yang mengembalikan.
    /// </remarks>
    public async Task<DrugReturnDetailResponse> VerifyAsync(Guid id,
        VerifyDrugReturnRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var note = Normalize(request.Note);
        var fingerprint = Hash($"verify:{id}|{note}|" + string.Join(',',
            request.Items.OrderBy(x => x.DrugReturnItemId)
                .Select(x => $"{x.DrugReturnItemId:N}:{x.AcceptedQuantity}:{(int)x.AcceptedStatus}")));

        var prior = await FindIdempotentAsync(VerifyAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Retur obat tidak ditemukan.");

        if (entity.Status != DrugReturnStatus.Submitted)
            throw new DrugReturnConflictException("PHM070",
                "Hanya retur yang sudah diajukan yang dapat diperiksa.");

        EnsureVersion(entity.Version, request.ExpectedVersion);
        await EnsureWorkforceAsync(request.VerifiedByWorkforceId, cancellationToken);

        var activeItems = entity.Items.Where(x => !x.IsDelete).ToList();
        EnsureVerificationCoversEveryLine(activeItems, request.Items);

        var byId = request.Items.ToDictionary(x => x.DrugReturnItemId, x => x);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in activeItems.OrderBy(x => x.LineNumber))
        {
            var decision = byId[item.Id];

            if (decision.AcceptedQuantity < 0)
                throw new DrugReturnUnprocessableException("PHM071",
                    "Jumlah yang diterima tidak boleh kurang dari nol.");

            // Menerima lebih banyak daripada yang dikembalikan berarti barang muncul entah
            // dari mana; itu bukan retur melainkan selisih yang harus ditelusuri sendiri.
            if (decision.AcceptedQuantity > item.Quantity)
                throw new DrugReturnUnprocessableException("PHM071",
                    $"Baris {item.LineNumber}: jumlah diterima melebihi jumlah yang dikembalikan.");

            item.AcceptedQuantity = decision.AcceptedQuantity;
            item.AcceptedStatus = decision.AcceptedStatus;
            if (!string.IsNullOrWhiteSpace(decision.Note)) item.Note = decision.Note.Trim();
            item.UpdateDateTime = now;
            item.UpdateBy = actorUserId;

            if (decision.AcceptedQuantity <= 0) continue;

            await _drugStockService.ReceiveBatchAsync(item.DrugBatchId,
                entity.StorageLocationId, decision.AcceptedQuantity,
                DrugStockSourceDocumentTypes.PrescriptionReturn, entity.Id,
                $"Retur {entity.ReturnNumber} baris {item.LineNumber}.",
                $"rtn-{item.Id:N}", cancellationToken, decision.AcceptedStatus);
        }

        entity.Status = DrugReturnStatus.Verified;
        entity.VerifiedAt = now;
        entity.VerifiedByWorkforceId = request.VerifiedByWorkforceId;
        entity.DecisionReason = note;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.TrxDrugReturnHistories.Add(NewHistory(entity.Id, DrugReturnStatus.Verified,
            DrugReturnStatus.Submitted, VerifyAction, note, request.IdempotencyKey,
            fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "DrugReturn.Verify",
            "Memeriksa retur obat dan mengembalikan barang yang layak ke stok.",
            new { entity.Id, entity.ReturnNumber, entity.ItemCount });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ================================================================= penolong

    private async Task<DrugReturnDetailResponse> TransitionAsync(Guid id, string idempotencyKey,
        int expectedVersion, string action, DrugReturnStatus? fromStatus,
        DrugReturnStatus toStatus, string? reason, Action<TrxDrugReturn, DateTime> apply,
        CancellationToken cancellationToken)
    {
        EnsureIdempotencyKey(idempotencyKey);

        if (toStatus is DrugReturnStatus.Rejected or DrugReturnStatus.Cancelled &&
            string.IsNullOrWhiteSpace(reason))
            throw new DrugReturnUnprocessableException("PHM072",
                "Alasan wajib diisi untuk penolakan maupun pembatalan.");

        var fingerprint = Hash($"{action}:{id}|{reason?.Trim()}");

        var prior = await FindIdempotentAsync(action, idempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Retur obat tidak ditemukan.");

        // Pembatalan sah dari dua keadaan, sedangkan perintah lain hanya dari satu.
        if (fromStatus.HasValue)
        {
            if (entity.Status != fromStatus)
                throw new DrugReturnConflictException("PHM070",
                    $"Perintah ini hanya sah pada retur berstatus {fromStatus}.");
        }
        else if (entity.Status is not (DrugReturnStatus.Draft or DrugReturnStatus.Submitted))
        {
            throw new DrugReturnConflictException("PHM070",
                "Retur yang sudah diperiksa tidak dapat dibatalkan.");
        }

        EnsureVersion(entity.Version, expectedVersion);

        if (toStatus == DrugReturnStatus.Submitted && !entity.Items.Any(x => !x.IsDelete))
            throw new DrugReturnUnprocessableException("PHM073",
                "Retur tanpa satu pun item tidak dapat diajukan.");

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;
        var from = entity.Status;

        apply(entity, now);
        entity.Status = toStatus;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.TrxDrugReturnHistories.Add(NewHistory(entity.Id, toStatus, from, action,
            reason?.Trim(), idempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    private Task<TrxDrugReturn?> LoadAsync(Guid id, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.TrxDrugReturns
            .Include(x => x.Encounter).ThenInclude(x => x!.Patient)
            .Include(x => x.StorageLocation)
            .Include(x => x.ReturnedByWorkforce)
            .Include(x => x.VerifiedByWorkforce)
            .Include(x => x.SourceDrugUsage)
            .Include(x => x.Items).ThenInclude(x => x.DrugBatch)
            .Where(x => x.Id == id && !x.IsDelete);

        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    private void AddItems(Guid returnId, List<DrugReturnItemInput> inputs,
        Dictionary<Guid, ItemInfo> info, Guid actorUserId, DateTime now)
    {
        var line = 1;
        foreach (var input in inputs)
        {
            var drug = info[input.DrugId];
            _dbContext.TrxDrugReturnItems.Add(new TrxDrugReturnItem
            {
                DrugReturnId = returnId,
                DrugId = input.DrugId,
                DrugBatchId = input.DrugBatchId,
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

    private sealed record ItemInfo(string Code, string Name, Dictionary<Guid, string> MeasurementNames);

    private async Task<Dictionary<Guid, ItemInfo>> ResolveItemsAsync(
        List<DrugReturnItemInput> items, CancellationToken cancellationToken)
    {
        var drugIds = items.Select(x => x.DrugId).Distinct().ToList();
        var batchIds = items.Select(x => x.DrugBatchId).Distinct().ToList();
        var measurementIds = items.Select(x => x.MeasurementId).Distinct().ToList();

        var drugs = await _dbContext.MstDrugs.AsNoTracking()
            .Where(x => drugIds.Contains(x.Id) && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.DrugCode, x.DrugName })
            .ToListAsync(cancellationToken);

        if (drugs.Count != drugIds.Count)
            throw new DrugReturnUnprocessableException("PHM074",
                "Ada obat yang tidak ditemukan atau tidak aktif.");

        var batches = await _dbContext.MstDrugBatches.AsNoTracking()
            .Where(x => batchIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.DrugId })
            .ToListAsync(cancellationToken);

        if (batches.Count != batchIds.Count)
            throw new DrugReturnUnprocessableException("PHM074",
                "Ada batch yang tidak ditemukan.");

        // Batch harus milik obat yang disebut pada baris yang sama. Batch milik obat lain
        // akan mengembalikan barang ke tempat yang keliru dan merusak penelusurannya.
        var batchOwner = batches.ToDictionary(x => x.Id, x => x.DrugId);
        foreach (var item in items)
        {
            if (batchOwner[item.DrugBatchId] != item.DrugId)
                throw new DrugReturnUnprocessableException("PHM075",
                    "Batch yang dipilih bukan milik obat pada baris tersebut.");
        }

        var measurements = await _dbContext.MstMeasurements.AsNoTracking()
            .Where(x => measurementIds.Contains(x.Id) && !x.IsDelete)
            .Select(x => new { x.Id, x.MeasurementName })
            .ToListAsync(cancellationToken);

        if (measurements.Count != measurementIds.Count)
            throw new DrugReturnUnprocessableException("PHM074",
                "Ada satuan yang tidak ditemukan pada master.");

        var names = measurements.ToDictionary(x => x.Id, x => x.MeasurementName);
        return drugs.ToDictionary(x => x.Id, x => new ItemInfo(x.DrugCode, x.DrugName, names));
    }

    private async Task EnsureEncounterAsync(Guid encounterId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.TrxPatientEncounters.AsNoTracking()
            .AnyAsync(x => x.Id == encounterId && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new DrugReturnUnprocessableException("PHM074", "Kunjungan pasien tidak ditemukan.");
    }

    private async Task EnsureWorkforceAsync(Guid workforceId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new DrugReturnUnprocessableException("PHM074",
                "Petugas tidak ditemukan atau tidak aktif.");
    }

    private async Task EnsureReceivingLocationAsync(Guid storageLocationId,
        CancellationToken cancellationToken)
    {
        var location = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == storageLocationId && !x.IsDelete, cancellationToken)
            ?? throw new DrugReturnUnprocessableException("PHM074",
                "Lokasi penyimpanan tidak ditemukan.");

        if (!location.IsActive)
            throw new DrugReturnUnprocessableException("PHM076",
                $"Lokasi {location.StorageLocationName} sudah tidak aktif.");

        if (!location.IsAllowReceiving)
            throw new DrugReturnUnprocessableException("PHM076",
                $"Lokasi {location.StorageLocationName} tidak diizinkan menerima barang.");
    }

    private static void EnsureItemsValid(List<DrugReturnItemInput> items)
    {
        if (items.Count == 0)
            throw new DrugReturnUnprocessableException("PHM073",
                "Retur harus memuat sekurang-kurangnya satu item.");

        if (items.Any(x => x.Quantity <= 0))
            throw new DrugReturnUnprocessableException("PHM077",
                "Jumlah setiap item harus lebih dari nol.");

        if (items.GroupBy(x => x.DrugBatchId).Any(g => g.Count() > 1))
            throw new DrugReturnUnprocessableException("PHM078",
                "Satu batch hanya boleh disebut satu kali. Gabungkan jumlahnya menjadi satu baris.");
    }

    private static void EnsureVerificationCoversEveryLine(List<TrxDrugReturnItem> activeItems,
        List<VerifyDrugReturnItemInput> inputs)
    {
        if (inputs.GroupBy(x => x.DrugReturnItemId).Any(g => g.Count() > 1))
            throw new DrugReturnUnprocessableException("PHM079",
                "Satu baris hanya boleh diputuskan satu kali.");

        var activeIds = activeItems.Select(x => x.Id).ToHashSet();
        var inputIds = inputs.Select(x => x.DrugReturnItemId).ToHashSet();

        if (inputIds.Except(activeIds).Any())
            throw new DrugReturnUnprocessableException("PHM079",
                "Ada baris pemeriksaan yang bukan bagian dari retur ini.");

        if (activeIds.Except(inputIds).Any())
            throw new DrugReturnUnprocessableException("PHM079",
                "Setiap baris harus diputuskan, walaupun diterima nol.");
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new DrugReturnConflictException("PHM080",
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
            throw new DrugReturnConflictException("PHM081",
                "Idempotency key dipakai dengan isi perintah yang berbeda.");
    }

    private Task<TrxDrugReturnHistory?> FindIdempotentAsync(string action, string key,
        CancellationToken cancellationToken) =>
        _dbContext.TrxDrugReturnHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Action == action && x.CorrelationId == key.Trim() &&
                                      !x.IsDelete, cancellationToken);

    private static TrxDrugReturnHistory NewHistory(Guid returnId, DrugReturnStatus to,
        DrugReturnStatus? from, string action, string? reason, string idempotencyKey,
        string fingerprint, Guid actorUserId, DateTime now) => new()
        {
            DrugReturnId = returnId,
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
            throw new DrugReturnConflictException("PHM080",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new DrugReturnForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static string BuildFingerprint(Guid storageLocationId, string? reason,
        List<DrugReturnItemInput> items) =>
        Hash(string.Join('|', storageLocationId, Normalize(reason),
            string.Join(',', items.OrderBy(x => x.DrugBatchId)
                .Select(x => $"{x.DrugBatchId:N}:{x.Quantity}"))));

    private static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"DrugReturn:{key.Trim()}"))[..16]);

    private static DrugReturnDetailResponse MapDetail(TrxDrugReturn x) => new()
    {
        Id = x.Id,
        ReturnNumber = x.ReturnNumber,
        EncounterId = x.EncounterId,
        EncounterNumber = x.Encounter?.EncounterNumber ?? string.Empty,
        PatientId = x.Encounter?.PatientId ?? Guid.Empty,
        PatientName = x.Encounter?.Patient?.FullName ?? string.Empty,
        MedicalRecordNumber = x.Encounter?.Patient?.MedicalRecordNumber ?? string.Empty,
        StorageLocationId = x.StorageLocationId,
        StorageLocationName = x.StorageLocation?.StorageLocationName ?? string.Empty,
        ReturnedByWorkforceId = x.ReturnedByWorkforceId,
        ReturnedByName = x.ReturnedByWorkforce?.DisplayName ?? string.Empty,
        VerifiedByWorkforceId = x.VerifiedByWorkforceId,
        VerifiedByName = x.VerifiedByWorkforce?.DisplayName,
        SourceDrugUsageId = x.SourceDrugUsageId,
        SourceUsageNumber = x.SourceDrugUsage?.UsageNumber,
        Status = x.Status,
        ReturnedAt = x.ReturnedAt,
        SubmittedAt = x.SubmittedAt,
        VerifiedAt = x.VerifiedAt,
        Reason = x.Reason,
        DecisionReason = x.DecisionReason,
        ItemCount = x.ItemCount,
        Version = x.Version,
        IsEditable = x.Status == DrugReturnStatus.Draft,
        CanSubmit = x.Status == DrugReturnStatus.Draft,
        CanVerify = x.Status == DrugReturnStatus.Submitted,
        CanReject = x.Status == DrugReturnStatus.Submitted,
        CanCancel = x.Status is DrugReturnStatus.Draft or DrugReturnStatus.Submitted,
        Items = [.. x.Items.Where(i => !i.IsDelete).OrderBy(i => i.LineNumber)
            .Select(i => new DrugReturnItemResponse
            {
                Id = i.Id,
                DrugId = i.DrugId,
                DrugCode = i.DrugCodeSnapshot,
                DrugName = i.DrugNameSnapshot,
                DrugBatchId = i.DrugBatchId,
                BatchNumber = i.DrugBatch?.BatchNumber ?? string.Empty,
                ExpiryDate = i.DrugBatch?.ExpiryDate,
                MeasurementId = i.MeasurementId,
                MeasurementName = i.MeasurementNameSnapshot,
                Quantity = i.Quantity,
                AcceptedQuantity = i.AcceptedQuantity,
                AcceptedStatus = i.AcceptedStatus,
                Note = i.Note,
                LineNumber = i.LineNumber
            })]
    };
}

/// <summary>Benturan atau transisi tidak sah; dipetakan ke `409`.</summary>
public sealed class DrugReturnConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Pengguna tidak berwenang; dipetakan ke `403`.</summary>
public sealed class DrugReturnForbiddenException(string message) : Exception(message);

/// <summary>Prasyarat aturan belum terpenuhi; dipetakan ke `422`.</summary>
public sealed class DrugReturnUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
