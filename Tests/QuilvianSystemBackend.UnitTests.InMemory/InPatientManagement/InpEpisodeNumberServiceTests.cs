using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// Menjaga dua janji <c>BE-RWI-004</c> tentang nomor episode: awalannya dibaca dari master,
/// dan dua permintaan yang datang bersamaan tidak menghasilkan nomor kembar.
/// </summary>
public sealed class InpEpisodeNumberServiceTests
{
    private static readonly Guid ActorUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task Awalan_DibacaDariMasterBukanDitanamDiKode()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = BuildService(db, out _);

        db.Set<MstInpatientSetting>().Add(BuildSetting(prefix: "RWI"));
        await db.SaveChangesAsync();

        var nomor = await service.GenerateAsync();

        Assert.StartsWith("RWI-", nomor);
    }

    [Fact]
    public async Task AwalanDiubahAdmin_NomorBerikutnyaMemakaiAwalanBaru()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = BuildService(db, out _);

        var entity = BuildSetting(prefix: "RI");
        db.Set<MstInpatientSetting>().Add(entity);
        await db.SaveChangesAsync();

        Assert.StartsWith("RI-", await service.GenerateAsync());

        entity.EpisodeNumberPrefix = "RANAP";
        await db.SaveChangesAsync();

        Assert.StartsWith("RANAP-", await service.GenerateAsync());
    }

    [Fact]
    public async Task MasterBelumTerisi_AwalanBawaanRiDipakai()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = BuildService(db, out var logger);

        var nomor = await service.GenerateAsync();

        Assert.StartsWith("RI-", nomor);
        Assert.Equal(1, logger.WarningCount);
    }

    [Fact]
    public async Task DuaPermintaanBersamaan_TidakMenghasilkanNomorKembar()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = BuildService(db, out _);

        db.Set<MstInpatientSetting>().Add(BuildSetting(prefix: "RI"));
        await db.SaveChangesAsync();

        // Waktunya sengaja dipatok sama persis, meniru dua petugas admisi yang menekan
        // Simpan pada detik yang sama. Yang membedakan nomornya adalah enam huruf/angka
        // terakhir, bukan waktunya.
        var waktuSama = new DateTime(2026, 8, 24, 15, 30, 12, DateTimeKind.Utc);

        // Dua puluh, bukan dua ribu. Bagian acaknya enam huruf/angka heksadesimal, yaitu
        // sekitar 16,7 juta kemungkinan. Dua puluh nomor punya peluang tabrakan kira-kira
        // satu berbanding 830.000 — cukup jarang untuk tidak pernah membuat test ini gagal
        // tanpa sebab. Dua ribu nomor peluangnya melonjak menjadi sekitar 12 persen, dan
        // test yang gagal satu dari delapan kali dijalankan akan diabaikan orang.
        //
        // Jaminan sebenarnya bukan angka peluang ini, melainkan unique index
        // IX_InpEpisode_EpisodeNumber di database, yang sudah terbukti menolak pada
        // BE-RWI-003.
        var nomor = Enumerable
            .Range(0, 20)
            .Select(_ => service.Generate("RI", waktuSama))
            .ToArray();

        Assert.Equal(nomor.Length, nomor.Distinct().Count());
        Assert.All(nomor, x => Assert.StartsWith("RI-260824153012-", x));
    }

    [Fact]
    public async Task BentukNomor_MemuatAwalanWaktuDanEnamHurufAngka()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = BuildService(db, out _);

        var nomor = service.Generate("RI", new DateTime(2026, 8, 24, 15, 30, 12, DateTimeKind.Utc));
        var bagian = nomor.Split('-');

        Assert.Equal(3, bagian.Length);
        Assert.Equal("RI", bagian[0]);
        Assert.Equal("260824153012", bagian[1]);
        Assert.Equal(6, bagian[2].Length);

        // Bagian terakhir wajib huruf besar dan angka saja. Nomor ini dibaca dan diketik
        // ulang petugas dari berkas kertas; huruf kecil membuat "l" dan "1" tertukar.
        Assert.All(bagian[2], huruf => Assert.True(char.IsAsciiLetterUpper(huruf) || char.IsAsciiDigit(huruf)));

        // Nomor tidak pernah dibentuk dari hitungan baris. QBE-CODE-003 melarang Count + 1
        // dan Max + 1 sebagai satu-satunya pembentuk nomor, karena dua petugas yang
        // menyimpan bersamaan akan membaca angka yang sama lalu menghasilkan nomor kembar.
        // Panjangnya tetap 22 karakter untuk awalan dua huruf, jauh di bawah batas kolom
        // EpisodeNumber yang 50 karakter.
        Assert.Equal(22, nomor.Length);
        Assert.True(nomor.Length <= 50);
    }

    [Fact]
    public async Task AwalanKosongDiMaster_TidakMenghasilkanNomorTanpaAwalan()
    {
        await using var db = IsolatedInpatientDbContextFactory.Create();
        var service = BuildService(db, out _);

        db.Set<MstInpatientSetting>().Add(BuildSetting(prefix: "   "));
        await db.SaveChangesAsync();

        var nomor = await service.GenerateAsync();

        Assert.StartsWith("RI-", nomor);
        Assert.False(nomor.StartsWith('-'));
    }

    private static InpEpisodeNumberService BuildService(
        QuilvianSystemBackend.Repositories.ApplicationDbContext db,
        out RecordingLogger<InpSettingService> logger)
    {
        logger = new RecordingLogger<InpSettingService>();

        return new InpEpisodeNumberService(new InpSettingService(db, logger));
    }

    private static MstInpatientSetting BuildSetting(string prefix)
        => new()
        {
            Id = Guid.NewGuid(),
            Code = "DEFAULT",
            Name = "Pengaturan Rawat Inap Default",
            BedReservationMinutes = 120,
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
