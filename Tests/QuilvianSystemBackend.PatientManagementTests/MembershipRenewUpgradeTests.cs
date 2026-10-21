using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using static QuilvianSystemBackend.Tests.PatientManagement.Infrastructure.MembershipHarness;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Perpanjangan dan upgrade — crm-membership `BE-CRM-012`, `FR-CRM-012`, `FR-CRM-013`,
/// `AT-14`, `AT-29`, `AT-30`, `VAL-05`, `VAL-07` s.d. `VAL-09`, `VAL-17`.
/// </summary>
public class MembershipRenewUpgradeTests : IDisposable
{
    // Gold Aktif 13 Okt 2026 s.d. 12 Okt 2027; jendela perpanjangan dibuka 12 Sep 2027.
    private readonly MembershipHarness _h = new(Wib(2027, 10, 1));
    private readonly Guid _silver;
    private readonly Guid _gold;
    private readonly Guid _platinum;
    private readonly Guid _pasien;

    public MembershipRenewUpgradeTests()
    {
        _silver = _h.TambahTier("Silver", MembershipTierType.Silver, 1, 1_200_000m);
        _gold = _h.TambahTier("Gold", MembershipTierType.Gold, 2);
        _platinum = _h.TambahTier("Platinum", MembershipTierType.Platinum, 3, 4_800_000m);
        _pasien = _h.TambahPasien();
    }

    public void Dispose() => _h.Dispose();

    private Areas.HealthServices.PatientManagement.MasterData.Models.MstPatientMembership GoldAktif() =>
        _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

    // ---------- perpanjangan ----------

    [Fact]
    public async Task Perpanjang_11HariSebelumHabis_MenungguPembayaran_TingkatSama_MenunjukYangLama()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
        var baru = _h.Baca(hasil.PatientMembershipId!.Value);
        Assert.Equal(MembershipStatus.PendingPayment, baru.MembershipStatus);
        Assert.Equal(MembershipRegistrationType.Renewal, baru.RegistrationType);
        Assert.Equal(_gold, baru.MembershipTierId);
        Assert.Equal(aktif.Id, baru.PreviousPatientMembershipId);
        Assert.Equal(2_400_000m, baru.FeeAmountSnapshot);

