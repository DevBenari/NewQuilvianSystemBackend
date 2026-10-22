using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.Membership.Models;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Tests.PatientManagement.Infrastructure;
using static QuilvianSystemBackend.Tests.PatientManagement.Infrastructure.MembershipHarness;

namespace QuilvianSystemBackend.Tests.PatientManagement;

/// <summary>
/// Batal, verifikasi data lama, catatan, dan riwayat — crm-membership `BE-CRM-013`,
/// `FR-CRM-030` s.d. `FR-CRM-035`, `AT-15`, `AT-16`, `AT-28`, `AT-35`, `VAL-10` s.d. `VAL-17`.
/// </summary>
public class MembershipCancelVerifyNotesTests : IDisposable
{
    private const string AlasanSah = "Pasien pindah ke luar kota";

    // Hari ini: 1 Des 2026 09.00 WIB.
    private readonly MembershipHarness _h = new(Wib(2026, 12, 1));
    private readonly Guid _gold;
    private readonly Guid _pasien;

    public MembershipCancelVerifyNotesTests()
    {
        _gold = _h.TambahTier("Gold", MembershipTierType.Gold, 2);
        _pasien = _h.TambahPasien();
    }

    public void Dispose() => _h.Dispose();

    private MstPatientMembership GoldAktif() =>
        _h.TambahMembership(_pasien, _gold, MembershipStatus.Active, Tgl(2026, 10, 13), Tgl(2027, 10, 12));

    private static CancelPatientMembershipRequest Batal(MstPatientMembership m, MembershipCancellationCategory kategori = MembershipCancellationCategory.PatientRequest, string alasan = AlasanSah) =>
        new() { Category = kategori, Reason = alasan, ExpectedRowVersion = m.RowVersion };

    private List<PatMembershipStatusHistory> Riwayat(Guid membershipId)
    {
        using var k = _h.Konteks();
        return k.Set<PatMembershipStatusHistory>().AsNoTracking().Where(x => x.PatientMembershipId == membershipId).ToList();
    }

    private MstPatient Pasien()
    {
        using var k = _h.Konteks();
        return k.Set<MstPatient>().AsNoTracking().Single(x => x.Id == _pasien);
    }

    private void JadikanProyeksiMember(Guid membershipId)
    {
        using var k = _h.Konteks();
        var p = k.Set<MstPatient>().Single(x => x.Id == _pasien);
        p.IsMember = true;
        p.DefaultMembershipTierId = _gold;
        p.ActivePatientMembershipId = membershipId;
        k.SaveChanges();
    }

    // ---------- batal ----------

