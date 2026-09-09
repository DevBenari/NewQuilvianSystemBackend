using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.Platform;

/// <summary>
/// Penjaga komposisi dependency untuk <c>PLT-BE-003</c>.
/// </summary>
/// <remarks>
/// <para>
/// <b>Kenapa berkas ini ada.</b> <c>NumberSeriesAllocator</c> menuntut koneksi dan transaksinya
/// sendiri agar pencacah bertahan ketika transaksi bisnis pemanggil dibatalkan
/// (<c>DEC-PLT-008</c>). Cara mendapatkannya adalah <c>IDbContextFactory&lt;ApplicationDbContext&gt;</c>,
/// yang harus hidup berdampingan dengan <c>AddDbContext&lt;ApplicationDbContext&gt;</c> yang sudah
/// melayani seluruh aplikasi — termasuk ASP.NET Core Identity lewat
/// <c>AddEntityFrameworkStores</c>.
/// </para>
/// <para>
/// Blueprint menandai kompatibilitas itu sebagai <b>risiko yang wajib diverifikasi, bukan
/// diasumsikan aman</b> (`02-backend-architecture.md` §H). Berkas ini yang memverifikasinya, dan
/// ia akan gagal bila kelak ada yang mengubah pendaftaran sehingga keduanya bertabrakan.
/// </para>
/// <para>
/// <b>Yang TIDAK diuji di sini:</b> perilaku alokasi nomor. Itu milik
/// <c>NumberSeriesAllocatorTests</c>, dan durabilitasnya milik <c>PLT-BE-004</c> di PostgreSQL
/// sungguhan.
/// </para>
/// </remarks>
public class NumberSeriesCompositionTests
{
    /// <summary>
    /// Keduanya terdaftar bersamaan — persis urutan yang dipakai <c>Program.cs</c> — dan
    /// keduanya tetap dapat diselesaikan container.
    /// </summary>
    [Fact]
    public void AddDbContext_DanAddDbContextFactory_HidupBerdampingan()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var services = new ServiceCollection();

        // Urutan sama dengan Program.cs: AddDbContext lebih dulu, factory menyusul.
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<ApplicationDbContext>(
            options => options.UseSqlite(connection),
            lifetime: ServiceLifetime.Scoped);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        var scoped = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        Assert.NotNull(scoped);
        Assert.NotNull(factory);
    }

    /// <summary>
    /// Konteks yang dibuat factory adalah <b>instance berbeda</b> dari konteks scoped. Inilah
    /// syarat teknis <c>DEC-PLT-008</c>: pencacah harus ditulis di luar transaksi pemanggil,
    /// dan itu mustahil bila keduanya memakai konteks yang sama.
    /// </summary>
    [Fact]
    public void KonteksDariFactory_BerbedaDariKonteksScoped()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<ApplicationDbContext>(
            options => options.UseSqlite(connection),
            lifetime: ServiceLifetime.Scoped);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        var scoped = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        using var dariFactory = factory.CreateDbContext();

        Assert.NotSame(scoped, dariFactory);
    }

    /// <summary>
    /// Konteks dari factory benar-benar dapat membaca dan menulis, bukan sekadar berhasil
    /// dibuat.
    /// </summary>
    [Fact]
    public void KonteksDariFactory_DapatMembacaModel()
    {
        using var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        var services = new ServiceCollection();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddDbContextFactory<ApplicationDbContext>(
            options => options.UseSqlite(connection),
            lifetime: ServiceLifetime.Scoped);

        using var provider = services.BuildServiceProvider(validateScopes: true);
        using var scope = provider.CreateScope();

        var scoped = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        scoped.Database.EnsureCreated();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var dariFactory = factory.CreateDbContext();

        Assert.Equal(0, dariFactory.NumNumberSeries.Count());
    }
}
