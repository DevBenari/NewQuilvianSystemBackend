using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// crm-membership `BE-CRM-003` (iuran di master tingkat) dan `BE-CRM-023` (respons baca
/// diperluas), diuji langsung pada controller dengan basis data SQLite di memori.
/// </summary>
public class MembershipReadAndTierControllerTests : IDisposable
{
    private static readonly Guid Aktor = Guid.Parse("22222222-2222-2222-2222-222222222222");

    // "Hari ini" uji: 1 Des 2026 10.00 WIB.
    private static readonly DateTimeOffset Sekarang = new(2026, 12, 1, 10, 0, 0, TimeSpan.FromHours(7));

    private readonly TestDatabase _db = TestDatabase.Create();
    private readonly TestDatabase _dbNomor = TestDatabase.Create();
    private readonly TiruanPortTagihanIuran _portTagihan = new(() => Sekarang);

    public void Dispose()
    {
        _db.Dispose();
        _dbNomor.Dispose();
    }

    // ---------- BE-CRM-003 ----------

    [Fact]
    public async Task Tingkat_IuranTersimpanDanTerbacaKembali()
    {
        using var konteks = _db.CreateContext();
        var controller = TierController(konteks);

        var buat = await controller.CreateMembershipTier(new CreateMembershipTierRequest
        {
            TierName = "Gold",
            TierType = MembershipTierType.Gold,
            AnnualFeeAmount = 2_400_000m
        });
        var dibuat = Data<MembershipTierCreateResponse>(buat);

        var detail = Data<MembershipTierDetailResponse>(await controller.GetMembershipTierById(dibuat.Id));
        Assert.Equal(2_400_000m, detail.AnnualFeeAmount);
    }

    [Fact]
    public async Task Tingkat_IuranNegatif_Ditolak400()
    {
        using var konteks = _db.CreateContext();

        var hasil = await TierController(konteks).CreateMembershipTier(new CreateMembershipTierRequest
        {
            TierName = "Salah",
            TierType = MembershipTierType.Silver,
            AnnualFeeAmount = -1m
        });

        var bad = Assert.IsType<BadRequestObjectResult>(hasil);
        Assert.Equal("Nominal iuran tidak boleh kurang dari 0.", Assert.IsType<ApiResponse<object>>(bad.Value).Message);
    }

    [Fact]
    public async Task PilihanTingkat_RegistrableOnly_HanyaEmpatTingkatBerbayar()
    {
        using (var k = _db.CreateContext())
        {
            foreach (var tipe in Enum.GetValues<MembershipTierType>())
            {
                k.Set<MstMembershipTier>().Add(new MstMembershipTier
                {
                    Id = Guid.NewGuid(), TierCode = "T-" + tipe, TierName = tipe.ToString(), TierType = tipe, IsActive = true
                });
            }
            k.SaveChanges();
        }

        using var konteks = _db.CreateContext();
        var hasil = Data<MembershipTierOptionPagedResponse>(
            await TierController(konteks).GetMembershipTierOptions(registrableOnly: true, pageSize: 50));

        Assert.Equal(
            new[] { MembershipTierType.Silver, MembershipTierType.Gold, MembershipTierType.Platinum, MembershipTierType.Executive }.OrderBy(x => x),
            hasil.Items.Select(x => x.TierType).OrderBy(x => x));
    }

    // ---------- BE-CRM-023 ----------

    [Fact]
    public async Task Daftar_FilterStatusMenunggu_HanyaMenunggu_DanFieldBaruTerisi()
    {
        var (tier, _) = Semai(out var menunggu, out _, out _);

        using var konteks = _db.CreateContext();
        var hasil = Data<PagedResult<PatientMembershipResponse>>(
            await MembershipController(konteks).GetPatientMembershipsForAdmin(
                null, null, null, null, null, null, null, membershipStatus: MembershipStatus.PendingPayment));

        var item = Assert.Single(hasil.Items);
        Assert.Equal(menunggu, item.Id);
        Assert.Equal(MembershipRegistrationType.New, item.RegistrationType);
        Assert.Equal(2_400_000m, item.FeeAmountSnapshot);
        Assert.NotNull(item.PaymentDueAt);
        Assert.NotEqual(Guid.Empty, item.RowVersion);
    }

    [Fact]
    public async Task Daftar_BerakhirDalam30Hari_HanyaAktifYangJatuhTempo()
    {
        Semai(out _, out var segeraBerakhir, out _);

        using var konteks = _db.CreateContext();
        var hasil = Data<PagedResult<PatientMembershipResponse>>(
            await MembershipController(konteks).GetPatientMembershipsForAdmin(
                null, null, null, null, null, null, null, expiringWithinDays: 30));

        Assert.Equal([segeraBerakhir], hasil.Items.Select(x => x.Id));
    }

