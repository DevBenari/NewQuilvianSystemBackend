using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Models;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.BillingTests.Infrastructure;
using QuilvianSystemBackend.Repositories;
using Xunit;

namespace QuilvianSystemBackend.BillingTests.Platform
{
    /// <summary>
    /// Bukti untuk <c>PLT-BE-004</c> — durabilitas dan antrean alokasi nomor, diverifikasi pada
    /// PostgreSQL nyata. Blueprint <c>PLT-BP-001</c>, kontrak <c>v1</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa berkas ini tidak dapat digabung ke uji SQLite.</b> Yang diuji di sini adalah apa
    /// yang terjadi pada <c>COMMIT</c> dan <c>ROLLBACK</c>, dan bagaimana dua permintaan bersamaan
    /// diantrekan <c>pg_advisory_xact_lock</c>. Provider InMemory tidak punya transaksi sungguhan,
    /// dan SQLite tidak punya kunci penasihat — keduanya akan <b>lulus tanpa membuktikan apa
    /// pun</b>, bentuk kegagalan yang paling berbahaya pada modul ini.
    /// </para>
    /// <para>
    /// <b>Fixture yang dipakai fail-closed.</b> Tanpa environment variable
    /// <c>QUILVIAN_BILLING_TEST_DB</c> yang menunjuk database test tersendiri, seluruh berkas ini
    /// berhenti dengan configuration error dan <b>nol perintah</b> dikirim ke server mana pun.
    /// Itu perilaku yang disengaja setelah temuan <c>RJ-BIL-BE-002</c>.
    /// </para>
    /// </remarks>
    public sealed class NumberSeriesDurabilityTests : IClassFixture<BillingTestDatabaseFixture>
    {
        private static readonly Guid ActorUserId = Guid.Parse("77777777-0000-0000-0000-000000000001");

        private readonly BillingTestDatabaseFixture _fixture;

