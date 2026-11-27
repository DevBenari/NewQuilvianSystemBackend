using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Ketahanan aturan kewenangan setelah disederhanakan (<c>FIN-LGS-070</c>): data organisasi HRD
/// yang tidak berlaku tidak berpengaruh, akun nonaktif dan izin yang dicabut langsung tidak
/// berlaku, data yang tidak ada, draf lintas unit, kesiapan yang menghitung akun berizin, dan
/// jejak audit di log.
/// </summary>
public class RobustnessAndAuditTests
{
    // ------------------------------------------------ data organisasi HRD tidak berlaku

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Unit_organisasi_HRD_yang_tidak_berlaku_tidak_memengaruhi_kewenangan_penyetuju(bool nonaktif)
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(unitId: h.UnitId);
        // Nonaktif, atau masih aktif tetapi masa berlakunya sudah habis.
        await h.UbahUnitOrganisasiAsync(h.OrgUnitId, aktif: !nonaktif,
            berakhir: nonaktif ? (DateTime?)null : DateTime.UtcNow.AddDays(-1));

        var cakupan = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetApprovalScopeAsync();
        Assert.True(cakupan.HasApprovePermission);

        var daftar = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .GetPagedAsync(new StockRequestPagedQuery { Scope = StockRequestListScope.MyAuthority });
        Assert.Contains(daftar.Items, x => x.Id == id);