    [Fact]
    public async Task Ringkasan_MenghitungStatusBaru()
    {
        Semai(out _, out _, out _);

        using var konteks = _db.CreateContext();
        var hasil = Data<PatientMembershipSummaryResponse>(await MembershipController(konteks).GetSummary());

        Assert.Equal(1, hasil.PendingPaymentPatientMembership);
        Assert.Equal(1, hasil.NeedsVerificationPatientMembership);
        Assert.Equal(1, hasil.CancelledPatientMembership);
        Assert.Equal(0, hasil.ScheduledPatientMembership);
        Assert.Equal(1, hasil.ExpiringIn30DaysPatientMembership);
    }

    [Fact]
    public async Task Detail_MemuatAlasanBatal_DaftarTidak()
    {
        Semai(out _, out _, out var batal);

        using var konteks = _db.CreateContext();
        var controller = MembershipController(konteks);
        var detail = Data<PatientMembershipDetailResponse>(await controller.GetPatientMembershipById(batal));

        Assert.Equal(MembershipCancellationCategory.PatientRequest, detail.CancellationCategory);
        Assert.Equal("Pasien pindah kota", detail.CancellationReason);
        Assert.Null(typeof(PatientMembershipResponse).GetProperty(nameof(PatientMembershipDetailResponse.CancellationReason),
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.DeclaredOnly));
    }

    // ---------- bantuan ----------

    /// <summary>Lima membership: Menunggu, Aktif berakhir 20 hari lagi, Aktif berakhir 200 hari lagi, Perlu Verifikasi, Dibatalkan.</summary>
    private (Guid Tier, Guid Pasien) Semai(out Guid menunggu, out Guid segeraBerakhir, out Guid batal)
    {
        var tier = Guid.NewGuid();
        using var k = _db.CreateContext();
        k.Set<MstMembershipTier>().Add(new MstMembershipTier
        {
            Id = tier, TierCode = "GOLD", TierName = "Gold", TierType = MembershipTierType.Gold, IsActive = true, AnnualFeeAmount = 2_400_000m
        });

        Guid Pasien()
        {
            var id = Guid.NewGuid();
            k.Set<MstPatient>().Add(new MstPatient { Id = id, PatientCode = "P-" + id.ToString("N")[..6], MedicalRecordNumber = "RM-" + id.ToString("N")[..6], FullName = "Pasien Uji" });
            return id;
        }

        Guid Tambah(MembershipStatus status, DateTime? mulai, DateTime? akhir, Action<MstPatientMembership>? atur = null)
        {
            var m = new MstPatientMembership
            {
                Id = Guid.NewGuid(), PatientId = Pasien(), MembershipTierId = tier, MemberNumber = "MBR-" + Guid.NewGuid().ToString("N")[..10],
                MembershipStatus = status, JoinDate = new DateTime(2026, 10, 1), PeriodStartDate = mulai, ExpiredDate = akhir,
                FeeAmountSnapshot = 2_400_000m
            };
            atur?.Invoke(m);
            k.Set<MstPatientMembership>().Add(m);
            return m.Id;
        }

        menunggu = Tambah(MembershipStatus.PendingPayment, null, null, m => m.PaymentDueAt = Sekarang.AddDays(7));
        segeraBerakhir = Tambah(MembershipStatus.Active, new DateTime(2025, 12, 21), new DateTime(2026, 12, 20));
        Tambah(MembershipStatus.Active, new DateTime(2026, 6, 19), new DateTime(2027, 6, 18));
        Tambah(MembershipStatus.NeedsVerification, null, null, m => m.RegistrationType = MembershipRegistrationType.LegacyImport);
        batal = Tambah(MembershipStatus.Cancelled, null, null, m =>
        {
            m.CancellationCategory = MembershipCancellationCategory.PatientRequest;
            m.CancellationReason = "Pasien pindah kota";
        });

        k.SaveChanges();
        return (tier, Guid.Empty);
    }

    private static MembershipTierController TierController(ApplicationDbContext konteks) =>
        WithUser(new MembershipTierController(konteks, Logger()));

    private PatientMembershipController MembershipController(ApplicationDbContext konteks)
    {
        var config = new ConfigurationBuilder().Build();
        var jam = new JamTetap(Sekarang);
        var resolver = new PatientMembershipStatusResolver(konteks, config, jam);
        var service = new PatientMembershipService(
            konteks, new NumberSeriesAllocator(new PabrikKonteks(_dbNomor)), resolver, Logger(), _portTagihan, config, jam);
        return WithUser(new PatientMembershipController(konteks, Logger(), resolver, service, _portTagihan));
    }

    private sealed class PabrikKonteks(TestDatabase db) : Microsoft.EntityFrameworkCore.IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => db.CreateContext();
    }

    private static LoggerService Logger() =>
        new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    private static T WithUser<T>(T controller) where T : ControllerBase
    {
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Aktor.ToString())], "uji"))
            }
        };
        return controller;
    }

    private static T Data<T>(IActionResult hasil)
    {
        var ok = Assert.IsType<OkObjectResult>(hasil);
        return Assert.IsType<ApiResponse<T>>(ok.Value).Data!;
    }

    private sealed class JamTetap(DateTimeOffset sekarang) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => sekarang.ToUniversalTime();
    }
}
