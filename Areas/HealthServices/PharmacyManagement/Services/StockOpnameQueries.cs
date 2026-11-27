using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Aturan perbandingan hasil hitung fisik dengan saldo sistem (FIN-LGS-084). Fungsi murni, supaya
/// aturan yang sama dipakai lagi tanpa perubahan ketika sesi Stock Opname sudah dapat disimpan.
/// </summary>
public static class StockOpnameCalculator
{
    /// <summary>Kolom saldo bertipe <c>numeric(18,3)</c>.</summary>
    public const int MaxDecimals = 3;

    /// <summary>Batas jumlah baris satu lembar hitung.</summary>
    public const int MaxLines = 2000;

    /// <summary>Batas atas satu hasil hitung, sama dengan batas jumlah perintah stok lainnya.</summary>
    public const decimal MaxQuantity = 100_000_000m;

    /// <summary>
    /// Toleransi selisih (keputusan pemilik FIN-LGS-088, prinsip 4): 0. Setiap selisih, sekecil apa pun,
    /// berstatus Lebih/Kurang dan wajib melalui persetujuan; tidak ada selisih yang dianggap cocok.
    /// </summary>
    public const decimal VarianceTolerance = 0m;

    public static bool HasValidScale(decimal value) => decimal.Round(value, MaxDecimals) == value;

    /// <summary>
    /// Membandingkan satu baris. Gudang tidak dibekukan selama opname (prinsip 1), jadi baris yang
    /// saldonya berubah atau bergerak sejak lembar hitung dimuat tidak pernah diberi selisih: hasil
    /// hitungnya diambil pada saldo yang sudah tidak berlaku dan wajib dihitung ulang sebelum keputusan.
    /// </summary>
    /// <param name="currentQuantity">Saldo fisik tersimpan saat ini.</param>
    /// <param name="snapshotQuantity">Saldo yang tercetak di lembar hitung.</param>
    /// <param name="movedSinceSnapshot">Ada mutasi pada batch × status ini sesudah lembar hitung dimuat.</param>
    /// <param name="physicalQuantity">Hasil hitung fisik; null berarti belum dihitung.</param>
    public static StockOpnameLineEvaluation Evaluate(decimal currentQuantity, decimal snapshotQuantity,
        bool movedSinceSnapshot, decimal? physicalQuantity)
    {
        var changed = movedSinceSnapshot || snapshotQuantity != currentQuantity;

        if (physicalQuantity is null)
            return new StockOpnameLineEvaluation(null, StockOpnameVarianceResults.NotCounted, changed);

        if (changed)
            return new StockOpnameLineEvaluation(null, StockOpnameVarianceResults.NeedsRecount, true);

        var variance = physicalQuantity.Value - currentQuantity;
        var result = Math.Abs(variance) <= VarianceTolerance
            ? StockOpnameVarianceResults.Match
            : variance > 0m
                ? StockOpnameVarianceResults.Surplus
                : StockOpnameVarianceResults.Shortage;

        return new StockOpnameLineEvaluation(variance, result, false);
    }

    /// <summary>Ringkasan jumlah baris per hasil.</summary>
    public static StockOpnameVarianceSummaryResponse Summarize(IReadOnlyCollection<StockOpnameVarianceLineResponse> lines) =>
        SummarizeResults(lines.Select(x => x.Result).ToList());

    /// <summary>Ringkasan dari kode hasil per baris; dipakai pratinjau dan sesi tersimpan (FIN-LGS-091).</summary>
    public static StockOpnameVarianceSummaryResponse SummarizeResults(IReadOnlyCollection<string> results)
    {
        var notCounted = results.Count(x => x == StockOpnameVarianceResults.NotCounted);
        var needsRecount = results.Count(x => x == StockOpnameVarianceResults.NeedsRecount);

        return new StockOpnameVarianceSummaryResponse
        {
            LineCount = results.Count,
            CountedLineCount = results.Count - notCounted,
            NotCountedLineCount = notCounted,
            MatchCount = results.Count(x => x == StockOpnameVarianceResults.Match),
            SurplusCount = results.Count(x => x == StockOpnameVarianceResults.Surplus),
            ShortageCount = results.Count(x => x == StockOpnameVarianceResults.Shortage),
            NeedsRecountCount = needsRecount,
            IsComplete = results.Count > 0 && notCounted == 0 && needsRecount == 0
        };
    }

    /// <summary>Hasil dari selisih yang sudah ditetapkan (sesi diajukan atau selesai).</summary>
    public static string ResultFromVariance(decimal variance) =>
        Math.Abs(variance) <= VarianceTolerance
            ? StockOpnameVarianceResults.Match
            : variance > 0m
                ? StockOpnameVarianceResults.Surplus
                : StockOpnameVarianceResults.Shortage;
}

/// <summary>Hasil perbandingan satu baris.</summary>
public sealed record StockOpnameLineEvaluation(decimal? Variance, string Result, bool ChangedSinceSnapshot);

