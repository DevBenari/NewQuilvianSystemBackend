using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Uji unit murni atas penafsiran batas aktivasi persetujuan (<c>LGS-DEC-006</c>). Tidak memakai
/// basis data maupun jam sistem: waktu "sekarang" diberikan eksplisit.
/// </summary>
public class CutoverUnitTests
{
    private static readonly DateTime Sekarang = new(2026, 11, 17, 8, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Nilai_kosong_bukan_izin_jalan_pintas(string? nilai)
    {
        var batas = StockRequestApprovalCutover.Resolve(nilai, Sekarang);

        Assert.Equal(StockRequestApprovalCutoverState.NotConfigured, batas.State);
        Assert.False(batas.AllowsLegacyTransition);
        Assert.False(batas.IsBeforeCutover(Sekarang.AddYears(-5)));
    }

    [Theory]
    [InlineData("bukan-tanggal")]
    [InlineData("2026-13-45")]
    public void Nilai_yang_tidak_dapat_dibaca_diabaikan_dan_semua_wajib_disetujui(string nilai)
    {
        var batas = StockRequestApprovalCutover.Resolve(nilai, Sekarang);

        Assert.Equal(StockRequestApprovalCutoverState.Invalid, batas.State);
        Assert.False(batas.IsBeforeCutover(Sekarang.AddYears(-5)));
    }

    [Fact]
    public void Batas_di_masa_depan_ditolak_karena_batas_dikunci_saat_aktivasi()
    {
        var batas = StockRequestApprovalCutover.Resolve(Sekarang.AddDays(1).ToString("O"), Sekarang);

        Assert.Equal(StockRequestApprovalCutoverState.FutureRejected, batas.State);
        // Tanpa penolakan ini, permintaan yang diajukan hari ini akan menjadi "permintaan lama"
        // dan lolos tanpa persetujuan.
        Assert.False(batas.IsBeforeCutover(Sekarang.AddMinutes(-1)));
    }

    [Fact]
    public void Batas_yang_sah_dan_sudah_lewat_memisahkan_permintaan_lama_dan_baru()
    {
        var aktivasi = Sekarang.AddDays(-1);
        var batas = StockRequestApprovalCutover.Resolve(aktivasi.ToString("O"), Sekarang);

        Assert.Equal(StockRequestApprovalCutoverState.Active, batas.State);
        Assert.Equal(aktivasi, batas.EffectiveFromUtc);
        Assert.True(batas.IsBeforeCutover(aktivasi.AddTicks(-1)));
        Assert.False(batas.IsBeforeCutover(aktivasi));
        Assert.False(batas.IsBeforeCutover(aktivasi.AddMinutes(1)));
    }

    [Fact]
    public void Permintaan_tanpa_waktu_pengajuan_tidak_pernah_dianggap_permintaan_lama()
    {
        var batas = StockRequestApprovalCutover.Resolve(Sekarang.AddDays(-1).ToString("O"), Sekarang);

        Assert.False(batas.IsBeforeCutover(null));
    }

    [Fact]
    public void Nilai_berzona_waktu_dinormalkan_ke_UTC()
    {
        // 15.00 WIB = 08.00 UTC.
        var batas = StockRequestApprovalCutover.Resolve("2026-11-16T15:00:00+07:00", Sekarang);

        Assert.Equal(StockRequestApprovalCutoverState.Active, batas.State);
        Assert.Equal(new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc), batas.EffectiveFromUtc);
        Assert.Equal(DateTimeKind.Utc, batas.EffectiveFromUtc!.Value.Kind);
    }

    [Fact]
    public void Nilai_tanpa_zona_waktu_dianggap_UTC()
    {
        var batas = StockRequestApprovalCutover.Resolve("2026-11-16T08:00:00", Sekarang);

        Assert.Equal(new DateTime(2026, 11, 16, 8, 0, 0, DateTimeKind.Utc), batas.EffectiveFromUtc);
    }

    [Fact]
    public void Selisih_jam_antarserver_dalam_toleransi_tetap_diterima()
    {
        var batas = StockRequestApprovalCutover.Resolve(Sekarang.AddMinutes(2).ToString("O"), Sekarang);

        Assert.Equal(StockRequestApprovalCutoverState.Active, batas.State);
    }
}
