using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Tests.Pharmacy.Infrastructure;
using static QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests.Perintah;

namespace QuilvianSystemBackend.Tests.Pharmacy.StockRequestApprovalTests;

/// <summary>
/// Celah yang ditemukan pada verifikasi pasca-finishing (FIN-LGS-VER-003): unit organisasi tidak
/// aktif, akun dinonaktifkan, data yang tidak ada, jabatan CITO yang dinonaktifkan, draf lintas
/// unit, kesiapan data yang mencocokkan atasan dengan izinnya, dan jejak audit di log.
/// </summary>
public class RobustnessAndAuditTests
{
    // ------------------------------------------------ data organisasi tidak berlaku

    [Fact]
    public async Task Unit_organisasi_nonaktif_mencabut_kewenangan_atasan_dan_cakupannya()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(unitId: h.UnitId);
        await h.UbahUnitOrganisasiAsync(h.OrgUnitId, aktif: false);

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-org-nonaktif")));
        Assert.Contains("tidak aktif", galat.Message);

        var cakupan = await h.Layanan(h.CreateContext(), h.AtasanUserId).GetApprovalScopeAsync();
        Assert.Empty(cakupan.SupervisedUnits);

        var daftar = await h.Layanan(h.CreateContext(), h.AtasanUserId)
            .GetPagedAsync(new StockRequestPagedQuery { Scope = StockRequestListScope.MyAuthority });
        Assert.Empty(daftar.Items);

        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Unit_organisasi_yang_masa_berlakunya_habis_tidak_memberi_kewenangan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(unitId: h.UnitId);
        await h.UbahUnitOrganisasiAsync(h.OrgUnitId, aktif: true, berakhir: DateTime.UtcNow.AddDays(-1));

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-org-habis")));

        var siap = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();
        Assert.Null(siap.Units.Single(x => x.ServiceUnitId == h.UnitId).OrganizationUnitName);

        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Akun_atasan_yang_dinonaktifkan_tidak_lagi_berwenang()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync();
        await h.NonaktifkanPenggunaAsync(h.AtasanUserId);

        await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.AtasanUserId).ApproveAsync(id, Setujui("uji-akun-nonaktif")));

        await AssertMasihMenungguAsync(h, id);
    }

    [Fact]
    public async Task Jabatan_CITO_yang_dinonaktifkan_tidak_lagi_memberi_kewenangan()
    {
        using var h = new StockRequestApprovalHarness();
        var (id, _) = await h.PermintaanAsync(prioritas: StockRequestPriority.Emergency);
        await h.NonaktifkanJabatanAsync(h.JabatanCitoId);

        var galat = await Assert.ThrowsAsync<StockRequestForbiddenException>(() =>
            h.Layanan(h.CreateContext(), h.PetugasCitoUserId).AuthorizeEmergencyAsync(id, Cito("uji-jabatan-nonaktif")));

        // Satu-satunya jabatan yang ditunjuk sudah nonaktif, jadi penunjukan efektifnya kosong.
        Assert.Contains("belum ditetapkan", galat.Message);
        await AssertMasihMenungguAsync(h, id);
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
    public async Task Kesiapan_mencocokkan_atasan_unit_dengan_izin_persetujuannya()
    {
        using var h = new StockRequestApprovalHarness();

        var siap = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();

        // OrgUnit: Atasan, AtasanCito, Pemohon, AtasanTanpaIzin, AtasanGanda (manajerial aktif);
        // AtasanTanpaIzin tidak memegang izin Approve.
        var unitA = siap.Units.Single(x => x.ServiceUnitId == h.UnitId);
        Assert.Equal(5, unitA.ActiveSupervisors);
        Assert.Equal(4, unitA.AuthorizedSupervisors);

        var unitB = siap.Units.Single(x => x.ServiceUnitId == h.UnitLainId);
        Assert.Equal(2, unitB.ActiveSupervisors);
        Assert.Equal(2, unitB.AuthorizedSupervisors);

        Assert.Equal(2, siap.MappedServiceUnitsWithAuthorizedSupervisor);
        Assert.True(siap.IsReadyForApproval);
    }

    [Fact]
    public async Task Kesiapan_tidak_menyatakan_siap_bila_atasan_tidak_memegang_izin()
    {
        using var h = new StockRequestApprovalHarness();
        // Seluruh atasan berizin dinonaktifkan akunnya; yang tersisa hanya atasan tanpa izin.
        foreach (var userId in new[] { h.AtasanUserId, h.AtasanCitoUserId, h.PemohonUserId, h.AtasanGandaUserId, h.AtasanLainUserId })
            await h.NonaktifkanPenggunaAsync(userId);

        var siap = await h.Layanan(h.CreateContext(), h.SuperAdminUserId).GetApprovalReadinessAsync();

        Assert.Equal(0, siap.MappedServiceUnitsWithAuthorizedSupervisor);
        Assert.False(siap.IsReadyForApproval);
        Assert.Contains(siap.Blockers, x => x.Contains("akunnya aktif dan memegang izin"));
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
        Assert.Equal(0, siap.EmergencyDesignatedPositionsWithGrant);
        Assert.Equal(0, siap.MappedServiceUnitsWithAuthorizedSupervisor);
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
