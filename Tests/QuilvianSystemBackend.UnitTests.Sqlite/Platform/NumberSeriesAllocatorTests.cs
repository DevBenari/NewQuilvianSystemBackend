using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.Tests.Platform;

/// <summary>
/// Bukti untuk <c>PLT-BE-003</c> — alokator nomor bisnis bersama
/// (<c>NumberSeriesAllocator</c>), blueprint <c>PLT-BP-001</c>, kontrak <c>v1</c>.
/// </summary>
/// <remarks>
/// <para>
/// Yang dibuktikan di sini: perakitan format, pemilihan periode, penolakan parameter, dan
/// kenaikan pencacah yang berurutan.
/// </para>
/// <para>
/// <b>Batas yang wajib disadari pembaca.</b> Berkas ini berjalan di SQLite, dan SQLite
/// <b>tidak punya</b> <c>pg_advisory_xact_lock</c>. Dua hal yang justru paling menentukan modul
/// ini karena itu <b>TIDAK</b> terbukti di sini:
/// </para>
/// <list type="number">
/// <item><b>Durabilitas</b> — bahwa pencacah bertahan ketika transaksi bisnis pemanggil
/// dibatalkan (<c>AC-PLT-003</c>).</item>
/// <item><b>Antrean</b> — bahwa dua alokasi bersamaan menghasilkan dua nomor berbeda tanpa
/// kegagalan (<c>AC-PLT-004</c>, <c>AC-PLT-005</c>).</item>
/// </list>
/// <para>
/// Keduanya menuntut PostgreSQL sungguhan dan menjadi lingkup <c>PLT-BE-004</c>. Menyatakannya
/// lulus berdasarkan berkas ini akan menghasilkan rasa aman yang keliru — bentuk kegagalan yang
/// paling berbahaya pada modul ini.
/// </para>
/// </remarks>
public class NumberSeriesAllocatorTests
{
    private static readonly Guid ActorUserId = Guid.Parse("66666666-6666-6666-6666-666666666666");
    private static readonly DateTimeOffset Instant = new(2026, 9, 9, 3, 15, 0, TimeSpan.Zero);

    // =====================================================================
    // 1. Perakitan nomor dan kenaikan pencacah
    // =====================================================================

