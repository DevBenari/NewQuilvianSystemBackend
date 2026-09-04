using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-025</c> — kelima syarat penutupan diperiksa dan dilaporkan satu per satu;
/// <c>BE-RWI-026</c> — jalan keluar supervisor sempit dan selalu tercatat.
/// </summary>
/// <remarks>
/// <b>Kriteria 3 milik <c>BE-RWI-026</c> adalah inti kedua task ini.</b> Jalan keluar yang
/// menembus semua syarat sekaligus akan menjadi jalur normal dalam hitungan minggu, dan kelima
/// syarat kehilangan artinya. Karena itu ada test yang mencoba menembus dengan resume yang
/// belum ditandatangani, dan membuktikan penolakannya tetap berlaku.
/// </remarks>
public sealed class InpEpisodeClosureTests
{
    // =========================================================================
    // BE-RWI-025 — Kelima syarat dan penutupan
    // =========================================================================

    /// <remarks>
    /// Kriteria 1 sering dikerjakan sebagai boolean karena lebih sederhana. Layar kemudian
    /// tidak dapat memberi tahu petugas apa yang harus dikejar, dan petugas menebak. Bentuk
    /// daftar adalah kontrak, bukan preferensi.
    /// </remarks>
    [Fact]
    public async Task Kriteria1_ClosureReadinessMengembalikanKelimaSyaratBesertaTandanya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        await world.AddClearanceItemAsync("Berkas administrasi lengkap");

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var belumApaApa = await world.DischargeService.EvaluateClosureReadinessAsync(episode.Id);

        Assert.NotNull(belumApaApa);
        Assert.Equal(5, belumApaApa!.Conditions.Count);
        Assert.False(belumApaApa.IsReady);
        Assert.False(belumApaApa.IsReadyWithOverride);

        Assert.Equal(
            new[] { 1, 2, 3, 4, 5 },
            belumApaApa.Conditions.Select(x => x.Number));

        Assert.Equal(
            new[]
            {
                "DISCHARGE_DECIDED",
                "SUMMARY_SIGNED",
                "CLEARANCE_COMPLETE",
                "FINANCIAL_CLEARED",
                "BED_STATE_RESOLVED"
            },
            belumApaApa.Conditions.Select(x => x.Code));

        // Syarat kelima sudah terpenuhi sejak pasien ditempatkan; empat lainnya belum.
        Assert.True(belumApaApa.Conditions.Single(x => x.Number == 5).IsSatisfied);

        foreach (var syarat in belumApaApa.Conditions.Where(x => !x.IsSatisfied))
        {
            Assert.False(string.IsNullOrWhiteSpace(syarat.UnmetMessage));
        }

