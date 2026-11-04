using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Seeders;

/// <summary>
/// Mengisi saldo pembuka depo Farmasi Rawat Jalan (<c>GDG-023</c>) dari salinan saldo
/// <c>QuilvianNewDevSukma</c> per 28 Oktober 2026 (<c>RJ-DOC-DEC-091</c>).
/// </summary>
/// <remarks>
/// <para>
/// <b>Mengapa salinan, bukan rumus.</b> Owner meminta database lain memiliki stok yang sama
/// persis dengan database pengembangannya, termasuk perubahan uji yang sengaja dibuat (stok nol,
/// barang rusak, karantina). Karena itu isinya berkas CSV tertanam, dikunci dengan kode obat —
/// bukan id — supaya tetap cocok walaupun id obat di database tujuan berbeda.
/// </para>
/// <para>
/// <b>Seeder tidak pernah menimpa.</b> Saldo yang sudah ada pada kombinasi batch, lokasi, dan
/// status yang sama dilewati apa adanya, sehingga stok yang sudah bergerak tidak pernah
/// dikembalikan ke angka salinan. Setiap saldo baru yang jumlahnya lebih dari nol disertai satu
/// baris kartu stok <see cref="DrugStockMutationType.OpeningBalance"/>, sehingga total kartu stok
/// selalu sama dengan saldo.
/// </para>
/// <para>
/// <b>Bukan untuk produksi.</b> Stok produksi lahir dari penerimaan barang resmi, bukan dari
/// salinan database pengembangan. Seeder menolak berjalan di lingkungan <c>Production</c>.
/// </para>
/// </remarks>
public static class PharmacyOpeningStockSeeder
{
    public const string ProductionEnvironmentName = "Production";
    public const string StorageLocationCode = "GDG-023";
    public const string OutpatientServiceUnitCode = "SU-OPD-001";

    private const string ResourceSuffix = "pharmacy-opening-stock-frj.csv";
    private const string MutationSource = "SEED:PharmacyOpeningStockSeeder";
    private const string MutationReason =
        "Saldo pembuka Farmasi Rawat Jalan (salinan QuilvianNewDevSukma 28 Okt 2026).";
    private const string BatchNotes = "Saldo pembuka Farmasi Rawat Jalan.";

    public sealed record StockRow(
        string DrugCode,
        string BatchNumber,
        DateOnly? ExpiryDate,
        DrugStockStatus Status,
        decimal Quantity);