        var hasil = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .ApproveAsync(id, Setujui($"uji-org-{(nonaktif ? "nonaktif" : "habis")}"));
        Assert.Equal(StockRequestStatus.Approved, hasil.Status);
    }

    [Fact]
    public async Task Akun_penyetuju_yang_dinonaktifkan_tidak_lagi_berwenang()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        await h.NonaktifkanPenggunaAsync(h.AtasanUserId);

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-akun-nonaktif")));

        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Izin_yang_dicabut_di_Akses_Role_langsung_tidak_berlaku()
    {
        using var h = new StockRequestApprovalHarness();
        var (rutin, _) = await h.PermintaanAsync();
        var (cito, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);
        await h.CabutIzinAsync(StockRequestApprovalAuthority.ApprovePermissionAction);
        await h.CabutIzinAsync(StockRequestApprovalAuthority.EmergencyPermissionAction);

        var setuju = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(rutin, Setujui("uji-izin-dicabut")));
        Assert.Contains("StockRequest:Approve", setuju.Message);

        var otorisasi = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId).AuthorizeEmergencyAsync(cito, Cito("uji-izin-dicabut-cito")));
        Assert.Contains("StockRequest:AuthorizeEmergency", otorisasi.Message);

        await AssertMasihMenungguAsync(h, rutin);
        await AssertMasihMenungguAsync(h, cito);
    }

    // ------------------------------------------------------- data yang tidak ada

    [Fact]
    public async Task Permintaan_yang_tidak_ada_ditolak_sebagai_tidak_ditemukan()
    {
        using var h = new StockRequestApprovalHarness();
        var tidakAda = Guid.NewGuid();

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(tidakAda, Setujui("uji-tidak-ada-1")));
        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).VerifyEmergencyAsync(tidakAda, Verifikasi("uji-tidak-ada-2", Versi)));
        Assert.Null(await h.Layanan(h.CreateContext(), h.AtasanUserId).GetDetailAsync(tidakAda));
    }

    // ------------------------------------------------------------ cakupan lintas unit

    [Fact]
    public async Task Draf_CITO_unit_lain_tidak_terlihat_oleh_petugas_CITO()
    {
        using var h = new StockRequestApprovalHarness();
        var (draf, _) = await h.PermintaanAsync(status: StockRequestStatus.Draft,
            unitId: h.UnitLainId, prioritas: StockRequestPriority.Emergency);
        var (diajukan, _) = await h.PermintaanAsync(unitId: h.UnitLainId, prioritas: StockRequestPriority.Emergency);

        var daftar = await h.Layanan(h.CreateContext(), h.PetugasCitoUserId)
            .GetPagedAsync(new StockRequestPagedQuery { Scope = StockRequestListScope.MyAuthority });

        Assert.Equal(diajukan, Assert.Single(daftar.Items).Id);
        Assert.DoesNotContain(daftar.Items, x => x.Id == draf);
    }

    // ------------------------------------------------------------- kesiapan data

    [Fact]
    public async Task Kesiapan_menghitung_akun_aktif_yang_benar_benar_memegang_izin()
    {
        using var h = new StockRequestApprovalHarness();

        var awal = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();
        Assert.Equal(7, awal.ApproveUsers);
        Assert.Equal(3, awal.EmergencyUsers);

        // Akun nonaktif tidak dihitung, walaupun kebijakan aksesnya tetap ada.
        await h.NonaktifkanPenggunaAsync(h.AtasanCitoUserId);
        var sesudah = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();

        Assert.Equal(6, sesudah.ApproveUsers);
        Assert.Equal(2, sesudah.EmergencyUsers);
        Assert.Equal(1, sesudah.ApprovePermissionGrants);
        Assert.True(sesudah.IsReadyForApproval);
        Assert.True(sesudah.IsReadyForEmergency);
    }

    [Fact]
    public async Task Kesiapan_tidak_menyatakan_siap_bila_tidak_ada_akun_aktif_berizin()
    {
        using var h = new StockRequestApprovalHarness();
        // Seluruh akun berizin Approve dinonaktifkan; kebijakan aksesnya tetap tercatat.
        foreach (var userId in new[]
                 {
                     h.PemohonUserId, h.AtasanUserId, h.AtasanCitoUserId, h.AtasanLainUserId,
                     h.AtasanKedaluwarsaUserId, h.AtasanNonaktifUserId, h.AtasanGandaUserId
                 })
            await h.NonaktifkanPenggunaAsync(userId);

        var siap = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();

        Assert.Equal(1, siap.ApprovePermissionGrants);
        Assert.Equal(0, siap.ApproveUsers);
        Assert.False(siap.IsReadyForApproval);
        Assert.Contains(siap.Blockers, x => x.Contains("Belum ada akun aktif yang memegang izin StockRequest:Approve"));
    }

    [Fact]
    public async Task Izin_pada_controller_nonaktif_tidak_berlaku_dan_tidak_dihitung_kesiapan()
    {
        using var h = new StockRequestApprovalHarness();
        var (rutin, _) = await h.PermintaanAsync();
        var (cito, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);
        await h.NonaktifkanControllerIzinAsync();

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(rutin, Setujui("uji-controller-nonaktif")));
        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId).AuthorizeEmergencyAsync(cito, Cito("uji-controller-nonaktif-cito")));

        // Laporan kesiapan memakai penyaring yang sama dengan penegakan: izin yang tidak dapat
        // dipakai tidak boleh dihitung sebagai izin yang sudah diberikan.
        var siap = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();
        Assert.Equal(0, siap.ApprovePermissionGrants);
        Assert.Equal(0, siap.EmergencyPermissionGrants);
        Assert.Equal(0, siap.ApproveUsers);
        Assert.Equal(0, siap.EmergencyUsers);
        Assert.False(siap.IsReadyForApproval);
        Assert.False(siap.IsReadyForEmergency);

        await AssertMasihMenungguAsync(h, rutin);
        await AssertMasihMenungguAsync(h, cito);
    }

    // ---------------------------------------------------------------- jejak audit

    [Fact]
    public async Task Keputusan_dan_penolakan_kewenangan_tercatat_di_log_audit()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        var log = new CatatanLogUji();

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.StafUserId, log: log).ApproveAsync(id, Setujui("uji-log-1")));
        await h.Layanan(h.CreateContext(), h.AtasanUserId, log: log).ApproveAsync(id, Setujui("uji-log-2", catatan: "Sah"));

        var ditolak = Assert.Single(log.UntukAksi("StockRequest.AuthorityDenied"));
        Assert.Equal(LogLevel.Warning, ditolak.Tingkat);
        Assert.Equal(h.StafUserId.ToString(), CatatanLogUji.PelakuPada(ditolak.Pesan));

        var disetujui = Assert.Single(log.UntukAksi("StockRequest.Approve"));
        Assert.Equal(LogLevel.Information, disetujui.Tingkat);
        Assert.Equal(h.AtasanUserId.ToString(), CatatanLogUji.PelakuPada(disetujui.Pesan));
    }

    [Fact]
    public async Task Pengajuan_dan_penyerahan_mencatat_pelaku_bukan_id_permintaan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, itemId) = await h.PermintaanAsync(status: StockRequestStatus.Draft);
        var log = new CatatanLogUji();

        var diajukan = await h.Layanan(h.CreateContext(), h.PemohonUserId, log: log)
            .SubmitAsync(id, new SubmitStockRequestRequest { ExpectedVersion = Versi, IdempotencyKey = "uji-log-ajukan" });
        var disetujui = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .ApproveAsync(id, Setujui("uji-log-setuju", versi: diajukan.Version));
        await h.Layanan(h.CreateContext(), h.StafUserId, log: log)
            .FulfillAsync(id, Serahkan(itemId, disetujui.Version, "uji-log-serah"));

        Assert.Equal(h.PemohonUserId.ToString(),
            CatatanLogUji.PelakuPada(Assert.Single(log.UntukAksi("StockRequest.Submit")).Pesan));
        Assert.Equal(h.StafUserId.ToString(),
            CatatanLogUji.PelakuPada(Assert.Single(log.UntukAksi("StockRequest.Fulfill")).Pesan));

        // Regresi yang pernah terjadi: id permintaan tercatat sebagai pelaku.
        Assert.DoesNotContain(log.Entri, x => CatatanLogUji.PelakuPada(x.Pesan) == id.ToString());
    }

    [Fact]
    public async Task Temuan_penyimpangan_tercatat_sebagai_audit_dan_peringatan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _, versi) = await h.CitoTerotorisasiAsync();
        var log = new CatatanLogUji();

        await h.Layanan(h.CreateContext(), h.AtasanUserId, log: log).VerifyEmergencyAsync(id,
            Verifikasi("uji-log-temuan", versi, StockRequestEmergencyReviewOutcome.Deviation, "Bukan gawat darurat"));

        var audit = Assert.Single(log.UntukAksi("StockRequest.VerifyEmergency"));
        Assert.Equal(h.AtasanUserId.ToString(), CatatanLogUji.PelakuPada(audit.Pesan));
        var peringatan = Assert.Single(log.UntukAksi("StockRequest.EmergencyDeviation"));
        Assert.Equal(LogLevel.Warning, peringatan.Tingkat);
        Assert.Equal(h.AtasanUserId.ToString(), CatatanLogUji.PelakuPada(peringatan.Pesan));
    }
}
