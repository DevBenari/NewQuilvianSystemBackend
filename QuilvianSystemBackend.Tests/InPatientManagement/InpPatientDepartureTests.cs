using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-027</c> — tempat tidur bebas sejak pasien meninggalkan kamar.
/// </summary>
/// <remarks>
/// <b>Kriteria 5 melawan intuisi, dan itulah yang paling penting dijaga.</b> Pencatatan
/// kepergian <b>tidak</b> menulis baris riwayat status, karena status episode memang tidak
/// berubah. <c>RWI-DEC-009</c> mengunci lima nilai status, dan kepergian fisik sengaja tidak
/// dijadikan status keenam — ia fakta yang dicatat, bukan tahapan yang dilalui.
/// </remarks>
public sealed class InpPatientDepartureTests
{
    [Fact]
    public async Task Kriteria1Dan2_KepergianMelepasTempatTidurTetapiEpisodeTetapDischargePending()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        var sebelum = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        Assert.Empty(sebelum.Items);

        var hasil = await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, hasil.Status);

        // Kriteria 1 — tempat tidur muncul pada pencarian berikutnya.
        var sesudah = await world.BedOccupancyService.SearchAvailableBedsAsync(
            new AvailableBedQuery());

        var tersedia = Assert.Single(sesudah.Items);
        Assert.Equal(bed.Id, tersedia.BedId);

        var bedSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Available, bedSesudah.BedStatus);

        var penempatan = await world.DbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .FirstAsync(x => x.EpisodeId == episode.Id);

        Assert.NotNull(penempatan.EndDateTime);
        Assert.Equal(InpBedPlacementEndReason.PatientDeparted, penempatan.EndReason);
        Assert.Equal(InpatientEpisodeTestWorld.ActorUserId, penempatan.EndedByUserId);

        // Kriteria 2 — episode TETAP DischargePending dan tetap wajib ditutup.
        var tersimpan = await world.DbContext.Set<InpEpisode>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == episode.Id);

        Assert.Equal(InpEpisodeStatus.DischargePending, tersimpan.EpisodeStatus);
        Assert.NotNull(tersimpan.PhysicallyLeftAt);
        Assert.Equal(InpatientEpisodeTestWorld.ActorUserId, tersimpan.PhysicallyLeftByUserId);
        Assert.Null(tersimpan.ClosedAt);
    }

    [Fact]
    public async Task Kriteria3_PasienYangSudahPergiTidakMunculDiCensusDanTidakDapatDipindahkan()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");
        var tujuan = await world.AddBedAsync(room, "3B");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        var censusSebelum = await world.CensusQueryService.GetCensusAsync(new CensusQuery());
        Assert.Single(censusSebelum.Items);

        await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        var censusSesudah = await world.CensusQueryService.GetCensusAsync(new CensusQuery());
        Assert.Empty(censusSesudah.Items);

        var pindah = await world.BedOccupancyService.TransferAsync(
            new TransferPatientRequest
            {
                EpisodeId = episode.Id,
                TargetBedId = tujuan.Id,
                TransferReason = "Permintaan keluarga."
            },
            InpatientEpisodeTestWorld.SupervisorUserId,
            actorDoctorId: null);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, pindah.Status);
    }

    /// <remarks>
    /// Kriteria 4. Kepergian fisik <b>tidak wajib</b> dicatat: episode yang ditutup tanpa
    /// pencatatan kepergian tetap melepas tempat tidurnya pada saat penutupan.
    /// </remarks>
    [Fact]
    public async Task Kriteria4_MenutupEpisodeTanpaMencatatKepergianTetapBerhasil()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        var tutup = await world.DischargeService.CloseEpisodeAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, tutup.Status);

        var penempatan = await world.DbContext.Set<InpBedPlacement>()
            .AsNoTracking()
            .FirstAsync(x => x.EpisodeId == episode.Id);

        Assert.NotNull(penempatan.EndDateTime);
        Assert.Equal(InpBedPlacementEndReason.EpisodeClosed, penempatan.EndReason);

        var bedSesudah = await world.DbContext.Set<MstBed>()
            .AsNoTracking()
            .FirstAsync(x => x.Id == bed.Id);

        Assert.Equal(BedStatus.Available, bedSesudah.BedStatus);
    }

    /// <remarks>
    /// Kriteria 5, dan roadmap mewajibkan barisnya <b>dihitung sebelum dan sesudah</b> — bukan
    /// sekadar diperiksa jenisnya.
    /// </remarks>
    [Fact]
    public async Task Kriteria5_KepergianTidakMenulisSatuPunBarisRiwayatStatus()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        var sebelum = await world.DbContext.Set<InpStatusHistory>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id);

        await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        var sesudah = await world.DbContext.Set<InpStatusHistory>()
            .AsNoTracking()
            .CountAsync(x => x.EpisodeId == episode.Id);

        Assert.Equal(sebelum, sesudah);
    }

    [Fact]
    public async Task Kriteria6_MencatatKepergianPadaEpisodeAdmittedDitolak422()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.OpenAndPlaceAsync(bed);

        var hasil = await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.BusinessRuleRejected, hasil.Status);
        Assert.Equal(
            "Kepergian hanya dapat dicatat setelah DPJP menyatakan pasien boleh pulang.",
            hasil.Message);
    }

    [Fact]
    public async Task Kriteria7_MencatatKepergianDuaKaliDitolak409()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        var kedua = await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Conflict, kedua.Status);
        Assert.Contains("Kepergian pasien sudah dicatat pada pukul", kedua.Message);
    }

    [Fact]
    public async Task Kriteria8_WaktuKepergianYangMendahuluiKeputusanPulangDitolak400()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        var mendahului = await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            new RecordDepartureRequest { DepartedAt = DateTime.UtcNow.AddHours(-5) },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, mendahului.Status);
        Assert.Equal(
            "Waktu kepergian tidak boleh mendahului keputusan pulang.",
            mendahului.Message);

        var masaDepan = await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            new RecordDepartureRequest { DepartedAt = DateTime.UtcNow.AddHours(1) },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Invalid, masaDepan.Status);
        Assert.Equal(
            "Waktu kepergian tidak boleh melewati waktu sekarang.",
            masaDepan.Message);
    }

    /// <remarks>
    /// Kriteria 9. Penyimpanan digagalkan di tengah jalan; yang dibuktikan adalah kolom
    /// kepergian pada episode <b>juga</b> tidak terisi, sehingga tidak pernah ada keadaan
    /// tempat tidur bebas sementara episodenya belum tahu pasiennya sudah pergi.
    /// </remarks>
    [Fact]
    public async Task Kriteria9_BilaPelepasanTempatTidurGagalKolomKepergianJugaTidakTerisi()
    {
        var databaseName = $"inpatient-departure-fail-{Guid.NewGuid():N}";

        var world = await InpatientEpisodeTestWorld.CreateAsync(
            dbContext: IsolatedInpatientDbContextFactory.Create(databaseName));

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        await using var failing = IsolatedInpatientDbContextFactory.CreateFailingSave(databaseName);

        var failingWorld = InpatientEpisodeTestWorld.Build(
            failing,
            world.Patient,
            world.Doctor,
            world.ServiceUnit,
            world.PatientClass);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            failingWorld.DischargeService.RecordPatientDepartureAsync(
                episode.Id,
                null,
                InpatientEpisodeTestWorld.ActorUserId));

        await using var pembaca = IsolatedInpatientDbContextFactory.Create(databaseName);

        var tersimpan = await pembaca.Set<InpEpisode>().FirstAsync(x => x.Id == episode.Id);

        Assert.Null(tersimpan.PhysicallyLeftAt);
        Assert.Null(tersimpan.PhysicallyLeftByUserId);

        var penempatan = await pembaca.Set<InpBedPlacement>()
            .FirstAsync(x => x.EpisodeId == episode.Id);

        Assert.Null(penempatan.EndDateTime);
    }

    /// <remarks>
    /// Setelah kepergian dicatat, episodenya tetap muncul pada daftar pantau penutupan
    /// tertunda — dengan penanda bahwa tempat tidurnya sudah tidak tertahan.
    /// </remarks>
    [Fact]
    public async Task EpisodeYangKepergiannyaSudahDicatatTetapMunculPadaDaftarPantauPenutupanTertunda()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();

        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room, "3A");

        var episode = await world.BuildClosableEpisodeAsync(bed);

        await world.DischargeService.RecordPatientDepartureAsync(
            episode.Id,
            null,
            InpatientEpisodeTestWorld.ActorUserId);

        // Keputusan pulangnya dimundurkan supaya melewati ambang.
        var tracked = await world.DbContext.Set<InpEpisode>().FirstAsync(x => x.Id == episode.Id);
        tracked.DischargeDecidedAt = DateTime.UtcNow.AddHours(-10);
        await world.DbContext.SaveChangesAsync();

        var daftar = await world.CensusQueryService.GetPendingClosuresAsync(
            new InpatientMonitoringQuery());

        var butir = Assert.Single(daftar.Items);

        Assert.Equal(episode.Id, butir.EpisodeId);
        Assert.False(butir.IsBedStillHeld);
        Assert.NotNull(butir.PhysicallyLeftAt);
    }
}
