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
/// Bukti untuk <c>BE-BD-014</c> — master lokasi penyimpanan darah
/// (<c>MstBloodStorageLocation</c>, <c>DEC-BD-035</c>, <c>DEC-BD-037</c>, <c>BD-DOM-24</c>),
/// blueprint <c>BD-BP-001</c> revisi 21, kontrak <c>v4</c>.
///
/// Yang dibuktikan di sini:
///   1. Master dapat dikelola ujung ke ujung — dibuat, diubah, dinonaktifkan, dihapus.
///   2. Kode <b>dan</b> nama lokasi sama-sama tunggal (<c>VAL-BD-067</c>).
///   3. Menonaktifkan lokasi <b>tidak pernah ditolak</b> dan <b>tidak menyentuh apa pun</b>
///      selain penanda aktifnya sendiri (<c>VAL-BD-068</c>, <c>DEC-BD-037</c>).
///   4. Kotak pilihan lokasi <b>tidak pernah</b> menawarkan lokasi nonaktif, dan penyaringan
///      itu tidak dapat dimatikan pemanggil (lapisan pertama <c>INV-BD-027</c>).
///   5. Master tanpa lokasi aktif ditandai sebagai keadaan yang menghentikan modul
///      (<c>INV-BD-025</c>).
///   6. Nol kolom suhu, kapasitas, rak, dan hierarki gudang (<c>AC-BD-064</c>).
///   7. Seeder mengisi lokasi minimum yang aktif, dapat diulang, dan menolak berjalan di
///      produksi karena lokasi penyimpanan adalah benda fisik yang tidak boleh ditebak.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini dapat dijalankan tanpa database mana pun.
/// Konsekuensinya index unik fisik tidak ikut diuji; penolakan kode kembar dibuktikan lewat
/// jalur pemeriksaan service, dan index unik menjadi bagian verifikasi migration.
///
/// <c>AC-BD-062</c>, <c>AC-BD-065</c>, <c>AC-BD-066</c>, dan <c>AC-BD-067</c> sengaja
/// <b>tidak</b> diuji di sini; ketiganya menuntut penempatan kantong yang belum ada. Lihat
/// laporan task bagian 6.
/// </remarks>
public class BloodStorageLocationServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("77777777-7777-7777-7777-777777777777");

    // =====================================================================
    // 1. Pengelolaan dasar
    // =====================================================================

    [Fact]
    public async Task Membuat_LokasiBaru_MenyimpanKodeDalamHurufBesarDanAktif()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var hasil = await service.CreateAsync(
            new CreateBloodStorageLocationRequest
            {
                StorageLocationCode = " klk-bsr ",
                StorageLocationName = "  Kulkas Besar  "
            },
            ActorUserId);

        Assert.Equal(BloodStorageLocationStatus.Success, hasil.Status);
        Assert.Equal("KLK-BSR", hasil.Entity!.StorageLocationCode);
        Assert.Equal("Kulkas Besar", hasil.Entity.StorageLocationName);
        Assert.True(hasil.Entity.IsActive);
        Assert.Equal(ActorUserId, hasil.Entity.CreateBy);
    }

    /// <summary><c>VAL-BD-067</c> bagian kode.</summary>
    [Fact]
    public async Task Membuat_KodeYangSudahDipakai_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);

        var kedua = await service.CreateAsync(Permintaan("klk-bsr", "Kulkas Lain"), ActorUserId);

        Assert.Equal(BloodStorageLocationStatus.DuplicateIdentity, kedua.Status);
        Assert.Contains("Kode lokasi penyimpanan", kedua.Message);
        Assert.Equal(1, await db.Set<MstBloodStorageLocation>().CountAsync());
    }

    /// <summary>
    /// <c>VAL-BD-067</c> bagian nama. Kondisinya berbunyi "kode <b>atau</b> nama lokasi sudah
    /// dipakai", sehingga nama kembar juga ditahan — dua kulkas bernama sama membuat petugas
    /// tidak dapat membedakannya saat memilih lokasi.
    /// </summary>
    [Fact]
    public async Task Membuat_NamaYangSudahDipakai_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);

        var kedua = await service.CreateAsync(Permintaan("KLK-BSR-2", "kulkas besar"), ActorUserId);

        Assert.Equal(BloodStorageLocationStatus.DuplicateIdentity, kedua.Status);
        Assert.Contains("Nama lokasi penyimpanan", kedua.Message);
    }

    [Fact]
    public async Task Mengubah_KodeSendiriYangSalahKetik_Diizinkan()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var dibuat = await service.CreateAsync(Permintaan("KLKBSR", "Kulkas Besar"), ActorUserId);

        var hasil = await service.UpdateAsync(
            dibuat.Entity!.Id,
            new UpdateBloodStorageLocationRequest
            {
                StorageLocationCode = "KLK-BSR",
                StorageLocationName = "Kulkas Besar"
            },
            ActorUserId);

        Assert.Equal(BloodStorageLocationStatus.Success, hasil.Status);
        Assert.Equal("KLK-BSR", hasil.Entity!.StorageLocationCode);
        Assert.Equal(ActorUserId, hasil.Entity.UpdateBy);
    }

    [Fact]
    public async Task Membuat_TanpaKodeAtauNama_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var tanpaKode = await service.CreateAsync(Permintaan("  ", "Kulkas Besar"), ActorUserId);
        var tanpaNama = await service.CreateAsync(Permintaan("KLK-BSR", "  "), ActorUserId);

        Assert.Equal(BloodStorageLocationStatus.Invalid, tanpaKode.Status);
        Assert.Equal(BloodStorageLocationStatus.Invalid, tanpaNama.Status);
        Assert.Equal(0, await db.Set<MstBloodStorageLocation>().CountAsync());
    }

    // =====================================================================
    // 2. Penonaktifan — VAL-BD-068 dan DEC-BD-037
    // =====================================================================

    /// <summary>
    /// <c>VAL-BD-068</c> — penonaktifan <b>tidak pernah ditolak</b>. Menonaktifkan lokasi
    /// justru dilakukan ketika kulkasnya rusak; menolaknya akan memaksa petugas memindahkan
    /// kantong ke lokasi yang sedang rusak.
    /// </summary>
    [Fact]
    public async Task MenonaktifkanLokasi_SelaluBerhasil()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var dibuat = await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);

        var hasil = await service.UpdateStatusAsync(dibuat.Entity!.Id, isActive: false, ActorUserId);

        Assert.Equal(BloodStorageLocationStatus.Success, hasil.Status);
        Assert.False(hasil.Entity!.IsActive);
        Assert.Contains("tidak berpindah", hasil.Message);
    }

    /// <summary>
    /// <c>DEC-BD-037</c> — penonaktifan hanya menyentuh penanda aktifnya sendiri. Kode, nama,
    /// keterangan, dan penanda hapus tidak ikut berubah.
    /// </summary>
    [Fact]
    public async Task MenonaktifkanLokasi_TidakMenyentuhKolomLain()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var permintaan = Permintaan("KLK-BSR", "Kulkas Besar");
        permintaan.Description = "Kulkas darah utama";

        var dibuat = await service.CreateAsync(permintaan, ActorUserId);

        await service.UpdateStatusAsync(dibuat.Entity!.Id, isActive: false, ActorUserId);

        var sesudah = await db.Set<MstBloodStorageLocation>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == dibuat.Entity.Id);

        Assert.Equal("KLK-BSR", sesudah.StorageLocationCode);
        Assert.Equal("Kulkas Besar", sesudah.StorageLocationName);
        Assert.Equal("Kulkas darah utama", sesudah.Description);
        Assert.False(sesudah.IsDelete);
        Assert.False(sesudah.IsActive);
    }

    [Fact]
    public async Task LokasiDapatDiaktifkanKembali()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var dibuat = await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);

        await service.UpdateStatusAsync(dibuat.Entity!.Id, isActive: false, ActorUserId);
        var hasil = await service.UpdateStatusAsync(dibuat.Entity.Id, isActive: true, ActorUserId);

        Assert.True(hasil.Entity!.IsActive);
        Assert.Contains("diaktifkan", hasil.Message);
    }

    // =====================================================================
    // 3. Kotak pilihan hanya menawarkan lokasi aktif
    // =====================================================================

    /// <summary>
    /// Lapisan pertama <c>INV-BD-027</c>: master tidak pernah menawarkan lokasi nonaktif,
    /// sehingga layar tidak dapat memilihnya walaupun penulis layarnya lupa menyaring.
    /// Lapisan yang mengikat tetap pemeriksaan di jalur penyimpanan (<c>BE-BD-015</c>).
    /// </summary>
    [Fact]
    public async Task Pilihan_TidakPernahMenawarkanLokasiNonaktif()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);
        var kecil = await service.CreateAsync(Permintaan("KLK-KCL", "Kulkas Kecil"), ActorUserId);

        await service.UpdateStatusAsync(kecil.Entity!.Id, isActive: false, ActorUserId);

        var pilihan = await service.GetOptionsAsync(search: null);

        Assert.Single(pilihan);
        Assert.Equal("KLK-BSR", pilihan[0].StorageLocationCode);
    }

    [Fact]
    public async Task Pilihan_LokasiTerhapusJugaTidakDitawarkan()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var dibuat = await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);
        await service.DeleteAsync(dibuat.Entity!.Id, ActorUserId);

        var pilihan = await service.GetOptionsAsync(search: null);

        Assert.Empty(pilihan);
    }

    // =====================================================================
    // 4. Ringkasan dan penanda modul berhenti — INV-BD-025
    // =====================================================================

    /// <summary>
    /// <c>INV-BD-025</c> — tanpa satu pun lokasi aktif, tidak ada kantong yang dapat disimpan,
    /// dialokasikan, maupun diberikan. Keadaan itu ditandai tegas supaya terlihat di halaman
    /// index sebelum ada pasien yang menunggu.
    /// </summary>
    [Fact]
    public async Task Ringkasan_MenandaiModulBerhentiKetikaTidakAdaLokasiAktif()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var dibuat = await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);

        var sebelum = await service.GetSummaryAsync();
        Assert.False(sebelum.IsBloodBankHaltedByEmptyActiveLocation);

        await service.UpdateStatusAsync(dibuat.Entity!.Id, isActive: false, ActorUserId);

        var sesudah = await service.GetSummaryAsync();

        Assert.True(sesudah.IsBloodBankHaltedByEmptyActiveLocation);
        Assert.Equal(1, sesudah.TotalBloodStorageLocation);
        Assert.Equal(0, sesudah.ActiveBloodStorageLocation);
        Assert.Equal(1, sesudah.InactiveBloodStorageLocation);
    }

    [Fact]
    public async Task Ringkasan_MasterKosongJugaDitandaiBerhenti()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var ringkasan = await service.GetSummaryAsync();

        Assert.Equal(0, ringkasan.TotalBloodStorageLocation);
        Assert.True(ringkasan.IsBloodBankHaltedByEmptyActiveLocation);
    }

    [Fact]
    public async Task Daftar_MenyaringBerdasarkanStatusAktifDanMenghitungHalamanDiBackend()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        foreach (var kode in new[] { "KLK-01", "KLK-02", "KLK-03" })
            await service.CreateAsync(Permintaan(kode, $"Kulkas {kode}"), ActorUserId);

        var halaman = await service.GetPagedAsync(
            search: null,
            isActive: true,
            sortBy: "storageLocationCode",
            sortDirection: "asc",
            pageNumber: 2,
            pageSize: 2);

        Assert.Equal(3, halaman.TotalData);
        Assert.Equal(2, halaman.TotalPage);
        Assert.Single(halaman.Items);
        Assert.Equal("KLK-03", halaman.Items[0].StorageLocationCode);
    }

    // =====================================================================
    // 5. Batas scope MVP — AC-BD-064
    // =====================================================================

    /// <summary>
    /// <c>AC-BD-064</c> — sistem diminta mencatat suhu atau kapasitas: tidak ada kolom maupun
    /// endpoint. Diuji langsung terhadap model EF, bukan lewat pembacaan mata.
    ///
    /// Status <c>Stored</c> pada kantong menyatakan kantong punya tempat yang tercatat —
    /// <b>bukan</b> menyatakan rantai dinginnya terjaga.
    /// </summary>
    [Fact]
    public void Entity_TidakPunyaKolomSuhuKapasitasRakMaupunHierarki()
    {
        using var db = CreateContext();

        var entityType = db.Model.FindEntityType(typeof(MstBloodStorageLocation));

        Assert.NotNull(entityType);

        var namaKolom = entityType!.GetProperties()
            .Select(x => x.Name.ToLowerInvariant())
            .ToList();

        string[] terlarang =
        {
            "temperature", "suhu", "capacity", "kapasitas", "shelf", "rak",
            "bin", "laci", "parentid", "warehouse", "gudang", "sensor", "device"
        };

        foreach (var kata in terlarang)
            Assert.DoesNotContain(namaKolom, nama => nama.Contains(kata));
    }

    /// <summary>
    /// Kolom yang memang ada, dan hanya itu. Penjaga terhadap penambahan kolom diam-diam
    /// di luar kamus data.
    /// </summary>
    [Fact]
    public void Entity_HanyaPunyaKolomYangDitetapkanKamusData()
    {
        using var db = CreateContext();

        var entityType = db.Model.FindEntityType(typeof(MstBloodStorageLocation))!;

        var kolomBisnis = entityType.GetProperties()
            .Select(x => x.Name)
            .Where(x => x is not (
                "CreateDateTime" or "CreateBy" or "UpdateDateTime" or "UpdateBy" or
                "DeleteDateTime" or "DeleteBy" or "CancelDateTime" or "CancelBy" or
                "IsCancel" or "IsDelete"))
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(
            new[] { "Description", "Id", "IsActive", "StorageLocationCode", "StorageLocationName" },
            kolomBisnis);
    }

    [Fact]
    public void KolomIsActive_BawaanDatabasenyaAktif()
    {
        using var db = CreateContext();

        var entityType = db.Model.FindEntityType(typeof(MstBloodStorageLocation))!;
        var property = entityType.FindProperty(nameof(MstBloodStorageLocation.IsActive));

        Assert.NotNull(property);
        Assert.False(property!.IsNullable);
        Assert.Equal(true, property.GetDefaultValue());
    }

    // =====================================================================
    // 6. Penghapusan adalah penandaan
    // =====================================================================

    [Fact]
    public async Task Menghapus_MenandaiBarisTanpaMenghapusnyaSecaraFisik()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        var dibuat = await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Besar"), ActorUserId);

        var hasil = await service.DeleteAsync(dibuat.Entity!.Id, ActorUserId);

        Assert.Equal(BloodStorageLocationStatus.Success, hasil.Status);

        var baris = await db.Set<MstBloodStorageLocation>()
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == dibuat.Entity.Id);

        Assert.True(baris.IsDelete);
        Assert.False(baris.IsActive);
        Assert.Equal(ActorUserId, baris.DeleteBy);
        Assert.Null(await service.GetByIdAsync(dibuat.Entity.Id));
    }

    // =====================================================================
    // 7. Seeder
    // =====================================================================

    [Fact]
    public async Task Seeder_MengisiDuaLokasiMinimumDanKeduanyaAktif()
    {
        await using var db = CreateContext();

        var hasil = await BloodStorageLocationSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.False(hasil.Refused);
        Assert.Equal(2, hasil.LocationInserted);

        var lokasi = await db.Set<MstBloodStorageLocation>().ToListAsync();

        Assert.All(lokasi, x => Assert.True(x.IsActive));
        Assert.Equal(
            new[] { "KLK-BSR", "KLK-KCL" },
            lokasi.Select(x => x.StorageLocationCode).OrderBy(x => x));
    }

    /// <summary>
    /// Setelah seeder berjalan, modul tidak lagi berada dalam keadaan berhenti — itulah
    /// gunanya seeder ini di lingkungan pengembangan.
    /// </summary>
    [Fact]
    public async Task Seeder_MembuatModulTidakLagiBerhenti()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        Assert.True((await service.GetSummaryAsync()).IsBloodBankHaltedByEmptyActiveLocation);

        await BloodStorageLocationSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.False((await service.GetSummaryAsync()).IsBloodBankHaltedByEmptyActiveLocation);
    }

    [Fact]
    public async Task Seeder_DijalankanDuaKali_TidakMenggandakanBaris()
    {
        await using var db = CreateContext();

        var pertama = await BloodStorageLocationSeeder.SeedAsync(db, ActorUserId, "Development");
        var kedua = await BloodStorageLocationSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.Equal(2, pertama.LocationInserted);
        Assert.Equal(0, kedua.LocationInserted);
        Assert.Equal(2, kedua.LocationSkipped);
        Assert.Equal(2, await db.Set<MstBloodStorageLocation>().CountAsync());
    }

    [Fact]
    public async Task Seeder_TidakMenimpaLokasiYangSudahDisesuaikanPetugas()
    {
        await using var db = CreateContext();
        var service = new BloodStorageLocationService(db);

        await service.CreateAsync(Permintaan("KLK-BSR", "Kulkas Ruang BDRS Lantai 2"), ActorUserId);

        await BloodStorageLocationSeeder.SeedAsync(db, ActorUserId, "Development");

        var lokasi = await db.Set<MstBloodStorageLocation>()
            .SingleAsync(x => x.StorageLocationCode == "KLK-BSR");

        Assert.Equal("Kulkas Ruang BDRS Lantai 2", lokasi.StorageLocationName);
    }

    /// <summary>
    /// Lokasi penyimpanan adalah benda fisik yang benar-benar ada. Menebaknya di produksi
    /// menghasilkan master palsu yang terlanjur dipakai penempatan kantong.
    /// </summary>
    [Fact]
    public async Task Seeder_MenolakBerjalanDiProduksi()
    {
        await using var db = CreateContext();

        var hasil = await BloodStorageLocationSeeder.SeedAsync(db, ActorUserId, "Production");

        Assert.True(hasil.Refused);
        Assert.Equal(0, hasil.LocationInserted);
        Assert.Equal(0, await db.Set<MstBloodStorageLocation>().CountAsync());
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("production")]
    [InlineData("PRODUCTION")]
    public void Seeder_MengenaliNamaLingkunganProduksiTanpaMembedakanHurufBesar(string nama)
    {
        Assert.True(BloodStorageLocationSeeder.IsProductionEnvironment(nama));
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static CreateBloodStorageLocationRequest Permintaan(string kode, string nama) => new()
    {
        StorageLocationCode = kode,
        StorageLocationName = nama,
        IsActive = true
    };

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"blood-storage-location-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
