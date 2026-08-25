using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Membuat <see cref="ApplicationDbContext"/> yang berdiri sendiri di memori untuk setiap
/// test, mengikuti pola <c>IsolatedBillingDbContextFactory</c> yang sudah ada.
/// </summary>
/// <remarks>
/// Provider InMemory TIDAK menegakkan index unik maupun foreign key. Karena itu test di
/// bawah folder ini hanya membuktikan aturan yang memang dijalankan kode — misalnya
/// idempotensi seeder dan penolakan kode kembar oleh service. Pembuktian bahwa database
/// sendiri menolak baris kembar dijalankan sebagai SQL terhadap PostgreSQL sungguhan, dan
/// tercatat pada laporan task yang bersangkutan.
///
/// <para>
/// <b>Kenapa peringatan transaksi diabaikan.</b> Provider InMemory tidak punya transaksi.
/// Tanpa baris <c>ConfigureWarnings</c> di bawah, setiap pemanggilan
/// <c>BeginTransactionAsync</c> melempar galat, sehingga service yang memang wajib
/// bertransaksi — seperti pembukaan dan pembatalan admisi — tidak dapat diuji sama sekali di
/// sini. Yang dibuktikan test InMemory karena itu adalah bahwa seluruh perubahan masuk ke
/// SATU <c>SaveChangesAsync</c>, sehingga kegagalan menyisakan nol baris. Bahwa PostgreSQL
/// benar-benar mengembalikan perubahan saat transaksi digagalkan adalah pembuktian terpisah
/// terhadap database sungguhan, dan dicatat pada laporan task.
/// </para>
/// </remarks>
internal static class IsolatedInpatientDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        return new ApplicationDbContext(BuildOptions(databaseName));
    }

    /// <summary>
    /// Membuat context yang selalu gagal saat menyimpan, untuk membuktikan bahwa kegagalan di
    /// tengah proses tidak menyisakan baris apa pun.
    /// </summary>
    public static FailingSaveApplicationDbContext CreateFailingSave(string databaseName)
    {
        return new FailingSaveApplicationDbContext(BuildOptions(databaseName));
    }

    public static DbContextOptions<ApplicationDbContext> BuildOptions(string? databaseName = null)
    {
        return new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"inpatient-tests-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
    }
}

/// <summary>
/// <see cref="ApplicationDbContext"/> yang menolak menyimpan. Dipakai untuk membuktikan
/// bahwa episode dan baris riwayat statusnya sama-sama tidak tersimpan ketika penyimpanan
/// gagal — keduanya berada di dalam satu penyimpanan, bukan dua.
/// </summary>
internal sealed class FailingSaveApplicationDbContext : ApplicationDbContext
{
    public FailingSaveApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public int SaveAttempts { get; private set; }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveAttempts++;

        throw new InvalidOperationException(
            "Penyimpanan sengaja digagalkan oleh test.");
    }
}
