using Microsoft.EntityFrameworkCore;
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
/// </remarks>
internal static class IsolatedInpatientDbContextFactory
{
    public static ApplicationDbContext Create(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName ?? $"inpatient-tests-{Guid.NewGuid():N}")
            .Options;

        return new ApplicationDbContext(options);
    }
}