        public NumberSeriesDurabilityTests(BillingTestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        // =================================================================
        // AC-PLT-003 — durabilitas
        // =================================================================

        /// <summary>
        /// <c>AC-PLT-003</c> — <b>inti seluruh slice ini.</b> Nomor terbit, pekerjaan bisnis
        /// pemanggil dibatalkan, dan pencacah <b>tetap naik</b>. Nomor yang hangus tidak pernah
        /// diterbitkan lagi (<c>DEC-PLT-008</c>, <c>INV-PLT-001</c>).
        /// </summary>
        /// <remarks>
        /// Inilah pembeda alokator ini dari mesin Billing yang diekstrak. Bila uji ini gagal,
        /// artinya pencacah masih ikut dibatalkan bersama transaksi pemanggil — perilaku
        /// <c>CONF-PLT-002</c> yang justru hendak diperbaiki.
        /// </remarks>
        [Fact]
        public async Task PekerjaanPemanggilDibatalkan_PencacahTetapNaik_NomorHangus()
        {
            var deret = UniqueSequenceKey("DURABILITAS");
            var alokator = CreateAllocator();

            string nomorPertama;

            // Transaksi bisnis pemanggil, memakai konteks yang BERBEDA dari milik alokator.
            await using (var pemanggil = _fixture.CreateContext())
            await using (var transaksiBisnis = await pemanggil.Database.BeginTransactionAsync())
            {
                nomorPertama = await alokator.AllocateAsync(Permintaan(deret, "DUR"));

                // Pekerjaan bisnis batal. Nomor sudah terlanjur terbit.
                await transaksiBisnis.RollbackAsync();
            }

            var nomorKedua = await alokator.AllocateAsync(Permintaan(deret, "DUR"));

            Assert.Equal("DUR-00000001", nomorPertama);

            // Yang membuktikan durabilitas: BUKAN 00000001 lagi.
            Assert.Equal("DUR-00000002", nomorKedua);
            Assert.NotEqual(nomorPertama, nomorKedua);

            await using var pemeriksa = _fixture.CreateContext();
            var baris = await pemeriksa.NumNumberSeries.SingleAsync(x => x.SequenceKey == deret);
            Assert.Equal(2, baris.CurrentValue);
        }

        /// <summary>
        /// Pekerjaan pemanggil yang <b>berhasil</b> tetap menghasilkan nomor berurutan — penjaga
        /// agar perbaikan durabilitas tidak diam-diam merusak jalur normal.
        /// </summary>
        [Fact]
        public async Task PekerjaanPemanggilBerhasil_NomorTetapBerurutan()
        {
            var deret = UniqueSequenceKey("NORMAL");
            var alokator = CreateAllocator();

            string pertama, kedua;

            await using (var pemanggil = _fixture.CreateContext())
            await using (var transaksiBisnis = await pemanggil.Database.BeginTransactionAsync())
            {
                pertama = await alokator.AllocateAsync(Permintaan(deret, "NRM"));
                await transaksiBisnis.CommitAsync();
            }

            kedua = await alokator.AllocateAsync(Permintaan(deret, "NRM"));

            Assert.Equal("NRM-00000001", pertama);
            Assert.Equal("NRM-00000002", kedua);
        }

        // =================================================================
        // AC-PLT-004 — antrean pada deret yang sama
        // =================================================================

        /// <summary>
        /// <c>AC-PLT-004</c> — dua puluh alokasi bersamaan pada deret yang sama menghasilkan
        /// <b>dua puluh nomor berbeda</b>, tanpa satu pun kegagalan.
        /// </summary>
        /// <remarks>
        /// Kunci penasihat <b>mengantre</b>, bukan menolak. Bila uji ini memulangkan nomor kembar,
        /// artinya <c>INV-PLT-001</c> dilanggar; bila memulangkan kegagalan, artinya kunci tidak
        /// mengantre sebagaimana dirancang.
        /// </remarks>
        [Fact]
        public async Task DuaPuluhAlokasiBersamaan_DeretSama_MenghasilkanNomorBerbedaSemua()
        {
            const int jumlah = 20;

            var deret = UniqueSequenceKey("REBUT");
            var alokator = CreateAllocator();

            var tugas = Enumerable
                .Range(0, jumlah)
                .Select(_ => alokator.AllocateAsync(Permintaan(deret, "RBT")))
                .ToArray();

            var nomor = await Task.WhenAll(tugas);

            Assert.Equal(jumlah, nomor.Length);
            Assert.Equal(jumlah, nomor.Distinct().Count());

            await using var pemeriksa = _fixture.CreateContext();
            var baris = await pemeriksa.NumNumberSeries.SingleAsync(x => x.SequenceKey == deret);
            Assert.Equal(jumlah, baris.CurrentValue);
        }

        // =================================================================
        // AC-PLT-005 — deret berbeda tidak saling menunggu
        // =================================================================

        /// <summary>
        /// <c>AC-PLT-005</c> — alokasi pada deret berbeda <b>tidak</b> ikut menunggu.
        /// </summary>
        /// <remarks>
        /// <b>Dibuktikan deterministik, bukan lewat pengukuran waktu.</b> Sebuah transaksi
        /// menahan kunci penasihat milik deret A dan sengaja tidak dilepas. Selama penahanan itu,
        /// alokasi pada deret B tetap harus selesai. Bila kuncinya ternyata global — bukan per
        /// deret — alokasi B akan menggantung dan uji ini gagal lewat batas waktunya sendiri.
        /// </remarks>
        [Fact]
        public async Task DeretBerbeda_TidakSalingMenunggu()
        {
            var deretA = UniqueSequenceKey("KUNCI_A");
            var deretB = UniqueSequenceKey("KUNCI_B");
            var alokator = CreateAllocator();

            await using var penahan = _fixture.CreateContext();
            await using var transaksiPenahan = await penahan.Database.BeginTransactionAsync();

            // Menahan kunci milik deret A dengan bentuk kunci yang sama persis dengan alokator.
            await penahan.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"NUM_SERIES_{deretA}_GLOBAL"]);

            using var batasWaktu = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            // Deret B harus tetap selesai walau kunci deret A sedang ditahan.
            var nomorB = await alokator.AllocateAsync(Permintaan(deretB, "KNB"), batasWaktu.Token);

            Assert.Equal("KNB-00000001", nomorB);

            await transaksiPenahan.RollbackAsync();
        }

        // =================================================================
        // AC-PLT-012 — empat deret Billing tidak berubah perilakunya
        // =================================================================

