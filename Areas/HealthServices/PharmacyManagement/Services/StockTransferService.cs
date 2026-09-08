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
/// Perpindahan stok antar lokasi penyimpanan.
/// </summary>
/// <remarks>
/// <para>
/// Alurnya: diajukan, disetujui sekaligus stoknya ditahan, dikeluarkan dari lokasi asal, lalu
/// diterima lokasi tujuan. Dua langkah terakhir sengaja terpisah karena di antara keduanya
/// barang sedang di jalan — sudah tidak ada di asal, belum ada di tujuan.
/// </para>
/// <para>
/// Kedua ujung wajib lokasi penyimpanan yang memang diizinkan mengirim dan menerima. Unit
/// pelayanan seperti ICU atau kamar operasi bukan lokasi penyimpanan; unit semacam itu
/// memperoleh obat lewat permintaan kepada depo, bukan lewat perpindahan saldo.
/// </para>
/// </remarks>
public sealed class StockTransferService
{
    private const string LogCategory = "PharmacyManagement";

    private const string CreateAction = "CreateStockTransfer";
    private const string UpdateAction = "UpdateStockTransfer";
    private const string SubmitAction = "SubmitStockTransfer";
    private const string ApproveAction = "ApproveStockTransfer";
    private const string RejectAction = "RejectStockTransfer";
    private const string IssueAction = "IssueStockTransfer";
    private const string ReceiveAction = "ReceiveStockTransfer";
    private const string CancelAction = "CancelStockTransfer";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;
    private readonly DrugStockService _drugStockService;

