using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// "Unit yang Saya Pimpin" (FIN-LGS-BE-006), cakupan kewenangan dan kesiapan data
/// (FIN-LGS-BE-002), penunjukan jabatan CITO (FIN-LGS-BE-003), dan tenggat verifikasi
/// susulan pada daftar (FIN-LGS-BE-005).
/// </summary>
public class ApprovalScopeTests
{
    private static StockRequestPagedQuery Lingkup(StockRequestStatus? status = null, int ukuran = 50) => new()
    {
        Scope = StockRequestListScope.MyAuthority,
        Status = status,
        PageSize = ukuran
    };

    // --------------------------------------------------- Unit yang Saya Pimpin

    [Fact]
    public async Task Atasan_hanya_melihat_permintaan_unit_yang_dipimpinnya()
    {
        using var h = new StockRequestApprovalHarness();
        var (unitA, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (unitB, _) = await h.PermintaanAsync(unitId: h.UnitLainId);
        var (tanpaTautan, _) = await h.PermintaanAsync(unitId: h.UnitTanpaTautanId);

        var daftar = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetPagedAsync(Lingkup());

        Assert.Equal(unitA, Assert.Single(daftar.Items).Id);
        Assert.Equal(1, daftar.TotalData);
        Assert.DoesNotContain(daftar.Items, x => x.Id == unitB || x.Id == tanpaTautan);
    }

    [Fact]
    public async Task Atasan_beberapa_unit_melihat_seluruh_unit_yang_sah()
    {
        using var h = new StockRequestApprovalHarness();
        var (unitA, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (unitB, _) = await h.PermintaanAsync(unitId: h.UnitLainId);
        await h.PermintaanAsync(unitId: h.UnitTanpaTautanId);

        var daftar = await h.Layanan(h.CreateContext(), h.AtasanGandaUserId).GetPagedAsync(Lingkup());

        Assert.Equal(2, daftar.TotalData);
        Assert.Equal(new HashSet<Guid> { unitA, unitB }, daftar.Items.Select(x => x.Id).ToHashSet());
    }

    [Theory]
    [InlineData("tanpa-izin")]
    [InlineData("staf")]
    [InlineData("superadmin")]
    public async Task Pengguna_tanpa_kewenangan_mendapat_daftar_kosong_bukan_seluruh_data(string siapa)
    {
        using var h = new StockRequestApprovalHarness();
        await h.PermintaanAsync(unitId: h.UnitId);
        await h.PermintaanAsync(unitId: h.UnitLainId, prioritas: StockRequestPriority.Emergency);

        var pengguna = siapa switch
        {
            "tanpa-izin" => h.AtasanTanpaIzinUserId,
            "staf" => h.StafUserId,
            _ => h.SuperAdminUserId
        };

        var daftar = await h.Layanan(h.CreateContext(), pengguna).GetPagedAsync(Lingkup());

        Assert.Empty(daftar.Items);
        Assert.Equal(0, daftar.TotalData);
    }

    [Fact]
    public async Task Petugas_CITO_melihat_permintaan_CITO_lintas_unit_tetapi_bukan_permintaan_rutin()
    {
        using var h = new StockRequestApprovalHarness();
        var (citoA, _) = await h.PermintaanAsync(unitId: h.UnitId, prioritas: StockRequestPriority.Emergency);
        var (citoB, _) = await h.PermintaanAsync(unitId: h.UnitLainId, prioritas: StockRequestPriority.Emergency);
        var (rutin, _) = await h.PermintaanAsync(unitId: h.UnitId);

        var daftar = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId).GetPagedAsync(Lingkup());

        Assert.Equal(new HashSet<Guid> { citoA, citoB }, daftar.Items.Select(x => x.Id).ToHashSet());
        Assert.DoesNotContain(daftar.Items, x => x.Id == rutin);
    }

    [Fact]
    public async Task Lingkup_konsisten_dengan_saringan_status_pencarian_dan_halaman()
    {
        using var h = new StockRequestApprovalHarness();
        var (menunggu1, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (menunggu2, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (disetujui, _) = await h.PermintaanAsync(unitId: h.UnitId);
        await h.Layanan(h.CreateContext(), h.AtasanCitoUserId).ApproveAsync(disetujui, Setujui("uji-lingkup"));
        await h.PermintaanAsync(unitId: h.UnitLainId);

        var layanan = h.Layanan(h.CreateContext(), h.AtasanUserId);

        var halaman1 = await layanan.GetPagedAsync(Lingkup(StockRequestStatus.Submitted, ukuran: 1));
        Assert.Equal(2, halaman1.TotalData);
        Assert.Equal(2, halaman1.TotalPage);
        Assert.Single(halaman1.Items);

        var disetujuiSaja = await layanan.GetPagedAsync(Lingkup(StockRequestStatus.Approved));
        Assert.Equal(disetujui, Assert.Single(disetujuiSaja.Items).Id);

        var cari = Lingkup();
        cari.Search = "UJI-SRQ";
        var hasilCari = await layanan.GetPagedAsync(cari);
        Assert.Equal(3, hasilCari.TotalData);
        Assert.Contains(hasilCari.Items, x => x.Id == menunggu1);
        Assert.Contains(hasilCari.Items, x => x.Id == menunggu2);
    }

    [Fact]
    public async Task Lingkup_bawaan_tetap_seluruh_data_demi_kompatibilitas()
    {
        using var h = new StockRequestApprovalHarness();
        await h.PermintaanAsync(unitId: h.UnitId);
        await h.PermintaanAsync(unitId: h.UnitLainId);

        var daftar = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .GetPagedAsync(new StockRequestPagedQuery());

        Assert.Equal(2, daftar.TotalData);
    }

    // ------------------------------------------------------- cakupan kewenangan

    [Fact]
    public async Task Cakupan_atasan_memuat_unit_yang_dipimpin_tanpa_catatan()
    {
        using var h = new StockRequestApprovalHarness();

        var cakupan = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetApprovalScopeAsync();

        Assert.True(cakupan.HasApprovePermission);
        Assert.False(cakupan.HasEmergencyAuthority);
        Assert.True(cakupan.EmergencyDesignationConfigured);
        Assert.Equal(h.UnitId, Assert.Single(cakupan.SupervisedUnits).ServiceUnitId);
        Assert.Null(cakupan.Message);
    }

    [Fact]
    public async Task Cakupan_kosong_disertai_penjelasan_yang_sesuai()
    {
        using var h = new StockRequestApprovalHarness();

        var staf = await h.Layanan(h.CreateContext(), h.StafUserId).GetApprovalScopeAsync();
        Assert.Empty(staf.SupervisedUnits);
        Assert.Contains("penempatan manajerial", staf.Message);

        var tanpaIzin = await h.Layanan(h.CreateContext(), h.AtasanTanpaIzinUserId).GetApprovalScopeAsync();
        Assert.False(tanpaIzin.HasApprovePermission);
        Assert.Contains("StockRequest:Approve", tanpaIzin.Message);

        var tanpaTunjuk = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId, jabatanCito: "")
            .GetApprovalScopeAsync();
        Assert.False(tanpaTunjuk.HasEmergencyAuthority);
        Assert.False(tanpaTunjuk.EmergencyDesignationConfigured);
        Assert.Contains("belum ditetapkan", tanpaTunjuk.Message);
    }

    [Fact]
    public async Task Kesiapan_data_melaporkan_jumlah_tanpa_identitas_pegawai()
    {
        using var h = new StockRequestApprovalHarness();

        var siap = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();

        Assert.Equal(3, siap.ActiveServiceUnits);
        Assert.Equal(2, siap.MappedServiceUnits);
        Assert.Equal(2, siap.MappedServiceUnitsWithSupervisor);
        Assert.Equal(1, siap.ApprovePermissionGrants);
        Assert.Equal(1, siap.EmergencyPermissionGrants);
        Assert.Equal(1, siap.EmergencyDesignatedPositions);
        Assert.Equal(1, siap.EmergencyDesignatedPositionsWithGrant);
        Assert.True(siap.IsReadyForApproval);
        Assert.True(siap.IsReadyForEmergency);
        Assert.Contains(siap.Blockers, x => x.Contains("Tenggat"));
        Assert.Null(siap.Units.Single(x => x.ServiceUnitId == h.UnitTanpaTautanId).OrganizationUnitName);
    }

    // ------------------------------------------------- penunjukan jabatan CITO

    [Fact]
    public async Task Tanpa_penunjukan_jabatan_otorisasi_CITO_ditolak_walau_izin_ada()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId, jabatanCito: "")
                .AuthorizeEmergencyAsync(id, Cito("uji-tunjuk-kosong")));

