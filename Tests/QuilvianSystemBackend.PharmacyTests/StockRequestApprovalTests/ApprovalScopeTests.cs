using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Cakupan "Dalam Kewenangan Saya" dan cakupan kewenangan setelah disederhanakan
/// (<c>FIN-LGS-070</c>), kesiapan izin, dan tenggat verifikasi susulan pada daftar (FIN-LGS-BE-005).
/// </summary>
public class ApprovalScopeTests
{
    private static StockRequestPagedQuery Lingkup(StockRequestStatus? status = null, int ukuran = 50) => new()
    {
        Scope = StockRequestListScope.MyAuthority,
        Status = status,
        PageSize = ukuran
    };

    // --------------------------------------------------- Dalam Kewenangan Saya

    [Fact]
    public async Task Penyetuju_melihat_permintaan_seluruh_unit_yang_sudah_diajukan_tanpa_draf()
    {
        using var h = new StockRequestApprovalHarness();
        var (unitA, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (unitB, _) = await h.PermintaanAsync(unitId: h.UnitLainId);
        var (tanpaTautan, _) = await h.PermintaanAsync(unitId: h.UnitTanpaTautanId);
        var (draf, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft, unitId: h.UnitId);

        var daftar = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetPagedAsync(Lingkup());

        Assert.Equal(3, daftar.TotalData);
        Assert.Equal(new HashSet<Guid> { unitA, unitB, tanpaTautan }, daftar.Items.Select(x => x.Id).ToHashSet());
        Assert.DoesNotContain(daftar.Items, x => x.Id == draf);
    }

    [Fact]
    public async Task Cakupan_penyetuju_sama_tanpa_melihat_penempatan_HRD()
    {
        using var h = new StockRequestApprovalHarness();
        var (unitA, _) = await h.PermintaanAsync(unitId: h.UnitId);
        var (unitB, _) = await h.PermintaanAsync(unitId: h.UnitLainId);
        var (tanpaTautan, _) = await h.PermintaanAsync(unitId: h.UnitTanpaTautanId);
        var harapan = new HashSet<Guid> { unitA, unitB, tanpaTautan };

        // Manajerial di dua unit, manajerial di unit lain, dan manajerial yang sudah berakhir:
        // semuanya berizin Approve, jadi cakupannya sama.
        foreach (var penyetuju in new[] { h.AtasanGandaUserId, h.AtasanLainUserId, h.AtasanKedaluwarsaUserId })
        {
            var daftar = await h.Layanan(h.CreateContext(), penyetuju).GetPagedAsync(Lingkup());
            Assert.Equal(harapan, daftar.Items.Select(x => x.Id).ToHashSet());
        }
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
        var (menunggu2, _) = await h.PermintaanAsync(unitId: h.UnitLainId);
        var (disetujui, _) = await h.PermintaanAsync(unitId: h.UnitId);
        await h.Layanan(h.CreateContext(), h.AtasanCitoUserId).ApproveAsync(disetujui, Setujui("uji-lingkup"));
        await h.PermintaanAsync(status: StockRequestStatus.Draft, unitId: h.UnitLainId);

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
    public async Task Cakupan_penyetuju_tanpa_catatan()
    {
        using var h = new StockRequestApprovalHarness();

        var cakupan = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetApprovalScopeAsync();

        Assert.True(cakupan.HasApprovePermission);
        Assert.False(cakupan.HasEmergencyAuthority);
        Assert.Null(cakupan.Message);
    }

    [Fact]
    public async Task Cakupan_tanpa_izin_persetujuan_disertai_penjelasan_yang_sesuai()
    {
        using var h = new StockRequestApprovalHarness();

        var staf = await h.Layanan(h.CreateContext(), h.StafUserId).GetApprovalScopeAsync();
        Assert.False(staf.HasApprovePermission);
        Assert.False(staf.HasEmergencyAuthority);
        Assert.Contains("StockRequest:Approve", staf.Message);
        Assert.Contains("Akses Role", staf.Message);

        var tanpaIzin = await h.Layanan(h.CreateContext(), h.AtasanTanpaIzinUserId).GetApprovalScopeAsync();
        Assert.False(tanpaIzin.HasApprovePermission);
        Assert.Contains("StockRequest:Approve", tanpaIzin.Message);

        var petugasCito = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId).GetApprovalScopeAsync();
        Assert.False(petugasCito.HasApprovePermission);
        Assert.True(petugasCito.HasEmergencyAuthority);
        Assert.Contains("CITO", petugasCito.Message);
    }

    [Fact]
    public async Task Kesiapan_melaporkan_jumlah_izin_dan_akun_tanpa_identitas_pegawai()
    {
        using var h = new StockRequestApprovalHarness();

        var siap = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();

        Assert.Equal(1, siap.ApprovePermissionGrants);
        Assert.Equal(1, siap.EmergencyPermissionGrants);
        // Approve: Pemohon, Atasan, AtasanCito, AtasanLain, AtasanKedaluwarsa, AtasanNonaktif, AtasanGanda.
        Assert.Equal(7, siap.ApproveUsers);
        // CITO: Pemohon, AtasanCito, PetugasCito (penempatan PetugasCitoKedaluwarsa sudah berakhir).
        Assert.Equal(3, siap.EmergencyUsers);
        Assert.True(siap.IsReadyForApproval);
        Assert.True(siap.IsReadyForEmergency);
        Assert.Contains(siap.Blockers, x => x.Contains("Tenggat"));
        Assert.DoesNotContain(siap.Blockers, x => x.Contains("HRD"));
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
