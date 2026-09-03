using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.MasterData;

/// <summary>
/// Bukti untuk <c>BE-BD-001</c> bagian katalog komponen darah (<c>MstBloodComponent</c>),
/// blueprint <c>BD-BP-001</c> revisi 21, kontrak <c>v4</c>.
///
/// Yang dibuktikan di sini:
///   1. Komponen dapat dikelola ujung ke ujung — dibuat, diubah, dinonaktifkan, dihapus.
///   2. Kode komponen tunggal. Kode kembar ditolak, termasuk yang hanya berbeda huruf besar.
///   3. Masa berlaku bukti kecocokan dibaca dari katalog per komponen dan tidak pernah diisi
///      angka bawaan oleh sistem (<c>AC-BD-055</c>, <c>AC-BD-056</c>, <c>INV-BD-023</c>).
///   4. Komponen tanpa masa berlaku ditandai sebagai tertahan, dan jumlahnya terbaca pada
///      ringkasan halaman index (<c>VAL-BD-020b</c>).
///   5. Penghapusan adalah penandaan, bukan penghapusan fisik.
///   6. Seeder mengisi katalog minimum PRC/TC/FFP, dapat dijalankan berulang, menolak berjalan
///      di produksi, dan tidak pernah menebak angka masa berlaku.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini dapat dijalankan tanpa database mana pun.
/// Konsekuensinya index unik fisik dan foreign key tidak ikut diuji di sini; keduanya menjadi
/// bagian verifikasi migration yang merupakan wewenang terpisah dan dicatat pada laporan task.
/// Karena itu penolakan kode kembar diuji lewat jalur pemeriksaan service, bukan lewat
/// pelanggaran index.
/// </remarks>
public class BloodComponentServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    // =====================================================================
    // 1. Pengelolaan dasar
    // =====================================================================

    [Fact]
    public async Task Membuat_KomponenBaru_MenyimpanKodeDalamHurufBesar()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var hasil = await service.CreateAsync(
            new CreateBloodComponentRequest
            {
                ComponentCode = " prc ",
                ComponentName = "  Packed Red Cells  "
            },
            ActorUserId);

        Assert.Equal(BloodComponentStatus.Success, hasil.Status);
        Assert.Equal("PRC", hasil.Entity!.ComponentCode);
        Assert.Equal("Packed Red Cells", hasil.Entity.ComponentName);
        Assert.Equal(ActorUserId, hasil.Entity.CreateBy);
        Assert.True(hasil.Entity.IsActive);
    }

    [Fact]
    public async Task Membuat_KodeYangSudahDipakai_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        await service.CreateAsync(Permintaan("PRC", "Packed Red Cells"), ActorUserId);

        // Huruf kecil sengaja dipakai: kode yang sama tetap kode yang sama.
        var kedua = await service.CreateAsync(Permintaan("prc", "Duplikat"), ActorUserId);

        Assert.Equal(BloodComponentStatus.DuplicateCode, kedua.Status);
        Assert.Contains("sudah dipakai", kedua.Message);
        Assert.Equal(1, await db.Set<MstBloodComponent>().CountAsync());
    }

    [Fact]
    public async Task Mengubah_KodeKeMilikKomponenLain_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        await service.CreateAsync(Permintaan("PRC", "Packed Red Cells"), ActorUserId);
        var tc = await service.CreateAsync(Permintaan("TC", "Trombosit Concentrate"), ActorUserId);

        var hasil = await service.UpdateAsync(
            tc.Entity!.Id,
            new UpdateBloodComponentRequest { ComponentCode = "PRC", ComponentName = "Trombosit" },
            ActorUserId);

        Assert.Equal(BloodComponentStatus.DuplicateCode, hasil.Status);
    }

    [Fact]
    public async Task Mengubah_KodeSendiriYangSalahKetik_Diizinkan()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var dibuat = await service.CreateAsync(Permintaan("PRCC", "Packed Red Cells"), ActorUserId);

        var hasil = await service.UpdateAsync(
            dibuat.Entity!.Id,
            new UpdateBloodComponentRequest { ComponentCode = "PRC", ComponentName = "Packed Red Cells" },
            ActorUserId);

        Assert.Equal(BloodComponentStatus.Success, hasil.Status);
        Assert.Equal("PRC", hasil.Entity!.ComponentCode);
        Assert.Equal(ActorUserId, hasil.Entity.UpdateBy);
    }

    // =====================================================================
    // 2. Masa berlaku bukti kecocokan — AC-BD-055 dan AC-BD-056
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-055</c> — dua komponen dengan masa berlaku berbeda, keduanya diterapkan
    /// sesuai komponennya masing-masing dan dibaca dari katalog.
    /// </summary>
    [Fact]
    public async Task MasaBerlaku_BerbedaPerKomponen_DibacaDariKatalog()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        await service.CreateAsync(Permintaan("PRC", "Packed Red Cells", 72), ActorUserId);
        await service.CreateAsync(Permintaan("TC", "Trombosit Concentrate", 24), ActorUserId);

        var opsi = await service.GetOptionsAsync(search: null, onlyActive: true);

        Assert.Equal(72, opsi.Single(x => x.ComponentCode == "PRC").CompatibilityEvidenceValidityHours);
        Assert.Equal(24, opsi.Single(x => x.ComponentCode == "TC").CompatibilityEvidenceValidityHours);
    }

    /// <summary>
    /// <c>AC-BD-056</c> dan <c>INV-BD-023</c> — komponen yang dibuat tanpa masa berlaku TIDAK
    /// memperoleh angka bawaan dari sistem. Nilainya tetap kosong, dan kekosongan itu ditandai
    /// sebagai penahan pemberian, bukan disembunyikan.
    /// </summary>
    [Fact]
    public async Task MasaBerlaku_TidakDiisi_TetapKosongDanDitandaiMenahanPemberian()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var dibuat = await service.CreateAsync(Permintaan("FFP", "Fresh Frozen Plasma"), ActorUserId);

        Assert.Null(dibuat.Entity!.CompatibilityEvidenceValidityHours);

        var response = BloodComponentService.ToResponse(dibuat.Entity);

        Assert.Null(response.CompatibilityEvidenceValidityHours);
        Assert.True(response.IsIssuanceBlockedByMissingValidity);
    }

    [Fact]
    public async Task MasaBerlaku_DiisiSetelahnya_PenandaTertahanIkutPadam()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var dibuat = await service.CreateAsync(Permintaan("FFP", "Fresh Frozen Plasma"), ActorUserId);

        var hasil = await service.UpdateAsync(
            dibuat.Entity!.Id,
            new UpdateBloodComponentRequest
            {
                ComponentCode = "FFP",
                ComponentName = "Fresh Frozen Plasma",
                CompatibilityEvidenceValidityHours = 48
            },
            ActorUserId);

        Assert.Equal(BloodComponentStatus.Success, hasil.Status);
        Assert.False(BloodComponentService.ToResponse(hasil.Entity!).IsIssuanceBlockedByMissingValidity);
    }

    /// <summary>
    /// Nol jam akan membuat bukti kedaluwarsa seketika dan menutup seluruh pemberian komponen
    /// ini secara diam-diam. Ditolak sebagai kesalahan isian supaya sebabnya terbaca.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task MasaBerlaku_NolAtauNegatif_Ditolak(int jam)
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var hasil = await service.CreateAsync(
            Permintaan("PRC", "Packed Red Cells", jam),
            ActorUserId);

        Assert.Equal(BloodComponentStatus.Invalid, hasil.Status);
        Assert.Contains("lebih dari nol jam", hasil.Message);
    }

    [Fact]
    public async Task Membuat_TanpaKodeAtauNama_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var tanpaKode = await service.CreateAsync(Permintaan("   ", "Packed Red Cells"), ActorUserId);
        var tanpaNama = await service.CreateAsync(Permintaan("PRC", "   "), ActorUserId);

        Assert.Equal(BloodComponentStatus.Invalid, tanpaKode.Status);
        Assert.Equal(BloodComponentStatus.Invalid, tanpaNama.Status);
        Assert.Equal(0, await db.Set<MstBloodComponent>().CountAsync());
    }

    // =====================================================================
    // 3. Ringkasan, pilihan, dan daftar
    // =====================================================================

    [Fact]
    public async Task Ringkasan_MenghitungKomponenYangMasaBerlakunyaBelumDitetapkan()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        await service.CreateAsync(Permintaan("PRC", "Packed Red Cells", 72), ActorUserId);
        await service.CreateAsync(Permintaan("TC", "Trombosit Concentrate"), ActorUserId);
        var ffp = await service.CreateAsync(Permintaan("FFP", "Fresh Frozen Plasma"), ActorUserId);

        await service.UpdateStatusAsync(ffp.Entity!.Id, isActive: false, ActorUserId);

        var ringkasan = await service.GetSummaryAsync();

        Assert.Equal(3, ringkasan.TotalBloodComponent);
        Assert.Equal(2, ringkasan.ActiveBloodComponent);
        Assert.Equal(1, ringkasan.InactiveBloodComponent);
        Assert.Equal(1, ringkasan.ValidityConfiguredBloodComponent);

        // FFP nonaktif tidak ikut dihitung: yang mendesak dikonfigurasi hanya komponen aktif.
        Assert.Equal(1, ringkasan.ValidityNotConfiguredBloodComponent);
    }

    [Fact]
    public async Task Pilihan_HanyaKomponenAktif_SecaraBawaan()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        await service.CreateAsync(Permintaan("PRC", "Packed Red Cells"), ActorUserId);
        var tc = await service.CreateAsync(Permintaan("TC", "Trombosit Concentrate"), ActorUserId);
        await service.UpdateStatusAsync(tc.Entity!.Id, isActive: false, ActorUserId);

        var bawaan = await service.GetOptionsAsync(search: null, onlyActive: true);
        var semua = await service.GetOptionsAsync(search: null, onlyActive: false);

        Assert.Single(bawaan);
        Assert.Equal("PRC", bawaan[0].ComponentCode);
        Assert.Equal(2, semua.Count);
    }

    [Fact]
    public async Task Daftar_MenyaringKomponenYangMasaBerlakunyaBelumDitetapkan()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        await service.CreateAsync(Permintaan("PRC", "Packed Red Cells", 72), ActorUserId);
        await service.CreateAsync(Permintaan("TC", "Trombosit Concentrate"), ActorUserId);

        var belumDikonfigurasi = await service.GetPagedAsync(
            search: null,
            isActive: null,
            isValidityConfigured: false,
            sortBy: null,
            sortDirection: null,
            pageNumber: 1,
            pageSize: 25);

        Assert.Equal(1, belumDikonfigurasi.TotalData);
        Assert.Equal("TC", belumDikonfigurasi.Items.Single().ComponentCode);
        Assert.True(belumDikonfigurasi.Items.Single().IsIssuanceBlockedByMissingValidity);
    }

    [Fact]
    public async Task Daftar_MenghitungHalamanDiSisiBackend()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        foreach (var kode in new[] { "PRC", "TC", "FFP", "WB", "CRYO" })
            await service.CreateAsync(Permintaan(kode, $"Komponen {kode}"), ActorUserId);

        var halaman = await service.GetPagedAsync(
            search: null,
            isActive: null,
            isValidityConfigured: null,
            sortBy: "componentCode",
            sortDirection: "asc",
            pageNumber: 2,
            pageSize: 2);

        Assert.Equal(5, halaman.TotalData);
        Assert.Equal(3, halaman.TotalPage);
        Assert.Equal(2, halaman.PageNumber);
        Assert.Equal(2, halaman.Items.Count);
    }

    [Fact]
    public void Metadata_MengumumkanPenyaringYangBenarBenarDidukungDaftar()
    {
        var metadata = BloodComponentService.BuildFilterMetadata();

        var namaParameter = metadata.QueryParameters.Select(x => x.Name).ToList();

        Assert.Contains("search", namaParameter);
        Assert.Contains("isActive", namaParameter);
        Assert.Contains("isValidityConfigured", namaParameter);
        Assert.Contains("sortBy", namaParameter);
        Assert.Contains("pageNumber", namaParameter);
        Assert.NotEmpty(metadata.SortOptions);
        Assert.NotEmpty(metadata.CreateFields);
        Assert.NotEmpty(metadata.UpdateFields);
    }

    // =====================================================================
    // 4. Penghapusan adalah penandaan
    // =====================================================================

    [Fact]
    public async Task Menghapus_MenandaiBarisTanpaMenghapusnyaSecaraFisik()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var dibuat = await service.CreateAsync(Permintaan("PRC", "Packed Red Cells"), ActorUserId);

        var hasil = await service.DeleteAsync(dibuat.Entity!.Id, ActorUserId);

        Assert.Equal(BloodComponentStatus.Success, hasil.Status);

        var baris = await db.Set<MstBloodComponent>()
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == dibuat.Entity.Id);

        Assert.True(baris.IsDelete);
        Assert.False(baris.IsActive);
        Assert.NotNull(baris.DeleteDateTime);
        Assert.Equal(ActorUserId, baris.DeleteBy);

        // Sudah tidak terbaca lewat jalur biasa.
        Assert.Null(await service.GetByIdAsync(dibuat.Entity.Id));
    }

    [Fact]
    public async Task Menghapus_KomponenYangSudahDihapus_MengembalikanTidakDitemukan()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var dibuat = await service.CreateAsync(Permintaan("PRC", "Packed Red Cells"), ActorUserId);
        await service.DeleteAsync(dibuat.Entity!.Id, ActorUserId);

        var kedua = await service.DeleteAsync(dibuat.Entity.Id, ActorUserId);

        Assert.Equal(BloodComponentStatus.NotFound, kedua.Status);
    }

    [Fact]
    public async Task KodeMilikKomponenTerhapus_DapatDipakaiLagi()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        var dibuat = await service.CreateAsync(Permintaan("PRC", "Packed Red Cells"), ActorUserId);
        await service.DeleteAsync(dibuat.Entity!.Id, ActorUserId);

        var ulang = await service.CreateAsync(Permintaan("PRC", "Packed Red Cells"), ActorUserId);

        Assert.Equal(BloodComponentStatus.Success, ulang.Status);
    }

    // =====================================================================
    // 5. Seeder katalog minimum
    // =====================================================================

    [Fact]
    public async Task Seeder_MengisiKatalogMinimumPrcTcFfp()
    {
        await using var db = CreateContext();

        var hasil = await BloodComponentSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.False(hasil.Refused);
        Assert.Equal(3, hasil.ComponentInserted);

        var kode = await db.Set<MstBloodComponent>()
            .Select(x => x.ComponentCode)
            .OrderBy(x => x)
            .ToListAsync();

        Assert.Equal(new[] { "FFP", "PRC", "TC" }, kode);
    }

    /// <summary>
    /// <c>INV-BD-023</c> — seeder tidak pernah menebak angka masa berlaku. Ketiga komponen
    /// lahir dengan nilai kosong, sehingga pemberiannya tertahan sampai BDRS mengisinya.
    /// </summary>
    [Fact]
    public async Task Seeder_TidakPernahMenebakMasaBerlaku()
    {
        await using var db = CreateContext();

        await BloodComponentSeeder.SeedAsync(db, ActorUserId, "Development");

        var semua = await db.Set<MstBloodComponent>().ToListAsync();

        Assert.All(semua, x => Assert.Null(x.CompatibilityEvidenceValidityHours));
    }

    [Fact]
    public async Task Seeder_DijalankanDuaKali_TidakMenggandakanBaris()
    {
        await using var db = CreateContext();

        var pertama = await BloodComponentSeeder.SeedAsync(db, ActorUserId, "Development");
        var kedua = await BloodComponentSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.Equal(3, pertama.ComponentInserted);
        Assert.Equal(0, kedua.ComponentInserted);
        Assert.Equal(3, kedua.ComponentSkipped);
        Assert.Equal(3, await db.Set<MstBloodComponent>().CountAsync());
    }

    [Fact]
    public async Task Seeder_TidakMenimpaNilaiYangSudahDisesuaikanPetugas()
    {
        await using var db = CreateContext();
        var service = new BloodComponentService(db);

        await service.CreateAsync(Permintaan("PRC", "PRC Khas MMC", 72), ActorUserId);

        await BloodComponentSeeder.SeedAsync(db, ActorUserId, "Development");

        var prc = await db.Set<MstBloodComponent>().SingleAsync(x => x.ComponentCode == "PRC");

        Assert.Equal("PRC Khas MMC", prc.ComponentName);
        Assert.Equal(72, prc.CompatibilityEvidenceValidityHours);
    }

    [Fact]
    public async Task Seeder_MenolakBerjalanDiProduksi()
    {
        await using var db = CreateContext();

        var hasil = await BloodComponentSeeder.SeedAsync(db, ActorUserId, "Production");

        Assert.True(hasil.Refused);
        Assert.Equal(0, hasil.ComponentInserted);
        Assert.Equal(0, await db.Set<MstBloodComponent>().CountAsync());
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("production")]
    [InlineData("PRODUCTION")]
    public void Seeder_MengenaliNamaLingkunganProduksiTanpaMembedakanHurufBesar(string nama)
    {
        Assert.True(BloodComponentSeeder.IsProductionEnvironment(nama));
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static CreateBloodComponentRequest Permintaan(
        string kode,
        string nama,
        int? masaBerlakuJam = null)
        => new()
        {
            ComponentCode = kode,
            ComponentName = nama,
            CompatibilityEvidenceValidityHours = masaBerlakuJam,
            IsActive = true
        };

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"blood-component-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
