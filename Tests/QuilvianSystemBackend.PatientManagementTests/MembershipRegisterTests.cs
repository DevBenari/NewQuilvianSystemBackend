using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Services;
using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Services;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Pendaftaran membership baru — crm-membership `BE-CRM-011`, `FR-CRM-010`, `FR-CRM-011`,
/// `AT-01`, `AT-02`, `AT-11`, `AT-20`, `VAL-01` s.d. `VAL-06`.
/// </summary>
public class MembershipRegisterTests : IDisposable
{
    private static readonly Guid Aktor = Guid.Parse("33333333-3333-3333-3333-333333333333");

    // 13 Okt 2026 09.00 WIB.
    private static readonly DateTimeOffset Sekarang = new(2026, 10, 13, 9, 0, 0, TimeSpan.FromHours(7));

    private readonly TestDatabase _db = TestDatabase.Create();

    // Alokator nomor membuka transaksinya sendiri; SQLite tidak mendukung transaksi bersarang
    // pada satu koneksi, jadi deret nomor uji tinggal di basis data terpisah.
    private readonly TestDatabase _dbNomor = TestDatabase.Create();

    private readonly TiruanPortTagihanIuran _portTagihan = new(() => Sekarang);

    private readonly Guid _gold = Guid.NewGuid();
    private readonly Guid _corporate = Guid.NewGuid();
    private readonly Guid _silverTanpaIuran = Guid.NewGuid();

    public MembershipRegisterTests()
    {
        using var k = _db.CreateContext();
        k.Set<MstMembershipTier>().AddRange(
            new MstMembershipTier { Id = _gold, TierCode = "GOLD", TierName = "Gold", TierType = MembershipTierType.Gold, IsActive = true, AnnualFeeAmount = 2_400_000m },
            new MstMembershipTier { Id = _corporate, TierCode = "CORP", TierName = "Corporate", TierType = MembershipTierType.Corporate, IsActive = true, AnnualFeeAmount = 5_000_000m },
            new MstMembershipTier { Id = _silverTanpaIuran, TierCode = "SILVER", TierName = "Silver", TierType = MembershipTierType.Silver, IsActive = true, AnnualFeeAmount = 0m });
        k.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _dbNomor.Dispose();
    }

    [Fact]
    public async Task Daftar_Berhasil_MenungguPembayaran_NomorDariSistem_RiwayatTercatat()
    {
        var pasien = TambahPasien();

        using var konteks = _db.CreateContext();
        var hasil = await Service(konteks).RegisterAsync(Permintaan(pasien, _gold, "Daftar via telepon"), Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);

        using var baca = _db.CreateContext();
        var m = baca.Set<MstPatientMembership>().AsNoTracking().Single(x => x.Id == hasil.PatientMembershipId);
        Assert.Equal(MembershipStatus.PendingPayment, m.MembershipStatus);
        Assert.Equal(MembershipRegistrationType.New, m.RegistrationType);
        Assert.Equal("MBR-000001", m.MemberNumber);
        Assert.Equal(new DateTime(2026, 10, 13), m.JoinDate);
        Assert.Equal(Sekarang.AddDays(7), m.PaymentDueAt);
        Assert.Equal(2_400_000m, m.FeeAmountSnapshot);
        Assert.False(m.IsActive);
        Assert.False(m.IsPrimary);
        Assert.True(m.IsCreatedByMarketing);
        Assert.Null(m.PeriodStartDate);
        Assert.Equal("Daftar via telepon", m.Notes);
        Assert.Equal(Aktor, m.CreateBy);

        // Pendaftaran sukses mencatat REGISTERED dan FEE_CHARGE_LINKED (tagihan iuran INT-06). Keduanya
        // ber-OccurredAt sama dan Id acak, jadi urutannya tidak dijamin; diperiksa menurut ActionCode.
        var riwayat = baca.Set<PatMembershipStatusHistory>().AsNoTracking().Where(x => x.PatientMembershipId == m.Id).ToList();
        Assert.Equal(2, riwayat.Count);

        var terdaftar = Assert.Single(riwayat, x => x.ActionCode == "REGISTERED");
        Assert.Null(terdaftar.FromStatus);
        Assert.Equal(MembershipStatus.PendingPayment, terdaftar.ToStatus);
        Assert.Equal(MembershipChangeSource.Staff, terdaftar.Source);
        Assert.Equal(Aktor, terdaftar.ActorUserId);

        var tagihan = _portTagihan.ChargeIdMilik(m.Id);
        Assert.NotNull(tagihan);
        Assert.Equal(tagihan, m.FeeChargeReferenceId);

        var tertaut = Assert.Single(riwayat, x => x.ActionCode == "FEE_CHARGE_LINKED");
        Assert.Equal(MembershipChangeSource.Billing, tertaut.Source);
        Assert.Equal(tagihan, tertaut.ReferenceId);

        // Menunggu Pembayaran tidak mengubah status member pasien (CRM-DEC-004).
        var p = baca.Set<MstPatient>().AsNoTracking().Single(x => x.Id == pasien);
        Assert.False(p.IsMember);
        Assert.Null(p.ActivePatientMembershipId);
    }

