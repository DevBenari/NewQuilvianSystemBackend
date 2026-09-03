using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.MasterData;

/// <summary>
/// Bukti untuk <c>BE-BD-002</c> — titipan kolom kewenangan memesan darah pada
/// <c>MstServiceUnit</c> (<c>DEC-BD-012</c>, <c>BD-DOM-18</c>), blueprint <c>BD-BP-001</c>
/// revisi 21, kontrak <c>v4</c>.
///
/// Yang dibuktikan di sini:
///   1. Kewenangan memesan darah <b>bawaannya menolak</b> di tiga lapisan sekaligus — nilai
///      bawaan entity, nilai bawaan permintaan API, dan nilai bawaan di sisi database
///      (<c>AC-BD-016</c>).
///   2. Kewenangan dapat dinyalakan dan dimatikan lewat konfigurasi biasa, dan nilainya
///      bertahan setelah disimpan (<c>AC-BD-015</c>).
///   3. Penanda baru tidak mengubah nilai bawaan penanda saudaranya yang sudah ada.
///   4. Unit dapat disaring berdasarkan kewenangan ini, sehingga admin dapat melihat daftar
///      unit yang berwenang memesan darah.
/// </summary>
/// <remarks>
/// Provider InMemory <b>tidak</b> menerapkan nilai bawaan di sisi database. Karena itu bukti
/// untuk lapisan ketiga diambil dari metadata model EF — yaitu sumber yang sama yang dipakai
/// <c>dotnet ef</c> saat menurunkan migration — bukan dari perilaku penyimpanan InMemory.
/// Menguji nilai bawaan database lewat InMemory akan menghasilkan bukti palsu.
///
/// <c>AC-BD-013</c> sengaja <b>tidak</b> diuji di sini; lihat laporan task bagian 6.
/// </remarks>
public class ServiceUnitBloodOrderFlagTests
{
    private static readonly Guid ActorUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    // =====================================================================
    // 1. Bawaan menolak — AC-BD-016
    // =====================================================================

    /// <summary>Lapisan pertama: unit baru yang dibuat di kode lahir tanpa kewenangan.</summary>
    [Fact]
    public void UnitBaru_LahirTanpaKewenanganMemesanDarah()
    {
        var unit = new MstServiceUnit();

        Assert.False(unit.IsAvailableForBloodOrder);
    }

    /// <summary>
    /// Lapisan kedua: permintaan pembuatan unit yang tidak menyebut penanda ini sama sekali
    /// tetap menghasilkan unit tanpa kewenangan. Frontend lama yang belum mengenal kolom ini
    /// karena itu tidak dapat memberi kewenangan secara tidak sengaja.
    /// </summary>
    [Fact]
    public void PermintaanPembuatanUnit_TanpaMenyebutPenanda_TetapMenolak()
    {
        var request = new CreateServiceUnitRequest
        {
            ServiceUnitName = "Kamar Operasi",
            ServiceUnitType = ServiceUnitType.Unknown
        };

        Assert.False(request.IsAvailableForBloodOrder);
    }

