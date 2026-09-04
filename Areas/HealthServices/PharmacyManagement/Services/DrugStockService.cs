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
/// Sumber kebenaran seluruh pergerakan stok obat.
/// </summary>
/// <remarks>
/// <para>
/// Empat aturan berlaku pada setiap perintah di sini, tanpa pengecualian per obat maupun per
/// lokasi. Stok tidak boleh negatif. Setiap perubahan saldo menghasilkan satu baris kartu stok.
/// Pengeluaran mengambil batch yang paling dekat kedaluwarsa lebih dahulu. Baris kartu stok
/// tidak pernah diubah — kekeliruan diperbaiki dengan menambah baris koreksi.
/// </para>
/// <para>
/// Reservasi menahan stok tanpa memindahkannya: barangnya tetap terhitung fisik, hanya yang
/// tersedia yang berkurang. Reservasi ganda dicegah karena setiap penahanan memakai token
/// konkurensi baris saldo, sehingga dua proses yang mengincar sisa stok yang sama tidak dapat
/// keduanya berhasil.
/// </para>
/// </remarks>
public sealed class DrugStockService
{
    private const string LogCategory = "PharmacyManagement";

    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LoggerService _loggerService;

    public DrugStockService(ApplicationDbContext dbContext,
        IHttpContextAccessor httpContextAccessor, LoggerService loggerService)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _loggerService = loggerService;
    }

    // ============================================================ 1. stok per lokasi

    public async Task<PagedResult<DrugStockBalanceResponse>> GetBalancesAsync(
        DrugStockBalanceQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrxDrugStockBalances.AsNoTracking().Where(x => !x.IsDelete);

        if (request.DrugId.HasValue) query = query.Where(x => x.DrugId == request.DrugId);
        if (request.StorageLocationId.HasValue)
            query = query.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (request.DrugBatchId.HasValue)
            query = query.Where(x => x.DrugBatchId == request.DrugBatchId);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.OnlyInStock) query = query.Where(x => x.QuantityOnHand > 0);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (request.OnlyExpired == true)
            query = query.Where(x => x.DrugBatch!.ExpiryDate != null && x.DrugBatch.ExpiryDate < today);

        if (request.ExpiringWithinDays.HasValue)
        {
            var limit = today.AddDays(request.ExpiringWithinDays.Value);
            query = query.Where(x => x.DrugBatch!.ExpiryDate != null &&
                                     x.DrugBatch.ExpiryDate >= today &&
                                     x.DrugBatch.ExpiryDate <= limit);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Drug!.DrugName.ToLower().Contains(keyword) ||
                x.Drug.DrugCode.ToLower().Contains(keyword) ||
                x.DrugBatch!.BatchNumber.ToLower().Contains(keyword));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 25 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);

        // Yang paling dekat kedaluwarsa ditampilkan lebih dahulu: itulah yang harus dipakai
        // lebih dahulu, dan itu pula yang paling mendesak diperhatikan.
        var items = await query
            .OrderBy(x => x.DrugBatch!.ExpiryDate == null)
            .ThenBy(x => x.DrugBatch!.ExpiryDate)
            .ThenBy(x => x.Drug!.DrugName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DrugStockBalanceResponse
            {
                Id = x.Id,
                DrugId = x.DrugId,
                DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                DrugBatchId = x.DrugBatchId,
                BatchNumber = x.DrugBatch != null ? x.DrugBatch.BatchNumber : string.Empty,
                ExpiryDate = x.DrugBatch != null ? x.DrugBatch.ExpiryDate : null,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation != null
                    ? x.StorageLocation.StorageLocationName : string.Empty,
                Status = x.Status,
                QuantityOnHand = x.QuantityOnHand,
                QuantityReserved = x.QuantityReserved,
                QuantityAvailable = x.QuantityOnHand - x.QuantityReserved,
                Version = x.Version
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
            item.DaysToExpiry = DaysToExpiry(item.ExpiryDate, today);

        return new PagedResult<DrugStockBalanceResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ==================================================== 1b. ringkasan per item

    /// <summary>
    /// Menjumlahkan seluruh batch satu obat pada satu lokasi menjadi satu baris.
    /// </summary>
    /// <remarks>
    /// Dipakai layar yang menjawab "berapa obat ini di lokasi ini", sementara rinciannya per
    /// batch dibaca lewat daftar saldo. Kedaluwarsa terdekat ikut dibawa karena itulah batch
    /// yang akan keluar lebih dahulu, sehingga angka yang paling mendesak terlihat tanpa
    /// perlu membuka rinciannya.
    /// </remarks>
    public async Task<PagedResult<DrugStockSummaryResponse>> GetSummaryAsync(
        DrugStockSummaryQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrxDrugStockBalances.AsNoTracking().Where(x => !x.IsDelete);

        if (request.DrugId.HasValue) query = query.Where(x => x.DrugId == request.DrugId);
        if (request.StorageLocationId.HasValue)
            query = query.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (request.Status.HasValue) query = query.Where(x => x.Status == request.Status);
        if (request.OnlyInStock) query = query.Where(x => x.QuantityOnHand > 0);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x =>
                x.Drug!.DrugName.ToLower().Contains(keyword) ||
                x.Drug.DrugCode.ToLower().Contains(keyword));
        }

        var grouped = query
            .GroupBy(x => new { x.DrugId, x.StorageLocationId, x.Status })
            .Select(g => new
            {
                g.Key.DrugId,
                g.Key.StorageLocationId,
                g.Key.Status,
                DrugCode = g.Max(x => x.Drug!.DrugCode),
                DrugName = g.Max(x => x.Drug!.DrugName),
                StorageLocationName = g.Max(x => x.StorageLocation!.StorageLocationName),
                OnHand = g.Sum(x => x.QuantityOnHand),
                Reserved = g.Sum(x => x.QuantityReserved),
                BatchCount = g.Count(),
                NearestExpiry = g.Min(x => x.DrugBatch!.ExpiryDate)
            });

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 25 : request.PageSize;

        var total = await grouped.CountAsync(cancellationToken);

        var page = await grouped
            .OrderBy(x => x.DrugName)
            .ThenBy(x => x.StorageLocationName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return new PagedResult<DrugStockSummaryResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = [.. page.Select(x => new DrugStockSummaryResponse
            {
                DrugId = x.DrugId,
                DrugCode = x.DrugCode ?? string.Empty,
                DrugName = x.DrugName ?? string.Empty,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocationName ?? string.Empty,
                Status = x.Status,
                QuantityOnHand = x.OnHand,
                QuantityReserved = x.Reserved,
                QuantityAvailable = x.OnHand - x.Reserved,
                BatchCount = x.BatchCount,
                NearestExpiryDate = x.NearestExpiry,
                DaysToNearestExpiry = DaysToExpiry(x.NearestExpiry, today)
            })]
        };
    }

    // ======================================================== 1c. daftar batch

    public async Task<PagedResult<DrugBatchResponse>> GetBatchesAsync(DrugBatchQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.MstDrugBatches.AsNoTracking().Where(x => !x.IsDelete);

        if (request.DrugId.HasValue) query = query.Where(x => x.DrugId == request.DrugId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        if (request.OnlyExpired == true)
            query = query.Where(x => x.ExpiryDate != null && x.ExpiryDate < today);

        if (request.ExpiringWithinDays.HasValue)
        {
            var limit = today.AddDays(request.ExpiringWithinDays.Value);
            query = query.Where(x => x.ExpiryDate != null &&
                                     x.ExpiryDate >= today && x.ExpiryDate <= limit);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            query = query.Where(x => x.BatchNumber.ToLower().Contains(keyword) ||
                                     x.Drug!.DrugName.ToLower().Contains(keyword) ||
                                     x.Drug.DrugCode.ToLower().Contains(keyword));
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 25 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.ExpiryDate == null)
            .ThenBy(x => x.ExpiryDate)
            .ThenBy(x => x.BatchNumber)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DrugBatchResponse
            {
                Id = x.Id,
                DrugId = x.DrugId,
                DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                BatchNumber = x.BatchNumber,
                ExpiryDate = x.ExpiryDate,
                SupplierId = x.SupplierId,
                SupplierName = x.Supplier != null ? x.Supplier.SupplierName : null,
                PrincipalName = x.PrincipalName
            })
            .ToListAsync(cancellationToken);

        foreach (var item in items)
            item.DaysToExpiry = DaysToExpiry(item.ExpiryDate, today);

        return new PagedResult<DrugBatchResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ================================================================ 2. kartu stok

    public async Task<PagedResult<DrugStockMutationResponse>> GetMutationsAsync(
        DrugStockMutationQuery request, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TrxDrugStockMutations.AsNoTracking().Where(x => !x.IsDelete);

        if (request.DrugId.HasValue) query = query.Where(x => x.DrugId == request.DrugId);
        if (request.DrugBatchId.HasValue) query = query.Where(x => x.DrugBatchId == request.DrugBatchId);
        if (request.StorageLocationId.HasValue)
            query = query.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (request.MutationType.HasValue)
            query = query.Where(x => x.MutationType == request.MutationType);
        if (!string.IsNullOrWhiteSpace(request.SourceDocumentType))
            query = query.Where(x => x.SourceDocumentType == request.SourceDocumentType);
        if (request.SourceDocumentId.HasValue)
            query = query.Where(x => x.SourceDocumentId == request.SourceDocumentId);

        if (request.StartDate.HasValue)
        {
            var start = request.StartDate.Value.Date.ToUniversalTime();
            query = query.Where(x => x.OccurredAt >= start);
        }

        if (request.EndDate.HasValue)
        {
            var endExclusive = request.EndDate.Value.Date.ToUniversalTime().AddDays(1);
            query = query.Where(x => x.OccurredAt < endExclusive);
        }

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize is < 1 or > 200 ? 25 : request.PageSize;

        var total = await query.CountAsync(cancellationToken);

        // Nama pelaku diambil lewat pencocokan terpisah, bukan relasi navigasi, karena baris
        // kartu stok hanya menyimpan identitas penggunanya dan tidak boleh bergantung pada
        // baris pengguna yang kelak bisa saja dinonaktifkan.
        var items = await query
            .OrderByDescending(x => x.OccurredAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new DrugStockMutationResponse
            {
                ActorUserId = x.ActorUserId,
                Id = x.Id,
                DrugId = x.DrugId,
                DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                DrugBatchId = x.DrugBatchId,
                BatchNumber = x.DrugBatch != null ? x.DrugBatch.BatchNumber : string.Empty,
                StorageLocationId = x.StorageLocationId,
                StorageLocationName = x.StorageLocation != null
                    ? x.StorageLocation.StorageLocationName : string.Empty,
                Status = x.Status,
                MutationType = x.MutationType,
                QuantityChange = x.QuantityChange,
                BalanceBefore = x.BalanceBefore,
                BalanceAfter = x.BalanceAfter,
                Reason = x.Reason,
                SourceDocumentType = x.SourceDocumentType,
                SourceDocumentId = x.SourceDocumentId,
                CorrectionOfMutationId = x.CorrectionOfMutationId,
                OccurredAt = x.OccurredAt
            })
            .ToListAsync(cancellationToken);

        var actorIds = items.Select(x => x.ActorUserId).Distinct().ToList();
        var actors = await _dbContext.Users.AsNoTracking()
            .Where(x => actorIds.Contains(x.Id))
            .Select(x => new { x.Id, x.DisplayName })
            .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);

        foreach (var item in items)
            item.ActorName = actors.GetValueOrDefault(item.ActorUserId);

        return new PagedResult<DrugStockMutationResponse>
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalData = total,
            TotalPage = (int)Math.Ceiling(total / (double)pageSize),
            Items = items
        };
    }

    // ============================================================ 3. saldo pembuka

    public async Task<DrugStockBalanceResponse> RecordOpeningBalanceAsync(
        RecordOpeningBalanceRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var existing = await FindMutationByKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing != null)
            return await LoadBalanceResponseAsync(existing.DrugBatchId, existing.StorageLocationId,
                existing.Status, cancellationToken);

        var batch = await EnsureBatchAsync(request.Batch, cancellationToken);
        await EnsureStorageLocationAsync(request.StorageLocationId, cancellationToken);

        var balance = await LoadOrCreateBalanceAsync(batch.DrugId, batch.Id,
            request.StorageLocationId, DrugStockStatus.Available, cancellationToken);

        await ApplyMutationAsync(balance, DrugStockMutationType.OpeningBalance, request.Quantity,
            Normalize(request.Reason) ?? "Saldo pembuka.", DrugStockSourceDocumentTypes.ManualAdjustment,
            null, null, request.IdempotencyKey, cancellationToken);

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "DrugStock.OpeningBalance",
            "Mencatat saldo pembuka stok obat.",
            new { batch.DrugId, BatchId = batch.Id, request.StorageLocationId, request.Quantity });

        return await LoadBalanceResponseAsync(batch.Id, request.StorageLocationId,
            DrugStockStatus.Available, cancellationToken);
    }

    // ============================================================== 4. penyesuaian

    public async Task<DrugStockBalanceResponse> AdjustAsync(AdjustStockRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new DrugStockUnprocessableException("PHM021",
                "Alasan penyesuaian wajib diisi.");

        if (request.QuantityChange == 0)
            throw new DrugStockUnprocessableException("PHM022",
                "Penyesuaian sebesar nol tidak mengubah apa pun.");

        var existing = await FindMutationByKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing != null)
            return await LoadBalanceResponseAsync(existing.DrugBatchId, existing.StorageLocationId,
                existing.Status, cancellationToken);

        var balance = await LoadBalanceAsync(request.DrugBatchId, request.StorageLocationId,
            request.Status, cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM020",
                "Saldo untuk batch dan lokasi tersebut belum ada.");

        EnsureVersion(balance.Version, request.ExpectedVersion);

        await ApplyMutationAsync(balance, DrugStockMutationType.Adjustment, request.QuantityChange,
            request.Reason.Trim(), DrugStockSourceDocumentTypes.ManualAdjustment, null, null,
            request.IdempotencyKey, cancellationToken);

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "DrugStock.Adjust",
            "Menyesuaikan saldo stok obat.",
            new { request.DrugBatchId, request.StorageLocationId, request.QuantityChange, request.Reason });

        return await LoadBalanceResponseAsync(request.DrugBatchId, request.StorageLocationId,
            request.Status, cancellationToken);
    }

    // ========================================================= 5. perpindahan status

    /// <summary>
    /// Memindahkan sebagian stok ke status lain, misalnya menahannya di karantina.
    /// </summary>
    /// <remarks>
    /// Barangnya tidak berpindah lokasi; yang berpindah adalah keadaannya. Karena itu perintah
    /// ini menghasilkan dua baris kartu stok yang saling berpasangan: keluar dari status asal
    /// dan masuk ke status tujuan, keduanya membawa alasan yang sama.
    /// </remarks>
    public async Task<List<DrugStockBalanceResponse>> ChangeStatusAsync(
        ChangeStockStatusRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (request.FromStatus == request.ToStatus)
            throw new DrugStockUnprocessableException("PHM023",
                "Status asal dan tujuan tidak boleh sama.");

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new DrugStockUnprocessableException("PHM021",
                "Alasan perpindahan status wajib diisi.");

        var existing = await FindMutationByKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing == null)
        {
            var from = await LoadBalanceAsync(request.DrugBatchId, request.StorageLocationId,
                request.FromStatus, cancellationToken)
                ?? throw new DrugStockUnprocessableException("PHM020",
                    "Saldo pada status asal belum ada.");

            EnsureVersion(from.Version, request.ExpectedVersion);

            // Yang sudah ditahan proses lain tidak boleh ikut dipindahkan; menahan dan
            // memindahkan barang yang sama membuat dua proses menghitungnya sebagai miliknya.
            if (from.QuantityOnHand - from.QuantityReserved < request.Quantity)
                throw new DrugStockUnprocessableException("PHM024",
                    "Jumlah yang dipindahkan melebihi stok tersedia pada status asal.");

            var to = await LoadOrCreateBalanceAsync(from.DrugId, request.DrugBatchId,
                request.StorageLocationId, request.ToStatus, cancellationToken);

            var reason = request.Reason.Trim();

            await ApplyMutationAsync(from, DrugStockMutationType.StatusChange, -request.Quantity,
                reason, DrugStockSourceDocumentTypes.ManualAdjustment, null, null,
                $"{request.IdempotencyKey}:from", cancellationToken);

            await ApplyMutationAsync(to, DrugStockMutationType.StatusChange, request.Quantity,
                reason, DrugStockSourceDocumentTypes.ManualAdjustment, null, null,
                request.IdempotencyKey, cancellationToken);

            await SaveAsync(cancellationToken);
            await _loggerService.AuditAsync(LogCategory, "DrugStock.ChangeStatus",
                "Memindahkan status stok obat.",
                new { request.DrugBatchId, request.StorageLocationId, request.FromStatus,
                      request.ToStatus, request.Quantity, request.Reason });
        }

        return
        [
            await LoadBalanceResponseAsync(request.DrugBatchId, request.StorageLocationId,
                request.FromStatus, cancellationToken),
            await LoadBalanceResponseAsync(request.DrugBatchId, request.StorageLocationId,
                request.ToStatus, cancellationToken)
        ];
    }

    // ============================================================== 6. reservasi

    /// <summary>Menahan stok menurut FEFO tanpa mengeluarkannya.</summary>
    public async Task<StockOperationResponse> ReserveAsync(ReserveStockRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var candidates = await LoadFefoCandidatesAsync(request.DrugId, request.StorageLocationId,
            cancellationToken);

        var plan = PlanFefo(candidates, request.Quantity);
        var allocations = new List<StockAllocationResponse>();

        foreach (var (balance, quantity) in plan)
        {
            // Reservasi hanya mengubah bagian yang ditahan; saldo fisik tidak bergerak,
            // sehingga tidak ada baris kartu stok untuk langkah ini. Kartu stok mencatat
            // perpindahan barang, bukan niat memakainya.
            balance.QuantityReserved += quantity;
            balance.Version++;
            Touch(balance);

            allocations.Add(new StockAllocationResponse
            {
                DrugBatchId = balance.DrugBatchId,
                BatchNumber = balance.DrugBatch?.BatchNumber ?? string.Empty,
                ExpiryDate = balance.DrugBatch?.ExpiryDate,
                Quantity = quantity
            });
        }

        await SaveAsync(cancellationToken);

        return new StockOperationResponse
        {
            DrugId = request.DrugId,
            StorageLocationId = request.StorageLocationId,
            TotalQuantity = request.Quantity,
            Allocations = allocations
        };
    }

    /// <summary>Melepas reservasi ketika prosesnya batal atau gagal.</summary>
    public async Task ReleaseReservationAsync(Guid drugBatchId, Guid storageLocationId,
        decimal quantity, CancellationToken cancellationToken = default)
    {
        var balance = await LoadBalanceAsync(drugBatchId, storageLocationId,
            DrugStockStatus.Available, cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM020", "Saldo tidak ditemukan.");

        if (balance.QuantityReserved < quantity)
            throw new DrugStockUnprocessableException("PHM025",
                "Jumlah yang dilepas melebihi yang sedang ditahan.");

        balance.QuantityReserved -= quantity;
        balance.Version++;
        Touch(balance);

        await SaveAsync(cancellationToken);
    }

    // ============================================================ 7. pengeluaran

    /// <summary>
    /// Mengeluarkan stok satu obat dari satu lokasi, mengambil batch yang paling dekat
    /// kedaluwarsa lebih dahulu.
    /// </summary>
    public async Task<StockOperationResponse> IssueAsync(IssueStockRequest request,
        CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        var prior = await FindMutationsByKeyPrefixAsync(request.IdempotencyKey, cancellationToken);
        if (prior.Count > 0)
        {
            return new StockOperationResponse
            {
                DrugId = request.DrugId,
                StorageLocationId = request.StorageLocationId,
                TotalQuantity = prior.Sum(x => Math.Abs(x.QuantityChange)),
                Allocations = [.. prior.Select(x => new StockAllocationResponse
                {
                    DrugBatchId = x.DrugBatchId,
                    Quantity = Math.Abs(x.QuantityChange),
                    MutationId = x.Id
                })]
            };
        }

        var candidates = await LoadFefoCandidatesAsync(request.DrugId, request.StorageLocationId,
            cancellationToken, includeReserved: request.ConsumeReservation);

        var plan = PlanFefo(candidates, request.Quantity,
            useReserved: request.ConsumeReservation);

        var allocations = new List<StockAllocationResponse>();
        var index = 0;

        foreach (var (balance, quantity) in plan)
        {
            // Stok yang dipakai dari reservasi harus melepas penahanannya juga, kalau tidak
            // barang yang sudah diserahkan akan tetap terhitung sebagai tertahan.
            if (request.ConsumeReservation)
            {
                var release = Math.Min(balance.QuantityReserved, quantity);
                balance.QuantityReserved -= release;
            }

            var mutation = await ApplyMutationAsync(balance, DrugStockMutationType.StockOut,
                -quantity, Normalize(request.Reason), request.SourceDocumentType,
                request.SourceDocumentId, null,
                $"{request.IdempotencyKey}:{index++}", cancellationToken);

            allocations.Add(new StockAllocationResponse
            {
                DrugBatchId = balance.DrugBatchId,
                BatchNumber = balance.DrugBatch?.BatchNumber ?? string.Empty,
                ExpiryDate = balance.DrugBatch?.ExpiryDate,
                Quantity = quantity,
                MutationId = mutation.Id
            });
        }

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "DrugStock.Issue",
            "Mengeluarkan stok obat.",
            new { request.DrugId, request.StorageLocationId, request.Quantity,
                  request.SourceDocumentType, request.SourceDocumentId });

        return new StockOperationResponse
        {
            DrugId = request.DrugId,
            StorageLocationId = request.StorageLocationId,
            TotalQuantity = request.Quantity,
            Allocations = allocations
        };
    }

    // ============================================ 7b. pergerakan batch tertentu

    /// <summary>
    /// Mengeluarkan satu batch tertentu dari satu lokasi.
    /// </summary>
    /// <remarks>
    /// Berbeda dari pengeluaran biasa, batchnya tidak dipilih ulang menurut FEFO — ia sudah
    /// ditentukan sebelumnya, misalnya oleh reservasi transfer. Memilih ulang di sini akan
    /// mengeluarkan batch yang berbeda dari yang sudah ditahan dan dijanjikan.
    /// </remarks>
    public async Task<TrxDrugStockMutation> IssueBatchAsync(Guid drugBatchId,
        Guid storageLocationId, decimal quantity, bool consumeReservation,
        string? sourceDocumentType, Guid? sourceDocumentId, string? reason,
        string correlationId, CancellationToken cancellationToken = default)
    {
        if (quantity <= 0)
            throw new DrugStockUnprocessableException("PHM022", "Jumlah harus lebih dari nol.");

        var balance = await LoadBalanceAsync(drugBatchId, storageLocationId,
            DrugStockStatus.Available, cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM020",
                "Saldo batch pada lokasi tersebut tidak ditemukan.");

        if (consumeReservation)
        {
            var release = Math.Min(balance.QuantityReserved, quantity);
            balance.QuantityReserved -= release;
        }

        var mutation = await ApplyMutationAsync(balance, DrugStockMutationType.StockOut,
            -quantity, reason, sourceDocumentType, sourceDocumentId, null, correlationId,
            cancellationToken);

        return mutation;
    }

    /// <summary>
    /// Menerima satu batch tertentu di satu lokasi, membuat saldonya bila belum ada.
    /// </summary>
    /// <remarks>
    /// Batch yang diterima adalah batch yang sama dengan yang dikeluarkan lokasi asal, bukan
    /// batch baru. Dengan begitu nomor batch dan kedaluwarsanya tetap melekat pada barangnya
    /// ke mana pun ia berpindah, dan penarikan obat masih dapat menelusurinya.
    /// </remarks>
    public async Task<TrxDrugStockMutation> ReceiveBatchAsync(Guid drugBatchId,
        Guid storageLocationId, decimal quantity, string? sourceDocumentType,
        Guid? sourceDocumentId, string? reason, string correlationId,
        CancellationToken cancellationToken = default,
        DrugStockStatus status = DrugStockStatus.Available)
    {
        if (quantity <= 0)
            throw new DrugStockUnprocessableException("PHM022", "Jumlah harus lebih dari nol.");

        var batch = await _dbContext.MstDrugBatches.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == drugBatchId && !x.IsDelete, cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM031", "Batch tidak ditemukan.");

        // Barang yang masuk tidak selalu langsung siap pakai. Obat retur, misalnya, dapat
        // masuk sebagai karantina bila pemeriksa meragukan penyimpanannya selama di luar.
        var balance = await LoadOrCreateBalanceAsync(batch.DrugId, drugBatchId,
            storageLocationId, status, cancellationToken);

        return await ApplyMutationAsync(balance, DrugStockMutationType.StockIn, quantity,
            reason, sourceDocumentType, sourceDocumentId, null, correlationId,
            cancellationToken);
    }

    /// <summary>
    /// Menahan sejumlah stok pada satu batch tertentu.
    /// </summary>
    /// <remarks>
    /// Dipakai ketika batchnya sudah ditentukan lebih dahulu. Penahanan tidak menulis kartu
    /// stok karena barangnya tidak berpindah; yang berubah hanya berapa yang boleh dijanjikan
    /// kepada proses lain.
    /// </remarks>
    public async Task ReserveBatchAsync(Guid drugBatchId, Guid storageLocationId,
        decimal quantity, CancellationToken cancellationToken = default)
    {
        var balance = await LoadBalanceAsync(drugBatchId, storageLocationId,
            DrugStockStatus.Available, cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM020", "Saldo tidak ditemukan.");

        if (balance.QuantityOnHand - balance.QuantityReserved < quantity)
            throw new DrugStockUnprocessableException("PHM028",
                "Stok tersedia pada batch ini tidak mencukupi.");

        balance.QuantityReserved += quantity;
        balance.Version++;
        Touch(balance);
    }

    /// <summary>
    /// Menyusun rencana pengambilan FEFO tanpa mengubah apa pun.
    /// </summary>
    /// <remarks>
    /// Dipakai ketika pemanggil perlu mengetahui batch mana yang akan terpakai lebih dahulu,
    /// misalnya untuk mencatat alokasi transfer, sebelum memutuskan menahannya.
    /// </remarks>
    public async Task<List<(Guid DrugBatchId, decimal Quantity)>> PlanFefoAsync(Guid drugId,
        Guid storageLocationId, decimal quantity, CancellationToken cancellationToken = default)
    {
        var candidates = await LoadFefoCandidatesAsync(drugId, storageLocationId, cancellationToken);
        return [.. PlanFefo(candidates, quantity).Select(x => (x.Balance.DrugBatchId, x.Quantity))];
    }

    /// <summary>
    /// Memastikan lokasi memang boleh melakukan peran yang diminta.
    /// </summary>
    /// <remarks>
    /// Bendera ini sudah ada di master lokasi dan sengaja dipakai ulang alih-alih menambah
    /// penanda depo tersendiri: pertanyaan "boleh menerima?" dan "boleh mengirim?" persis
    /// itulah yang menentukan sah tidaknya sebuah ujung transfer.
    /// </remarks>
    public async Task<MstDrugStorageLocation> EnsureLocationCapabilityAsync(Guid storageLocationId,
        bool requireTransferOut, bool requireTransferIn, CancellationToken cancellationToken)
    {
        var location = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == storageLocationId && !x.IsDelete, cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM031",
                "Lokasi penyimpanan tidak ditemukan.");

        if (!location.IsActive)
            throw new DrugStockUnprocessableException("PHM034",
                $"Lokasi {location.StorageLocationName} sudah tidak aktif.");

        if (requireTransferOut && !location.IsAllowTransferOut)
            throw new DrugStockUnprocessableException("PHM034",
                $"Lokasi {location.StorageLocationName} tidak diizinkan mengirim stok.");

        if (requireTransferIn && !location.IsAllowTransferIn)
            throw new DrugStockUnprocessableException("PHM034",
                $"Lokasi {location.StorageLocationName} tidak diizinkan menerima stok.");

        return location;
    }

    // ================================================================ 8. koreksi

    /// <summary>
    /// Mengoreksi satu baris kartu stok yang terlanjur salah.
    /// </summary>
    /// <remarks>
    /// Baris lama tidak disentuh. Yang ditambahkan adalah baris baru sebesar selisih antara
    /// nilai yang seharusnya dan nilai yang terlanjur tercatat, menunjuk baris asalnya. Dengan
    /// begitu saldo menjadi benar tanpa menghapus jejak bahwa pernah terjadi kekeliruan.
    /// </remarks>
    public async Task<DrugStockMutationResponse> CorrectMutationAsync(
        CorrectMutationRequest request, CancellationToken cancellationToken = default)
    {
        EnsureIdempotencyKey(request.IdempotencyKey);

        if (string.IsNullOrWhiteSpace(request.Reason))
            throw new DrugStockUnprocessableException("PHM021", "Alasan koreksi wajib diisi.");

        var existing = await FindMutationByKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing != null) return MapMutation(existing);

        var original = await _dbContext.TrxDrugStockMutations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.MutationId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Baris kartu stok tidak ditemukan.");

        if (original.CorrectionOfMutationId != null)
            throw new DrugStockConflictException("PHM026",
                "Baris koreksi tidak dapat dikoreksi lagi. Koreksi baris aslinya.");

        var alreadyCorrected = await _dbContext.TrxDrugStockMutations.AsNoTracking()
            .AnyAsync(x => x.CorrectionOfMutationId == original.Id && !x.IsDelete, cancellationToken);
        if (alreadyCorrected)
            throw new DrugStockConflictException("PHM027",
                "Baris ini sudah pernah dikoreksi.");

        var delta = request.CorrectedQuantityChange - original.QuantityChange;
        if (delta == 0)
            throw new DrugStockUnprocessableException("PHM022",
                "Nilai koreksi sama dengan nilai yang sudah tercatat.");

        var balance = await LoadBalanceAsync(original.DrugBatchId, original.StorageLocationId,
            original.Status, cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM020", "Saldo tidak ditemukan.");

        var mutation = await ApplyMutationAsync(balance, DrugStockMutationType.Adjustment, delta,
            request.Reason.Trim(), original.SourceDocumentType, original.SourceDocumentId,
            original.Id, request.IdempotencyKey, cancellationToken);

        await SaveAsync(cancellationToken);
        await _loggerService.AuditAsync(LogCategory, "DrugStock.Correct",
            "Mengoreksi baris kartu stok.",
            new { OriginalMutationId = original.Id, Before = original.QuantityChange,
                  After = request.CorrectedQuantityChange, request.Reason });

        return MapMutation(mutation);
    }

    // ================================================================== penolong

    /// <summary>
    /// Menerapkan satu pergerakan: memeriksa kecukupan, menulis kartu stok, lalu memperbarui saldo.
    /// </summary>
    /// <remarks>
    /// Seluruh perintah bermuara di sini supaya tidak ada satu pun jalan mengubah saldo tanpa
    /// meninggalkan jejak. Saldo sebelum dan sesudah disimpan pada barisnya sendiri, sehingga
    /// kartu stok dapat dibaca tanpa menghitung ulang seluruh riwayat.
    /// </remarks>
    private async Task<TrxDrugStockMutation> ApplyMutationAsync(TrxDrugStockBalance balance,
        DrugStockMutationType type, decimal quantityChange, string? reason,
        string? sourceDocumentType, Guid? sourceDocumentId, Guid? correctionOf,
        string correlationId, CancellationToken cancellationToken)
    {
        var before = balance.QuantityOnHand;
        var after = before + quantityChange;

        if (after < 0)
            throw new DrugStockUnprocessableException("PHM028",
                $"Stok tidak mencukupi. Tersedia {before}, diminta {Math.Abs(quantityChange)}.");

        // Menurunkan saldo fisik di bawah yang sedang ditahan berarti menjanjikan barang yang
        // sudah tidak ada kepada proses yang menahannya.
        if (after < balance.QuantityReserved)
            throw new DrugStockUnprocessableException("PHM029",
                "Saldo tidak boleh turun di bawah jumlah yang sedang ditahan proses lain.");

        var actorUserId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var mutation = new TrxDrugStockMutation
        {
            DrugId = balance.DrugId,
            DrugBatchId = balance.DrugBatchId,
            StorageLocationId = balance.StorageLocationId,
            Status = balance.Status,
            MutationType = type,
            QuantityChange = quantityChange,
            BalanceBefore = before,
            BalanceAfter = after,
            Reason = reason,
            SourceDocumentType = sourceDocumentType,
            SourceDocumentId = sourceDocumentId,
            CorrectionOfMutationId = correctionOf,
            ActorUserId = actorUserId,
            OccurredAt = now,
            Source = BuildSource(Hash($"{balance.DrugBatchId}|{quantityChange}|{correlationId}")),
            CorrelationId = correlationId.Trim(),
            CreateDateTime = now,
            CreateBy = actorUserId
        };

        _dbContext.TrxDrugStockMutations.Add(mutation);

        balance.QuantityOnHand = after;
        balance.Version++;
        Touch(balance);

        await Task.CompletedTask;
        return mutation;
    }

    /// <summary>
    /// Menyusun rencana pengambilan menurut FEFO.
    /// </summary>
    /// <remarks>
    /// Batch yang paling dekat kedaluwarsa diambil lebih dahulu supaya obat tidak menua di rak
    /// sementara yang lebih baru terpakai. Batch tanpa tanggal kedaluwarsa diletakkan paling
    /// belakang: tanpa tanggal, tidak ada dasar mendahulukannya.
    /// </remarks>
    private static List<(TrxDrugStockBalance Balance, decimal Quantity)> PlanFefo(
        List<TrxDrugStockBalance> candidates, decimal requested, bool useReserved = false)
    {
        if (requested <= 0)
            throw new DrugStockUnprocessableException("PHM022",
                "Jumlah harus lebih dari nol.");

        var plan = new List<(TrxDrugStockBalance, decimal)>();
        var remaining = requested;

        foreach (var balance in candidates)
        {
            if (remaining <= 0) break;

            var usable = useReserved
                ? balance.QuantityOnHand
                : balance.QuantityOnHand - balance.QuantityReserved;

            if (usable <= 0) continue;

            var take = Math.Min(usable, remaining);
            plan.Add((balance, take));
            remaining -= take;
        }

        if (remaining > 0)
            throw new DrugStockUnprocessableException("PHM028",
                $"Stok tidak mencukupi. Kurang {remaining}.");

        return plan;
    }

    private Task<List<TrxDrugStockBalance>> LoadFefoCandidatesAsync(Guid drugId,
        Guid storageLocationId, CancellationToken cancellationToken, bool includeReserved = false)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return _dbContext.TrxDrugStockBalances
            .Include(x => x.DrugBatch)
            .Where(x => !x.IsDelete &&
                        x.DrugId == drugId &&
                        x.StorageLocationId == storageLocationId &&
                        // Hanya stok siap pakai yang boleh keluar. Karantina, kedaluwarsa, dan
                        // barang rusak tetap tercatat tetapi tidak pernah ikut dilayankan.
                        x.Status == DrugStockStatus.Available &&
                        x.QuantityOnHand > 0 &&
                        (includeReserved || x.QuantityOnHand > x.QuantityReserved) &&
                        // Batch yang sudah lewat tanggal tidak boleh keluar walaupun statusnya
                        // belum sempat dipindahkan petugas.
                        (x.DrugBatch!.ExpiryDate == null || x.DrugBatch.ExpiryDate >= today))
            .OrderBy(x => x.DrugBatch!.ExpiryDate == null)
            .ThenBy(x => x.DrugBatch!.ExpiryDate)
            .ThenBy(x => x.CreateDateTime)
            .ToListAsync(cancellationToken);
    }

    private async Task<MstDrugBatch> EnsureBatchAsync(DrugBatchInput input,
        CancellationToken cancellationToken)
    {
        if (input.ExpiryDate == null)
            throw new DrugStockUnprocessableException("PHM030",
                "Tanggal kedaluwarsa batch wajib diisi.");

        if (string.IsNullOrWhiteSpace(input.BatchNumber))
            throw new DrugStockUnprocessableException("PHM030", "Nomor batch wajib diisi.");

        var drugValid = await _dbContext.MstDrugs.AsNoTracking()
            .AnyAsync(x => x.Id == input.DrugId && x.IsActive && !x.IsDelete, cancellationToken);
        if (!drugValid)
            throw new DrugStockUnprocessableException("PHM031",
                "Obat tidak ditemukan atau tidak aktif.");

        var batchNumber = input.BatchNumber.Trim();

        var existing = await _dbContext.MstDrugBatches
            .FirstOrDefaultAsync(x => x.DrugId == input.DrugId &&
                                      x.BatchNumber == batchNumber && !x.IsDelete,
                cancellationToken);

        if (existing != null)
        {
            // Satu nomor batch hanya boleh punya satu tanggal kedaluwarsa. Dua tanggal berbeda
            // pada nomor yang sama berarti salah satunya salah ketik, dan menerimanya diam-diam
            // membuat FEFO memilih berdasarkan angka yang keliru.
            if (existing.ExpiryDate != input.ExpiryDate)
                throw new DrugStockConflictException("PHM032",
                    $"Batch {batchNumber} sudah tercatat dengan tanggal kedaluwarsa berbeda.");

            return existing;
        }

        var actorUserId = GetCurrentUserId();
        var batch = new MstDrugBatch
        {
            DrugId = input.DrugId,
            BatchNumber = batchNumber,
            ExpiryDate = input.ExpiryDate,
            SupplierId = input.SupplierId,
            PrincipalName = Normalize(input.PrincipalName),
            Notes = Normalize(input.Notes),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.MstDrugBatches.Add(batch);
        return batch;
    }

    private async Task EnsureStorageLocationAsync(Guid storageLocationId,
        CancellationToken cancellationToken)
    {
        var valid = await _dbContext.MstDrugStorageLocations.AsNoTracking()
            .AnyAsync(x => x.Id == storageLocationId && !x.IsDelete, cancellationToken);
        if (!valid)
            throw new DrugStockUnprocessableException("PHM031",
                "Lokasi penyimpanan tidak ditemukan.");
    }

    private Task<TrxDrugStockBalance?> LoadBalanceAsync(Guid drugBatchId, Guid storageLocationId,
        DrugStockStatus status, CancellationToken cancellationToken) =>
        _dbContext.TrxDrugStockBalances
            .Include(x => x.DrugBatch)
            .FirstOrDefaultAsync(x => x.DrugBatchId == drugBatchId &&
                                      x.StorageLocationId == storageLocationId &&
                                      x.Status == status && !x.IsDelete, cancellationToken);

    private async Task<TrxDrugStockBalance> LoadOrCreateBalanceAsync(Guid drugId, Guid drugBatchId,
        Guid storageLocationId, DrugStockStatus status, CancellationToken cancellationToken)
    {
        var existing = await LoadBalanceAsync(drugBatchId, storageLocationId, status,
            cancellationToken);
        if (existing != null) return existing;

        var actorUserId = GetCurrentUserId();
        var balance = new TrxDrugStockBalance
        {
            DrugId = drugId,
            DrugBatchId = drugBatchId,
            StorageLocationId = storageLocationId,
            Status = status,
            QuantityOnHand = 0,
            QuantityReserved = 0,
            Version = 0,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.TrxDrugStockBalances.Add(balance);
        return balance;
    }

    private async Task<DrugStockBalanceResponse> LoadBalanceResponseAsync(Guid drugBatchId,
        Guid storageLocationId, DrugStockStatus status, CancellationToken cancellationToken)
    {
        var page = await GetBalancesAsync(new DrugStockBalanceQuery
        {
            DrugBatchId = drugBatchId,
            StorageLocationId = storageLocationId,
            Status = status,
            PageSize = 1
        }, cancellationToken);

        return page.Items.FirstOrDefault()
            ?? throw new DrugStockUnprocessableException("PHM020", "Saldo tidak ditemukan.");
    }

    private Task<TrxDrugStockMutation?> FindMutationByKeyAsync(string key,
        CancellationToken cancellationToken) =>
        _dbContext.TrxDrugStockMutations.AsNoTracking()
            .FirstOrDefaultAsync(x => x.CorrelationId == key.Trim() && !x.IsDelete,
                cancellationToken);

    private Task<List<TrxDrugStockMutation>> FindMutationsByKeyPrefixAsync(string key,
        CancellationToken cancellationToken)
    {
        var prefix = $"{key.Trim()}:";
        return _dbContext.TrxDrugStockMutations.AsNoTracking()
            .Where(x => x.CorrelationId != null && x.CorrelationId.StartsWith(prefix) && !x.IsDelete)
            .OrderBy(x => x.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    private void Touch(TrxDrugStockBalance balance)
    {
        balance.UpdateDateTime = DateTime.UtcNow;
        balance.UpdateBy = GetCurrentUserId();
    }

    private static void EnsureVersion(int current, int expected)
    {
        if (current != expected)
            throw new DrugStockConflictException("PHM033",
                "Saldo telah berubah sejak terakhir dibaca. Muat ulang lalu coba kembali.");
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
            throw new DrugStockConflictException("PHM033",
                "Saldo telah berubah sejak terakhir dibaca. Muat ulang lalu coba kembali.");
        }
    }

    private Guid GetCurrentUserId()
    {
        var value = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? _httpContextAccessor.HttpContext?.User.FindFirstValue("user_id");
        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
            throw new DrugStockForbiddenException("Identitas pengguna tidak valid.");
        return id;
    }

    private static int? DaysToExpiry(DateOnly? expiry, DateOnly today) =>
        expiry == null ? null : expiry.Value.DayNumber - today.DayNumber;

    private static DrugStockMutationResponse MapMutation(TrxDrugStockMutation x) => new()
    {
        Id = x.Id,
        DrugId = x.DrugId,
        DrugBatchId = x.DrugBatchId,
        StorageLocationId = x.StorageLocationId,
        Status = x.Status,
        MutationType = x.MutationType,
        QuantityChange = x.QuantityChange,
        BalanceBefore = x.BalanceBefore,
        BalanceAfter = x.BalanceAfter,
        Reason = x.Reason,
        SourceDocumentType = x.SourceDocumentType,
        SourceDocumentId = x.SourceDocumentId,
        CorrectionOfMutationId = x.CorrectionOfMutationId,
        OccurredAt = x.OccurredAt
    };

    private static string BuildSource(string fingerprint) => $"API:{fingerprint[..46]}";
    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static string Hash(string value) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}

/// <summary>Benturan data atau keadaan yang tidak sah; dipetakan ke `409`.</summary>
public sealed class DrugStockConflictException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>Pengguna tidak berwenang; dipetakan ke `403`.</summary>
public sealed class DrugStockForbiddenException(string message) : Exception(message);

/// <summary>Prasyarat aturan stok belum terpenuhi; dipetakan ke `422`.</summary>
public sealed class DrugStockUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
