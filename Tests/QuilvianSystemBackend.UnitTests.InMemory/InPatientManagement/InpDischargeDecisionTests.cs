using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-020</c> — DPJP dapat menyatakan pasien boleh pulang.
/// </summary>
/// <remarks>
/// <b>Dua cara pulang yang aturan klinisnya belum disahkan.</b> Meninggal dan kabur sengaja
/// tidak punya nilai enum pada revisi ini; sisi klinis keduanya masih terbuka pada
/// <c>RWI-OQ-039</c> dan <c>RWI-DEC-059</c>, menunggu pemilik klinis. Nomor 4 dan 5
/// dikosongkan supaya penambahannya kelak tidak mengubah angka yang sudah tersimpan. Delta
/// terhadap roadmap — yang menyebut "lima cara pulang" — dicatat pada laporan task.
/// </remarks>
public sealed class InpDischargeDecisionTests
{
    [Fact]
    public async Task Kriteria1_HanyaDpjpAktifYangDapatMemutuskan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var dokterJaga = await world.AddDoctorAsync("dr. Rina");

        var episode = await world.OpenAndPlaceAsync(bed);

        // Dokter lain.
        var olehDokterLain = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.SupervisorUserId,
            dokterJaga.Id);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, olehDokterLain.Status);
        Assert.Equal(
            "Hanya DPJP episode ini yang dapat menyatakan pasien boleh pulang.",
            olehDokterLain.Message);

        // Peran bukan dokter — supervisor sekalipun.
        var olehSupervisor = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.Forbidden, olehSupervisor.Status);
    }

    [Theory]
    [InlineData((int)InpDischargeType.DoctorApproved)]
    [InlineData((int)InpDischargeType.AgainstMedicalAdvice)]
    [InlineData((int)InpDischargeType.Referred)]
    public async Task Kriteria2_CaraPulangYangBerlakuPadaRevisiIniDikenali(int caraPulang)
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = caraPulang },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal((InpDischargeType)caraPulang, tersimpan.DischargeType);
    }

    [Fact]
    public async Task Kriteria2_CaraPulangKosongDitolak400DanYangBelumTersediaDitolak422()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var kosong = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.Unknown },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, kosong.Status);
        Assert.Equal("Cara pulang wajib dipilih.", kosong.Message);

        // Nomor 4 dan 5 dikosongkan untuk meninggal dan kabur, yang aturan klinisnya belum
        // disahkan. Keduanya karena itu ditolak 422 dengan pesan yang menyebutkannya.
        var belumTersedia = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = 4 },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, belumTersedia.Status);
        Assert.Equal(
            "Cara pulang yang dipilih belum tersedia pada versi ini.",
            belumTersedia.Message);
    }

    /// <remarks>
    /// Kriteria 3, 4, dan 5 sekaligus. Yang paling mudah dikerjakan terlalu jauh adalah
    /// melepas tempat tidur di langkah ini: pasien yang sudah diizinkan pulang biasanya masih
    /// berada di kamarnya beberapa jam, dan menganggap tempat tidurnya kosong akan membuat
    /// pasien berikutnya ditempatkan di atasnya.
    /// </remarks>
    [Fact]
    public async Task Kriteria3Dan4Dan5_TempatTidurTetapTerisiPasienTetapDiCensusDanSatuBarisRiwayatLahir()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var riwayatSebelum = await world.DbContext.Set<InpStatusHistory>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id);

        var hasil = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest
            {
                DischargeType = (int)InpDischargeType.DoctorApproved,
                Reason = "Kondisi klinis membaik."
            },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.DischargePending, tersimpan.EpisodeStatus);
        Assert.NotNull(tersimpan.DischargeDecidedAt);

        // Kriteria 3 — salinan status tempat tidur TIDAK berubah pada langkah ini.
        var bedSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Occupied, bedSesudah.BedStatus);

        var penempatan = await world.DbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .FirstAsync(x => x.EpisodeId == episode.Id);

        Assert.Null(penempatan.EndDateTime);

        // Kriteria 4 — pasien masih muncul pada census.
        var census = await world.CensusQueryService.GetCensusAsync(new CensusQuery());
        Assert.Single(census.Items);

        // Kriteria 5 — tepat satu baris riwayat status lahir.
        var riwayat = await world.DbContext.Set<InpStatusHistory>()
            .AsNoTracking()
            .Where(x => x.EpisodeId == episode.Id)
            .OrderBy(x => x.SequenceNumber)
            .ToListAsync();

        Assert.Equal(riwayatSebelum + 1, riwayat.Count);

        var terakhir = riwayat[^1];
        Assert.Equal(InpEpisodeStatus.Admitted, terakhir.FromStatus);
        Assert.Equal(InpEpisodeStatus.DischargePending, terakhir.ToStatus);
        Assert.Equal(InpDischargeService.ActionDecideDischarge, terakhir.ActionType);
        Assert.Equal("Kondisi klinis membaik.", terakhir.Reason);
        Assert.Equal(InpStatusChangeActorType.User, terakhir.ActorType);
    }

    [Fact]
    public async Task EpisodeDraftBelumDapatDiputuskanPulang()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var episode = await world.OpenDraftEpisodeAsync();

        var hasil = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal(
            "Pasien belum menempati tempat tidur. Selesaikan penempatan lebih dulu.",
            hasil.Message);
    }

    [Fact]
    public async Task KeputusanPulangTidakDapatDiulang()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.DoctorApproved },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        var kedua = await world.DischargeService.DecideDischargeAsync(
            episode.Id,
            new DecideDischargeRequest { DischargeType = (int)InpDischargeType.Referred },
            InpatientEpisodeTestWorld.ActorUserId,
            world.Doctor.Id);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, kedua.Status);
        Assert.Equal("Pasien sudah diputuskan boleh pulang sebelumnya.", kedua.Message);
    }
}
