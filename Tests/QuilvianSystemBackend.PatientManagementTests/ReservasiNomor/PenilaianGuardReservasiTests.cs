using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Services;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement.ReservasiNomor;

/// <summary>
/// Aturan environment dan konfigurasi guard reservasi — `PAT-DEC-066`, `PAT-DEC-067`,
/// `PAT-MIG3-AC-08`, `PAT-MIG3-AC-14`, kontrak `1.2.1` B14.3.
/// </summary>
public class PenilaianGuardReservasiTests
{
    [Fact]
    public void StagingPersis_EnabledTrue_GuardAktif()
    {
        var hasil = Nilai("Staging", KonfigurasiGuardUji.EnabledTrue);

        Assert.Equal(RsmmcReservationGuardState.Active, hasil.State);
        Assert.True(hasil.IsActive);
        Assert.False(hasil.IsFailClosed);
        Assert.True(hasil.ConfigurationValid);
        Assert.Equal("ACTIVE", hasil.StateCode);
    }

    [Theory]
    [InlineData("true")]
    [InlineData("True")]
    [InlineData("TRUE")]
    public void StagingPersis_EjaanBooleanTrue_GuardAktif(string nilai)
    {
        var hasil = RsmmcIdentifierReservationProfile.Evaluate(
            "Staging",
            ReservasiHarness.BuatKonfigurasi(KonfigurasiGuardUji.EnabledSalahBentuk, nilai));

        Assert.Equal(RsmmcReservationGuardState.Active, hasil.State);
    }

    [Fact]
    public void StagingPersis_EnabledFalse_GagalTertutup_TidakMematikanGuard()
    {
        // PAT-DEC-067: false eksplisit di Staging bukan saklar mati, melainkan gagal tertutup.
        var hasil = Nilai("Staging", KonfigurasiGuardUji.EnabledFalse);

        Assert.Equal(RsmmcReservationGuardState.Disabled, hasil.State);
        Assert.True(hasil.IsFailClosed);
        Assert.False(hasil.IsActive);
        Assert.False(hasil.ConfigurationValid);
        Assert.Equal("DISABLED", hasil.StateCode);
    }

    [Theory]
    [InlineData(KonfigurasiGuardUji.BagianTidakAda)]
    [InlineData(KonfigurasiGuardUji.KunciEnabledTidakAda)]
    public void StagingPersis_KonfigurasiHilang_GagalTertutup(KonfigurasiGuardUji konfigurasi)
    {
        var hasil = Nilai("Staging", konfigurasi);

        Assert.Equal(RsmmcReservationGuardState.ConfigMissing, hasil.State);
        Assert.True(hasil.IsFailClosed);
        Assert.False(hasil.ConfigurationValid);
    }

    [Theory]
    [InlineData("yes")]
    [InlineData("1")]
    [InlineData("0")]
    [InlineData("aktif")]
    [InlineData("")]
    [InlineData("   ")]
    public void StagingPersis_NilaiSalahBentuk_GagalTertutup(string nilai)
    {
        var hasil = RsmmcIdentifierReservationProfile.Evaluate(
            "Staging",
            ReservasiHarness.BuatKonfigurasi(KonfigurasiGuardUji.EnabledSalahBentuk, nilai));

        Assert.Equal(RsmmcReservationGuardState.ConfigMalformed, hasil.State);
        Assert.True(hasil.IsFailClosed);
        Assert.Equal("CONFIG_MALFORMED", hasil.StateCode);
    }

    [Fact]
    public void StagingPersis_KonfigurasiTidakTerbaca_GagalTertutup()
    {
        var hasil = RsmmcIdentifierReservationProfile.Evaluate("Staging", new KonfigurasiTidakTerbaca());

        Assert.Equal(RsmmcReservationGuardState.ConfigMalformed, hasil.State);
        Assert.True(hasil.IsFailClosed);
    }

