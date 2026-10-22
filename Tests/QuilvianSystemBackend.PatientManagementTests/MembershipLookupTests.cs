using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using static QuilvianSystemBackend.Tests.PatientManagement.Infrastructure.MembershipHarness;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Pencarian pasien dan status aktif — crm-membership `BE-CRM-014`, `FR-CRM-014`, `AT-18`, `VAL-22`.
/// </summary>
public class MembershipLookupTests : IDisposable
{
    // Hari ini: 1 Des 2026 09.00 WIB. Data samaran.
    private readonly MembershipHarness _h = new(Wib(2026, 12, 1));
    private readonly Guid _gold;

    public MembershipLookupTests()
    {
        _gold = _h.TambahTier("Gold", MembershipTierType.Gold, 2);
    }

    public void Dispose() => _h.Dispose();

    private Guid Pasien(string nama, string rm, string? nik, string? telp, bool aktif = true)
    {
        var id = Guid.NewGuid();
        using var k = _h.Konteks();
        k.Set<MstPatient>().Add(new MstPatient
        {
            Id = id, PatientCode = "P-" + rm, MedicalRecordNumber = rm, FullName = nama,
            IdentityNumber = nik, PhoneNumber = telp, IsActive = aktif, BirthDate = new DateTime(1980, 5, 17)
        });
        k.SaveChanges();
        return id;
    }

    private async Task<PagedResult<PatientMembershipLookupResponse>> Cari(string kata)
    {
        using var k = _h.Konteks();
        var ok = Assert.IsType<OkObjectResult>(await _h.Controller(k).LookupPatientsForMembership(new PatientMembershipLookupQuery { Keyword = kata }));
        return Assert.IsType<ApiResponse<PagedResult<PatientMembershipLookupResponse>>>(ok.Value).Data!;
    }

    [Fact]
    public async Task NoTelpDipakaiDuaPasien_KeduanyaTampil_DenganStatusMasingMasing_Tersamar()
    {
        var ibu = Pasien("Sari Samaran", "00-00-01", "3174010101010001", "081200001234");
        var anak = Pasien("Dika Samaran", "00-00-02", null, "081200001234");
        _h.TambahMembership(ibu, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

        var hasil = await Cari("081200001234");

        Assert.Equal(2, hasil.TotalData);
        var s = hasil.Items.Single(x => x.PatientId == ibu);
        var d = hasil.Items.Single(x => x.PatientId == anak);
        Assert.True(s.IsMember);
        Assert.Equal("Gold", s.ActiveMembership!.TierName);
        Assert.Equal(Tgl(2027, 10, 12), s.ActiveMembership.PeriodEndDate);
        Assert.False(d.IsMember);
        Assert.Null(d.ActiveMembership);

        Assert.Equal("3174********0001", s.IdentityNumberMasked);
        Assert.Equal("0812*****234", s.PhoneNumberMasked);
        Assert.Null(d.IdentityNumberMasked);
    }

    [Fact]
    public async Task CariNoRmDanNik_Ditemukan_PasienNonaktifTidak()
    {
        var a = Pasien("Ani Samaran", "00-77-01", "3275000000000077", null);
        Pasien("Budi Nonaktif", "00-77-02", null, null, aktif: false);

        Assert.Equal([a], (await Cari("00-77")).Items.Select(x => x.PatientId));
        Assert.Equal([a], (await Cari("3275000000")).Items.Select(x => x.PatientId));
    }

    [Fact]
    public async Task PendaftaranMenunggu_IkutTampil()
    {
        var p = Pasien("Citra Samaran", "00-88-01", null, "081277770000");
        var menunggu = _h.TambahMembership(p, _gold, MembershipStatus.PendingPayment);

        var item = Assert.Single((await Cari("081277770000")).Items);

        Assert.False(item.IsMember);
        Assert.Equal(menunggu.MemberNumber, item.PendingMemberNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("08")]
    public async Task KataKunciKurangDari3_400(string? kata)
    {
        using var k = _h.Konteks();
        var bad = Assert.IsType<BadRequestObjectResult>(await _h.Controller(k).LookupPatientsForMembership(new PatientMembershipLookupQuery { Keyword = kata }));

        Assert.Equal("Ketik minimal 3 karakter No. RM, NIK, atau No. Telp.", Assert.IsType<ApiResponse<object>>(bad.Value).Message);
    }

    [Fact]
    public async Task StatusAktif_HariIni_TanggalSebelumMulai_DanPasienTidakAda()
    {
        var p = Pasien("Dewi Samaran", "00-99-01", null, null);
        var m = _h.TambahMembership(p, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

        using var k = _h.Konteks();
        var controller = _h.Controller(k);

        var hariIni = Data(await controller.GetPatientActiveMembership(p, null));
        Assert.True(hariIni.IsMember);
        Assert.Equal(Tgl(2026, 12, 1), hariIni.Date);
        Assert.Equal(m.Id, hariIni.ActiveMembership!.PatientMembershipId);

        var sebelumMulai = Data(await controller.GetPatientActiveMembership(p, Tgl(2026, 10, 12)));
        Assert.False(sebelumMulai.IsMember);
        Assert.Null(sebelumMulai.ActiveMembership);

        Assert.IsType<NotFoundObjectResult>(await controller.GetPatientActiveMembership(Guid.NewGuid(), null));
    }

    private static PatientActiveMembershipResponse Data(IActionResult hasil) =>
        Assert.IsType<ApiResponse<PatientActiveMembershipResponse>>(Assert.IsType<OkObjectResult>(hasil).Value).Data!;
}
