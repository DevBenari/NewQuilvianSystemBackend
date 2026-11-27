using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Uji unit murni atas pengaturan operasional: tenggat verifikasi susulan CITO (FIN-LGS-BE-005).
/// Penunjukan jabatan otorisasi CITO dihapus pada FIN-LGS-070. Tanpa basis data maupun jam sistem.
/// </summary>
public class OperationalSettingsUnitTests
{
    private static readonly DateTime Otorisasi = new(2026, 11, 18, 0, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Tenggat_tanpa_nilai_tidak_dikonfigurasi_dan_tidak_ada_bawaan(string? jam)
    {
        var tenggat = StockRequestEmergencyReviewDeadline.Resolve(jam, null);

        Assert.False(tenggat.IsConfigured);
        Assert.Null(tenggat.DueAtUtc(Otorisasi));
        Assert.Equal(StockRequestEmergencyReviewDueState.NotConfigured, tenggat.Classify(Otorisasi, Otorisasi.AddDays(9)));
    }

    [Theory]
    [InlineData("0", null)]
    [InlineData("-5", null)]
    [InlineData("abc", null)]
    [InlineData("24", "24")]
    [InlineData("24", "30")]
    [InlineData("24", "-1")]
    // Di atas batas teknis: tanpa penjaga, "1e300" membuat TimeSpan.FromHours melempar galat
    // saat service dibuat dan seluruh endpoint permintaan stok ikut gagal.
    [InlineData("1000000", null)]
    [InlineData("1e300", null)]
    public void Nilai_tenggat_atau_peringatan_yang_tidak_sah_dianggap_belum_dikonfigurasi(string jam, string? peringatan)
    {
        var tenggat = StockRequestEmergencyReviewDeadline.Resolve(jam, peringatan);

        Assert.False(tenggat.IsConfigured);
        Assert.Null(tenggat.DueAtUtc(Otorisasi));
    }

    [Fact]
    public void Klasifikasi_belum_jatuh_tempo_mendekati_dan_terlambat_mengikuti_konfigurasi()
    {
        var tenggat = StockRequestEmergencyReviewDeadline.Resolve("24", "4");

        Assert.Equal(Otorisasi.AddHours(24), tenggat.DueAtUtc(Otorisasi));
        Assert.Equal(StockRequestEmergencyReviewDueState.NotDue, tenggat.Classify(Otorisasi, Otorisasi.AddHours(19)));
        Assert.Equal(StockRequestEmergencyReviewDueState.DueSoon, tenggat.Classify(Otorisasi, Otorisasi.AddHours(20)));
        Assert.Equal(StockRequestEmergencyReviewDueState.DueSoon, tenggat.Classify(Otorisasi, Otorisasi.AddHours(23.9)));
        Assert.Equal(StockRequestEmergencyReviewDueState.Overdue, tenggat.Classify(Otorisasi, Otorisasi.AddHours(24)));
    }

    [Fact]
    public void Tanpa_batas_peringatan_tidak_ada_keadaan_mendekati_tenggat()
    {
        var tenggat = StockRequestEmergencyReviewDeadline.Resolve("8", null);

        Assert.Equal(StockRequestEmergencyReviewDueState.NotDue, tenggat.Classify(Otorisasi, Otorisasi.AddHours(7.99)));
        Assert.Equal(StockRequestEmergencyReviewDueState.Overdue, tenggat.Classify(Otorisasi, Otorisasi.AddHours(8)));
    }
}
