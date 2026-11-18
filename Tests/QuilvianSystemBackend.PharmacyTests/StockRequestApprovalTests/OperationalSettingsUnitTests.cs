using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Uji unit murni atas pengaturan operasional: tenggat verifikasi susulan CITO (FIN-LGS-BE-005)
/// dan penunjukan jabatan otorisasi CITO (FIN-LGS-BE-003). Tanpa basis data maupun jam sistem.
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

    [Fact]
    public void Penunjukan_jabatan_menerima_id_dan_kode_dalam_daftar_maupun_teks_berkoma()
    {
        var id = Guid.NewGuid();
        var tunjuk = StockRequestEmergencyAuthorizerDesignation.Parse(
            [$"{id}, POS-KEP-002", "POS-LOG-002", null, " "]);

        Assert.True(tunjuk.IsConfigured);
        Assert.Equal(id, Assert.Single(tunjuk.PositionIds));
        Assert.Equal(2, tunjuk.PositionCodes.Count);
        Assert.Contains("POS-KEP-002", tunjuk.PositionCodes);
    }

    [Fact]
    public void Penunjukan_kosong_berarti_tidak_ada_jabatan_berwenang()
    {
        var kosong = StockRequestEmergencyAuthorizerDesignation.Parse([null, "", " , "]);

        Assert.False(kosong.IsConfigured);
        Assert.Empty(kosong.PositionIds);
        Assert.Empty(kosong.PositionCodes);
    }
}