    [Theory]
    [InlineData("Production", KonfigurasiGuardUji.BagianTidakAda)]
    [InlineData("Production", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("Production", KonfigurasiGuardUji.EnabledFalse)]
    [InlineData("Production", KonfigurasiGuardUji.EnabledSalahBentuk)]
    [InlineData("Development", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("Test", KonfigurasiGuardUji.BagianTidakAda)]
    [InlineData("staging", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("STAGING", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("Staging ", KonfigurasiGuardUji.EnabledTrue)]
    [InlineData("", KonfigurasiGuardUji.EnabledTrue)]
    public void BukanStagingPersis_GuardTidakBerlaku_KonfigurasiTidakDibutuhkan(
        string environmentName,
        KonfigurasiGuardUji konfigurasi)
    {
        var hasil = Nilai(environmentName, konfigurasi);

        Assert.Equal(RsmmcReservationGuardState.NotApplicable, hasil.State);
        Assert.False(hasil.IsActive);
        Assert.False(hasil.IsFailClosed);
        Assert.True(hasil.ConfigurationValid);
        Assert.Equal("NOT_APPLICABLE", hasil.StateCode);
    }

    [Fact]
    public void BukanStaging_KonfigurasiTidakTerbaca_TidakDibaca()
    {
        // Di luar Staging persis konfigurasi guard tidak dibaca sama sekali, sehingga penyedia
        // konfigurasi yang rusak pun tidak mengubah perilaku Produksi.
        var hasil = RsmmcIdentifierReservationProfile.Evaluate("Production", new KonfigurasiTidakTerbaca());

        Assert.Equal(RsmmcReservationGuardState.NotApplicable, hasil.State);
    }

    [Fact]
    public void LantaiDanFormat_SesuaiKeputusanPemilik()
    {
        Assert.Equal(705250, RsmmcIdentifierReservationProfile.PatientCodeFloor);
        Assert.Equal(890188, RsmmcIdentifierReservationProfile.MedicalRecordNumberFloor);
        Assert.Equal("PAT-RSMMC-705250", RsmmcIdentifierReservationProfile.FormatPatientCode(705250));
        Assert.Equal("PAT-RSMMC-00742", RsmmcIdentifierReservationProfile.FormatPatientCode(742));
        Assert.Equal("00-89-01-88", RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(890188));
        Assert.Equal("00-00-00-27", RsmmcIdentifierReservationProfile.FormatMedicalRecordNumber(27));
        Assert.Equal("PATIENT_RESERVATION_GUARD_CONFIG_INVALID", RsmmcIdentifierReservationProfile.GuardConfigInvalidErrorCode);
    }

    [Fact]
    public void KunciAdvisory_PasanganTetapYangDidokumentasikan_TidakBertabrakanDenganWorkerMigrasi()
    {
        // Kunci dua bagian dengan konstanta tetap — tanpa hashtext() — supaya seluruh instance
        // aplikasi memakai kunci yang persis sama.
        Assert.Equal(1346458697, RsmmcIdentifierReservationProfile.AllocationLockNamespace);
        Assert.Equal(1313821261, RsmmcIdentifierReservationProfile.AllocationLockId);
        Assert.Equal("PATI", TeksAscii(RsmmcIdentifierReservationProfile.AllocationLockNamespace));
        Assert.Equal("NORM", TeksAscii(RsmmcIdentifierReservationProfile.AllocationLockId));

        // Namespace worker BE-PAT-MIG-002 yang sudah dipesan kontrak B4.3 (0x45EEEFBA).
        Assert.NotEqual(1173286842, RsmmcIdentifierReservationProfile.AllocationLockNamespace);
    }

    private static string TeksAscii(int nilai) =>
        string.Concat(Enumerable.Reverse(BitConverter.GetBytes(nilai)).Select(b => (char)b));

    private static RsmmcReservationGuardEvaluation Nilai(string environmentName, KonfigurasiGuardUji konfigurasi) =>
        RsmmcIdentifierReservationProfile.Evaluate(environmentName, ReservasiHarness.BuatKonfigurasi(konfigurasi));
}
