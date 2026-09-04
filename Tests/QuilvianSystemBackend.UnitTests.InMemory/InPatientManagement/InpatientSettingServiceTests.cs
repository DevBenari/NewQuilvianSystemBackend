using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Aturan layar pengaturan Rawat Inap yang diminta <c>BE-RWI-005</c>.
/// </summary>
public sealed class InpatientSettingServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task MengubahBatasPemesanan_BerlakuPadaPembacaanBerikutnya()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var masterService = new InpatientSettingService(db);
        var moduleLogger = new RecordingLogger<InpSettingService>();
        var moduleService = new InpSettingService(db, moduleLogger);

        var entity = BuildSetting(bedReservationMinutes: 120);
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        Assert.Equal(120, (await moduleService.GetEffectiveSettingAsync()).BedReservationMinutes);

        // Admin mengubah batas pemesanan dari 2 jam menjadi 3 jam pada layar pengaturan.
        var hasil = await masterService.UpdateAsync(
            entity.Id,
            BuildUpdateRequest(bedReservationMinutes: 180),
            ActorUserId);

        Assert.Equal(InpatientSettingUpdateStatus.Success, hasil.Status);

        // RWI-AC-003 dan RWI-AC-110: nilai baru berlaku pada pembacaan berikutnya, tanpa
        // aplikasi dinyalakan ulang.
        Assert.Equal(180, (await moduleService.GetEffectiveSettingAsync()).BedReservationMinutes);
    }

    [Fact]
    public async Task PerubahanTercatatSebagaiJejakAudit()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientSettingService(db);

        var entity = BuildSetting(bedReservationMinutes: 120);
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        await service.UpdateAsync(entity.Id, BuildUpdateRequest(180), ActorUserId);

        var tersimpan = await db.Set<MstInpatientSetting>().AsNoTracking().SingleAsync();

        Assert.Equal(ActorUserId, tersimpan.UpdateBy);
        Assert.NotNull(tersimpan.UpdateDateTime);
    }

    [Fact]
    public async Task KodeBarisPengaturan_TidakIkutBerubah()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientSettingService(db);

        var entity = BuildSetting(bedReservationMinutes: 120);
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        await service.UpdateAsync(entity.Id, BuildUpdateRequest(180), ActorUserId);

        // Modul membaca baris ini lewat Code. Mengganti kodenya akan membuat seluruh modul
        // kehilangan baris yang dibacanya, karena itu Code tidak ada pada request.
        Assert.Equal("DEFAULT", (await db.Set<MstInpatientSetting>().AsNoTracking().SingleAsync()).Code);
    }

    [Fact]
    public async Task MenonaktifkanBarisPengaturanTerakhir_Ditolak()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientSettingService(db);

        var entity = BuildSetting(bedReservationMinutes: 120);
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        var request = BuildUpdateRequest(120);
        request.IsActive = false;

        var hasil = await service.UpdateAsync(entity.Id, request, ActorUserId);

        Assert.Equal(InpatientSettingUpdateStatus.Invalid, hasil.Status);
        Assert.Contains("satu-satunya", hasil.Message);
        Assert.True((await db.Set<MstInpatientSetting>().AsNoTracking().SingleAsync()).IsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-30)]
    [InlineData(1441)]
    public async Task BatasPemesananDiLuarRentangWajar_Ditolak(int menit)
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientSettingService(db);

        var entity = BuildSetting(bedReservationMinutes: 120);
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        var hasil = await service.UpdateAsync(entity.Id, BuildUpdateRequest(menit), ActorUserId);

        Assert.Equal(InpatientSettingUpdateStatus.Invalid, hasil.Status);
        Assert.Equal(120, (await db.Set<MstInpatientSetting>().AsNoTracking().SingleAsync()).BedReservationMinutes);
    }

    [Fact]
    public async Task AwalanNomorEpisode_DisimpanSebagaiHurufBesar()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientSettingService(db);

        var entity = BuildSetting(bedReservationMinutes: 120);
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        var request = BuildUpdateRequest(120);
        request.EpisodeNumberPrefix = " ranap ";

        await service.UpdateAsync(entity.Id, request, ActorUserId);

        Assert.Equal("RANAP", (await db.Set<MstInpatientSetting>().AsNoTracking().SingleAsync()).EpisodeNumberPrefix);
    }

    [Fact]
    public async Task BarisPengaturanYangTidakAda_MengembalikanNotFound()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientSettingService(db);

        var hasil = await service.UpdateAsync(Guid.NewGuid(), BuildUpdateRequest(120), ActorUserId);

        Assert.Equal(InpatientSettingUpdateStatus.NotFound, hasil.Status);
    }

    [Fact]
    public async Task PengaturanBelumTerisi_PembacaanMengembalikanKosong()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = new InpatientSettingService(db);

        Assert.Null(await service.GetEffectiveAsync());
        Assert.Equal(0, await service.CountLiveSettingsAsync());
    }

    private static MstInpatientSetting BuildSetting(int bedReservationMinutes)
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
            EpisodeNumberPrefix = "RI",
            IsDefault = true,
            IsActive = true,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = ActorUserId
        };

    private static UpdateInpatientSettingRequest BuildUpdateRequest(int bedReservationMinutes)
        => new()
        {
            Name = "Pengaturan Rawat Inap Default",
            BedReservationMinutes = bedReservationMinutes,
            DraftEpisodeExpiryHours = 24,
            InitialAssessmentTargetHours = 24,
            ProgressNoteVerificationTargetHours = 24,
            PendingClosureThresholdHours = 4,
            EpisodeNumberPrefix = "RI",
            IsActive = true
        };
}
