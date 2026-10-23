using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.ReservasiNomor;

/// <summary>
/// Guard reservasi nomor pada jalur pendaftaran bayi `POST /newborns` — `BE-PAT-MIG-003A`,
/// `PAT-DEC-069`–`072`, kontrak `1.3.0` B14.2 dan B14.4.
/// </summary>
/// <remarks>
/// SQLite tidak dapat membuktikan kunci PostgreSQL maupun menjalankan allocator platform
/// <c>NumberSeries</c> (transaksi kedua pada koneksi yang sama). Bagian itu dibuktikan
/// <see cref="PendaftaranBayiPostgresSungguhanTests"/>; di sini dibuktikan cabang, gagal tertutup,
/// lantai, urutan normal↔bayi, idempotency, urutan kelahiran, dan pemeriksa.
/// </remarks>
public class PendaftaranBayiReservasiTests
{
    private const string KunciIdempotency = "uji-bayi-mig3a-0001";

    // ---- T3, T4, T5: Staging persis tanpa guard sah → gagal tertutup ------------------------

    [Theory]
    [InlineData(KonfigurasiGuardUji.EnabledFalse, "DISABLED")]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.KunciEnabledTidakAda, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk, "CONFIG_MALFORMED")]
    public async Task T3_T4_T5_StagingTidakSah_GagalTertutupSebelumAksesBasisData(
        KonfigurasiGuardUji konfigurasi,
        string keadaan)
    {
        using var h = new ReservasiHarness("Staging", konfigurasi);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00001", "00-00-00-01"));
        var pasienSebelum = h.JumlahPasien();
        var pabrik = new PabrikNumberSeriesTerlarang();
        var perintah = new PenghitungPerintah();

        NewbornRegistrationResult hasil;
        await using (var konteks = h.Database.CreateContext(perintah))
        {
            hasil = await BayiUji.BuatLayanan(konteks, h.BuatService(konteks), pabrik)
                .RegisterAsync(BayiUji.Permintaan(kasus), KunciIdempotency, ReservasiHarness.AktorUji);
        }

        Assert.Equal(NewbornRegistrationStatus.ReservationGuardRejected, hasil.Status);
        Assert.Null(hasil.Data);
        Assert.Equal(keadaan, hasil.ReservationGuard!.StateCode);
        Assert.Equal(0, perintah.Jumlah);
        Assert.Equal(0, pabrik.JumlahPemakaian);
        Assert.Equal(pasienSebelum, h.JumlahPasien());
        await AssertTanpaDeretDanUrutanAsync(h);
    }

    [Fact]
    public async Task T5_StagingKonfigurasiTidakTerbaca_GagalTertutup()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00001", "00-00-00-01"));
        h.Configuration = new KonfigurasiTidakTerbaca();
        var pabrik = new PabrikNumberSeriesTerlarang();

        await using var konteks = h.Database.CreateContext();
        var hasil = await BayiUji.BuatLayanan(konteks, h.BuatService(konteks), pabrik)
            .RegisterAsync(BayiUji.Permintaan(kasus), KunciIdempotency, ReservasiHarness.AktorUji);

        Assert.Equal(NewbornRegistrationStatus.ReservationGuardRejected, hasil.Status);
        Assert.Equal("CONFIG_MALFORMED", hasil.ReservationGuard!.StateCode);
        Assert.Equal(0, pabrik.JumlahPemakaian);
        Assert.Equal(1, h.JumlahPasien());
    }

    [Theory]
    [InlineData(KonfigurasiGuardUji.EnabledFalse, "DISABLED")]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk, "CONFIG_MALFORMED")]
    public async Task Rute_StagingTidakSah_Ditolak503DenganKodeGalatYangSama(
        KonfigurasiGuardUji konfigurasi,
        string keadaan)
    {
        using var h = new ReservasiHarness("Staging", konfigurasi);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00001", "00-00-00-01"));

        await using var konteks = h.Database.CreateContext();
        var controller = h.BuatController(
            konteks,
            BayiUji.BuatLayanan(konteks, h.BuatService(konteks), new PabrikNumberSeriesTerlarang()));

        var objek = Assert.IsAssignableFrom<ObjectResult>(
            await controller.CreateNewbornPatient(BayiUji.Permintaan(kasus), KunciIdempotency));
        var body = Assert.IsType<ApiResponse<object>>(objek.Value);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, objek.StatusCode);
        Assert.Equal("PATIENT_RESERVATION_GUARD_CONFIG_INVALID", Properti(body.Errors, "code"));
        Assert.Equal(keadaan, Properti(body.Errors, "guardState"));
        Assert.Equal(1, h.JumlahPasien());
        Assert.False(Directory.Exists(Path.Combine(h.StorageRoot, "patient-qrcodes")));
    }

    // ---- T6: Staging persis, Enabled = true → lantai, tanpa NumberSeries ---------------------

    [Fact]
    public async Task T6_StagingAktif_NomorBayiDiAtasLantaiTanpaNumberSeries()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        // Ibu memakai kode Pilot 741; dua nomor tepat di bawah lantai milik perencana migrasi.
        var kasus = await SiapkanAsync(
            h,
            ("PAT-RSMMC-00741", "00-00-07-41"),
            ("PAT-RSMMC-705249", "00-89-01-87"),
            ("PAT-RSMMC-00742", "00-00-00-27"));
        var pabrik = new PabrikNumberSeriesTerlarang();

        await using var konteks = h.Database.CreateContext();
        var hasil = await BayiUji.BuatLayanan(konteks, h.BuatService(konteks), pabrik)
            .RegisterAsync(BayiUji.Permintaan(kasus), KunciIdempotency, ReservasiHarness.AktorUji);

        Assert.Equal(NewbornRegistrationStatus.Success, hasil.Status);
        Assert.Equal("PAT-RSMMC-705250", hasil.Data!.PatientCode);
        Assert.Equal("00-89-01-88", hasil.Data.MedicalRecordNumber);
        Assert.Equal(1, hasil.Data.BirthOrder);
        Assert.True(hasil.Data.IsNewborn);
        Assert.Equal("ACTIVE", hasil.ReservationGuard!.StateCode);
        Assert.Equal(0, pabrik.JumlahPemakaian);

        await using var baca = h.Database.CreateContext();
        Assert.Equal(0, await BayiUji.JumlahDeretAsync(baca));
        var bayi = await baca.Set<MstPatient>().AsNoTracking().SingleAsync(x => x.Id == hasil.Data.Id);
        Assert.False(bayi.IsMember);
        Assert.Equal(kasus.KasusId, bayi.DeliveryCaseId);
    }

    [Fact]
    public async Task T6_StagingAktif_CelahDiAtasLantai_NomorKosongPertama()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var kasus = await SiapkanAsync(
            h,
            ("PAT-RSMMC-705250", "00-89-01-88"),
            ("PAT-RSMMC-705252", "00-89-01-90"),
            ("PAT-RSMMC-0705251", "00890189"));

        await using var konteks = h.Database.CreateContext();
        var hasil = await BayiUji.BuatLayanan(konteks, h.BuatService(konteks), new PabrikNumberSeriesTerlarang())
            .RegisterAsync(BayiUji.Permintaan(kasus), KunciIdempotency, ReservasiHarness.AktorUji);

        Assert.Equal("PAT-RSMMC-705253", hasil.Data!.PatientCode);
        Assert.Equal("00-89-01-91", hasil.Data.MedicalRecordNumber);
    }

    // ---- T1, T2 (cabang): di luar Staging persis tetap NumberSeries ------------------------

    [Theory]
    [InlineData("Production", KonfigurasiGuardUji.BagianTidakAda)]
    [InlineData("Production", KonfigurasiGuardUji.EnabledFalse)]
    [InlineData("Production", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("Development", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("staging", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("Test", KonfigurasiGuardUji.EnabledSalahBentuk)]
    public async Task T1_T2_BukanStagingPersis_MemakaiNumberSeriesSepertiSebelumnya(
        string environmentName,
        KonfigurasiGuardUji konfigurasi)
    {
        using var h = new ReservasiHarness(environmentName, konfigurasi);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));
        var pabrik = new PabrikNumberSeriesTerlarang();

        await using var konteks = h.Database.CreateContext();
        await Assert.ThrowsAsync<NumberSeriesDipakaiException>(() =>
            BayiUji.BuatLayanan(konteks, h.BuatService(konteks), pabrik)
                .RegisterAsync(BayiUji.Permintaan(kasus), KunciIdempotency, ReservasiHarness.AktorUji));

        // Cabang NumberSeries tercapai; transaksi bayi dibatalkan sehingga tidak ada pasien baru.
        Assert.Equal(1, pabrik.JumlahPemakaian);
        Assert.Equal(1, h.JumlahPasien());
    }

    [Fact]
    public async Task T2_BukanStaging_KonfigurasiGuardTidakDibaca()
    {
        using var h = new ReservasiHarness("Production", KonfigurasiGuardUji.BagianTidakAda);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));
        h.Configuration = new KonfigurasiTidakTerbaca();
        var pabrik = new PabrikNumberSeriesTerlarang();

        await using var konteks = h.Database.CreateContext();
        await Assert.ThrowsAsync<NumberSeriesDipakaiException>(() =>
            BayiUji.BuatLayanan(konteks, h.BuatService(konteks), pabrik)
                .RegisterAsync(BayiUji.Permintaan(kasus), KunciIdempotency, ReservasiHarness.AktorUji));

        Assert.Equal(1, pabrik.JumlahPemakaian);
    }

    // ---- T7: normal dan bayi tidak pernah mendapat nomor yang sama -------------------------

    [Fact]
    public async Task T7_NormalLaluBayi_NomorBerbedaDanBerurutanDiAtasLantai()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));

        var normal = await DaftarNormalAsync(h);
        var bayi = await DaftarBayiAsync(h, kasus, KunciIdempotency);

        Assert.Equal(("PAT-RSMMC-705250", "00-89-01-88"), normal);
        Assert.Equal("PAT-RSMMC-705251", bayi.Data!.PatientCode);
        Assert.Equal("00-89-01-89", bayi.Data.MedicalRecordNumber);
    }

    [Fact]
    public async Task T7_BayiLaluNormal_NomorBerbedaDanBerurutanDiAtasLantai()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));

        var bayi = await DaftarBayiAsync(h, kasus, KunciIdempotency);
        var normal = await DaftarNormalAsync(h);

        Assert.Equal("PAT-RSMMC-705250", bayi.Data!.PatientCode);
        Assert.Equal("00-89-01-88", bayi.Data.MedicalRecordNumber);
        Assert.Equal(("PAT-RSMMC-705251", "00-89-01-89"), normal);
    }

    // ---- T15: idempotency bayi tidak berubah ----------------------------------------------

    [Fact]
    public async Task T15_StagingAktif_KunciSamaDataSama_HasilSebelumnyaTanpaNomorBaru()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));

        var pertama = await DaftarBayiAsync(h, kasus, KunciIdempotency);
        var ulang = await DaftarBayiAsync(h, kasus, KunciIdempotency);
        var berikutnya = await DaftarNormalAsync(h);

        Assert.Equal(NewbornRegistrationStatus.Success, ulang.Status);
        Assert.False(pertama.Data!.WasExisting);
        Assert.True(ulang.Data!.WasExisting);
        Assert.Equal(pertama.Data.Id, ulang.Data.Id);
        Assert.Equal(pertama.Data.PatientCode, ulang.Data.PatientCode);
        Assert.Equal(pertama.Data.MedicalRecordNumber, ulang.Data.MedicalRecordNumber);
        // Pemutaran ulang tidak menghabiskan nomor: pendaftaran berikutnya tepat sesudah bayi pertama.
        Assert.Equal(("PAT-RSMMC-705251", "00-89-01-89"), berikutnya);
        Assert.Equal(3, h.JumlahPasien());
    }

    [Fact]
    public async Task T15_StagingAktif_KunciSamaDataBerbeda_KonflikTanpaPasienBaru()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));

        await DaftarBayiAsync(h, kasus, KunciIdempotency);
        var berbeda = await DaftarBayiAsync(h, kasus, KunciIdempotency, tempatLahir: "RS Lain");

        Assert.Equal(NewbornRegistrationStatus.Conflict, berbeda.Status);
        Assert.Null(berbeda.Data);
        Assert.Equal(2, h.JumlahPasien());
    }

    // ---- T16: urutan kelahiran per kasus tidak berubah ------------------------------------

    [Fact]
    public async Task T16_StagingAktif_DuaBayiSatuKasus_UrutanKelahiranSatuDanDua()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        var kasus = await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));

        var pertama = await DaftarBayiAsync(h, kasus, "uji-bayi-kembar-0001");
        var kedua = await DaftarBayiAsync(h, kasus, "uji-bayi-kembar-0002", tempatLahir: "RS Uji Kembar");

        Assert.Equal(1, pertama.Data!.BirthOrder);
        Assert.Equal(2, kedua.Data!.BirthOrder);
        Assert.Equal("PAT-RSMMC-705250", pertama.Data.PatientCode);
        Assert.Equal("PAT-RSMMC-705251", kedua.Data.PatientCode);

        await using var baca = h.Database.CreateContext();
        var urutan = await baca.PatNewbornBirthSequences.AsNoTracking().SingleAsync();
        Assert.Equal(kasus.KasusId, urutan.DeliveryCaseId);
        Assert.Equal(2, urutan.LastAllocatedBirthOrder);
    }

    // ---- T11, T12, T14: pemeriksa melaporkan kedua jalur ----------------------------------

    [Fact]
    public async Task T11_T12_T14_Pemeriksa_StagingAktif_KeduaJalurTerlindungiTanpaMutasi()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        await SiapkanAsync(h, ("PAT-RSMMC-00741", "00-00-07-41"));
        var pasienSebelum = h.JumlahPasien();
        var berkasSebelum = h.JumlahBerkasUnggahan();

        await using var konteks = h.Database.CreateContext();
        var controller = h.BuatController(
            konteks,
            BayiUji.BuatLayanan(konteks, h.BuatService(konteks), new PabrikNumberSeriesTerlarang()));
        var objek = Assert.IsType<OkObjectResult>(await controller.GetRsmmcReservationGuardStatus(CancellationToken.None));
        var data = Assert.IsType<ApiResponse<RsmmcReservationGuardStatusResponse>>(objek.Value).Data!;

        Assert.Equal("Staging", data.Environment);
        Assert.Equal("ACTIVE", data.GuardState);
        Assert.True(data.GuardEnabled);
        Assert.True(data.GuardConfigurationValid);
        Assert.True(data.NormalPatientCreateProtected);
        Assert.True(data.NewbornPatientCreateProtected);
        Assert.Equal("RESERVED_FLOOR", data.NormalPatientCreateAllocationMode);
        Assert.Equal("RESERVED_FLOOR", data.NewbornPatientCreateAllocationMode);
        Assert.True(data.PatientCodeFloorSatisfied);
        Assert.True(data.MrnFloorSatisfied);
        Assert.Equal("PAT-RSMMC-705250", data.NextPatientCodeFormatted);
        Assert.Equal("00-89-01-88", data.NextMrnFormatted);
        Assert.True(data.OverallGuardReady);

        // T14: tidak ada pasien, berkas, deret, maupun urutan kelahiran yang dibuat.
        Assert.Equal(pasienSebelum, h.JumlahPasien());
        Assert.Equal(berkasSebelum, h.JumlahBerkasUnggahan());
        await AssertTanpaDeretDanUrutanAsync(h);
    }

    [Theory]
    [InlineData(KonfigurasiGuardUji.EnabledFalse, "DISABLED")]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk, "CONFIG_MALFORMED")]
    public async Task T11_T12_Pemeriksa_StagingTidakSah_JalurDitolakTetapiBelumSiap(
        KonfigurasiGuardUji konfigurasi,
        string keadaan)
    {
        using var h = new ReservasiHarness("Staging", konfigurasi);

        await using var konteks = h.Database.CreateContext();
        var status = await h.BuatService(konteks).CheckAsync();

        Assert.Equal(keadaan, status.GuardState);
        Assert.Equal("FAIL_CLOSED", status.NormalPatientCreateAllocationMode);
        Assert.Equal("FAIL_CLOSED", status.NewbornPatientCreateAllocationMode);
        // Kedua jalur menolak sebelum alokasi, sehingga tidak dapat memakai nomor cadangan…
        Assert.True(status.NormalPatientCreateProtected);
        Assert.True(status.NewbornPatientCreateProtected);
        // …tetapi pendaftaran tidak tersedia, sehingga gerbang belum lulus.
        Assert.False(status.OverallGuardReady);
    }

    [Fact]
    public async Task Pemeriksa_BukanStaging_KeduaJalurLegacyDanTidakSiap()
    {
        using var h = new ReservasiHarness("Production", KonfigurasiGuardUji.EnabledTrue);

        await using var konteks = h.Database.CreateContext();
        var status = await h.BuatService(konteks).CheckAsync();

        Assert.Equal("NOT_APPLICABLE", status.GuardState);
        Assert.Equal("LEGACY", status.NormalPatientCreateAllocationMode);
        Assert.Equal("LEGACY", status.NewbornPatientCreateAllocationMode);
        Assert.False(status.NormalPatientCreateProtected);
        Assert.False(status.NewbornPatientCreateProtected);
        Assert.False(status.OverallGuardReady);
    }

    // ---- T13: gerbang tidak lulus bila salah satu jalur tidak aman ------------------------

    [Theory]
    [InlineData(RsmmcIdentifierAllocationMode.ReservedFloor, RsmmcIdentifierAllocationMode.ReservedFloor, true, true, true)]
    [InlineData(RsmmcIdentifierAllocationMode.ReservedFloor, RsmmcIdentifierAllocationMode.Legacy, true, true, false)]
    [InlineData(RsmmcIdentifierAllocationMode.Legacy, RsmmcIdentifierAllocationMode.ReservedFloor, true, true, false)]
    [InlineData(RsmmcIdentifierAllocationMode.ReservedFloor, RsmmcIdentifierAllocationMode.FailClosed, true, true, false)]
    [InlineData(RsmmcIdentifierAllocationMode.FailClosed, RsmmcIdentifierAllocationMode.ReservedFloor, true, true, false)]
    [InlineData(RsmmcIdentifierAllocationMode.ReservedFloor, RsmmcIdentifierAllocationMode.ReservedFloor, false, true, false)]
    [InlineData(RsmmcIdentifierAllocationMode.ReservedFloor, RsmmcIdentifierAllocationMode.ReservedFloor, true, false, false)]
    public void T13_GuardAktif_SiapHanyaBilaKeduaJalurBerlantaiDanKeduaLantaiTerpenuhi(
        RsmmcIdentifierAllocationMode normal,
        RsmmcIdentifierAllocationMode bayi,
        bool lantaiKode,
        bool lantaiMrn,
        bool siap)
    {
        var guard = new RsmmcReservationGuardEvaluation("Staging", RsmmcReservationGuardState.Active);

        Assert.Equal(siap, RsmmcIdentifierReservationProfile.IsGuardReady(guard, normal, bayi, lantaiKode, lantaiMrn));
    }

    [Theory]
    [InlineData(RsmmcReservationGuardState.Disabled)]
    [InlineData(RsmmcReservationGuardState.ConfigMissing)]
    [InlineData(RsmmcReservationGuardState.ConfigMalformed)]
    [InlineData(RsmmcReservationGuardState.NotApplicable)]
    public void T13_GuardTidakAktif_TidakPernahSiap(RsmmcReservationGuardState keadaan)
    {
        var guard = new RsmmcReservationGuardEvaluation("Staging", keadaan);

        Assert.False(RsmmcIdentifierReservationProfile.IsGuardReady(
            guard,
            RsmmcIdentifierAllocationMode.ReservedFloor,
            RsmmcIdentifierAllocationMode.ReservedFloor,
            patientCodeFloorSatisfied: true,
            medicalRecordNumberFloorSatisfied: true));
    }

    [Theory]
    [InlineData(RsmmcReservationGuardState.NotApplicable, RsmmcIdentifierAllocationMode.Legacy)]
    [InlineData(RsmmcReservationGuardState.Active, RsmmcIdentifierAllocationMode.ReservedFloor)]
    [InlineData(RsmmcReservationGuardState.Disabled, RsmmcIdentifierAllocationMode.FailClosed)]
    [InlineData(RsmmcReservationGuardState.ConfigMissing, RsmmcIdentifierAllocationMode.FailClosed)]
    [InlineData(RsmmcReservationGuardState.ConfigMalformed, RsmmcIdentifierAllocationMode.FailClosed)]
    public void CaraAlokasiBayi_SamaDenganPendaftaranNormal(
        RsmmcReservationGuardState keadaan,
        RsmmcIdentifierAllocationMode diharapkan)
    {
        var guard = new RsmmcReservationGuardEvaluation("Staging", keadaan);
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        using var konteks = h.Database.CreateContext();

        Assert.Equal(diharapkan, NewbornPatientRegistrationService.ResolveIdentifierAllocationMode(guard));
        Assert.Equal(diharapkan, h.BuatService(konteks).ResolveAllocationMode(guard));
    }

    [Theory]
    [InlineData(RsmmcReservationGuardState.NotApplicable)]
    [InlineData(RsmmcReservationGuardState.Disabled)]
    [InlineData(RsmmcReservationGuardState.ConfigMissing)]
    public async Task AlokasiBerlantai_MenolakGuardYangTidakAktif(RsmmcReservationGuardState keadaan)
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        await using var konteks = h.Database.CreateContext();
        await using var transaksi = await konteks.Database.BeginTransactionAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            h.BuatService(konteks).AllocateReservedAsync(new RsmmcReservationGuardEvaluation("Staging", keadaan)));
    }

    // ---- Pembantu -------------------------------------------------------------------------

    private static async Task<KasusPersalinanUji> SiapkanAsync(
        ReservasiHarness h,
        (string Code, string Mrn) ibu,
        params (string Code, string Mrn)[] pasienLain)
    {
        foreach (var (code, mrn) in pasienLain) h.TambahPasien(code, mrn);

        await using var konteks = h.Database.CreateContext();
        return await BayiUji.TambahIbuDanKasusAsync(konteks, ibu.Code, ibu.Mrn);
    }

    private static async Task<NewbornRegistrationResult> DaftarBayiAsync(
        ReservasiHarness h,
        KasusPersalinanUji kasus,
        string kunci,
        string tempatLahir = "RS Uji")
    {
        await using var konteks = h.Database.CreateContext();

        return await BayiUji.BuatLayanan(konteks, h.BuatService(konteks), new PabrikNumberSeriesTerlarang())
            .RegisterAsync(BayiUji.Permintaan(kasus, tempatLahir), kunci, ReservasiHarness.AktorUji);
    }

    private static async Task<(string PatientCode, string MedicalRecordNumber)> DaftarNormalAsync(ReservasiHarness h)
    {
        await using var konteks = h.Database.CreateContext();
        var hasil = await h.BuatController(konteks).CreatePatient(new CreatePatientRequest
        {
            FullName = "Pasien Normal Uji",
            BirthDate = new DateTime(1990, 5, 17, 0, 0, 0, DateTimeKind.Utc)
        });

        var objek = Assert.IsAssignableFrom<ObjectResult>(hasil);
        Assert.Equal(StatusCodes.Status200OK, objek.StatusCode ?? StatusCodes.Status200OK);

        await using var baca = h.Database.CreateContext();
        var pasien = await baca.Set<MstPatient>()
            .AsNoTracking()
            .Where(x => x.FullName == "Pasien Normal Uji")
            .OrderByDescending(x => x.CreateDateTime)
            .FirstAsync();

        return (pasien.PatientCode, pasien.MedicalRecordNumber);
    }

    private static async Task AssertTanpaDeretDanUrutanAsync(ReservasiHarness h)
    {
        await using var baca = h.Database.CreateContext();
        Assert.Equal(0, await BayiUji.JumlahDeretAsync(baca));
        Assert.Equal(0, await BayiUji.JumlahUrutanKelahiranAsync(baca));
    }

    private static string? Properti(object? objek, string nama) =>
        objek?.GetType().GetProperty(nama)?.GetValue(objek)?.ToString();
}
