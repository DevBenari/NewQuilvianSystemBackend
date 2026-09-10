using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.DTOs;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.Infrastructure;
using Xunit;

namespace QuilvianSystemBackend.Tests.Platform;

/// <summary>
/// Bukti untuk <c>PLT-BE-005</c> — layar pemantauan deret nomor, kontrak <c>v1</c>
/// (<c>api-contract.md</c> §2 dan §3).
/// </summary>
/// <remarks>
/// <para>
/// Yang dibuktikan di sini adalah <b>bentuk dan isi bacaan layar</b>: ringkasan yang benar,
/// penyaringan, pengurutan, paging, dan bahwa membaca tidak pernah mengubah satu baris pun.
/// </para>
/// <para>
/// <b>Batas yang jujur.</b> Berkas ini <b>tidak</b> membuktikan durabilitas maupun antrean
/// alokasi — keduanya milik <c>PLT-BE-004</c> dan mustahil dibuktikan di luar PostgreSQL, karena
/// SQLite tidak punya <c>pg_advisory_xact_lock</c>. Baris uji di sini ditulis langsung ke tabel,
/// bukan lewat alokator, justru supaya kedua hal itu tidak tampak seolah teruji di sini.
/// </para>
/// </remarks>
public class NumberSeriesQueryServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly DateTimeOffset Acuan =
        new(2026, 9, 9, 8, 15, 0, TimeSpan.FromHours(7));

    // =====================================================================
    // Ringkasan
    // =====================================================================

    /// <summary>
    /// <c>FR-PLT-012</c> — <c>TotalSeries</c> menghitung penanda deret yang berbeda, sedangkan
    /// <c>TotalScope</c> menghitung barisnya.
    /// </summary>
    /// <remarks>
    /// Keduanya sama selama seluruh deret memakai <c>NEVER</c>, dan baru berbeda ketika ada deret
    /// yang diulang per periode. Uji ini sengaja memakai satu deret dengan dua periode supaya
    /// perbedaannya benar-benar terlihat — bila keduanya diam-diam dihitung dengan cara yang sama,
    /// uji ini gagal.
    /// </remarks>
    [Fact]
    public async Task Ringkasan_MembedakanJumlahDeretDariJumlahBaris()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            Tambah(penulis, "BBK_BLOOD_ORDER", "GLOBAL", 12, Acuan);
            Tambah(penulis, "LAB_SPECIMEN", "2026", 5, Acuan.AddMinutes(-30), NumberSeriesResetPolicies.Yearly);
            Tambah(penulis, "LAB_SPECIMEN", "2025", 99, Acuan.AddDays(-400), NumberSeriesResetPolicies.Yearly);
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();
        var service = new NumberSeriesQueryService(context);

        var ringkasan = await service.GetSummaryAsync();

        // Dua penanda deret berbeda, tetapi tiga baris.
        Assert.Equal(2, ringkasan.TotalSeries);
        Assert.Equal(3, ringkasan.TotalScope);
        Assert.Equal(Acuan, ringkasan.LastAllocatedAt);
    }

    /// <summary>
    /// Tabel lahir kosong dan barisnya baru muncul pada alokasi pertama, sehingga keadaan "belum
    /// ada deret sama sekali" wajib terbaca sebagai nol dan <c>null</c> — bukan sebagai galat,
    /// dan bukan sebagai tanggal minimum yang menyesatkan pembacanya.
    /// </summary>
    [Fact]
    public async Task Ringkasan_TabelKosong_MemulangkanNolDanWaktuKosong()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();

        var ringkasan = await new NumberSeriesQueryService(context).GetSummaryAsync();

        Assert.Equal(0, ringkasan.TotalSeries);
        Assert.Equal(0, ringkasan.TotalScope);
        Assert.Null(ringkasan.LastAllocatedAt);
    }

    // =====================================================================
    // Daftar — penyaringan, pengurutan, paging
    // =====================================================================

    /// <summary>
    /// <c>FR-PLT-013</c> — daftar memulangkan nilai pencacah apa adanya, yaitu nilai terakhir
    /// yang <b>sudah terbit</b>, bukan nomor berikutnya.
    /// </summary>
    [Fact]
    public async Task Daftar_MemulangkanNilaiPencacahApaAdanya()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            Tambah(penulis, "BBK_BLOOD_ORDER", "GLOBAL", 57, Acuan);
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();

        var hasil = await new NumberSeriesQueryService(context)
            .GetPagedAsync(new NumberSeriesPagedQuery());

        var baris = Assert.Single(hasil.Items);

        Assert.Equal("BBK_BLOOD_ORDER", baris.SequenceKey);
        Assert.Equal("GLOBAL", baris.ScopeKey);
        Assert.Equal(NumberSeriesResetPolicies.Never, baris.ResetPolicy);
        Assert.Equal(57, baris.CurrentValue);
        Assert.Equal(Acuan, baris.LastAllocatedAt);
    }

    /// <summary>
    /// Pencarian menyasar penanda deret dan periode, dan tidak peka huruf besar-kecil.
    /// </summary>
    [Fact]
    public async Task Daftar_PencarianMenyasarPenandaDeretDanPeriode()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            Tambah(penulis, "BBK_BLOOD_ORDER", "GLOBAL", 1, Acuan);
            Tambah(penulis, "LAB_SPECIMEN", "GLOBAL", 2, Acuan);
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();
        var service = new NumberSeriesQueryService(context);

        var hasil = await service.GetPagedAsync(new NumberSeriesPagedQuery { Search = "bbk_blood" });

        Assert.Equal(1, hasil.TotalData);
        Assert.Equal("BBK_BLOOD_ORDER", Assert.Single(hasil.Items).SequenceKey);
    }

    /// <summary>
    /// Penyaring penanda deret bersifat sama persis, bukan mengandung. Deret adalah penanda
    /// teknis milik satu modul; pencocokan separuh akan mencampur deret milik modul lain yang
    /// kebetulan berawalan sama.
    /// </summary>
    [Fact]
    public async Task Daftar_PenyaringPenandaDeret_SamaPersisBukanMengandung()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            Tambah(penulis, "LAB_SPECIMEN", "GLOBAL", 1, Acuan);
            Tambah(penulis, "LAB_SPECIMEN_BATCH", "GLOBAL", 2, Acuan);
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();

        var hasil = await new NumberSeriesQueryService(context)
            .GetPagedAsync(new NumberSeriesPagedQuery { SequenceKey = "LAB_SPECIMEN" });

        Assert.Equal(1, hasil.TotalData);
        Assert.Equal("LAB_SPECIMEN", Assert.Single(hasil.Items).SequenceKey);
    }

    /// <summary>
    /// Pengurutan bawaan mengumpulkan periode milik satu deret dan menyusunnya berurutan,
    /// sehingga layar terbaca sebagai kelompok, bukan sebagai daftar acak.
    /// </summary>
    [Fact]
    public async Task Daftar_UrutanBawaan_MengelompokkanPeriodeMilikSatuDeret()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            Tambah(penulis, "LAB_SPECIMEN", "2026", 5, Acuan, NumberSeriesResetPolicies.Yearly);
            Tambah(penulis, "BBK_BLOOD_ORDER", "GLOBAL", 12, Acuan);
            Tambah(penulis, "LAB_SPECIMEN", "2025", 99, Acuan, NumberSeriesResetPolicies.Yearly);
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();

        var hasil = await new NumberSeriesQueryService(context)
            .GetPagedAsync(new NumberSeriesPagedQuery());

        Assert.Equal(
            new[] { "BBK_BLOOD_ORDER|GLOBAL", "LAB_SPECIMEN|2025", "LAB_SPECIMEN|2026" },
            hasil.Items.Select(x => $"{x.SequenceKey}|{x.ScopeKey}"));
    }

    /// <summary>
    /// Pengurutan menurun pada nilai pencacah — cara administrator menemukan deret yang paling
    /// jauh berjalan saat menelusuri keluhan nomor.
    /// </summary>
    [Fact]
    public async Task Daftar_DapatDiurutkanMenurunMenurutNilaiPencacah()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            Tambah(penulis, "A_SERIES", "GLOBAL", 3, Acuan);
            Tambah(penulis, "B_SERIES", "GLOBAL", 91, Acuan);
            Tambah(penulis, "C_SERIES", "GLOBAL", 47, Acuan);
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();

        var hasil = await new NumberSeriesQueryService(context).GetPagedAsync(
            new NumberSeriesPagedQuery { SortBy = "currentValue", SortDirection = "desc" });

        Assert.Equal(new long[] { 91, 47, 3 }, hasil.Items.Select(x => x.CurrentValue));
    }

    /// <summary>
    /// Paging memulangkan jumlah halaman dan total baris yang benar, bukan hanya potongan
    /// datanya.
    /// </summary>
    [Fact]
    public async Task Daftar_PagingMemulangkanTotalDanJumlahHalamanYangBenar()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            for (var i = 1; i <= 7; i++)
            {
                Tambah(penulis, $"SERIES_{i:00}", "GLOBAL", i, Acuan);
            }

            penulis.SaveChanges();
        }

        using var context = database.CreateContext();

        var hasil = await new NumberSeriesQueryService(context).GetPagedAsync(
            new NumberSeriesPagedQuery { PageNumber = 2, PageSize = 3 });

        Assert.Equal(7, hasil.TotalData);
        Assert.Equal(3, hasil.TotalPage);
        Assert.Equal(2, hasil.PageNumber);
        Assert.Equal(3, hasil.Items.Count);
        Assert.Equal("SERIES_04", hasil.Items[0].SequenceKey);
    }

    /// <summary>
    /// Ukuran halaman dibatasi 100 supaya satu permintaan tidak menarik seluruh tabel, dan nilai
    /// tak masuk akal dikembalikan ke nilai bawaan alih-alih melempar galat.
    /// </summary>
    [Fact]
    public async Task Daftar_UkuranHalamanTakMasukAkal_DikembalikanKeBatasnya()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();

        var service = new NumberSeriesQueryService(context);

        var terlaluBesar = await service.GetPagedAsync(
            new NumberSeriesPagedQuery { PageSize = 5000 });

        var negatif = await service.GetPagedAsync(
            new NumberSeriesPagedQuery { PageNumber = -3, PageSize = 0 });

        Assert.Equal(100, terlaluBesar.PageSize);
        Assert.Equal(1, negatif.PageNumber);
        Assert.Equal(25, negatif.PageSize);
    }

    // =====================================================================
    // Detail
    // =====================================================================

    /// <summary>
    /// Detail satu deret memulangkan barisnya apa adanya.
    /// </summary>
    [Fact]
    public async Task Detail_MemulangkanDeretYangDiminta()
    {
        using var database = TestDatabase.Create();

        var id = Guid.NewGuid();

        using (var penulis = database.CreateContext())
        {
            var baris = Tambah(penulis, "BBK_BLOOD_ORDER", "GLOBAL", 12, Acuan);
            baris.Id = id;
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();

        var hasil = await new NumberSeriesQueryService(context).GetByIdAsync(id);

        Assert.NotNull(hasil);
        Assert.Equal(id, hasil!.Id);
        Assert.Equal(12, hasil.CurrentValue);
    }

    /// <summary>
    /// Deret yang tidak ada memulangkan <c>null</c>, sehingga controller yang memutuskan bentuk
    /// <c>404</c>-nya — bukan service yang melempar galat.
    /// </summary>
    [Fact]
    public async Task Detail_DeretTidakAda_MemulangkanNull()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();

        var hasil = await new NumberSeriesQueryService(context).GetByIdAsync(Guid.NewGuid());

        Assert.Null(hasil);
    }

    // =====================================================================
    // Metadata penyaring
    // =====================================================================

    /// <summary>
    /// Metadata memuat keempat kebijakan pengulangan yang sah, sehingga kotak penyaring layar
    /// tidak perlu menuliskannya sendiri dan tidak dapat menyimpang dari konstanta backend.
    /// </summary>
    [Fact]
    public void Metadata_MemuatKeempatKebijakanPengulanganYangSah()
    {
        using var database = TestDatabase.Create();
        using var context = database.CreateContext();

        var metadata = new NumberSeriesQueryService(context).GetFilterMetadata();

        Assert.Equal(
            NumberSeriesResetPolicies.All.OrderBy(x => x, StringComparer.Ordinal),
            metadata.ResetPolicyOptions);

        Assert.Equal(new[] { "asc", "desc" }, metadata.SortDirections);
        Assert.NotEmpty(metadata.SortOptions);
    }

    /// <summary>
    /// Setiap pilihan pengurutan yang ditawarkan metadata benar-benar dikenali daftar. Pilihan
    /// yang tidak dikenali akan diam-diam jatuh ke urutan bawaan, sehingga layar tampak menuruti
    /// permintaan padahal tidak.
    /// </summary>
    [Fact]
    public async Task Metadata_SetiapPilihanPengurutan_BenarBenarDikenaliDaftar()
    {
        using var database = TestDatabase.Create();

        using (var penulis = database.CreateContext())
        {
            Tambah(penulis, "A_SERIES", "2025", 3, Acuan.AddDays(-2), NumberSeriesResetPolicies.Yearly);
            Tambah(penulis, "B_SERIES", "GLOBAL", 91, Acuan);
            penulis.SaveChanges();
        }

        using var context = database.CreateContext();
        var service = new NumberSeriesQueryService(context);

        foreach (var pilihan in service.GetFilterMetadata().SortOptions)
        {
            var naik = await service.GetPagedAsync(
                new NumberSeriesPagedQuery { SortBy = pilihan.Value, SortDirection = "asc" });

            var turun = await service.GetPagedAsync(
                new NumberSeriesPagedQuery { SortBy = pilihan.Value, SortDirection = "desc" });

            Assert.Equal(2, naik.Items.Count);

            Assert.True(
                naik.Items[0].SequenceKey != turun.Items[0].SequenceKey,
                $"Pilihan pengurutan '{pilihan.Value}' tidak mengubah urutan, " +
                "artinya ia jatuh ke urutan bawaan dan tidak benar-benar dikenali.");
        }
    }

    // =====================================================================
    // Penjaga baca-saja
    // =====================================================================

    /// <summary>
    /// <c>INV-PLT-001</c> dan <c>INV-PLT-002</c> — membaca layar pemantauan tidak boleh mengubah
    /// satu baris pun.
    /// </summary>
    /// <remarks>
    /// Penjaga terhadap kekeliruan yang paling mungkin terjadi di kemudian hari: menambahkan
    /// "rapikan nilai" atau "isi periode yang hilang" ke dalam jalur baca. Uji ini membaca
    /// seluruh permukaan lalu membandingkan isi tabel sebelum dan sesudahnya lewat konteks baru,
    /// supaya perubahan yang hanya tertahan di memori pun tetap ketahuan.
    /// </remarks>
    [Fact]
    public async Task MembacaSeluruhPermukaan_TidakMengubahSatuBarisPun()
    {
        using var database = TestDatabase.Create();

        var id = Guid.NewGuid();

        using (var penulis = database.CreateContext())
        {
            var baris = Tambah(penulis, "BBK_BLOOD_ORDER", "GLOBAL", 12, Acuan);
            baris.Id = id;
            penulis.SaveChanges();
        }

        var sebelum = Potret(database);

        using (var context = database.CreateContext())
        {
            var service = new NumberSeriesQueryService(context);

            service.GetFilterMetadata();
            await service.GetSummaryAsync();
            await service.GetPagedAsync(new NumberSeriesPagedQuery());
            await service.GetByIdAsync(id);
            await service.GetByIdAsync(Guid.NewGuid());
        }

        Assert.Equal(sebelum, Potret(database));
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static NumNumberSeries Tambah(
        ApplicationDbContext context,
        string sequenceKey,
        string scopeKey,
        long currentValue,
        DateTimeOffset lastAllocatedAt,
        string resetPolicy = NumberSeriesResetPolicies.Never)
    {
        var baris = new NumNumberSeries
        {
            SequenceKey = sequenceKey,
            ScopeKey = scopeKey,
            ResetPolicy = resetPolicy,
            CurrentValue = currentValue,
            LastAllocatedAt = lastAllocatedAt,
            CreateBy = ActorUserId,
            UpdateBy = ActorUserId
        };

        context.NumNumberSeries.Add(baris);

        return baris;
    }

    /// <summary>
    /// Potret isi tabel lewat konteks baru, supaya perubahan yang belum tersimpan pun terbaca.
    /// </summary>
    private static List<string> Potret(TestDatabase database)
    {
        using var context = database.CreateContext();

        return context.NumNumberSeries
            .OrderBy(x => x.SequenceKey)
            .ThenBy(x => x.ScopeKey)
            .Select(x => $"{x.Id}|{x.SequenceKey}|{x.ScopeKey}|{x.ResetPolicy}|{x.CurrentValue}|{x.LastAllocatedAt:O}")
            .ToList();
    }
}
