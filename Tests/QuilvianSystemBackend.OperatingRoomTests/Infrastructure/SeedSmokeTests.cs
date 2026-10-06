namespace QuilvianSystemBackend.Tests.OperatingRoom.Infrastructure;

/// <summary>
/// Membuktikan basis data uji dan seeder demo Operasi benar-benar terbentuk. Bila uji ini
/// gagal, kegagalan uji lain di proyek ini tidak berarti apa-apa.
/// </summary>
public class SeedSmokeTests
{
    [Fact]
    public async Task Seeder_demo_membentuk_tenaga_dan_dokter_yang_diperlukan()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);

        Assert.NotEqual(Guid.Empty, seed.PasienId);
        Assert.NotEqual(Guid.Empty, seed.RuangId);
        Assert.NotEqual(Guid.Empty, seed.UnitTujuanId);
        Assert.Equal(3, seed.Hasil.TeamWorkforceIds.Count);
        Assert.NotEmpty(seed.TindakanIds);
    }

    [Fact]
    public async Task Tiga_peran_kesiapan_dipegang_tiga_tenaga_yang_berbeda()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);

        var tenaga = new[] { seed.TenagaBedahId, seed.TenagaAnestesiId, seed.TenagaPerawatInstrumenId };

        Assert.DoesNotContain(Guid.Empty, tenaga);
        Assert.Equal(3, tenaga.Distinct().Count());
    }

    [Fact]
    public async Task Akun_dokter_bedah_demo_tertaut_dokter_sehingga_dapat_membuat_permintaan()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);

        await using var db = harness.NewContext();
        var akun = db.Users.Single(x => x.Id == OperatingRoomSeed.AkunBedah);

        Assert.Equal(seed.DokterBedahId, akun.DoctorId);
        Assert.Equal(seed.TenagaBedahId, akun.WorkforceProfileId);
    }

    [Fact]
    public async Task Seeder_mengisi_id_dokter_bedah_tenaga_dan_satuan()
    {
        using var harness = new OperatingRoomHarness();
        var seed = await OperatingRoomSeed.BuatAsync(harness);
        Assert.NotEqual(Guid.Empty, seed.Hasil.DemoSurgeonDoctorId);
        Assert.NotEqual(Guid.Empty, seed.Hasil.DemoSurgeonWorkforceId);
        Assert.NotEqual(Guid.Empty, seed.Hasil.MeasurementId);
    }
}