        Assert.Contains("belum ditetapkan", galat.Message);
        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Jabatan_berizin_yang_tidak_ditunjuk_tidak_berwenang_CITO()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        // Hanya jabatan penyetuju yang ditunjuk; izin CITO petugas ada pada jabatan lain.
        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId, jabatanCito: h.JabatanPenyetujuId.ToString())
                .AuthorizeEmergencyAsync(id, Cito("uji-tunjuk-lain")));

        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Penunjukan_berdasarkan_kode_jabatan_berlaku()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);

        var hasil = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId,
                jabatanCito: StockRequestApprovalHarness.KodeJabatanCito)
            .AuthorizeEmergencyAsync(id, Cito("uji-tunjuk-kode"));

        Assert.Equal(StockRequestStatus.Approved, hasil.Status);
    }

    // ------------------------------------------------------ tenggat pada daftar

    [Fact]
    public async Task Daftar_menggolongkan_tenggat_sama_dengan_label_per_baris()
    {
        using var h = new StockRequestApprovalHarness();
        var (terlambat, _, _) = await h.CitoTerotorisasiAsync();
        var (mendekati, _, _) = await h.CitoTerotorisasiAsync();
        var (belum, _, _) = await h.CitoTerotorisasiAsync();
        await h.MundurkanOtorisasiAsync(terlambat, TimeSpan.FromHours(30));
        await h.MundurkanOtorisasiAsync(mendekati, TimeSpan.FromHours(22));

        var layanan = h.Layanan(h.CreateContext(), h.AtasanUserId, tenggatJam: "24", peringatanJam: "4");

        async Task<Guid> SatuSaja(StockRequestEmergencyReviewDueState keadaan)
        {
            var daftar = await layanan.GetPagedAsync(new StockRequestPagedQuery { EmergencyReviewDueState = keadaan });
            var baris = Assert.Single(daftar.Items);
            Assert.Equal(keadaan, baris.EmergencyReviewDueState);
            Assert.NotNull(baris.EmergencyReviewDueAt);
            // Nama pengotorisasi dibaca dari profil tenaga kerjanya (harness: kode profil).
            Assert.Equal("UJI-WFP-CITO", baris.EmergencyAuthorizedByName);
            return baris.Id;
        }

        Assert.Equal(terlambat, await SatuSaja(StockRequestEmergencyReviewDueState.Overdue));
        Assert.Equal(mendekati, await SatuSaja(StockRequestEmergencyReviewDueState.DueSoon));
        Assert.Equal(belum, await SatuSaja(StockRequestEmergencyReviewDueState.NotDue));
    }

    [Fact]
    public async Task Tenggat_belum_dikonfigurasi_tidak_menggolongkan_apa_pun()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, _) = await h.CitoTerotorisasiAsync();
        await h.MundurkanOtorisasiAsync(id, TimeSpan.FromDays(10));

        var layanan = h.Layanan(h.CreateContext(), h.AtasanUserId);

        var terlambat = await layanan.GetPagedAsync(new StockRequestPagedQuery
        {
            EmergencyReviewDueState = StockRequestEmergencyReviewDueState.Overdue
        });
        Assert.Empty(terlambat.Items);

        var detail = await layanan.GetDetailAsync(id);
        Assert.Equal(StockRequestEmergencyReviewDueState.NotConfigured, detail!.EmergencyReviewDueState);
        Assert.Null(detail.EmergencyReviewDueAt);
        Assert.NotNull(detail.EmergencyAuthorizedAt);
    }

    [Fact]
    public async Task Tenggat_tidak_berlaku_lagi_setelah_diverifikasi_dan_tidak_memverifikasi_otomatis()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();
        await h.MundurkanOtorisasiAsync(id, TimeSpan.FromHours(30));

        var sebelum = await h.Layanan(h.CreateContext(), h.AtasanUserId, tenggatJam: "24").GetDetailAsync(id);
        Assert.Equal(StockRequestEmergencyReviewDueState.Overdue, sebelum!.EmergencyReviewDueState);
        Assert.Equal(StockRequestEmergencyReviewState.Pending, sebelum.EmergencyReviewState);

        var sesudah = await h.Layanan(h.CreateContext(), h.AtasanUserId, tenggatJam: "24")
            .VerifyEmergencyAsync(id, Verifikasi("uji-tenggat-verif", versi));

        Assert.Equal(StockRequestEmergencyReviewDueState.NotApplicable, sesudah.EmergencyReviewDueState);
        Assert.Null(sesudah.EmergencyReviewDueAt);
    }
}