/// <summary>
/// Hitung selisih Stock Opname (FIN-LGS-084), hanya membaca.
/// </summary>
/// <remarks>
/// <para>
/// Saldo sistem dibaca server, bukan dipercaya dari klien; nilai lembar hitung dari klien hanya
/// dipakai untuk mendeteksi perubahan sejak lembar itu dimuat. Tidak ada sesi, hasil hitung,
/// keputusan, penyesuaian, atau berita acara yang disimpan: tabel Stock Opname belum ada, dan
/// menyimpannya di tempat lain akan menjadi penyimpanan transaksi tiruan.
/// </para>
/// <para>
/// Penyesuaian hasil opname kelak memakai jalur penyesuaian yang sudah ada
/// (<c>DrugStockService.AdjustAsync</c>: versi saldo, kunci idempotensi, alasan wajib, mutasi
/// tercatat di kartu stok) dengan sumber <c>DrugStockSourceDocumentTypes.StockOpname</c>.
/// </para>
/// </remarks>
public sealed class StockOpnameQueries(ApplicationDbContext dbContext)
{
    public async Task<StockOpnameVariancePreviewResponse> PreviewVarianceAsync(
        StockOpnameVariancePreviewRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.StorageLocationId.HasValue || request.StorageLocationId.Value == Guid.Empty)
            throw new LogisticsReportUnprocessableException("PHM036", "Lokasi penyimpanan wajib dipilih.");

        var locationId = request.StorageLocationId.Value;
        var location = await dbContext.MstDrugStorageLocations.AsNoTracking()
            .Where(x => x.Id == locationId && !x.IsDelete)
            .Select(x => new { x.Id, x.StorageLocationName })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new LogisticsReportUnprocessableException("PHM036", "Lokasi penyimpanan tidak terdaftar.");

        var counts = request.Counts ?? [];
        ValidateCounts(counts);

