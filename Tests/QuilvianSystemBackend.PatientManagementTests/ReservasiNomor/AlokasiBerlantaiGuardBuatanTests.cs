using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.ReservasiNomor;

/// <summary>
/// <c>AllocateReservedAsync</c> tidak memercayai guard buatan pemanggil — temuan review final
/// `F-07` `BE-PAT-MIG-003A`, `PAT-DEC-070` (b).
/// </summary>
/// <remarks>
/// Setiap penolakan wajib terjadi sebelum SQL apa pun: tanpa kunci, tanpa membaca nomor terpakai,
/// tanpa alokasi, dan tanpa mutasi pasien maupun <c>NumberSeries</c>. Transaksi sengaja dibuka lebih
/// dulu — seperti pemanggil sungguhan — supaya terbukti penolakan tidak bergantung pada ketiadaan
/// transaksi.
/// </remarks>
public class AlokasiBerlantaiGuardBuatanTests
{
    // ---- T-F07-01: runtime Production ----------------------------------------------------------

    [Theory]
    [InlineData("Staging")]
    [InlineData("Production")]
    public async Task T_F07_01_RuntimeProduction_GuardAktifBuatan_DitolakSebelumSql(string environmentGuard)
    {
        using var h = new ReservasiHarness("Production", KonfigurasiGuardUji.EnabledTrue);

        await AssertDitolakSebelumSqlAsync(h, GuardAktifBuatan(environmentGuard));
    }

    // ---- T-F07-02: runtime Development ---------------------------------------------------------

    [Theory]
    [InlineData("Staging")]
    [InlineData("Development")]
    public async Task T_F07_02_RuntimeDevelopment_GuardAktifBuatan_DitolakSebelumSql(string environmentGuard)
    {
        using var h = new ReservasiHarness("Development", KonfigurasiGuardUji.EnabledTrue);

        await AssertDitolakSebelumSqlAsync(h, GuardAktifBuatan(environmentGuard));
    }

    // ---- T-F07-03: runtime "staging" (huruf kecil) ---------------------------------------------

    [Theory]
    [InlineData("Staging")]
    [InlineData("staging")]
    public async Task T_F07_03_RuntimeStagingHurufKecil_GuardAktifBuatan_DitolakSebelumSql(string environmentGuard)
    {
        using var h = new ReservasiHarness("staging", KonfigurasiGuardUji.EnabledTrue);

        await AssertDitolakSebelumSqlAsync(h, GuardAktifBuatan(environmentGuard));
    }

    // ---- T-F07-04: runtime Staging persis, asal guard tidak cocok ------------------------------

    [Theory]
    [InlineData("staging")]
    [InlineData("STAGING")]
    [InlineData("Staging ")]
    [InlineData("Production")]
    [InlineData("")]
    [InlineData(null)]
    public async Task T_F07_04_RuntimeStagingPersis_EnvironmentGuardTidakCocok_DitolakSebelumSql(string? environmentGuard)
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);

        await AssertDitolakSebelumSqlAsync(h, GuardAktifBuatan(environmentGuard));
    }

    // ---- Pertahanan berlapis: konfigurasi Staging saat ini tidak aktif ------------------------

    [Theory]
    [InlineData(KonfigurasiGuardUji.EnabledFalse)]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda)]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk)]
    public async Task T_F07_06_RuntimeStagingPersis_KonfigurasiTidakAktif_GuardAktifBuatan_DitolakSebelumSql(
        KonfigurasiGuardUji konfigurasi)
    {
        using var h = new ReservasiHarness("Staging", konfigurasi);

        await AssertDitolakSebelumSqlAsync(h, GuardAktifBuatan("Staging"));
    }

    [Fact]
    public async Task GuardNull_DitolakSebelumSql()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var perintah = new PenghitungPerintah();
        await using var konteks = h.Database.CreateContext(perintah);
        await using var transaksi = await konteks.Database.BeginTransactionAsync();
        var sebelum = perintah.Jumlah;

        await Assert.ThrowsAsync<ArgumentNullException>(() => h.BuatService(konteks).AllocateReservedAsync(null!));

        Assert.Equal(sebelum, perintah.Jumlah);
    }

    // ---- T-F07-05: Staging persis + guard hasil evaluasi tetap berjalan -----------------------

    [Fact]
    public async Task T_F07_05_StagingPersis_GuardHasilEvaluasi_TetapMemberiNomorBerlantai()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-00741", "00-00-07-41");
        h.TambahPasien("PAT-RSMMC-705249", "00-89-01-87");
        var perintah = new PenghitungPerintah();

        await using var konteks = h.Database.CreateContext(perintah);
        await using var transaksi = await konteks.Database.BeginTransactionAsync();
        var sebelum = perintah.Jumlah;
        var service = h.BuatService(konteks);

        var hasil = await service.AllocateReservedAsync(service.EvaluateGuard());

        Assert.True(hasil.Succeeded);
        Assert.Equal("PAT-RSMMC-705250", hasil.PatientCode);
        Assert.Equal("00-89-01-88", hasil.MedicalRecordNumber);
        // Pembanding: jalur sah memang menjalankan SQL, sehingga hitungan nol pada uji penolakan bermakna.
        Assert.True(perintah.Jumlah > sebelum);
    }

    // ---- Pembantu -------------------------------------------------------------------------

    private static RsmmcReservationGuardEvaluation GuardAktifBuatan(string? environmentName) =>
        new(environmentName, RsmmcReservationGuardState.Active);

    private static async Task AssertDitolakSebelumSqlAsync(
        ReservasiHarness h,
        RsmmcReservationGuardEvaluation guardBuatan)
    {
        h.TambahPasien("PAT-RSMMC-00741", "00-00-07-41");
        var pasienSebelum = h.JumlahPasien();
        var perintah = new PenghitungPerintah();
        PatientIdentifierAllocationResult? hasil = null;

        await using (var konteks = h.Database.CreateContext(perintah))
        {
            await using var transaksi = await konteks.Database.BeginTransactionAsync();
            var sebelum = perintah.Jumlah;

            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                hasil = await h.BuatService(konteks).AllocateReservedAsync(guardBuatan));

            // 0 perintah SQL: tanpa kunci, tanpa membaca nomor terpakai.
            Assert.Equal(sebelum, perintah.Jumlah);
            await transaksi.RollbackAsync();
        }

        // 0 alokasi, 0 mutasi pasien, 0 mutasi NumberSeries.
        Assert.Null(hasil);
        Assert.Equal(pasienSebelum, h.JumlahPasien());

        await using var baca = h.Database.CreateContext();
        Assert.Equal(0, await BayiUji.JumlahDeretAsync(baca));
    }
}
