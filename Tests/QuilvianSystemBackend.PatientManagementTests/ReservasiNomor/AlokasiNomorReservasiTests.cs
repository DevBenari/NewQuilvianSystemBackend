using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.ReservasiNomor;

/// <summary>
/// Allocator nomor pasien normal pada module service — `PAT-MIG3-AC-03`–`08`, `PAT-MIG3-AC-14`,
/// kontrak `1.2.1` B14.1–B14.3. SQLite tidak dapat membuktikan kunci PostgreSQL; perilaku
/// bersamaan dibuktikan `PAT-MIG3-AC-13` pada PostgreSQL sungguhan.
/// </summary>
public class AlokasiNomorReservasiTests
{
    // ---- A, B, E: Staging persis + Enabled = true --------------------------------------

    [Fact]
    public async Task StagingAktif_TanpaPasienDiAtasLantai_MemberiLantai()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        // Nomor cadangan migrasi dan nomor lama yang kosong di bawah lantai tidak boleh dipakai.
        h.TambahPasien("PAT-RSMMC-00001", "00-00-00-01");
        h.TambahPasien("PAT-RSMMC-00742", "00-79-70-15");
        h.TambahPasien("PAT-RSMMC-101416", "00-00-00-05");
        h.TambahPasien("PAT-RSMMC-705249", "00-89-01-87");

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.True(hasil.Succeeded);
        Assert.Equal("PAT-RSMMC-705250", hasil.PatientCode);
        Assert.Equal("00-89-01-88", hasil.MedicalRecordNumber);
        Assert.Null(hasil.ErrorCode);
    }

    [Fact]
    public async Task StagingAktif_DatabaseKosong_TidakPernahDiBawahLantai()
    {
        // Pembangkit lama akan memberi PAT-RSMMC-00001 dan 00-00-00-01 di sini.
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.Equal("PAT-RSMMC-705250", hasil.PatientCode);
        Assert.Equal("00-89-01-88", hasil.MedicalRecordNumber);
    }

    [Fact]
    public async Task StagingAktif_Celah_MemilihNomorKosongPertamaDiAtasLantai()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-705250", "00-89-01-88");
        h.TambahPasien("PAT-RSMMC-705251", "00-89-01-90");
        h.TambahPasien("PAT-RSMMC-705253", "00-89-01-89");

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.Equal("PAT-RSMMC-705252", hasil.PatientCode);
        Assert.Equal("00-89-01-91", hasil.MedicalRecordNumber);
    }

    [Fact]
    public async Task StagingAktif_PasienTerhapusTetapDihitungTerpakai()
    {
        // Indeks unik PatientCode dan MedicalRecordNumber tanpa syarat, sehingga nomor pasien yang
        // ditandai terhapus tetap tidak boleh diberikan lagi — sama dengan pembangkit lama.
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-705250", "00-89-01-88", terhapus: true);

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.Equal("PAT-RSMMC-705251", hasil.PatientCode);
        Assert.Equal("00-89-01-89", hasil.MedicalRecordNumber);
    }

    [Fact]
    public async Task StagingAktif_NomorSamaDalamFormatBerbeda_TetapDihitungTerpakai()
    {
        // Indeks unik tidak dapat menangkap 705250 vs 0705250 atau 00890188 vs 00-89-01-88,
        // sehingga allocator yang menjaganya, seperti pembangkit lama.
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-0705250", "00890188");
        h.TambahPasien("PAT-RSMMC-99999", "RM 0089-0189");

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.Equal("PAT-RSMMC-705251", hasil.PatientCode);
        Assert.Equal("00-89-01-90", hasil.MedicalRecordNumber);
    }

    [Fact]
    public async Task StagingAktif_KodeLimaDigitBesarTidakDianggapDiAtasLantai()
    {
        // PAT-RSMMC-99999 lebih besar dari "PAT-RSMMC-705250" secara teks, tetapi angkanya 99.999.
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-99999", "00-99-99-99");
        h.TambahPasien("PAT-RSMMC-70000", "00-70-00-00");

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.Equal("PAT-RSMMC-705250", hasil.PatientCode);
        Assert.Equal("00-89-01-88", hasil.MedicalRecordNumber);
    }

    [Fact]
    public async Task StagingAktif_DiLuarTransaksi_Ditolak()
    {
        // Kunci database hanya berarti bila dipegang sampai pasien tersimpan.
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        await using var konteks = h.Database.CreateContext();

        await Assert.ThrowsAsync<InvalidOperationException>(() => h.BuatService(konteks).AllocateAsync());
    }

    // ---- F, G, H: Staging persis + konfigurasi tidak sah --------------------------------

    [Theory]
    [InlineData(KonfigurasiGuardUji.EnabledFalse, RsmmcReservationGuardState.Disabled)]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda, RsmmcReservationGuardState.ConfigMissing)]
    [InlineData(KonfigurasiGuardUji.KunciEnabledTidakAda, RsmmcReservationGuardState.ConfigMissing)]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk, RsmmcReservationGuardState.ConfigMalformed)]
    public async Task StagingTidakSah_DitolakTanpaNomorDanTanpaMutasi(
        KonfigurasiGuardUji konfigurasi,
        RsmmcReservationGuardState keadaan)
    {
        using var h = new ReservasiHarness("Staging", konfigurasi);
        h.TambahPasien("PAT-RSMMC-00001", "00-00-00-01");
        var sebelum = h.JumlahPasien();

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.False(hasil.Succeeded);
        Assert.Null(hasil.PatientCode);
        Assert.Null(hasil.MedicalRecordNumber);
        Assert.Equal("PATIENT_RESERVATION_GUARD_CONFIG_INVALID", hasil.ErrorCode);
        Assert.Equal(keadaan, hasil.Guard.State);
        Assert.Equal(sebelum, h.JumlahPasien());
    }

    // ---- I: di luar Staging persis, hasil sama dengan pembangkit lama -------------------

    public static IEnumerable<object[]> DataPembanding()
    {
        yield return [Array.Empty<(string, string)>()];
        yield return [new[] { ("PAT-RSMMC-00001", "00-00-00-01"), ("PAT-RSMMC-00002", "00-00-00-02") }];
        yield return [new[] { ("PAT-RSMMC-00001", "00-00-00-01"), ("PAT-RSMMC-00003", "00-00-00-03") }];
        yield return [new[] { ("PAT-RSMMC-00742", "00-00-00-27"), ("PAT-RSMMC-00027", "00-79-70-15") }];
        yield return [new[] { ("PAT-RSMMC-705250", "00-89-01-88"), ("PAT-RSMMC-00001", "00890188") }];
        yield return [new[] { ("DEMO-OPR-001", "RM-DEMO-1"), ("PAT-RSMMC-ABCDE", "TANPA-ANGKA") }];
    }

    [Theory]
    [MemberData(nameof(DataPembanding))]
    public async Task BukanStaging_HasilSamaDenganPembangkitLama((string Code, string Mrn)[] pasien)
    {
        using var h = new ReservasiHarness("Production", KonfigurasiGuardUji.BagianTidakAda);
        foreach (var (code, mrn) in pasien) h.TambahPasien(code, mrn);
        var acuan = PembangkitLamaAcuan.Hitung(pasien);

        foreach (var environmentName in new[] { "Production", "Development", "staging" })
        {
            foreach (var konfigurasi in new[]
            {
                KonfigurasiGuardUji.BagianTidakAda,
                KonfigurasiGuardUji.EnabledTrue,
                KonfigurasiGuardUji.EnabledFalse,
                KonfigurasiGuardUji.EnabledSalahBentuk
            })
            {
                h.Environment.EnvironmentName = environmentName;
                h.Configuration = ReservasiHarness.BuatKonfigurasi(konfigurasi);

                var hasil = await AlokasikanDalamTransaksiAsync(h);

                Assert.True(hasil.Succeeded);
                Assert.Equal(RsmmcReservationGuardState.NotApplicable, hasil.Guard.State);
                Assert.Equal(acuan.PatientCode, hasil.PatientCode);
                Assert.Equal(acuan.MedicalRecordNumber, hasil.MedicalRecordNumber);
            }
        }
    }

    [Fact]
    public async Task BukanStaging_DiLuarTransaksiTetapBerjalanSepertiDulu()
    {
        // Pembangkit lama tidak menuntut transaksi atau kunci; perilaku itu dipertahankan.
        using var h = new ReservasiHarness("Production", KonfigurasiGuardUji.BagianTidakAda);
        await using var konteks = h.Database.CreateContext();

        var hasil = await h.BuatService(konteks).AllocateAsync();

        Assert.Equal("PAT-RSMMC-00001", hasil.PatientCode);
        Assert.Equal("00-00-00-01", hasil.MedicalRecordNumber);
    }

    // ---- Pemeriksa baca-saja (PAT-MIG3-AC-07) --------------------------------------------

    [Fact]
    public async Task Pemeriksa_StagingAktif_SiapDanTidakMenulis()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-705250", "00-89-01-88");
        var sebelum = h.JumlahPasien();
        await using var konteks = h.Database.CreateContext();

        var status = await h.BuatService(konteks).CheckAsync();

        Assert.Equal("Staging", status.Environment);
        Assert.Equal("ACTIVE", status.GuardState);
        Assert.True(status.GuardEnabled);
        Assert.True(status.GuardConfigurationValid);
        Assert.Equal(705250, status.PatientCodeFloor);
        Assert.Equal("PAT-RSMMC-705250", status.PatientCodeFloorFormatted);
        Assert.Equal(890188, status.MrnFloor);
        Assert.Equal("00-89-01-88", status.MrnFloorFormatted);
        Assert.Equal(705251, status.NextPatientCodeNumeric);
        Assert.Equal("PAT-RSMMC-705251", status.NextPatientCodeFormatted);
        Assert.Equal(890189, status.NextMrnNumeric);
        Assert.Equal("00-89-01-89", status.NextMrnFormatted);
        Assert.True(status.PatientCodeFloorSatisfied);
        Assert.True(status.MrnFloorSatisfied);
        Assert.True(status.OverallGuardReady);

        Assert.Equal(sebelum, h.JumlahPasien());
        Assert.Empty(konteks.ChangeTracker.Entries());
        Assert.Null(konteks.Database.CurrentTransaction);
        Assert.Equal(0, h.JumlahBerkasUnggahan());
    }

    [Theory]
    [InlineData(KonfigurasiGuardUji.EnabledFalse, "DISABLED")]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda, "CONFIG_MISSING")]
    [InlineData(KonfigurasiGuardUji.EnabledSalahBentuk, "CONFIG_MALFORMED")]
    public async Task Pemeriksa_StagingTidakSah_BelumSiapTetapiMelaporkanNomorBerlantai(
        KonfigurasiGuardUji konfigurasi,
        string keadaan)
    {
        using var h = new ReservasiHarness("Staging", konfigurasi);
        await using var konteks = h.Database.CreateContext();

        var status = await h.BuatService(konteks).CheckAsync();

        Assert.Equal(keadaan, status.GuardState);
        Assert.False(status.GuardEnabled);
        Assert.False(status.GuardConfigurationValid);
        Assert.Equal("PAT-RSMMC-705250", status.NextPatientCodeFormatted);
        Assert.Equal("00-89-01-88", status.NextMrnFormatted);
        Assert.False(status.OverallGuardReady);
    }

    [Fact]
    public async Task Pemeriksa_BukanStaging_MelaporkanNomorLamaDanTidakSiap()
    {
        using var h = new ReservasiHarness("Production", KonfigurasiGuardUji.BagianTidakAda);
        h.TambahPasien("PAT-RSMMC-00001", "00-00-00-01");
        await using var konteks = h.Database.CreateContext();

        var status = await h.BuatService(konteks).CheckAsync();

        Assert.Equal("NOT_APPLICABLE", status.GuardState);
        Assert.False(status.GuardEnabled);
        Assert.True(status.GuardConfigurationValid);
        Assert.Equal("PAT-RSMMC-00002", status.NextPatientCodeFormatted);
        Assert.Equal("00-00-00-02", status.NextMrnFormatted);
        Assert.False(status.PatientCodeFloorSatisfied);
        Assert.False(status.MrnFloorSatisfied);
        Assert.False(status.OverallGuardReady);
    }

    [Fact]
    public async Task Pemeriksa_HasilnyaSamaDenganAlokasiBerikutnya()
    {
        using var h = new ReservasiHarness("Staging", KonfigurasiGuardUji.EnabledTrue);
        h.TambahPasien("PAT-RSMMC-705250", "00-89-01-88");
        h.TambahPasien("PAT-RSMMC-705252", "00-89-01-90");

        string? kodeDiperiksa;
        string? mrnDiperiksa;
        await using (var konteks = h.Database.CreateContext())
        {
            var status = await h.BuatService(konteks).CheckAsync();
            kodeDiperiksa = status.NextPatientCodeFormatted;
            mrnDiperiksa = status.NextMrnFormatted;
        }

        var hasil = await AlokasikanDalamTransaksiAsync(h);

        Assert.Equal(kodeDiperiksa, hasil.PatientCode);
        Assert.Equal(mrnDiperiksa, hasil.MedicalRecordNumber);
    }

    private static async Task<PatientIdentifierAllocationResult> AlokasikanDalamTransaksiAsync(ReservasiHarness h)
    {
        await using var konteks = h.Database.CreateContext();
        await using var transaksi = await konteks.Database.BeginTransactionAsync();

        var hasil = await h.BuatService(konteks).AllocateAsync();

        await transaksi.RollbackAsync();

        return hasil;
    }
}