    /// <summary>
    /// Lapisan ketiga: nilai bawaan di sisi database. Inilah yang membuat seluruh baris
    /// <c>MstServiceUnit</c> yang <b>sudah ada</b> ikut bernilai menolak begitu migration
    /// dijalankan, tanpa satu pun pengisian data susulan.
    /// </summary>
    [Fact]
    public void KolomKewenangan_BawaanDatabasenyaMenolak()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(MstServiceUnit));

        Assert.NotNull(entityType);

        var property = entityType!.FindProperty(nameof(MstServiceUnit.IsAvailableForBloodOrder));

        Assert.NotNull(property);
        Assert.False(property!.IsNullable);
        Assert.Equal(typeof(bool), property.ClrType);
        Assert.Equal(false, property.GetDefaultValue());
    }

    /// <summary>
    /// Penanda baru tidak boleh menggeser nilai bawaan penanda saudaranya. Ini penjaga
    /// terhadap kekeliruan salin-tempel yang mudah terjadi pada tabel dengan banyak penanda.
    /// </summary>
    [Fact]
    public void PenandaSaudara_NilaiBawaannyaTidakBergeser()
    {
        using var context = CreateContext();

        var entityType = context.Model.FindEntityType(typeof(MstServiceUnit))!;

        Assert.Equal(true, Bawaan(entityType, nameof(MstServiceUnit.IsAvailableForRegistration)));
        Assert.Equal(false, Bawaan(entityType, nameof(MstServiceUnit.IsAvailableForKiosk)));
        Assert.Equal(false, Bawaan(entityType, nameof(MstServiceUnit.IsAvailableForAppointment)));
        Assert.Equal(false, Bawaan(entityType, nameof(MstServiceUnit.IsAvailableForBloodOrder)));
        Assert.Equal(true, Bawaan(entityType, nameof(MstServiceUnit.IsQueueRequired)));
    }

    // =====================================================================
    // 2. Kewenangan diberikan lewat konfigurasi — AC-BD-015
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-015</c> — unit yang semula tidak berwenang diberi kewenangan lewat konfigurasi
    /// biasa, tanpa satu baris kode pun berubah, dan nilainya bertahan setelah disimpan.
    ///
    /// <b>Contoh.</b> Kamar Operasi awalnya tidak dapat memesan darah. Admin menyalakan
    /// penandanya lewat layar master unit pelayanan. Sejak saat itu Kamar Operasi terbaca
    /// sebagai unit berwenang, dan tidak ada penyebaran ulang aplikasi yang dibutuhkan.
    /// </summary>
    [Fact]
    public async Task UnitDiberiKewenanganLewatKonfigurasi_NilainyaBertahan()
    {
        await using var context = CreateContext();

        var unit = BuatUnit("SU-OK", "Kamar Operasi");

        context.Set<MstServiceUnit>().Add(unit);
        await context.SaveChangesAsync();

        Assert.False(unit.IsAvailableForBloodOrder);

        // Konfigurasi, bukan perubahan kode.
        var tersimpan = await context.Set<MstServiceUnit>().SingleAsync(x => x.Id == unit.Id);
        tersimpan.IsAvailableForBloodOrder = true;
        tersimpan.UpdateDateTime = DateTime.UtcNow;
        tersimpan.UpdateBy = ActorUserId;
        await context.SaveChangesAsync();

        var dibacaUlang = await context.Set<MstServiceUnit>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == unit.Id);

        Assert.True(dibacaUlang.IsAvailableForBloodOrder);
        Assert.Equal(ActorUserId, dibacaUlang.UpdateBy);
    }

    [Fact]
    public async Task KewenanganDapatDicabutKembali()
    {
        await using var context = CreateContext();

        var unit = BuatUnit("SU-IGD", "IGD");
        unit.IsAvailableForBloodOrder = true;

        context.Set<MstServiceUnit>().Add(unit);
        await context.SaveChangesAsync();

        unit.IsAvailableForBloodOrder = false;
        await context.SaveChangesAsync();

        var dibacaUlang = await context.Set<MstServiceUnit>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == unit.Id);

        Assert.False(dibacaUlang.IsAvailableForBloodOrder);
    }

    // =====================================================================
    // 3. Penyaringan daftar unit
    // =====================================================================

    [Fact]
    public async Task DaftarUnit_DapatDisaringBerdasarkanKewenanganMemesanDarah()
    {
        await using var context = CreateContext();

        var rawatInap = BuatUnit("SU-RWI", "Rawat Inap");
        rawatInap.IsAvailableForBloodOrder = true;

        var igd = BuatUnit("SU-IGD", "IGD");
        igd.IsAvailableForBloodOrder = true;

        var gizi = BuatUnit("SU-GIZ", "Instalasi Gizi");

        context.Set<MstServiceUnit>().AddRange(rawatInap, igd, gizi);
        await context.SaveChangesAsync();

        var berwenang = await context.Set<MstServiceUnit>()
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsAvailableForBloodOrder)
            .OrderBy(x => x.ServiceUnitName)
            .Select(x => x.ServiceUnitName)
            .ToListAsync();

        var tidakBerwenang = await context.Set<MstServiceUnit>()
            .AsNoTracking()
            .Where(x => !x.IsDelete && !x.IsAvailableForBloodOrder)
            .Select(x => x.ServiceUnitName)
            .ToListAsync();

        Assert.Equal(new[] { "IGD", "Rawat Inap" }, berwenang);
        Assert.Equal(new[] { "Instalasi Gizi" }, tidakBerwenang);
    }

    /// <summary>
    /// Unit yang sudah ditandai terhapus tidak boleh ikut terbaca sebagai unit berwenang,
    /// walaupun penandanya menyala saat dihapus.
    /// </summary>
    [Fact]
    public async Task UnitTerhapus_TidakTerbacaSebagaiUnitBerwenang()
    {
        await using var context = CreateContext();

        var unit = BuatUnit("SU-LAMA", "Unit Lama");
        unit.IsAvailableForBloodOrder = true;
        unit.IsDelete = true;

        context.Set<MstServiceUnit>().Add(unit);
        await context.SaveChangesAsync();

        var berwenang = await context.Set<MstServiceUnit>()
            .AsNoTracking()
            .Where(x => !x.IsDelete && x.IsAvailableForBloodOrder)
            .ToListAsync();

        Assert.Empty(berwenang);
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static object? Bawaan(
        Microsoft.EntityFrameworkCore.Metadata.IEntityType entityType,
        string namaProperty)
        => entityType.FindProperty(namaProperty)!.GetDefaultValue();

    private static MstServiceUnit BuatUnit(string kode, string nama) => new()
    {
        Id = Guid.NewGuid(),
        ServiceUnitCode = kode,
        ServiceUnitName = nama,
        ServiceUnitType = ServiceUnitType.Unknown,
        IsActive = true,
        CreateDateTime = DateTime.UtcNow,
        CreateBy = ActorUserId
    };

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"service-unit-blood-order-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
