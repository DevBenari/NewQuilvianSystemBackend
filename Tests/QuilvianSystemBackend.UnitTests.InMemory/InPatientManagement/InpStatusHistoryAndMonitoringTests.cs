using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-028</c> — riwayat status terbaca lengkap dan tidak dapat dihapus;
/// <c>BE-RWI-029</c> — empat daftar pantau dan satu laporan selisih tersedia.
/// </summary>
public sealed class InpStatusHistoryAndMonitoringTests
{
    // =========================================================================
    // BE-RWI-028 — Riwayat status
    // =========================================================================

    /// <remarks>
    /// Kriteria 1 dan 4: riwayat terbaca urut beserta pelaku, status asal, status tujuan, dan
    /// alasannya — dan tetap terbaca setelah episode <c>Closed</c>. Justru episode yang sudah
    /// ditutup yang paling sering ditelusuri auditor.
    /// </remarks>
    [Fact]
    public async Task Kriteria1Dan4_RiwayatTerbacaUrutDanTetapTerbacaSetelahEpisodeDitutup()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            new CloseEpisodeRequest { Note = "Seluruh syarat terpenuhi." },
            InpatientEpisodeTestWorld.ActorUserId);

        var riwayat = await world.EpisodeService.GetStatusHistoryAsync(episode.Id);

        Assert.Equal(4, riwayat.Count);
        Assert.Equal(new[] { 1, 2, 3, 4 }, riwayat.Select(x => x.SequenceNumber));

        Assert.Null(riwayat[0].FromStatus);
        Assert.Equal((int)InpEpisodeStatus.Draft, riwayat[0].ToStatus);
        Assert.Equal("Draft", riwayat[0].ToStatusName);

        Assert.Equal((int)InpEpisodeStatus.Draft, riwayat[1].FromStatus);
        Assert.Equal((int)InpEpisodeStatus.Admitted, riwayat[1].ToStatus);

        Assert.Equal((int)InpEpisodeStatus.Admitted, riwayat[2].FromStatus);
        Assert.Equal((int)InpEpisodeStatus.DischargePending, riwayat[2].ToStatus);

        Assert.Equal((int)InpEpisodeStatus.DischargePending, riwayat[3].FromStatus);
        Assert.Equal((int)InpEpisodeStatus.Closed, riwayat[3].ToStatus);
        Assert.Equal("Seluruh syarat terpenuhi.", riwayat[3].Reason);

        foreach (var baris in riwayat)
        {
            Assert.Equal("User", baris.ActorTypeName);
            Assert.NotNull(baris.ChangedByUserId);
            Assert.NotEqual(default, baris.ChangedAt);
            Assert.False(string.IsNullOrWhiteSpace(baris.ActionType));
        }
    }

    /// <remarks>
    /// <b>Kriteria 3 adalah masalah keadilan, bukan teknis.</b> Mencatat kedaluwarsa otomatis
    /// atas nama pengguna yang kebetulan membaca layar akan membuat laporan pengecualian
    /// menuduh orang yang tidak melakukan apa-apa.
    /// </remarks>
    [Fact]
    public async Task Kriteria3_KedaluwarsaDicatatSebagaiTindakanSistemDenganPelakuKosong()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync(draftEpisodeExpiryHours: 24);

        var episode = await world.OpenDraftEpisodeAsync();

        var tracked = await world.DbContext.Set<InpEpisode>().FirstAsync(x => x.Id == episode.Id);
        tracked.CreateDateTime = DateTime.UtcNow.AddHours(-25);
        tracked.UpdateDateTime = null;
        await world.DbContext.SaveChangesAsync();

        // Dibaca oleh SupervisorUserId — dan justru itu yang TIDAK boleh tercatat sebagai
        // pelaku pembatalannya.
        await world.EpisodeService.GetEpisodeDetailAsync(episode.Id);

        var riwayat = await world.EpisodeService.GetStatusHistoryAsync(episode.Id);

        var kedaluwarsa = riwayat.Single(x => x.ToStatus == (int)InpEpisodeStatus.Cancelled);

        Assert.Equal("System", kedaluwarsa.ActorTypeName);
        Assert.Equal((int)InpStatusChangeActorType.System, kedaluwarsa.ActorType);
        Assert.Null(kedaluwarsa.ChangedByUserId);
        Assert.Equal(InpEpisodeService.ActionExpireDraft, kedaluwarsa.ActionType);
    }

    // =========================================================================
    // BE-RWI-029 — Empat daftar pantau dan laporan selisih
    // =========================================================================

    /// <remarks>Kriteria 1 dan 2: ambangnya milik admin, bukan angka di dalam kode.</remarks>
    [Fact]
    public async Task Kriteria1Dan2_DaftarPenutupanTertundaMemakaiAmbangYangDapatDiubahAdmin()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        // Baru saja diputuskan pulang — belum melewati ambang bawaan 4 jam.
        var belumLewat = await world.CensusQueryService.GetPendingClosuresAsync(
            new InpatientMonitoringQuery());

        Assert.Empty(belumLewat.Items);

        var tracked = await world.DbContext.Set<InpEpisode>().FirstAsync(x => x.Id == episode.Id);
        tracked.DischargeDecidedAt = DateTime.UtcNow.AddHours(-6);
        await world.DbContext.SaveChangesAsync();

        var sudahLewat = await world.CensusQueryService.GetPendingClosuresAsync(
            new InpatientMonitoringQuery());

        var butir = Assert.Single(sudahLewat.Items);

        Assert.Equal(episode.Id, butir.EpisodeId);
        Assert.Equal(4, butir.ThresholdHours);
        Assert.InRange(butir.PendingHours, 6, 7);
        Assert.True(butir.IsBedStillHeld);

        // Admin menaikkan ambang menjadi 12 jam; pembacaan berikutnya memakai angka baru.
        var setting = await world.DbContext.Set<MstInpatientSetting>().FirstAsync();
        setting.PendingClosureThresholdHours = 12;
        await world.DbContext.SaveChangesAsync();

        var setelahAmbangNaik = await world.CensusQueryService.GetPendingClosuresAsync(
            new InpatientMonitoringQuery());

        Assert.Empty(setelahAmbangNaik.Items);
    }

    [Fact]
    public async Task Kriteria4_DaftarEpisodeTanpaPerawatBertingkatDanMengikutiPenugasan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var perawat = await world.AddEmployeeAsync("Ns. Sari");

        var episode = await world.OpenAndPlaceAsync(bed);

        var sebelum = await world.CensusQueryService.GetUnassignedNurseEpisodesPagedAsync(
            new InpatientMonitoringQuery());

        Assert.Equal(1, sebelum.TotalData);
        Assert.Single(sebelum.Items);

        await world.EpisodeService.AssignNurseAsync(
            episode.Id,
            new AssignNurseRequest { EmployeeId = perawat.Id },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorIsWardHeadOrSupervisor: true);

        var sesudah = await world.CensusQueryService.GetUnassignedNurseEpisodesPagedAsync(
            new InpatientMonitoringQuery());

        Assert.Equal(0, sesudah.TotalData);
        Assert.Empty(sesudah.Items);
    }

    /// <remarks>
    /// Kriteria 5, dan roadmap mewajibkan selisihnya dibuat <b>secara sengaja</b> lewat
    /// perubahan langsung di database uji, lalu dibuktikan laporan menemukannya.
    ///
    /// <para>
    /// <b>Kenapa laporan ini penting.</b> Ia satu-satunya pengawas atas satu-satunya arah tulis
    /// lintas modul. Bila tidak pernah dibaca siapa pun, <c>MstBed.BedStatus</c> akan menyimpang
    /// diam-diam sampai seorang pasien ditempatkan di tempat tidur yang sudah ada orangnya.
    /// </para>
    /// </remarks>
    [Fact]
    public async Task Kriteria5_LaporanSelisihMenemukanSalinanStatusYangMenyimpang()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var terisi = await world.AddBedAsync(room, "3A");
        var kosong = await world.AddBedAsync(room, "3B");

        await world.OpenAndPlaceAsync(terisi);

        var bersih = await world.CensusQueryService.GetBedDriftAsync(
            new InpatientMonitoringQuery());

        Assert.Empty(bersih.Items);

        // Selisih dibuat SENGAJA: salinan status kedua tempat tidur diputar terbalik lewat
        // perubahan langsung, meniru penyimpangan yang terjadi di luar sepengetahuan modul ini.
        var trackedTerisi = await world.DbContext.Set<MstBed>().FirstAsync(x => x.Id == terisi.Id);
        var trackedKosong = await world.DbContext.Set<MstBed>().FirstAsync(x => x.Id == kosong.Id);

        trackedTerisi.BedStatus = BedStatus.Available;
        trackedKosong.BedStatus = BedStatus.Occupied;
        await world.DbContext.SaveChangesAsync();

        var selisih = await world.CensusQueryService.GetBedDriftAsync(
            new InpatientMonitoringQuery());

        Assert.Equal(2, selisih.TotalData);

        var barisTerisi = selisih.Items.Single(x => x.BedId == terisi.Id);
        Assert.Equal((int)BedStatus.Available, barisTerisi.CopiedStatus);
        Assert.Equal((int)BedStatus.Occupied, barisTerisi.ExpectedStatus);
        Assert.True(barisTerisi.HasActivePlacement);
        Assert.False(string.IsNullOrWhiteSpace(barisTerisi.HoldingEpisodeNumber));

        var barisKosong = selisih.Items.Single(x => x.BedId == kosong.Id);
        Assert.Equal((int)BedStatus.Occupied, barisKosong.CopiedStatus);
        Assert.Equal((int)BedStatus.Available, barisKosong.ExpectedStatus);
        Assert.False(barisKosong.HasActivePlacement);
        Assert.Null(barisKosong.HoldingEpisodeNumber);
    }

    /// <remarks>
    /// Keempat keadaan yang merupakan wewenang admin tidak dihitung sebagai selisih — modul
    /// Rawat Inap memang tidak pernah menuliskannya, sehingga menyalahkannya di sini akan
    /// membanjiri laporan dengan baris yang tidak dapat ditindaklanjuti siapa pun.
    /// </remarks>
    [Fact]
    public async Task LaporanSelisihTidakMenyalahkanKeadaanYangMerupakanWewenangAdmin()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        await world.AddBedAsync(room, "3A", bedStatus: BedStatus.Maintenance);
        await world.AddBedAsync(room, "3B", bedStatus: BedStatus.Cleaning);

        var selisih = await world.CensusQueryService.GetBedDriftAsync(
            new InpatientMonitoringQuery());

        Assert.Empty(selisih.Items);
    }

    [Fact]
    public async Task Kriteria6_KeempatDaftarPantauYangKosongMengembalikanDaftarKosongBukanGalat()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var query = new InpatientMonitoringQuery();

        var pendingClosures = await world.CensusQueryService.GetPendingClosuresAsync(query);
        var overrideClosures = await world.CensusQueryService.GetOverrideClosuresAsync(query);
        var unassignedNurse = await world.CensusQueryService.GetUnassignedNurseEpisodesPagedAsync(query);
        var bedDrift = await world.CensusQueryService.GetBedDriftAsync(query);

        Assert.Empty(pendingClosures.Items);
        Assert.Equal(0, pendingClosures.TotalData);

        Assert.Empty(overrideClosures.Items);
        Assert.Equal(0, overrideClosures.TotalData);

        Assert.Empty(unassignedNurse.Items);
        Assert.Equal(0, unassignedNurse.TotalData);

        Assert.Empty(bedDrift.Items);
        Assert.Equal(0, bedDrift.TotalData);
    }

    [Fact]
    public async Task DaftarPantauDapatDisaringMenurutUnitLayanan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var budi = await world.AddPatientAsync("Tn. Budi", Gender.Male);

        await world.OpenAndPlaceAsync(bed, budi.Id);

        var unitSendiri = await world.CensusQueryService.GetUnassignedNurseEpisodesPagedAsync(
            new InpatientMonitoringQuery { ServiceUnitId = world.ServiceUnit.Id });

        Assert.Single(unitSendiri.Items);

        var unitLain = await world.CensusQueryService.GetUnassignedNurseEpisodesPagedAsync(
            new InpatientMonitoringQuery { ServiceUnitId = Guid.NewGuid() });

        Assert.Empty(unitLain.Items);
    }
}