        // Membership lama tidak disentuh.
        var lama = _h.Baca(aktif.Id);
        Assert.Equal(MembershipStatus.Active, lama.MembershipStatus);
        Assert.Equal(aktif.RowVersion, lama.RowVersion);
    }

    [Fact]
    public async Task Perpanjang_40HariSebelumHabis_409_MenyebutTanggalBuka()
    {
        _h.Sekarang = Wib(2027, 9, 2);
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Perpanjangan baru bisa mulai 12 Sep 2027.", hasil.Message);
    }

    [Fact]
    public async Task Perpanjang_TepatHariJendelaDibuka_Diterima()
    {
        _h.Sekarang = Wib(2027, 9, 12, 0, 1);
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
    }

    [Fact]
    public async Task Perpanjang_JendelaIkutKonfigurasi()
    {
        _h.Konfigurasi["PatientMembership:RenewalWindowDays"] = "7";
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal("Perpanjangan baru bisa mulai 5 Okt 2027.", hasil.Message);
    }

    [Fact]
    public async Task Perpanjang_SudahAdaTerjadwal_409()
    {
        var aktif = GoldAktif();
        _h.TambahMembership(_pasien, _gold, MembershipStatus.Scheduled, Tgl(2027, 10, 13), Tgl(2028, 10, 12), MembershipRegistrationType.Renewal);

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Perpanjangan untuk periode berikutnya sudah dibayar.", hasil.Message);
    }

    [Fact]
    public async Task Perpanjang_DuaKali_YangKedua409KarenaMasihMenunggu()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var service = _h.Service(k);
        await service.RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }, Aktor);
        var kedua = await service.RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
        Assert.StartsWith("Pasien ini sudah punya pendaftaran yang menunggu pembayaran", kedua.Message);
    }

    [Fact]
    public async Task Perpanjang_RowVersionUsang_409()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = Guid.NewGuid() }, Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi.", hasil.Message);
    }

    [Theory]
    [InlineData(MembershipStatus.Expired)]
    [InlineData(MembershipStatus.PendingPayment)]
    [InlineData(MembershipStatus.NeedsVerification)]
    public async Task Perpanjang_BukanAktif_409(MembershipStatus status)
    {
        var m = _h.TambahMembership(_pasien, _gold, status, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(m.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = m.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Perpanjangan hanya untuk membership yang sedang aktif.", hasil.Message);
    }

    [Fact]
    public async Task Perpanjang_MembershipTidakAda_404()
    {
        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RenewAsync(Guid.NewGuid(), new RenewPatientMembershipRequest { ExpectedRowVersion = Guid.NewGuid() }, Aktor);

        Assert.Equal(StatusCodes.Status404NotFound, hasil.StatusCode);
    }

    [Fact]
    public async Task SetelahKedaluwarsa_DaftarBaruTingkatSama_Diterima()
    {
        // AT-29: tidak ada lagi penolakan "pasien sudah memiliki tier tersebut".
        _h.TambahMembership(_pasien, _gold, MembershipStatus.Expired, Tgl(2025, 10, 13), Tgl(2026, 10, 12));

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).RegisterAsync(new RegisterPatientMembershipRequest { PatientId = _pasien, MembershipTierId = _gold }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
    }

    // ---------- upgrade ----------

    [Fact]
    public async Task Upgrade_GoldKePlatinum_MenungguPembayaran_YangLamaTetapAktif()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).UpgradeAsync(aktif.Id,
            new UpgradePatientMembershipRequest { MembershipTierId = _platinum, ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
        var baru = _h.Baca(hasil.PatientMembershipId!.Value);
        Assert.Equal(MembershipRegistrationType.Upgrade, baru.RegistrationType);
        Assert.Equal(_platinum, baru.MembershipTierId);
        Assert.Equal(4_800_000m, baru.FeeAmountSnapshot);
        Assert.Equal(aktif.Id, baru.PreviousPatientMembershipId);
        Assert.Equal(MembershipStatus.Active, _h.Baca(aktif.Id).MembershipStatus);
    }

    [Theory]
    [InlineData("Silver")]
    [InlineData("Gold")]
    public async Task Upgrade_KeTingkatSamaAtauLebihRendah_409(string tujuan)
    {
        var aktif = GoldAktif();
        var tierTujuan = tujuan == "Silver" ? _silver : _gold;

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).UpgradeAsync(aktif.Id,
            new UpgradePatientMembershipRequest { MembershipTierId = tierTujuan, ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Upgrade hanya ke tingkat yang lebih tinggi dari Gold.", hasil.Message);
    }

    [Fact]
    public async Task Upgrade_SaatAdaTerjadwal_Diizinkan()
    {
        // Terjadwal dibatalkan dengan refund saat iuran upgrade lunas (CRM-DEC-025, EPIC CRM-08).
        var aktif = GoldAktif();
        _h.TambahMembership(_pasien, _gold, MembershipStatus.Scheduled, Tgl(2027, 10, 13), Tgl(2028, 10, 12), MembershipRegistrationType.Renewal);

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).UpgradeAsync(aktif.Id,
            new UpgradePatientMembershipRequest { MembershipTierId = _platinum, ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
    }

    [Fact]
    public async Task Upgrade_KeTingkatTanpaIuran_409()
    {
        var aktif = GoldAktif();
        var executive = _h.TambahTier("Executive", MembershipTierType.Executive, 4, 0m);

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).UpgradeAsync(aktif.Id,
            new UpgradePatientMembershipRequest { MembershipTierId = executive, ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal("Nominal iuran tingkat Executive belum diisi. Hubungi admin master data.", hasil.Message);
    }

    // ---------- controller ----------

    [Fact]
    public async Task Controller_RenewalsDanUpgrades_200DenganDetail_Dan409DenganPesan()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var controller = _h.Controller(k);

        var renew = Assert.IsType<OkObjectResult>(await controller.RenewPatientMembership(
            aktif.Id, new RenewPatientMembershipRequest { ExpectedRowVersion = aktif.RowVersion }));
        var detail = Assert.IsType<ApiResponse<PatientMembershipDetailResponse>>(renew.Value).Data!;
        Assert.Equal(MembershipRegistrationType.Renewal, detail.RegistrationType);
        Assert.Equal(aktif.Id, detail.PreviousPatientMembershipId);

        // Masih ada perpanjangan yang menunggu bayar, sehingga upgrade ditolak.
        var upgrade = Assert.IsType<ObjectResult>(await controller.UpgradePatientMembership(
            aktif.Id, new UpgradePatientMembershipRequest { MembershipTierId = _platinum, ExpectedRowVersion = aktif.RowVersion }));
        Assert.Equal(StatusCodes.Status409Conflict, upgrade.StatusCode);
    }
}
