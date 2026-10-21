using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using static QuilvianSystemBackend.Tests.PatientManagement.Infrastructure.MembershipHarness;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Pekerjaan harian (`BE-CRM-020`: `FR-CRM-020` s.d. `023`, `AT-06`, `AT-08`) dan pembacaan kiosk
/// (`BE-CRM-021`: `FR-CRM-043`, `AT-23`) crm-membership.
/// </summary>
public class MembershipLifecycleAndKioskTests : IDisposable
{
    // Gold berlaku 13 Okt 2026 s.d. 12 Okt 2027. Hari uji bawaan: 13 Okt 2027 00.05 WIB.
    private readonly MembershipHarness _h = new(Wib(2027, 10, 13, 0, 5));
    private readonly Guid _gold;
    private readonly Guid _pasien;

    public MembershipLifecycleAndKioskTests()
    {
        _gold = _h.TambahTier("Gold", MembershipTierType.Gold, 2);
        _pasien = _h.TambahPasien();
    }

    public void Dispose() => _h.Dispose();

    private async Task<MembershipLifecycleRunResult> Jalankan()
    {
        using var k = _h.Konteks();
        return await _h.Service(k).RunDailyLifecycleAsync();
    }

    private List<PatMembershipStatusHistory> Riwayat(Guid id)
    {
        using var k = _h.Konteks();
        return k.Set<PatMembershipStatusHistory>().AsNoTracking().Where(x => x.PatientMembershipId == id).ToList();
    }

    private MstPatient Pasien(Guid? id = null)
    {
        using var k = _h.Konteks();
        return k.Set<MstPatient>().AsNoTracking().Single(x => x.Id == (id ?? _pasien));
    }

    private void Proyeksi(Guid pasien, bool isMember, Guid? membershipId)
    {
        using var k = _h.Konteks();
        var p = k.Set<MstPatient>().Single(x => x.Id == pasien);
        p.IsMember = isMember;
        p.ActivePatientMembershipId = membershipId;
        p.DefaultMembershipTierId = membershipId == null ? null : _gold;
        k.SaveChanges();
    }

    // ---------- BE-CRM-020 ----------

    [Fact]
    public async Task AktifLewatTanggalAkhir_Kedaluwarsa_OlehSistem_ProyeksiKosong()
    {
        var aktif = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        Proyeksi(_pasien, true, aktif.Id);

        var hasil = await Jalankan();

        Assert.Equal(1, hasil.Expired);
        var m = _h.Baca(aktif.Id);
        Assert.Equal(MembershipStatus.Expired, m.MembershipStatus);
        Assert.False(m.IsActive);
        var r = Assert.Single(Riwayat(aktif.Id));
        Assert.Equal("EXPIRED", r.ActionCode);
        Assert.Equal(MembershipChangeSource.System, r.Source);
        Assert.Null(r.ActorUserId);
        Assert.False(Pasien().IsMember);
    }

    [Fact]
    public async Task HariTerakhir_BelumKedaluwarsa()
    {
        _h.Sekarang = Wib(2027, 10, 12, 23, 30);
        var aktif = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

        var hasil = await Jalankan();

        Assert.Equal(0, hasil.Expired);
        Assert.Equal(MembershipStatus.Active, _h.Baca(aktif.Id).MembershipStatus);
    }

    [Fact]
    public async Task PeriodeLamaBerakhir_TerjadwalLangsungAktif_ProyeksiPindah()
    {
        var lama = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        var baru = _h.TambahMembership(_pasien, _gold, MembershipStatus.Scheduled, Tgl(2027, 10, 13), Tgl(2028, 10, 12), MembershipRegistrationType.Renewal);
        Proyeksi(_pasien, true, lama.Id);

        var hasil = await Jalankan();

        Assert.Equal(1, hasil.Expired);
        Assert.Equal(1, hasil.ScheduledStarted);
        Assert.Equal(MembershipStatus.Expired, _h.Baca(lama.Id).MembershipStatus);
        var b = _h.Baca(baru.Id);
        Assert.Equal(MembershipStatus.Active, b.MembershipStatus);
        Assert.True(b.IsActive);
        Assert.NotNull(b.ActivatedAt);
        Assert.Equal("SCHEDULE_STARTED", Assert.Single(Riwayat(baru.Id)).ActionCode);
        Assert.Equal(baru.Id, Pasien().ActivePatientMembershipId);
    }