        // Hanya syarat keuangan yang dapat ditembus supervisor.
        Assert.Single(belumApaApa.Conditions.Where(x => x.CanBeOverridden));
        Assert.Equal(
            "FINANCIAL_CLEARED",
            belumApaApa.Conditions.Single(x => x.CanBeOverridden).Code);
    }

    [Fact]
    public async Task Kriteria2_PenutupanDenganSyaratBelumTerpenuhiDitolak422DisertaiDaftarnya()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        await world.AddClearanceItemAsync("Berkas administrasi lengkap");

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);

        // Pesannya menyebut syarat-syarat yang kurang, bukan satu kalimat umum.
        Assert.Contains("DPJP menyatakan pasien boleh pulang", hasil.Message);
        Assert.Contains("Resume pulang belum ditandatangani", hasil.Message);
        Assert.Contains("Berkas administrasi lengkap", hasil.Message);
        Assert.Contains("Kelayakan keuangan belum dinyatakan lunas", hasil.Message);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.Admitted, tersimpan.EpisodeStatus);
    }

    /// <remarks>
    /// Kriteria 3, 4, dan 5 sekaligus, ditambah verifikasi yang diminta roadmap: menutup
    /// episode lalu mencari tempat tidur kosong dan menemukannya.
    /// </remarks>
    [Fact]
    public async Task Kriteria3Dan4Dan5_PenutupanMelepasTempatTidurDanMenulisSatuBarisRiwayat()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        await world.AddClearanceItemAsync("Berkas administrasi lengkap");

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        var sebelumTutup = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        Assert.Empty(sebelumTutup.Items);

        var riwayatSebelum = await world.DbContext.Set<InpStatusHistory>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id);

        var tutup = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            new CloseEpisodeRequest { Note = "Seluruh syarat terpenuhi." },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, tutup.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.Closed, tersimpan.EpisodeStatus);
        Assert.NotNull(tersimpan.ClosedAt);
        Assert.False(tersimpan.IsClosedWithoutFinancialClearance);

        // Penempatan ditutup dengan alasan penutupan episode.
        var penempatan = await world.DbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .FirstAsync(x => x.EpisodeId == episode.Id);

        Assert.NotNull(penempatan.EndDateTime);
        Assert.Equal(InpBedPlacementEndReason.EpisodeClosed, penempatan.EndReason);

        var bedSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Available, bedSesudah.BedStatus);

        // Kriteria 4 — tempat tidur terbaca Available pada pencarian berikutnya.
        var sesudahTutup = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        var tersedia = Assert.Single(sesudahTutup.Items);
        Assert.Equal(bed.Id, tersedia.BedId);

        // Kriteria 5 — tepat satu baris riwayat status lahir.
        var riwayat = await world.DbContext.Set<InpStatusHistory>()
            .AsNoTracking()
            .Where(x => x.EpisodeId == episode.Id)
            .OrderBy(x => x.SequenceNumber)
            .ToListAsync();

        Assert.Equal(riwayatSebelum + 1, riwayat.Count);
        Assert.Equal(InpEpisodeStatus.DischargePending, riwayat[^1].FromStatus);
        Assert.Equal(InpEpisodeStatus.Closed, riwayat[^1].ToStatus);
        Assert.Equal(InpDischargeService.ActionCloseEpisode, riwayat[^1].ActionType);

        // Penugasan DPJP ikut ditutup, supaya riwayatnya tidak menggantung.
        var dpjpAktif = await world.DbContext.Set<InpDoctorAssignment>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id && x.EndDateTime == null);

        Assert.Equal(0, dpjpAktif);
    }

    [Fact]
    public async Task EpisodeYangSudahDitutupTidakDapatDitutupLagi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        var kedua = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, kedua.Status);
        Assert.Equal("Episode sudah ditutup.", kedua.Message);
    }

    // =========================================================================
    // BE-RWI-026 — Jalan keluar supervisor
    // =========================================================================

    [Fact]
    public async Task Kriteria1_HanyaSupervisorYangDapatMemanggilJalanKeluar()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed, markFinancialCleared: false);

        var hasil = await world.DischargeService.CloseWithOverrideAsync(
            episode.Id,
            new CloseEpisodeOverrideRequest
            {
                Reason = "Pasien harus segera pulang atas permintaan keluarga."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            actorIsSupervisor: false);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, hasil.Status);
        Assert.Equal(
            "Hanya supervisor yang dapat menutup episode tanpa kelayakan keuangan.",
            hasil.Message);
    }

    [Fact]
    public async Task Kriteria2_JalanKeluarTanpaAlasanDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed, markFinancialCleared: false);

        var hasil = await world.DischargeService.CloseWithOverrideAsync(
            episode.Id,
            new CloseEpisodeOverrideRequest { Reason = "  -  " },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, hasil.Status);
        Assert.Equal(
            "Alasan penutupan tanpa kelayakan keuangan wajib diisi.",
            hasil.Message);
    }

    /// <remarks>
    /// <b>Inti task ini.</b> Verifikasi yang diminta roadmap: mencoba menembus dengan resume
    /// yang <b>belum</b> ditandatangani dan membuktikan tetap ditolak. Jalan keluar yang
    /// menembus semua syarat sekaligus akan menjadi jalur normal, dan kelima syarat kehilangan
    /// arti.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_JalanKeluarMenembusHanyaSyaratKeuangan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        await world.AddClearanceItemAsync("Berkas administrasi lengkap");

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        // Resume BELUM ditandatangani, butir administrasi BELUM ditandai.
        var ditolak = await world.DischargeService.CloseWithOverrideAsync(
            episode.Id,
            new CloseEpisodeOverrideRequest { Reason = "Pasien harus segera pulang." },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, ditolak.Status);
        Assert.Contains("Resume pulang belum ditandatangani", ditolak.Message);
        Assert.Contains("Berkas administrasi lengkap", ditolak.Message);

        // Yang TIDAK boleh muncul: penolakan karena kelayakan keuangan — justru itulah yang
        // ditembus.
        Assert.DoesNotContain("Kelayakan keuangan belum dinyatakan lunas", ditolak.Message);
    }

    /// <remarks>Kriteria 4 dan 5: penandaan tersimpan, dan episodenya muncul pada daftar pantau.</remarks>
    [Fact]
    public async Task Kriteria4Dan5_EpisodeDitandaiDanMunculPadaDaftarPantauPenutupanMenembusGerbang()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed, markFinancialCleared: false);

        var sebelum = await world.CensusQueryService.GetOverrideClosuresAsync(
            new InpatientMonitoringQuery());

        Assert.Empty(sebelum.Items);

        var hasil = await world.DischargeService.CloseWithOverrideAsync(
            episode.Id,
            new CloseEpisodeOverrideRequest
            {
                Reason = "Pasien harus segera pulang atas permintaan keluarga."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsSupervisor: true);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.Closed, tersimpan.EpisodeStatus);
        Assert.True(tersimpan.IsClosedWithoutFinancialClearance);
        Assert.Equal(
            "Pasien harus segera pulang atas permintaan keluarga.",
            tersimpan.ClosedWithoutClearanceReason);

        var riwayat = await world.DbContext.Set<InpStatusHistory>()
            .AsNoTracking()
            .Where(x => x.EpisodeId == episode.Id)
            .OrderBy(x => x.SequenceNumber)
            .ToListAsync();

        Assert.Equal(
            InpDischargeService.ActionCloseEpisodeWithOverride,
            riwayat[^1].ActionType);

        var sesudah = await world.CensusQueryService.GetOverrideClosuresAsync(
            new InpatientMonitoringQuery());

        var butir = Assert.Single(sesudah.Items);
        Assert.Equal(episode.Id, butir.EpisodeId);
        Assert.Equal(
            "Pasien harus segera pulang atas permintaan keluarga.",
            butir.ClosedWithoutClearanceReason);
    }
}
