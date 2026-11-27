using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Kewenangan memutus permintaan setelah disederhanakan (<c>FIN-LGS-070</c>): izin
/// <c>StockRequest:Approve</c> atau <c>StockRequest:AuthorizeEmergency</c> yang aktif, larangan
/// memutus permintaan sendiri, dan tidak ada lagi ketergantungan pada jabatan atau penempatan HRD.
/// </summary>
/// <remarks>
/// Service dipanggil langsung, sehingga filter <c>[AccessPermission]</c> di controller tidak
/// ikut berjalan — sama keadaannya dengan SuperAdmin atau <c>Authorization:Enabled=false</c>.
/// Yang dibuktikan adalah penjagaan domain yang harus tetap berlaku dalam keadaan itu.
/// </remarks>
public class AuthorizationTests
{
    // ------------------------------------------------------------ izin persetujuan

    [Fact]
    public async Task Penyetuju_berizin_menyetujui_dan_keputusannya_tercatat_lengkap()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var hasil = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .ApproveAsync(id, Setujui("uji-setuju-1", catatan: "Stok depo menipis"));

        Assert.Equal(StockRequestStatus.Approved, hasil.Status);
        var riwayat = Assert.Single(hasil.Histories, x => x.Action == "ApproveStockRequest");
        Assert.Equal(h.AtasanUserId, riwayat.ActorUserId);
        Assert.Equal(StockRequestStatus.Submitted, riwayat.FromStatus);
        Assert.Equal(StockRequestStatus.Approved, riwayat.ToStatus);
        Assert.Equal("Stok depo menipis", riwayat.Reason);
        Assert.True(riwayat.OccurredAt > DateTime.UtcNow.AddMinutes(-5));
    }

    [Fact]
    public async Task Kasus3_penempatan_manajerial_tanpa_izin_Approve_eksplisit_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanTanpaIzinUserId).ApproveAsync(id, Setujui("uji-tanpa-izin")));

        Assert.Contains("StockRequest:Approve", galat.Message);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Kasus2_penyetuju_dapat_memutus_permintaan_lintas_unit()
    {
        using var h = new StockRequestApprovalHarness();
        var (dariUnitA, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (dariUnitB, _) = await h.PermintaanAsync(unitId: h.UnitLainId);

        // Penempatan manajerial HRD AtasanLain ada di unit B, AtasanUser di unit A: keduanya tetap
        // dapat memutus permintaan unit lain karena yang menentukan hanya izinnya.
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanLainUserId)
            .ApproveAsync(dariUnitA, Setujui("uji-lintas-1"));
        var ditolak = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .RejectAsync(dariUnitB, Tolak("uji-lintas-2"));

        Assert.Equal(StockRequestStatus.Approved, disetujui.Status);
        Assert.Equal(StockRequestStatus.Rejected, ditolak.Status);
        Assert.Equal(h.AtasanLainUserId,
            Assert.Single(disetujui.Histories, x => x.Action == "ApproveStockRequest").ActorUserId);
        Assert.Equal(h.AtasanUserId,
            Assert.Single(ditolak.Histories, x => x.Action == "RejectStockRequest").ActorUserId);
    }

    [Theory]
    [InlineData("penempatan-manajerial-berakhir")]
    [InlineData("profil-tenaga-kerja-nonaktif")]
    public async Task Penempatan_dan_profil_HRD_tidak_lagi_menentukan_kewenangan(string keadaan)
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        var penyetuju = keadaan == "penempatan-manajerial-berakhir"
            ? h.AtasanKedaluwarsaUserId
            : h.AtasanNonaktifUserId;

        var hasil = await h.Layanan(h.CreateContext(), penyetuju)
            .ApproveAsync(id, Setujui($"uji-hrd-{keadaan}"));

        Assert.Equal(StockRequestStatus.Approved, hasil.Status);
    }

    [Fact]
    public async Task Unit_layanan_tanpa_tautan_organisasi_tetap_dapat_diputus_penyetuju()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(unitId: h.UnitTanpaTautanId);

        var hasil = await h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-tautan"));

        Assert.Equal(StockRequestStatus.Approved, hasil.Status);
    }

    [Fact]
    public async Task Kasus4_SuperAdmin_tanpa_penempatan_yang_sah_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.SuperAdminUserId).ApproveAsync(id, Setujui("uji-sa-setuju")));
        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.SuperAdminUserId).RejectAsync(id, Tolak("uji-sa-tolak")));

        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Izin_CITO_saja_tidak_memberi_kewenangan_menyetujui_atau_menolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId).ApproveAsync(id, Setujui("uji-cito-setuju")));
        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId).RejectAsync(id, Tolak("uji-cito-tolak")));

        Assert.Contains("StockRequest:Approve", galat.Message);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Detail_menyampaikan_kewenangan_dari_server_tanpa_masukan_layar()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var penyetuju = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetDetailAsync(id);
        Assert.True(penyetuju!.HasApproveAuthority);
        Assert.False(penyetuju.HasEmergencyAuthority);
        Assert.False(penyetuju.IsOwnRequest);

        var tanpaIzin = await h.Layanan(h.CreateContext(), h.AtasanTanpaIzinUserId).GetDetailAsync(id);
        Assert.False(tanpaIzin!.HasApproveAuthority);

        var unitLain = await h.Layanan(h.CreateContext(), h.AtasanLainUserId).GetDetailAsync(id);
        Assert.True(unitLain!.HasApproveAuthority);

        var staf = await h.Layanan(h.CreateContext(), h.StafUserId).GetDetailAsync(id);
        Assert.False(staf!.HasApproveAuthority);
        Assert.False(staf.HasEmergencyAuthority);

        var petugasCito = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId).GetDetailAsync(id);
        Assert.True(petugasCito!.HasEmergencyAuthority);
        Assert.False(petugasCito.HasApproveAuthority);

        var pemohon = await h.Layanan(h.CreateContext(), h.PemohonUserId).GetDetailAsync(id);
        Assert.True(pemohon!.IsOwnRequest);
    }

    // ------------------------------------------------------------------ otorisasi CITO

    [Fact]
    public async Task Kasus6_CITO_dengan_izin_eksplisit_tanpa_penunjukan_jabatan_dapat_segera_diproses()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var hasil = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId)
            .AuthorizeEmergencyAsync(id, Cito("uji-cito-sah"));

        Assert.Equal(StockRequestStatus.Approved, hasil.Status);
        Assert.True(hasil.CanFulfill);
        Assert.Equal(StockRequestEmergencyReviewState.Pending, hasil.EmergencyReviewState);
        Assert.NotNull(hasil.EmergencyAuthorization);
        Assert.Equal(h.PetugasCitoUserId, hasil.EmergencyAuthorization!.ActorUserId);
        Assert.Equal("Pasien IGD, stok depo habis", hasil.EmergencyAuthorization.Reason);
    }

    [Fact]
    public async Task Kasus5_CITO_tanpa_izin_eksplisit_ditolak_termasuk_SuperAdmin_dan_penyetuju_biasa()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var superAdmin = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.SuperAdminUserId).AuthorizeEmergencyAsync(id, Cito("uji-cito-sa")));
        Assert.Contains("SuperAdmin", superAdmin.Message);
        Assert.Contains("StockRequest:AuthorizeEmergency", superAdmin.Message);

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).AuthorizeEmergencyAsync(id, Cito("uji-cito-atasan")));
        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoKedaluwarsaUserId)
                .AuthorizeEmergencyAsync(id, Cito("uji-cito-lama")));

        await AssertMasihMenungguAsync(h, id);
    }

    // ------------------------------------------------------------ larangan memutus sendiri

    [Fact]
    public async Task Kasus1_pemohon_berwenang_penuh_tetap_tidak_dapat_memutus_permintaannya_sendiri()
    {
        using var h = new StockRequestApprovalHarness();
        var (rutin, _) = await h.PermintaanAsync();
        var (cito, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var setuju = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PemohonUserId).ApproveAsync(rutin, Setujui("uji-sendiri-1")));
        Assert.Contains("Pemohon tidak boleh", setuju.Message);

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PemohonUserId).RejectAsync(rutin, Tolak("uji-sendiri-2")));
        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PemohonUserId).AuthorizeEmergencyAsync(cito, Cito("uji-sendiri-3")));

        await AssertMasihMenungguAsync(h, rutin);
        await AssertMasihMenungguAsync(h, cito);
    }

    [Fact]
    public async Task Penyetuju_yang_membuatkan_permintaan_tidak_dapat_menyetujuinya()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(dibuatOleh: h.AtasanUserId);

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-pembuat")));

        await AssertMasihMenungguAsync(h, id);
    }
}