    [Theory]
    [InlineData("")]
    [InlineData("singkat")]
    public async Task Batal_AlasanKosongAtauPendek_400(string alasan)
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).CancelAsync(aktif.Id, Batal(aktif, alasan: alasan), Aktor);

        Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
        Assert.Equal("Tulis alasan minimal 10 karakter.", hasil.Message);
        Assert.Equal(MembershipStatus.Active, _h.Baca(aktif.Id).MembershipStatus);
    }

    [Fact]
    public async Task Batal_KategoriSistem_400()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).CancelAsync(aktif.Id, Batal(aktif, MembershipCancellationCategory.Unpaid), Aktor);

        Assert.Equal("Pilih alasan: permintaan pasien atau salah input.", hasil.Message);
    }

    [Fact]
    public async Task Batal_AktifPermintaanPasien_Dibatalkan_TanpaRefund_ProyeksiKosong()
    {
        var aktif = GoldAktif();
        JadikanProyeksiMember(aktif.Id);

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).CancelAsync(aktif.Id, Batal(aktif), Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
        var m = _h.Baca(aktif.Id);
        Assert.Equal(MembershipStatus.Cancelled, m.MembershipStatus);
        Assert.Equal(MembershipCancellationCategory.PatientRequest, m.CancellationCategory);
        Assert.Equal(AlasanSah, m.CancellationReason);
        Assert.True(m.IsCancel);
        Assert.Equal(Aktor, m.CancelBy);
        Assert.False(m.IsActive);
        Assert.NotEqual(aktif.RowVersion, m.RowVersion);

        var riwayat = Riwayat(aktif.Id);
        var batal = Assert.Single(riwayat);
        Assert.Equal("CANCELLED", batal.ActionCode);
        Assert.Equal(MembershipStatus.Active, batal.FromStatus);
        Assert.Equal(AlasanSah, batal.Reason);
        Assert.DoesNotContain(riwayat, x => x.ActionCode == "REFUND_REQUESTED");

        Assert.False(Pasien().IsMember);
        Assert.Null(Pasien().ActivePatientMembershipId);
    }

    [Fact]
    public async Task Batal_AktifYangPunyaTerjadwal_TerjadwalIkutBatal_DenganPermintaanRefund()
    {
        var aktif = GoldAktif();
        var terjadwal = _h.TambahMembership(_pasien, _gold, MembershipStatus.Scheduled, Tgl(2027, 10, 13), Tgl(2028, 10, 12), MembershipRegistrationType.Renewal);

        using var k = _h.Konteks();
        await _h.Service(k).CancelAsync(aktif.Id, Batal(aktif), Aktor);

        var t = _h.Baca(terjadwal.Id);
        Assert.Equal(MembershipStatus.Cancelled, t.MembershipStatus);
        Assert.StartsWith("Ikut dibatalkan bersama", t.CancellationReason);
        Assert.Contains(Riwayat(terjadwal.Id), x => x.ActionCode == "REFUND_REQUESTED");
    }

    [Fact]
    public async Task Batal_AktifSalahInput_409FailClosed_TidakBerubah()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).CancelAsync(aktif.Id, Batal(aktif, MembershipCancellationCategory.InputError), Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.StartsWith("Pembatalan karena salah input belum bisa diproses", hasil.Message);
        Assert.Equal(MembershipStatus.Active, _h.Baca(aktif.Id).MembershipStatus);
        Assert.Empty(Riwayat(aktif.Id));
    }

    [Fact]
    public async Task Batal_MenungguPembayaran_Dibatalkan_DenganPermintaanBatalTagihan()
    {
        var menunggu = _h.TambahMembership(_pasien, _gold, MembershipStatus.PendingPayment);

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).CancelAsync(menunggu.Id, Batal(menunggu, MembershipCancellationCategory.InputError), Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
        Assert.Equal(MembershipStatus.Cancelled, _h.Baca(menunggu.Id).MembershipStatus);
        Assert.Contains(Riwayat(menunggu.Id), x => x.ActionCode == "FEE_CHARGE_CANCEL_REQUESTED");
    }

    [Fact]
    public async Task Batal_Terjadwal_Dibatalkan_DenganPermintaanRefund()
    {
        var terjadwal = _h.TambahMembership(_pasien, _gold, MembershipStatus.Scheduled, Tgl(2027, 10, 13), Tgl(2028, 10, 12), MembershipRegistrationType.Renewal);

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).CancelAsync(terjadwal.Id, Batal(terjadwal), Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
        Assert.Contains(Riwayat(terjadwal.Id), x => x.ActionCode == "REFUND_REQUESTED");
    }

    [Theory]
    [InlineData(MembershipStatus.NeedsVerification, "Data lama ditolak lewat verifikasi, bukan pembatalan.")]
    [InlineData(MembershipStatus.Expired, "Membership yang sudah berakhir tidak bisa dibatalkan.")]
    [InlineData(MembershipStatus.Cancelled, "Membership yang sudah berakhir tidak bisa dibatalkan.")]
    public async Task Batal_StatusTidakSah_409(MembershipStatus status, string pesan)
    {
        var m = _h.TambahMembership(_pasien, _gold, status, Tgl(2025, 10, 13), Tgl(2026, 10, 12));

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).CancelAsync(m.Id, Batal(m), Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal(pesan, hasil.Message);
    }

    [Fact]
    public async Task Batal_DuaPetugasBerurutanDenganRowVersionSama_YangKedua409()
    {
        // AT-35: petugas kedua membuka data sebelum petugas pertama menyimpan.
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var service = _h.Service(k);
        var pertama = await service.CancelAsync(aktif.Id, Batal(aktif), Aktor);

        using var k2 = _h.Konteks();
        var kedua = await _h.Service(k2).CancelAsync(aktif.Id, Batal(aktif), Aktor);

        Assert.Equal(StatusCodes.Status200OK, pertama.StatusCode);
        Assert.Equal(StatusCodes.Status409Conflict, kedua.StatusCode);
        Assert.Equal("Data ini baru saja diubah petugas lain. Muat ulang lalu coba lagi.", kedua.Message);
    }

    // ---------- verifikasi data lama ----------

    private MstPatientMembership DataLama() =>
        _h.TambahMembership(_pasien, _gold, MembershipStatus.NeedsVerification, jenis: MembershipRegistrationType.LegacyImport);

    [Fact]
    public async Task Verifikasi_AkuiPeriodeBerjalan_Aktif_ProyeksiTerisi()
    {
        var lama = DataLama();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).VerifyLegacyAsync(lama.Id, new VerifyLegacyPatientMembershipRequest
        {
            Decision = MembershipVerificationDecision.Accept,
            PeriodStartDate = Tgl(2026, 3, 1),
            PeriodEndDate = Tgl(2027, 2, 28),
            ExpectedRowVersion = lama.RowVersion
        }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
        var m = _h.Baca(lama.Id);
        Assert.Equal(MembershipStatus.Active, m.MembershipStatus);
        Assert.Equal(Tgl(2026, 3, 1), m.PeriodStartDate);
        Assert.Equal(Tgl(2027, 2, 28), m.ExpiredDate);
        Assert.NotNull(m.ActivatedAt);
        Assert.True(m.IsActive);
        Assert.Equal("LEGACY_ACCEPTED", Assert.Single(Riwayat(lama.Id)).ActionCode);

        Assert.True(Pasien().IsMember);
        Assert.Equal(lama.Id, Pasien().ActivePatientMembershipId);
    }

    [Fact]
    public async Task Verifikasi_AkuiPeriodeSudahLewat_Kedaluwarsa()
    {
        var lama = DataLama();

        using var k = _h.Konteks();
        await _h.Service(k).VerifyLegacyAsync(lama.Id, new VerifyLegacyPatientMembershipRequest
        {
            Decision = MembershipVerificationDecision.Accept,
            PeriodStartDate = Tgl(2025, 6, 1),
            PeriodEndDate = Tgl(2026, 5, 31),
            ExpectedRowVersion = lama.RowVersion
        }, Aktor);

        Assert.Equal(MembershipStatus.Expired, _h.Baca(lama.Id).MembershipStatus);
        Assert.False(Pasien().IsMember);
    }

    [Fact]
    public async Task Verifikasi_AkuiSaatSudahAdaAktif_409()
    {
        GoldAktif();
        var lama = DataLama();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).VerifyLegacyAsync(lama.Id, new VerifyLegacyPatientMembershipRequest
        {
            Decision = MembershipVerificationDecision.Accept,
            PeriodStartDate = Tgl(2026, 3, 1),
            PeriodEndDate = Tgl(2027, 2, 28),
            ExpectedRowVersion = lama.RowVersion
        }, Aktor);

        Assert.Equal(StatusCodes.Status409Conflict, hasil.StatusCode);
        Assert.Equal("Pasien sudah punya membership aktif. Tolak data lama ini atau batalkan yang aktif lebih dulu.", hasil.Message);
    }

    [Theory]
    [InlineData(2026, 5, 1, 2026, 4, 30)]   // akhir sebelum mulai
    [InlineData(2026, 3, 1, 2027, 3, 1)]    // 12 bulan + 1 hari
    public async Task Verifikasi_PeriodeTidakSah_400(int y1, int m1, int d1, int y2, int m2, int d2)
    {
        var lama = DataLama();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).VerifyLegacyAsync(lama.Id, new VerifyLegacyPatientMembershipRequest
        {
            Decision = MembershipVerificationDecision.Accept,
            PeriodStartDate = Tgl(y1, m1, d1),
            PeriodEndDate = Tgl(y2, m2, d2),
            ExpectedRowVersion = lama.RowVersion
        }, Aktor);

        Assert.Equal(StatusCodes.Status400BadRequest, hasil.StatusCode);
        Assert.Equal("Periode yang diakui tidak sah. Tanggal mulai harus sebelum tanggal akhir dan maksimal 12 bulan.", hasil.Message);
    }

    [Fact]
    public async Task Verifikasi_TolakTanpaAlasan_400_DenganAlasan_Dibatalkan()
    {
        var lama = DataLama();

        using var k = _h.Konteks();
        var service = _h.Service(k);
        var tanpaAlasan = await service.VerifyLegacyAsync(lama.Id, new VerifyLegacyPatientMembershipRequest
        {
            Decision = MembershipVerificationDecision.Reject, ExpectedRowVersion = lama.RowVersion
        }, Aktor);
        Assert.Equal(StatusCodes.Status400BadRequest, tanpaAlasan.StatusCode);

        var tolak = await service.VerifyLegacyAsync(lama.Id, new VerifyLegacyPatientMembershipRequest
        {
            Decision = MembershipVerificationDecision.Reject, Reason = "Tidak ada bukti pembayaran", ExpectedRowVersion = lama.RowVersion
        }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, tolak.StatusCode);
        var m = _h.Baca(lama.Id);
        Assert.Equal(MembershipStatus.Cancelled, m.MembershipStatus);
        Assert.Null(m.CancellationCategory);
        Assert.Equal("LEGACY_REJECTED", Assert.Single(Riwayat(lama.Id)).ActionCode);
    }

    [Fact]
    public async Task Verifikasi_BukanPerluVerifikasi_409()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).VerifyLegacyAsync(aktif.Id, new VerifyLegacyPatientMembershipRequest
        {
            Decision = MembershipVerificationDecision.Reject, Reason = "Tidak ada bukti pembayaran", ExpectedRowVersion = aktif.RowVersion
        }, Aktor);

        Assert.Equal("Membership ini tidak sedang menunggu verifikasi.", hasil.Message);
    }

    // ---------- catatan ----------

    [Fact]
    public async Task Catatan_DiubahPadaAktif_RiwayatMenyimpanNilaiLamaDanBaru()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).UpdateNotesAsync(aktif.Id,
            new UpdatePatientMembershipNotesRequest { Notes = "Minta dihubungi lewat WA", ExpectedRowVersion = aktif.RowVersion }, Aktor);

        Assert.Equal(StatusCodes.Status200OK, hasil.StatusCode);
        var m = _h.Baca(aktif.Id);
        Assert.Equal("Minta dihubungi lewat WA", m.Notes);
        Assert.NotEqual(aktif.RowVersion, m.RowVersion);
        Assert.Equal(MembershipStatus.Active, m.MembershipStatus);

        var r = Assert.Single(Riwayat(aktif.Id));
        Assert.Equal("NOTES_UPDATED", r.ActionCode);
        Assert.Null(r.OldNotes);
        Assert.Equal("Minta dihubungi lewat WA", r.NewNotes);
        Assert.Equal(MembershipStatus.Active, r.FromStatus);
        Assert.Equal(MembershipStatus.Active, r.ToStatus);
    }

    [Fact]
    public async Task Catatan_PadaStatusAkhir_409()
    {
        var batal = _h.TambahMembership(_pasien, _gold, MembershipStatus.Cancelled);

        using var k = _h.Konteks();
        var hasil = await _h.Service(k).UpdateNotesAsync(batal.Id,
            new UpdatePatientMembershipNotesRequest { Notes = "x", ExpectedRowVersion = batal.RowVersion }, Aktor);

        Assert.Equal("Membership yang sudah berakhir tidak bisa diubah.", hasil.Message);
    }

    // ---------- riwayat dan controller ----------

    [Fact]
    public async Task Riwayat_TerbaruDiAtas_Dan404BilaTidakAda()
    {
        var menunggu = _h.TambahMembership(_pasien, _gold, MembershipStatus.PendingPayment);

        using var k = _h.Konteks();
        await _h.Service(k).CancelAsync(menunggu.Id, Batal(menunggu), Aktor);

        using var k2 = _h.Konteks();
        var controller = _h.Controller(k2);
        var ok = Assert.IsType<OkObjectResult>(await controller.GetPatientMembershipStatusHistories(menunggu.Id));
        var data = Assert.IsType<ApiResponse<List<PatientMembershipStatusHistoryResponse>>>(ok.Value).Data!;
        Assert.Equal(2, data.Count);
        Assert.Contains(data, x => x.ActionCode == "CANCELLED" && x.Reason == AlasanSah && x.ActorUserId == Aktor);

        Assert.IsType<NotFoundObjectResult>(await controller.GetPatientMembershipStatusHistories(Guid.NewGuid()));
    }

    [Fact]
    public async Task Controller_Cancel_200DenganDetailDibatalkan()
    {
        var aktif = GoldAktif();

        using var k = _h.Konteks();
        var ok = Assert.IsType<OkObjectResult>(await _h.Controller(k).CancelPatientMembership(aktif.Id, Batal(aktif)));
        var detail = Assert.IsType<ApiResponse<PatientMembershipDetailResponse>>(ok.Value).Data!;

        Assert.Equal(MembershipStatus.Cancelled, detail.MembershipStatus);
        Assert.Equal(AlasanSah, detail.CancellationReason);
    }
}