        /// <summary>
        /// <c>AC-PLT-012</c> — deret Billing <b>tetap memakai ulang</b> nomor ketika transaksi
        /// pemanggil dibatalkan, persis seperti sebelum slice ini.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Uji ini sengaja membuktikan perilaku yang <b>berlawanan</b> dengan
        /// <c>AC-PLT-003</c>, dan itu memang yang dituntut <c>DEC-PLT-003</c> dan
        /// <c>INV-PLT-003</c>: selama masa peralihan, satu deret dilayani tepat satu mekanisme.
        /// Keempat deret Billing masih dilayani mesin lama sampai <c>PLT-SLICE-02</c>
        /// memindahkannya.
        /// </para>
        /// <para>
        /// Bila uji ini <b>gagal</b> — yaitu nomor Billing ternyata ikut hangus — artinya slice
        /// ini diam-diam mengubah perilaku empat deret produksi, dan itu justru kekhawatiran
        /// yang dicatat <c>NOTE-PLT-001</c>.
        /// </para>
        /// </remarks>
        [Fact]
        public async Task DeretBilling_TetapMemakaiUlangNomorSaatTransaksiDibatalkan()
        {
            await using var context = _fixture.CreateContext();

            var mesinLama = new BillingNumberSeriesService(
                context,
                Options.Create(new BillingInvoiceNumberOptions
                {
                    Prefix = "UJIBIL",
                    ResetPolicy = BillingNumberResetPolicies.Never,
                    SequenceDigits = 8
                }));

            var instant = DateTimeOffset.UtcNow;
            string pertama;

            await using (var transaksi = await context.Database.BeginTransactionAsync())
            {
                pertama = await mesinLama.AllocateInvoiceNumberAsync(ActorUserId, instant, default);
                await transaksi.RollbackAsync();
            }

            context.ChangeTracker.Clear();

            string kedua;

            await using (var transaksi = await context.Database.BeginTransactionAsync())
            {
                kedua = await mesinLama.AllocateInvoiceNumberAsync(ActorUserId, instant, default);
                await transaksi.RollbackAsync();
            }

            // Perilaku LAMA dipertahankan: nomor yang sama terbit lagi.
            Assert.Equal(pertama, kedua);
        }

        /// <summary>
        /// Tabel Billing dan tabel Platform benar-benar terpisah — nol baris Platform lahir dari
        /// alokasi Billing, dan sebaliknya.
        /// </summary>
        [Fact]
        public async Task TabelBillingDanPlatform_Terpisah()
        {
            var deret = UniqueSequenceKey("PISAH");
            var alokator = CreateAllocator();

            await alokator.AllocateAsync(Permintaan(deret, "PSH"));

            await using var context = _fixture.CreateContext();

            var adaDiPlatform = await context.NumNumberSeries.AnyAsync(x => x.SequenceKey == deret);
            var adaDiBilling = await context.Set<BilNumberSeries>().AnyAsync(x => x.SequenceKey == deret);

            Assert.True(adaDiPlatform);
            Assert.False(adaDiBilling);
        }

        // =================================================================
        // Penolong
        // =================================================================

        /// <summary>
        /// Penanda deret unik per pemanggilan. Fixture dipakai bersama seluruh berkas uji, dan
        /// pencacah tidak pernah dihapus (<c>INV-PLT-001</c>), sehingga uji tidak boleh
        /// mengandalkan tabel yang kosong.
        /// </summary>
        private static string UniqueSequenceKey(string label)
            => $"PLT_UJI_{label}_{Guid.NewGuid():N}"[..Math.Min(50, $"PLT_UJI_{label}_{Guid.NewGuid():N}".Length)];

        private static NumberAllocationRequest Permintaan(string deret, string awalan)
            => new(deret, awalan, NumberSeriesResetPolicies.Never, 8, ActorUserId, DateTimeOffset.UtcNow);

        private NumberSeriesAllocator CreateAllocator()
            => new(new FixtureContextFactory(_fixture));

        /// <summary>
        /// Factory yang memberi alokator konteks dengan <b>koneksi tersendiri</b>, sama seperti
        /// pendaftaran <c>AddDbContextFactory</c> pada aplikasi.
        /// </summary>
        private sealed class FixtureContextFactory : IDbContextFactory<ApplicationDbContext>
        {
            private readonly BillingTestDatabaseFixture _fixture;

            public FixtureContextFactory(BillingTestDatabaseFixture fixture)
            {
                _fixture = fixture;
            }

            public ApplicationDbContext CreateDbContext() => _fixture.CreateContext();
        }
    }
}