    [Fact]
    public async Task TerjadwalBelumWaktunya_TidakDisentuh()
    {
        _h.Sekarang = Wib(2027, 10, 1);
        _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        var baru = _h.TambahMembership(_pasien, _gold, MembershipStatus.Scheduled, Tgl(2027, 10, 13), Tgl(2028, 10, 12), MembershipRegistrationType.Renewal);

        await Jalankan();

        Assert.Equal(MembershipStatus.Scheduled, _h.Baca(baru.Id).MembershipStatus);
    }

    [Fact]
    public async Task ServerMatiDuaHari_KetertinggalanDikejar()
    {
        _h.Sekarang = Wib(2027, 10, 15, 8, 0);
        var lama = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        var baru = _h.TambahMembership(_pasien, _gold, MembershipStatus.Scheduled, Tgl(2027, 10, 13), Tgl(2028, 10, 12), MembershipRegistrationType.Renewal);

        await Jalankan();

        Assert.Equal(MembershipStatus.Expired, _h.Baca(lama.Id).MembershipStatus);
        Assert.Equal(MembershipStatus.Active, _h.Baca(baru.Id).MembershipStatus);
    }

    [Fact]
    public async Task MenungguLewatBatasBayar_DibatalkanTidakDibayar_YangBelumLewatTetap()
    {
        var lewat = _h.TambahMembership(_pasien, _gold, MembershipStatus.PendingPayment);
        var pasienLain = _h.TambahPasien();
        var belum = _h.TambahMembership(pasienLain, _gold, MembershipStatus.PendingPayment);
        using (var k = _h.Konteks())
        {
            k.Set<MstPatientMembership>().Single(x => x.Id == lewat.Id).PaymentDueAt = Wib(2027, 10, 12, 9, 0);
            k.Set<MstPatientMembership>().Single(x => x.Id == belum.Id).PaymentDueAt = Wib(2027, 10, 19, 9, 0);
            k.SaveChanges();
        }

        var hasil = await Jalankan();

        Assert.Equal(1, hasil.UnpaidCancelled);
        var m = _h.Baca(lewat.Id);
        Assert.Equal(MembershipStatus.Cancelled, m.MembershipStatus);
        Assert.Equal(MembershipCancellationCategory.Unpaid, m.CancellationCategory);
        var riwayat = Riwayat(lewat.Id);
        Assert.Contains(riwayat, x => x.ActionCode == "UNPAID_CANCELLED" && x.Source == MembershipChangeSource.System);
        Assert.Contains(riwayat, x => x.ActionCode == "FEE_CHARGE_CANCEL_REQUESTED");
        Assert.Equal(MembershipStatus.PendingPayment, _h.Baca(belum.Id).MembershipStatus);
    }

    [Fact]
    public async Task DijalankanDuaKali_TidakMenambahRiwayat()
    {
        var aktif = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        var menunggu = _h.TambahMembership(_h.TambahPasien(), _gold, MembershipStatus.PendingPayment);
        using (var k = _h.Konteks())
        {
            k.Set<MstPatientMembership>().Single(x => x.Id == menunggu.Id).PaymentDueAt = Wib(2027, 10, 1);
            k.SaveChanges();
        }

        await Jalankan();
        var kedua = await Jalankan();

        Assert.Equal(0, kedua.Expired + kedua.ScheduledStarted + kedua.UnpaidCancelled + kedua.ProjectionsSynced);
        Assert.Single(Riwayat(aktif.Id));
        Assert.Equal(2, Riwayat(menunggu.Id).Count);
    }

