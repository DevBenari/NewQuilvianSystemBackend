using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Tiga kasus <c>InpSettingService</c> yang diminta <c>BE-RWI-004</c>: baris pengaturan ada,
/// baris tidak ada, dan nilainya diubah admin.
/// </summary>
public sealed class InpSettingServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task BarisAda_AngkanyaDibacaDariMasterDanTidakAdaPeringatan()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var logger = new RecordingLogger<InpSettingService>();
        var service = new InpSettingService(db, logger);

        db.Set<MstInpatientSetting>().Add(BuildSetting(bedReservationMinutes: 90, prefix: "RWI"));
        await db.SaveChangesAsync();

        var setting = await service.GetEffectiveSettingAsync();

        Assert.True(setting.IsFromMasterData);
        Assert.Equal(90, setting.BedReservationMinutes);
        Assert.Equal("RWI", setting.EpisodeNumberPrefix);
        Assert.Equal(0, logger.WarningCount);
    }

    [Fact]
    public async Task BarisTidakAda_NilaiBawaanDipakaiDanPeringatanDicatat()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var logger = new RecordingLogger<InpSettingService>();
        var service = new InpSettingService(db, logger);

        var setting = await service.GetEffectiveSettingAsync();

        Assert.False(setting.IsFromMasterData);
        Assert.Equal(120, setting.BedReservationMinutes);
        Assert.Equal(24, setting.DraftEpisodeExpiryHours);
        Assert.Equal(4, setting.PendingClosureThresholdHours);
        Assert.Equal("RI", setting.EpisodeNumberPrefix);

        // Peringatan ini bukan hiasan. Tanpa peringatan, angka bawaan yang salah dapat
        // terpakai berbulan-bulan di produksi tanpa ada yang menyadarinya.
        Assert.Equal(1, logger.WarningCount);
        Assert.True(logger.HasWarningContaining("nilai bawaan"));
    }

    [Fact]
    public async Task NilaiDiubahAdmin_PembacaanBerikutnyaMemakaiNilaiBaru()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var logger = new RecordingLogger<InpSettingService>();
        var service = new InpSettingService(db, logger);

        var entity = BuildSetting(bedReservationMinutes: 120, prefix: "RI");
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        var before = await service.GetEffectiveSettingAsync();
        Assert.Equal(120, before.BedReservationMinutes);

        // Admin mengubah batas pemesanan dari 2 jam menjadi 3 jam lewat layar pengaturan.
        entity.BedReservationMinutes = 180;
        await db.SaveChangesAsync();

        var after = await service.GetEffectiveSettingAsync();

        // RWI-AC-003 dan RWI-AC-110: nilai baru berlaku pada pembacaan berikutnya, tanpa
        // aplikasi dinyalakan ulang. Karena itu service ini tidak boleh menyimpan hasil
        // pembacaan sebelumnya.
        Assert.Equal(180, after.BedReservationMinutes);
    }

    [Fact]
    public async Task BarisNonaktif_TidakDipakaiDanNilaiBawaanBerlaku()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var logger = new RecordingLogger<InpSettingService>();
        var service = new InpSettingService(db, logger);

        var entity = BuildSetting(bedReservationMinutes: 90, prefix: "RI");
        entity.IsActive = false;
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        var setting = await service.GetEffectiveSettingAsync();

        Assert.False(setting.IsFromMasterData);
        Assert.Equal(120, setting.BedReservationMinutes);
        Assert.Equal(1, logger.WarningCount);
    }

    [Fact]
    public async Task BarisBertandaDefault_DidahulukanDaripadaBarisAktifLain()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var logger = new RecordingLogger<InpSettingService>();
        var service = new InpSettingService(db, logger);

        var nonDefault = BuildSetting(bedReservationMinutes: 45, prefix: "XX");
        nonDefault.Code = "CADANGAN";
        nonDefault.IsDefault = false;

        db.Set<MstInpatientSetting>().Add(nonDefault);
        db.Set<MstInpatientSetting>().Add(BuildSetting(bedReservationMinutes: 120, prefix: "RI"));
        await db.SaveChangesAsync();

        var setting = await service.GetEffectiveSettingAsync();

        Assert.Equal(120, setting.BedReservationMinutes);
        Assert.Equal("RI", setting.EpisodeNumberPrefix);
    }

    [Fact]
    public async Task AwalanDibacaSebagaiHurufBesarDanTanpaSpasi()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var logger = new RecordingLogger<InpSettingService>();
        var service = new InpSettingService(db, logger);

        db.Set<MstInpatientSetting>().Add(BuildSetting(bedReservationMinutes: 120, prefix: " ri "));
        await db.SaveChangesAsync();

        Assert.Equal("RI", await service.GetEpisodeNumberPrefixAsync());
    }

    private static MstInpatientSetting BuildSetting(int bedReservationMinutes, string prefix)
        => new()
        {
            Id = Guid.NewGuid(),
            Code = "DEFAULT",
            Name = "Pengaturan Rawat Inap Default",
            BedReservationMinutes = bedReservationMinutes,
            DraftEpisodeExpiryHours = 24,
            InitialAssessmentTargetHours = 24,
            ProgressNoteVerificationTargetHours = 24,
            PendingClosureThresholdHours = 4,
            EpisodeNumberPrefix = prefix,
            IsDefault = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };
}
