using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Aturan "member aktif" tunggal — crm-membership `BE-CRM-010`, `FR-CRM-040`, `AT-03`, `AT-09`.
/// </summary>
public class MembershipStatusResolverTests : IDisposable
{
    private static readonly Guid Aktor = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly Guid _tierGold = Guid.NewGuid();

    public MembershipStatusResolverTests()
    {
        using var konteks = _db.CreateContext();
        konteks.Set<MstMembershipTier>().Add(new MstMembershipTier
        {
            Id = _tierGold,
            TierCode = "GOLD",
            TierName = "Gold",
            TierType = MembershipTierType.Gold,
            IsActive = true
        });
        konteks.SaveChanges();
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task Aktif_DalamPeriode_TerbacaMember()
    {
        var pasien = TambahPasien();
        var membership = TambahMembership(pasien, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

        using var konteks = _db.CreateContext();
        var hasil = await Resolver(konteks, Wib(2026, 12, 1, 10, 0)).GetActiveAsync(pasien);

        Assert.NotNull(hasil);
        Assert.Equal(membership, hasil!.PatientMembershipId);
        Assert.Equal("GOLD", hasil.TierCode);
        Assert.Equal(Tgl(2027, 10, 12), hasil.PeriodEndDate);
    }

    [Fact]
    public async Task HariTerakhir_Pukul2330Wib_MasihAktif_Pukul0001Besok_Reguler()
    {
        var pasien = TambahPasien();
        TambahMembership(pasien, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

        using var konteks = _db.CreateContext();

        // 12 Okt 2027 23.30 WIB = 12 Okt 2027 16.30 UTC: masih hari terakhir periode.
        Assert.NotNull(await Resolver(konteks, Wib(2027, 10, 12, 23, 30)).GetActiveAsync(pasien));

        // 13 Okt 2027 00.01 WIB = 12 Okt 2027 17.01 UTC: jebakan UTC — tanggal UTC masih 12 Okt.
        Assert.Null(await Resolver(konteks, Wib(2027, 10, 13, 0, 1)).GetActiveAsync(pasien));
    }

    [Theory]
    [InlineData(MembershipStatus.PendingPayment)]
    [InlineData(MembershipStatus.Scheduled)]
    [InlineData(MembershipStatus.NeedsVerification)]
    [InlineData(MembershipStatus.Expired)]
    [InlineData(MembershipStatus.Cancelled)]
    [InlineData(MembershipStatus.Upgraded)]
    public async Task StatusSelainAktif_TerbacaReguler(MembershipStatus status)
    {
        var pasien = TambahPasien();
        TambahMembership(pasien, status, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

        using var konteks = _db.CreateContext();
        Assert.Null(await Resolver(konteks, Wib(2026, 12, 1, 10, 0)).GetActiveAsync(pasien));
    }

    [Fact]
    public async Task AktifTanpaPeriodeLengkap_TerbacaReguler()
    {
        var pasien = TambahPasien();
        TambahMembership(pasien, MembershipStatus.Active, null, Tgl(2027, 10, 12));

        using var konteks = _db.CreateContext();
        Assert.Null(await Resolver(konteks, Wib(2026, 12, 1, 10, 0)).GetActiveAsync(pasien));
    }

    [Fact]
    public async Task SebelumTanggalMulai_TerbacaReguler()
    {
        var pasien = TambahPasien();
        TambahMembership(pasien, MembershipStatus.Active, Tgl(2027, 10, 13), Tgl(2028, 10, 12));

        using var konteks = _db.CreateContext();
        Assert.Null(await Resolver(konteks, Wib(2027, 10, 12, 12, 0)).GetActiveAsync(pasien));
    }

    [Fact]
    public async Task TingkatNonaktif_AtauMembershipTerhapus_TerbacaReguler()
    {
        var pasienA = TambahPasien();
        TambahMembership(pasienA, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12), terhapus: true);

        var tierNonaktif = Guid.NewGuid();
        using (var k = _db.CreateContext())
        {
            k.Set<MstMembershipTier>().Add(new MstMembershipTier
            {
                Id = tierNonaktif, TierCode = "OLD", TierName = "Lama", TierType = MembershipTierType.Silver, IsActive = false
            });
            k.SaveChanges();
        }
        var pasienB = TambahPasien();
        TambahMembership(pasienB, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12), tierId: tierNonaktif);

        using var konteks = _db.CreateContext();
        var hasil = await Resolver(konteks, Wib(2026, 12, 1, 10, 0)).GetActiveForPatientsAsync([pasienA, pasienB]);

        Assert.Empty(hasil);
    }

    [Fact]
    public async Task Batch_HanyaPasienAktifYangMuncul()
    {
        var aktif = TambahPasien();
        var menunggu = TambahPasien();
        var tanpa = TambahPasien();
        TambahMembership(aktif, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        TambahMembership(menunggu, MembershipStatus.PendingPayment, null, null);

        using var konteks = _db.CreateContext();
        var hasil = await Resolver(konteks, Wib(2026, 12, 1, 10, 0)).GetActiveForPatientsAsync([aktif, menunggu, tanpa]);

        Assert.Equal([aktif], hasil.Keys);
    }

    [Fact]
    public async Task SyncProyeksi_MengisiLaluMengosongkan_DanTidakMenulisBilaSama()
    {
        var pasien = TambahPasien(isMember: true);
        var membership = TambahMembership(pasien, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));
        var waktu = Wib(2026, 12, 1, 10, 0);

        using (var konteks = _db.CreateContext())
        {
            Assert.True(await Resolver(konteks, waktu).SyncPatientProjectionAsync(pasien, Aktor));
            await konteks.SaveChangesAsync();
        }

        using (var konteks = _db.CreateContext())
        {
            var p = konteks.Set<MstPatient>().AsNoTracking().Single(x => x.Id == pasien);
            Assert.True(p.IsMember);
            Assert.Equal(_tierGold, p.DefaultMembershipTierId);
            Assert.Equal(membership, p.ActivePatientMembershipId);
            Assert.Equal(Aktor, p.UpdateBy);

            Assert.False(await Resolver(konteks, waktu).SyncPatientProjectionAsync(pasien, Aktor));
        }

        // Setelah kedaluwarsa (lewat tanggal akhir), proyeksi dikosongkan.
        using (var konteks = _db.CreateContext())
        {
            Assert.True(await Resolver(konteks, Wib(2027, 10, 13, 0, 5)).SyncPatientProjectionAsync(pasien, Aktor));
            await konteks.SaveChangesAsync();
        }

        using (var konteks = _db.CreateContext())
        {
            var p = konteks.Set<MstPatient>().AsNoTracking().Single(x => x.Id == pasien);
            Assert.False(p.IsMember);
            Assert.Null(p.DefaultMembershipTierId);
            Assert.Null(p.ActivePatientMembershipId);
        }
    }

    [Fact]
    public void QueryActiveOn_TerjemahKePostgreSql_TanpaKoneksi()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql("Host=localhost;Database=uji_tanpa_koneksi")
            .Options;
        using var konteks = new ApplicationDbContext(options);

        var sql = Resolver(konteks, Wib(2026, 12, 1, 10, 0)).QueryActiveOn(Tgl(2026, 12, 1)).ToQueryString();

        Assert.Contains("\"PeriodStartDate\" <=", sql);
        Assert.Contains("\"ExpiredDate\" >=", sql);
        Assert.Contains("\"MembershipStatus\" = 1", sql);
    }

    [Fact]
    public void Di_MembuatResolverTanpaTimeProviderTerdaftar()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddScoped(_ => _db.CreateContext());
        services.AddScoped<PatientMembershipStatusResolver>();

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        using var scope = provider.CreateScope();

        var resolver = scope.ServiceProvider.GetRequiredService<PatientMembershipStatusResolver>();

        Assert.Equal(DateTimeKind.Unspecified, resolver.GetTodayWib().Kind);
    }