    [Fact]
    public async Task SapuProyeksi_MemberPalsuDikosongkan_MemberSahDiisi()
    {
        var palsu = _h.TambahPasien();
        Proyeksi(palsu, true, null);
        var aktif = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2027, 1, 1), Tgl(2027, 12, 31));
        Proyeksi(_pasien, false, null);

        var hasil = await Jalankan();

        Assert.Equal(2, hasil.ProjectionsSynced);
        Assert.False(Pasien(palsu).IsMember);
        Assert.True(Pasien().IsMember);
        Assert.Equal(aktif.Id, Pasien().ActivePatientMembershipId);
    }

    [Fact]
    public void JedaSampaiJalanBerikutnya_Pukul0005Wib()
    {
        var wib = TimeZoneInfo.CreateCustomTimeZone("WIB", TimeSpan.FromHours(7), "WIB", "WIB");
        var jamJalan = new TimeSpan(0, 5, 0);

        Assert.Equal(new TimeSpan(15, 5, 0),
            PatientMembershipLifecycleHostedService.DelayUntilNextRun(Wib(2026, 12, 1, 9, 0), wib, jamJalan));
        Assert.Equal(TimeSpan.FromMinutes(4),
            PatientMembershipLifecycleHostedService.DelayUntilNextRun(Wib(2026, 12, 2, 0, 1), wib, jamJalan));
        Assert.Equal(TimeSpan.FromHours(24),
            PatientMembershipLifecycleHostedService.DelayUntilNextRun(Wib(2026, 12, 2, 0, 5), wib, jamJalan));
    }

    [Fact]
    public void Di_MembuatHostedService_DenganOpsiBawaanMenyala()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddOptions<PatientMembershipLifecycleOptions>();
        services.AddSingleton<PatientMembershipLifecycleHostedService>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        Assert.NotNull(provider.GetRequiredService<PatientMembershipLifecycleHostedService>());
        Assert.True(provider.GetRequiredService<Microsoft.Extensions.Options.IOptionsMonitor<PatientMembershipLifecycleOptions>>().CurrentValue.Enabled);
    }

    // ---------- BE-CRM-021 ----------

    private async Task<KioskPatientLookupResult?> CariKiosk(string nomorMember)
    {
        using var k = _h.Konteks();
        var resolver = new PatientMembershipStatusResolver(k, new ConfigurationBuilder().Build(), new JamTetapUji(_h.Sekarang));
        var service = new KioskPatientLookupService(k, NullLogger<KioskPatientLookupService>.Instance, resolver);
        var hasil = await service.LookupAsync(
            new KioskPatientLookupRequest { SearchType = KioskPatientLookupSearchType.MemberNumber, Value = nomorMember }, null);
        return hasil.Data?.Result;
    }

    [Fact]
    public async Task Kiosk_NomorMemberAktif_Ditemukan_YangKedaluwarsaDanMenungguTidak()
    {
        _h.Sekarang = Wib(2027, 5, 1);
        var aktif = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        var kedaluwarsa = _h.TambahMembership(_h.TambahPasien(), _gold, MembershipStatus.Active, Tgl(2025, 10, 13), Tgl(2026, 10, 12));
        var menunggu = _h.TambahMembership(_h.TambahPasien(), _gold, MembershipStatus.PendingPayment);

        Assert.Equal(KioskPatientLookupResult.Found, await CariKiosk(aktif.MemberNumber));
        // Status masih Aktif tetapi tanggal akhir sudah lewat (proses harian belum jalan): tetap tidak dianggap member.
        Assert.Equal(KioskPatientLookupResult.NotFound, await CariKiosk(kedaluwarsa.MemberNumber));
        Assert.Equal(KioskPatientLookupResult.NotFound, await CariKiosk(menunggu.MemberNumber));
    }

    [Fact]
    public async Task PilihanMembership_HanyaYangAktifDalamPeriode()
    {
        _h.Sekarang = Wib(2027, 5, 1);
        var aktif = _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        _h.TambahMembership(_h.TambahPasien(), _gold, MembershipStatus.Active, Tgl(2025, 10, 13), Tgl(2026, 10, 12));
        _h.TambahMembership(_h.TambahPasien(), _gold, MembershipStatus.NeedsVerification);

        using var k = _h.Konteks();
        var ok = Assert.IsType<OkObjectResult>(await _h.Controller(k).GetPatientMembershipOptions());
        var data = Assert.IsType<ApiResponse<PatientMembershipOptionPagedResponse>>(ok.Value).Data!;

        Assert.Equal([aktif.Id], data.Items.Select(x => x.Id));
    }

    private sealed class JamTetapUji(DateTimeOffset sekarang) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => sekarang.ToUniversalTime();
    }
}
