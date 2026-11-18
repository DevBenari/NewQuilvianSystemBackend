using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Kewenangan memutus permintaan (<c>LGS-DEC-003</c>, <c>LGS-DEC-004</c>): izin eksplisit,
/// atasan unit, otorisasi CITO, dan larangan memutus permintaan sendiri.
/// </summary>
/// <remarks>
/// Service dipanggil langsung, sehingga filter <c>[AccessPermission]</c> di controller tidak
/// ikut berjalan — sama keadaannya dengan SuperAdmin atau <c>Authorization:Enabled=false</c>.
/// Yang dibuktikan adalah penjagaan domain yang harus tetap berlaku dalam keadaan itu.
/// </remarks>
public class AuthorizationTests
{
    // ------------------------------------------------------------ atasan unit + izin

    [Fact]
    public async Task Atasan_unit_berizin_menyetujui_dan_keputusannya_tercatat_lengkap()
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
    public async Task Kasus3_atasan_unit_tanpa_izin_Approve_eksplisit_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanTanpaIzinUserId).ApproveAsync(id, Setujui("uji-tanpa-izin")));

        Assert.Contains("StockRequest:Approve", galat.Message);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Staf_berizin_tanpa_penempatan_manajerial_ditolak()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId).ApproveAsync(id, Setujui("uji-staf")));

        Assert.Contains("atasan unit", galat.Message);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Kasus2_atasan_unit_A_tidak_dapat_memutus_permintaan_unit_B()
    {
        using var h = new StockRequestApprovalHarness();
        var (dariUnitA, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (dariUnitB, _) = await h.PermintaanAsync(unitId: h.UnitLainId);

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanLainUserId).ApproveAsync(dariUnitA, Setujui("uji-lintas-1")));
        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).RejectAsync(dariUnitB, Tolak("uji-lintas-2")));

        await AssertMasihMenungguAsync(h, dariUnitA);
        await AssertMasihMenungguAsync(h, dariUnitB);
    }

    [Fact]
    public async Task Penempatan_manajerial_yang_berakhir_tidak_memberi_kewenangan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanKedaluwarsaUserId).ApproveAsync(id, Setujui("uji-lama")));

        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Profil_tenaga_kerja_nonaktif_tidak_memberi_kewenangan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanNonaktifUserId).ApproveAsync(id, Setujui("uji-nonaktif")));

        Assert.Contains("tidak aktif", galat.Message);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Unit_layanan_yang_belum_ditautkan_tidak_dapat_diputus_siapa_pun()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(unitId: h.UnitTanpaTautanId);

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-tautan")));

        Assert.Contains("belum ditautkan ke unit organisasi", galat.Message);
        await AssertMasihMenungguAsync(h, id);
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
    public async Task Detail_menyampaikan_kewenangan_dari_server_tanpa_masukan_layar()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var atasan = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetDetailAsync(id);
        Assert.True(atasan!.HasApproveAuthority);
        Assert.True(atasan.HasUnitAuthority);
        Assert.Null(atasan.UnitAuthorityMessage);
        Assert.False(atasan.HasEmergencyAuthority);

        var tanpaIzin = await h.Layanan(h.CreateContext(), h.AtasanTanpaIzinUserId).GetDetailAsync(id);
        Assert.False(tanpaIzin!.HasApproveAuthority);
        Assert.True(tanpaIzin.HasUnitAuthority);

        var staf = await h.Layanan(h.CreateContext(), h.StafUserId).GetDetailAsync(id);
        Assert.True(staf!.HasApproveAuthority);
        Assert.False(staf.HasUnitAuthority);
        Assert.False(string.IsNullOrWhiteSpace(staf.UnitAuthorityMessage));

        var petugasCito = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId).GetDetailAsync(id);
        Assert.True(petugasCito!.HasEmergencyAuthority);

        var pemohon = await h.Layanan(h.CreateContext(), h.PemohonUserId).GetDetailAsync(id);
        Assert.True(pemohon!.IsOwnRequest);
    }

    // ------------------------------------------------------------------ otorisasi CITO

    [Fact]
    public async Task Kasus6_CITO_dengan_otorisasi_eksplisit_dapat_segera_diproses()
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
    public async Task Kasus5_CITO_tanpa_otorisasi_eksplisit_ditolak_termasuk_SuperAdmin_dan_atasan_unit()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var superAdmin = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.SuperAdminUserId).AuthorizeEmergencyAsync(id, Cito("uji-cito-sa")));
        Assert.Contains("SuperAdmin", superAdmin.Message);

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
    public async Task Atasan_yang_membuatkan_permintaan_tidak_dapat_menyetujuinya()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(dibuatOleh: h.AtasanUserId);

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-pembuat")));

        await AssertMasihMenungguAsync(h, id);
    }
}