    private PatientMembershipStatusResolver Resolver(ApplicationDbContext konteks, DateTimeOffset sekarang) =>
        new(konteks, new ConfigurationBuilder().Build(), new JamTetap(sekarang));

    private Guid TambahPasien(bool isMember = false)
    {
        var id = Guid.NewGuid();
        using var konteks = _db.CreateContext();
        konteks.Set<MstPatient>().Add(new MstPatient
        {
            Id = id,
            PatientCode = "PAT-" + id.ToString("N")[..8],
            MedicalRecordNumber = "RM-" + id.ToString("N")[..8],
            FullName = "Pasien Uji",
            IsMember = isMember
        });
        konteks.SaveChanges();
        return id;
    }

    private Guid TambahMembership(
        Guid pasien, MembershipStatus status, DateTime? mulai, DateTime? akhir,
        bool terhapus = false, Guid? tierId = null)
    {
        var id = Guid.NewGuid();
        using var konteks = _db.CreateContext();
        konteks.Set<MstPatientMembership>().Add(new MstPatientMembership
        {
            Id = id,
            PatientId = pasien,
            MembershipTierId = tierId ?? _tierGold,
            MemberNumber = "MBR-" + id.ToString("N")[..10],
            MembershipStatus = status,
            JoinDate = Tgl(2026, 10, 13),
            PeriodStartDate = mulai,
            ExpiredDate = akhir,
            IsDelete = terhapus
        });
        konteks.SaveChanges();
        return id;
    }

    private static DateTime Tgl(int y, int m, int d) => new(y, m, d, 0, 0, 0, DateTimeKind.Unspecified);

    private static DateTimeOffset Wib(int y, int m, int d, int jam, int menit) =>
        new(y, m, d, jam, menit, 0, TimeSpan.FromHours(7));

    private sealed class JamTetap(DateTimeOffset sekarang) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => sekarang.ToUniversalTime();
    }
}
