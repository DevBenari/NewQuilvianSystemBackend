using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Tests.InPatientManagement;

/// <summary>
/// <c>BE-RWI-036</c> — metadata pemegang dan reservasi aktif pada papan tempat tidur.
/// </summary>
public sealed class InpBedBoardReservationMetadataTests
{
    [Fact]
    public async Task Reserved_MengembalikanIdentitasPemegangDanBatasWaktuReservasiAktif()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room);
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        Assert.Equal(InpEpisodeOperationStatus.Success, reserve.Status);

        var reservation = await world.DbContext.Set<InpBedReservation>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == reserve.ReservationId!.Value);

        var board = await world.BedOccupancyService.GetBedBoardAsync(null);
        var response = FindBed(board, bed.Id);

        Assert.True(response.IsReserved);
        Assert.False(response.IsOccupied);
        Assert.Equal(reservation.Id, response.ReservationId);
        Assert.Equal(reservation.ExpiresAt, response.ReservationExpiresAt);
        Assert.Equal(episode.Id, response.HoldingEpisodeId);
        Assert.Equal(episode.EpisodeNumber, response.HoldingEpisodeNumber);
        Assert.Equal("Ibu Rina", response.PatientName);
        Assert.Equal(1, board.TotalReserved);
        Assert.Equal(0, board.TotalAvailable);
    }

    [Fact]
    public async Task Expired_TidakMengeksposIdentitasAtauMetadataReservasi()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room);
        var episode = await world.OpenDraftEpisodeAsync();

        var reserve = await world.BedOccupancyService.ReserveBedAsync(
            new ReserveBedRequest { EpisodeId = episode.Id, BedId = bed.Id },
            InpatientEpisodeTestWorld.ActorUserId);

        var reservation = await world.DbContext.Set<InpBedReservation>()
            .SingleAsync(x => x.Id == reserve.ReservationId!.Value);

        reservation.ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await world.DbContext.SaveChangesAsync();

        var board = await world.BedOccupancyService.GetBedBoardAsync(null);
        var response = FindBed(board, bed.Id);

        Assert.False(response.IsReserved);
        Assert.False(response.IsOccupied);
        Assert.Null(response.ReservationId);
        Assert.Null(response.ReservationExpiresAt);
        Assert.Null(response.HoldingEpisodeId);
        Assert.Null(response.HoldingEpisodeNumber);
        Assert.Null(response.PatientName);
        Assert.Equal(0, board.TotalReserved);
        Assert.Equal(1, board.TotalAvailable);

        var expired = await world.DbContext.Set<InpBedReservation>()
            .AsNoTracking()
            .SingleAsync(x => x.Id == reservation.Id);

        Assert.Equal(InpBedReservationStatus.Expired, expired.ReservationStatus);
        Assert.NotNull(expired.ReleasedAt);
    }

    [Fact]
    public async Task Occupied_MemprioritaskanPenghuniDanTidakMengeksposReservasiStale()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var room = await world.AddRoomAsync();
        var bed = await world.AddBedAsync(room);
        var occupiedEpisode = await world.OpenAndPlaceAsync(bed);

        var otherPatient = await world.AddPatientAsync("Tn. Budi", Gender.Male);
        var otherEpisode = await world.OpenDraftEpisodeAsync(otherPatient.Id);
        var now = DateTime.UtcNow;

        world.DbContext.Set<InpBedReservation>().Add(new InpBedReservation
        {
            Id = Guid.NewGuid(),
            EpisodeId = otherEpisode.Id,
            BedId = bed.Id,
            ReservedAt = now,
            ExpiresAt = now.AddHours(1),
            ReservationStatus = InpBedReservationStatus.Active,
            ReservedByUserId = InpatientEpisodeTestWorld.ActorUserId,
            IsActive = true,
            CreateDateTime = now,
            CreateBy = InpatientEpisodeTestWorld.ActorUserId
        });

        await world.DbContext.SaveChangesAsync();

        var board = await world.BedOccupancyService.GetBedBoardAsync(null);
        var response = FindBed(board, bed.Id);

        Assert.True(response.IsOccupied);
        Assert.False(response.IsReserved);
        Assert.Equal(occupiedEpisode.Id, response.HoldingEpisodeId);
        Assert.Equal(occupiedEpisode.EpisodeNumber, response.HoldingEpisodeNumber);
        Assert.Equal("Ibu Rina", response.PatientName);
        Assert.Null(response.ReservationId);
        Assert.Null(response.ReservationExpiresAt);
        Assert.Equal(1, board.TotalOccupied);
        Assert.Equal(0, board.TotalReserved);
    }

    [Fact]
    public async Task TanpaPemegang_TidakMengeksposMetadataDanMempertahankanCounterExisting()
    {
        var world = await InpatientEpisodeTestWorld.CreateAsync();
        var room = await world.AddRoomAsync();
        var availableBed = await world.AddBedAsync(room, "3A");
        var unavailableBed = await world.AddBedAsync(
            room,
            "3B",
            bedStatus: BedStatus.Maintenance);

        var board = await world.BedOccupancyService.GetBedBoardAsync(null);

        foreach (var response in new[]
                 {
                     FindBed(board, availableBed.Id),
                     FindBed(board, unavailableBed.Id)
                 })
        {
            Assert.False(response.IsOccupied);
            Assert.False(response.IsReserved);
            Assert.Null(response.HoldingEpisodeId);
            Assert.Null(response.HoldingEpisodeNumber);
            Assert.Null(response.PatientName);
            Assert.Null(response.ReservationId);
            Assert.Null(response.ReservationExpiresAt);
        }

        Assert.Equal(2, board.TotalBed);
        Assert.Equal(1, board.TotalAvailable);
        Assert.Equal(0, board.TotalOccupied);
        Assert.Equal(0, board.TotalReserved);
        Assert.Equal(1, board.TotalUnavailable);
    }

    private static BedBoardBedResponse FindBed(BedBoardResponse board, Guid bedId)
        => board.ServiceUnits
            .SelectMany(x => x.Rooms)
            .SelectMany(x => x.Beds)
            .Single(x => x.BedId == bedId);
}