    public StockTransferService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService,
        DrugStockService drugStockService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
        _drugStockService = drugStockService;
    }

    // ==================================================================== daftar

    public async Task<PagedResult<StockTransferSummaryResponse>> GetPagedAsync(
        StockTransferPagedQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.PhmStockTransfers.AsNoTracking().Where(x => !x.IsDelete);

        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.SourceStorageLocationId.HasValue)
            query = query.Where(x => x.SourceStorageLocationId == request.SourceStorageLocationId);
        if (request.DestinationStorageLocationId.HasValue)
            query = query.Where(x =>
                x.DestinationStorageLocationId == request.DestinationStorageLocationId);

        if (request.DrugId.HasValue)
            query = query.Where(x => x.Items.Any(i => i.DrugId == request.DrugId && !i.IsDelete));

        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date.ToUniversalTime();
            query = query.Where(x => x.RequestedAt >= start);
        }

        if (request.EndDate.HasValue)
        {
            var endExclusive = request.EndDate.Value.Date.ToUniversalTime().AddDays(1);
            query = query.Where(x => x.RequestedAt < endExclusive);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.TransferNumber.ToLower().Contains(keyword) ||
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
            .Select(x => new StockTransferSummaryResponse
            {
                Id = x.Id,
                TransferNumber = x.TransferNumber,
                SourceStorageLocationId = x.SourceStorageLocationId,
                SourceStorageLocationName = x.SourceStorageLocation != null
                    ? x.SourceStorageLocation.StorageLocationName : string.Empty,
                DestinationStorageLocationId = x.DestinationStorageLocationId,
                DestinationStorageLocationName = x.DestinationStorageLocation != null
                    ? x.DestinationStorageLocation.StorageLocationName : string.Empty,
                Status = x.Status,
                RequestedAt = x.RequestedAt,
                SubmittedAt = x.SubmittedAt,
                ApprovedAt = x.ApprovedAt,
                IssuedAt = x.IssuedAt,
                ReceivedAt = x.ReceivedAt,
                ItemCount = x.ItemCount,
                Version = x.Version,
                IsEditable = x.Status == StockTransferStatus.Draft,
                CanSubmit = x.Status == StockTransferStatus.Draft,
                CanApprove = x.Status == StockTransferStatus.Requested,
                CanReject = x.Status == StockTransferStatus.Requested,
                CanIssue = x.Status == StockTransferStatus.Approved,
                CanReceive = x.Status == StockTransferStatus.InTransit,
                CanCancel = x.Status == StockTransferStatus.Draft ||
                            x.Status == StockTransferStatus.Requested ||
                            x.Status == StockTransferStatus.Approved
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<StockTransferSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    public async Task<StockTransferDetailResponse?> GetDetailAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await LoadAsync(id, tracking: false, cancellationToken);
        return entity == null ? null : MapDetail(entity);
    }

    // ==================================================================== buat

    public async Task<StockTransferDetailResponse> CreateAsync(CreateStockTransferRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        if (request.SourceStorageLocationId == request.DestinationStorageLocationId)
            throw new StockTransferUnprocessableException("PHM040",
                "Lokasi asal dan tujuan tidak boleh sama.");

        var fingerprint = BuildFingerprint(request.DestinationStorageLocationId, request.Notes,
            request.Items);

        var prior = await FindIdempotentAsync(CreateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(prior.StockTransferId, cancellationToken))!;
        }

        // Kedua ujung wajib lokasi yang memang boleh mengirim dan menerima. Inilah yang
        // mencegah unit tanpa saldo sendiri menjadi ujung perpindahan stok.
        await _drugStockService.EnsureLocationCapabilityAsync(request.SourceStorageLocationId,
            requireTransferOut: true, requireTransferIn: false, cancellationToken);
        await _drugStockService.EnsureLocationCapabilityAsync(request.DestinationStorageLocationId,
            requireTransferOut: false, requireTransferIn: true, cancellationToken);

        await EnsureWorkforceAsync(request.RequestedByWorkforceId, cancellationToken);

        var drugs = await ResolveDrugsAsync(request.Items.Select(x => x.DrugId), cancellationToken);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;
        var id = DeterministicId(request.IdempotencyKey);

        var entity = new PhmStockTransfer
        {
            Id = id,
            TransferNumber = $"TRF-{now:yyyyMMdd}-{id.ToString("N")[..6].ToUpperInvariant()}",
            SourceStorageLocationId = request.SourceStorageLocationId,
            DestinationStorageLocationId = request.DestinationStorageLocationId,
            RequestedByWorkforceId = request.RequestedByWorkforceId,
            Status = StockTransferStatus.Draft,
            Notes = Normalize(request.Notes),
            RequestedAt = now,
            ItemCount = request.Items.Count,
            Version = 0,
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.PhmStockTransfers.Add(entity);
        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        _dbContext.PhmStockTransferHistories.Add(NewHistory(entity.Id, StockTransferStatus.Draft,
            null, CreateAction, null, request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockTransfer.Create",
            "Membuat transfer stok antar lokasi.",
            new { entity.Id, entity.TransferNumber, entity.ItemCount });

        return (await GetDetailAsync(entity.Id, cancellationToken))!;
    }

    // =================================================================== ubah

    public async Task<StockTransferDetailResponse> UpdateAsync(Guid id,
        UpdateStockTransferRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        EnsureItemsValid(request.Items);

        var fingerprint = BuildFingerprint(request.DestinationStorageLocationId, request.Notes,
            request.Items);

        var prior = await FindIdempotentAsync(UpdateAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Transfer stok tidak ditemukan.");

        if (entity.Status != StockTransferStatus.Draft)
            throw new StockTransferConflictException("PHM041",
                "Transfer yang sudah diajukan tidak dapat diubah. Batalkan lalu buat baru.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        if (request.DestinationStorageLocationId == entity.SourceStorageLocationId)
            throw new StockTransferUnprocessableException("PHM040",
                "Lokasi asal dan tujuan tidak boleh sama.");

        await _drugStockService.EnsureLocationCapabilityAsync(request.DestinationStorageLocationId,
            requireTransferOut: false, requireTransferIn: true, cancellationToken);

        var drugs = await ResolveDrugsAsync(request.Items.Select(x => x.DrugId), cancellationToken);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var existing in entity.Items.Where(x => !x.IsDelete))
        {
            existing.IsDelete = true;
            existing.DeleteDateTime = now;
            existing.DeleteBy = actorUserId;
        }

        AddItems(entity.Id, request.Items, drugs, actorUserId, now);

        entity.DestinationStorageLocationId = request.DestinationStorageLocationId;
        entity.Notes = Normalize(request.Notes);
        entity.ItemCount = request.Items.Count;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockTransferHistories.Add(NewHistory(entity.Id, entity.Status,
            entity.Status, UpdateAction, null, request.IdempotencyKey, fingerprint,
            actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ================================================================= ajukan

    public Task<StockTransferDetailResponse> SubmitAsync(Guid id,
        StockTransferCommandRequest request, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, request.IdempotencyKey, request.ExpectedVersion, SubmitAction,
            StockTransferStatus.Draft, StockTransferStatus.Requested, null,
            (entity, now) => entity.SubmittedAt = now, cancellationToken);

    // ================================================================ setujui

    /// <summary>
    /// Menyetujui transfer dan menahan stok di lokasi asal.
    /// </summary>
    /// <remarks>
    /// Batch dipilih di sini menurut FEFO lalu disimpan sebagai alokasi. Pemilihan tidak
    /// diulang saat pengeluaran, karena stok yang ditahan haruslah stok yang sama dengan yang
    /// kemudian keluar — kalau tidak, yang ditahan dan yang dikirim bisa berbeda batch.
    /// </remarks>
    public async Task<StockTransferDetailResponse> ApproveAsync(Guid id,
        StockTransferCommandRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var fingerprint = Hash($"approve:{id}");

        var prior = await FindIdempotentAsync(ApproveAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Transfer stok tidak ditemukan.");

        if (entity.Status != StockTransferStatus.Requested)
            throw new StockTransferConflictException("PHM041",
                "Hanya transfer yang sudah diajukan yang dapat disetujui.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in entity.Items.Where(x => !x.IsDelete).OrderBy(x => x.LineNumber))
        {
            List<(Guid DrugBatchId, decimal Quantity)> plan;
            try
            {
                plan = await _drugStockService.PlanFefoAsync(item.DrugId,
                    entity.SourceStorageLocationId, item.RequestedQuantity, cancellationToken);
            }
            catch (DrugStockUnprocessableException ex)
            {
                throw new StockTransferUnprocessableException(ex.Code,
                    $"{item.DrugNameSnapshot}: {ex.Message}");
            }

            var sequence = 1;
            foreach (var (batchId, quantity) in plan)
            {
                await _drugStockService.ReserveBatchAsync(batchId, entity.SourceStorageLocationId,
                    quantity, cancellationToken);

                _dbContext.PhmStockTransferAllocations.Add(new PhmStockTransferAllocation
                {
                    StockTransferItemId = item.Id,
                    DrugBatchId = batchId,
                    SequenceNumber = sequence++,
                    Quantity = quantity,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                });
            }
        }

        entity.Status = StockTransferStatus.Approved;
        entity.ApprovedAt = now;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockTransferHistories.Add(NewHistory(entity.Id,
            StockTransferStatus.Approved, StockTransferStatus.Requested, ApproveAction, null,
            request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockTransfer.Approve",
            "Menyetujui transfer stok dan menahan stok di lokasi asal.",
            new { entity.Id, entity.TransferNumber });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ================================================================== tolak

    public Task<StockTransferDetailResponse> RejectAsync(Guid id,
        StockTransferReasonRequest request, CancellationToken cancellationToken = default) =>
        TransitionAsync(id, request.IdempotencyKey, request.ExpectedVersion, RejectAction,
            StockTransferStatus.Requested, StockTransferStatus.Rejected, request.Reason,
            (entity, _) => entity.DecisionReason = request.Reason.Trim(), cancellationToken);

    // ================================================================ keluarkan

    /// <summary>Mengeluarkan barang dari lokasi asal; sesudah ini barang sedang di jalan.</summary>
    public async Task<StockTransferDetailResponse> IssueAsync(Guid id,
        StockTransferCommandRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);
        var fingerprint = Hash($"issue:{id}");

        var prior = await FindIdempotentAsync(IssueAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Transfer stok tidak ditemukan.");

        if (entity.Status != StockTransferStatus.Approved)
            throw new StockTransferConflictException("PHM041",
                "Hanya transfer yang sudah disetujui yang dapat dikeluarkan.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in entity.Items.Where(x => !x.IsDelete).OrderBy(x => x.LineNumber))
        {
            decimal issued = 0;

            foreach (var allocation in item.Allocations.Where(x => !x.IsDelete && !x.IsReleased))
            {
                try
                {
                    await _drugStockService.IssueBatchAsync(allocation.DrugBatchId,
                        entity.SourceStorageLocationId, allocation.Quantity,
                        consumeReservation: true, DrugStockSourceDocumentTypes.StockTransfer,
                        entity.Id,
                        $"Transfer {entity.TransferNumber} keluar dari lokasi asal.",
                        $"trf-out-{allocation.Id:N}", cancellationToken);
                }
                catch (DrugStockUnprocessableException ex)
                {
                    throw new StockTransferUnprocessableException(ex.Code,
                        $"{item.DrugNameSnapshot}: {ex.Message}");
                }

                allocation.IsReleased = true;
                allocation.UpdateDateTime = now;
                allocation.UpdateBy = actorUserId;
                issued += allocation.Quantity;
            }

            item.IssuedQuantity = issued;
            item.UpdateDateTime = now;
            item.UpdateBy = actorUserId;
        }

        entity.Status = StockTransferStatus.InTransit;
        entity.IssuedAt = now;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockTransferHistories.Add(NewHistory(entity.Id,
            StockTransferStatus.InTransit, StockTransferStatus.Approved, IssueAction, null,
            request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockTransfer.Issue",
            "Mengeluarkan barang transfer dari lokasi asal.",
            new { entity.Id, entity.TransferNumber });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ==================================================================== terima

    /// <summary>
    /// Mencatat penerimaan di lokasi tujuan, baris per baris.
    /// </summary>
    /// <remarks>
    /// Jumlah diterima boleh lebih kecil daripada yang dikirim. Selisihnya adalah barang yang
    /// hilang atau rusak di perjalanan, dan sengaja dibiarkan terbaca alih-alih disamakan
    /// diam-diam — selisih yang disembunyikan tidak akan pernah bisa ditelusuri.
    /// </remarks>
    public async Task<StockTransferDetailResponse> ReceiveAsync(Guid id,
        ReceiveStockTransferRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var note = Normalize(request.Note);
        var fingerprint = Hash($"receive:{id}|{note}|" + string.Join(',',
            request.Items.OrderBy(x => x.StockTransferItemId)
                .Select(x => $"{x.StockTransferItemId:N}:{x.ReceivedQuantity}")));

        var prior = await FindIdempotentAsync(ReceiveAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Transfer stok tidak ditemukan.");

        if (entity.Status != StockTransferStatus.InTransit)
            throw new StockTransferConflictException("PHM041",
                "Hanya transfer yang sedang di jalan yang dapat diterima.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var activeItems = entity.Items.Where(x => !x.IsDelete).ToList();
        EnsureReceiptCoversEveryLine(activeItems, request.Items);

        var byId = request.Items.ToDictionary(x => x.StockTransferItemId, x => x.ReceivedQuantity);
        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        foreach (var item in activeItems.OrderBy(x => x.LineNumber))
        {
            var received = byId[item.Id];
            var issued = item.IssuedQuantity ?? 0;

            if (received < 0)
                throw new StockTransferUnprocessableException("PHM042",
                    "Jumlah diterima tidak boleh kurang dari nol.");

            // Menerima lebih banyak daripada yang dikirim berarti barang muncul entah dari
            // mana; itu bukan penerimaan transfer melainkan selisih yang harus ditelusuri.
            if (received > issued)
                throw new StockTransferUnprocessableException("PHM042",
                    $"Baris {item.LineNumber}: jumlah diterima melebihi jumlah yang dikirim.");

            // Batch dibagikan menurut urutan alokasi. Bila yang diterima lebih sedikit, batch
            // terakhir yang paling jauh kedaluwarsanya menanggung kekurangannya lebih dahulu.
            var remaining = received;

            foreach (var allocation in item.Allocations.Where(x => !x.IsDelete)
                .OrderBy(x => x.SequenceNumber))
            {
                if (remaining <= 0) break;

                var take = Math.Min(allocation.Quantity, remaining);
                remaining -= take;

                await _drugStockService.ReceiveBatchAsync(allocation.DrugBatchId,
                    entity.DestinationStorageLocationId, take,
                    DrugStockSourceDocumentTypes.StockTransfer, entity.Id,
                    $"Transfer {entity.TransferNumber} diterima di lokasi tujuan.",
                    $"trf-in-{allocation.Id:N}", cancellationToken);
            }

            item.ReceivedQuantity = received;
            item.UpdateDateTime = now;
            item.UpdateBy = actorUserId;
        }

        entity.Status = StockTransferStatus.Completed;
        entity.ReceivedAt = now;
        entity.DecisionReason = note;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockTransferHistories.Add(NewHistory(entity.Id,
            StockTransferStatus.Completed, StockTransferStatus.InTransit, ReceiveAction, note,
            request.IdempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "StockTransfer.Receive",
            "Mencatat penerimaan transfer di lokasi tujuan.",
            new { entity.Id, entity.TransferNumber });

        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ================================================================== batal

    /// <summary>
    /// Membatalkan transfer sebelum barangnya keluar; reservasi yang sudah dipasang dilepas.
    /// </summary>
    public async Task<StockTransferDetailResponse> CancelAsync(Guid id,
        StockTransferReasonRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new StockTransferUnprocessableException("PHM043",
                "Alasan pembatalan wajib diisi.");

        var reason = request.Reason.Trim();
        var fingerprint = Hash($"cancel:{id}|{reason}");

        var prior = await FindIdempotentAsync(CancelAction, request.IdempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Transfer stok tidak ditemukan.");

        if (entity.Status is not (StockTransferStatus.Draft or StockTransferStatus.Requested
            or StockTransferStatus.Approved))
            throw new StockTransferConflictException("PHM041",
                "Transfer yang barangnya sudah keluar tidak dapat dibatalkan.");

        EnsureVersion(entity.Version, request.ExpectedVersion);

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;
        var from = entity.Status;

        // Stok yang sudah ditahan harus dilepas; kalau tidak, ia tersandera selamanya oleh
        // transfer yang tidak akan pernah terjadi.
        foreach (var item in entity.Items.Where(x => !x.IsDelete))
        {
            foreach (var allocation in item.Allocations.Where(x => !x.IsDelete && !x.IsReleased))
            {
                await _drugStockService.ReleaseReservationAsync(allocation.DrugBatchId,
                    entity.SourceStorageLocationId, allocation.Quantity, cancellationToken);

                allocation.IsReleased = true;
                allocation.UpdateDateTime = now;
                allocation.UpdateBy = actorUserId;
            }
        }

        entity.Status = StockTransferStatus.Cancelled;
        entity.DecisionReason = reason;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockTransferHistories.Add(NewHistory(entity.Id,
            StockTransferStatus.Cancelled, from, CancelAction, reason, request.IdempotencyKey,
            fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    // ================================================================ penolong

    /// <summary>Perpindahan status sederhana yang tidak menyentuh stok.</summary>
    private async Task<StockTransferDetailResponse> TransitionAsync(Guid id,
        string idempotencyKey, int expectedVersion, string action,
        StockTransferStatus fromStatus, StockTransferStatus toStatus, string? reason,
        Action<PhmStockTransfer, DateTime> apply, CancellationToken cancellationToken)
    {
        EnsureIdempotencyKey(idempotencyKey);

        if (toStatus == StockTransferStatus.Rejected && string.IsNullOrWhiteSpace(reason))
            throw new StockTransferUnprocessableException("PHM043",
                "Alasan penolakan wajib diisi.");

        var fingerprint = Hash($"{action}:{id}|{reason?.Trim()}");

        var prior = await FindIdempotentAsync(action, idempotencyKey, cancellationToken);
        if (prior != null)
        {
            EnsureSameFingerprint(prior.Source, fingerprint);
            return (await GetDetailAsync(id, cancellationToken))!;
        }

        var entity = await LoadAsync(id, tracking: true, cancellationToken)
            ?? throw new KeyNotFoundException("Transfer stok tidak ditemukan.");

        if (entity.Status != fromStatus)
            throw new StockTransferConflictException("PHM041",
                $"Perintah ini hanya sah pada transfer berstatus {fromStatus}.");

        EnsureVersion(entity.Version, expectedVersion);

        if (fromStatus == StockTransferStatus.Draft && !entity.Items.Any(x => !x.IsDelete))
            throw new StockTransferUnprocessableException("PHM044",
                "Transfer tanpa satu pun item tidak dapat diajukan.");

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        apply(entity, now);
        entity.Status = toStatus;
        entity.Version++;
        entity.UpdateDateTime = now;
        entity.UpdateBy = actorUserId;

        _dbContext.PhmStockTransferHistories.Add(NewHistory(entity.Id, toStatus, fromStatus,
            action, reason?.Trim(), idempotencyKey, fingerprint, actorUserId, now));

        await SaveAsync(cancellationToken);
        return (await GetDetailAsync(id, cancellationToken))!;
    }

    private Task<PhmStockTransfer?> LoadAsync(Guid id, bool tracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.PhmStockTransfers
            .Include(x => x.SourceStorageLocation)
            .Include(x => x.DestinationStorageLocation)
            .Include(x => x.Items).ThenInclude(x => x.Allocations).ThenInclude(x => x.DrugBatch)
            .Include(x => x.Histories.Where(h => !h.IsDelete))
            .Where(x => x.Id == id && !x.IsDelete);

        if (!tracking) query = query.AsNoTracking();
        return query.FirstOrDefaultAsync(cancellationToken);
    }

    private void AddItems(Guid transferId, List<StockTransferItemInput> inputs,
        Dictionary<Guid, (string Code, string Name)> drugs, Guid actorUserId, DateTime now)
    {
        var line = 1;
        foreach (var input in inputs)
        {
            var drug = drugs[input.DrugId];
            _dbContext.PhmStockTransferItems.Add(new PhmStockTransferItem
            {
                StockTransferId = transferId,
                DrugId = input.DrugId,
                DrugCodeSnapshot = drug.Code,
                DrugNameSnapshot = drug.Name,
                RequestedQuantity = input.RequestedQuantity,
                Note = Normalize(input.Note),
                LineNumber = line++,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
        }
    }

    private async Task<Dictionary<Guid, (string Code, string Name)>> ResolveDrugsAsync(
        IEnumerable<Guid> drugIds, CancellationToken cancellationToken)
    {
        var ids = drugIds.Distinct().ToList();
        var drugs = await _dbContext.MstDrugs.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.IsActive && !x.IsDelete)
            .Select(x => new { x.Id, x.DrugCode, x.DrugName })
            .ToListAsync(cancellationToken);

        if (drugs.Count != ids.Count)
            throw new StockTransferUnprocessableException("PHM045",
                "Ada obat yang tidak ditemukan atau tidak aktif.");

        return drugs.ToDictionary(x => x.Id, x => (x.DrugCode, x.DrugName));
    }

    private async Task EnsureWorkforceAsync(Guid workforceId, CancellationToken cancellationToken)
    {
        var valid = await _dbContext.MstWorkforceProfiles.AsNoTracking()
            .AnyAsync(x => x.Id == workforceId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new StockTransferUnprocessableException("PHM045",
                "Petugas peminta tidak ditemukan atau tidak aktif.");
    }

    private static void EnsureItemsValid(List<StockTransferItemInput> items)
    {
        if (items.Count == 0)
            throw new StockTransferUnprocessableException("PHM044",
                "Transfer harus memuat sekurang-kurangnya satu item.");

        if (items.Any(x => x.RequestedQuantity <= 0))
            throw new StockTransferUnprocessableException("PHM046",
                "Jumlah setiap item harus lebih dari nol.");

        if (items.GroupBy(x => x.DrugId).Any(g => g.Count() > 1))
            throw new StockTransferUnprocessableException("PHM047",
                "Satu obat hanya boleh muncul satu kali. Gabungkan jumlahnya menjadi satu baris.");
    }

    private static void EnsureReceiptCoversEveryLine(List<PhmStockTransferItem> activeItems,
        List<ReceiveStockTransferItemInput> inputs)
    {
        if (inputs.GroupBy(x => x.StockTransferItemId).Any(g => g.Count() > 1))
            throw new StockTransferUnprocessableException("PHM048",
                "Satu baris hanya boleh disebut satu kali pada penerimaan.");

        var activeIds = activeItems.Select(x => x.Id).ToHashSet();
        var inputIds = inputs.Select(x => x.StockTransferItemId).ToHashSet();

        if (inputIds.Except(activeIds).Any())
            throw new StockTransferUnprocessableException("PHM048",
                "Ada baris penerimaan yang bukan bagian dari transfer ini.");

        if (activeIds.Except(inputIds).Any())
            throw new StockTransferUnprocessableException("PHM048",
                "Setiap baris harus disebut jumlah penerimaannya, walaupun nol.");
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new StockTransferConflictException("PHM049",
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
            throw new StockTransferConflictException("PHM050",
                "Idempotency key dipakai dengan isi perintah yang berbeda.");
    }

    private Task<PhmStockTransferHistory?> FindIdempotentAsync(string action, string key,
        CancellationToken cancellationToken) =>
        _dbContext.PhmStockTransferHistories.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Action == action && x.CorrelationId == key.Trim() &&
                                      !x.IsDelete, cancellationToken);

    private static PhmStockTransferHistory NewHistory(Guid transferId, StockTransferStatus to,
        StockTransferStatus? from, string action, string? reason, string idempotencyKey,
        string fingerprint, Guid actorUserId, DateTime now) => new()
        {
            StockTransferId = transferId,
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
            throw new StockTransferConflictException("PHM049",
                "Data telah diperbarui pengguna lain. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new StockTransferForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static string BuildFingerprint(Guid destinationId, string? notes,
        List<StockTransferItemInput> items) =>
        Hash(string.Join('|', destinationId, Normalize(notes),
            string.Join(',', items.OrderBy(x => x.DrugId)
                .Select(x => $"{x.DrugId:N}:{x.RequestedQuantity}"))));

    private static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
    private static Guid DeterministicId(string key) =>
        new(SHA256.HashData(Encoding.UTF8.GetBytes($"StockTransfer:{key.Trim()}"))[..16]);

    private static StockTransferDetailResponse MapDetail(PhmStockTransfer x) => new()
    {
        Id = x.Id,
        TransferNumber = x.TransferNumber,
        SourceStorageLocationId = x.SourceStorageLocationId,
        SourceStorageLocationName = x.SourceStorageLocation?.StorageLocationName ?? string.Empty,
        DestinationStorageLocationId = x.DestinationStorageLocationId,
        DestinationStorageLocationName =
            x.DestinationStorageLocation?.StorageLocationName ?? string.Empty,
        RequestedByWorkforceId = x.RequestedByWorkforceId,
        Status = x.Status,
        Notes = x.Notes,
        DecisionReason = x.DecisionReason,
        RequestedAt = x.RequestedAt,
        SubmittedAt = x.SubmittedAt,
        ApprovedAt = x.ApprovedAt,
        IssuedAt = x.IssuedAt,
        ReceivedAt = x.ReceivedAt,
        ItemCount = x.ItemCount,
        Version = x.Version,
        IsEditable = x.Status == StockTransferStatus.Draft,
        CanSubmit = x.Status == StockTransferStatus.Draft,
        CanApprove = x.Status == StockTransferStatus.Requested,
        CanReject = x.Status == StockTransferStatus.Requested,
        CanIssue = x.Status == StockTransferStatus.Approved,
        CanReceive = x.Status == StockTransferStatus.InTransit,
        CanCancel = x.Status is StockTransferStatus.Draft or StockTransferStatus.Requested
            or StockTransferStatus.Approved,
        Items = [.. x.Items.Where(i => !i.IsDelete).OrderBy(i => i.LineNumber)
            .Select(i => new StockTransferItemResponse
            {
                Id = i.Id,
                DrugId = i.DrugId,
                DrugCode = i.DrugCodeSnapshot,
                DrugName = i.DrugNameSnapshot,
                RequestedQuantity = i.RequestedQuantity,
                IssuedQuantity = i.IssuedQuantity,
                ReceivedQuantity = i.ReceivedQuantity,
                Note = i.Note,
                LineNumber = i.LineNumber,
                Allocations = [.. i.Allocations.Where(a => !a.IsDelete)
                    .OrderBy(a => a.SequenceNumber)
                    .Select(a => new StockTransferAllocationResponse
                    {
                        DrugBatchId = a.DrugBatchId,
                        BatchNumber = a.DrugBatch != null ? a.DrugBatch.BatchNumber : string.Empty,
                        ExpiryDate = a.DrugBatch != null ? a.DrugBatch.ExpiryDate : null,
                        Quantity = a.Quantity,
                        IsReleased = a.IsReleased
                    })]
            })],
        Histories = [.. x.Histories.Where(h => !h.IsDelete).OrderByDescending(h => h.OccurredAt)
            .Select(h => new StockTransferHistoryResponse
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

/// <summary>Benturan atau transisi tidak sah; dipetakan ke `409`.</summary>
public sealed class StockTransferConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Pengguna tidak berwenang; dipetakan ke `403`.</summary>
public sealed class StockTransferForbiddenException(string message) : Exception(message);

/// <summary>Prasyarat aturan belum terpenuhi; dipetakan ke `422`.</summary>
public sealed class StockTransferUnprocessableException(string code, string message)
    : Exception(message)
{
    public string Code { get; } = code;
}
