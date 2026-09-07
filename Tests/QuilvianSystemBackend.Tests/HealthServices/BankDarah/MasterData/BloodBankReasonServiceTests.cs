using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.HealthServices.BankDarah.MasterData;

/// <summary>
/// Bukti untuk sisa <c>BE-BD-001</c> — daftar alasan terkendali Bank Darah
/// (<c>MstBloodBankReason</c>, <c>DEC-BD-024</c>, <c>INV-BD-016</c>, <c>BD-DOM-14</c>),
/// blueprint <c>BD-BP-001</c> revisi 23, kontrak <c>v4</c>.
///
/// Yang dibuktikan di sini:
///   1. Daftar dapat dikelola ujung ke ujung — dibuat, diubah, dinonaktifkan, dihapus.
///   2. Kode alasan tunggal.
///   3. <b>Kategori wajib berasal dari daftar tertutup.</b> Nilai di luar daftar ditolak,
///      sehingga salah ketik tidak diam-diam menciptakan kategori yang tak pernah dibaca.
///   4. Pembatalan order punya <b>dua</b> kategori terpisah (<c>DEC-BD-044</c>), dan keduanya
///      tidak boleh menyatu.
///   5. Kotak pilihan menyaring per kategori, dan kategori tak dikenal memulangkan daftar
///      kosong — bukan seluruh isi tabel.
///   6. Kategori yang belum punya alasan aktif terbaca sebagai angka pada ringkasan.
///   7. Seeder mengisi satu alasan untuk setiap kategori, dapat diulang, dan menolak berjalan
///      di produksi.
/// </summary>
/// <remarks>
/// Provider InMemory dipakai supaya bukti ini dapat dijalankan tanpa database mana pun.
/// Konsekuensinya index unik fisik tidak ikut diuji; penolakan kode kembar dibuktikan lewat
/// jalur pemeriksaan service.
/// </remarks>
public class BloodBankReasonServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("88888888-8888-8888-8888-888888888888");

    // =====================================================================
    // 1. Pengelolaan dasar
    // =====================================================================

    [Fact]
    public async Task Membuat_AlasanBaru_MenyimpanKodeDalamHurufBesar()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var hasil = await service.CreateAsync(
            new CreateBloodBankReasonRequest
            {
                ReasonCode = " cancel-klinis-01 ",
                ReasonText = "  Kebutuhan transfusi dibatalkan dokter  ",
                ReasonCategory = BloodBankReasonCategories.OrderCancellationClinical
            },
            ActorUserId);

        Assert.Equal(BloodBankReasonStatus.Success, hasil.Status);
        Assert.Equal("CANCEL-KLINIS-01", hasil.Entity!.ReasonCode);
        Assert.Equal("Kebutuhan transfusi dibatalkan dokter", hasil.Entity.ReasonText);
        Assert.True(hasil.Entity.IsActive);
    }

    [Fact]
    public async Task Membuat_KodeYangSudahDipakai_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        await service.CreateAsync(Permintaan("CANCEL-01", "Alasan pertama"), ActorUserId);

        var kedua = await service.CreateAsync(Permintaan("cancel-01", "Alasan kedua"), ActorUserId);

        Assert.Equal(BloodBankReasonStatus.DuplicateCode, kedua.Status);
        Assert.Equal(1, await db.Set<MstBloodBankReason>().CountAsync());
    }

    [Fact]
    public async Task Membuat_TanpaKodeAtauTeks_Ditolak()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var tanpaKode = await service.CreateAsync(Permintaan("   ", "Alasan"), ActorUserId);
        var tanpaTeks = await service.CreateAsync(Permintaan("CANCEL-01", "  "), ActorUserId);

        Assert.Equal(BloodBankReasonStatus.Invalid, tanpaKode.Status);
        Assert.Equal(BloodBankReasonStatus.Invalid, tanpaTeks.Status);
        Assert.Equal(0, await db.Set<MstBloodBankReason>().CountAsync());
    }

    // =====================================================================
    // 2. Kategori berasal dari daftar tertutup
    // =====================================================================

    /// <summary>
    /// Kolomnya bertipe teks mengikuti kamus data, sehingga daftar tertutupnya dijaga service.
    /// Tanpa penjagaan ini, satu salah ketik menciptakan kategori baru yang tidak pernah dibaca
    /// layar mana pun — dan alasannya hilang dari kotak pilihan tanpa ada yang menyadarinya.
    /// </summary>
    [Theory]
    [InlineData("OrderCancellation")]
    [InlineData("Pembatalan")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Membuat_KategoriDiLuarDaftarTertutup_Ditolak(string kategori)
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var hasil = await service.CreateAsync(
            new CreateBloodBankReasonRequest
            {
                ReasonCode = "CANCEL-01",
                ReasonText = "Alasan",
                ReasonCategory = kategori
            },
            ActorUserId);

        Assert.Equal(BloodBankReasonStatus.Invalid, hasil.Status);
        Assert.Contains("Kategori alasan", hasil.Message);
        Assert.Equal(0, await db.Set<MstBloodBankReason>().CountAsync());
    }

    /// <summary>
    /// Kategori dicocokkan tanpa membedakan huruf besar-kecil, lalu disimpan dalam bentuk
    /// bakunya — supaya penyaring dan laporan tidak pecah karena perbedaan penulisan.
    /// </summary>
    [Fact]
    public async Task Membuat_KategoriBedaHurufBesar_DinormalkanKeBentukBaku()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var hasil = await service.CreateAsync(
            new CreateBloodBankReasonRequest
            {
                ReasonCode = "CANCEL-01",
                ReasonText = "Alasan",
                ReasonCategory = "ordercancellationCLINICAL"
            },
            ActorUserId);

        Assert.Equal(BloodBankReasonStatus.Success, hasil.Status);
        Assert.Equal(
            BloodBankReasonCategories.OrderCancellationClinical,
            hasil.Entity!.ReasonCategory);
    }

    [Fact]
    public void DaftarKategori_BerisiSepuluhNilaiSesuaiKamusData()
    {
        Assert.Equal(10, BloodBankReasonCategories.All.Length);
        Assert.Equal(
            BloodBankReasonCategories.All.Length,
            BloodBankReasonCategories.All.Distinct().Count());
    }

    /// <summary>
    /// <c>DEC-BD-044</c> — pembatalan order punya dua kategori terpisah. Keduanya memakai butir
    /// hak akses yang sama, sehingga kategori inilah satu-satunya yang membedakan pembatalan
    /// klinis dari pembatalan operasional saat ditinjau.
    /// </summary>
    [Fact]
    public void PembatalanOrder_PunyaDuaKategoriTerpisahYangTidakMenyatu()
    {
        var pembatalan = BloodBankReasonCategories.All
            .Where(x => x.StartsWith("OrderCancellation", StringComparison.Ordinal))
            .OrderBy(x => x)
            .ToList();

        Assert.Equal(
            new[]
            {
                BloodBankReasonCategories.OrderCancellationClinical,
                BloodBankReasonCategories.OrderCancellationOperational
            },
            pembatalan);

        // Kategori gabungan lama tidak boleh hidup berdampingan dengan kedua penggantinya.
        Assert.Null(BloodBankReasonCategories.Normalize("OrderCancellation"));
    }

    [Fact]
    public void SetiapKategori_PunyaLabelYangDibacaPetugas()
    {
        var opsi = BloodBankReasonService.BuildCategoryOptions();

        Assert.Equal(BloodBankReasonCategories.All.Length, opsi.Count);
        Assert.All(opsi, x => Assert.False(string.IsNullOrWhiteSpace(x.Label)));
        Assert.All(opsi, x => Assert.False(string.IsNullOrWhiteSpace(x.Description)));
        Assert.All(opsi, x => Assert.Contains(x.Value, BloodBankReasonCategories.All));
    }

    // =====================================================================
    // 3. Kotak pilihan per kategori
    // =====================================================================

    [Fact]
    public async Task Pilihan_MenyaringPerKategori()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        await service.CreateAsync(
            Permintaan("CANCEL-KLINIS", "Operasi ditunda", BloodBankReasonCategories.OrderCancellationClinical),
            ActorUserId);
        await service.CreateAsync(
            Permintaan("CANCEL-OPS", "Order ganda", BloodBankReasonCategories.OrderCancellationOperational),
            ActorUserId);
        await service.CreateAsync(
            Permintaan("RETUR", "Dikembalikan ke PMI", BloodBankReasonCategories.Return),
            ActorUserId);

        var klinis = await service.GetOptionsAsync(
            BloodBankReasonCategories.OrderCancellationClinical, search: null);

        Assert.Single(klinis);
        Assert.Equal("CANCEL-KLINIS", klinis[0].ReasonCode);
    }

    /// <summary>
    /// Kategori tak dikenal memulangkan daftar <b>kosong</b>, bukan seluruh isi tabel.
    /// Memulangkan semuanya justru menawarkan alasan yang salah konteks kepada petugas —
    /// misalnya alasan pengembalian ke PMI muncul di layar pembatalan order.
    /// </summary>
    [Fact]
    public async Task Pilihan_KategoriTidakDikenal_MemulangkanDaftarKosong()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        await service.CreateAsync(Permintaan("CANCEL-01", "Alasan"), ActorUserId);

        var hasil = await service.GetOptionsAsync("KategoriKarangan", search: null);

        Assert.Empty(hasil);
    }

    [Fact]
    public async Task Pilihan_TidakMenawarkanAlasanNonaktifMaupunTerhapus()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var aktif = await service.CreateAsync(Permintaan("CANCEL-01", "Masih dipakai"), ActorUserId);
        var nonaktif = await service.CreateAsync(Permintaan("CANCEL-02", "Sudah tidak dipakai"), ActorUserId);
        var terhapus = await service.CreateAsync(Permintaan("CANCEL-03", "Dihapus"), ActorUserId);

        await service.UpdateStatusAsync(nonaktif.Entity!.Id, isActive: false, ActorUserId);
        await service.DeleteAsync(terhapus.Entity!.Id, ActorUserId);

        var pilihan = await service.GetOptionsAsync(
            BloodBankReasonCategories.OrderCancellationClinical, search: null);

        Assert.Single(pilihan);
        Assert.Equal(aktif.Entity!.ReasonCode, pilihan[0].ReasonCode);
    }

    // =====================================================================
    // 4. Ringkasan kategori yang belum terisi
    // =====================================================================

    /// <summary>
    /// Kategori tanpa satu pun alasan aktif membuat tindakan yang memerlukannya tidak dapat
    /// diselesaikan sama sekali (<c>INV-BD-016</c>). Angkanya ditampilkan supaya keadaan itu
    /// terlihat sebelum petugas menemuinya di tengah proses.
    /// </summary>
    [Fact]
    public async Task Ringkasan_MenghitungKategoriYangBelumPunyaAlasanAktif()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var kosongDiAwal = await service.GetSummaryAsync();
        Assert.Equal(10, kosongDiAwal.CategoryWithoutActiveReasonCount);

        await service.CreateAsync(
            Permintaan("CANCEL-KLINIS", "Operasi ditunda", BloodBankReasonCategories.OrderCancellationClinical),
            ActorUserId);

        var sesudah = await service.GetSummaryAsync();

        Assert.Equal(9, sesudah.CategoryWithoutActiveReasonCount);
        Assert.DoesNotContain(
            BloodBankReasonCategories.OrderCancellationClinical,
            sesudah.CategoryWithoutActiveReason);
        Assert.Equal(1, sesudah.ActiveBloodBankReason);
    }

    /// <summary>
    /// Alasan yang dinonaktifkan membuat kategorinya kembali terhitung kosong — karena yang
    /// menentukan adalah tersedianya pilihan, bukan adanya baris.
    /// </summary>
    [Fact]
    public async Task Ringkasan_AlasanDinonaktifkan_KategorinyaKembaliTerhitungKosong()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var dibuat = await service.CreateAsync(Permintaan("CANCEL-01", "Alasan"), ActorUserId);

        Assert.Equal(9, (await service.GetSummaryAsync()).CategoryWithoutActiveReasonCount);

        await service.UpdateStatusAsync(dibuat.Entity!.Id, isActive: false, ActorUserId);

        var sesudah = await service.GetSummaryAsync();

        Assert.Equal(10, sesudah.CategoryWithoutActiveReasonCount);
        Assert.Equal(1, sesudah.TotalBloodBankReason);
        Assert.Equal(0, sesudah.ActiveBloodBankReason);
    }

    [Fact]
    public async Task Daftar_MenyaringPerKategoriDanMenghitungHalamanDiBackend()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        foreach (var kode in new[] { "A1", "A2", "A3" })
            await service.CreateAsync(Permintaan(kode, $"Alasan {kode}"), ActorUserId);

        await service.CreateAsync(
            Permintaan("B1", "Alasan lain", BloodBankReasonCategories.Return), ActorUserId);

        var halaman = await service.GetPagedAsync(
            search: null,
            isActive: null,
            reasonCategory: BloodBankReasonCategories.OrderCancellationClinical,
            sortBy: "reasonCode",
            sortDirection: "asc",
            pageNumber: 2,
            pageSize: 2);

        Assert.Equal(3, halaman.TotalData);
        Assert.Equal(2, halaman.TotalPage);
        Assert.Single(halaman.Items);
        Assert.Equal("A3", halaman.Items[0].ReasonCode);
    }

    // =====================================================================
    // 5. Penonaktifan dan penghapusan
    // =====================================================================

    [Fact]
    public async Task Menonaktifkan_TidakMenyentuhKolomLain()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var dibuat = await service.CreateAsync(Permintaan("CANCEL-01", "Operasi ditunda"), ActorUserId);

        await service.UpdateStatusAsync(dibuat.Entity!.Id, isActive: false, ActorUserId);

        var sesudah = await db.Set<MstBloodBankReason>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == dibuat.Entity.Id);

        Assert.Equal("CANCEL-01", sesudah.ReasonCode);
        Assert.Equal("Operasi ditunda", sesudah.ReasonText);
        Assert.Equal(BloodBankReasonCategories.OrderCancellationClinical, sesudah.ReasonCategory);
        Assert.False(sesudah.IsActive);
        Assert.False(sesudah.IsDelete);
    }

    [Fact]
    public async Task Menghapus_MenandaiBarisTanpaMenghapusnyaSecaraFisik()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var dibuat = await service.CreateAsync(Permintaan("CANCEL-01", "Alasan"), ActorUserId);

        await service.DeleteAsync(dibuat.Entity!.Id, ActorUserId);

        var baris = await db.Set<MstBloodBankReason>()
            .IgnoreQueryFilters()
            .SingleAsync(x => x.Id == dibuat.Entity.Id);

        Assert.True(baris.IsDelete);
        Assert.False(baris.IsActive);
        Assert.Equal(ActorUserId, baris.DeleteBy);
        Assert.Null(await service.GetByIdAsync(dibuat.Entity.Id));
    }

    [Fact]
    public async Task Mengubah_KategoriYangSalahDipilih_DapatDibetulkan()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var dibuat = await service.CreateAsync(
            Permintaan("RETUR-01", "Dikembalikan ke PMI", BloodBankReasonCategories.OrderCancellationClinical),
            ActorUserId);

        var hasil = await service.UpdateAsync(
            dibuat.Entity!.Id,
            new UpdateBloodBankReasonRequest
            {
                ReasonCode = "RETUR-01",
                ReasonText = "Dikembalikan ke PMI",
                ReasonCategory = BloodBankReasonCategories.Return
            },
            ActorUserId);

        Assert.Equal(BloodBankReasonStatus.Success, hasil.Status);
        Assert.Equal(BloodBankReasonCategories.Return, hasil.Entity!.ReasonCategory);
    }

    // =====================================================================
    // 6. Seeder
    // =====================================================================

    /// <summary>
    /// Satu alasan untuk setiap kategori. Menyisakan satu kategori kosong berarti menyisakan
    /// satu jalur proses yang buntu.
    /// </summary>
    [Fact]
    public async Task Seeder_MengisiSatuAlasanUntukSetiapKategori()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        var hasil = await BloodBankReasonSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.False(hasil.Refused);
        Assert.Equal(BloodBankReasonCategories.All.Length, hasil.ReasonInserted);

        var ringkasan = await service.GetSummaryAsync();

        Assert.Equal(0, ringkasan.CategoryWithoutActiveReasonCount);
        Assert.Empty(ringkasan.CategoryWithoutActiveReason);
    }

    /// <summary>
    /// Kode bawaan diberi awalan <c>SEED-</c> supaya baris seeder mudah dibedakan dari baris
    /// yang benar-benar disusun BDRS, dan mudah dinonaktifkan setelah daftar aslinya masuk.
    /// </summary>
    [Fact]
    public async Task Seeder_MenandaiBarisnyaDenganAwalanSeed()
    {
        await using var db = CreateContext();

        await BloodBankReasonSeeder.SeedAsync(db, ActorUserId, "Development");

        var semua = await db.Set<MstBloodBankReason>().ToListAsync();

        Assert.All(semua, x => Assert.StartsWith(
            BloodBankReasonSeeder.SeedCodePrefix, x.ReasonCode, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Seeder_SeluruhKategoriYangDiseedSah()
    {
        await using var db = CreateContext();

        await BloodBankReasonSeeder.SeedAsync(db, ActorUserId, "Development");

        var kategori = await db.Set<MstBloodBankReason>()
            .Select(x => x.ReasonCategory)
            .ToListAsync();

        Assert.All(kategori, x => Assert.Contains(x, BloodBankReasonCategories.All));
    }

    [Fact]
    public async Task Seeder_DijalankanDuaKali_TidakMenggandakanBaris()
    {
        await using var db = CreateContext();

        var pertama = await BloodBankReasonSeeder.SeedAsync(db, ActorUserId, "Development");
        var kedua = await BloodBankReasonSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.Equal(10, pertama.ReasonInserted);
        Assert.Equal(0, kedua.ReasonInserted);
        Assert.Equal(10, kedua.ReasonSkipped);
        Assert.Equal(10, await db.Set<MstBloodBankReason>().CountAsync());
    }

    [Fact]
    public async Task Seeder_TidakMenimpaAlasanYangSudahDisesuaikanPetugas()
    {
        await using var db = CreateContext();
        var service = new BloodBankReasonService(db);

        await service.CreateAsync(
            Permintaan(
                BloodBankReasonSeeder.SeedCodePrefix + "CANCEL-KLINIS",
                "Rumusan khas MMC"),
            ActorUserId);

        await BloodBankReasonSeeder.SeedAsync(db, ActorUserId, "Development");

        var alasan = await db.Set<MstBloodBankReason>()
            .SingleAsync(x => x.ReasonCode == BloodBankReasonSeeder.SeedCodePrefix + "CANCEL-KLINIS");

        Assert.Equal("Rumusan khas MMC", alasan.ReasonText);
    }

    [Fact]
    public async Task Seeder_MenolakBerjalanDiProduksi()
    {
        await using var db = CreateContext();

        var hasil = await BloodBankReasonSeeder.SeedAsync(db, ActorUserId, "Production");

        Assert.True(hasil.Refused);
        Assert.Equal(0, hasil.ReasonInserted);
        Assert.Equal(0, await db.Set<MstBloodBankReason>().CountAsync());
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("production")]
    [InlineData("PRODUCTION")]
    public void Seeder_MengenaliNamaLingkunganProduksiTanpaMembedakanHurufBesar(string nama)
    {
        Assert.True(BloodBankReasonSeeder.IsProductionEnvironment(nama));
    }

    // =====================================================================
    // 7. Metadata
    // =====================================================================

    [Fact]
    public void Metadata_MengumumkanPenyaringYangBenarBenarDidukungDaftar()
    {
        var metadata = BloodBankReasonService.BuildFilterMetadata();

        var namaParameter = metadata.QueryParameters.Select(x => x.Name).ToList();

        Assert.Contains("search", namaParameter);
        Assert.Contains("isActive", namaParameter);
        Assert.Contains("reasonCategory", namaParameter);
        Assert.Equal(BloodBankReasonCategories.All.Length, metadata.ReasonCategoryOptions.Count);
        Assert.NotEmpty(metadata.CreateFields);
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static CreateBloodBankReasonRequest Permintaan(
        string kode,
        string teks,
        string? kategori = null) => new()
        {
            ReasonCode = kode,
            ReasonText = teks,
            ReasonCategory = kategori ?? BloodBankReasonCategories.OrderCancellationClinical,
            IsActive = true
        };

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"blood-bank-reason-{Guid.NewGuid():N}")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new ApplicationDbContext(options);
    }
}
