using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Periode laporan Logistik. Kosong berarti 30 hari terakhir; paling panjang 366 hari.
/// </summary>
/// <remarks>
/// Tanggal dibaca sebagai tanggal lokal server dan batas akhirnya eksklusif pada hari berikutnya,
/// sama dengan saringan tanggal pada daftar permintaan dan kartu stok. Batas 366 hari adalah
/// batas teknis: laporan dijumlah di memori sesudah disaring basis data (pola yang sama dengan
/// laporan lain yang teruji di SQLite), sehingga rentangnya perlu dibatasi.
/// </remarks>
public sealed record LogisticsReportPeriod(DateOnly StartDate, DateOnly EndDate, DateTime FromUtc,
    DateTime ToUtcExclusive)
{
    public const int DefaultDays = 30;
    public const int MaxDays = 366;
    public const string InvalidPeriodCode = "PHM035";

    public int DayCount => EndDate.DayNumber - StartDate.DayNumber + 1;

    public static LogisticsReportPeriod Resolve(DateTime? start, DateTime? end, DateTime nowLocal)
    {
        var endDate = DateOnly.FromDateTime((end ?? nowLocal).Date);
        var startDate = start.HasValue
            ? DateOnly.FromDateTime(start.Value.Date)
            : endDate.AddDays(-(DefaultDays - 1));

        if (startDate > endDate)
            throw new LogisticsReportUnprocessableException(InvalidPeriodCode,
                "Tanggal mulai laporan tidak boleh sesudah tanggal selesai.");

        if (endDate.DayNumber - startDate.DayNumber + 1 > MaxDays)
            throw new LogisticsReportUnprocessableException(InvalidPeriodCode,
                $"Periode laporan paling panjang {MaxDays} hari. Persempit rentang tanggal.");

        var fromUtc = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local).ToUniversalTime();
        var toUtcExclusive = endDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Local)
            .ToUniversalTime();
        return new LogisticsReportPeriod(startDate, endDate, fromUtc, toUtcExclusive);
    }

    public LogisticsReportPeriodResponse ToResponse() =>
        new() { StartDate = StartDate, EndDate = EndDate, DayCount = DayCount };
}

/// <summary>Masukan laporan yang tidak dapat diproses, mis. periode yang tidak sah (422).</summary>
public sealed class LogisticsReportUnprocessableException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}

/// <summary>
/// Lokasi penyimpanan yang buku mutasinya sudah lolos UAT L1–L2 (FIN-LGS-056). Hanya untuk lokasi
/// inilah saldo historis Mutasi Saldo Persediaan berstatus valid.
/// </summary>
/// <remarks>
/// Diisi operator per lingkungan sesudah bukti UAT tersedia, berupa id lokasi dipisah koma atau
/// daftar anak konfigurasi. Kosong berarti belum ada lokasi yang terverifikasi: angka tetap tampil,
/// tetapi berstatus "belum terverifikasi". Id yang tidak sah diabaikan.
/// </remarks>
public static class LogisticsLedgerVerification
{
    public const string ConfigKey = "PharmacyManagement:Logistics:LedgerVerifiedStorageLocations";