        var ids = counts.Select(x => x.BalanceId!.Value).ToList();
        var balances = await dbContext.PhmDrugStockBalances.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && !x.IsDelete)
            .Select(x => new
            {
                x.Id,
                x.DrugId,
                x.DrugBatchId,
                x.StorageLocationId,
                x.Status,
                x.QuantityOnHand
            })
            .ToListAsync(cancellationToken);

        // Lembar hitung harus utuh untuk lokasi itu: baris yang saldonya sudah tidak ada atau milik
        // lokasi lain berarti lembarnya usang, bukan sesuatu yang boleh dilewati diam-diam.
        var foundIds = balances.Where(x => x.StorageLocationId == locationId).Select(x => x.Id).ToHashSet();
        var unusable = ids.Count(id => !foundIds.Contains(id));
        if (unusable > 0)
            throw new LogisticsReportUnprocessableException("PHM038",
                $"{unusable} baris lembar hitung tidak lagi menunjuk saldo pada lokasi {location.StorageLocationName}. " +
                "Muat ulang lembar hitung.");

        // Nama dan satuan dimuat terpisah, bukan lewat navigasi, mengikuti laporan Logistik lain:
        // saldo tetap terbaca walaupun data induknya tidak lengkap.
        var drugIds = balances.Select(x => x.DrugId).Distinct().ToList();
        var drugs = await dbContext.MstDrugs.AsNoTracking()
            .Where(x => drugIds.Contains(x.Id))
            .Select(x => new { x.Id, x.DrugCode, x.DrugName, x.StockUnitMeasurementId })
            .ToDictionaryAsync(x => x.Id, cancellationToken);
        var unitIds = drugs.Values
            .Where(x => x.StockUnitMeasurementId.HasValue)
            .Select(x => x.StockUnitMeasurementId!.Value)
            .Distinct()
            .ToList();
        var unitNames = unitIds.Count == 0
            ? new Dictionary<Guid, string>()
            : await dbContext.MstMeasurements.AsNoTracking()
                .Where(x => unitIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.MeasurementName, cancellationToken);
        var batchIds = balances.Select(x => x.DrugBatchId).Distinct().ToList();
        var batches = await dbContext.PhmDrugBatches.AsNoTracking()
            .Where(x => batchIds.Contains(x.Id))
            .Select(x => new { x.Id, x.BatchNumber, x.ExpiryDate })
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var movedKeys = new HashSet<(Guid DrugBatchId, DrugStockStatus Status)>();
        if (request.SnapshotTakenAtUtc.HasValue)
        {
            var since = AsUtc(request.SnapshotTakenAtUtc.Value);
            var moved = await dbContext.PhmDrugStockMutations.AsNoTracking()
                .Where(x => !x.IsDelete &&
                            x.StorageLocationId == locationId &&
                            batchIds.Contains(x.DrugBatchId) &&
                            x.OccurredAt > since)
                .Select(x => new { x.DrugBatchId, x.Status })
                .Distinct()
                .ToListAsync(cancellationToken);
            foreach (var key in moved) movedKeys.Add((key.DrugBatchId, key.Status));
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var countById = counts.ToDictionary(x => x.BalanceId!.Value);

        var lines = balances
            .Select(balance =>
            {
                var input = countById[balance.Id];
                var evaluation = StockOpnameCalculator.Evaluate(balance.QuantityOnHand,
                    input.SnapshotQuantity!.Value, movedKeys.Contains((balance.DrugBatchId, balance.Status)),
                    input.PhysicalQuantity);
                var drug = drugs.GetValueOrDefault(balance.DrugId);
                var batch = batches.GetValueOrDefault(balance.DrugBatchId);
                var expiryDate = batch?.ExpiryDate;

                return new StockOpnameVarianceLineResponse
                {
                    BalanceId = balance.Id,
                    DrugId = balance.DrugId,
                    DrugCode = drug?.DrugCode ?? string.Empty,
                    DrugName = drug?.DrugName ?? string.Empty,
                    StockUnitName = drug?.StockUnitMeasurementId is { } unitId
                        ? unitNames.GetValueOrDefault(unitId)
                        : null,
                    DrugBatchId = balance.DrugBatchId,
                    BatchNumber = batch?.BatchNumber ?? string.Empty,
                    ExpiryDate = expiryDate,
                    IsExpired = expiryDate.HasValue && expiryDate.Value < today,
                    Status = balance.Status,
                    SnapshotQuantity = input.SnapshotQuantity.Value,
                    CurrentQuantity = balance.QuantityOnHand,
                    ChangedSinceSnapshot = evaluation.ChangedSinceSnapshot,
                    PhysicalQuantity = input.PhysicalQuantity,
                    Variance = evaluation.Variance,
                    Result = evaluation.Result,
                    Note = string.IsNullOrWhiteSpace(input.Note) ? null : input.Note.Trim()
                };
            })
            .OrderBy(x => x.DrugName)
            .ThenBy(x => x.ExpiryDate == null)
            .ThenBy(x => x.ExpiryDate)
            .ThenBy(x => x.Status)
            .ToList();

        return new StockOpnameVariancePreviewResponse
        {
            StorageLocationId = location.Id,
            StorageLocationName = location.StorageLocationName,
            CalculatedAtUtc = DateTime.UtcNow,
            IsPersisted = false,
            Summary = StockOpnameCalculator.Summarize(lines),
            Lines = lines,
            Limitations =
            [
                "Hasil hitung selisih ini tidak disimpan. Sesi, hasil hitung, persetujuan, penyesuaian, dan berita acara " +
                "menunggu tabel Stock Opname.",
                "Saldo tidak berubah. Penyesuaian hasil opname hanya dapat diajukan dan disetujui sesudah sesi dapat disimpan.",
                "Barang atau batch yang ditemukan tetapi tidak punya saldo di lokasi ini akan dicatat sebagai temuan " +
                "sesudah sesi dapat disimpan; temuan tidak menambah stok secara otomatis.",
                request.SnapshotTakenAtUtc.HasValue
                    ? "Perubahan sejak lembar hitung dideteksi dari jumlah saldo dan mutasi sesudah waktu lembar hitung dimuat menurut jam perangkat."
                    : "Waktu lembar hitung tidak dikirim, jadi perubahan hanya dideteksi dari jumlah saldo."
            ]
        };
    }

    /// <summary>Waktu tanpa zona dibaca sebagai UTC (namanya <c>...Utc</c>), bukan sebagai jam lokal server.</summary>
    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    /// <summary>Isian lembar hitung yang tidak sah ditolak utuh (422 <c>PHM037</c>) dengan nomor barisnya.</summary>
    private static void ValidateCounts(IReadOnlyList<StockOpnameCountInput> counts)
    {
        if (counts.Count == 0)
            throw new LogisticsReportUnprocessableException("PHM037", "Lembar hitung kosong.");

        if (counts.Count > StockOpnameCalculator.MaxLines)
            throw new LogisticsReportUnprocessableException("PHM037",
                $"Lembar hitung paling banyak {StockOpnameCalculator.MaxLines} baris.");

        var seen = new HashSet<Guid>();
        for (var index = 0; index < counts.Count; index++)
        {
            var count = counts[index];
            var row = index + 1;

            if (count?.BalanceId is not { } balanceId || balanceId == Guid.Empty)
                throw new LogisticsReportUnprocessableException("PHM037", $"Baris {row}: saldo yang dihitung tidak dikenali.");

            if (!seen.Add(balanceId))
                throw new LogisticsReportUnprocessableException("PHM037",
                    $"Baris {row}: saldo yang sama dihitung lebih dari sekali.");

            if (count.SnapshotQuantity is null)
                throw new LogisticsReportUnprocessableException("PHM037",
                    $"Baris {row}: saldo sistem pada lembar hitung wajib dikirim.");

            if (count.PhysicalQuantity is not { } physical) continue;

            if (physical < 0m)
                throw new LogisticsReportUnprocessableException("PHM037", $"Baris {row}: stok fisik tidak boleh negatif.");

            if (physical > StockOpnameCalculator.MaxQuantity)
                throw new LogisticsReportUnprocessableException("PHM037", $"Baris {row}: stok fisik melebihi batas.");

            if (!StockOpnameCalculator.HasValidScale(physical))
                throw new LogisticsReportUnprocessableException("PHM037",
                    $"Baris {row}: stok fisik paling banyak {StockOpnameCalculator.MaxDecimals} angka desimal.");
        }
    }
}
