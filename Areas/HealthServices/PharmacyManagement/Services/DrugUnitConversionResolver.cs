using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

/// <summary>
/// Menerjemahkan jumlah dalam satuan pemakaian menjadi jumlah dalam satuan stok obat.
/// </summary>
/// <remarks>
/// <para>
/// Satuan adalah pengetahuan Farmasi, bukan pengetahuan modul yang memakai obatnya. Kelas ini
/// menjadi satu-satunya tempat pertanyaan "berapa ini dalam satuan stok?" dijawab, sehingga
/// modul lain tidak perlu — dan tidak boleh — menyalin tabel konversinya.
/// </para>
/// <para>
/// Satuan yang tidak dikenal ditolak, bukan dianggap sama dengan satuan stok. Satu botol yang
/// diperlakukan sebagai satu tablet akan mengurangi stok seratus kali lebih sedikit daripada
/// yang sebenarnya terpakai, dan selisih itu baru terlihat saat stok opname.
/// </para>
/// </remarks>
public sealed class DrugUnitConversionResolver
{
    private readonly ApplicationDbContext _dbContext;

    public DrugUnitConversionResolver(ApplicationDbContext dbContext) => _dbContext = dbContext;

    /// <summary>
    /// Mengubah jumlah dalam satuan yang dicatat menjadi jumlah dalam satuan stok obat.
    /// </summary>
    /// <remarks>
    /// Satuan yang sama dengan satuan stok dipakai apa adanya. Satuan lain harus memiliki
    /// konversi yang berlaku pada master obat; bila tidak ada, jumlahnya tidak dibukukan.
    /// </remarks>
    public async Task<decimal> ToStockQuantityAsync(Guid drugId, Guid? measurementId, decimal quantity,
        CancellationToken cancellationToken = default)
    {
        var factor = await ResolveFactorAsync(drugId, measurementId, cancellationToken);
        return quantity * factor;
    }

    /// <summary>
    /// Memastikan satuan dapat diterjemahkan ke satuan stok, tanpa mengubah apa pun.
    /// </summary>
    /// <remarks>
    /// Dipakai saat pencatatan supaya kesalahan satuan ketahuan di depan petugas yang masih
    /// dapat memperbaikinya, bukan nanti ketika pembukuan berjalan tanpa siapa pun menunggu.
    /// </remarks>
    public Task EnsureConvertibleAsync(Guid drugId, Guid? measurementId,
        CancellationToken cancellationToken = default) =>
        ResolveFactorAsync(drugId, measurementId, cancellationToken);

    private async Task<decimal> ResolveFactorAsync(Guid drugId, Guid? measurementId,
        CancellationToken cancellationToken)
    {
        if (!measurementId.HasValue || measurementId.Value == Guid.Empty)
            throw new DrugStockUnprocessableException("PHM090",
                "Satuan pemakaian tidak dicatat, sehingga jumlahnya tidak dapat dibukukan ke stok.");

        var stockUnitId = await _dbContext.Set<MstDrug>().AsNoTracking()
            .Where(x => x.Id == drugId && !x.IsDelete)
            .Select(x => x.StockUnitMeasurementId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new DrugStockUnprocessableException("PHM091",
                "Obat ini belum memiliki satuan stok pada master farmasi.");

        if (stockUnitId == measurementId.Value) return 1m;

        var now = DateTime.UtcNow;

        var conversions = await _dbContext.MstDrugUnitConversions.AsNoTracking()
            .Where(x => x.DrugId == drugId && x.IsActive && !x.IsDelete &&
                (x.EffectiveStartDate == null || x.EffectiveStartDate <= now) &&
                (x.EffectiveEndDate == null || x.EffectiveEndDate >= now) &&
                ((x.FromMeasurementId == measurementId.Value && x.ToMeasurementId == stockUnitId) ||
                 (x.IsBidirectional && x.FromMeasurementId == stockUnitId &&
                  x.ToMeasurementId == measurementId.Value)))
            // Konversi yang ditandai default dipakai lebih dahulu bila ada beberapa yang cocok.
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.SortOrder)
            .Select(x => new
            {
                x.FromMeasurementId, x.ToMeasurementId, x.FromQuantity, x.ToQuantity
            })
            .ToListAsync(cancellationToken);

        var conversion = conversions.FirstOrDefault()
            ?? throw new DrugStockUnprocessableException("PHM092",
                "Satuan yang dipakai tidak dikenal untuk obat ini dan tidak memiliki konversi " +
                "ke satuan stok. Lengkapi konversi satuannya di master obat.");

        // Arah maju: `FromQuantity` satuan pemakaian setara `ToQuantity` satuan stok.
        // Arah balik dipakai hanya bila konversinya memang dinyatakan dua arah.
        var (numerator, denominator) = conversion.FromMeasurementId == measurementId.Value
            ? (conversion.ToQuantity, conversion.FromQuantity)
            : (conversion.FromQuantity, conversion.ToQuantity);

        if (denominator <= 0 || numerator <= 0)
            throw new DrugStockUnprocessableException("PHM093",
                "Konversi satuan obat ini tidak sah karena memuat jumlah nol atau negatif.");

        return numerator / denominator;
    }
}