    /// <summary>Menafsirkan daftar entri mentah. Fungsi murni.</summary>
    public static IReadOnlySet<Guid> Parse(IEnumerable<string?> rawEntries)
    {
        var ids = new HashSet<Guid>();
        foreach (var raw in rawEntries)
        {
            foreach (var part in (raw ?? string.Empty).Split(',',
                         StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
            {
                if (Guid.TryParse(part, out var id) && id != Guid.Empty) ids.Add(id);
            }
        }

        return ids;
    }

    /// <summary>Membaca dari konfigurasi: nilai tunggal maupun daftar anak.</summary>
    public static IReadOnlySet<Guid> FromConfiguration(IConfiguration? configuration)
    {
        if (configuration == null) return new HashSet<Guid>();

        var section = configuration.GetSection(ConfigKey);
        var entries = new List<string?> { section.Value };
        entries.AddRange(section.GetChildren().Select(x => x.Value));
        return Parse(entries);
    }
}

/// <summary>
/// Query laporan Logistik berbasis jumlah (FIN-LGS-024): Laporan Persediaan, Mutasi, Distribusi,
/// Pemakaian per Unit, dan Dashboard Penerimaan &amp; Pengeluaran. Hanya membaca.
/// </summary>
/// <remarks>
/// <para>
/// Dipanggil lewat service pemilik izin masing-masing (<see cref="DrugStockService"/>,
/// <see cref="StockRequestService"/>, <see cref="DrugUsageService"/>), sehingga controller tidak
/// menyentuh basis data dan izinnya memakai resource yang sudah ada (<c>DrugStock:Read</c>,
/// <c>StockRequest:Read</c>, <c>DrugUsage:Read</c>).
/// </para>
/// <para>
/// Aturan angka: tidak ada nilai rupiah (harga per batch belum disimpan, <c>LGS-CAP-32</c>);
/// jumlah barang selalu per obat dalam satuannya sendiri dan tidak pernah dijumlahkan lintas obat;
/// ringkasan lintas obat berupa hitungan baris atau transaksi.
/// </para>
/// </remarks>
public sealed class LogisticsReportQueries(ApplicationDbContext db)
{
    /// <summary>Aksi riwayat penyerahan, sama dengan konstanta di <see cref="StockRequestService"/>.</summary>
    private const string FulfillStockRequestAction = "FulfillStockRequest";

    private const string NoValuationLimitation =
        "Nilai persediaan (rupiah) belum tersedia: harga per batch belum disimpan sistem " +
        "(LGS-CAP-32; metode valuasi menunggu LGS-OQ-008).";

    private const string PerDrugUnitLimitation =
        "Jumlah ditampilkan per obat dalam satuannya sendiri dan tidak dijumlahkan lintas obat.";

    private const string ProcurementLimitation =
        "Penerimaan dari pengadaan (Goods Receipt) belum menambah stok gudang (LGS-CAP-07; " +
        "menunggu LGS-OQ-017), sehingga tidak muncul sebagai barang masuk.";

    // ================================================================ Laporan Persediaan

    public async Task<InventoryReportResponse> GetInventoryAsync(InventoryReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var balances = db.PhmDrugStockBalances.AsNoTracking().Where(x => !x.IsDelete);

        if (request.StorageLocationId.HasValue)
            balances = balances.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (request.DrugId.HasValue) balances = balances.Where(x => x.DrugId == request.DrugId);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            balances = balances.Where(x =>
                x.Drug!.DrugName.ToLower().Contains(keyword) ||
                x.Drug.DrugCode.ToLower().Contains(keyword));
        }

        // Disaring di basis data, dijumlah di memori.
        var rows = await balances
            .Select(x => new
            {
                x.DrugId,
                x.DrugBatchId,
                x.StorageLocationId,
                x.Status,
                x.QuantityOnHand,
                x.QuantityReserved,
                ExpiryDate = x.DrugBatch!.ExpiryDate
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.Now);

        var groups = rows
            .GroupBy(x => (x.DrugId, x.StorageLocationId))
            .Select(g =>
            {
                decimal OnHand(DrugStockStatus status) =>
                    g.Where(r => r.Status == status).Sum(r => r.QuantityOnHand);

                var available = OnHand(DrugStockStatus.Available);
                var reserved = g.Where(r => r.Status == DrugStockStatus.Available).Sum(r => r.QuantityReserved);
                var nearest = g
                    .Where(r => r.Status == DrugStockStatus.Available && r.QuantityOnHand > 0)
                    .Min(r => r.ExpiryDate);

                return new
                {
                    g.Key.DrugId,
                    g.Key.StorageLocationId,
                    Available = available,
                    Reserved = reserved,
                    Quarantine = OnHand(DrugStockStatus.Quarantine),
                    Expired = OnHand(DrugStockStatus.Expired),
                    Damaged = OnHand(DrugStockStatus.Damaged),
                    Total = g.Sum(r => r.QuantityOnHand),
                    BatchCount = g.Where(r => r.QuantityOnHand > 0).Select(r => r.DrugBatchId).Distinct().Count(),
                    Nearest = nearest
                };
            })
            .Where(x => request.IncludeEmpty || x.Total > 0 || x.Reserved > 0)
            .ToList();

        var drugs = await LoadDrugInfoAsync(groups.Select(x => x.DrugId).Distinct().ToList(), cancellationToken);
        var locations = await LoadLocationNamesAsync(groups.Select(x => x.StorageLocationId).Distinct().ToList(),
            cancellationToken);

        var items = groups
            .Select(x =>
            {
                var drug = drugs.GetValueOrDefault(x.DrugId);
                return new InventoryReportRowResponse
                {
                    DrugId = x.DrugId,
                    DrugCode = drug?.Code ?? string.Empty,
                    DrugName = drug?.Name ?? string.Empty,
                    StockUnitName = drug?.StockUnitName,
                    StorageLocationId = x.StorageLocationId,
                    StorageLocationName = locations.GetValueOrDefault(x.StorageLocationId) ?? string.Empty,
                    AvailableOnHand = x.Available,
                    Reserved = x.Reserved,
                    AvailableToIssue = x.Available - x.Reserved,
                    Quarantine = x.Quarantine,
                    Expired = x.Expired,
                    Damaged = x.Damaged,
                    TotalOnHand = x.Total,
                    BatchCount = x.BatchCount,
                    NearestExpiryDate = x.Nearest,
                    DaysToNearestExpiry = x.Nearest.HasValue ? x.Nearest.Value.DayNumber - today.DayNumber : null
                };
            })
            .OrderBy(x => x.DrugName)
            .ThenBy(x => x.StorageLocationName)
            .ToList();

        return new InventoryReportResponse
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Summary = new InventoryReportSummaryResponse
            {
                RowCount = items.Count,
                DrugCount = items.Select(x => x.DrugId).Distinct().Count(),
                LocationCount = items.Select(x => x.StorageLocationId).Distinct().Count(),
                RowsWithoutAvailable = items.Count(x => x.AvailableToIssue <= 0),
                RowsWithQuarantine = items.Count(x => x.Quarantine > 0),
                RowsWithExpired = items.Count(x => x.Expired > 0),
                RowsWithDamaged = items.Count(x => x.Damaged > 0),
                RowsWithExpiredBatchStillAvailable = items.Count(x => x.DaysToNearestExpiry < 0)
            },
            Rows = Page(items, request.PageNumber, request.PageSize),
            Limitations =
            [
                NoValuationLimitation,
                PerDrugUnitLimitation,
                "Saldo adalah keadaan saat laporan dibuat; potret saldo per tanggal belum disimpan sistem."
            ]
        };
    }

    // ============================================================ Laporan Mutasi Barang

    public async Task<MutationReportResponse> GetMutationsAsync(MutationReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var period = LogisticsReportPeriod.Resolve(request.StartDate, request.EndDate, DateTime.Now);

        var mutations = db.PhmDrugStockMutations.AsNoTracking()
            .Where(x => !x.IsDelete && x.OccurredAt >= period.FromUtc && x.OccurredAt < period.ToUtcExclusive);

        if (request.StorageLocationId.HasValue)
            mutations = mutations.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (request.DrugId.HasValue) mutations = mutations.Where(x => x.DrugId == request.DrugId);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            mutations = mutations.Where(x =>
                x.Drug!.DrugName.ToLower().Contains(keyword) ||
                x.Drug.DrugCode.ToLower().Contains(keyword));
        }

        var rows = await mutations
            .Select(x => new { x.DrugId, x.StorageLocationId, x.MutationType, x.QuantityChange, x.OccurredAt })
            .ToListAsync(cancellationToken);

        // Perubahan status memindahkan saldo antarstatus pada batch dan lokasi yang sama; bukan
        // barang masuk atau keluar, sehingga hanya dihitung jumlah transaksinya.
        var movements = rows.Where(x => x.MutationType != DrugStockMutationType.StatusChange).ToList();

        var groups = movements
            .GroupBy(x => (x.DrugId, x.StorageLocationId))
            .Select(g =>
            {
                decimal Sum(DrugStockMutationType type) =>
                    g.Where(r => r.MutationType == type).Sum(r => r.QuantityChange);

                var opening = Sum(DrugStockMutationType.OpeningBalance);
                var quantityIn = Sum(DrugStockMutationType.StockIn);
                var quantityOut = -Sum(DrugStockMutationType.StockOut);
                var adjustment = Sum(DrugStockMutationType.Adjustment);

                return new
                {
                    g.Key.DrugId,
                    g.Key.StorageLocationId,
                    Opening = opening,
                    In = quantityIn,
                    Out = quantityOut,
                    Adjustment = adjustment,
                    Net = opening + quantityIn - quantityOut + adjustment,
                    Count = g.Count(),
                    Last = g.Max(r => r.OccurredAt)
                };
            })
            .ToList();

        var drugs = await LoadDrugInfoAsync(groups.Select(x => x.DrugId).Distinct().ToList(), cancellationToken);
        var locations = await LoadLocationNamesAsync(groups.Select(x => x.StorageLocationId).Distinct().ToList(),
            cancellationToken);

        var ordered = groups
            .Select(x => new
            {
                Group = x,
                Drug = drugs.GetValueOrDefault(x.DrugId),
                LocationName = locations.GetValueOrDefault(x.StorageLocationId) ?? string.Empty
            })
            .OrderBy(x => x.Drug?.Name ?? string.Empty)
            .ThenBy(x => x.LocationName)
            .ToList();

        var page = Page(ordered, request.PageNumber, request.PageSize);

        // Saldo saat ini hanya untuk baris pada halaman ini.
        var pageDrugIds = page.Items.Select(x => x.Group.DrugId).Distinct().ToList();
        var pageLocationIds = page.Items.Select(x => x.Group.StorageLocationId).Distinct().ToList();
        var currentRows = pageDrugIds.Count == 0
            ? []
            : await db.PhmDrugStockBalances.AsNoTracking()
                .Where(x => !x.IsDelete && pageDrugIds.Contains(x.DrugId) &&
                            pageLocationIds.Contains(x.StorageLocationId))
                .Select(x => new { x.DrugId, x.StorageLocationId, x.QuantityOnHand })
                .ToListAsync(cancellationToken);
        var current = currentRows
            .GroupBy(x => (x.DrugId, x.StorageLocationId))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantityOnHand));

        return new MutationReportResponse
        {
            Period = period.ToResponse(),
            Summary = new MutationReportSummaryResponse
            {
                MutationCount = rows.Count,
                InCount = rows.Count(x => x.MutationType == DrugStockMutationType.StockIn),
                OutCount = rows.Count(x => x.MutationType == DrugStockMutationType.StockOut),
                AdjustmentCount = rows.Count(x => x.MutationType == DrugStockMutationType.Adjustment),
                OpeningCount = rows.Count(x => x.MutationType == DrugStockMutationType.OpeningBalance),
                StatusChangeCount = rows.Count(x => x.MutationType == DrugStockMutationType.StatusChange),
                DrugCount = groups.Select(x => x.DrugId).Distinct().Count(),
                LocationCount = groups.Select(x => x.StorageLocationId).Distinct().Count()
            },
            Rows = new PagedResult<MutationReportRowResponse>
            {
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalData = page.TotalData,
                TotalPage = page.TotalPage,
                Items = [.. page.Items.Select(x => new MutationReportRowResponse
                {
                    DrugId = x.Group.DrugId,
                    DrugCode = x.Drug?.Code ?? string.Empty,
                    DrugName = x.Drug?.Name ?? string.Empty,
                    StockUnitName = x.Drug?.StockUnitName,
                    StorageLocationId = x.Group.StorageLocationId,
                    StorageLocationName = x.LocationName,
                    OpeningRecorded = x.Group.Opening,
                    QuantityIn = x.Group.In,
                    QuantityOut = x.Group.Out,
                    AdjustmentNet = x.Group.Adjustment,
                    NetChange = x.Group.Net,
                    MutationCount = x.Group.Count,
                    LastMutationAt = x.Group.Last,
                    CurrentOnHand = current.GetValueOrDefault((x.Group.DrugId, x.Group.StorageLocationId))
                })]
            },
            Limitations =
            [
                "Saldo awal dan saldo akhir per periode belum disajikan karena sistem tidak menyimpan " +
                "potret saldo per tanggal. Kolom \"Saldo Saat Ini\" adalah saldo ketika laporan dibuat.",
                "Perubahan status (mis. ke karantina atau rusak) tidak dihitung sebagai masuk/keluar.",
                ProcurementLimitation,
                PerDrugUnitLimitation
            ]
        };
    }

    // ================================================ Dashboard Penerimaan & Pengeluaran

    public async Task<StockFlowSummaryResponse> GetStockFlowAsync(StockFlowSummaryQuery request,
        CancellationToken cancellationToken = default)
    {
        var period = LogisticsReportPeriod.Resolve(request.StartDate, request.EndDate, DateTime.Now);

        var mutations = db.PhmDrugStockMutations.AsNoTracking()
            .Where(x => !x.IsDelete && x.OccurredAt >= period.FromUtc && x.OccurredAt < period.ToUtcExclusive);

        if (request.StorageLocationId.HasValue)
            mutations = mutations.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (request.DrugId.HasValue) mutations = mutations.Where(x => x.DrugId == request.DrugId);

        var rows = await mutations
            .Select(x => new { x.DrugId, x.MutationType, x.QuantityChange, x.OccurredAt, x.SourceDocumentType })
            .ToListAsync(cancellationToken);

        var classified = rows
            .Select(x => new { Row = x, Direction = DirectionOf(x.MutationType) })
            .ToList();
        var flows = classified.Where(x => x.Direction != null).ToList();

        var bySource = flows
            .GroupBy(x => (Direction: x.Direction!, x.Row.SourceDocumentType))
            .Select(g => new StockFlowSourceRowResponse
            {
                Direction = g.Key.Direction,
                SourceDocumentType = g.Key.SourceDocumentType,
                MutationCount = g.Count(),
                DrugCount = g.Select(x => x.Row.DrugId).Distinct().Count()
            })
            .OrderBy(x => DirectionOrder(x.Direction))
            .ThenByDescending(x => x.MutationCount)
            .ToList();

        var perDay = flows
            .GroupBy(x => ToLocalDate(x.Row.OccurredAt))
            .ToDictionary(g => g.Key, g => g.ToList());

        var daily = new List<StockFlowDailyRowResponse>();
        for (var date = period.StartDate; date <= period.EndDate; date = date.AddDays(1))
        {
            var day = perDay.GetValueOrDefault(date) ?? [];
            daily.Add(new StockFlowDailyRowResponse
            {
                Date = date,
                InCount = day.Count(x => x.Direction == StockFlowDirections.In),
                OutCount = day.Count(x => x.Direction == StockFlowDirections.Out),
                AdjustmentCount = day.Count(x => x.Direction == StockFlowDirections.Adjustment)
            });
        }

        var received = flows.Where(x => x.Direction == StockFlowDirections.In).Select(x => x.Row).ToList();
        var issued = flows.Where(x => x.Direction == StockFlowDirections.Out).Select(x => x.Row).ToList();

        var topReceivedRaw = received
            .GroupBy(x => x.DrugId)
            .Select(g => new { DrugId = g.Key, Quantity = g.Sum(x => x.QuantityChange), Count = g.Count() })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToList();
        var topIssuedRaw = issued
            .GroupBy(x => x.DrugId)
            .Select(g => new { DrugId = g.Key, Quantity = -g.Sum(x => x.QuantityChange), Count = g.Count() })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToList();

        var drugs = await LoadDrugInfoAsync(
            topReceivedRaw.Select(x => x.DrugId).Concat(topIssuedRaw.Select(x => x.DrugId)).Distinct().ToList(),
            cancellationToken);

        StockFlowDrugRowResponse ToDrugRow(Guid drugId, decimal quantity, int count)
        {
            var drug = drugs.GetValueOrDefault(drugId);
            return new StockFlowDrugRowResponse
            {
                DrugId = drugId,
                DrugCode = drug?.Code ?? string.Empty,
                DrugName = drug?.Name ?? string.Empty,
                StockUnitName = drug?.StockUnitName,
                Quantity = quantity,
                MutationCount = count
            };
        }

        var hasProcurementReceipts = received.Any(x =>
            x.SourceDocumentType == DrugStockSourceDocumentTypes.GoodsReceipt);

        var limitations = new List<string>();
        if (!hasProcurementReceipts) limitations.Add(ProcurementLimitation);
        limitations.Add("Ringkasan dan grafik harian memakai jumlah transaksi mutasi; jumlah barang hanya " +
                        "ditampilkan per obat dalam satuannya sendiri.");
        limitations.Add("Perubahan status (mis. ke karantina) tidak dihitung sebagai masuk/keluar.");
        limitations.Add("Pengelompokan harian memakai zona waktu server.");

        return new StockFlowSummaryResponse
        {
            Period = period.ToResponse(),
            Totals = new StockFlowTotalsResponse
            {
                InMutationCount = received.Count,
                OutMutationCount = issued.Count,
                AdjustmentCount = flows.Count(x => x.Direction == StockFlowDirections.Adjustment),
                OpeningCount = flows.Count(x => x.Direction == StockFlowDirections.Opening),
                StatusChangeCount = rows.Count(x => x.MutationType == DrugStockMutationType.StatusChange),
                DrugsReceivedCount = received.Select(x => x.DrugId).Distinct().Count(),
                DrugsIssuedCount = issued.Select(x => x.DrugId).Distinct().Count()
            },
            BySource = bySource,
            Daily = daily,
            TopReceivedDrugs = [.. topReceivedRaw.Select(x => ToDrugRow(x.DrugId, x.Quantity, x.Count))],
            TopIssuedDrugs = [.. topIssuedRaw.Select(x => ToDrugRow(x.DrugId, x.Quantity, x.Count))],
            HasProcurementReceipts = hasProcurementReceipts,
            Limitations = limitations
        };
    }

    // ======================================================= Laporan Distribusi Barang

    /// <param name="request">Saringan laporan.</param>
    /// <param name="visibleRequests">
    /// Permintaan yang boleh dibaca pengguna, sudah diberi cakupan oleh <see cref="StockRequestService"/>.
    /// </param>
    /// <param name="cancellationToken">Pembatalan operasi.</param>
    public async Task<DistributionReportResponse> GetDistributionAsync(DistributionReportQuery request,
        IQueryable<PhmStockRequest> visibleRequests, CancellationToken cancellationToken = default)
    {
        var period = LogisticsReportPeriod.Resolve(request.StartDate, request.EndDate, DateTime.Now);

        var requests = visibleRequests.Where(x => !x.IsDelete && x.Status == StockRequestStatus.Completed);
        if (request.RequestingServiceUnitId.HasValue)
            requests = requests.Where(x => x.RequestingServiceUnitId == request.RequestingServiceUnitId);
        if (request.StorageLocationId.HasValue)
            requests = requests.Where(x => x.StorageLocationId == request.StorageLocationId);

        // Waktu distribusi = waktu penyerahan yang tercatat di riwayat.
        var fulfillments = await (
                from history in db.PhmStockRequestHistories.AsNoTracking()
                join stockRequest in requests on history.StockRequestId equals stockRequest.Id
                where !history.IsDelete && history.Action == FulfillStockRequestAction &&
                      history.OccurredAt >= period.FromUtc && history.OccurredAt < period.ToUtcExclusive
                select new
                {
                    stockRequest.Id,
                    stockRequest.RequestingServiceUnitId,
                    UnitName = stockRequest.RequestingServiceUnit != null
                        ? stockRequest.RequestingServiceUnit.ServiceUnitName : string.Empty,
                    history.OccurredAt
                })
            .ToListAsync(cancellationToken);

        var byRequest = fulfillments
            .GroupBy(x => x.Id)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.OccurredAt).First());
        var requestIds = byRequest.Keys.ToList();

        var itemRows = requestIds.Count == 0
            ? []
            : await db.PhmStockRequestItems.AsNoTracking()
                .Where(i => requestIds.Contains(i.StockRequestId) && !i.IsDelete && i.FulfilledQuantity != null)
                .Select(i => new
                {
                    i.StockRequestId,
                    i.DrugId,
                    i.DrugCodeSnapshot,
                    i.DrugNameSnapshot,
                    i.MeasurementId,
                    i.MeasurementNameSnapshot,
                    i.RequestedQuantity,
                    Fulfilled = i.FulfilledQuantity!.Value
                })
                .ToListAsync(cancellationToken);

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();
        var lines = itemRows
            .Select(i => new { Item = i, Request = byRequest[i.StockRequestId] })
            .Where(x => !request.DrugId.HasValue || x.Item.DrugId == request.DrugId)
            .Where(x => keyword == null ||
                        x.Item.DrugNameSnapshot.ToLower().Contains(keyword) ||
                        x.Item.DrugCodeSnapshot.ToLower().Contains(keyword) ||
                        x.Request.UnitName.ToLower().Contains(keyword))
            .ToList();

        var rows = lines
            .GroupBy(x => (x.Request.RequestingServiceUnitId, x.Item.DrugId, x.Item.MeasurementId))
            .Select(g =>
            {
                var first = g.First();
                var shortLines = g.Where(x => x.Item.Fulfilled < x.Item.RequestedQuantity).ToList();
                return new DistributionReportRowResponse
                {
                    RequestingServiceUnitId = g.Key.RequestingServiceUnitId,
                    RequestingServiceUnitName = first.Request.UnitName,
                    DrugId = g.Key.DrugId,
                    DrugCode = first.Item.DrugCodeSnapshot,
                    DrugName = first.Item.DrugNameSnapshot,
                    MeasurementId = g.Key.MeasurementId,
                    MeasurementName = first.Item.MeasurementNameSnapshot,
                    RequestCount = g.Select(x => x.Item.StockRequestId).Distinct().Count(),
                    RequestedQuantity = g.Sum(x => x.Item.RequestedQuantity),
                    FulfilledQuantity = g.Sum(x => x.Item.Fulfilled),
                    ShortfallQuantity = shortLines.Sum(x => x.Item.RequestedQuantity - x.Item.Fulfilled),
                    ShortfallLineCount = shortLines.Count,
                    LastFulfilledAt = g.Max(x => x.Request.OccurredAt)
                };
            })
            .OrderBy(x => x.RequestingServiceUnitName)
            .ThenBy(x => x.DrugName)
            .ToList();

        var includedRequests = lines.Select(x => x.Item.StockRequestId).Distinct().ToList();

        return new DistributionReportResponse
        {
            Period = period.ToResponse(),
            Summary = new DistributionReportSummaryResponse
            {
                CompletedRequestCount = includedRequests.Count,
                PartiallyFulfilledRequestCount = lines
                    .Where(x => x.Item.Fulfilled < x.Item.RequestedQuantity)
                    .Select(x => x.Item.StockRequestId)
                    .Distinct()
                    .Count(),
                LineCount = lines.Count,
                UnitCount = rows.Select(x => x.RequestingServiceUnitId).Distinct().Count(),
                DrugCount = rows.Select(x => x.DrugId).Distinct().Count()
            },
            Rows = Page(rows, request.PageNumber, request.PageSize),
            Limitations =
            [
                "Distribusi di sini adalah penyerahan permintaan unit yang berstatus Selesai, menurut waktu " +
                "penyerahan. Transfer antar gudang dan penyerahan resep ke pasien tidak termasuk.",
                "Jumlah dalam satuan permintaan masing-masing baris; tidak dijumlahkan lintas obat atau satuan.",
                "Cakupan baca mengikuti daftar permintaan; pembatasan lintas unit menunggu LGS-OQ-027."
            ]
        };
    }

    // ===================================================== Laporan Pemakaian per Unit

    public async Task<UsageByUnitReportResponse> GetUsageByUnitAsync(UsageByUnitReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var period = LogisticsReportPeriod.Resolve(request.StartDate, request.EndDate, DateTime.Now);

        // Hanya pemakaian yang sudah dicatat (stok sudah dipotong): belum ditagih atau sudah ditagih.
        var usages = db.PhmDrugUsages.AsNoTracking()
            .Where(x => !x.IsDelete &&
                        (x.Status == DrugUsageStatus.NotBilled || x.Status == DrugUsageStatus.Billed) &&
                        x.UsedAt >= period.FromUtc && x.UsedAt < period.ToUtcExclusive);

        if (request.StorageLocationId.HasValue)
            usages = usages.Where(x => x.StorageLocationId == request.StorageLocationId);

        var documentQuery =
            from usage in usages
            join encounter in db.RegPatientEncounters.AsNoTracking() on usage.EncounterId equals encounter.Id
            select new { usage.Id, usage.EncounterId, encounter.ServiceUnitId, usage.UsedAt };

        if (request.ServiceUnitId.HasValue)
            documentQuery = documentQuery.Where(x => x.ServiceUnitId == request.ServiceUnitId);

        var documents = (await documentQuery.ToListAsync(cancellationToken)).ToDictionary(x => x.Id);
        var documentIds = documents.Keys.ToList();

        var itemRows = documentIds.Count == 0
            ? []
            : await db.PhmDrugUsageItems.AsNoTracking()
                .Where(i => documentIds.Contains(i.DrugUsageId) && !i.IsDelete)
                .Select(i => new
                {
                    i.DrugUsageId,
                    i.DrugId,
                    i.DrugCodeSnapshot,
                    i.DrugNameSnapshot,
                    i.MeasurementId,
                    i.MeasurementNameSnapshot,
                    i.Quantity
                })
                .ToListAsync(cancellationToken);

        var unitIds = documents.Values.Select(x => x.ServiceUnitId).Distinct().ToList();
        var unitNames = unitIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.MstServiceUnits.AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.ServiceUnitName, cancellationToken);

        var keyword = string.IsNullOrWhiteSpace(request.Search) ? null : request.Search.Trim().ToLower();
        var lines = itemRows
            .Select(i => new
            {
                Item = i,
                Document = documents[i.DrugUsageId],
                UnitName = unitNames.GetValueOrDefault(documents[i.DrugUsageId].ServiceUnitId) ?? string.Empty
            })
            .Where(x => !request.DrugId.HasValue || x.Item.DrugId == request.DrugId)
            .Where(x => keyword == null ||
                        x.Item.DrugNameSnapshot.ToLower().Contains(keyword) ||
                        x.Item.DrugCodeSnapshot.ToLower().Contains(keyword) ||
                        x.UnitName.ToLower().Contains(keyword))
            .ToList();

        var rows = lines
            .GroupBy(x => (x.Document.ServiceUnitId, x.Item.DrugId, x.Item.MeasurementId))
            .Select(g =>
            {
                var first = g.First();
                return new UsageByUnitReportRowResponse
                {
                    ServiceUnitId = g.Key.ServiceUnitId,
                    ServiceUnitName = first.UnitName,
                    DrugId = g.Key.DrugId,
                    DrugCode = first.Item.DrugCodeSnapshot,
                    DrugName = first.Item.DrugNameSnapshot,
                    MeasurementId = g.Key.MeasurementId,
                    MeasurementName = first.Item.MeasurementNameSnapshot,
                    UsageDocumentCount = g.Select(x => x.Item.DrugUsageId).Distinct().Count(),
                    EncounterCount = g.Select(x => x.Document.EncounterId).Distinct().Count(),
                    Quantity = g.Sum(x => x.Item.Quantity),
                    LastUsedAt = g.Max(x => x.Document.UsedAt)
                };
            })
            .OrderBy(x => x.ServiceUnitName)
            .ThenBy(x => x.DrugName)
            .ToList();

        return new UsageByUnitReportResponse
        {
            Period = period.ToResponse(),
            Summary = new UsageByUnitReportSummaryResponse
            {
                UsageDocumentCount = lines.Select(x => x.Item.DrugUsageId).Distinct().Count(),
                EncounterCount = lines.Select(x => x.Document.EncounterId).Distinct().Count(),
                UnitCount = rows.Select(x => x.ServiceUnitId).Distinct().Count(),
                DrugCount = rows.Select(x => x.DrugId).Distinct().Count()
            },
            Rows = Page(rows, request.PageNumber, request.PageSize),
            Limitations =
            [
                "Pemakaian di sini adalah pemakaian obat dan alat kesehatan yang dicatat pada kunjungan pasien " +
                "(status tercatat atau sudah ditagih). Draf dan pemakaian yang dibatalkan tidak dihitung.",
                "Unit = unit layanan kunjungan pasien. Penyerahan resep, distribusi ke unit, dan pemakaian yang " +
                "dicatat di luar Pemakaian Obat tidak termasuk; arti unit yang memegang stok sendiri menunggu LGS-OQ-003.",
                "Jumlah dalam satuan pemakaian masing-masing baris; tidak dijumlahkan lintas obat atau satuan."
            ]
        };
    }

    // ============================================ Pemeriksaan kelengkapan buku mutasi

    /// <summary>Kode 422 bila lokasi tidak dipilih pada pemeriksaan buku mutasi.</summary>
    public const string LedgerLocationRequiredCode = "PHM036";

    /// <summary>
    /// Membandingkan saldo fisik tersimpan dengan jumlah semua perubahan di buku mutasi, per batch ×
    /// status pada satu lokasi (FIN-LGS-039). Hanya membaca.
    /// </summary>
    /// <remarks>
    /// Ini bukti untuk keputusan "Mutasi Saldo Persediaan": saldo awal suatu periode hanya dapat
    /// dihitung dari buku mutasi bila setiap saldo dapat direkonstruksi darinya. Di source, setiap
    /// perubahan <c>QuantityOnHand</c> melewati satu jalur yang selalu menulis mutasi
    /// (<c>DrugStockService</c>) dan seeder saldo pembuka juga menulis mutasinya; pemeriksaan ini
    /// membuktikan hal yang sama pada data di lingkungan sebenarnya. Tidak ada saldo yang dikoreksi.
    /// </remarks>
    public async Task<LedgerConsistencyResponse> GetLedgerConsistencyAsync(LedgerConsistencyQuery request,
        CancellationToken cancellationToken = default)
    {
        if (!request.StorageLocationId.HasValue || request.StorageLocationId.Value == Guid.Empty)
            throw new LogisticsReportUnprocessableException(LedgerLocationRequiredCode,
                "Pilih lokasi penyimpanan yang akan diperiksa.");

        var locationId = request.StorageLocationId.Value;

        // Lokasi yang tidak terdaftar tidak boleh menghasilkan "konsisten" karena tidak ada yang
        // diperiksa. Lokasi nonaktif atau terhapus tetap boleh diperiksa: sisa saldonya justru
        // perlu dibuktikan.
        var locationName = await db.MstDrugStorageLocations.AsNoTracking()
            .Where(x => x.Id == locationId)
            .Select(x => x.StorageLocationName)
            .FirstOrDefaultAsync(cancellationToken);
        if (locationName == null)
            throw new LogisticsReportUnprocessableException(LedgerLocationRequiredCode,
                "Lokasi penyimpanan tidak ditemukan.");

        var balanceQuery = db.PhmDrugStockBalances.AsNoTracking()
            .Where(x => !x.IsDelete && x.StorageLocationId == locationId);
        var mutationQuery = db.PhmDrugStockMutations.AsNoTracking()
            .Where(x => !x.IsDelete && x.StorageLocationId == locationId);
        if (request.DrugId.HasValue)
        {
            balanceQuery = balanceQuery.Where(x => x.DrugId == request.DrugId);
            mutationQuery = mutationQuery.Where(x => x.DrugId == request.DrugId);
        }

        // Disaring di basis data, dijumlah di memori (SQLite tidak dapat menjumlah decimal).
        var balanceRows = await balanceQuery
            .Select(x => new { x.DrugId, x.DrugBatchId, x.Status, x.QuantityOnHand })
            .ToListAsync(cancellationToken);
        var mutationRows = await mutationQuery
            .Select(x => new { x.DrugId, x.DrugBatchId, x.Status, x.QuantityChange, x.OccurredAt })
            .ToListAsync(cancellationToken);

        // Kunci = obat × batch × status pada lokasi ini. Saldo hidup unik per batch × lokasi × status
        // (indeks unik dengan saringan IsDelete = false), sehingga tidak ada saldo yang terhitung dua
        // kali; setiap baris mutasi dijumlah tepat sekali di kuncinya sendiri. Perubahan status
        // menulis dua baris (status asal dan tujuan) dan masing-masing masuk ke status miliknya.
        // Obat ikut dalam kunci supaya baris yang obatnya tidak sesuai dengan batch tampil terpisah,
        // tidak tercampur diam-diam.
        var balances = balanceRows
            .GroupBy(x => (x.DrugId, x.DrugBatchId, x.Status))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantityOnHand));
        var ledger = mutationRows
            .GroupBy(x => (x.DrugId, x.DrugBatchId, x.Status))
            .ToDictionary(g => g.Key, g => new LedgerTotal(g.Sum(x => x.QuantityChange),
                g.Count(), g.Max(x => x.OccurredAt)));

        var checkedRows = balances.Keys.Union(ledger.Keys)
            .Select(key =>
            {
                var hasBalance = balances.TryGetValue(key, out var onHand);
                var total = ledger.GetValueOrDefault(key);
                var ledgerQuantity = total?.Quantity ?? 0m;

                string finding;
                if (onHand == ledgerQuantity) finding = LedgerConsistencyFindings.Match;
                else if (total == null) finding = LedgerConsistencyFindings.NoMutation;
                else if (!hasBalance) finding = LedgerConsistencyFindings.NoBalance;
                else finding = LedgerConsistencyFindings.SumMismatch;

                return new
                {
                    key.DrugId,
                    key.DrugBatchId,
                    key.Status,
                    OnHand = onHand,
                    LedgerQuantity = ledgerQuantity,
                    MutationCount = total?.Count ?? 0,
                    LastMutationAt = total?.Last,
                    Finding = finding
                };
            })
            .ToList();

        var drugs = await LoadDrugInfoAsync(checkedRows.Select(x => x.DrugId).Distinct().ToList(), cancellationToken);
        var batchIds = checkedRows.Select(x => x.DrugBatchId).Distinct().ToList();
        var batchNumbers = batchIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.PhmDrugBatches.AsNoTracking()
                .Where(x => batchIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.BatchNumber, cancellationToken);

        var items = checkedRows
            .Where(x => !request.OnlyInconsistent || x.Finding != LedgerConsistencyFindings.Match)
            .Select(x =>
            {
                var drug = drugs.GetValueOrDefault(x.DrugId);
                return new LedgerConsistencyRowResponse
                {
                    DrugId = x.DrugId,
                    DrugCode = drug?.Code ?? string.Empty,
                    DrugName = drug?.Name ?? string.Empty,
                    StockUnitName = drug?.StockUnitName,
                    DrugBatchId = x.DrugBatchId,
                    BatchNumber = batchNumbers.GetValueOrDefault(x.DrugBatchId) ?? string.Empty,
                    Status = x.Status,
                    QuantityOnHand = x.OnHand,
                    LedgerQuantity = x.LedgerQuantity,
                    Difference = x.OnHand - x.LedgerQuantity,
                    MutationCount = x.MutationCount,
                    LastMutationAt = x.LastMutationAt,
                    Finding = x.Finding
                };
            })
            // Yang tidak cocok lebih dahulu, lalu menurut obat dan batch.
            .OrderBy(x => x.Finding == LedgerConsistencyFindings.Match)
            .ThenBy(x => x.DrugName)
            .ThenBy(x => x.BatchNumber)
            .ThenBy(x => x.Status)
            .ToList();

        int CountOf(string finding) => checkedRows.Count(x => x.Finding == finding);

        return new LedgerConsistencyResponse
        {
            GeneratedAtUtc = DateTime.UtcNow,
            StorageLocationId = locationId,
            StorageLocationName = locationName,
            Summary = new LedgerConsistencySummaryResponse
            {
                CheckedKeyCount = checkedRows.Count,
                MatchCount = CountOf(LedgerConsistencyFindings.Match),
                SumMismatchCount = CountOf(LedgerConsistencyFindings.SumMismatch),
                NoMutationCount = CountOf(LedgerConsistencyFindings.NoMutation),
                NoBalanceCount = CountOf(LedgerConsistencyFindings.NoBalance),
                MutationCount = mutationRows.Count,
                IsConsistent = checkedRows.All(x => x.Finding == LedgerConsistencyFindings.Match)
            },
            Rows = Page(items, request.PageNumber, request.PageSize),
            Limitations =
            [
                "Pemeriksaan per lokasi: jumlah semua perubahan di buku mutasi per batch × status " +
                "dibandingkan dengan saldo fisik tersimpan. Tidak ada data yang diubah.",
                "Jumlah yang ditahan (reserved) tidak dicatat di buku mutasi dan tidak diperiksa.",
                "Hasil ini adalah bukti untuk keputusan cara menghitung saldo awal periode " +
                "(Mutasi Saldo Persediaan); bukan keputusan itu sendiri."
            ]
        };
    }

    private sealed record LedgerTotal(decimal Quantity, int Count, DateTime Last);

    // ============================================ Mutasi Saldo Persediaan (FIN-LGS-056)

    /// <summary>
    /// Saldo awal, masuk, keluar, penyesuaian, dan saldo akhir per obat × lokasi dengan rincian batch,
    /// menurut keputusan S1–S5. Hanya membaca.
    /// </summary>
    /// <param name="request">Saringan; lokasi wajib.</param>
    /// <param name="verifiedLocations">Lokasi yang buku mutasinya sudah lolos UAT L1–L2.</param>
    /// <param name="cancellationToken">Pembatalan operasi.</param>
    /// <remarks>
    /// <para>
    /// S1: saldo awal dihitung mundur — saldo tersimpan sekarang dikurangi semua mutasi sejak tanggal
    /// mulai — dan hanya untuk obat × batch × status yang buku mutasinya cocok (jumlah semua mutasi =
    /// saldo tersimpan). Saldo akhir = saldo awal + mutasi dalam periode.
    /// </para>
    /// <para>
    /// S2: semua status fisik dijumlah; perubahan status (status asal −x, status tujuan +x) saling
    /// meniadakan di tingkat batch dan obat, sehingga tidak muncul sebagai masuk atau keluar.
    /// S4: bila satu saja batch × status tidak cocok, saldo awal/akhir batch dan obat itu tidak
    /// dihitung (null, "Tidak dapat dihitung"); pergerakannya tetap ditampilkan karena tercatat.
    /// </para>
    /// </remarks>
    public async Task<BalanceMovementResponse> GetBalanceMovementAsync(BalanceMovementQuery request,
        IReadOnlySet<Guid> verifiedLocations, CancellationToken cancellationToken = default)
    {
        var (locationId, locationName) = await RequireLocationAsync(request.StorageLocationId, cancellationToken);
        var period = LogisticsReportPeriod.Resolve(request.StartDate, request.EndDate, DateTime.Now);
        var isVerified = verifiedLocations.Contains(locationId);

        var balanceQuery = db.PhmDrugStockBalances.AsNoTracking()
            .Where(x => !x.IsDelete && x.StorageLocationId == locationId);
        var mutationQuery = db.PhmDrugStockMutations.AsNoTracking()
            .Where(x => !x.IsDelete && x.StorageLocationId == locationId);
        if (request.DrugId.HasValue)
        {
            balanceQuery = balanceQuery.Where(x => x.DrugId == request.DrugId);
            mutationQuery = mutationQuery.Where(x => x.DrugId == request.DrugId);
        }
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            balanceQuery = balanceQuery.Where(x =>
                x.Drug!.DrugName.ToLower().Contains(keyword) || x.Drug.DrugCode.ToLower().Contains(keyword));
            mutationQuery = mutationQuery.Where(x =>
                x.Drug!.DrugName.ToLower().Contains(keyword) || x.Drug.DrugCode.ToLower().Contains(keyword));
        }

        // Disaring di basis data, dijumlah di memori (SQLite tidak dapat menjumlah decimal).
        var balanceRows = await balanceQuery
            .Select(x => new { x.DrugId, x.DrugBatchId, x.Status, x.QuantityOnHand })
            .ToListAsync(cancellationToken);
        var mutationRows = await mutationQuery
            .Select(x => new { x.DrugId, x.DrugBatchId, x.Status, x.MutationType, x.QuantityChange, x.OccurredAt })
            .ToListAsync(cancellationToken);

        var onHandByKey = balanceRows
            .GroupBy(x => (x.DrugId, x.DrugBatchId, x.Status))
            .ToDictionary(g => g.Key, g => g.Sum(x => x.QuantityOnHand));
        var mutationsByKey = mutationRows
            .GroupBy(x => (x.DrugId, x.DrugBatchId, x.Status))
            .ToDictionary(g => g.Key, g => g.ToList());

        var perKey = onHandByKey.Keys.Union(mutationsByKey.Keys)
            .Select(key =>
            {
                var onHand = onHandByKey.GetValueOrDefault(key);
                var rows = mutationsByKey.GetValueOrDefault(key) ?? [];
                var inPeriod = rows
                    .Where(x => x.OccurredAt >= period.FromUtc && x.OccurredAt < period.ToUtcExclusive)
                    .ToList();

                decimal SumOf(DrugStockMutationType type) =>
                    inPeriod.Where(x => x.MutationType == type).Sum(x => x.QuantityChange);

                var opening = onHand - rows.Where(x => x.OccurredAt >= period.FromUtc).Sum(x => x.QuantityChange);
                return new BalanceKey(
                    key.DrugId,
                    key.DrugBatchId,
                    IsConsistent: onHand == rows.Sum(x => x.QuantityChange),
                    Opening: opening,
                    Closing: opening + inPeriod.Sum(x => x.QuantityChange),
                    OpeningRecorded: SumOf(DrugStockMutationType.OpeningBalance),
                    In: SumOf(DrugStockMutationType.StockIn),
                    Out: -SumOf(DrugStockMutationType.StockOut),
                    Adjustment: SumOf(DrugStockMutationType.Adjustment),
                    CurrentOnHand: onHand,
                    MutationCount: inPeriod.Count(x => x.MutationType != DrugStockMutationType.StatusChange),
                    StatusChangeCount: inPeriod.Count(x => x.MutationType == DrugStockMutationType.StatusChange));
            })
            .ToList();

        var drugs = await LoadDrugInfoAsync(perKey.Select(x => x.DrugId).Distinct().ToList(), cancellationToken);
        var batchIds = perKey.Select(x => x.DrugBatchId).Distinct().ToList();
        var batches = batchIds.Count == 0
            ? new Dictionary<Guid, BatchInfo>()
            : await db.PhmDrugBatches.AsNoTracking()
                .Where(x => batchIds.Contains(x.Id))
                .Select(x => new BatchInfo(x.Id, x.BatchNumber, x.ExpiryDate))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        string StatusOf(int inconsistentKeys) =>
            inconsistentKeys > 0 ? BalanceMovementStatuses.NotComputable
            : isVerified ? BalanceMovementStatuses.Valid
            : BalanceMovementStatuses.Unverified;

        var drugRows = perKey
            .GroupBy(x => x.DrugId)
            .Select(drugGroup =>
            {
                var batchRows = drugGroup
                    .GroupBy(x => x.DrugBatchId)
                    .Select(batchGroup =>
                    {
                        var inconsistent = batchGroup.Count(x => !x.IsConsistent);
                        var info = batches.GetValueOrDefault(batchGroup.Key);
                        return new BalanceMovementBatchResponse
                        {
                            DrugBatchId = batchGroup.Key,
                            BatchNumber = info?.Number ?? string.Empty,
                            ExpiryDate = info?.Expiry,
                            OpeningBalance = inconsistent > 0 ? null : batchGroup.Sum(x => x.Opening),
                            OpeningRecorded = batchGroup.Sum(x => x.OpeningRecorded),
                            QuantityIn = batchGroup.Sum(x => x.In),
                            QuantityOut = batchGroup.Sum(x => x.Out),
                            AdjustmentNet = batchGroup.Sum(x => x.Adjustment),
                            ClosingBalance = inconsistent > 0 ? null : batchGroup.Sum(x => x.Closing),
                            CurrentOnHand = batchGroup.Sum(x => x.CurrentOnHand),
                            MutationCount = batchGroup.Sum(x => x.MutationCount),
                            StatusChangeCount = batchGroup.Sum(x => x.StatusChangeCount),
                            InconsistentKeyCount = inconsistent,
                            Status = StatusOf(inconsistent)
                        };
                    })
                    // Batch tanpa saldo, tanpa pergerakan, dan cocok tidak menambah informasi.
                    .Where(x => x.InconsistentKeyCount > 0 || x.MutationCount > 0 || x.StatusChangeCount > 0 ||
                                x.CurrentOnHand != 0 || (x.OpeningBalance ?? 0m) != 0)
                    .OrderBy(x => x.ExpiryDate ?? DateOnly.MaxValue)
                    .ThenBy(x => x.BatchNumber)
                    .ToList();

                var inconsistentKeys = drugGroup.Count(x => !x.IsConsistent);
                var drug = drugs.GetValueOrDefault(drugGroup.Key);
                return new BalanceMovementRowResponse
                {
                    DrugId = drugGroup.Key,
                    DrugCode = drug?.Code ?? string.Empty,
                    DrugName = drug?.Name ?? string.Empty,
                    StockUnitName = drug?.StockUnitName,
                    OpeningBalance = inconsistentKeys > 0 ? null : batchRows.Sum(x => x.OpeningBalance ?? 0m),
                    OpeningRecorded = batchRows.Sum(x => x.OpeningRecorded),
                    QuantityIn = batchRows.Sum(x => x.QuantityIn),
                    QuantityOut = batchRows.Sum(x => x.QuantityOut),
                    AdjustmentNet = batchRows.Sum(x => x.AdjustmentNet),
                    ClosingBalance = inconsistentKeys > 0 ? null : batchRows.Sum(x => x.ClosingBalance ?? 0m),
                    CurrentOnHand = batchRows.Sum(x => x.CurrentOnHand),
                    MutationCount = batchRows.Sum(x => x.MutationCount),
                    StatusChangeCount = batchRows.Sum(x => x.StatusChangeCount),
                    InconsistentKeyCount = inconsistentKeys,
                    Status = StatusOf(inconsistentKeys),
                    Batches = batchRows
                };
            })
            .Where(x => x.Batches.Count > 0)
            .OrderBy(x => x.DrugName)
            .ToList();

        var limitations = new List<string>
        {
            "Saldo awal = saldo tersimpan saat ini − semua mutasi sejak tanggal mulai (S1); saldo akhir = " +
            "saldo awal + mutasi dalam periode. Dihitung hanya untuk batch × status yang buku mutasinya cocok.",
            "Batch atau obat yang buku mutasinya tidak cocok ditandai \"Tidak dapat dihitung\" (S4); " +
            "pergerakannya tetap ditampilkan. Rinciannya ada di pemeriksaan kelengkapan buku mutasi.",
            "Semua status fisik dijumlah; perubahan status tidak dihitung sebagai masuk atau keluar (S2).",
            PerDrugUnitLimitation,
            NoValuationLimitation
        };
        if (!isVerified)
            limitations.Insert(0,
                "Lokasi ini belum lolos UAT buku mutasi (L1–L2). Saldo awal dan akhir berstatus \"belum " +
                "terverifikasi\" dan tidak boleh dipakai sebagai saldo resmi sampai operator mencantumkan " +
                $"lokasi ini di konfigurasi {LogisticsLedgerVerification.ConfigKey}.");

        return new BalanceMovementResponse
        {
            Period = period.ToResponse(),
            StorageLocationId = locationId,
            StorageLocationName = locationName,
            IsLocationVerified = isVerified,
            Summary = new BalanceMovementSummaryResponse
            {
                RowCount = drugRows.Count,
                ValidRowCount = drugRows.Count(x => x.Status == BalanceMovementStatuses.Valid),
                UnverifiedRowCount = drugRows.Count(x => x.Status == BalanceMovementStatuses.Unverified),
                NotComputableRowCount = drugRows.Count(x => x.Status == BalanceMovementStatuses.NotComputable),
                IsLedgerConsistent = perKey.All(x => x.IsConsistent)
            },
            Rows = Page(drugRows, request.PageNumber, request.PageSize),
            Limitations = limitations
        };
    }

    private sealed record BalanceKey(Guid DrugId, Guid DrugBatchId, bool IsConsistent, decimal Opening,
        decimal Closing, decimal OpeningRecorded, decimal In, decimal Out, decimal Adjustment,
        decimal CurrentOnHand, int MutationCount, int StatusChangeCount);

    private sealed record BatchInfo(Guid Id, string Number, DateOnly? Expiry);

    /// <summary>Lokasi wajib dan terdaftar; sama dengan pemeriksaan buku mutasi (<c>PHM036</c>).</summary>
    private async Task<(Guid Id, string Name)> RequireLocationAsync(Guid? storageLocationId,
        CancellationToken cancellationToken)
    {
        if (!storageLocationId.HasValue || storageLocationId.Value == Guid.Empty)
            throw new LogisticsReportUnprocessableException(LedgerLocationRequiredCode,
                "Pilih lokasi penyimpanan yang akan diperiksa.");

        var id = storageLocationId.Value;
        var name = await db.MstDrugStorageLocations.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => x.StorageLocationName)
            .FirstOrDefaultAsync(cancellationToken);
        if (name == null)
            throw new LogisticsReportUnprocessableException(LedgerLocationRequiredCode,
                "Lokasi penyimpanan tidak ditemukan.");

        return (id, name);
    }

    // ======================================== Rekomendasi Pengisian Ulang (FIN-LGS-057)

    /// <summary>Status internal: kebijakan valid tetapi siap pakai masih di atas titik pesan ulang.</summary>
    private const string AboveReorderPointStatus = "AboveReorderPoint";

    /// <summary>
    /// Rekomendasi pengisian ulang menurut keputusan R1–R5. Hanya membaca; tidak membuat permintaan
    /// maupun PO.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Kebijakan yang dibaca: aktif, tidak terhapus, dan sedang berlaku. Usulan hanya dihitung bila
    /// lokasi kebijakan terdaftar dan aktif, titik pesan ulang &gt; 0, maksimum &gt; titik pesan ulang,
    /// dan satuan kebijakan = satuan stok obat (R5); selain itu baris berstatus "kebijakan belum
    /// lengkap" atau "satuan keliru" tanpa angka.
    /// </para>
    /// <para>
    /// R1/R4: usulan = maksimum − siap pakai, dibulatkan ke atas ke bilangan bulat satuan stok, hanya
    /// bila siap pakai ≤ titik pesan ulang. "Siap pakai" = saldo fisik berstatus siap pakai, sama
    /// dengan layar Barang Hampir Habis. R2: tombol permintaan hanya untuk lokasi yang bukan gudang
    /// induk (dipenuhi dari gudang internal).
    /// </para>
    /// </remarks>
    public async Task<ReplenishmentResponse> GetReplenishmentAsync(ReplenishmentQuery request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var policyQuery = db.MstDrugStockPolicies.AsNoTracking()
            .Where(x => !x.IsDelete && x.IsActive &&
                        (x.EffectiveStartDate == null || x.EffectiveStartDate <= now) &&
                        (x.EffectiveEndDate == null || x.EffectiveEndDate >= now));
        if (request.DrugId.HasValue) policyQuery = policyQuery.Where(x => x.DrugId == request.DrugId);
        if (request.StorageLocationId.HasValue)
            policyQuery = policyQuery.Where(x => x.StorageLocationId == request.StorageLocationId);
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var keyword = request.Search.Trim().ToLower();
            policyQuery = policyQuery.Where(x =>
                x.Drug!.DrugName.ToLower().Contains(keyword) || x.Drug.DrugCode.ToLower().Contains(keyword));
        }

        var policies = await policyQuery
            .Select(x => new
            {
                x.Id,
                x.StockPolicyCode,
                x.DrugId,
                x.StorageLocationId,
                PolicyUnitId = x.StockUnitMeasurementId,
                DrugUnitId = x.Drug != null ? x.Drug.StockUnitMeasurementId : null,
                x.ReorderPointQuantity,
                x.MaximumStockQuantity
            })
            .ToListAsync(cancellationToken);

        var locationIds = policies
            .Where(x => x.StorageLocationId.HasValue)
            .Select(x => x.StorageLocationId!.Value)
            .Distinct()
            .ToList();
        var locations = locationIds.Count == 0
            ? new Dictionary<Guid, ReplenishmentLocation>()
            : await db.MstDrugStorageLocations.AsNoTracking()
                .Where(x => locationIds.Contains(x.Id))
                .Select(x => new ReplenishmentLocation(x.Id, x.StorageLocationName, x.IsActive && !x.IsDelete,
                    x.IsMainWarehouse, x.ServiceUnitId))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var serviceUnitIds = locations.Values
            .Where(x => x.ServiceUnitId.HasValue)
            .Select(x => x.ServiceUnitId!.Value)
            .Distinct()
            .ToList();
        var serviceUnitNames = serviceUnitIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.MstServiceUnits.AsNoTracking()
                .Where(x => serviceUnitIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.ServiceUnitName, cancellationToken);

        var drugIds = policies.Select(x => x.DrugId).Distinct().ToList();
        var balanceRows = drugIds.Count == 0 || locationIds.Count == 0
            ? []
            : await db.PhmDrugStockBalances.AsNoTracking()
                .Where(x => !x.IsDelete && x.Status == DrugStockStatus.Available &&
                            drugIds.Contains(x.DrugId) && locationIds.Contains(x.StorageLocationId))
                .Select(x => new { x.DrugId, x.StorageLocationId, x.QuantityOnHand, x.QuantityReserved })
                .ToListAsync(cancellationToken);
        var stock = balanceRows
            .GroupBy(x => (x.DrugId, x.StorageLocationId))
            .ToDictionary(g => g.Key, g => (OnHand: g.Sum(x => x.QuantityOnHand), Reserved: g.Sum(x => x.QuantityReserved)));

        var drugs = await LoadDrugInfoAsync(drugIds, cancellationToken);

        var classified = policies
            .Select(policy =>
            {
                var location = policy.StorageLocationId.HasValue
                    ? locations.GetValueOrDefault(policy.StorageLocationId.Value)
                    : null;

                var reasons = new List<string>();
                if (location == null) reasons.Add(ReplenishmentIncompleteReasons.NoLocation);
                else if (!location.IsUsable) reasons.Add(ReplenishmentIncompleteReasons.LocationInactive);
                if (policy.ReorderPointQuantity <= 0) reasons.Add(ReplenishmentIncompleteReasons.ReorderPointNotSet);
                if (policy.MaximumStockQuantity <= policy.ReorderPointQuantity)
                    reasons.Add(ReplenishmentIncompleteReasons.MaximumNotAboveReorderPoint);

                var unitMismatch = policy.DrugUnitId == null || policy.DrugUnitId != policy.PolicyUnitId;
                var (onHand, reserved) = location != null
                    ? stock.GetValueOrDefault((policy.DrugId, location.Id))
                    : default;

                string status;
                decimal? recommended = null;
                if (reasons.Count > 0) status = ReplenishmentStatuses.PolicyIncomplete;
                else if (unitMismatch) status = ReplenishmentStatuses.UnitMismatch;
                else if (onHand > policy.ReorderPointQuantity) status = AboveReorderPointStatus;
                else
                {
                    status = ReplenishmentStatuses.Recommended;
                    recommended = Math.Ceiling(policy.MaximumStockQuantity - onHand);
                }

                var drug = drugs.GetValueOrDefault(policy.DrugId);
                return new ReplenishmentRowResponse
                {
                    StockPolicyId = policy.Id,
                    StockPolicyCode = policy.StockPolicyCode,
                    DrugId = policy.DrugId,
                    DrugCode = drug?.Code ?? string.Empty,
                    DrugName = drug?.Name ?? string.Empty,
                    StockUnitMeasurementId = policy.DrugUnitId,
                    StockUnitName = drug?.StockUnitName,
                    StorageLocationId = policy.StorageLocationId,
                    StorageLocationName = location?.Name ?? string.Empty,
                    IsMainWarehouse = location?.IsMainWarehouse ?? false,
                    RequestingServiceUnitId = location?.ServiceUnitId,
                    RequestingServiceUnitName = location?.ServiceUnitId is Guid unitId
                        ? serviceUnitNames.GetValueOrDefault(unitId)
                        : null,
                    QuantityAvailable = onHand,
                    QuantityReserved = reserved,
                    ReorderPointQuantity = policy.ReorderPointQuantity,
                    MaximumStockQuantity = policy.MaximumStockQuantity,
                    RecommendedQuantity = recommended,
                    Status = status,
                    IncompleteReasons = reasons,
                    CanCreateRequest = status == ReplenishmentStatuses.Recommended && location is { IsMainWarehouse: false }
                };
            })
            .ToList();

        static int StatusOrder(string status) => status switch
        {
            ReplenishmentStatuses.Recommended => 0,
            ReplenishmentStatuses.PolicyIncomplete => 1,
            _ => 2
        };

        var rows = classified
            .Where(x => x.Status != AboveReorderPointStatus)
            .Where(x => string.IsNullOrWhiteSpace(request.Status) ||
                        string.Equals(x.Status, request.Status.Trim(), StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => StatusOrder(x.Status))
            .ThenBy(x => x.QuantityAvailable - x.ReorderPointQuantity)
            .ThenBy(x => x.DrugName)
            .ThenBy(x => x.StorageLocationName)
            .ToList();

        return new ReplenishmentResponse
        {
            GeneratedAtUtc = DateTime.UtcNow,
            Summary = new ReplenishmentSummaryResponse
            {
                ActivePolicyCount = classified.Count,
                RecommendedCount = classified.Count(x => x.Status == ReplenishmentStatuses.Recommended),
                PolicyIncompleteCount = classified.Count(x => x.Status == ReplenishmentStatuses.PolicyIncomplete),
                UnitMismatchCount = classified.Count(x => x.Status == ReplenishmentStatuses.UnitMismatch),
                AboveReorderPointCount = classified.Count(x => x.Status == AboveReorderPointStatus)
            },
            Rows = Page(rows, request.PageNumber, request.PageSize),
            Limitations =
            [
                "Usulan = maksimum stok − siap pakai (R1), dibulatkan ke atas ke satuan stok obat (R4), " +
                "hanya bila siap pakai ≤ titik pesan ulang. Permintaan yang sedang berjalan tidak dikurangkan.",
                "Tidak ada permintaan barang atau PO yang dibuat otomatis (R3). Tombol \"Buat Permintaan\" " +
                "hanya mengisi formulir; petugas meninjau, menyimpan, dan mengajukannya sendiri.",
                "Tombol hanya tersedia untuk lokasi yang dipenuhi dari gudang internal; gudang induk diisi " +
                "lewat pengadaan (R2).",
                "Kebijakan yang satuannya berbeda dengan satuan stok obat dikecualikan tanpa konversi (R5). " +
                "Kebijakan yang belum lengkap tidak diberi angka usulan."
            ]
        };
    }

    private sealed record ReplenishmentLocation(Guid Id, string Name, bool IsUsable, bool IsMainWarehouse,
        Guid? ServiceUnitId);

    // ================================================================= bantuan bersama

    private sealed record DrugInfo(string Code, string Name, string? StockUnitName);

    private async Task<Dictionary<Guid, DrugInfo>> LoadDrugInfoAsync(List<Guid> drugIds,
        CancellationToken cancellationToken)
    {
        if (drugIds.Count == 0) return new Dictionary<Guid, DrugInfo>();

        var drugs = await db.MstDrugs.AsNoTracking()
            .Where(x => drugIds.Contains(x.Id))
            .Select(x => new { x.Id, x.DrugCode, x.DrugName, x.StockUnitMeasurementId })
            .ToListAsync(cancellationToken);

        var unitIds = drugs
            .Where(x => x.StockUnitMeasurementId.HasValue)
            .Select(x => x.StockUnitMeasurementId!.Value)
            .Distinct()
            .ToList();
        var unitNames = unitIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await db.MstMeasurements.AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.MeasurementName, cancellationToken);

        return drugs.ToDictionary(x => x.Id, x => new DrugInfo(x.DrugCode, x.DrugName,
            x.StockUnitMeasurementId.HasValue ? unitNames.GetValueOrDefault(x.StockUnitMeasurementId.Value) : null));
    }

    private async Task<Dictionary<Guid, string>> LoadLocationNamesAsync(List<Guid> locationIds,
        CancellationToken cancellationToken)
    {
        if (locationIds.Count == 0) return new Dictionary<Guid, string>();

        return await db.MstDrugStorageLocations.AsNoTracking()
            .Where(x => locationIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.StorageLocationName, cancellationToken);
    }

    private static PagedResult<T> Page<T>(IReadOnlyList<T> all, int pageNumber, int pageSize)
    {
        var number = pageNumber < 1 ? 1 : pageNumber;
        var size = pageSize is < 1 or > 200 ? 25 : pageSize;
        return new PagedResult<T>
        {
            PageNumber = number,
            PageSize = size,
            TotalData = all.Count,
            TotalPage = (int)Math.Ceiling(all.Count / (double)size),
            Items = [.. all.Skip((number - 1) * size).Take(size)]
        };
    }

    private static string? DirectionOf(DrugStockMutationType type) => type switch
    {
        DrugStockMutationType.StockIn => StockFlowDirections.In,
        DrugStockMutationType.StockOut => StockFlowDirections.Out,
        DrugStockMutationType.Adjustment => StockFlowDirections.Adjustment,
        DrugStockMutationType.OpeningBalance => StockFlowDirections.Opening,
        _ => null
    };

    private static int DirectionOrder(string direction) => direction switch
    {
        StockFlowDirections.In => 0,
        StockFlowDirections.Out => 1,
        StockFlowDirections.Adjustment => 2,
        _ => 3
    };

    /// <summary>Waktu tersimpan (UTC) ke tanggal lokal server.</summary>
    private static DateOnly ToLocalDate(DateTime storedUtc) =>
        DateOnly.FromDateTime(DateTime.SpecifyKind(storedUtc, DateTimeKind.Utc).ToLocalTime());
}