    [Fact]
    public async Task AlokasiPertama_MenerbitkanNomorUrutSatu()
    {
        await using var uji = Lingkungan.Buat();

        var nomor = await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));

        Assert.Equal("BDO-00000001", nomor);
    }

    [Fact]
    public async Task AlokasiBerikutnya_MenerbitkanNomorBerurutan()
    {
        await using var uji = Lingkungan.Buat();

        var pertama = await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));
        var kedua = await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));
        var ketiga = await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));

        Assert.Equal("BDO-00000001", pertama);
        Assert.Equal("BDO-00000002", kedua);
        Assert.Equal("BDO-00000003", ketiga);
    }

    /// <summary>Deret berbeda punya pencacah sendiri dan tidak saling menyela.</summary>
    [Fact]
    public async Task DeretBerbeda_PencacahnyaSendiriSendiri()
    {
        await using var uji = Lingkungan.Buat();

        await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));
        await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));

        var deretLain = await uji.Alokator.AllocateAsync(Permintaan("BBK_PROVIDER_REQUEST", "BDR"));

        Assert.Equal("BDR-00000001", deretLain);
    }

    /// <summary>
    /// <c>QBE-CODE-003</c> — nilai diambil dari pencacah tersimpan, bukan dihitung dari data.
    /// </summary>
    /// <remarks>
    /// Dibuktikan dengan menaikkan pencacah langsung ke 500 tanpa satu pun catatan yang
    /// menempel. Alokator yang menghitung dari data akan memulangkan 1; alokator yang membaca
    /// pencacah memulangkan 501.
    /// </remarks>
    [Fact]
    public async Task NilaiDiambilDariPencacah_BukanDihitungDariData()
    {
        await using var uji = Lingkungan.Buat();

        await uji.Alokator.AllocateAsync(Permintaan("BBK_PROCEDURE", "BDP"));

        await using (var context = uji.BuatKonteks())
        {
            var deret = await context.NumNumberSeries.SingleAsync();
            deret.CurrentValue = 500;
            await context.SaveChangesAsync();
        }

        var nomor = await uji.Alokator.AllocateAsync(Permintaan("BBK_PROCEDURE", "BDP"));

        Assert.Equal("BDP-00000501", nomor);
    }

    /// <summary>
    /// <c>INV-PLT-002</c> — deret yang berlubang tidak pernah diisi. Lubang dibuat dengan
    /// menaikkan pencacah, lalu dibuktikan alokasi berikutnya melompatinya.
    /// </summary>
    [Fact]
    public async Task DeretBerlubang_LubangnyaTidakPernahDiisi()
    {
        await using var uji = Lingkungan.Buat();

        await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));

        await using (var context = uji.BuatKonteks())
        {
            var deret = await context.NumNumberSeries.SingleAsync();
            deret.CurrentValue = 3;   // nomor 2 dan 3 dianggap hangus
            await context.SaveChangesAsync();
        }

        var berikutnya = await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));

        Assert.Equal("BDO-00000004", berikutnya);
    }

    [Fact]
    public async Task JumlahDigit_MengikutiPermintaanModul()
    {
        await using var uji = Lingkungan.Buat();

        var empat = await uji.Alokator.AllocateAsync(Permintaan("UJI_4", "AAA", digits: 4));
        var duaBelas = await uji.Alokator.AllocateAsync(Permintaan("UJI_12", "BBB", digits: 12));

        Assert.Equal("AAA-0001", empat);
        Assert.Equal("BBB-000000000001", duaBelas);
    }

    /// <summary><c>DEC-PLT-005</c> — awalan datang dari modul, dan dinormalkan huruf besar.</summary>
    [Fact]
    public async Task Awalan_DatangDariModulDanDinormalkan()
    {
        await using var uji = Lingkungan.Buat();

        var nomor = await uji.Alokator.AllocateAsync(Permintaan("UJI_AWALAN", "  bdo  "));

        Assert.StartsWith("BDO-", nomor);
    }

    // =====================================================================
    // 2. Periode dan kebijakan pengulangan
    // =====================================================================

    /// <summary>
    /// <c>DEC-PLT-004</c> / <c>INV-PLT-004</c> — deret <c>NEVER</c> memakai periode
    /// <c>GLOBAL</c>, dan nomornya tidak memuat penanda periode.
    /// </summary>
    [Fact]
    public async Task KebijakanNever_PeriodeGlobalDanNomorTanpaPenandaPeriode()
    {
        await using var uji = Lingkungan.Buat();

        var nomor = await uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO"));

        Assert.Equal("BDO-00000001", nomor);

        await using var context = uji.BuatKonteks();
        var deret = await context.NumNumberSeries.SingleAsync();
        Assert.Equal("GLOBAL", deret.ScopeKey);
    }

    /// <summary>
    /// <c>DEC-PLT-004</c> — pergantian tahun **tidak** mengulang deret <c>NEVER</c>.
    /// </summary>
    [Fact]
    public async Task KebijakanNever_MelewatiPergantianTahun_TidakDiulang()
    {
        await using var uji = Lingkungan.Buat();

        await uji.Alokator.AllocateAsync(
            Permintaan("BBK_BLOOD_ORDER", "BDO") with { Instant = new DateTimeOffset(2026, 12, 31, 20, 0, 0, TimeSpan.Zero) });

        var tahunBaru = await uji.Alokator.AllocateAsync(
            Permintaan("BBK_BLOOD_ORDER", "BDO") with { Instant = new DateTimeOffset(2027, 1, 1, 5, 0, 0, TimeSpan.Zero) });

        Assert.Equal("BDO-00000002", tahunBaru);
    }

    /// <summary>
    /// Kebijakan harian memakai periode setempat dan menomori nomornya dengan penanda periode.
    /// Hanya untuk menampung deret lama saat <c>PLT-SLICE-02</c>.
    /// </summary>
    [Fact]
    public async Task KebijakanDaily_PeriodeBerpindahDanPencacahMulaiDariSatu()
    {
        await using var uji = Lingkungan.Buat();

        var hariPertama = await uji.Alokator.AllocateAsync(
            Permintaan("BIL_INVOICE", "BIL", kebijakan: NumberSeriesResetPolicies.Daily)
                with { Instant = new DateTimeOffset(2026, 9, 9, 5, 0, 0, TimeSpan.Zero) });

        var hariKedua = await uji.Alokator.AllocateAsync(
            Permintaan("BIL_INVOICE", "BIL", kebijakan: NumberSeriesResetPolicies.Daily)
                with { Instant = new DateTimeOffset(2026, 9, 10, 5, 0, 0, TimeSpan.Zero) });

        Assert.Equal("BIL-20260909-00000001", hariPertama);
        Assert.Equal("BIL-20260910-00000001", hariKedua);
    }

    // =====================================================================
    // 3. Penolakan parameter — nol nomor terbit
    // =====================================================================

    [Theory]
    [InlineData("", "BDO", 8, "VAL-PLT-001")]
    [InlineData("BBK_BLOOD_ORDER", "", 8, "VAL-PLT-002")]
    [InlineData("BBK_BLOOD_ORDER", "AWALAN_YANG_TERLALU_PANJANG", 8, "VAL-PLT-002")]
    [InlineData("BBK_BLOOD_ORDER", "BDO", 3, "VAL-PLT-004")]
    [InlineData("BBK_BLOOD_ORDER", "BDO", 13, "VAL-PLT-004")]
    public async Task ParameterTidakSah_DitolakDanNolNomorTerbit(
        string deret, string awalan, int digits, string kodeValidasi)
    {
        await using var uji = Lingkungan.Buat();

        var galat = await Assert.ThrowsAsync<NumberSeriesAllocationException>(
            () => uji.Alokator.AllocateAsync(Permintaan(deret, awalan, digits: digits)));

        Assert.Equal(kodeValidasi, galat.ValidationCode);

        await using var context = uji.BuatKonteks();
        Assert.Equal(0, await context.NumNumberSeries.CountAsync());
    }

    [Fact]
    public async Task KebijakanPengulanganAsing_Ditolak()
    {
        await using var uji = Lingkungan.Buat();

        var galat = await Assert.ThrowsAsync<NumberSeriesAllocationException>(
            () => uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO", kebijakan: "WEEKLY")));

        Assert.Equal("VAL-PLT-003", galat.ValidationCode);
    }

    [Fact]
    public async Task PelakuTidakDikenali_Ditolak()
    {
        await using var uji = Lingkungan.Buat();

        var galat = await Assert.ThrowsAsync<NumberSeriesAllocationException>(
            () => uji.Alokator.AllocateAsync(Permintaan("BBK_BLOOD_ORDER", "BDO") with { ActorUserId = Guid.Empty }));

        Assert.Equal("VAL-PLT-005", galat.ValidationCode);
    }

    /// <summary>
    /// <c>VAL-PLT-007</c> — nilai yang tidak muat pada jumlah digit **ditolak**, bukan
    /// diterbitkan dengan bentuk yang menyimpang. Pelajaran dari `FACT-PLT-009`.
    /// </summary>
    [Fact]
    public async Task NilaiTidakMuatPadaJumlahDigit_DitolakBukanDiterbitkanMenyimpang()
    {
        await using var uji = Lingkungan.Buat();

        await uji.Alokator.AllocateAsync(Permintaan("UJI_HABIS", "XXX", digits: 4));

        await using (var context = uji.BuatKonteks())
        {
            var deret = await context.NumNumberSeries.SingleAsync();
            deret.CurrentValue = 9999;
            await context.SaveChangesAsync();
        }

        var galat = await Assert.ThrowsAsync<NumberSeriesAllocationException>(
            () => uji.Alokator.AllocateAsync(Permintaan("UJI_HABIS", "XXX", digits: 4)));

        Assert.Equal("VAL-PLT-007", galat.ValidationCode);

        // Pencacah TIDAK naik: penolakan terjadi sebelum commit.
        await using var pemeriksa = uji.BuatKonteks();
        Assert.Equal(9999, (await pemeriksa.NumNumberSeries.SingleAsync()).CurrentValue);
    }

    // =====================================================================
    // 4. Penjaga arsitektur
    // =====================================================================

    /// <summary>
    /// Alokator <b>tidak boleh</b> menerima <c>ApplicationDbContext</c> scoped. Menerimanya
    /// berarti menulis pencacah di dalam transaksi pemanggil — persis perilaku yang ditolak
    /// <c>DEC-PLT-008</c>. Dijaga sebagai bukti, bukan diserahkan pada ingatan.
    /// </summary>
    [Fact]
    public void Alokator_TidakMenerimaKonteksScoped()
    {
        var parameter = typeof(NumberSeriesAllocator)
            .GetConstructors()
            .Single()
            .GetParameters();

        Assert.Single(parameter);
        Assert.Equal(typeof(IDbContextFactory<ApplicationDbContext>), parameter[0].ParameterType);
        Assert.DoesNotContain(parameter, p => p.ParameterType == typeof(ApplicationDbContext));
    }

    // =====================================================================
    // Penolong
    // =====================================================================

    private static NumberAllocationRequest Permintaan(
        string deret,
        string awalan,
        int digits = 8,
        string kebijakan = NumberSeriesResetPolicies.Never)
        => new(deret, awalan, kebijakan, digits, ActorUserId, Instant);

    /// <summary>
    /// Lingkungan uji: satu basis data SQLite di memori, dipakai bersama konteks scoped dan
    /// konteks yang dibuat factory — sama seperti komposisi aplikasi.
    /// </summary>
    private sealed class Lingkungan : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly ServiceProvider _provider;

        private Lingkungan(SqliteConnection connection, ServiceProvider provider)
        {
            _connection = connection;
            _provider = provider;
            Alokator = provider.GetRequiredService<NumberSeriesAllocator>();
        }

        public NumberSeriesAllocator Alokator { get; }

        public ApplicationDbContext BuatKonteks()
            => _provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext();

        public static Lingkungan Buat()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();

            var services = new ServiceCollection();
            services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connection));
            services.AddDbContextFactory<ApplicationDbContext>(
                o => o.UseSqlite(connection),
                lifetime: ServiceLifetime.Scoped);
            services.AddScoped<NumberSeriesAllocator>();

            var provider = services.BuildServiceProvider(validateScopes: false);

            using (var context = provider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext())
            {
                context.Database.EnsureCreated();
            }

            return new Lingkungan(connection, provider);
        }

        public async ValueTask DisposeAsync()
        {
            await _provider.DisposeAsync();
            _connection.Dispose();
        }
    }
}
