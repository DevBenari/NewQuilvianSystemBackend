using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Hubs;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Tests.HumanResource;

// BE-HRD-240 / AT-HRD-TRN-04 (VR-TRN-09) — siaran peringatan sertifikat pelatihan wajib yang hampir/sudah kedaluwarsa ke STAF
// lewat pusat pemberitahuan dalam aplikasi (kontrak: "ke staf"; WhatsApp/email belum ada di platform dan dicatat sebagai kanal
// yang diminta). Hari ini = tanggal UTC saat uji berjalan.
public sealed class TrainingExpiryRadarBroadcastTests : IDisposable
{
    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _ani = Guid.NewGuid(), _budi = Guid.NewGuid(), _citra = Guid.NewGuid(), _dodi = Guid.NewGuid(), _eko = Guid.NewGuid(), _fani = Guid.NewGuid();
    private readonly Guid _akunAni = Guid.NewGuid(), _akunBudi = Guid.NewGuid(), _akunCitra = Guid.NewGuid();
    private static readonly DateTime Hari = DateTime.UtcNow.Date;
    private int _urut;

    // Dorongan realtime hanya kenyamanan dan kegagalannya ditangkap service; hub tidak dibutuhkan untuk menyimpan pemberitahuan.
    private sealed class HubTidakAda : IHubContext<HrNotificationHub>
    {
        public IHubClients Clients => throw new NotSupportedException("SignalR tidak dipakai di harness");
        public IGroupManager Groups => throw new NotSupportedException("SignalR tidak dipakai di harness");
    }