    [Fact]
    public async Task Daftar_DuaPasienBerurutan_NomorNaik()
    {
        var a = TambahPasien();
        var b = TambahPasien();

        using var konteks = _db.CreateContext();
        var service = Service(konteks);
        var ha = await service.RegisterAsync(Permintaan(a, _gold), Aktor);
        var hb = await service.RegisterAsync(Permintaan(b, _gold), Aktor);

        using var baca = _db.CreateContext();
        Assert.Equal("MBR-000001", baca.Set<MstPatientMembership>().Single(x => x.Id == ha.PatientMembershipId).MemberNumber);
        Assert.Equal("MBR-000002", baca.Set<MstPatientMembership>().Single(x => x.Id == hb.PatientMembershipId).MemberNumber);
    }

    [Fact]
    public async Task PasienTidakAda_404()
    {
        using var konteks = _db.CreateContext();
        var hasil = await Service(konteks).RegisterAsync(Permintaan(Guid.NewGuid(), _gold), Aktor);

        Assert.Equal(StatusCodes.Status404NotFound, hasil.StatusCode);
        Assert.Equal("Pasien tidak ditemukan atau tidak aktif. Daftarkan pasien lebih dulu.", hasil.Message);
        Assert.Empty(_db.CreateContext().Set<MstPatientMembership>());
    }

    [Fact]
    public async Task PasienNonaktif_404()
    {
        var pasien = TambahPasien(aktif: false);

        using var konteks = _db.CreateContext();
        Assert.Equal(StatusCodes.Status404NotFound, (await Service(konteks).RegisterAsync(Permintaan(pasien, _gold), Aktor)).StatusCode);
    }

    [Fact]
    public async Task TingkatCorporate_400()
    {
        var pasien = TambahPasien();

        using var konteks = _db.CreateContext();
        var hasil = await Service(konteks).RegisterAsync(Permintaan(pasien, _corporate), Aktor);

        Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
        Assert.Equal("Tingkat ini tidak bisa dipilih untuk pendaftaran membership.", hasil.Message);
    }

