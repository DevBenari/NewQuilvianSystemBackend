using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Seeders;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Menjaga tiga janji <c>BE-RWI-002</c>: seeder mengisi persis apa yang dirancang,
/// menjalankannya dua kali tidak menghasilkan data ganda, dan seeder menolak berjalan di
/// lingkungan produksi.
/// </summary>
public sealed class InpatientMasterDataSeederTests
{
    private static readonly Guid ActorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Seeder_MengisiSatuBarisPengaturanBerkodeDefault()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();

        await InpatientMasterDataSeeder.SeedAsync(db, ActorUserId, "Development");

        var setting = await db.Set<MstInpatientSetting>().SingleAsync();

        Assert.Equal("DEFAULT", setting.Code);
        Assert.Equal(120, setting.BedReservationMinutes);
        Assert.Equal(24, setting.DraftEpisodeExpiryHours);
        Assert.Equal(24, setting.InitialAssessmentTargetHours);
        Assert.Equal(24, setting.ProgressNoteVerificationTargetHours);
        Assert.Equal(4, setting.PendingClosureThresholdHours);
        Assert.Equal("RI", setting.EpisodeNumberPrefix);
        Assert.True(setting.IsDefault);
        Assert.True(setting.IsActive);
    }

    [Fact]
    public async Task Seeder_MengisiTigaButirAdministrasiDenganDischargeMedTidakWajib()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();

        await InpatientMasterDataSeeder.SeedAsync(db, ActorUserId, "Development");

        var items = await db.Set<MstInpatientClearanceItem>()
            .OrderBy(x => x.SortOrder)
            .ToListAsync();

        Assert.Equal(3, items.Count);
        Assert.Equal(new[] { "ADM-DOC", "RETURN-ITEM", "DISCHARGE-MED" }, items.Select(x => x.ItemCode));

        Assert.True(items.Single(x => x.ItemCode == "ADM-DOC").IsMandatory);
        Assert.True(items.Single(x => x.ItemCode == "RETURN-ITEM").IsMandatory);

        // RWI-RULE-024: obat pulang belum dapat ditutup otomatis karena modul Farmasi di
        // luar scope MVP. Menjadikannya wajib akan menahan penutupan setiap episode tanpa
        // ada cara menyelesaikannya.
        Assert.False(items.Single(x => x.ItemCode == "DISCHARGE-MED").IsMandatory);
    }

    [Fact]
    public async Task Seeder_DijalankanDuaKali_TidakMenghasilkanDataGanda()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();

        var first = await InpatientMasterDataSeeder.SeedAsync(db, ActorUserId, "Development");
        var second = await InpatientMasterDataSeeder.SeedAsync(db, ActorUserId, "Development");

        Assert.Equal(4, first.TotalInserted);
        Assert.Equal(0, second.TotalInserted);
        Assert.Equal(3, second.ClearanceItemSkipped);
        Assert.NotNull(second.SettingSkippedReason);

        Assert.Equal(1, await db.Set<MstInpatientSetting>().CountAsync());
        Assert.Equal(3, await db.Set<MstInpatientClearanceItem>().CountAsync());
    }

    [Fact]
    public async Task Seeder_MenolakBerjalanDiLingkunganProduksi()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();

        var result = await InpatientMasterDataSeeder.SeedAsync(db, ActorUserId, "Production");

        Assert.True(result.Refused);
        Assert.Equal(0, result.TotalInserted);
        Assert.Equal(0, await db.Set<MstInpatientSetting>().CountAsync());
        Assert.Equal(0, await db.Set<MstInpatientClearanceItem>().CountAsync());
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("production")]
    [InlineData("PRODUCTION")]
    [InlineData(" Production ")]
    public void Seeder_MengenaliNamaLingkunganProduksiTanpaPeduliBesarKecilHuruf(string environmentName)
    {
        Assert.True(InpatientMasterDataSeeder.IsProductionEnvironment(environmentName));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Staging")]
    [InlineData("")]
    [InlineData(null)]
    public void Seeder_TidakMenganggapLingkunganLainSebagaiProduksi(string? environmentName)
    {
        Assert.False(InpatientMasterDataSeeder.IsProductionEnvironment(environmentName));
    }

    [Fact]
    public async Task Seeder_TidakPernahMembuatKamarMaupunTempatTidur()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();

        await InpatientMasterDataSeeder.SeedAsync(db, ActorUserId, "Development");

        // RWI-DEC-048: susunan kamar dan tempat tidur khas tiap rumah sakit. Menebaknya
        // menghasilkan master palsu yang terlanjur dipakai penempatan pasien.
        Assert.Equal(0, await db.Set<MstRoom>().CountAsync());
        Assert.Equal(0, await db.Set<MstBed>().CountAsync());
    }
}