    public TrainingExpiryRadarBroadcastTests()
    {
        using var ctx = _db.CreateContext();
        foreach (var (id, nama) in new[] { (_ani, "Ani"), (_budi, "Budi"), (_citra, "Citra"), (_dodi, "Dodi"), (_eko, "Eko"), (_fani, "Fani") })
            ctx.MstWorkforceProfiles.Add(new MstWorkforceProfile { Id = id, ProfileCode = "P-" + nama, DisplayName = nama, UserType = UserType.Employee, IsActive = true });
        ApplicationUser Akun(Guid id, Guid wf, bool aktif) => new() { Id = id, UserName = id.ToString(), UserCode = id.ToString("N")[..8], DisplayName = "Uji", UserType = UserType.Employee, IsActive = aktif, WorkforceProfileId = wf };
        ctx.Users.AddRange(Akun(_akunAni, _ani, true), Akun(_akunBudi, _budi, true), Akun(_akunCitra, _citra, false));   // Dodi tanpa akun

        Sertifikat(ctx, _ani, -2);              // sudah kedaluwarsa
        Sertifikat(ctx, _budi, 20);             // H-30 kritis
        Sertifikat(ctx, _budi, 80);             // H-90 (staf yang sama, sertifikat kedua)
        Sertifikat(ctx, _citra, 45);            // H-60, akun nonaktif
        Sertifikat(ctx, _dodi, 75);             // H-90, tanpa akun
        Sertifikat(ctx, _eko, 120);             // di luar radar 90 hari
        Sertifikat(ctx, _fani, -2, aktif: false); // sertifikat nonaktif tidak dipantau
        ctx.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    private void Sertifikat(Repositories.ApplicationDbContext ctx, Guid wf, int sisaHari, bool aktif = true) =>
        ctx.TrxTrainingCertificates.Add(new TrxTrainingCertificate
        {
            WorkforceProfileId = wf, TrainingParticipantId = Guid.NewGuid(), TrainingResultId = Guid.NewGuid(), CertificateNumber = $"CERT-{++_urut:000}",
            IssuedDate = Hari.AddYears(-2), ExpiredDate = Hari.AddDays(sisaHari), IsActive = aktif,
        });

    private TrainingExpiryRadarService Layanan()
    {
        var ctx = _db.CreateContext();
        return new TrainingExpiryRadarService(ctx, new HrdNotificationService(ctx, new HubTidakAda(), NullLogger<HrdNotificationService>.Instance));
    }

    private Task<BroadcastExpiryAlertResponseDto> Siarkan(string filter) =>
        Layanan().BroadcastAlertsAsync(new BroadcastExpiryAlertRequestDto { TargetZoneFilter = filter, DeliveryChannels = new() { "ESS", "WHATSAPP" } });

    private List<Areas.Corporate.HumanResource.HrServiceManagement.Models.HrdNotification> Pemberitahuan()
    {
        using var ctx = _db.CreateContext();
        return ctx.HrdNotifications.AsNoTracking().OrderBy(x => x.Title).ToList();
    }

    // VR-TRN-09: zona dihitung dari sisa hari; sertifikat di luar 90 hari dan sertifikat nonaktif tidak dipantau.
    [Fact]
    public async Task Radar_mengelompokkan_zona_H90_H60_H30_dan_kedaluwarsa()
    {
        var r = await Layanan().GetExpiringRadarSummaryAsync();
        Assert.Equal((5, 2, 1, 1, 1), (r.TotalMonitoredCertificates, r.ZoneH90Count, r.ZoneH60Count, r.ZoneH30CriticalCount, r.OverdueH0Count));
        Assert.DoesNotContain(r.Items, x => x.WorkforceProfileId == _eko || x.WorkforceProfileId == _fani);
        Assert.Equal(("OVERDUE_H0", -2), (r.Items.Single(x => x.WorkforceProfileId == _ani).RadarZone, r.Items.Single(x => x.WorkforceProfileId == _ani).DaysUntilExpiry));
    }

    // AT-HRD-TRN-04: siaran semua zona → satu pemberitahuan per staf berakun aktif, berisi seluruh sertifikatnya; staf tanpa akun atau
    // berakun nonaktif dihitung sebagai penerima tetapi tidak sebagai pengiriman; kanal yang diminta tercatat.
    [Fact]
    public async Task TRN04_siaran_massal_satu_pemberitahuan_per_staf_berakun()
    {
        var r = await Siarkan("ALL");
        Assert.Equal((4, 2), (r.TotalRecipients, r.SuccessfulDispatches));
        Assert.Contains("2 pemberitahuan dalam aplikasi terkirim ke 4 staf (5 sertifikat)", r.Message);
        Assert.Contains("Kanal email/WhatsApp belum tersedia", r.Message);

        var p = Pemberitahuan();
        Assert.Equal(new[] { _akunAni, _akunBudi }.OrderBy(x => x), p.Select(x => x.RecipientUserId).OrderBy(x => x));
        var ani = p.Single(x => x.RecipientUserId == _akunAni);
        var budi = p.Single(x => x.RecipientUserId == _akunBudi);
        Assert.Equal(("Sertifikat pelatihan wajib Anda sudah kedaluwarsa", HrdNotificationPriorities.High), (ani.Title, ani.Priority));
        Assert.Contains("sudah kedaluwarsa", ani.Body);
        Assert.Equal(("Sertifikat pelatihan wajib Anda akan kedaluwarsa", HrdNotificationPriorities.Normal), (budi.Title, budi.Priority));
        Assert.Contains("20 hari lagi", budi.Body);
        Assert.Contains("80 hari lagi", budi.Body);
        Assert.True(budi.Body.IndexOf("20 hari lagi", StringComparison.Ordinal) < budi.Body.IndexOf("80 hari lagi", StringComparison.Ordinal));
        Assert.All(p, x =>
        {
            Assert.Equal((HrdNotificationCategories.Training, "InApp", "ESS,WHATSAPP", "/self-service/training/my-certificates"), (x.Category, x.Channel, x.RequestedChannel, x.DeepLinkUrl));
            Assert.True(x.RecipientWorkforceProfileId == _ani || x.RecipientWorkforceProfileId == _budi);
        });
    }

    // Siaran ulang pada hari yang sama dengan filter yang sama tidak menggandakan pemberitahuan.
    [Fact]
    public async Task Siaran_ulang_hari_yang_sama_tidak_dobel()
    {
        await Siarkan("ALL");
        var ulang = await Siarkan("ALL");
        Assert.Equal((4, 0), (ulang.TotalRecipients, ulang.SuccessfulDispatches));
        Assert.Equal(2, Pemberitahuan().Count);
    }

    // Filter zona: OVERDUE_ONLY hanya Ani; H30_AND_OVERDUE (bawaan) Ani dan Budi.
    [Theory]
    [InlineData("OVERDUE_ONLY", 1, 1)]
    [InlineData("H30_AND_OVERDUE", 2, 2)]
    public async Task Filter_zona_membatasi_penerima(string filter, int penerima, int terkirim)
    {
        var r = await Siarkan(filter);
        Assert.Equal((penerima, terkirim), (r.TotalRecipients, r.SuccessfulDispatches));
        Assert.Equal(terkirim, Pemberitahuan().Count);
    }

    [Fact]
    public async Task Radar_kosong_tidak_mengirim_apa_pun()
    {
        using (var ctx = _db.CreateContext())
        {
            ctx.TrxTrainingCertificates.RemoveRange(ctx.TrxTrainingCertificates);
            ctx.SaveChanges();
        }
        var r = await Siarkan("ALL");
        Assert.Equal((0, 0), (r.TotalRecipients, r.SuccessfulDispatches));
        Assert.Equal("Tidak ada staf dengan sertifikat pelatihan wajib pada zona radar yang dipilih.", r.Message);
        Assert.Empty(Pemberitahuan());
    }
}