    [Fact]
    public async Task TingkatTanpaIuran_409_TanpaMenghabiskanNomor()
    {
        var pasien = TambahPasien();

        using var konteks = _db.CreateContext();
        var hasil = await Service(konteks).RegisterAsync(Permintaan(pasien, _silverTanpaIuran), Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Nominal iuran tingkat Silver belum diisi. Hubungi admin master data.", hasil.Message);
        Assert.Empty(_dbNomor.CreateContext().NumNumberSeries);
    }

    [Fact]
    public async Task MasihAktif_409_MenyarankanPerpanjanganAtauUpgrade()
    {
        var pasien = TambahPasien();
        using (var k = _db.CreateContext())
        {
            k.Set<MstPatientMembership>().Add(new MstPatientMembership
            {
                Id = Guid.NewGuid(), PatientId = pasien, MembershipTierId = _gold, MemberNumber = "MBR-LAMA-1",
                MembershipStatus = MembershipStatus.Active, PeriodStartDate = new DateTime(2026, 10, 13), ExpiredDate = new DateTime(2027, 10, 12)
            });
            k.SaveChanges();
        }

        using var konteks = _db.CreateContext();
        var hasil = await Service(konteks).RegisterAsync(Permintaan(pasien, _gold), Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Pasien sudah member Gold sampai 12 Okt 2027. Gunakan perpanjangan atau upgrade.", hasil.Message);
    }

    [Fact]
    public async Task SudahAdaMenunggu_409_MenyebutNomornya()
    {
        var pasien = TambahPasien();

        using var konteks = _db.CreateContext();
        var service = Service(konteks);
        await service.RegisterAsync(Permintaan(pasien, _gold), Aktor);
        var kedua = await service.RegisterAsync(Permintaan(pasien, _gold), Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
        Assert.Equal("Pasien ini sudah punya pendaftaran yang menunggu pembayaran (MBR-000001).", kedua.Message);
        Assert.Single(_db.CreateContext().Set<MstPatientMembership>());
    }

    [Fact]
    public async Task CatatanLebihDari250_400()
    {
        var pasien = TambahPasien();

        using var konteks = _db.CreateContext();
        var hasil = await Service(konteks).RegisterAsync(Permintaan(pasien, _gold, new string('x', 251)), Aktor);

        Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
    }

    [Fact]
    public async Task BatasBayar_IkutKonfigurasi()
    {
        var pasien = TambahPasien();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { [PatientMembershipService.PaymentDueDaysConfigKey] = "3" })
            .Build();

        using var konteks = _db.CreateContext();
        var hasil = await Service(konteks, config).RegisterAsync(Permintaan(pasien, _gold), Aktor);

        using var baca = _db.CreateContext();
        Assert.Equal(Sekarang.AddDays(3), baca.Set<MstPatientMembership>().Single(x => x.Id == hasil.PatientMembershipId).PaymentDueAt);
    }

    [Fact]
    public async Task Controller_PostAdmin_200DenganDetail_Dan409DenganPesan()
    {
        var pasien = TambahPasien();

        using var konteks = _db.CreateContext();
        var controller = Controller(konteks);

        var ok = Assert.IsType<OkObjectResult>(await controller.RegisterPatientMembershipForAdmin(Permintaan(pasien, _gold)));
        var detail = Assert.IsType<ApiResponse<PatientMembershipDetailResponse>>(ok.Value).Data!;
        Assert.Equal(MembershipStatus.PendingPayment, detail.MembershipStatus);
        Assert.Equal("MBR-000001", detail.MemberNumber);
        Assert.Equal(2_400_000m, detail.FeeAmountSnapshot);

        var konflik = Assert.IsType<ObjectResult>(await controller.RegisterPatientMembershipForAdmin(Permintaan(pasien, _gold)));
        Assert.Equal(StatusCodes.Status409Conflict, konflik.StatusCode);
        Assert.Equal("Pasien ini sudah punya pendaftaran yang menunggu pembayaran (MBR-000001).",
            Assert.IsType<ApiResponse<object>>(konflik.Value).Message);
    }

    // ---------- bantuan ----------

    private static RegisterPatientMembershipRequest Permintaan(Guid pasien, Guid tier, string? catatan = null) =>
        new() { PatientId = pasien, MembershipTierId = tier, Notes = catatan };

    private PatientMembershipService Service(ApplicationDbContext konteks, IConfiguration? config = null)
    {
        var jam = new JamTetap(Sekarang);
        var konfigurasi = config ?? new ConfigurationBuilder().Build();
        return new PatientMembershipService(
            konteks,
            new NumberSeriesAllocator(new PabrikKonteks(_dbNomor)),
            new PatientMembershipStatusResolver(konteks, konfigurasi, jam),
            Logger(),
            _portTagihan,
            konfigurasi,
            jam);
    }

    private PatientMembershipController Controller(ApplicationDbContext konteks)
    {
        var resolver = new PatientMembershipStatusResolver(konteks, new ConfigurationBuilder().Build(), new JamTetap(Sekarang));
        var controller = new PatientMembershipController(konteks, Logger(), resolver, Service(konteks), _portTagihan);
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, Aktor.ToString())], "uji"))
            }
        };
        return controller;
    }

    private Guid TambahPasien(bool aktif = true)
    {
        var id = Guid.NewGuid();
        using var k = _db.CreateContext();
        k.Set<MstPatient>().Add(new MstPatient
        {
            Id = id, PatientCode = "P-" + id.ToString("N")[..6], MedicalRecordNumber = "RM-" + id.ToString("N")[..6],
            FullName = "Pasien Uji", IsActive = aktif, IsMember = false
        });
        k.SaveChanges();
        return id;
    }

    private static LoggerService Logger() => new(NullLogger<LoggerService>.Instance, new HttpContextAccessor());

    private sealed class PabrikKonteks(TestDatabase db) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => db.CreateContext();
    }

    private sealed class JamTetap(DateTimeOffset sekarang) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => sekarang.ToUniversalTime();
    }
}