    /// <summary>Membaca salinan saldo yang tertanam di assembly.</summary>
    public static IReadOnlyList<StockRow> ReadEmbeddedRows()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(x => x.EndsWith(ResourceSuffix, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException(
                "Berkas salinan stok Farmasi Rawat Jalan tidak tertanam. Periksa EmbeddedResource pada csproj.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException("Berkas salinan stok Farmasi Rawat Jalan tidak dapat dibuka.");
        using var reader = new StreamReader(stream, Encoding.UTF8);

        var rows = new List<StockRow>();
        var lineNumber = 0;

        while (reader.ReadLine() is { } line)
        {
            lineNumber++;
            if (lineNumber == 1 || string.IsNullOrWhiteSpace(line)) continue;

            // Kolom: DrugCode, BatchNumber, ExpiryDate, Status, QuantityOnHand. Isinya tidak pernah
            // memuat koma maupun tanda kutip, jadi pemisahan sederhana sudah cukup.
            var column = line.Split(',');
            if (column.Length < 5)
                throw new InvalidOperationException(
                    $"Baris {lineNumber} salinan stok Farmasi Rawat Jalan kolomnya kurang.");

            rows.Add(new StockRow(
                column[0].Trim().ToUpperInvariant(),
                column[1].Trim(),
                string.IsNullOrWhiteSpace(column[2])
                    ? null
                    : DateOnly.ParseExact(column[2].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                (DrugStockStatus)int.Parse(column[3].Trim(), CultureInfo.InvariantCulture),
                decimal.Parse(column[4].Trim(), CultureInfo.InvariantCulture)));
        }

        return rows;
    }

    /// <summary>
    /// Menerapkan salinan saldo ke <c>GDG-023</c>. Baris yang saldonya sudah ada dilewati.
    /// </summary>
    /// <remarks>
    /// Parameter <c>rows</c> bernilai <c>null</c> memakai salinan tertanam; uji memberi baris sendiri.
    /// </remarks>
    public static async Task<PharmacyOpeningStockSeedResult> SeedAsync(
        ApplicationDbContext db,
        string? environmentName,
        Guid actorUserId,
        IReadOnlyList<StockRow>? rows = null,
        CancellationToken ct = default)
    {
        var result = new PharmacyOpeningStockSeedResult();

        if (string.Equals(environmentName?.Trim(), ProductionEnvironmentName, StringComparison.OrdinalIgnoreCase))
        {
            result.RefusedReason =
                "Seeder stok Farmasi Rawat Jalan menolak berjalan di lingkungan produksi: stok produksi " +
                "lahir dari penerimaan barang, bukan salinan database pengembangan.";
            return result;
        }

        var locationId = DrugStorageLocationSeeder.DeterministicId(StorageLocationCode);
        var location = await db.MstDrugStorageLocations
            .FirstOrDefaultAsync(x => x.Id == locationId && !x.IsDelete, ct);

        if (location == null)
        {
            result.RefusedReason =
                $"Lokasi {StorageLocationCode} belum ada. Jalankan DrugStorageLocationSeeder lebih dahulu.";
            return result;
        }

        rows ??= ReadEmbeddedRows();
        var now = DateTime.UtcNow;

        // Tautan depo Rawat Jalan (RJ-DOC-DEC-085): hanya mengisi yang kosong, tautan pilihan admin dibiarkan.
        if (location.ServiceUnitId == null)
        {
            var serviceUnitId = await db.MstServiceUnits.AsNoTracking()
                .Where(x => x.ServiceUnitCode == OutpatientServiceUnitCode && !x.IsDelete)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(ct);

            if (serviceUnitId.HasValue)
            {
                location.ServiceUnitId = serviceUnitId;
                location.UpdateDateTime = now;
                location.UpdateBy = actorUserId;
                result.LinkedServiceUnit = true;
            }
        }

        var codes = rows.Select(x => x.DrugCode).Distinct().ToList();
        var drugs = await db.MstDrugs.AsNoTracking()
            .Where(x => codes.Contains(x.DrugCode.ToUpper()) && !x.IsDelete)
            .Select(x => new { Code = x.DrugCode.ToUpper(), x.Id })
            .ToListAsync(ct);
        var drugIdByCode = drugs
            .GroupBy(x => x.Code)
            .ToDictionary(g => g.Key, g => g.First().Id);

        var drugIds = drugIdByCode.Values.ToList();
        var batches = await db.PhmDrugBatches
            .Where(x => drugIds.Contains(x.DrugId) && !x.IsDelete)
            .ToListAsync(ct);
        var batchByKey = batches.ToDictionary(x => (x.DrugId, x.BatchNumber));

        var existingBalances = await db.PhmDrugStockBalances.AsNoTracking()
            .Where(x => x.StorageLocationId == locationId && !x.IsDelete)
            .Select(x => new { x.DrugBatchId, x.Status })
            .ToListAsync(ct);
        var balanceKeys = existingBalances.Select(x => (x.DrugBatchId, x.Status)).ToHashSet();

        foreach (var row in rows)
        {
            if (!drugIdByCode.TryGetValue(row.DrugCode, out var drugId))
            {
                result.UnknownDrugCodes.Add(row.DrugCode);
                continue;
            }

            if (!batchByKey.TryGetValue((drugId, row.BatchNumber), out var batch))
            {
                batch = new PhmDrugBatch
                {
                    DrugId = drugId,
                    BatchNumber = row.BatchNumber,
                    ExpiryDate = row.ExpiryDate,
                    Notes = BatchNotes,
                    CreateDateTime = now,
                    CreateBy = actorUserId
                };
                db.PhmDrugBatches.Add(batch);
                batchByKey[(drugId, row.BatchNumber)] = batch;
                result.BatchesCreated++;
            }
            else if (batch.ExpiryDate != row.ExpiryDate)
            {
                // Aturan yang sama dengan DrugStockService (PHM032): satu nomor batch hanya boleh
                // punya satu tanggal kedaluwarsa. Barisnya dilewati, bukan menimpa batch yang ada.
                result.ExpiryConflicts.Add($"{row.DrugCode}/{row.BatchNumber}");
                continue;
            }

            if (!balanceKeys.Add((batch.Id, row.Status)))
            {
                result.Skipped++;
                continue;
            }

            db.PhmDrugStockBalances.Add(new PhmDrugStockBalance
            {
                DrugId = drugId,
                DrugBatchId = batch.Id,
                StorageLocationId = locationId,
                Status = row.Status,
                QuantityOnHand = row.Quantity,
                QuantityReserved = 0,
                Version = row.Quantity > 0 ? 1 : 0,
                CreateDateTime = now,
                CreateBy = actorUserId
            });
            result.BalancesCreated++;

            if (row.Quantity <= 0) continue;

            db.PhmDrugStockMutations.Add(new PhmDrugStockMutation
            {
                DrugId = drugId,
                DrugBatchId = batch.Id,
                StorageLocationId = locationId,
                Status = row.Status,
                MutationType = DrugStockMutationType.OpeningBalance,
                QuantityChange = row.Quantity,
                BalanceBefore = 0,
                BalanceAfter = row.Quantity,
                Reason = MutationReason,
                SourceDocumentType = DrugStockSourceDocumentTypes.ManualAdjustment,
                ActorUserId = actorUserId,
                OccurredAt = now,
                Source = MutationSource,
                CorrelationId = $"SEED-FRJ:{row.DrugCode}:{row.BatchNumber}:{(int)row.Status}",
                CreateDateTime = now,
                CreateBy = actorUserId
            });
            result.MutationsCreated++;
        }

        if (result.BalancesCreated == 0 && !result.LinkedServiceUnit)
            return result;

        // Satu transaksi untuk seluruh salinan: gagal di tengah berarti tidak ada yang tersimpan.
        // Bila pemanggil sudah membuka transaksi (uji yang dibatalkan), transaksinya dipakai bersama.
        if (db.Database.CurrentTransaction != null)
        {
            await db.SaveChangesAsync(ct);
            return result;
        }

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return result;
    }
}

public class PharmacyOpeningStockSeedResult
{
    public int BalancesCreated { get; set; }

    public int BatchesCreated { get; set; }

    public int MutationsCreated { get; set; }

    public int Skipped { get; set; }

    public bool LinkedServiceUnit { get; set; }

    public List<string> UnknownDrugCodes { get; } = [];

    public List<string> ExpiryConflicts { get; } = [];

    public string? RefusedReason { get; set; }
}
